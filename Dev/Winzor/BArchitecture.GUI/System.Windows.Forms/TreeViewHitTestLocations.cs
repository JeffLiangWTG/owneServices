namespace System.Windows.Forms;

public enum TreeViewHitTestLocations
{
	None = 1,
	Image = 2,
	Label = 4,
	Indent = 8,
	PlusMinus = 16,
	RightOfLabel = 32,
	StateImage = 64,
	AboveClientArea = 256,
	BelowClientArea = 512,
	RightOfClientArea = 1024,
	LeftOfClientArea = 2048
}
