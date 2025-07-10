using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.GUI;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	class OutturnAndGateInOutController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ZAModuleIDs.OutturnAndGateInOut;

		public override ControllerID ID => ZAControllerIDs.OutturnAndGateInOut;

		public override Type TypeOfTopLevelBusinessObject => typeof(AsycudaManifestHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new OutturnAndGateInOutForm((AsycudaManifestHeader)businessEntity);

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAOutturnAndGateInOut);

		protected override SecurityCheckpoint CheckPointForView => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAOutturnAndGateInOut);

		protected override SecurityCheckpoint CheckPointForNew => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAOutturnAndGateInOut);

		protected override SecurityCheckpoint CheckPointForDelete => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAOutturnAndGateInOut);

		#endregion
	}
}
