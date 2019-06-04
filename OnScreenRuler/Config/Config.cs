using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnScreenRuler {
    public static partial class Config {
        public static Settings AppSettings { get; private set; }



        public static void SetAppSettings(Settings settings) {
            AppSettings = settings;
        }
    }
}
