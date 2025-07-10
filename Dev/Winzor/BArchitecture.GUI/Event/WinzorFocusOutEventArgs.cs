namespace WinzorFramework;

public class WinzorFocusOutEventArgs : EventArgs
{
	public bool InitiatedFromServer { get; set; }
	public string? TargetWinzorControlId { get; set; }
	public string? RelatedTargetWinzorControlId { get; set; }
}
