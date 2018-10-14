using System;
using Foundation;

namespace MenuBarCodeReader
{
    public class Settings
    {
        #region Public

        public bool ShouldOutputScanResultsToClipboard
        {
            get { return GetOrDefault(Constants.SETTING_OUTPUT_CLIPBOARD, true); }
            set { NSUserDefaults.StandardUserDefaults.SetBool(value, Constants.SETTING_OUTPUT_CLIPBOARD); }
        }

        public bool ShouldOutputScanResultsToNotificationCenter
        {
            get { return GetOrDefault(Constants.SETTING_OUTPUT_NOTIFICATION, true); }
            set { NSUserDefaults.StandardUserDefaults.SetBool(value, Constants.SETTING_OUTPUT_NOTIFICATION); }
        }

        #endregion

        #region Private

        bool GetOrDefault(string key, bool defaultValue)
        {
            if (NSUserDefaults.StandardUserDefaults[key] == null)
                return defaultValue;

            return NSUserDefaults.StandardUserDefaults.BoolForKey(key);
        }

        #endregion
    }
}
