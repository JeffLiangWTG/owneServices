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
	public class CompletionTriggerActionModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CompletionTriggerAction;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WorkflowCompletionTriggerAction;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Workflow;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CompletionTriggerAction);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ProcessTaskNotificationCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CompletionTriggerActionFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CompletionTriggerActionFilterBusinessObject();
		}

		#region Allowed actions

		public override bool AllowNew => false;

		public override bool AllowView => false;

		public override bool AllowEdit => false;

		public override bool AllowDelete => false;

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#endregion
	}
}
