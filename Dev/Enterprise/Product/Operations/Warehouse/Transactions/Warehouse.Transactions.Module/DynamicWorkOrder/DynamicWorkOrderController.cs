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
	public class DynamicWorkOrderController : WhsControllerBase
	{
		public override ControllerID ID => ControllerIDs.WhsDynamicWorkOrder;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsDynamicWorkOrder;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsDynamicWorkOrder);

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			return new DynamicWorkOrderEntryForm((WhsDynamicWorkOrder)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsDynamicWorkOrderView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsDynamicWorkOrderNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsDynamicWorkOrderEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsDynamicWorkOrderDelete;
	}
}
