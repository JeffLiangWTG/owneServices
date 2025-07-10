using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class ReconController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.US.Recon;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.Reconciliation;

		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override IZForm GetForm(IBusiness businessEntity) => new ReconDeclarationForm(new ReconDeclaration((JobDeclaration)businessEntity));

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var reconDeclaration = sourceEntity as ReconDeclaration;
			return base.GetLoadedBusinessEntityInLocalFactory(reconDeclaration == null ? sourceEntity : reconDeclaration.ReconWrappedJobDeclaration);
		}

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.USReconModify;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.USReconModify;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.USReconModify;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.USReconView;

		protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
		{
			Globals.Message.ShowInformation(MultipleDeactivationNotSupported);
		}
		internal const string MultipleDeactivationNotSupported = "Reconciliation jobs should be deactivated/activated individually.";
	}
}
