using WinzorFramework.JSInterop;

namespace WinzorFramework;

public class WinzorPasteEventArgs : EventArgs
{
	public string? Text { get; set; }
	public string? Html { get; set; }
	public BrowserFile[]? Files { get; set; }
}
