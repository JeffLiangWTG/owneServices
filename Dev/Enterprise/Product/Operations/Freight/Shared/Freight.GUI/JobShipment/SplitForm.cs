using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class SplitForm : ZChildForm
	{
		public SplitForm(PackLineSplitter splitter) : base(splitter)
		{
			InitializeComponent();
			this.Splitter = splitter;
		}

		public readonly PackLineSplitter Splitter;

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Buttons

		void SplitButton_Click(object sender, System.EventArgs e)
		{
			ZBool result = Splitter.Split();
			if (result)
			{
				this.Close();
			}
			else
			{
				Globals.Message.Show(Res.GetString("f5e46aa0-469d-4dc4-91bd-e88a3c9f7338", "There are errors, please correct them before splitting."), Res.GetString("7712f034-b7b7-4808-b5b8-2c02233ec4f5", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion
	}
}
