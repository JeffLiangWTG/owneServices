using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class ProofOfPaymentForm : ZForm
	{
		public ProofOfPaymentForm(CusEntryPayInfo businessObject) : base(businessObject)
		{
			InitializeComponent();
			AutoAddPreviousNextButtons = false;
			if (!DesignModeFinder.IsDesigning)
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, saveButtonUserControl);
			}

#if DEBUG
			TypeDescriptor.AddAttributes(customsOfficeDropEdit, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		#region Overrides

		public new CusEntryPayInfo BusinessEntity
		{
			get { return (CusEntryPayInfo)base.BusinessEntity; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();

			List<CusEntryPayInfo> list = new List<CusEntryPayInfo>();
			list.Add(BusinessEntity);
			if (CusEntryPayInfo.DoesDbContainConflictingRecords(BusinessEntity.Factory, BusinessEntity.C9_PaymentReference, BusinessEntity.C9_ReceiptDate, list))
			{
				Globals.Message.ShowError("This Receipt Number already exists in the database with a different Receipt Date", "Proof Of Payment Bulk Update");
				result = ContinueWithSave.No;
			}

			return result;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				saveButtonUserControl.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

