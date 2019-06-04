using System;
using System.Windows;
using System.Windows.Media;

namespace OnScreenRuler {


    public struct MeasureLine {

        public override bool Equals(object obj) {
            if (obj is MeasureLine li) {
                if (this.P0.Equals(li.P0) && this.P1.Equals(li.P1))
                    return true;

                return false;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode() {
            return P0.GetHashCode() + P1.GetHashCode();
        }

        public MeasureLine(Point p0, Point p1, Color col) {
            P0 = p0;
            P1 = p1;
            Color = col;

            Axis = p0.CalculateAxis(p1, out double rad);
            Rad = rad;
        }
        public Color Color { get; private set; }
        public Point P0 { get; private set; }
        public Point P1 { get; private set; }
        public Point Center { get {
                var x = Math.Min(P0.X, P1.X) + Math.Abs(DistanceX) / 2.0;
                var y = Math.Min(P0.Y, P1.Y) + Math.Abs(DistanceY) / 2.0;

                return new Point(x, y);
            }
        }
        public EAxis Axis { get; private set; }
        public double Rad { get; private set; }
        public double Deg => (Rad * 180 / Math.PI) % 360;

        public double DistanceX => Math.Abs(P0.X - P1.X);
        public double DistanceY => Math.Abs(P0.Y - P1.Y);

        public double Distance {
            get {
                var dX = DistanceX;
                var dY = DistanceY;

                var dist = Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));
                return dist;
            }
        }

    }
}
