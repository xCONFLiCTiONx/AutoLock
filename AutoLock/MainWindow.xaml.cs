using DesktopToast;
using Microsoft.Win32;
using NotificationsExtensions;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Timer = System.Timers.Timer;

namespace AutoLock
{
    public partial class MainWindow : Window, IDisposable
    {
        private Timer timer;
        private bool WarningShown = false;
        private string _toastTitle;
        private string _toastBody;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private int _time = 0;
        private int WarningCount;
        private int LogoffCount;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                EasyLogger.BackupLogs(EasyLogger.LogFile);
                EasyLogger.AddListener(EasyLogger.LogFile);
                EasyLogger.Info("Initializing");

                Opacity = 0;

                Hide();

                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\AutoLock");
                if (key != null)
                {
                    DateTime _loginTime = DateTime.Parse(key.GetValue("LoginTime").ToString());
                    TimeSpan _logoutTime = TimeSpan.Parse(key.GetValue("LogoutTime").ToString());

                    LogoffCount = (Convert.ToInt32(_logoutTime.TotalSeconds) - Convert.ToInt32((DateTime.Now - _loginTime).TotalSeconds));

                    WarningCount = (LogoffCount - 300);

                    EasyLogger.Info("Total Time until 5 Minute Warning: " + TimeSpan.FromSeconds(LogoffCount).ToString(@"mm\:ss"));

                    EasyLogger.Info("Total Time until Logging out: " + TimeSpan.FromSeconds(WarningCount).ToString(@"mm\:ss"));
                    // Give a 1 minute gap in case the user has closed the
                    // application near the time it should have logged them out.
                    if (LogoffCount < -60)
                    {
                        Panic();
                    }

                    key.Close();
                }
                else
                {
                    Panic();
                }

                timer = new Timer();
                timer.Interval = 1000;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();

                notifyIcon = new System.Windows.Forms.NotifyIcon
                {
                    Visible = true,

                    Text = "Your session will end in: " + TimeSpan.FromSeconds(LogoffCount).ToString(@"mm\:ss") + " minutes"
                };

                notifyIcon.MouseDown += NotifyIcon_MouseDown;

                System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();

                System.Windows.Forms.MenuItem menuItemClose = new System.Windows.Forms.MenuItem("&Close AutoLock");

                menuItemClose.Click += MenuItemClose_Click;

                contextMenu.MenuItems.Add(menuItemClose);

                notifyIcon.ContextMenu = contextMenu;

