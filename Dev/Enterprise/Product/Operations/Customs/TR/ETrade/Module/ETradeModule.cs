using CargoWise.EntityFramework;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TR.ETrade.Module
{
	public class ETradeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.TR.ETrade;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.TRETrade;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.TR.ETrade);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ETradeFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
		{
			return new ETradeFilterControl(GridCollection, (ETradeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new ETradeCollection(Factory);

		public override bool AllowNew => true;
		public override bool AllowDelete => true;
		public override bool AllowEdit => true;
		public override bool SupportsWorkflow => true;
		public override string WorkflowType => Enterprise.Customs.ASYCUDA.Business.AsycudaManifestWorkflowDescriptor.Constants.Code;
	}
}
