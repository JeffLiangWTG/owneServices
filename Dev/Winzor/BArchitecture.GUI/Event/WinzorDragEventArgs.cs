namespace WinzorFramework;

public class WinzorDragEventArgs : EventArgs {
	public string? ControlID { get; set; }

	public int ClientX { get; set; }

	public int ClientY { get; set; }
}
