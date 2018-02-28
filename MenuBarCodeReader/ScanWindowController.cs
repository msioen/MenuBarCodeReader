using System;

using Foundation;
using AppKit;

namespace MenuBarCodeReader
{
    public partial class ScanWindowController : NSWindowController
    {
        public const string NOTIFICATION_CLOSE = "notificationClose";

        NSObject _closeObserver;

        #region Properties

        public new ScanWindow Window
        {
            get { return (ScanWindow)base.Window; }
        }

        #endregion

        #region Constructors

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

        #endregion

        #region Public

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            _closeObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString(NOTIFICATION_CLOSE), OnCloseRequested);
        }

        public override void Close()
        {
            if (_closeObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_closeObserver);
            }
            base.Close();
        }

        public void OnCloseRequested(NSNotification notification)
        {
            Close();
        }

        #endregion
    }
}
