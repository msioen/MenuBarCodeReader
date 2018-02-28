using System;

using Foundation;
using AppKit;
using CoreGraphics;

namespace MenuBarCodeReader
{
    public partial class ScanWindow : NSWindow
    {
        #region Constructors

        public ScanWindow(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public ScanWindow(NSCoder coder) : base(coder)
        {
        }

        #endregion

        #region Public

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            StyleMask = NSWindowStyle.Borderless;
            IsOpaque = false;
            HasShadow = false;
            Level = NSWindowLevel.Floating;
            IgnoresMouseEvents = false;

            SetFrame(NSScreen.MainScreen.Frame, true);
        }

        #endregion
    }
}
