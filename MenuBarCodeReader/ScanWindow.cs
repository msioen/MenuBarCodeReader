using System;

using Foundation;
using AppKit;
using CoreGraphics;

namespace MenuBarCodeReader
{
    public class BorderlessWindow : NSWindow
    {
        #region Properties

        public override bool CanBecomeKeyWindow
        {
            get { return true; }
        }

        public override bool CanBecomeMainWindow
        {
            get { return true; }
        }

        #endregion

        #region Constructors

        public BorderlessWindow(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public BorderlessWindow(NSCoder coder) : base(coder)
        {
        }

        #endregion
    }

    public partial class ScanWindow : BorderlessWindow
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

            SetFrame(NSScreen.MainScreen.Frame, true);

            // TODO - window/view doesn't automatically get focus => fix
            MakeKeyAndOrderFront(this);
        }

        public override void KeyDown(NSEvent theEvent)
        {
            if (theEvent.KeyCode == 53)
            {
                Close();
                return;
            }

            base.KeyDown(theEvent);
        }

        #endregion
    }
}
