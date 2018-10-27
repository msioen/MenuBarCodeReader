using System;
using AppKit;
using Foundation;
using ShortcutRecorder;

namespace MenuBarCodeReader
{
    public class ShortcutRecorderDelegate : SRRecorderControlDelegate
    {
        readonly SRValidator _validator;
        readonly Action<NSError> _onError;

        #region Constructors

        public ShortcutRecorderDelegate(SRValidator validator, Action<NSError> onError)
        {
            _validator = validator;
            _onError = onError;
        }

        #endregion

        #region Public

        public override bool ShortcutRecorderCanRecordShortcut(SRRecorderControl aRecorder, NSDictionary aShortcut)
        {
            var isTaken = _validator.IsKeyCode(
                ((NSNumber)aShortcut[ShortcutRecorder.Constants.SRShortcutKeyCode]).UInt16Value,
                (NSEventModifierMask)((NSNumber)aShortcut[ShortcutRecorder.Constants.SRShortcutModifierFlagsKey]).UInt64Value,
                out NSError error);

            if (isTaken)
            {
                AppKitFramework.NSBeep();
                _onError?.Invoke(error);
            }
            return !isTaken;
        }

        public override bool ShortcutRecorderShouldBeginRecording(SRRecorderControl aRecorder)
        {
            return true;
        }

        public override void ShortcutRecorderDidEndRecording(SRRecorderControl aRecorder)
        {
        }

        public override bool ShortcutRecorderShouldUnconditionallyAllowModifierFlags(SRRecorderControl aRecorder, NSEventModifierMask aModifierFlags, ushort aKeyCode)
        {
            // Keep required flags required.
            if ((aModifierFlags & aRecorder.RequiredModifierFlags) != aRecorder.RequiredModifierFlags)
                return false;

            // Don't allow disallowed flags.
            if ((aModifierFlags & aRecorder.AllowedModifierFlags) != aModifierFlags)
                return false;

            switch (aKeyCode)
            {
                case (ushort)EKeyCode.kVK_F1:
                case (ushort)EKeyCode.kVK_F2:
                case (ushort)EKeyCode.kVK_F3:
                case (ushort)EKeyCode.kVK_F4:
                case (ushort)EKeyCode.kVK_F5:
                case (ushort)EKeyCode.kVK_F6:
                case (ushort)EKeyCode.kVK_F7:
                case (ushort)EKeyCode.kVK_F8:
                case (ushort)EKeyCode.kVK_F9:
                case (ushort)EKeyCode.kVK_F10:
                case (ushort)EKeyCode.kVK_F11:
                case (ushort)EKeyCode.kVK_F12:
                case (ushort)EKeyCode.kVK_F13:
                case (ushort)EKeyCode.kVK_F14:
                case (ushort)EKeyCode.kVK_F15:
                case (ushort)EKeyCode.kVK_F16:
                case (ushort)EKeyCode.kVK_F17:
                case (ushort)EKeyCode.kVK_F18:
                case (ushort)EKeyCode.kVK_F19:
                case (ushort)EKeyCode.kVK_F20:
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}
