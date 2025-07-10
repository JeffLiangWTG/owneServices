using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WorkOrderController : WhsControllerBase
	{
		public override ControllerID ID => ControllerIDs.WhsWorkOrder;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsWorkOrder;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsWorkOrder);

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			var docket = (WhsWorkOrder)businessEntity;
			((WhsWorkOrderInvoicingSupporter)docket.InvoicingSupporter).SetAuditBillingSecurity(Env.Security.WhsWorkOrderAuditBilling);
			return new WorkOrderEntryForm(docket, whsNotificationSubscriberGuiHelper);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsWorkOrderView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsWorkOrderNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsWorkOrderEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsWorkOrderDelete;
	}
}
