using Gma.System.MouseKeyHook;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using DrawingColor = System.Drawing.Color;

namespace ColorPickerApp
{
    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents m_GlobalHook;

        public MainWindow()
        {
            InitializeComponent();
            Subscribe();
        }

        private void Subscribe()
        {
            // 전역 후크를 설정하여 Ctrl + Shift + C 키 조합을 감지합니다.
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyUp += GlobalHookKeyUp;
        }
        

        private void GlobalHookKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // 사용자가 'Shift + C'를 눌렀는지 확인합니다. e.Control && 
            if (e.Shift && e.KeyCode == System.Windows.Forms.Keys.C)
            {
                CaptureScreenColor();
            }
        }

        private void CaptureScreenColor()
        {
            this.Dispatcher.Invoke(() =>
            {
                // 애플리케이션 윈도우를 숨깁니다.
                this.WindowState = WindowState.Minimized;
            });

            System.Threading.Thread.Sleep(500); // 윈도우가 완전히 숨겨질 시간을 줍니다.

            POINT cursorPos;
            GetCursorPos(out cursorPos);
            var color = GetPixelColor(cursorPos.X, cursorPos.Y);

            this.Dispatcher.Invoke(() =>
            {
                // RGB 값을 표현하는 문자열
                string rgbText = $"RGB: {color.R}, {color.G}, {color.B}";

                // HEX 값을 표현하는 문자열
                string hexText = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                
                // 캡처된 색상 정보를 UI에 업데이트합니다.
                lblColorInfo.Content = rgbText + " - " + hexText;
                lblColorInfo2.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                
                Clipboard.SetText(hexText);
                
                // 애플리케이션 윈도우를 다시 보이게 합니다.
                this.WindowState = WindowState.Normal;
            });
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private DrawingColor GetPixelColor(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(IntPtr.Zero, hdc);
            DrawingColor color = DrawingColor.FromArgb((int)(pixel & 0x000000FF), (int)(pixel & 0x0000FF00) >> 8, (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            m_GlobalHook.KeyUp -= GlobalHookKeyUp;
            m_GlobalHook.Dispose();
        }
    }
}
