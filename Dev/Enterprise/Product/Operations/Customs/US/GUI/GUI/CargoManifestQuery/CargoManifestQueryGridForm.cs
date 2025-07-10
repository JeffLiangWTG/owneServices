using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class CargoManifestQueryGridForm : ZChildForm
	{
		public CargoManifestQueryGridForm(CargoManifestQueryHeader header)
			: base(header)
		{
			header.ActionCodeInfo.ValueChanged += ActionCodeInfo_ValueChanged;
		}

		public new CargoManifestQueryHeader BusinessEntity => (CargoManifestQueryHeader)base.BusinessEntity;

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			UpdateGridVisibility();
		}

		void ActionCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateGridVisibility();
		}
		void UpdateGridVisibility()
		{
			using (LevelsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var header = BusinessEntity;
				LevelsGrid.SetAvailability(header.IsHouseBillNumberVisible, CargoManifestQueryBizObj.Schema.HouseBillNumber);
				LevelsGrid.SetAvailability(header.IsInBondNumberVisible, CargoManifestQueryBizObj.Schema.InBondNumber);
				LevelsGrid.SetAvailability(header.IsIssuerVisible, CargoManifestQueryBizObj.Schema.Issuer);
				LevelsGrid.SetAvailability(header.IsMasterBillNumberVisible, CargoManifestQueryBizObj.Schema.MasterBillNumber);
				LevelsGrid.SetColumnCaption(CargoManifestQueryBizObj.Schema.MasterBillNumber, header.IsIssuerVisible ? "Bill Number" : "Master Bill Number");

				LevelsGrid.ReOrderColumns(
					[
						CargoManifestQueryBizObj.Schema.Issuer,
						CargoManifestQueryBizObj.Schema.MasterBillNumber,
						CargoManifestQueryBizObj.Schema.HouseBillNumber,
						CargoManifestQueryBizObj.Schema.InBondNumber,
						CargoManifestQueryBizObj.Schema.RequestForRelatedBills,
						CargoManifestQueryBizObj.Schema.OutputOption
					]);
			}
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			var header = BusinessEntity;
			header.RunPreSaveValidation();

			if (header.HasErrors)
			{
				Globals.Message.ShowError(FixErrorsMessage);
			}
			else if (header.SendingObjects.Count == 0)
			{
				Globals.Message.ShowError(ThereIsNothingToSendMessagesFor);
			}
			else
			{
				if (!header.HasNotifications() || Globals.Message.Show(ThereAreNotifications, "Send messages",
					MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		const string FixErrorsMessage = "Please fix the errors first";
		const string ThereIsNothingToSendMessagesFor = "There is nothing to send.";
		const string ThereAreNotifications = "There are notifications. Do you still wish to continue?";

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
