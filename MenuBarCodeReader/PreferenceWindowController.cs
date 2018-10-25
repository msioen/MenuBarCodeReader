using System;

using Foundation;
using AppKit;

namespace MenuBarCodeReader
{
    public partial class PreferenceWindowController : NSWindowController
    {
        public PreferenceWindowController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public PreferenceWindowController(NSCoder coder) : base(coder)
        {
        }

        public PreferenceWindowController() : base("PreferenceWindow")
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }

        public new PreferenceWindow Window
        {
            get { return (PreferenceWindow)base.Window; }
        }
    }
}
