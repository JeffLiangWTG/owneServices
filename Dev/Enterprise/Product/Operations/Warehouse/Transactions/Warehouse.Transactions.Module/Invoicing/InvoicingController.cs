using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class InvoicingController : WhsControllerBase
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.WhsInvoicing; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsInvoicing; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsInvoice); }
		}

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			businessEntity.Factory.SetContext(BusinessContext.UseCacheToGetOrgsFromJobChargesInDb);
			return new InvoicingForm((WhsInvoice)businessEntity);
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			((WhsInvoice)sourceEntity).SetFromDateReadOnlyIfFirstBilling();
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
			var invoice = businessObject as WhsInvoice;
			return invoice != null && invoice.JobHeader == null;
		}

		DialogResult ShouldViewInsteadOfEdit
		{
			get { return Globals.Message.Show(CannotModifyInvoiceWithoutHeaderText, CannotModifyInvoiceWithoutHeaderCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation); }
		}

		static string CannotModifyInvoiceWithoutHeaderText
		{
			get { return Res.GetString("0691707d-fc4d-49c2-a0dd-04c667dd1802", "The selected Periodic Invoice is missing Job Header details and cannot be modified. Would you like to View it instead?"); }
		}

		static string CannotModifyInvoiceWithoutHeaderCaption
		{
			get { return Res.GetString("20258e03-a7a0-48af-85af-d12db953fbc6", "Cannot Modify Periodic Invoice"); }
		}

		#endregion

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WhsInvoicingNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WhsInvoicingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WhsInvoicingView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WhsInvoicingDelete; }
		}
	}
}
