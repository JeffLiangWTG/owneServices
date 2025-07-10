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
	public class RowController : WhsControllerBase
	{
		public override ControllerID ID => ControllerIDs.WhsConfigRow;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigRow;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsRow);

		protected override IZForm GetForm(IBusiness businessEntity) => new RowEntryForm((WhsRow)businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsConfigLocationView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsConfigLocationNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsConfigLocationEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsConfigLocationDelete;
	}
}
