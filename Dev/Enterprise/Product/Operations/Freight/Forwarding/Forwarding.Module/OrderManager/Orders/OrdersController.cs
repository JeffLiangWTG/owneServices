using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrdersController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ModuleIdentifier ModuleID => ModuleIDs.Orders;
		public override ControllerID ID => ControllerIDs.Orders;

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Order); }
		}

		public IZForm ShowSplitForm(Order orderToSplit, CreateOrderType splitType)
		{
			IZForm result = null;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			if (orderToSplit.IsInDatabase)
			{
				Order orderToSplitInOtherFactory = newFactory.Load<Order>(orderToSplit.PK);
				Order newSplitOrder = orderToSplitInOtherFactory.SplitOrder(splitType);
				result = ShowFormForNewEntity(newSplitOrder);
				newSplitOrder.HasChanges = true;
			}

			return result;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrdersForm((Order)businessEntity);
		}

		#region SetStrategyProvider

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			ChildEditableService.SetState(factory, ChildEditableServiceStates.Order);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.OrderTracking;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.OrderTrackingNew;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.OrderTrackingEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.OrderTrackingDelete;

		#endregion

		#region CRM Security

		protected virtual OrdersCRMSecurityProvider CRMSecurityProvider => new OrdersCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as Order, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as Order, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as Order, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
