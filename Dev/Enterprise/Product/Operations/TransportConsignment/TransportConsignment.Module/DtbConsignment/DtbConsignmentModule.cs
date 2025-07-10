using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbConsignmentModule : GlowOnlyModuleThatAllowsCopy, IOperationalActionSupportable
	{
		public DtbConsignmentModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.DtbConsignment;

		protected override ControllerID ControllerID => ControllerIDs.DtbConsignment;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.LandTransport;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.DtbConsignment;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new DtbConsignmentFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new DtbConsignmentFilterControl((DtbConsignmentCollection)GridCollection, (DtbConsignmentFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new DtbConsignmentCollection(Factory);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return base.GetNewController(selectedBusinessObject);
		}

		#region Workflow

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.DtbConsignmentWorkflowDescriptorCode;

		#endregion

		public OperationalActionSupporter OperationalActionSupporter
		{
			get { return new DtbConsignmentOperationalActionSupporter(); }
		}
	}
}
