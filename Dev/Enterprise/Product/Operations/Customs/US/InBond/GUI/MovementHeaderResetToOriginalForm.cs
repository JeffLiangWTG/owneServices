using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class MovementHeaderResetToOriginalForm : ZChildForm
	{
		readonly USMovementHeaderResetCollection movementHeadersToReset;

		public MovementHeaderResetToOriginalForm()
		{
		}
		public MovementHeaderResetToOriginalForm(USMovementHeaderResetCollection movementHeadersToReset)
			: base(movementHeadersToReset)
		{
			this.movementHeadersToReset = movementHeadersToReset;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			movementHeadersToReset.ToList().ForEach(x => x.RunPreSaveValidation());

			if (movementHeadersToReset.Any(x => x.HasErrors))
			{
				Globals.Message.Show("Please fix the errors first.");
			}
			else
			{
				movementHeadersToReset.ResetMoveHeadersCollectionToOriginal();
				this.DialogResult = DialogResult.OK;
				Close();
			}
		}
		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
