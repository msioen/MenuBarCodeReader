using System;
using AppKit;
using Foundation;
using ShortcutRecorder;

namespace MenuBarCodeReader
{
    public static class Extensions
    {
        public static void BindHotKey(this NSObject target, NSObject observable, string keyPath)
        {
            var keyOptions = new NSMutableDictionary();
            keyOptions.SetValueForKey(new SRKeyEquivalentTransformer(), ShortcutRecorder.Constants.NSValueTransformerBindingOption);
            target.Bind("keyEquivalent", observable, keyPath, keyOptions);

            var keyModifierOptions = new NSMutableDictionary();
            keyModifierOptions.SetValueForKey(new SRKeyEquivalentModifierMaskTransformer(), ShortcutRecorder.Constants.NSValueTransformerBindingOption);
            target.Bind("keyEquivalentModifierMask", observable, keyPath, keyModifierOptions);
        }

        public static void SendToClipboard(this string text)
        {
            NSPasteboard.GeneralPasteboard.ClearContents();
            NSPasteboard.GeneralPasteboard.SetStringForType(text, NSPasteboard.NSPasteboardTypeString);
        }
    }
}
