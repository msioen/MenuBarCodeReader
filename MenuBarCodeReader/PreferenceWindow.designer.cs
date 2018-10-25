// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace MenuBarCodeReader
{
	[Register ("PreferenceWindow")]
	partial class PreferenceWindow
	{
		[Outlet]
		AppKit.NSButton BtnResultsClipboard { get; set; }

		[Outlet]
		AppKit.NSButton BtnResultsNotificationCenter { get; set; }

		[Outlet]
		ShortcutRecorder.SRRecorderControl WdgRecorderScanClick { get; set; }

		[Outlet]
		ShortcutRecorder.SRRecorderControl WdgRecorderScanSelect { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (WdgRecorderScanSelect != null) {
				WdgRecorderScanSelect.Dispose ();
				WdgRecorderScanSelect = null;
			}

			if (WdgRecorderScanClick != null) {
				WdgRecorderScanClick.Dispose ();
				WdgRecorderScanClick = null;
			}

			if (BtnResultsClipboard != null) {
				BtnResultsClipboard.Dispose ();
				BtnResultsClipboard = null;
			}

			if (BtnResultsNotificationCenter != null) {
				BtnResultsNotificationCenter.Dispose ();
				BtnResultsNotificationCenter = null;
			}
		}
	}
}
