using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using HotKeyManager;

namespace MenuBarCodeReader
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        NSStatusItem _statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);

        JFHotkeyManager _hotkeyManager;
        List<nuint> _hotkeyBindings = new List<nuint>();

        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            _statusItem.Button.Image = NSImage.ImageNamed("StatusBarIcon");
            ConstructMenu();
            SetupHotKeys();
        }

        public override void WillTerminate(NSNotification notification)
        {
            foreach (var keyBinding in _hotkeyBindings)
            {
                _hotkeyManager.Unbind(keyBinding);
            }
        }

        void ConstructMenu()
        {
            var menu = new NSMenu();

            menu.AddItem(new NSMenuItem("Scan", OnScan)
            {
                KeyEquivalent = "b",
                KeyEquivalentModifierMask = NSEventModifierMask.CommandKeyMask | NSEventModifierMask.ShiftKeyMask
            });
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
            _hotkeyBindings.Add(_hotkeyManager.BindKeyRef(53, 0, this, new ObjCRuntime.Selector("onEscape")));

            // command shift b should start scanning
            _hotkeyBindings.Add(_hotkeyManager.BindKeyRef(11, (uint)(EModifierKeys.CmdKey | EModifierKeys.ShiftKey), this, new ObjCRuntime.Selector("onScan")));
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
    }
}