                Thread thread = new Thread(FirstAlert)
                {
                    IsBackground = true
                };
                thread.Start();
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }
        }

        private void FirstAlert()
        {
            // This is only to give enough to for the desktop to be created
            Thread.Sleep(5000);

            Dispatcher.Invoke(() => AlertUser("AutoLock Time Expiring", "Windows will logout in " + TimeSpan.FromSeconds(LogoffCount - _time).ToString(@"mm\:ss") + " minutes"));
        }

        private void Panic()
        {
            EasyLogger.Info("The registry entry to get the login time was null or less than zero. There may be a problem with AutoLockService.");

            MessageBox.Show("AutoLock is in error and will continue to log out immediately upon login." + Environment.NewLine + Environment.NewLine + "Please let a staff memeber know about this before you close this message!", "AutoLock", MessageBoxButton.OK, MessageBoxImage.Error);

            MessageBox.Show("If you are a staff member and are unsure what to do, please start by rebooting the computer and log back in. If the error is repeated, let the manager know and leave this error message open.", "AutoLock", MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(0);
        }

        private async void AlertUser(string toastTitle, string toastBody)
        {
            _toastTitle = toastTitle;
            _toastBody = toastBody;

            ToastResult = await ShowInteractiveToastAsync();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _time++;

            notifyIcon.Text = "Your session will end in: " + TimeSpan.FromSeconds(LogoffCount - _time).ToString(@"mm\:ss") + " Minutes";

            CreateTextIcon((TimeSpan.FromSeconds(LogoffCount - _time).Minutes + 1).ToString());

            if (_time >= WarningCount)
            {
                if (!WarningShown)
                {
                    EasyLogger.Info("5 Minute warning");
                    WarningShown = true;

                    Clear();

                    Dispatcher.Invoke(() => SendToast(true));
                }
                else if (_time >= LogoffCount)
                {
                    EasyLogger.Info("Time's up; logging off...");
                    timer.Stop();

                    Clear();

                    Dispatcher.Invoke(() => SendToast(false));

                    Thread.Sleep(10000);

                    LogoutWindows();
                }
            }
        }

        private void MenuItemClose_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ServiceCheck.isServiceInstalled("AutoLockService"))
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    if (ServiceCheck.isServiceRunning("AutoLockService"))
                    {
                        var dialog = System.Windows.Forms.MessageBox.Show("Are you sure you want to close this application?", "Quit AutoLock", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question, System.Windows.Forms.MessageBoxDefaultButton.Button1);
                        if (dialog == System.Windows.Forms.DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "AutoLockHelper.exe", "/stopService");

                            while (ServiceCheck.isServiceRunning("AutoLockService"))
                            {
                                Thread.Sleep(1000);
                            }

                            Application.Current.Shutdown();
                        }
                    }
                    else
                    {
                        Application.Current.Shutdown();
                    }
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }
        }

        public void CreateTextIcon(string str)
        {
            Font fontToUse = new Font(System.Drawing.FontFamily.GenericSansSerif, 11, GraphicsUnit.Pixel);
            Brush brushToUse = new SolidBrush(Color.White);
            Bitmap bitmapText = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(bitmapText);

            IntPtr hIcon;

            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(str, fontToUse, brushToUse, 0, 0);
            hIcon = (bitmapText.GetHicon());
            notifyIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
        }

        private void NotifyIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                AlertUser("AutoLock Time Expiring", "Windows will logout in " + TimeSpan.FromSeconds(LogoffCount - _time).ToString(@"mm\:ss") + " minutes");

            }
        }

        private void LogoutWindows()
        {
            EasyLogger.Info("Windows is logging out the current user");

            NativeMethods.LogoffWindows();
        }

        private void SendToast(bool _warning)
        {
            if (_warning)
            {
                AlertUser("AutoLock Time Expiring", "Windows will logout in 5 minutes. Please save your work before hand!");
            }
            else
            {
                AlertUser("AutoLock Time Expiring", "Your time has expired! Windows is logging out now!");
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // For Action Center of Windows 10
            NotificationActivatorBase.RegisterComType(typeof(NotificationActivator), OnActivated);

            NotificationHelper.RegisterComServer(typeof(NotificationActivator), Assembly.GetExecutingAssembly().Location);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            EasyLogger.Info("Application is now closing");

            base.OnClosing(e);

            Clear();

            notifyIcon.Visible = false;

            try
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Close();
                    timer.Dispose();
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }

            NotificationHelper.UnregisterComServer(typeof(NotificationActivator));
            NotificationActivatorBase.UnregisterComType();
        }

        private const string MessageId = "Message";

        private void OnActivated(string arguments, Dictionary<string, string> data)
        {
            var result = "Activated";
            if ((arguments?.StartsWith("action=")).GetValueOrDefault())
            {
                result = arguments.Substring("action=".Length);

                if ((data?.ContainsKey(MessageId)).GetValueOrDefault())
                    Dispatcher.Invoke(() => Message = data[MessageId]);
            }
            Dispatcher.Invoke(() => ActivationResult = result);

            EasyLogger.Info("User has activated the Toast Notification with results: " + result);

            if (result == "LOGOUT")
            {
                LogoutWindows();
            }
        }

        #region Property

        public string ToastResult
        {
            get { return (string)GetValue(ToastResultProperty); }
            set
            {
                Dispatcher.Invoke(() => SetValue(ToastResultProperty, value));
            }
        }
        public static readonly DependencyProperty ToastResultProperty =
            DependencyProperty.Register(
                nameof(ToastResult),
                typeof(string),
                typeof(MainWindow),
                new PropertyMetadata(string.Empty));

        public string ActivationResult
        {
            get { return (string)GetValue(ActivationResultProperty); }
            set
            {
                Dispatcher.Invoke(() => SetValue(ActivationResultProperty, value));
            }
        }
        public static readonly DependencyProperty ActivationResultProperty =
            DependencyProperty.Register(
                nameof(ActivationResult),
                typeof(string),
                typeof(MainWindow),
                new PropertyMetadata(string.Empty));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set
            {
                Dispatcher.Invoke(() => SetValue(MessageProperty, value));
            }
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(MainWindow),
                new PropertyMetadata(string.Empty));

        public bool CanUseInteractiveToast
        {
            get { return (bool)GetValue(CanUseInteractiveToastProperty); }
            set { SetValue(CanUseInteractiveToastProperty, value); }
        }
        public static readonly DependencyProperty CanUseInteractiveToastProperty =
            DependencyProperty.Register(
                nameof(CanUseInteractiveToast),
                typeof(bool),
                typeof(MainWindow),
                new PropertyMetadata(Environment.OSVersion.Version.Major >= 10));

        #endregion

        private void Clear()
        {
            ToastResult = "";
            ActivationResult = "";
            Message = "";
        }

        private async Task<string> ShowInteractiveToastAsync()
        {
            var request = new ToastRequest
            {
                ToastXml = ComposeInteractiveToast(),
                ShortcutFileName = "AutoLock.lnk",
                ShortcutTargetFilePath = Assembly.GetExecutingAssembly().Location,
                ShortcutWorkingFolder = AppDomain.CurrentDomain.BaseDirectory,
                AppId = "AutoLock",
                ActivatorId = typeof(NotificationActivator).GUID
            };

            var result = await ToastManager.ShowAsync(request);

            return result.ToString();
        }

        private string ComposeInteractiveToast()
        {
            var toastVisual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    Children =
                    {
                        new AdaptiveText { Text = _toastTitle }, // Title
						new AdaptiveText { Text = _toastBody }, // Body
					},
                    AppLogoOverride = new ToastGenericAppLogo
                    {
                        Source = string.Format("file:///{0}", Path.GetFullPath("DesktopToast.png")),
                        AlternateText = "Logo"
                    }
                }
            };
            var toastAction = new ToastActionsCustom
            {
                Buttons =
                {
                    new ToastButton(content: "Logout", arguments: "action=LOGOUT") { ActivationType = ToastActivationType.Background },
                    new ToastButton(content: "Ignore", arguments: "action=Ignored")
                }
            };
            var toastContent = new ToastContent
            {
                Visual = toastVisual,
                Actions = toastAction,
                Duration = ToastDuration.Long
            };

            return toastContent.GetContent();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (timer != null)
                    timer.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}