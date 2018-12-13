using System;
using GLKit;
using AVFoundation;
using CoreMedia;
using UIKit;
using OpenGLES;
using CoreImage;
using CoreGraphics;
using Foundation;
using CoreFoundation;
using CoreVideo;

namespace GLDelegateCamVideoPreview
{
    public partial class ViewController : UIViewController, IGLKViewDelegate, IAVCaptureVideoDataOutputSampleBufferDelegate
    {
        EAGLContext _eaglContext;
        AVCaptureSession _captureSession;
        GLKView _imageView;
        CIImage _cameraImage;
        CIContext _cIContext;

        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            _eaglContext = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
            _captureSession = new AVCaptureSession();
            _imageView = new GLKView();
            _cIContext = CIContext.FromContext(_eaglContext);

            InitialiseCaptureSession();

            View.AddSubview(_imageView);
            _imageView.Context = _eaglContext;
            _imageView.Delegate = this;

        }

        private void InitialiseCaptureSession()
        {
            try
            {
                _captureSession.SessionPreset = AVCaptureSession.Preset1920x1080;
                var captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video) as AVCaptureDevice;
                NSError error;
                var input = new AVCaptureDeviceInput(captureDevice, out error);
                if (error?.Code != 0)
                    Console.WriteLine($"Error {error.ToString()}");

                if (_captureSession.CanAddInput(input))
                    _captureSession.AddInput(input);

                var videoOutput = new AVCaptureVideoDataOutput();
                videoOutput.SetSampleBufferDelegateQueue(this, new DispatchQueue("sample buffer delegate"));

                if (_captureSession.CanAddOutput(videoOutput))
                    _captureSession.AddOutput(videoOutput);

                _captureSession.StartRunning();
            }
            catch (Exception ex)
            {
                int i = 0;
                i++;

            }
        }

        public override void ViewDidLayoutSubviews()
        {
            _imageView.Frame = View.Bounds;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public void DrawInRect(GLKView view, CGRect rect)
        {
            _cIContext.DrawImage(
                _cameraImage, new CGRect(0, 0, _imageView.DrawableWidth, _imageView.DrawableHeight), _cameraImage.Extent);
        }

        [Export("captureOutput:didOutputSampleBuffer:fromConnection:")]
        public void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            //connection.VideoOrientation = UIApplication.SharedApplication.StatusBarOrientation;
            using (var pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
            using (var cIImage = new CIImage(pixelBuffer))
            {
                _cameraImage = cIImage;
            }
            DispatchQueue.MainQueue.DispatchAsync(
                () => this._imageView.SetNeedsDisplay()
            );
        }
    }
}
