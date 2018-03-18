using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using HotKeyManager;
using PTHotKey;

namespace MenuBarCodeReader
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        NSStatusItem _statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);

        JFHotkeyManager _hotkeyManager;
        Dictionary<NSString, nuint> _hotkeyBindings = new Dictionary<NSString, nuint>();

        public AppDelegate()
        {
            var defaultValues = new NSMutableDictionary();
            var defaultScanSelect = new NSMutableDictionary();

            defaultScanSelect.SetValueForKey(new NSString("b"), new NSString("characters"));
            defaultScanSelect.SetValueForKey(new NSString("B"), new NSString("charactersIgnoringModifiers"));
            defaultScanSelect.SetValueForKey(new NSNumber(11), new NSString("keyCode"));
            defaultScanSelect.SetValueForKey(new NSNumber((ulong)(NSEventModifierMask.CommandKeyMask | NSEventModifierMask.ShiftKeyMask)), new NSString("modifierFlags"));

            defaultValues.SetValueForKey(defaultScanSelect, Constants.VALUE_HOTKEY_SCAN_SELECT);
            NSUserDefaults.StandardUserDefaults.RegisterDefaults(defaultValues);

            NSUserDefaultsController.SharedUserDefaultsController.InitialValues = defaultValues;
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            // check if we're running from the Applications folder (to make Sparkle work properly)
            LetsMove.CFunctions.PFMoveToApplicationsFolderIfNecessary();

            // setup menu
            _statusItem.Button.Image = NSImage.ImageNamed("StatusBarIcon");
            ConstructMenu();
            SetupHotKeys();
        }

        public override void WillTerminate(NSNotification notification)
        {
            foreach (var keyBinding in _hotkeyBindings.Values)
            {
                _hotkeyManager.Unbind(keyBinding);
            }
        }

        void ConstructMenu()
        {
            var menu = new NSMenu();

            var scanItem = new NSMenuItem("Scan", OnScan);
            scanItem.BindHotKey(NSUserDefaultsController.SharedUserDefaultsController, Constants.KEY_HOTKEY_SCAN_SELECT);
            menu.AddItem(scanItem);

            menu.AddItem(NSMenuItem.SeparatorItem);
            //menu.AddItem(new NSMenuItem("Preferences", OnPreferences)); // TODO
            menu.AddItem(new NSMenuItem("Check for updates", OnCheckForUpdates));
            menu.AddItem(new NSMenuItem("Quit", OnQuit));
            _statusItem.Menu = menu;
        }

        void SetupHotKeys()
        {
            _hotkeyManager = new JFHotkeyManager();

            // escape hotkey should stop scan selection
            _hotkeyBindings.Add(Constants.KEY_HOTKEY_ESC, _hotkeyManager.BindKeyRef(53, 0, this, new ObjCRuntime.Selector("onEscape")));

            // command shift b should start scanning
            var currSelectHotKey = NSUserDefaults.StandardUserDefaults.ValueForKey(Constants.VALUE_HOTKEY_SCAN_SELECT);
            if (currSelectHotKey != null)
            {
                UpdateGlobalHotKey(Constants.KEY_HOTKEY_SCAN_SELECT, currSelectHotKey, "onScan");
            }

            NSUserDefaultsController.SharedUserDefaultsController.AddObserver(this, Constants.KEY_HOTKEY_SCAN_SELECT, NSKeyValueObservingOptions.Initial, IntPtr.Zero);
        }

        [Export("onEscape")]
        void OnEscape()
        {
            NSNotificationCenter.DefaultCenter.PostNotificationName(ScanWindowController.NOTIFICATION_CLOSE, null);
        }

        [Export("onScan")]
        void OnScan()
        {
            var scanWindowController = new ScanWindowController();
            scanWindowController.ShowWindow(this);
        }

        void OnScan(object sender, EventArgs e)
        {
            OnScan();
        }

        void OnPreferences(object sender, EventArgs e)
        {
            // TODO => preference window
        }

        void OnCheckForUpdates(object sender, EventArgs e)
        {
            var updater = new Sparkle.SUUpdater();
            updater.CheckForUpdates(this);
        }

        void OnQuit(object sender, EventArgs e)
        {
            NSApplication.SharedApplication.Terminate(this);
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath == Constants.KEY_HOTKEY_SCAN_SELECT)
            {
                // todo update keys
                //UpdateHotKey(keyPath, ofObject, "onScan");
                if (_hotkeyBindings.ContainsKey(Constants.KEY_HOTKEY_SCAN_SELECT))
                {
                    _hotkeyManager.Unbind(_hotkeyBindings[Constants.KEY_HOTKEY_SCAN_SELECT]);
                    _hotkeyBindings.Remove(Constants.KEY_HOTKEY_SCAN_SELECT);
                }

                UpdateGlobalHotKey(Constants.KEY_HOTKEY_SCAN_SELECT, ofObject.ValueForKeyPath(keyPath), "onScan");
            }
            else
            {
                base.ObserveValue(keyPath, ofObject, change, context);
            }
        }

        void UpdateGlobalHotKey(NSString keyPath, NSObject currSelectHotKey, string selector)
        {
            uint keyCode = ((NSNumber)currSelectHotKey.ValueForKey(new NSString("keyCode"))).UInt32Value;
            ulong keyModifiers = ((NSNumber)currSelectHotKey.ValueForKey(new NSString("modifierFlags"))).UInt64Value;
            uint keyModifiersCarbon = ShortcutRecorder.CFunctions.SRCocoaToCarbonFlags((NSEventModifierMask)keyModifiers);
            _hotkeyBindings.Add(keyPath, _hotkeyManager.BindKeyRef(keyCode, keyModifiersCarbon, this, new ObjCRuntime.Selector(selector)));
        }
    }
}
