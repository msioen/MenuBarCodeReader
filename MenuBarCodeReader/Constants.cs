using System;
using Foundation;

namespace MenuBarCodeReader
{
    public static class Constants
    {
        public static readonly NSString VALUE_HOTKEY_SCAN_SELECT = new NSString("hotKeyScanSelect");
        public static readonly NSString VALUE_HOTKEY_ESC = new NSString("hotKeyEscape");
        public static readonly NSString VALUE_HOTKEY_SCAN_CLICK = new NSString("hotKeyScanClick");

        public static readonly NSString KEY_HOTKEY_SCAN_SELECT = new NSString("values.hotKeyScanSelect");
        public static readonly NSString KEY_HOTKEY_ESC = new NSString("values.hotKeyEscape");
        public static readonly NSString KEY_HOTKEY_SCAN_CLICK = new NSString("values.hotKeyScanClick");

        public const string SETTING_OUTPUT_CLIPBOARD = "SETTING_OUTPUT_CLIPBOARD";
        public const string SETTING_OUTPUT_NOTIFICATION = "SETTING_OUTPUT_NOTIFICATION";
    }
}
