using System.Drawing;

namespace System.Windows.Forms
{
	public class StatusBarDrawItemEventArgs : EventArgs
	{
		public StatusBarDrawItemEventArgs() { }

		public StatusBarPanel Panel { get; }

	}
}
