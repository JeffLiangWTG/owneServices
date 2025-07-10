using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowExceptionTypesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WorkflowExceptionTypes;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WorkflowExceptionTypes);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WorkflowExceptionTypesFilterControl(GridCollection, (WorkflowExceptionTypesFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ProcessWorkflowExceptionTypeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WorkflowExceptionTypesFilterBusinessObject();
		}

		#region CheckPoints

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Workflow;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WorkflowExceptionTypes;

		#endregion
	}
}
