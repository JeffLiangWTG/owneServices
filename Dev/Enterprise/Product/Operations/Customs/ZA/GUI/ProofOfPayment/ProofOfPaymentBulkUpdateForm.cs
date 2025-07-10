using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class ProofOfPaymentBulkUpdateForm : ZForm
	{
		public ProofOfPaymentBulkUpdateForm(CusEntryPayInfoBulkUpdateBusinessObject bo) : base(bo)
		{
			this.CaptionRenderingEnabled = true;
			InitializeComponent();
			AutoAddPreviousNextButtons = false;
			ZFormPostingButtonsStrategy.SetupPosting(this, posingButtons);
			BuildDescriptionsAndValues();
			posingButtons.AllowOutsideOfParent();
			MainStatusBar.AllowOutsideOfParent();
		}

		#region Overrides

		public new CusEntryPayInfoBulkUpdateBusinessObject BusinessEntity
		{
			get { return (CusEntryPayInfoBulkUpdateBusinessObject)base.BusinessEntity; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();

			List<CusEntryPayInfo> list = new List<CusEntryPayInfo>();
			BusinessEntity.SelectedPayInfos.CopyToList(list);
			if (CusEntryPayInfo.DoesDbContainConflictingRecords(BusinessEntity.Factory, BusinessEntity.ReceiptNumber, BusinessEntity.ReceiptDate, list))
			{
				Globals.Message.ShowError("This Receipt Number already exists in the database with a different Receipt Date", "Proof Of Payment Bulk Update");
				result = ContinueWithSave.No;
			}

			return result;
		}

		#endregion

		void BuildDescriptionsAndValues()
		{
			var bulkUpdate = BusinessEntity;
			if (bulkUpdate != null && bulkUpdate.SelectedPayInfos.Count > 0)
			{
				this.descriptionLabel.Text = string.Format(CultureInfo.InvariantCulture, "You are about to perform a Bulk Update on {0} record{1}.",
					bulkUpdate.SelectedPayInfos.Count, (bulkUpdate.SelectedPayInfos.Count != 1 ? "s" : ""));
				this.warningLabel.Text = ZString.Empty;

				var receiptNumber = bulkUpdate.SelectedPayInfos[0].C9_PaymentReference.ToUpper();
				var receiptDate = bulkUpdate.SelectedPayInfos[0].C9_ReceiptDate;
				if (bulkUpdate.SelectedPayInfos.Count > 1)
				{
					for (int i = 1; i < bulkUpdate.SelectedPayInfos.Count; i++)
					{
						CusEntryPayInfo payinfo = bulkUpdate.SelectedPayInfos[i];
						if (payinfo.C9_PaymentReference.ToUpper() != receiptNumber || payinfo.C9_ReceiptDate != receiptDate)
						{
							receiptNumber = ZString.Empty;
							receiptDate = ZDate.Empty;
							this.warningLabel.Text = "WARNING! The selected records have different Receipt Numbers. Proceeding will override these values.";
							break;
						}
					}
				}

				using (bulkUpdate.SuspendSettingHasChanges())
				{
					bulkUpdate.ReceiptNumber = receiptNumber;
					bulkUpdate.ReceiptDate = receiptDate;
				}
			}
		}
	}
}

