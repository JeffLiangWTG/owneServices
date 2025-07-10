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
	public class OrderController : WhsControllerBase
	{
		public override ControllerID ID => ControllerIDs.WhsOrder;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsOrder;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsOrder);

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			WhsOrder order = (WhsOrder)businessEntity;
			((WhsOrderInvoicingSupporter)order.InvoicingSupporter).SetAuditBillingSecurity(Env.Security.WhsOrderAuditBilling);
			return new OrderEntryForm(order, whsNotificationSubscriberGuiHelper);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsOrderView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsOrderNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsOrderEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#region CRM Security

		readonly OrderCRMSecurityProvider SecurityProvider = new OrderCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as WhsOrder, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as WhsOrder, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as WhsOrder, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
