using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace OnScreenRuler {
    public static partial class Config {
        public static class Defaults {
            public const Key TOGGLE_KEY_DEFAULT = Key.F10;
            public const Key SHOW_HIDE_KEY_DEFAULT = Key.F11;
            public const ModifierKeys TOGGLE_KEY_MODIFIERS_DEFAULT = ModifierKeys.Control | ModifierKeys.Shift;
            public const ModifierKeys SHOW_HIDE_KEY_MODIFIERS_DEFAULT = ModifierKeys.Control | ModifierKeys.Shift;

            public static IEnumerable<Color> DRAWING_COLORS { get {
                    return new List<Color>() { Colors.Red, Colors.Pink, Colors.Azure, Colors.DarkOrange, Colors.Violet, Colors.Cyan };
                }
            }

        }
    }
}
