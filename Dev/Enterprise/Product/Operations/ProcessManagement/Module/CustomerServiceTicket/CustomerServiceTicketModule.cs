using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Module
{
	public class CustomerServiceTicketModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public CustomerServiceTicketModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.CustomerServiceTicket;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CustomerServiceTicket);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CustomerServiceTicketFilterBusinessObject();
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new CustomerServiceTicketFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WorkRequestCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ProductivityTools;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomerServiceTicket;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode;

		#region IOperationalActionSupportable

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new CustomerServiceTicketOperationalActionSupporter(); }
		}

		#endregion
	}
}
