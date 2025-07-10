using Microsoft.AspNetCore.Components.Web;

namespace WinzorFramework;

public class RichTextBoxContextMenuEventArgs : PointerEventArgs
{
	public int SelectionStart { get; set; }
	public int SelectionLength { get; set; }
}
