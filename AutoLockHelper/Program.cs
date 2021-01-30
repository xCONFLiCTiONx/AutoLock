using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Windows;
using Action = Microsoft.Win32.TaskScheduler.Action;
using System.Runtime.InteropServices;

namespace AutoLockHelper
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            EasyLogger.BackupLogs(EasyLogger.LogFile);
            EasyLogger.AddListener(EasyLogger.LogFile);
            EasyLogger.Info("Initializing");

            if (args.Length > 0)
            {
                EasyLogger.Info("Arguments: " + args[0]);
                if (args[0] == "/settings" || args[0] == "-settings" || args[0] == "settings")
                {
                    ShowSettings();
                }
                if (args[0] == "/startService" || args[0] == "-startService" || args[0] == "startService")
                {
                    StartService();
                }
                else if (args[0] == "/stopService" || args[0] == "-stopService" || args[0] == "stopService")
                {
                    StopService();
                }
                else if (args[0] == "/install" || args[0] == "-install" || args[0] == "install")
                {
                    Installer();
                }
                else if (args[0] == "/uninstall" || args[0] == "-uninstall" || args[0] == "uninstall")
                {
                    Uninstaller();
                }
            }
            else
            {
                Installer();
            }

            Environment.Exit(0);
        }

        private static void ShowSettings()
        {
            EasyLogger.Info("Showing Settings...");
            Settings settings = new Settings();
            settings.ShowDialog();
        }

        private static void Installer()
        {
            EasyLogger.Info("Installing...");

            InstallTask();

            AddRegistryEntry();

            CreateShortcut();

            InstallService();
        }

        private static void Uninstaller()
        {
            EasyLogger.Info("Uninstalling AutoLock");

            UninstallService();

            UninstallTask();

            RemoveRegistryEntry();
        }

        #region INSTALL
        private static void InstallService()
        {
            ServiceController ctl = ServiceController.GetServices()
                .FirstOrDefault(s => s.ServiceName == "AutoLockService");
            if (ctl == null)
            {
                try
                {
                    EasyLogger.Info("Installing Service...");
                    RedirectStandardOutput.RunCommand(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe", "\"" + AppDomain.CurrentDomain.BaseDirectory + @"AutoLockService.exe" + "\"");
                }
                catch (Exception ex)
                {
                    EasyLogger.Error(ex);

                    Environment.Exit(0);
                }
            }

            try
            {
                EasyLogger.Info("Starting AutoLockService...");
                RedirectStandardOutput.RunCommand("cmd.exe", "/c net start AutoLockService");
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);

                Environment.Exit(0);
            }
        }

        private static void InstallTask()
        {
            EasyLogger.Info("Installing Tasks...");
            using (TaskService tService = new TaskService())
            {
                // AutoLock Watcher
                EasyLogger.Info("Installing Tasks: AutoLock Watcher");
                using (TaskDefinition tDefinition = tService.NewTask())
                {
                    tDefinition.Principal.Id = "Users";
                    tDefinition.Principal.LogonType = TaskLogonType.Group;
                    tDefinition.RegistrationInfo.Description = "AutoLock";

                    ExecAction action = (ExecAction)Action.CreateAction(TaskActionType.Execute);
                    action.Path = AppDomain.CurrentDomain.BaseDirectory + "AutoLock.exe";
                    action.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    tDefinition.Actions.Add(action);

                    tService.RootFolder.RegisterTaskDefinition(@"AutoLock", tDefinition, TaskCreation.CreateOrUpdate, "Users", null, TaskLogonType.Group);
                }
                // Service Watcher
                EasyLogger.Info("Installing Tasks: Service Watcher");
                using (TaskDefinition tDefinition = tService.NewTask())
                {
                    tDefinition.Principal.Id = "SYSTEM";
                    tDefinition.Principal.LogonType = TaskLogonType.ServiceAccount;
                    tDefinition.RegistrationInfo.Description = "AutoLockService";
                    tDefinition.Triggers.AddNew(TaskTriggerType.Logon);
                    tDefinition.Principal.RunLevel = TaskRunLevel.Highest;

                    ExecAction action = (ExecAction)Action.CreateAction(TaskActionType.Execute);
                    action.Path = AppDomain.CurrentDomain.BaseDirectory + "AutoLockHelper.exe";
                    action.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    action.Arguments = "/startService";

                    tDefinition.Actions.Add(action);

                    tService.RootFolder.RegisterTaskDefinition(@"AutoLockService", tDefinition, TaskCreation.CreateOrUpdate, "SYSTEM", null, TaskLogonType.ServiceAccount);
                }
            }
        }

        private static void AddRegistryEntry()
        {
            EasyLogger.Info("Adding Registry Settings: LoginTime: " + DateTime.Now + " and LogoutTime: 00:45:00");
            RegistryKey key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\AutoLock");
            key.SetValue("LoginTime", DateTime.Now);
            key.SetValue("LogoutTime", "00:45:00");
            key.Close();
        }

        private static void CreateShortcut()
        {
            try
            {
                EasyLogger.Info("Creating shortcuts...");

                string _programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                Directory.CreateDirectory(_programs + "\\AutoLock\\");

                // Settings
                EasyLogger.Info("Creating shortcut: Settings");

                string shortcutPath = _programs + "\\AutoLock\\Settings.lnk";
                string targetPath = Assembly.GetEntryAssembly().Location;
                string arguments = "/settings";
                string comments = "AutoLock Settings";
                string workingFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string iconPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\AutoLock.exe";
                string appID = "";
                // GUID
                var assembly = typeof(Program).Assembly;
                var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
                Guid activatorID = Guid.Parse(attribute.Value);

                Helper.Shortcut.InstallShortcut(shortcutPath, targetPath, arguments, comments, workingFolder, ShortcutWindowState.Normal, iconPath, appID, activatorID);

                // Start Service
                EasyLogger.Info("Creating shortcut: Start Service");

                shortcutPath = _programs + "\\AutoLock\\Start Service.lnk";
                targetPath = Assembly.GetEntryAssembly().Location;
                arguments = "/startService";
                comments = "Starts the AutoLockService";
                workingFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                iconPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\AutoLock.exe";
                appID = "";
                // GUID
                assembly = typeof(Program).Assembly;
                attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
                activatorID = Guid.Parse(attribute.Value);

                Helper.Shortcut.InstallShortcut(shortcutPath, targetPath, arguments, comments, workingFolder, ShortcutWindowState.Normal, iconPath, appID, activatorID);

                // Stop Service
                EasyLogger.Info("Creating shortcut: Stop Service");

                shortcutPath = _programs + "\\AutoLock\\Stop Service.lnk";
                targetPath = Assembly.GetEntryAssembly().Location;
                arguments = "/stopService";
                comments = "Stops the AutoLockService";
                workingFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                iconPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\AutoLock.exe";
                appID = "";
                // GUID
                assembly = typeof(Program).Assembly;
                attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
                activatorID = Guid.Parse(attribute.Value);

                Helper.Shortcut.InstallShortcut(shortcutPath, targetPath, arguments, comments, workingFolder, ShortcutWindowState.Normal, iconPath, appID, activatorID);
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }
        }

        #endregion INSTALL

        #region UNINSTALL

        private static void UninstallService()
        {
            ServiceController ctl = ServiceController.GetServices()
                .FirstOrDefault(s => s.ServiceName == "AutoLockService");
            if (ctl != null)
            {
                try
                {
                    EasyLogger.Info("Uninstalling AutoLockService...");
                    RedirectStandardOutput.RunCommand("cmd.exe", "/c net stop AutoLockService");

                    RedirectStandardOutput.RunCommand(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe", "/u \"" + AppDomain.CurrentDomain.BaseDirectory + @"AutoLockService.exe" + "\"");
                }
                catch (Exception ex)
                {
                    EasyLogger.Error(ex);

                    Environment.Exit(0);
                }
            }
        }

        private static void UninstallTask()
        {
            EasyLogger.Info("Removing Tasks...");
            using (TaskService ts = new TaskService())
            {
                ts.RootFolder.DeleteTask("AutoLock");
                ts.RootFolder.DeleteTask("AutoLockService");
            }
        }

        private static void RemoveRegistryEntry()
        {
            try
            {
                EasyLogger.Info("Removing Registry Key: LocalMachine\\Software\\AutoLock");
                DeleteSubKeyTree(Registry.LocalMachine, "Software\\AutoLock");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void DeleteSubKeyTree(RegistryKey registryPath, string keyToDelete)
        {
            if (registryPath != null && !string.IsNullOrWhiteSpace(keyToDelete))
            {
                bool throwOnMissingSubKey = false;
                try { registryPath.DeleteSubKeyTree(keyToDelete, throwOnMissingSubKey); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        #endregion UNINSTALL

        #region HELPERS

        private static void StartService()
        {
            EasyLogger.Info("Starting the AutoLockService");
            ServiceController ctl = ServiceController.GetServices()
                   .FirstOrDefault(s => s.ServiceName == "AutoLockService");
            if (ctl != null)
            {
                try
                {
                    RedirectStandardOutput.RunCommand("cmd.exe", "/c net start AutoLockService");
                }
                catch (Exception ex)
                {
                    EasyLogger.Error(ex);
                }
            }
        }

        private static void StopService()
        {
            EasyLogger.Info("Stopping the AutoLockService");
            ServiceController ctl = ServiceController.GetServices()
                   .FirstOrDefault(s => s.ServiceName == "AutoLockService");
            if (ctl != null)
            {
                try
                {
                    RedirectStandardOutput.RunCommand("cmd.exe", "/c net stop AutoLockService");
                }
                catch (Exception ex)
                {
                    EasyLogger.Error(ex);
                }
            }
        }

        #endregion HELPERS
    }
}
