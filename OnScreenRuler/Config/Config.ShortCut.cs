using System.Windows.Input;

namespace OnScreenRuler {
    public static partial class Config {
        public class ShortCut {
            public Key Key { get; private set; }
            public ModifierKeys Modifiers { get; private set; }
            public ShortCut(Key key, ModifierKeys modifiers) {
                Key = key;
                Modifiers = modifiers;
            }

            public bool IsPressed(Key key, ModifierKeys mods) {
                return Key == key && Modifiers == mods;
            }

        }
    }
}
