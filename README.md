# AutoLock
**Automatically log out the current user after a 45 minute session (Windows 10 Desktop)**  
  
AutoLock is used to give the general public access to public computers at an organization and allow them an allotted time before logging them out. The time can be preset by an admin or just use the default 45 minutes. The applicatoin can still be closed by an admin and will be restarted at the next login.  

## How It Works

There is a service, 2 scheduled tasks, an application that runs in the system tray, Windows 10 Toast Notifications, and 3 shortuts in the start menu.
* Service
	* The system notifies the service if someone logs in/out
	* Starts the scheduled task which starts the applicatoin to the system tray
	* Re-starts the application from the scheduled task if the user closes/kills the application somehow
* Scheduled Tasks
	* The task named AutoLock is the actual Application that runs in the system tray and alerts the user with Toast Notifications how much time is left
	* The task named AutoLockService will restart the service at the next login if the service is stopped
* The Application (in the notficication tray)
	* Sends a toast notification once at login to alert the user that they have 45 minutes (default) remaining, a 5 minute warning and then 10 seconds before the time is up and then logs the user out
	* Right click menu item to close the applicatoin which will require administrative priveleges to do. If the service isn't running, the application will just close without needing administrator rights
* Windows 10 Toast Notifications
	* a nice way to alert the user of the time remaining with 2 buttons:
		* ignore: dismiss the notification
		* logout: logs out the user instantly
* Shortcuts
	* Settings shortcut
		* only setting, currently, is change the alloted time for the user until logged out
	* Stop the AutoLockService service (for admins if needed)
	* Start the AutoLockService service (for admins if needed)

You can obtain the time remaining 3 different ways
* Hover the cursor over the tray icon
* Left click the tray icon which will trigger a toast notification
* The tray icon is in text using the minutes of the time remaining
  
An admin doing maintenence on the PC will want to close the application by right clicking the tray icon and exit. Once the admin is done they can simply just logout and walk away and next time a user logs in, the service will be automatically started by the scheduled task.  
  
If you are setting up the PC for using this application, it is important that you set proper rights such as make the user account a guest/standard user. You should also make sure that your group policy settings are accurate.
  
Settings are stored in the registry at HKLM\Software\AutoLock
  
It's advised that you disable "Use my sign in info to automatically finish setting up my device after an update or restart" in Windows 10 Settings.
    
Please [Report Bugs](https://github.com/New-Life-Evangelistic-Center/AutoLock/issues)  
Logs can be found in CommonApplicationData "C:\ProgramData\Easy Logger\"  
  
### Prerequisites

.NET Framework 4.7.2 (preinstalled in Windows 10)  

### Installing

Just run the [MSI Setup file](https://github.com/New-Life-Evangelistic-Center/AutoLock/releases) and install to the default location. You can install anywhere you like but keep in mind how the admin has file securities in place.

## Built With

* [EasyLogger](https://github.com/xCONFLiCTiONx/Logger) | [Nuget](https://www.nuget.org/packages/xCONFLiCTiONx.Logger/) - The Logger used
* [NotificationsExtensions.Win10](https://www.nuget.org/packages/NotificationsExtensions.Win10/14332.0.2/) - Windows 10 Toast Notifications
* [DesktopToast](https://github.com/emoacht/DesktopToast) - Windows 10 Toast Notifications
* [TaskScheduler](https://www.nuget.org/packages/TaskScheduler/2.8.18) - Task Scheduler

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details