using Windows.Win32;
using Windows.Win32.Foundation;

namespace Neoshade;

public static class RemoteCommunication {
    public enum MOUSE_MESSAGES {
        WM_LBUTTONDOWN = 0x201, 
        WM_LBUTTONUP = 0x202,   
        WM_LBUTTONDBLCLK = 0x203,
        WM_RBUTTONDOWN = 0x204,
        WM_RBUTTONUP = 0x205,
        WM_RBUTTONDBLCLK = 0x206,
        WM_MOUSEMOVE = 0x0200
    }
    public static void SendClick(bool leftClick, int x, int y, object window) {
        
        HWND hWND = (HWND)window;
        PInvoke.PostMessage(hWND, (uint)MOUSE_MESSAGES.WM_MOUSEMOVE, 0, PInvoke.MAKELPARAM((ushort)x, (ushort)y));

        PInvoke.PostMessage(hWND, (uint)(leftClick ? MOUSE_MESSAGES.WM_LBUTTONDOWN : MOUSE_MESSAGES.WM_RBUTTONDOWN), 1, PInvoke.MAKELPARAM((ushort)x, (ushort)y));
        PInvoke.PostMessage(hWND, (uint)(leftClick ? MOUSE_MESSAGES.WM_LBUTTONUP : MOUSE_MESSAGES.WM_RBUTTONUP), 0, PInvoke.MAKELPARAM((ushort)x, (ushort)y));

    }
}