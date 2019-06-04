using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZZControls.Utils;

namespace OnScreenRuler {




    /// <summary>
    /// Interaction logic for MeasureWnd.xaml
    /// </summary>
    public partial class MeasureWnd : Window {

        private const double SCREEN_FONTSIZE_EM = 14;
        private FontFamily SCREEN_FONT_FAMILY => new FontFamily("Consolas");
        private FontStyle SCREEN_FONT_STYLE => FontStyles.Normal;
        private FontWeight SCREEN_FONT_WEIGHT => FontWeights.Medium;
        private double SCREEN_DPI => 1 / 96.0;


        #region fields
        private MeasureContext context = new MeasureContext();
        private System.Drawing.Bitmap _currentBitmap;
        private BitmapData _currentBitmapData;
        private Screen _screen;






        public Point? MousePosition {
            get { return (Point?)GetValue(MousePositionProperty); }
            set { SetValue(MousePositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MagnifierPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MousePositionProperty =
            DependencyProperty.Register("MousePosition", typeof(Point?), typeof(MeasureWnd), new PropertyMetadata(null, new PropertyChangedCallback(OnMousePositionChanged)));

        private static void OnMousePositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var instance = (MeasureWnd)d;
            var point = (Point?)e.NewValue;
            if (point.HasValue)
                instance.context.SetMousePosition(point.Value);
            else
                instance.context.ClearMousePosition();

            ((MeasureWnd)d).renderAreaMagnifier?.Render();
            instance.refreshMagnifier();
        }

        public double MagnifierZoom {
            get { return (double)GetValue(MagnifierZoomProperty); }
            set { SetValue(MagnifierZoomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MagnifierZoom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MagnifierZoomProperty =
            DependencyProperty.Register("MagnifierZoom", typeof(double), typeof(MeasureWnd), new PropertyMetadata(2.5d, new PropertyChangedCallback(OnZoomChanged), new CoerceValueCallback(CoerceZoom)));

        private static object CoerceZoom(DependencyObject d, object baseValue) {
            double z = (double)baseValue;
            const double MIN_ZOOM = 1;
            const double MAX_ZOOM = 5;

            if (z < MIN_ZOOM)
                return MIN_ZOOM;
            else if (z > MAX_ZOOM)
                return MAX_ZOOM;

            return z;
        }
        private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((MeasureWnd)d).renderAreaMagnifier?.Render();
            ((MeasureWnd)d).refreshMagnifier();
        }



        public int MagnifierRadius {
            get { return (int)GetValue(MagnifierRadiusProperty); }
            set { SetValue(MagnifierRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MagnifierRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MagnifierRadiusProperty =
            DependencyProperty.Register("MagnifierRadius", typeof(int), typeof(MeasureWnd), new PropertyMetadata(30, new PropertyChangedCallback(OnMagnifierRadiusChanged), new CoerceValueCallback(CoerceMagnifierRadius)));

        private static object CoerceMagnifierRadius(DependencyObject d, object baseValue) {
            int r = (int)baseValue;
            const int MIN_RADIUS = 0;
            const int MAX_RADIUS = 90;

            if (r < MIN_RADIUS)
                return MIN_RADIUS;
            else if (r > MAX_RADIUS)
                return MAX_RADIUS;

            return r;
        }

        private static void OnMagnifierRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((MeasureWnd)d).renderAreaMagnifier?.Render();
            ((MeasureWnd)d).refreshMagnifier();
        }





        #endregion fields
        public Screen Screen => _screen;


        public MeasureWnd(Screen screen) {
            InitializeComponent();
            if (screen == null)
                throw new ArgumentNullException(nameof(screen));
            _screen = screen;
            context.OnChange += context_OnChange;

            renderArea.CustomRenderingDelegate = new Action<RenderArea, DrawingVisual, System.Windows.Media.DrawingContext>(RenderMeasurings);
            renderAreaMagnifier.CustomRenderingDelegate = new Action<RenderArea, DrawingVisual, DrawingContext>(RenderMeasurings);

            this.IsVisibleChanged += MeasureWnd_IsVisibleChanged;
        }

        private void context_OnChange(object sender, EventArgs e) {
            renderArea.Render();
            renderAreaMagnifier.Render();
        }

        private void RenderMeasurings(RenderArea arg1, DrawingVisual arg2, System.Windows.Media.DrawingContext arg3) {
            const double TH_DEFAULT = 1; const double RADIUS_ELLIPSE_DEFAULT= 3;
            double stroke = TH_DEFAULT;
            double radius_ellipse = RADIUS_ELLIPSE_DEFAULT;

            var d = context;

            if (arg1 == renderAreaMagnifier) {
                if (!GetIsZoomEnabled())
                    return;

                arg3.PushTransform(GetMagnfierRenderAreaTransform());

                stroke = TH_DEFAULT / MagnifierZoom;
                radius_ellipse = RADIUS_ELLIPSE_DEFAULT / MagnifierZoom;
            }



            foreach (var l in d.Lines) {
                arg3.DrawLine(new Pen(new SolidColorBrush(l.Color), stroke), l.P0, l.P1);
            }
            if (d.LinesUnderMouse.Length > 0) {
                var nl = d.LinesUnderMouse[0].Line;

                drawPointHighlighted(nl.P0, nl.Color, arg3,radius_ellipse, stroke);
                drawPointHighlighted(nl.P1, nl.Color, arg3,radius_ellipse, stroke);

                mouseOverLine_info.LineInfo = nl;
                mouseOverLine_info.Foreground = new SolidColorBrush(nl.Color);
                mouseOverLine_info.Visibility = Visibility.Visible;
            } else
                mouseOverLine_info.Visibility = Visibility.Hidden;

            if (d.MousePosition.HasValue && d.TempPoint.HasValue) {
                var tempLine = new MeasureLine(d.MousePosition.Value, d.TempPoint.Value, d.CurrentColor);
                //drawLineText(1, tempLine, arg3);

                arg3.DrawLine(new Pen(new SolidColorBrush(tempLine.Color), stroke), tempLine.P0, tempLine.P1);
                currentLine_info.LineInfo = tempLine;
                currentLine_info.Foreground = new SolidColorBrush(tempLine.Color);
                currentLine_info.Visibility = Visibility.Visible;
            } else
                currentLine_info.Visibility = Visibility.Hidden;

            if (d.MousePosition.HasValue)
                drawPointHighlighted(d.MousePosition.Value, d.CurrentColor, arg3, radius_ellipse, stroke);

            if (d.TempPoint.HasValue) 
                drawPointHighlighted(d.TempPoint.Value, d.CurrentColor, arg3, radius_ellipse, stroke);
        }

        private Transform GetMagnfierRenderAreaTransform() {
            Matrix mtxTrans = Matrix.Identity;
            Matrix mtxScale = new Matrix(MagnifierZoom, 0, 0, MagnifierZoom, 0, 0);
            if (context.MousePosition.HasValue) 
                mtxTrans = new Matrix(1, 0, 0, 1, -context.MousePosition.Value.X, -context.MousePosition.Value.Y);

            var mtxView = Matrix.Multiply(mtxTrans, mtxScale);
            mtxView.Translate(MagnifierRadius, MagnifierRadius);

            return new MatrixTransform(mtxView);
        }

        private static void drawPointHighlighted(Point p, Color col , DrawingContext context, double radius, double stroke) {
            var brush = new SolidColorBrush(col);
            context.DrawEllipse(brush, new Pen(brush, stroke), p, radius, radius);
        }



        public bool GetIsZoomEnabled() {
            return (MagnifierZoom > 1 && MousePosition.HasValue);

        }
        private void refreshMagnifier() {
            Point center = MousePosition.GetValueOrDefault(new Point());
            int radiusMagnifier = MagnifierRadius;
            double zoom = MagnifierZoom;

            if (!GetIsZoomEnabled()) {
                magnfier.Visibility = Visibility.Collapsed;
                return;
            }
            else
                magnfier.Visibility = Visibility.Visible;



            if (_currentBitmapData == null)
                return;
            int radius = (int)(radiusMagnifier / zoom);
            Stopwatch st = new Stopwatch();
            st.Start();

            var bitmapScaled = Helper.GetZoomedVersionOfRegion(_currentBitmapData, getMagnifierRegion(center, radiusMagnifier, zoom), zoom, zoom);
            var src = Helper.ImageSourceFromHBitmap(bitmapScaled);
            magnifierImg.Source = src;
            magnfier.RenderTransform = new MatrixTransform(1, 0, 0, 1, center.X - radiusMagnifier, center.Y - radiusMagnifier);
            magnfier.Clip = new EllipseGeometry(new Point(radiusMagnifier, radiusMagnifier), radiusMagnifier, radiusMagnifier) ;

            st.Stop();
            System.Diagnostics.Debug.WriteLine("refreshing magnifier: " + st.ElapsedMilliseconds + " ms");
        }

        private System.Drawing.Rectangle getMagnifierRegion(Point center, int radiusMagnifier, double zoom) {
            //TODO: Maybe always round up?
            var region = new System.Drawing.Rectangle(
                x: (int)(center.X - radiusMagnifier / MagnifierZoom),
                y: (int)(center.Y - radiusMagnifier / MagnifierZoom),
                width: (int)(radiusMagnifier * 2 / MagnifierZoom),
                height: (int)(radiusMagnifier * 2 / MagnifierZoom));

            return region;
        }


        public void Reposition() {

            this.Width = _screen.Bounds.Width;
            this.Height = _screen.Bounds.Height;

            this.Top = _screen.Bounds.Top;
            this.Left = _screen.Bounds.Left;
        }

        public void SetScreenAsBackgroundImage() {
            Stopwatch st = new Stopwatch();
            st.Start();

            try {
                rootPanel.Visibility = Visibility.Hidden;
                cleanupCurrentBitmap();

                _currentBitmap = Helper.BitmapFromScreen(Screen.Bounds);
                _currentBitmapData = _currentBitmap.LockBits(new System.Drawing.Rectangle(0, 0, Screen.Bounds.Width, Screen.Bounds.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, _currentBitmap.PixelFormat);

                var imgsrc = Helper.ImageSourceFromHBitmap(_currentBitmap);
                imgCtrl.Source = imgsrc;
            } finally {
                rootPanel.Visibility = Visibility.Visible;
                st.Stop();
                System.Diagnostics.Debug.WriteLine("Refresh and show: " + st.ElapsedMilliseconds + " ms");
            }
        }

        private void cleanupCurrentBitmap() {
            if(_currentBitmap != null && _currentBitmapData != null)
                _currentBitmap.UnlockBits(_currentBitmapData);
            _currentBitmap = null;
            _currentBitmapData = null;
        }
        private void MeasureWnd_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if ((bool)e.NewValue) {
                Reposition();
                renderArea.Render();
                renderAreaMagnifier.Render();
            }

        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e) {
            base.OnKeyDown(e);
            switch (e.Key) {
                case Key.Delete:
                    deleteLineUnderMouse();
                    break;

            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e) {
            base.OnPreviewMouseWheel(e);
            const double CHANGE = 0.2;
            if(Keyboard.Modifiers == ModifierKeys.Control) {
                MagnifierZoom += (e.Delta > 0.5 ? CHANGE : - CHANGE);
            }
        }

        private void deleteLineUnderMouse() {
            var lines = context.GetLinesUnderPoint(context.MousePosition);
            if(lines.Length > 0)
                context.RemoveLine(lines[0].Line);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e) {
            base.OnMouseRightButtonUp(e);

            if (context.HasMovingOrStaticPoint) {
                context.ClearTempPoint();
                context.ClearMousePosition();
            }else {
                context.Reset();
            }
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);
            context.ApplyMousePosition();
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e) {
            base.OnMouseMove(e);
            var pos = e.GetPosition(null);
            MousePosition = pos;
        }

    }
}
