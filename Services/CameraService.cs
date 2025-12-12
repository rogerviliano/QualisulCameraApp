using OpenCvSharp;
using System.Management;

using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QualisulCameraApp.Services
{
    public class CameraService : IDisposable
    {
        private VideoCapture? _capture;
        private CancellationTokenSource? _cts;
        private Task? _previewTask;
        
        public event Action<BitmapSource>? FrameArrived;





        public List<string> GetAvailableCameras()
        {
            var list = new List<string>();
            try
            {
                // Use WMI to find USB video devices. This is fast and non-blocking (mostly)
                // We look for PnPEntity with PNPClass 'Image' or 'Camera' or Service 'usbvideo'
                // Simplest is usually identifying by class.
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
                {
                    int index = 0;
                    foreach (var device in searcher.Get())
                    {
                        var name = device["Caption"]?.ToString() ?? device["Name"]?.ToString() ?? "Unknown Camera";
                        list.Add($"{index}: {name}");
                        index++;
                    }
                }

                // Fallback: If WMI finds nothing (internal cams sometimes differ), try naive "0" and "1" just in case
                if (list.Count == 0)
                {
                     // Try checking just index 0 quickly
                     list.Add("0: Câmera Padrão (Tentativa)");
                }
            }
            catch (Exception)
            {
                // Fallback completely
                 list.Add("0: Câmera Principal");
                 list.Add("1: Câmera Secundária");
            }
            return list;
        }

        public void StartCamera(int cameraIndex)
        {
            StopCamera();
            // Force DirectShow to match the DirectShowLib enumeration indices
            _capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
            
            // Retry logic if camera is slow to open
            if (!_capture.IsOpened())
            {
                Thread.Sleep(500);
                _capture.Open(cameraIndex, VideoCaptureAPIs.DSHOW);
            }
            
            if (!_capture.IsOpened()) throw new Exception("Não foi possível acessar a câmera. Verifique se ela está conectada ou sendo usada por outro programa.");

            _cts = new CancellationTokenSource();
            _previewTask = Task.Run(() => CaptureLoop(_cts.Token));
        }

        public void StopCamera()
        {
            _cts?.Cancel();
            _previewTask?.Wait(1000); // Wait for loop to finish
            _capture?.Dispose();
            _capture = null;
        }

        private void CaptureLoop(CancellationToken token)
        {
            using Mat frame = new Mat();
            while (!token.IsCancellationRequested && _capture != null && _capture.IsOpened())
            {
                _capture.Read(frame);
                if (!frame.Empty())
                {
                    // Convert to BitmapSource on UI thread or freeze it?
                    // Better to freeze so we can pass it to UI.
                    // WriteableBitmap is better for performance but WriteableBitmapConverter is needed.
                    // For now, simple ToBitmapSource.
                    
                    try 
                    {
                        var bitmap = frame.ToBitmapSource();
                        bitmap.Freeze();
                        FrameArrived?.Invoke(bitmap);
                    }
                    catch { }
                }
                Thread.Sleep(33); // ~30 FPS
            }
        }

        public BitmapSource? CaptureCurrentFrame()
        {
             // If we are already streaming, we can just pick the last frame, but FrameArrived handles preview.
             // We can just grab the frame from inside the loop if we stored it, or ask the loop to send one specific for capture.
             // Actually, the UI listens to FrameArrived. When the user clicks "Capture", the ViewModel can just use the current ImageSource it has.
             // But for higher quality (maybe different resolution?), we might want a dedicated capture. 
             // For this app, WYSIWYG is fine.
             return null; 
        }

        public void Dispose()
        {
            StopCamera();
        }
    }
}
