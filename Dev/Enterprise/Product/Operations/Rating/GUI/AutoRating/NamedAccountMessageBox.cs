using System.Windows.Forms;

namespace Enterprise.Rating.GUI.AutoRating
{
	public partial class NamedAccountMessageBox : ZArchitecture.GUI.ZMessageBox
	{
		public NamedAccountMessageBox(string message)
			: base(message,
				Enterprise.Rating.GUI.Res.GetString("1c0af689-f4f2-4cbe-a801-0161e0337609", "Rates with Named Accounts"),
				MessageBoxButtons.OK,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1)
		{
			InitializeComponent();

			this.Button1.Visible = true;
			this.Button2.Visible = true;
			this.Button3.Visible = true;
		}
	}
}
