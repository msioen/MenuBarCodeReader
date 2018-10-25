using System;

using Foundation;
using AppKit;

namespace MenuBarCodeReader
{
    public partial class PreferenceWindow : NSWindow
    {
        Settings _settings;

        IDisposable _btnResultsClipboardObserver;
        IDisposable _btnResultsNotificationCenterObserver;

        #region Constructors

        public PreferenceWindow(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public PreferenceWindow(NSCoder coder) : base(coder)
        {
        }

        #endregion

        #region Public

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Level = NSWindowLevel.ModalPanel;
            NSWindow.Notifications.ObserveWillClose(OnClose);

            _settings = new Settings();

            // setup shortcut recorder

            // setup settings
            BtnResultsClipboard.State = _settings.ShouldOutputScanResultsToClipboard ? NSCellStateValue.On : NSCellStateValue.Off;
            BtnResultsNotificationCenter.State = _settings.ShouldOutputScanResultsToNotificationCenter ? NSCellStateValue.On : NSCellStateValue.Off;

            _btnResultsClipboardObserver = BtnResultsClipboard.AddObserver(Constants.KEY_CHECKBOX_CHANGED, NSKeyValueObservingOptions.Initial, OnCheckboxChanged);
            _btnResultsNotificationCenterObserver = BtnResultsNotificationCenter.AddObserver(Constants.KEY_CHECKBOX_CHANGED, NSKeyValueObservingOptions.Initial, OnCheckboxChanged);
        }

        #endregion

        #region Private

        void OnCheckboxChanged(NSObservedChange obj)
        {
            _settings.ShouldOutputScanResultsToClipboard = BtnResultsClipboard.State == NSCellStateValue.On;
            _settings.ShouldOutputScanResultsToNotificationCenter = BtnResultsNotificationCenter.State == NSCellStateValue.On;
        }

        void OnClose(object sender, NSNotificationEventArgs e)
        {
            _btnResultsClipboardObserver?.Dispose();
            _btnResultsNotificationCenterObserver?.Dispose();
        }

        #endregion
    }
}
