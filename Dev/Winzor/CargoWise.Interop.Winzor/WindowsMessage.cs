#if DEBUG

namespace CargoWise.Interop;

public static class WindowsMessage
{
	public const int WM_DISPLAYCHANGE = 126;
	public const int WM_KEYDOWN = 256;
	public const int WM_KEYUP = 257;
	public const int WM_CHAR = 258;
	public const int WM_COMMAND = 273;
	public const int WM_LBUTTONDOWN = 513;
	public const int WM_LBUTTONUP = 514;
	public const int WM_RBUTTONDOWN = 516;
	public const int WM_CUT = 768;
	public const int WM_COPY = 769;
	public const int WM_PASTE = 770;
	public const int WM_DELETE = 771;
}
#endif
