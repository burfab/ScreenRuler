using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OnScreenRuler {
    /// <summary>
    /// Interaction logic for LineTextInfo.xaml
    /// </summary>
    public partial class LineTextInfo : UserControl {
        private MeasureLine? _lineinfo;

        public LineTextInfo() {
            InitializeComponent();
        }

        public MeasureLine? LineInfo {
            get { return _lineinfo; }
            set { _lineinfo = value; refresh(); }
        }

        private void refresh() {
            string p0Text = "-", p1Text = "-";
            string degText = "-";
            string distTotalText = "-", distXText = "-", distYText = "-";


            if (_lineinfo.HasValue) {
                var li = _lineinfo.Value;

                p0Text = formatPointText(li.P0);
                p1Text = formatPointText(li.P1);
                degText = formatDegText(li.Deg);
                distTotalText = formatDistText(li.Distance,1);
                distXText = formatDistText(li.DistanceX,0);
                distYText = formatDistText(li.DistanceY,0);
            }
            p0_tb.Text = p0Text;
            p1_tb.Text = p1Text;

            deg_tb.Text = degText;
            totalDist_tb.Text = distTotalText;
            xDist_tb.Text = distXText;
            yDist_tb.Text = distYText;
        }

        private string formatDistText(double d, int decimals) {
            if (decimals < 0)
                throw new ArgumentException("Must be >= 0",nameof(decimals));
            int PAD = 4 + (decimals > 0 ? 1 + decimals : 0);

            string format = "0";
            if(decimals > 0) {
                format += ".";
                format = format.PadRight(decimals + format.Length, '0');
            }

            string ret = d.ToString(format, System.Globalization.CultureInfo.InvariantCulture).PadRight(PAD, ' ');
            return ret;
        }

        private string formatDegText(double deg) {
            var degDisplay = deg < 0 ? deg + 360 : deg;
            degDisplay += 90;
            degDisplay %= 180;
            if (degDisplay > 90)
                degDisplay = 180 - degDisplay;

            string ret = degDisplay.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture).PadLeft(5);
            return ret;
        }

        private string formatPointText(Point p) {
            const int PAD = 5;
            string ret = $"{((int)p.X).ToString().PadLeft(PAD, ' ')} / {((int)p.Y).ToString().PadRight(PAD, ' ')}";

            return ret;
        }
    }
}
