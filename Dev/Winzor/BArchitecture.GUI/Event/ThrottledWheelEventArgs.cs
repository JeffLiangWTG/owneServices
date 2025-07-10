namespace WinzorFramework;

public class ThrottledWheelEventArgs : EventArgs
{
	public double DeltaY { get; set; }

	public bool CtrlKey { get; set; }
}
