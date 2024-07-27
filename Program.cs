using System.Security.Cryptography;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Neoshade {
    public class Program {
        public static object windowHDC;
        public static Window window;
     
        public static void Main() {
 
        

            
            Neoshade.Window window = new Neoshade.Window() {
                Title = "Neoshade"
            };

            window.Create();
            
            Program.window = window;
            
            Task.Run(() => {
                ulong ticks = 0;
                HWND desktopWindow = PInvoke.GetDesktopWindow();
                for(;;) {
                    HWND targetWindow = PInvoke.FindWindow(null, "Roblox");

                    RECT rect = new RECT();
                    PInvoke.GetWindowRect(targetWindow, out rect);
                    
                    int width = rect.Width;
                    int height = rect.Height;
                    window.WinForm.Width = width;
                    window.WinForm.Height = height;          
                    window.WinForm.Left = rect.left;
                    window.WinForm.Top = rect.top;
                              

            
                    byte[] data = Neoshade.InternalSystem.BitmapToByte(Neoshade.InternalSystem.GetFramebuffer(800, 800));
                    if(width >= 120 && height >= 120) {
                        Renderer.previousTextureData = Renderer.textureData.ToList();
                        

                        Renderer.textureData = new List<byte>(data);
                        Renderer.didUpdateFrame = true;
                        Renderer.width = width;
                        Renderer.height = height;  
                        Renderer.textureWidth = 800;
                        Renderer.textureHeight = 800;                      
                    }

                    Random rnd = new Random();


                    window.control.Width = width;
                    window.control.Height = height;

                    window.control.Refresh();
                    // PInvoke.SetForegroundWindow(targetWindow);
                    
                    // Neoshade.InternalSystem.Draw(data, width, height, 0, 0, (nint)window.hWnd);
                }            
            });


            Application.Run(window.WinForm);

        }   
    }
}