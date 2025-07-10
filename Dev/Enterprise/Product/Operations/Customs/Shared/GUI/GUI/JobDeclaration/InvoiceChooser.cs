using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	class InvoiceChooser : ZRecordChooser<BaseJobComInvoiceHeader>
	{
		public InvoiceChooser(IBusinessObjectCollection invoicesToChooseFrom) : base(ModuleIDs.CopyCommercialInvoice, invoicesToChooseFrom)
		{
			collection = invoicesToChooseFrom;
		}
		readonly IBusinessObjectCollection collection;

		protected override void ShowModalCore(Form parentForm, HandleSelectedBusinessObjects handler)
		{
			collection.SuspendValidation();
			var suspender = BaseJobComInvoiceHeaderTypeDecider.TemporarilySuspendCountrySpecificTypesForBinding();
			base.ShowModalCore(parentForm, handler);
			((IFindBox)this).PopupForm.Closed += (sender, e) =>
			{
				suspender.Dispose();
				collection.ResumeValidation();
			};
		}
	}
}
