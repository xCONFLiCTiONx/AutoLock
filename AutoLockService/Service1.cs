using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using System.Management;
using System.ServiceProcess;

namespace AutoLockService
{
    public partial class Service1 : ServiceBase
    {
        internal ManagementEventWatcher stopWatch;

        public Service1()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            EasyLogger.BackupLogs(EasyLogger.LogFile);
            EasyLogger.AddListener(EasyLogger.LogFile);
            EasyLogger.Info("Initializing");

            InitializeComponent();

            CanHandleSessionChangeEvent = true;
        }

        internal void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            Processor();
        }

        internal void Processor()
        {
            try
            {
                EasyLogger.Info("Watching the AutoLock system tray application");

                stopWatch = new ManagementEventWatcher(
                  new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
                stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);

                StartTracking();
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }
        }

        private void stopWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                if ((string)e.NewEvent.Properties["ProcessName"].Value == "AutoLock.exe")
                {
                    EasyLogger.Info("" + "AutoLock was closed. Restarting AutoLock");

                    using (TaskService tasksrvc = new TaskService())
                    {
                        Task task = tasksrvc.FindTask("AutoLock");
                        task.Run();
                    }
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                    StartTracking();
                    break;

                case SessionChangeReason.SessionLogoff:
                    StopTracking();
                    break;
            }
        }

        private void StartTracking()
        {
            try
            {
                EasyLogger.Info("SimpleService.OnSessionChange: Logon");

                using (TaskService tasksrvc = new TaskService())
                {
                    Task task = tasksrvc.FindTask("AutoLock");
                    task.Run();
                }

                RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\AutoLock");
                key.SetValue("LoginTime", DateTime.Now);
                key.Close();

                stopWatch.Start();
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }
        }

        private void StopTracking()
        {
            EasyLogger.Info("SimpleService.OnSessionChange: Logoff");

            stopWatch.Stop();
        }

        protected override void OnStop()
        {
            if (stopWatch != null)
            {
                stopWatch.Stop();
                stopWatch.Dispose();
            }
        }
    }
}
