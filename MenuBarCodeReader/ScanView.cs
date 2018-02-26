using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using CoreGraphics;
using System.Runtime.InteropServices;
using System.IO;
using ImageIO;
using MobileCoreServices;
using ObjCRuntime;

namespace MenuBarCodeReader
{
    public partial class ScanView : AppKit.NSView
    {
        [DllImport(Constants.CoreGraphicsLibrary)]
        static extern IntPtr CGWindowListCreateImage(CGRect screenBounds, CGWindowListOption windowOption, uint windowID, CGWindowImageOption imageOption);

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

            var bounds = BuildRect(_startLocation.Value, _endLocation.Value);

            _startLocation = null;
            _endLocation = null;

            NeedsDisplay = true;
            DisplayIfNeeded();

            Scan(bounds);
            Window.Close();
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
                var path = NSBezierPath.FromRect(BuildRect(_startLocation.Value, _endLocation.Value));
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

        CGRect BuildRect(CGPoint startPoint, CGPoint endPoint)
        {
            var x1 = startPoint.X;
            var x2 = endPoint.X;
            if (x2 < x1)
            {
                x1 = x2;
                x2 = startPoint.X;
            }

            var y1 = startPoint.Y;
            var y2 = endPoint.Y;
            if (y2 < y1)
            {
                y1 = y2;
                y2 = startPoint.Y;
            }

            return CGRect.FromLTRB(x1, y1, x2, y2);
        }

        CGRect CGRectFlipped(CGRect rect, CGRect bounds)
        {
            return new CGRect(rect.GetMinX(),
                              bounds.GetMaxY() - rect.GetMaxY(),
                              rect.Width,
                              rect.Height);
        }

        void Scan(CGRect bounds)
        {
            bounds = Window.ConvertRectToScreen(bounds);
            bounds = CGRectFlipped(bounds, NSScreen.MainScreen.Frame);

            IntPtr imageRef = CGWindowListCreateImage(bounds, CGWindowListOption.OnScreenBelowWindow, (uint)Window.WindowNumber, CGWindowImageOption.Default);
            var cgImage = new CGImage(imageRef);

            // tmp storing of image
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "scanned.png");
            var fileURL = new NSUrl(filePath, false);
            var imageDestination = CGImageDestination.Create(fileURL, UTType.PNG, 1);
            imageDestination.AddImage(cgImage);
            imageDestination.Close();


            // TODO animate background color based on result
        }

        #endregion
    }
}
