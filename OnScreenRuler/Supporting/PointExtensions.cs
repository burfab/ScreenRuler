using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnScreenRuler {
    public static class PointExtensions {
        public static EAxis CalculateAxis(this Point pt, Point pt2, out double rad) {
            rad = Math.Atan2(pt2.X - pt.X, pt2.Y - pt.Y);
            var deg = rad * 180 / Math.PI;


            var deg0H_90V = deg < 0 ? deg + 360 : deg;
            deg0H_90V += 90;
            deg0H_90V %= 180;
            if (deg0H_90V > 90)
                deg0H_90V = 180 - deg0H_90V;



            if (deg0H_90V < 45)
                return EAxis.H;
            else
                return EAxis.V;
        }
    }
}
