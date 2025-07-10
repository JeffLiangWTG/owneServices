using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	#region OrderLineController

	public class OrderLineController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.OrderLine; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.OrderLine; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrderLine); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			OrderLine line = (OrderLine)businessEntity;
			if (line.Order == null)
			{
				Order order = line.Factory.New<Order>();
				line.JO_JD = order.PK;
			}

			return new OrdersForm(line);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrderLineTracking; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrderLineTrackingNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrderLineTrackingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrderLineTrackingDelete; }
		}

		#endregion

		#region CRM Security

		readonly OrderLineCRMSecurityProvider SecurityProvider = new OrderLineCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrderLine, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrderLine, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrderLine, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}

	#endregion

	#region OrderLineFromOrderController

	public class OrderLineFromOrderController : OrderLineController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrderLineForm((OrderLine)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.OrderLineFromOrder; }
		}
	}

	#endregion
}
