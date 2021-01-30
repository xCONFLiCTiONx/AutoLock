using DesktopToast;
using System;
using System.Runtime.InteropServices;

namespace AutoLock
{
    /// <summary>
	/// Inherited class of notification activator (for Action Center of Windows 10)
	/// </summary>
	/// <remarks>The CLSID of this class must be unique for each application.</remarks>
	[Guid("0E24360C-830F-4F2F-BBE7-026747143128"), ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    public class NotificationActivator : NotificationActivatorBase
    { }
}
