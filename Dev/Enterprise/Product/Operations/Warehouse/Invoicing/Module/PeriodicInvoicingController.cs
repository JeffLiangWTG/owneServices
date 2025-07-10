using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.Warehouse.Invoicing.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Invoicing.Module
{
	public abstract class PeriodicInvoicingController : ZController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(PeriodicInvoicing); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			businessEntity.Factory.SetContext(BusinessContext.UseCacheToGetOrgsFromJobChargesInDb);
			return new PeriodicInvoicingForm((PeriodicInvoicing)businessEntity);
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			((PeriodicInvoicing)sourceEntity).SetFromDateReadOnlyIfFirstBilling();
			return base.ShowLoadedForm(sourceEntity, action);
		}

		#region ShowEditForm

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			IZForm result = null;

			if (IsInvoiceMissingJobHeader(sourceEntity))
			{
				result = ShouldViewInsteadOfEdit == DialogResult.Yes ? base.ShowViewForm(sourceEntity) : null; // ask user if they want to see in view mode instead of edit
			}
			else
			{
				result = base.ShowEditForm(sourceEntity);
			}

			return result;
		}

		bool IsInvoiceMissingJobHeader(IBusiness businessObject)
		{
			var invoice = businessObject as PeriodicInvoicing;
			return invoice != null && invoice.JobHeader == null;
		}

		DialogResult ShouldViewInsteadOfEdit
		{
			get { return Globals.Message.Show(CannotModifyInvoiceWithoutHeaderText, CannotModifyInvoiceWithoutHeaderCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation); }
		}

		static string CannotModifyInvoiceWithoutHeaderText
		{
			get { return Res.GetString("ea184d0e-058e-423d-9b4f-e4f1b200726d", "The selected Periodic Invoice is missing Job Header details and cannot be modified. Would you like to View it instead?"); }
		}

		static string CannotModifyInvoiceWithoutHeaderCaption
		{
			get { return Res.GetString("569d8708-ba70-4ae1-951b-422d9370bf5c", "Cannot Modify Periodic Invoice"); }
		}

		#endregion

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected abstract ZString StorageType { get; }

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var periodicInvoice = (PeriodicInvoicing)base.GetNewBusinessEntityInLocalFactory();
			periodicInvoice.ET_StorageType = StorageType;
			return periodicInvoice;
		}
	}
}
