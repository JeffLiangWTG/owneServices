using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public class WarehouseController : WhsControllerBase
	{
		public override ControllerID ID => ControllerIDs.WhsConfigWarehouse;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigWarehouse;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsWarehouse);

		protected override IZForm GetForm(IBusiness businessEntity) => new WarehouseEntryForm((WhsWarehouse)businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsConfigWarehouseView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsConfigWarehouseNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsConfigWarehouseEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsConfigWarehouseDelete;
	}
}
