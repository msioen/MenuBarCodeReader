using System;

using Foundation;
using AppKit;

namespace MenuBarCodeReader
{
    public partial class ScanWindowController : NSWindowController
    {
        public ScanWindowController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public ScanWindowController(NSCoder coder) : base(coder)
        {
        }

        public ScanWindowController() : base("ScanWindow")
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }

        public new ScanWindow Window
        {
            get { return (ScanWindow)base.Window; }
        }
    }
}
