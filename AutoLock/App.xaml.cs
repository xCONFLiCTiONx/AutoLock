using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace AutoLock
{
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var id = attribute.Value;

            var mutex = new Mutex(true, id);

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    exitApp();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        private void exitApp()
        {
            Environment.Exit(0);
        }
    }
}