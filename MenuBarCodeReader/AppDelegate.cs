using System;
using AppKit;
using Foundation;

namespace MenuBarCodeReader
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        NSStatusItem _statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);

        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            _statusItem.Button.Image = NSImage.ImageNamed("StatusBarIcon");
            ConstructMenu();
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }

        void ConstructMenu()
        {
            var menu = new NSMenu();

            // TODO - look into 'global' hotkeys
            menu.AddItem(new NSMenuItem("Scan", OnScan));
            menu.AddItem(new NSMenuItem("Preferences", OnPreferences));
            menu.AddItem(NSMenuItem.SeparatorItem);
            menu.AddItem(new NSMenuItem("Quit", OnQuit));
            _statusItem.Menu = menu;
        }

        void OnScan(object sender, EventArgs e)
        {
            var scanWindowController = new ScanWindowController();
            scanWindowController.ShowWindow(this);
        }

        void OnPreferences(object sender, EventArgs e)
        {
            // TODO => preference window
        }

        void OnQuit(object sender, EventArgs e)
        {
            NSApplication.SharedApplication.Terminate(this);
        }
    }
}
