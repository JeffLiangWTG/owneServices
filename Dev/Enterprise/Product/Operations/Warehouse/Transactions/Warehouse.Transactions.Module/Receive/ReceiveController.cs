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
	public class ReceiveController : WhsControllerBase
	{
		public ReceiveController()
		{
		}

		public override ControllerID ID => ControllerIDs.WhsReceive;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsReceive;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsReceive);

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			return new ReceiveEntryForm((WhsReceive)businessEntity, whsNotificationSubscriberGuiHelper);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsReceiveView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsReceiveNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsReceiveEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#region CRM Security

		readonly ReceiveCRMSecurityProvider SecurityProvider = new ReceiveCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as WhsReceive, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as WhsReceive, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as WhsReceive, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
