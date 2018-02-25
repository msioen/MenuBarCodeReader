using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using CoreGraphics;

namespace MenuBarCodeReader
{
    public partial class ScanView : AppKit.NSView
    {
        CGPoint? _startLocation;
        CGPoint? _endLocation;

        #region Constructors

        // Called when created from unmanaged code
        public ScanView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public ScanView(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        #region Public

        public override void MouseDown(NSEvent theEvent)
        {
            base.MouseDown(theEvent);

            _startLocation = theEvent.LocationInWindow;

            NeedsDisplay = true;
            DisplayIfNeeded();
        }

        public override void MouseUp(NSEvent theEvent)
        {
            base.MouseUp(theEvent);

            _endLocation = theEvent.LocationInWindow;

            Scan();
            Window.Close();

            _startLocation = null;
            _endLocation = null;

            NeedsDisplay = true;
            DisplayIfNeeded();
        }

        public override void MouseDragged(NSEvent theEvent)
        {
            base.MouseDragged(theEvent);

            _endLocation = theEvent.LocationInWindow;

            NeedsDisplay = true;
            DisplayIfNeeded();
        }

        public override void DrawRect(CGRect dirtyRect)
        {
            if (_startLocation.HasValue && _endLocation.HasValue)
            {
                var x1 = _startLocation.Value.X;
                var x2 = _endLocation.Value.X;
                if (x2 < x1)
                {
                    x1 = x2;
                    x2 = _startLocation.Value.X;
                }

                var y1 = _startLocation.Value.Y;
                var y2 = _endLocation.Value.Y;
                if (y2 < y1)
                {
                    y1 = y2;
                    y2 = _startLocation.Value.Y;
                }

                var path = NSBezierPath.FromRect(CGRect.FromLTRB(x1, y1, x2, y2));
                NSColor.Blue.SetStroke();
                NSColor.Gray.ColorWithAlphaComponent(0.2f).SetFill();
                path.LineWidth = 1;
                path.Fill();
                path.Stroke();
            }

            base.DrawRect(dirtyRect);
        }

        #endregion

        #region Private

        void Scan()
        {
            // TODO - scan current selection
        }

        #endregion
    }
}
