using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OnScreenRuler {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private Dictionary<string, MeasureWnd> _MeasureWnds = new Dictionary<string,MeasureWnd>();
        private KB_Hook.KeyboardListener _KListener = new KB_Hook.KeyboardListener();

        private MeasureWnd _toggledWindow = null;
        private void Application_Startup(object sender, StartupEventArgs e) {
            initApp();

#if DEBUG
#endif
        }

        private void initApp() {
            Config.SetAppSettings(Config.Settings.CreateDefault());
            registerKeyShortCuts();
        }

        private MeasureWnd createWindowForScreen(System.Windows.Forms.Screen screen) {
            var ret = new MeasureWnd(screen);
            _MeasureWnds.Add(screen.DeviceName, ret);
            System.Diagnostics.Debug.WriteLine($"Created window for screen {screen.DeviceName}");
            return ret;
        }
        private MeasureWnd getWindowForScreen(System.Windows.Forms.Screen screen) {
            if(!_MeasureWnds.TryGetValue(screen.DeviceName, out MeasureWnd wnd)) {
                return createWindowForScreen(screen);
            }

            return wnd;
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            _KListener.Dispose();
        }

        private bool isMeasureWndVisible(System.Windows.Forms.Screen onScreen = null) {
            if (onScreen == null)
                onScreen = Helper.GetScreenByPosition(Helper.GetCursorPosition());

            if (onScreen == null) {
                MessageBox.Show("No Screen could be detected");
                return false;
            }

            var _MeasureWnd = getWindowForScreen(onScreen);
            if (_MeasureWnd.IsVisible) {
                return true;
            } else {
                return false;
            }
        }
        private void hideToggledWindow() {
            _toggledWindow?.Hide();
            _toggledWindow = null;
        }

        private MeasureWnd showMeasureWnd(System.Windows.Forms.Screen onScreen = null) {
            if (onScreen == null)
                onScreen = Helper.GetScreenByPosition(Helper.GetCursorPosition());

            if (onScreen == null) {
                MessageBox.Show("No Screen could be detected");
                return null;
            }

            var _MeasureWnd = getWindowForScreen(onScreen);
            _MeasureWnd.SetScreenAsBackgroundImage();
            if (_MeasureWnd.IsVisible) {
                System.Diagnostics.Debug.WriteLine("Already showed");
            } else {
                _MeasureWnd.Show();
            }

            return _MeasureWnd;
        }
        private void hideMeasureWnd(System.Windows.Forms.Screen onScreen = null) {
            if (onScreen == null)
                onScreen = Helper.GetScreenByPosition(Helper.GetCursorPosition());

            if (onScreen == null) {
                MessageBox.Show("No Screen could be detected");
                return;
            }

            var _MeasureWnd = getWindowForScreen(onScreen);
            _MeasureWnd.Hide();
        }

        private void registerKeyShortCuts() {
            _KListener.KeyDown += _KListener_KeyDown;
            _KListener.KeyUp += _KListener_KeyUp;
        }



        private void _KListener_KeyUp(object sender, KB_Hook.RawKeyEventArgs args) {
            if (Config.AppSettings.ToggleMeasureWindowShortCut.IsPressed(args.Key, checkModifiers()))
                hideToggledWindow();

        }
        private void _KListener_KeyDown(object sender, KB_Hook.RawKeyEventArgs args) {

            try {
                var key = args.Key;
                var mods = checkModifiers();
                if (Config.AppSettings.ToggleMeasureWindowShortCut.IsPressed(args.Key, mods))
                    _toggledWindow = _toggledWindow ?? showMeasureWnd();
                else if (Config.AppSettings.ShowHideMeasureWindowShortCut.IsPressed(args.Key, mods)) {
                    if (isMeasureWndVisible())
                        hideMeasureWnd();
                    else
                        showMeasureWnd();
                }
            } catch (Exception e) {
                handleException(e);
            }
        }

        private void handleException(Exception e) {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private ModifierKeys checkModifiers() {
            return Keyboard.Modifiers;
        }
    }
}
