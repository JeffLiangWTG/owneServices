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
	public class WhsPutawayGroupController : ZController
	{
		public override ControllerID ID => ControllerIDs.WhsConfigPutawayGroup;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigPutawayGroup;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsPutawayGroup);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsConfigPutawayGroupView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsConfigPutawayGroupNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsConfigPutawayGroupEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsConfigPutawayGroupDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new WhsPutawayGroupEntryForm((WhsPutawayGroup)businessEntity);
		}
	}
}
