using System;
using System.Runtime.InteropServices;

namespace CargoWise.Interop;

public static class UnsafeNativeMethods
{
#if DEBUG

	public static bool PostMessage(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam) => false;

	public static bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam) => false;

	public static IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, int lParam) => IntPtr.Zero;

	public static IntPtr SendMessage(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam) => IntPtr.Zero;

	#endif
	public static short GetAsyncKeyState(int vkey) => 0;
}
