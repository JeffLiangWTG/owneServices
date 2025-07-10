using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefFacilityController : ZController
	{
		public override ControllerID ID => ControllerIDs.RefFacility;

		public override ModuleIdentifier ModuleID => ModuleIDs.RefFacility;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefFacility);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.RefFacilityView;

		protected override SecurityCheckpoint CheckPointForNew => new RefFacilityNewSecurityCheckpoint();

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.RefFacilityEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.RefFacilityDelete;

		protected override IZForm GetForm(IBusiness businessEntity) => new RefFacilityForm((RefFacility)businessEntity);
	}
}
