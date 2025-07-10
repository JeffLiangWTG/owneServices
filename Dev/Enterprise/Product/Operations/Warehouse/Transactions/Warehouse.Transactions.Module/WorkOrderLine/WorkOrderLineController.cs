using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	class WorkOrderLineController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrderLineEntryForm((WhsWorkOrderLine)businessEntity);
		}

		public override ControllerID ID => ControllerIDs.WhsWorkOrderLine;

		public override ModuleIdentifier ModuleID => null;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsWorkOrderLine);

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsWorkOrderDelete;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsWorkOrderEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsWorkOrderView;
	}
}
