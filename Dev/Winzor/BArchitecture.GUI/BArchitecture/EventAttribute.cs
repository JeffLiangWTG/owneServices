namespace WinzorFramework;

[Flags]
public enum EventAttribute
{
	None = 0,
	MouseDown = 1 << 0,
	MouseUp = 1 << 1,
	Click = 1 << 2,
	ContextMenu = 1 << 3,
}
