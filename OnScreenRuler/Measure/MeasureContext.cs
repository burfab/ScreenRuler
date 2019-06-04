using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace OnScreenRuler {



    public class MeasureContext {
        const int LINE_UNDERPOINT_MAINAXIS_TOLERANCE = 5;
        const int LINE_UNDERPOINT_SECONDARYAXIS_TOLERANCE = 3;


        public MeasureContext() {
            _currentColor = getNextColor();
        }
        public MeasureLine[] Lines => _lines.ToArray();
        public PointLineIntersection[] LinesUnderMouse => _linesUnderMouse ?? new PointLineIntersection[0];

        public Point? TempPoint => _temp_point;
        public Point? MousePosition => _mouse_position;
        public Color CurrentColor => _currentColor;


        private List<MeasureLine> _lines = new List<MeasureLine>();
        private PointLineIntersection[] _linesUnderMouse;
        private Point? _temp_point = null;
        private Point? _mouse_position = null;
        private Color _currentColor;
        private int _skippedRefreshes = -1;

        public void Reset() {
            _temp_point = null;
            _mouse_position = null;
            _lines.Clear();
            _linesUnderMouse = GetLinesUnderPoint(null);

            raiseChanged();
        }

        public bool HasMovingOrStaticPoint => !(_mouse_position == null && _temp_point == null);

        public void SetMousePosition(Point p) {
            if (_mouse_position == p)
                return;
            _mouse_position = p;
            _linesUnderMouse = GetLinesUnderPoint(p);

            raiseChanged();
        }

        public PointLineIntersection[] GetLinesUnderPoint(Point? pN) {
            if (!pN.HasValue)
                return new PointLineIntersection[0];

            var p = pN.Value;

            List<PointLineIntersection> ret = new List<PointLineIntersection>();
            foreach (var l in _lines) {
                if (pointOnLineSegment(l.P0, l.P1, p, out double distance, LINE_UNDERPOINT_MAINAXIS_TOLERANCE, LINE_UNDERPOINT_SECONDARYAXIS_TOLERANCE))
                    ret.Add(new PointLineIntersection(l, p, distance));
            }
            return ret.OrderBy(x => x.Distance).ToArray();
        }
        private static bool pointOnLineSegment(Point pt1, Point pt2, Point pt, out double distance, int toleranceMainAxis = 3, int toleranceSecondaryAxis = 0) {
            distance = double.MaxValue;

            var mainAxis = pt1.CalculateAxis(pt2, out double rad);
            int epsilonX = mainAxis == EAxis.H ? toleranceMainAxis : toleranceSecondaryAxis;
            int epsilonY = mainAxis == EAxis.V ? toleranceSecondaryAxis : toleranceMainAxis;



            if (pt.X - Math.Max(pt1.X, pt2.X) > epsilonX ||
                    Math.Min(pt1.X, pt2.X) - pt.X > epsilonX ||
                    pt.Y - Math.Max(pt1.Y, pt2.Y) > epsilonY ||
                    Math.Min(pt1.Y, pt2.Y) - pt.Y > epsilonY)
                return false;

            if (Math.Abs(pt2.X - pt1.X) < epsilonX)
                return Math.Abs(pt1.X - pt.X) < epsilonX || Math.Abs(pt2.X - pt.X) < epsilonX;
            if (Math.Abs(pt2.Y - pt1.Y) < epsilonY)
                return Math.Abs(pt1.Y - pt.Y) < epsilonY || Math.Abs(pt2.Y - pt.Y) < epsilonY;

            double x = pt1.X + (pt.Y - pt1.Y) * (pt2.X - pt1.X) / (pt2.Y - pt1.Y);
            double y = pt1.Y + (pt.X - pt1.X) * (pt2.Y - pt1.Y) / (pt2.X - pt1.X);

            distance = Math.Sqrt(Math.Pow(x - pt.X, 2) * Math.Pow(y - pt.Y, 2));
            return Math.Abs(pt.X - x) < epsilonX || Math.Abs(pt.Y - y) < epsilonY;
        }

        public void ClearMousePosition() {
            if (_mouse_position == null)
                return;
            _mouse_position = null;

            raiseChanged();
        }
        public void ClearTempPoint() {
            if (_temp_point == null)
                return;
            _temp_point = null;

            raiseChanged();
        }

        public void ApplyMousePosition() {
            if (_mouse_position == null)
                return;

            if (_temp_point == null) {
                _temp_point = _mouse_position;
                _mouse_position = null;
            } else if (_temp_point != null) {
                var li = createMLine(_temp_point.Value, _mouse_position.Value, _currentColor);
                _lines.Add(li);
                _mouse_position = _temp_point = null;
                _currentColor = getNextColor();
            }
            raiseChanged();
        }
        public int RemoveLine(params MeasureLine[] pointLineIntersection) {
            int cnt = 0;
            for (int i = 0; i < pointLineIntersection.Length; i++) {
                bool removed = _lines.Remove(pointLineIntersection[i]);
                if (removed)
                    cnt++;
            }
            _linesUnderMouse = GetLinesUnderPoint(MousePosition);
            raiseChanged();
            return cnt;
        }

        public void PauseRefreshes() {
            _skippedRefreshes = 0;
        }
        public void ContinueRefreshes() {
            bool needsRefresh = _skippedRefreshes > -1;
            _skippedRefreshes = -1;
            if (needsRefresh) {
                raiseChanged();
            }
        }


        private int colorIndex = 0;
        private Color getNextColor() {
            var colorList = Config.AppSettings.Colors;
            colorIndex = (colorIndex + 1) % colorList.Count();
            return colorList.ElementAt(colorIndex);
        }

        private static MeasureLine createMLine(Point p0, Point p1, Color col) {
            return new MeasureLine(p0, p1, col);
        }
        private void raiseChanged() {
            if(_skippedRefreshes > -1) {
                _skippedRefreshes++;
                return;
            }
            OnChange?.Invoke(this, new EventArgs());
        }

        public event EventHandler OnChange;
    }
}
