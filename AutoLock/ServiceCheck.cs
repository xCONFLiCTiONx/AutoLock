using System.Linq;
using System.ServiceProcess;

namespace AutoLock
{
    class ServiceCheck
    {
        internal static bool isServiceInstalled(string service)
        {
            ServiceController ctl = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == service);
            if (ctl == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal static bool isServiceRunning(string service)
        {
            ServiceController sc = new ServiceController(service);

            switch (sc.Status)
            {
                case ServiceControllerStatus.Running:
                    return true;
                case ServiceControllerStatus.Stopped:
                    return false;
                case ServiceControllerStatus.StopPending:
                    return true;
                default:
                    return false;
            }
        }
    }
}
