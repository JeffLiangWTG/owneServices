using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Module
{
	public class ManifestModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.TW.BriefCustomsDeclarations;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.AsycudaManifestReporting;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.TW.BriefCustomsDeclarations);
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject() => new ManifestBusinessObject();

		protected override IFilterControl GetNewFilterControl()
		{
			return new ASYCUDA.Module.AsycudaFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new AsycudaManifestModuleCollection(Factory);

		public override bool AllowNew => true;
		public override bool AllowDelete => true;
		public override bool AllowEdit => true;
		public override bool SupportsWorkflow => true;
		public override string WorkflowType => Enterprise.Customs.ASYCUDA.Business.AsycudaManifestWorkflowDescriptor.Constants.Code;
	}
}
