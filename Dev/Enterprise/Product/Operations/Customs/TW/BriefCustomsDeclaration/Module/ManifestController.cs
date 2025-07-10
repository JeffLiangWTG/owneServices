using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.Customs.TW.BriefCustomsDeclaration.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Module
{
	public class ManifestController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.TW.BriefCustomsDeclarations;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.TW.BriefCustomsDeclarations;

		public override Type TypeOfTopLevelBusinessObject => typeof(AsycudaManifestHeader);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected sealed override IZForm GetForm(IBusiness businessEntity) => new BriefDeclarationForm((AsycudaManifestHeader)businessEntity);

		#region Security
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.AsycudaManifestReporting;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.AsycudaManifestReporting;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.AsycudaManifestReporting;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.AsycudaManifestReporting;

		#endregion
	}
}
