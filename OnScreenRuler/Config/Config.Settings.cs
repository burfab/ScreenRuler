using System.Collections.Generic;
using System.Windows.Media;

namespace OnScreenRuler {
    public static partial class Config {
        public class Settings {

            public ShortCut ToggleMeasureWindowShortCut{ get; set; }
            public ShortCut ShowHideMeasureWindowShortCut { get; set; }
            public IEnumerable<Color> Colors { get; set; }

            public static Settings CreateDefault() {
                return new Settings() {
                    ToggleMeasureWindowShortCut = new ShortCut(Defaults.TOGGLE_KEY_DEFAULT, Defaults.TOGGLE_KEY_MODIFIERS_DEFAULT),
                    ShowHideMeasureWindowShortCut= new ShortCut(Defaults.SHOW_HIDE_KEY_DEFAULT, Defaults.SHOW_HIDE_KEY_MODIFIERS_DEFAULT),
                    Colors = Defaults.DRAWING_COLORS
                    };

            }
        }
    }
}
