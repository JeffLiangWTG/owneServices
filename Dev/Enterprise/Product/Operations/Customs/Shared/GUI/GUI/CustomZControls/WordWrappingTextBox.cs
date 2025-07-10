using System.Windows.Forms;

namespace Enterprise.Customs.GUI
{
	public class WordWrappingTextBox : ZArchitecture.ZTextBox
	{
		public WordWrappingTextBox() : base()
		{
			WordWrap = true;
			Multiline = true;
			AcceptsReturn = false;
			AcceptsTab = false;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter && !AcceptsReturn)
			{
				e.SuppressKeyPress = true;
				e.Handled = true;
			}

			base.OnKeyDown(e);
		}
	}
}
