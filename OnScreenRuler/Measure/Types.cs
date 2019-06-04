using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OnScreenRuler {
    public enum EAxis {
        V, H
    }


    public struct PointLineIntersection {
        public MeasureLine Line { get; private set; }
        public Point P { get; private set; }
        public double Distance { get; private set; }

        public PointLineIntersection(MeasureLine l, Point p, double d) {
            Distance = d;
            P = p;
            Line = l;
        }
    }
}
