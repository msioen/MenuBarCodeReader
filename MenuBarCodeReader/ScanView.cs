using System;
using System.IO;
using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using ImageIO;
using MobileCoreServices;
using ZXingObjC.OSX.Binding;

namespace MenuBarCodeReader
{
    public partial class ScanView : AppKit.NSView
    {
        [DllImport(ObjCRuntime.Constants.CoreGraphicsLibrary)]
        static extern IntPtr CGWindowListCreateImage(CGRect screenBounds, CGWindowListOption windowOption, uint windowID, CGWindowImageOption imageOption);

        const string KEYPATH_BACKGROUNDCOLOR = "backgroundColor";

        static CGColor _red = new CGColor(0.86f, 0.14f, 0f, 0.7f);
        static CGColor _green = new CGColor(0.04f, 0.4f, 0.11f, 0.7f);

        CGPoint? _startLocation;
        CGPoint? _endLocation;

        bool _selecting = true;

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
            WantsLayer = true;
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

            _selecting = false;

            NeedsDisplay = true;
            DisplayIfNeeded();

            Scan(bounds);
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
            if (_selecting)
            {
                var bgPath = NSBezierPath.FromRect(Bounds);

                if (_startLocation.HasValue && _endLocation.HasValue)
                {
                    var path = NSBezierPath.FromRect(BuildRect(_startLocation.Value, _endLocation.Value));
                    path = path.BezierPathByReversingPath();
                    bgPath.AppendPath(path);

                    NSColor.Black.SetStroke();
                    path.LineWidth = 1;
                    path.Stroke();
                }

                NSColor.Gray.ColorWithAlphaComponent(0.2f).SetFill();
                bgPath.Fill();
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
            var screenBounds = Window.ConvertRectToScreen(bounds);
            screenBounds = CGRectFlipped(screenBounds, NSScreen.MainScreen.Frame);

            IntPtr imageRef = CGWindowListCreateImage(screenBounds, CGWindowListOption.OnScreenBelowWindow, (uint)Window.WindowNumber, CGWindowImageOption.Default);
            var cgImage = new CGImage(imageRef);

            //DebugHelperSaveImageToDisk(cgImage);

            var scanResults = DecodeImage(cgImage);
            if (scanResults != null)
            {
                // we scanned something => paste to clipboard
                NSPasteboard.GeneralPasteboard.ClearContents();
                NSPasteboard.GeneralPasteboard.SetStringForType(scanResults, NSPasteboard.NSPasteboardTypeString);

                AnimateBackgroundAndClose(bounds, _green);
            }
            else
            {
                // nothing scanned
                AnimateBackgroundAndClose(bounds, _red);
            }
        }

        void AnimateBackgroundAndClose(CGRect bounds, CGColor color)
        {
            var view = new NSView(bounds);
            view.WantsLayer = true;
            AddSubview(view);

            var animation = CABasicAnimation.FromKeyPath(KEYPATH_BACKGROUNDCOLOR);
            animation.From = FromObject(view.Layer.BackgroundColor);
            animation.To = FromObject(color);
            animation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);
            animation.Duration = 0.2;
            animation.AutoReverses = true;
            animation.AnimationStopped += Animation_AnimationStopped;

            view.Layer.AddAnimation(animation, KEYPATH_BACKGROUNDCOLOR);
        }

        void Animation_AnimationStopped(object sender, CAAnimationStateEventArgs e)
        {
            Window.Close();
        }

        string DecodeImage(CGImage imageToDecode)
        {
            var source = new ZXCGImageLuminanceSource(imageToDecode);
            var bitmap = ZXBinaryBitmap.BinaryBitmapWithBinarizer(ZXHybridBinarizer.BinarizerWithSource(source));

            NSError error;
            var hints = new ZXDecodeHints();
            var reader = new ZXMultiFormatReader();
            var result = reader.Decode(bitmap, hints, out error);

            if (result != null)
            {
                return result.Text;
            }
            else
            {
                return null;
            }
        }

        static void DebugHelperSaveImageToDisk(CGImage cgImage)
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "scanned.png");
            var fileURL = new NSUrl(filePath, false);
            var imageDestination = CGImageDestination.Create(fileURL, UTType.PNG, 1);
            imageDestination.AddImage(cgImage);
            imageDestination.Close();
        }

        #endregion
    }
}
