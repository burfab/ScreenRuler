using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace OnScreenRuler {
    public static class Helper {

        static class WIN32 {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr ptr);


            /// <summary>
            /// Retrieves the cursor's position, in screen coordinates.
            /// </summary>
            /// <see>See MSDN documentation for further information.</see>
            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(out POINT lpPoint);
        }


        public const PixelFormat PIXEL_FORMAT = PixelFormat.Format32bppArgb;


        public static Bitmap BitmapFromScreen(Rectangle rect) {
            Bitmap bitmap;

            bitmap = new Bitmap(rect.Width, rect.Height, PIXEL_FORMAT);

            using (Graphics g = Graphics.FromImage(bitmap)) {
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bitmap.Size);
            }
            return bitmap;
        }
        public static System.Windows.Media.ImageSource ImageSourceFromHBitmap(Bitmap bitmap) {
            IntPtr handle = IntPtr.Zero;
            System.Windows.Media.ImageSource ret = null;
            try {
                handle = bitmap.GetHbitmap();
                ret = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                                    BitmapSizeOptions.FromEmptyOptions());
                ret.Freeze();
            } catch (Exception) {

            } finally {
                WIN32.DeleteObject(handle);
            }

            return ret;
        }

        private static byte[] imageRegionFromBitmapData(BitmapData data, System.Drawing.Rectangle region) {
            var comps = data.Stride / data.Width;
            byte[] arr = new byte[comps * region.Height * region.Width];

            var X_OFF = Math.Max(region.Left,0) * comps;
            var INITAL_OFF = Math.Max(region.Top, 0) * data.Width * comps;


            int width = region.Right > data.Width ? data.Width - region.Left : region.Width;
            int height = region.Bottom > data.Height ? data.Height - region.Top : region.Height;

            unsafe {
                int idx = 0;
                byte* p0 = (byte*)data.Scan0.ToPointer() + INITAL_OFF;

                for (int y = 0; y < height; y++) {
                    byte* p = p0 + y * (comps * data.Width) + X_OFF;
                    for (int x = 0; x < width; x++) {
                        for (int b = 0; b < comps; b++) {
                            arr[idx++] = *(p++);
                        }
                    }
                }
            }

            return arr;
        }

        private static Bitmap bitmapFromArray(PixelFormat pf, int width, int height, int comps, byte[] arr) {
            Bitmap bitmap;

            unsafe {
                fixed (byte* p0 = arr) { // stay in the fixed block until the bitmap is created, because the garbage collector could move the object around 
                    var manP = new IntPtr(p0);
                    bitmap = new Bitmap(width, height, comps * width, pf, manP);
                }
            }

            return bitmap;
        }

        public static Bitmap GetZoomedVersionOfRegion(BitmapData bitmapData, Rectangle region, double zoomX, double zoomY) {
            var byteArr = imageRegionFromBitmapData(bitmapData, region);
            var bitmapOfRegion = bitmapFromArray(bitmapData.PixelFormat, region.Width, region.Height, bitmapData.Stride / bitmapData.Width, byteArr);

            var sizeScaled = new System.Drawing.Size((int)(region.Width * zoomX), (int)(region.Height * zoomY));
            var scaled = new Bitmap(sizeScaled.Width, sizeScaled.Height);
            using (Graphics g = Graphics.FromImage(scaled)) {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(bitmapOfRegion, 0, 0, sizeScaled.Width, sizeScaled.Height);
            }
            bitmapOfRegion.Dispose();

            return scaled;
        }





        public static System.Windows.Forms.Screen GetScreenByPosition(POINT p) {
            var screens = System.Windows.Forms.Screen.AllScreens;

            foreach(var sc in screens) {
                var bounds = sc.Bounds;

                var x0 = bounds.Left;
                var x1 = bounds.Right;

                var y0 = bounds.Top;
                var y1 = bounds.Bottom;


                if (p.X >= x0 && p.X <= x1 && p.Y >= y0 && p.Y <= y1)
                    return sc;
            }

            System.Diagnostics.Debug.WriteLine($"No screen detected for coordinates {p.X}/{p.Y} ");
            return null;
        }


        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int X;
            public int Y;
        }

        public static POINT GetCursorPosition() {
            POINT lpPoint;
            WIN32.GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

    }
}
