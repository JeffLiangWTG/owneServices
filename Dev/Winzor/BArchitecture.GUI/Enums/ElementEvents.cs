namespace WinzorFramework.Enums;

[Flags]
public enum ElementEvents : short
{
	None = 0,
	KeyDown = 1,
	KeyPress = 2,
	KeyUp = 4,
	Click = 8,
	LeftDoubleClick = 16,
	LeftMouseDown = 32,
	LeftMouseUp = 64,
	RightDoubleClick = 128,
	RightMouseDown = 256,
	RightMouseUp = 512,
	ContextMenu = 1024
}
