using System.ServiceProcess;

namespace AutoLockHelper
{
    internal static class ServiceStat
    {
        internal static bool CheckServiceRunning(string _service)
        {
            ServiceController sc = new ServiceController(_service);

            switch (sc.Status)
            {
                case ServiceControllerStatus.Running:
                    return true;
                case ServiceControllerStatus.Stopped:
                    return false;
                default:
                    return false;
            }
        }
    }
}
