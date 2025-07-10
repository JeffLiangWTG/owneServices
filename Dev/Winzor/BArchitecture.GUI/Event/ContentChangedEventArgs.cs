using WinzorFramework.JSInterop;

namespace WinzorFramework;

public class ContentChangedEventArgs : EventArgs
{
	public EditorContent? Content { get; set; }
}
