using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.MasterFiles.GUI
{
	public class TraceMessageWriter : IMessageWriter
	{
		public TraceMessageWriter(ZTextBox textBox)
		{
			this.textBox = textBox;
		}

		public void WriteMessage(string message)
		{
			textBox.AppendText(message);
			textBox.Refresh();
			textBox.ScrollToCaret();
		}

		ZTextBox textBox { get; }
	}
}
