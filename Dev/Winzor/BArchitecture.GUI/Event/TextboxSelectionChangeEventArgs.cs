namespace WinzorFramework;

public class TextboxSelectionChangeEventArgs : EventArgs
{
	public int SelectionStart { get; set; }
	public int SelectionEnd { get; set; }
}
