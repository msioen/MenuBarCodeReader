using System;
using Foundation;
using UserNotifications;

namespace MenuBarCodeReader
{
    public class NotificationService : NSObject, INSUserNotificationCenterDelegate
    {
        const string IDENTIFIER = "MenuBarCodeReader";

        static NotificationService _instance;

        #region Properties

        public static NotificationService Instance
        {
            get
            {
                _instance = _instance ?? new NotificationService();
                return _instance;
            }
        }

        #endregion

        #region Constructors

        NotificationService()
        {
            if (UseUserNotificationsFramework())
            {

            }
            else
            {
                NSUserNotificationCenter.DefaultUserNotificationCenter.ShouldPresentNotification = DefaultUserNotificationCenter_ShouldPresentNotification;
                //NSUserNotificationCenter.DefaultUserNotificationCenter.Delegate = this;
                NSUserNotificationCenter.DefaultUserNotificationCenter.DidActivateNotification += DefaultUserNotificationCenter_DidActivateNotification;
            }
        }

        #endregion

        #region Public

        public void ShowNotification(string title, string text)
        {
            if (UseUserNotificationsFramework())
            {
                ShowNotificationWithUserNotifications(title, text);
            }
            else
            {
                ShowNotificationWithNSUserNotifications(title, text);
            }
        }

        #endregion

        #region Private

        bool UseUserNotificationsFramework()
        {
            var tmp = NSProcessInfo.ProcessInfo.OperatingSystemVersion;
            return NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(10, 14, 0));
        }

        void ShowNotificationWithNSUserNotifications(string title, string text)
        {
            var notification = new NSUserNotification();
            notification.Title = title;
            notification.InformativeText = text;

            NSUserNotificationCenter.DefaultUserNotificationCenter.DeliverNotification(notification);
        }

        // macOS 10.14 - Mojave introduces UserNotifications framework
        void ShowNotificationWithUserNotifications(string title, string text)
        {
            var notificationContent = new UNMutableNotificationContent();
            notificationContent.Title = title;
            notificationContent.Body = text;

            var notificationTrigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0, false);
            var notificationRequest = UNNotificationRequest.FromIdentifier(IDENTIFIER, notificationContent, notificationTrigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(notificationRequest, x => { });
        }

        static bool DefaultUserNotificationCenter_ShouldPresentNotification(NSUserNotificationCenter center, NSUserNotification notification)
        {
            return true;
        }

        void DefaultUserNotificationCenter_DidActivateNotification(object sender, UNCDidActivateNotificationEventArgs e)
        {
            var notification = e?.Notification;
            if (notification == null)
                return;

            var notificationText = notification.InformativeText;
            notificationText.SendToClipboard();
        }

        #endregion
    }
}
