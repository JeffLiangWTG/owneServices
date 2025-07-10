namespace System.Windows.Forms;

public class Message
{
	public int Msg { get; set; }
	public IntPtr HWnd { get; set; }
	public IntPtr LParam { get; set; }
	public IntPtr WParam { get; set; }
}
