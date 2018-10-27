using System;

using Foundation;
using AppKit;
using ShortcutRecorder;

namespace MenuBarCodeReader
{
    public partial class PreferenceWindow : NSWindow, ISRValidatorDelegate
    {
        Settings _settings;

        SRValidator _validator;
        ShortcutRecorderDelegate _controlDelegate;

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

            // ensure we can safely change shortcuts without executing other bound actions
            UnbindGlobalShortcuts();

            Level = NSWindowLevel.ModalPanel;
            NSWindow.Notifications.ObserveWillClose(OnClose);

            _settings = new Settings();

            // setup shortcut recorders
            _validator = new SRValidator(this);
            _controlDelegate = new ShortcutRecorderDelegate(_validator, x => PresentError(x));

            WdgRecorderScanSelect.Delegate = _controlDelegate;
            WdgRecorderScanClick.Delegate = _controlDelegate;

            var defaults = NSUserDefaultsController.SharedUserDefaultsController;
            WdgRecorderScanSelect.Bind(ShortcutRecorder.Constants.NSValueBinding, defaults, Constants.KEY_HOTKEY_SCAN_SELECT, null);
            WdgRecorderScanClick.Bind(ShortcutRecorder.Constants.NSValueBinding, defaults, Constants.KEY_HOTKEY_SCAN_CLICK, null);

            // setup settings
            BtnResultsClipboard.State = _settings.ShouldOutputScanResultsToClipboard ? NSCellStateValue.On : NSCellStateValue.Off;
            BtnResultsNotificationCenter.State = _settings.ShouldOutputScanResultsToNotificationCenter ? NSCellStateValue.On : NSCellStateValue.Off;

            _btnResultsClipboardObserver = BtnResultsClipboard.AddObserver(Constants.KEY_CHECKBOX_CHANGED, NSKeyValueObservingOptions.Initial, OnCheckboxChanged);
            _btnResultsNotificationCenterObserver = BtnResultsNotificationCenter.AddObserver(Constants.KEY_CHECKBOX_CHANGED, NSKeyValueObservingOptions.Initial, OnCheckboxChanged);
        }

        #endregion

        #region ISRValidatorDelegate

        public bool ShortcutValidator(SRValidator aValidator, ushort aKeyCode, NSEventModifierMask aFlags, out string outReason)
        {
            outReason = string.Empty;

            if (!(FirstResponder is SRRecorderControl recorder))
                return false;

            var shortcut = CFunctions.SRShortcutWithCocoaModifierFlagsAndKeyCode(aFlags, aKeyCode);
            if (IsTaken(WdgRecorderScanSelect, shortcut) ||
                IsTaken(WdgRecorderScanClick, shortcut))
            {
                outReason = "it's already used. To use this shortcut, first remove or change the other shortcut";
                return true;
            }

            return false;
        }

        bool IsTaken(SRRecorderControl recorder, NSDictionary shortcut)
        {
            return CFunctions.SRShortcutEqualToShortcut(shortcut, recorder.ObjectValue);
        }

        public bool ShortcutValidatorShouldCheckMenu(SRValidator aValidator)
        {
            return true;
        }

        public bool ShortcutValidatorShouldCheckSystemShortcuts(SRValidator aValidator)
        {
            return false;
        }

        public bool ShortcutValidatorShouldUseASCIIStringForKeyCodes(SRValidator aValidator)
        {
            return false;
        }

        #endregion

        #region Private

        void BindGlobalShortcuts()
        {
            NSNotificationCenter.DefaultCenter.PostNotificationName(Constants.NOTIFICATION_BIND_GLOBAL_SHORTCUTS, null);
        }

        void UnbindGlobalShortcuts()
        {
            NSNotificationCenter.DefaultCenter.PostNotificationName(Constants.NOTIFICATION_UNBIND_GLOBAL_SHORTCUTS, null);
        }

        void OnCheckboxChanged(NSObservedChange obj)
        {
            _settings.ShouldOutputScanResultsToClipboard = BtnResultsClipboard.State == NSCellStateValue.On;
            _settings.ShouldOutputScanResultsToNotificationCenter = BtnResultsNotificationCenter.State == NSCellStateValue.On;
        }

        void OnClose(object sender, NSNotificationEventArgs e)
        {
            _btnResultsClipboardObserver?.Dispose();
            _btnResultsNotificationCenterObserver?.Dispose();

            BindGlobalShortcuts();
        }

        #endregion
    }
}
