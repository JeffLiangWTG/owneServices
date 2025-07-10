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
	public class OrderLineController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public OrderLineController()
		{
		}

		public override ControllerID ID => ControllerIDs.WhsOrderLine;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsOrderLine;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsOrderLine);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrderLineEntryForm((WhsOrderLine)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsOrder;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsOrder;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsOrder;
	}
}
