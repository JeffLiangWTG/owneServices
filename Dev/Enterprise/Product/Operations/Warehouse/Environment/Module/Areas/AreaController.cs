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
	public class AreaController : WhsControllerBase
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigArea;

		public override ControllerID ID => ControllerIDs.WhsConfigArea;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsArea);

		protected override IZForm GetForm(IBusiness businessEntity) => new AreaEntryForm((WhsArea)businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsConfigAreaView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsConfigAreaNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsConfigAreaEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsConfigAreaDelete;
	}
}
