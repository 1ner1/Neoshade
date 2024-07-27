using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using System.Windows.Forms;
using OpenTK.WinForms;
using OpenTK.Graphics.OpenGL;
namespace Neoshade {
    public class Window {
        public string Title {get; set;}
        public object hWnd {get; set;}
        public Form WinForm {get; set;}
        // cause for duplication: windows being horrible operating system
        public Form RenderMiddle {get; set;}
        public GLControl control {get; set;}

        public void Create() {
            Form form = new Form();
            form.Width = 800;
            form.Height = 800;
            hWnd = form.Handle;
            form.Text = Title;
            form.Show();
            form.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            RenderMiddle = new Form();
            RenderMiddle.Width = 1024;
            RenderMiddle.Height = 1024;
            RenderMiddle.ShowInTaskbar = false;
            RenderMiddle.Opacity = 0;
            RenderMiddle.Show();

            
            PInvoke.SetWindowLong((HWND)form.Handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)(WINDOW_EX_STYLE.WS_EX_NOACTIVATE | WINDOW_EX_STYLE.WS_EX_LAYERED | WINDOW_EX_STYLE.WS_EX_TRANSPARENT));
            
            form.TopMost = true;

            control = new GLControl();
            control.Width = form.Width;
            control.Height = form.Width;

            control.Load += (sender, eventArgs) => {
                Renderer.Initialize(control);

            };

            control.Paint += (sender, eventArgs) => {
                GL.Viewport(0, 0, this.WinForm.Width + 200, this.WinForm.Height);

                Renderer.Render(control);
            };
            


            form.Controls.Add(control);
            WinForm = form;
            
        }
    }
}