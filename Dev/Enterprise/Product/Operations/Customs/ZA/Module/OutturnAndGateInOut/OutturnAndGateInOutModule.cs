using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class OutturnAndGateInOutModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ZAModuleIDs.OutturnAndGateInOut;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ZAControllerIDs.OutturnAndGateInOut);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new OutturnAndGateInOutFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
		{
			return new OutturnAndGateInOutFilterStripControl(GridCollection, (OutturnAndGateInOutFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new OutturnAndGateInOutModuleCollection(Factory);

		public override SecurityCheckpoint SecurityCheckpoint => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAOutturnAndGateInOut);

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.AsycudaManifestHeaderWorkflowDescriptorCode; }
		}
	}
}
