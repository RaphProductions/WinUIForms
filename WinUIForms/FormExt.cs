using Microsoft.UI.Xaml;
using WinRT;
using System.Runtime.InteropServices;
using System.IO;
using Vanara.PInvoke;
using Microsoft.UI.Windowing;
using System.Drawing;
using Microsoft.UI.Xaml.Media;
using WinUIForms;
using Microsoft.UI.Dispatching;
namespace System.Windows.Forms;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
internal interface IWindowNative
{
    IntPtr WindowHandle { get; }
}

public static class FormExt
{
    #region Window conversion
    private static void LoadIcon(Window w, string iconName)
    {
        // Since WinUI 3 doesn't have a Window.Icon property, we will use P/Invoke to set the icon
        var hwnd = w.As<IWindowNative>().WindowHandle;
        IntPtr hIcon = Vanara.PInvoke.User32.LoadImage(IntPtr.Zero, iconName,
                  Vanara.PInvoke.User32.LoadImageType.IMAGE_ICON, 16, 16, Vanara.PInvoke.User32.LoadImageOptions.LR_LOADFROMFILE);

        Vanara.PInvoke.User32.SendMessage(hwnd, Vanara.PInvoke.User32.WindowMessage.WM_SETICON, (IntPtr)0, hIcon);
    }

    /// <summary>
    ///  Move & resize an AppWindow to reflect the window bounds and start position.
    ///  
    ///  Original implementation: https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms/src/System/Windows/Forms/Form.cs#L3492
    /// </summary>
    private static void MoveAppWindowStartPosition(Form f, AppWindow aw)
    {
        switch (f.StartPosition)
        {
            case FormStartPosition.WindowsDefaultBounds:
                goto case FormStartPosition.WindowsDefaultLocation;
                break;
            case FormStartPosition.WindowsDefaultLocation:
                aw.Move(new(50, 50));
                break;
            case FormStartPosition.Manual:
                break;
            case FormStartPosition.CenterParent:
                goto case FormStartPosition.CenterScreen;
            case FormStartPosition.CenterScreen:
                Screen desktop = Screen.FromPoint(Cursor.Position);
                Rectangle screenRect = desktop.WorkingArea;

                // if, we're maximized, then don't set the x & y coordinates (they're @ (0,0) )
                if (f.WindowState != FormWindowState.Maximized)
                    aw.Move(new(Math.Max(screenRect.X, screenRect.X + (screenRect.Width - f.Width) / 2), Math.Max(screenRect.Y, screenRect.Y + (screenRect.Height - f.Height) / 2)));

                break;
        }
    }

    private static Window ConvertToWindow(Form f, SystemBackdrop bd)
    {
        var icp = Path.GetTempFileName();
        var fs = File.Open(icp, FileMode.OpenOrCreate);
        f.Icon.Save(fs);
        fs.Close();

        Window w = new();
        var whwnd = WinRT.Interop.WindowNative.GetWindowHandle(w);
        w.Title = f.Text;
        w.AppWindow.MoveAndResize(new(f.Left, f.Top, f.Size.Width, f.Size.Height));
        MoveAppWindowStartPosition(f, w.AppWindow);
        switch (f.WindowState)
        {
            case FormWindowState.Maximized:
                User32.ShowWindow(whwnd, ShowWindowCommand.SW_MAXIMIZE);
                break;
            case FormWindowState.Minimized:
                User32.ShowWindow(whwnd, ShowWindowCommand.SW_MINIMIZE);
                break;
            case FormWindowState.Normal:
                // Do nothing, since it's "normal" by default
                break;
        }

        w.SystemBackdrop = bd;

        if (bd != null)
        {
            if (bd.GetType() == typeof(MicaBackdrop))
            {
                DWMUtil.SystemBackdrop bdr = DWMUtil.SystemBackdrop.MicaRegular;

                if (((MicaBackdrop)bd).Kind == Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt)
                    bdr = DWMUtil.SystemBackdrop.MicaAlt;

                DWMUtil.SetSystemBackdrop(whwnd, bdr);
            }
            else if (bd.GetType() == typeof(DesktopAcrylicBackdrop))
                DWMUtil.SetSystemBackdrop(whwnd, DWMUtil.SystemBackdrop.AcrylicRegular);
        }

        LoadIcon(w, icp);

        // Use a ThemeListener to change the window frame theme
        ThemeListener lt = new(DispatcherQueue.GetForCurrentThread());
        DWMUtil.EnableImmersiveDarkMode(whwnd, lt.CurrentTheme == ApplicationTheme.Dark);
        lt.ThemeChanged += new((s) => DWMUtil.EnableImmersiveDarkMode(whwnd, lt.CurrentTheme == ApplicationTheme.Dark));
        return w;
    }
    #endregion

    public static Window ToWinUI(this Form f, SystemBackdrop bd = null)
    {
        Window w = ConvertToWindow(f, bd);

        // TODO: make converters

        return w;
    }
}
