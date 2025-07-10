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
	public class ProductStyleController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Controller / Module ID

		public override ControllerID ID => ControllerIDs.WhsConfigProductStyle;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigProductStyle;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsProductStyle);

		#endregion

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity) => new ProductStyleForm((WhsProductStyle)businessEntity);

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsConfigProductStyleDelete;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsConfigProductStyleEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsConfigProductStyleNew;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsConfigProductStyleView;

		#endregion
	}
}
