using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class AllocateInBondNumberForm : ZChildForm
	{
		public AllocateInBondNumberForm(AllocateInBondNumber allocateInBondNumber)
			: base(allocateInBondNumber)
		{
			this.allocateInBondNumber = allocateInBondNumber;
		}

		readonly AllocateInBondNumber allocateInBondNumber;

		public override string FormCaption
		{
			get { return Res.GetString("AllocateInBondNumberForm|FormCaption", "Allocate In-Bond Number"); }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.InitializeComponent();
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else if (allocateInBondNumber.AI_InBondNumber.IsEmpty)
			{
				DialogResult = DialogResult.Cancel;
				var result = allocateInBondNumber.GetNextAvailableInBondNumberAndCheckReusable(out var information);
				if (information.IsEmpty)
				{
					DialogResult = DialogResult.OK;
				}
				else
				{
					if (result == ZDecimal.Zero)
					{
						Globals.Message.ShowError(information);
					}
					else
					{
						DialogResult = Globals.Message.Show(information, "Re-use Confirmation", MessageBoxButtons.OKCancel, DialogResult.Cancel);
						if (DialogResult == DialogResult.OK)
						{
							allocateInBondNumber.PostNextNumber(result);
						}
					}
				}
			}
			else
			{
				DialogResult = DialogResult.OK;
			}
		}

		protected override ZMessageBox CreateErrorMessageBox(CargoWise.EntityFramework.IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("ab9c3884-4ccc-4f65-80a6-d4c434ea16bc", "In-Bond Number"), Res.GetString("d05df6df-340c-4ff0-9259-46589885ca4c", "allocate"), Res.GetString("c90e95cb-838e-4580-a113-1b2286b32cb2", "allocated"), includeIgnoreOption);
		}
	}
}
