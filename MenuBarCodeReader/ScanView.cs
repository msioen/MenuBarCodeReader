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

        #region Properties

        public static EScanMode ScanMode { get; set; }

        #endregion

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

                if (ScanMode == EScanMode.Select && _startLocation.HasValue && _endLocation.HasValue)
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
            switch (ScanMode)
            {
                case EScanMode.Select:
                    ScanSelect(bounds);
                    break;
                case EScanMode.Click:
                    ScanClick(bounds);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        void ScanSelect(CGRect bounds)
        {
            var screenBounds = Window.ConvertRectToScreen(bounds);
            screenBounds = CGRectFlipped(screenBounds, NSScreen.MainScreen.Frame);

            IntPtr imageRef = CGWindowListCreateImage(screenBounds, CGWindowListOption.OnScreenBelowWindow, (uint)Window.WindowNumber, CGWindowImageOption.Default);
            var cgImage = new CGImage(imageRef);

            //DebugHelperSaveImageToDisk(cgImage);

            var scanResults = DecodeImage(cgImage);
            HandleDecodeResults(bounds, scanResults);
        }

        void ScanClick(CGRect bounds)
        {
            var rect = NSScreen.MainScreen.Frame;
            rect.Width = this.Bounds.Width;
            rect.Height = this.Bounds.Height;
            rect.Y = 0;

            IntPtr imageRef = CGWindowListCreateImage(rect, CGWindowListOption.OnScreenBelowWindow, (uint)Window.WindowNumber, CGWindowImageOption.Default);
            var cgImage = new CGImage(imageRef);

            //DebugHelperSaveImageToDisk(cgImage, "screen.png");

            bounds.Inflate(50, 50);

            var scanResults = DetectAndDecodeImage(cgImage, bounds);
            HandleDecodeResults(!string.IsNullOrWhiteSpace(scanResults.output) ? scanResults.bounds : bounds, scanResults.output);
        }

        void HandleDecodeResults(CGRect bounds, string scanResults)
        {
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

        (string output, CGRect bounds) DetectAndDecodeImage(CGImage imageToDecode, CGRect bounds)
        {
            var boundsHeight = Math.Max(Bounds.Height, NSScreen.MainScreen.Frame.Height);

            var source = new ZXCGImageLuminanceSource(imageToDecode);
            var bitmap = ZXBinaryBitmap.BinaryBitmapWithBinarizer(ZXHybridBinarizer.BinarizerWithSource(source));

            NSError error;
            var matrix = bitmap.BlackMatrixWithError(out error);
            if (matrix == null)
                return (null, CGRect.Null);

            var detector = new ZXMultiDetector(matrix);
            var detectorResults = detector.DetectMulti(null, out error);
            if (detectorResults == null)
                return (null, CGRect.Null);

            ZXDetectorResult bestResult = null;
            float currDistance = float.MaxValue;

            var clickRelativeX = (float)(bounds.GetMidX() / Bounds.Width * imageToDecode.Width);
            var clickRelativeY = (float)(bounds.GetMidY() / boundsHeight * imageToDecode.Height);

            foreach (var detectorResult in detectorResults)
            {
                foreach (var point in detectorResult.Points)
                {
                    var relativeX = point.X;
                    var relativeY = imageToDecode.Height - point.Y;

                    var distance = SquareDistance(clickRelativeX, clickRelativeY, relativeX, relativeY);
                    if (distance < currDistance)
                    {
                        currDistance = distance;
                        bestResult = detectorResult;
                    }
                }
            }

            var reader = new ZXQRCodeDecoder();
            var result = reader.DecodeMatrix(bestResult.Bits, out error);

            var x = bestResult.Points[1].X;
            var y = bestResult.Points[1].Y;
            var width = bestResult.Points[2].X - bestResult.Points[1].X;
            var height = bestResult.Points[2].Y - bestResult.Points[0].Y;

            var scanResult = CGRectFlipped(new CGRect(x, y, width, height), new CGRect(0, 0, imageToDecode.Width, imageToDecode.Height));
            scanResult.Inflate(50, -50);

            DebugHelperDrawScanResult(imageToDecode, clickRelativeX, clickRelativeY, scanResult);

            x = (float)(scanResult.X / imageToDecode.Width * NSScreen.MainScreen.Frame.Width);
            y = (float)((scanResult.Y + scanResult.Height) / imageToDecode.Height * boundsHeight);
            width = (float)(scanResult.Width / imageToDecode.Width * NSScreen.MainScreen.Frame.Width);
            height = (float)(scanResult.Height / imageToDecode.Height * boundsHeight);

            var rect = new CGRect(x, y, width, height);

            if (result != null)
            {
                return (result.Text, rect);
            }
            else
            {
                return (null, CGRect.Null);
            }
        }

        static float SquareDistance(float x1, float y1, float x2, float y2)
        {
            return ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        static void DebugHelperSaveImageToDisk(CGImage cgImage, string filename = "scanned.png")
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);
            var fileURL = new NSUrl(filePath, false);
            var imageDestination = CGImageDestination.Create(fileURL, UTType.PNG, 1);
            imageDestination.AddImage(cgImage);
            imageDestination.Close();
        }

        static void DebugHelperDrawScanResult(CGImage imageToDecode, float clickRelativeX, float clickRelativeY, CGRect scanResult)
        {
            using (var ctx = new CGBitmapContext(IntPtr.Zero, imageToDecode.Width, imageToDecode.Height, imageToDecode.BitsPerComponent, imageToDecode.BytesPerRow, imageToDecode.ColorSpace, imageToDecode.BitmapInfo))
            {
                ctx.DrawImage(new CGRect(0, 0, imageToDecode.Width, imageToDecode.Height), imageToDecode);

                ctx.SetStrokeColor(new CGColor(255, 0, 0, 0.6f));
                ctx.SetFillColor(new CGColor(255, 0, 0, 0.4f));
                ctx.SetLineJoin(CGLineJoin.Round);
                ctx.SetLineWidth(4);

                ctx.BeginPath();
                ctx.MoveTo(scanResult.GetMinX(), scanResult.GetMinY());
                ctx.AddLineToPoint(scanResult.GetMinX(), scanResult.GetMaxY());
                ctx.AddLineToPoint(scanResult.GetMaxX(), scanResult.GetMaxY());
                ctx.AddLineToPoint(scanResult.GetMaxX(), scanResult.GetMinY());
                ctx.AddLineToPoint(scanResult.GetMinX(), scanResult.GetMinY());
                ctx.ClosePath();
                ctx.FillPath();
                ctx.StrokePath();

                ctx.SetFillColor(new CGColor(0, 255, 0, 0.4f));

                ctx.BeginPath();
                ctx.MoveTo(clickRelativeX - 25, clickRelativeY - 25);
                ctx.AddLineToPoint(clickRelativeX - 25, clickRelativeY + 25);
                ctx.AddLineToPoint(clickRelativeX + 25, clickRelativeY + 25);
                ctx.AddLineToPoint(clickRelativeX + 25, clickRelativeY - 25);
                ctx.AddLineToPoint(clickRelativeX - 25, clickRelativeY - 25);
                ctx.ClosePath();
                ctx.FillPath();
                ctx.StrokePath();

                var test = ctx.ToImage();
                DebugHelperSaveImageToDisk(test);
            }
        }

        #endregion
    }
}
