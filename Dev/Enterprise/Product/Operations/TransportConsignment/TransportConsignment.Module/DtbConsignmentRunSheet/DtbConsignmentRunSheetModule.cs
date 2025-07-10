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
	public class DtbConsignmentRunSheetModule : GlowOnlyModuleThatAllowsCopy, IOperationalActionSupportable
	{
		public DtbConsignmentRunSheetModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Module

		protected override ControllerID ControllerID => ControllerIDs.DtbConsignmentRunSheet;

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DtbConsignmentRunSheetFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DtbConsignmentRunSheetFilterControl(GridCollection, (DtbConsignmentRunSheetFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DtbConsignmentRunSheetCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DtbConsignmentRunSheet; }
		}

		public override bool AllowDelete => true;

		#endregion

		#region Workflow

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode; }
		}

		#endregion

		#region Security

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.LandTransport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DtbConsignmentRunSheet; }
		}

		#endregion

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter
		{
			get { return new DtbConsignmentRunSheetOperationalActionSupporter(); }
		}

		#endregion
	}
}
