﻿using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using HotKeyManager;
using PTHotKey;

namespace MenuBarCodeReader
{
    public enum EScanMode
    {
        Select,
        Click
    }

    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        NSStatusItem _statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);

        JFHotkeyManager _hotkeyManager;
        Dictionary<NSString, nuint> _hotkeyBindings = new Dictionary<NSString, nuint>();

        public AppDelegate()
        {
            var defaultValues = new NSMutableDictionary();
            SetupDefaultHotKey(Constants.VALUE_HOTKEY_SCAN_SELECT, defaultValues, "b", 11, NSEventModifierMask.CommandKeyMask | NSEventModifierMask.ShiftKeyMask);
            SetupDefaultHotKey(Constants.VALUE_HOTKEY_SCAN_CLICK, defaultValues, "c", 8, NSEventModifierMask.CommandKeyMask | NSEventModifierMask.ShiftKeyMask);
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

            var scanSelectItem = new NSMenuItem("Scan - select", OnScanSelect);
            scanSelectItem.BindHotKey(NSUserDefaultsController.SharedUserDefaultsController, Constants.KEY_HOTKEY_SCAN_SELECT);
            menu.AddItem(scanSelectItem);

            var scanClickItem = new NSMenuItem("Scan - click", OnScanClick);
            scanClickItem.BindHotKey(NSUserDefaultsController.SharedUserDefaultsController, Constants.KEY_HOTKEY_SCAN_CLICK);
            menu.AddItem(scanClickItem);

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

            var currSelectHotKey = NSUserDefaults.StandardUserDefaults.ValueForKey(Constants.VALUE_HOTKEY_SCAN_SELECT);
            if (currSelectHotKey != null)
            {
                UpdateGlobalHotKey(Constants.KEY_HOTKEY_SCAN_SELECT, currSelectHotKey, "onScanSelect");
            }
            var currClickHotKey = NSUserDefaults.StandardUserDefaults.ValueForKey(Constants.VALUE_HOTKEY_SCAN_CLICK);
            if (currClickHotKey != null)
            {
                UpdateGlobalHotKey(Constants.KEY_HOTKEY_SCAN_CLICK, currClickHotKey, "onScanClick");
            }

            NSUserDefaultsController.SharedUserDefaultsController.AddObserver(this, Constants.KEY_HOTKEY_SCAN_SELECT, NSKeyValueObservingOptions.Initial, IntPtr.Zero);
            NSUserDefaultsController.SharedUserDefaultsController.AddObserver(this, Constants.KEY_HOTKEY_SCAN_CLICK, NSKeyValueObservingOptions.Initial, IntPtr.Zero);
        }

        [Export("onEscape")]
        void OnEscape()
        {
            NSNotificationCenter.DefaultCenter.PostNotificationName(ScanWindowController.NOTIFICATION_CLOSE, null);
        }

        [Export("onScanSelect")]
        void OnScanSelect()
        {
            OnScan(EScanMode.Select);
        }

        [Export("onScanClick")]
        void OnScanClick()
        {
            OnScan(EScanMode.Click);
        }

        void OnScanSelect(object sender, EventArgs e)
        {
            OnScanSelect();
        }

        void OnScanClick(object sender, EventArgs e)
        {
            OnScanClick();
        }

        void OnScan(EScanMode scanMode)
        {
            ScanView.ScanMode = scanMode;
            var scanWindowController = new ScanWindowController();
            scanWindowController.ShowWindow(this);
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
                if (_hotkeyBindings.ContainsKey(Constants.KEY_HOTKEY_SCAN_SELECT))
                {
                    _hotkeyManager.Unbind(_hotkeyBindings[Constants.KEY_HOTKEY_SCAN_SELECT]);
                    _hotkeyBindings.Remove(Constants.KEY_HOTKEY_SCAN_SELECT);
                }

                UpdateGlobalHotKey(Constants.KEY_HOTKEY_SCAN_SELECT, ofObject.ValueForKeyPath(keyPath), "onScanSelect");
            }
            else if (keyPath == Constants.KEY_HOTKEY_SCAN_CLICK)
            {
                if (_hotkeyBindings.ContainsKey(Constants.KEY_HOTKEY_SCAN_CLICK))
                {
                    _hotkeyManager.Unbind(_hotkeyBindings[Constants.KEY_HOTKEY_SCAN_CLICK]);
                    _hotkeyBindings.Remove(Constants.KEY_HOTKEY_SCAN_CLICK);
                }

                UpdateGlobalHotKey(Constants.KEY_HOTKEY_SCAN_CLICK, ofObject.ValueForKeyPath(keyPath), "onScanClick");
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

        static void SetupDefaultHotKey(NSString key, NSMutableDictionary defaultValues, string character, int keyCode, NSEventModifierMask modifierFlags)
        {
            var defaultScanSelect = new NSMutableDictionary();
            defaultScanSelect.SetValueForKey(new NSString(character), new NSString("characters"));
            defaultScanSelect.SetValueForKey(new NSString(character.ToUpperInvariant()), new NSString("charactersIgnoringModifiers"));
            defaultScanSelect.SetValueForKey(new NSNumber(keyCode), new NSString("keyCode"));
            defaultScanSelect.SetValueForKey(new NSNumber((ulong)modifierFlags), new NSString("modifierFlags"));
            defaultValues.SetValueForKey(defaultScanSelect, key);
        }
    }
}
