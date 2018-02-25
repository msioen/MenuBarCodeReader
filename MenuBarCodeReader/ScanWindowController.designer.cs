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
	[Register ("ScanWindowController")]
	partial class ScanWindowController
	{
		[Outlet]
		AppKit.NSButton btnClose { get; set; }

		[Action ("escButton:")]
		partial void escButton (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (btnClose != null) {
				btnClose.Dispose ();
				btnClose = null;
			}
		}
	}
}
