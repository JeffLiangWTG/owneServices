using System.Globalization;
using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class NewNumberFountainDialog : ZChildForm
	{
		public NewNumberFountainDialog(INumberFountainProxy fountain)
		{
			this.Fountain = fountain;
			CurrentSequenceNumberLabel.Text = Res.GetString("NewNumberFountainDialog|CurrentSequenceNumberLabel", "Current Sequence Number is:") + " " + fountain.PeekPreliminary(Db.Connection).ToString(CultureInfo.CurrentCulture);
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}

		readonly INumberFountainProxy Fountain;

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

		#region Implementation

		internal void OKButton_Click(object sender, System.EventArgs e)
		{
			if (int.TryParse(NewSequenceNumberCalcEdit.Text, NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out int result)
				&& result > 0)
			{
				using (var transactionManager = Db.Connection.BeginTransactionWithManager()) // Number fountain testing requires transaction level 2.
				{
					Fountain.SetNext(Db.Connection, result);
					transactionManager.CommitTransaction(); // Number fountain testing requires transaction level 2.
				}

				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				NewSequenceNumberCalcEdit.Text = Res.GetString("a0aa281b-feb0-4181-b187-7fa4dd2a73f6", "Invalid");
			}
		}

		internal void CloseButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion
	}
}
