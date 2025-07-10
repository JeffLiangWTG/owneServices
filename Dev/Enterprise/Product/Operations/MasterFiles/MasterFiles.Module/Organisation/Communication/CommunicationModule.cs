using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class CommunicationModule : ZFilterGridModule, ICommunicationModule, IOperationalActionSupportable
	{
		public CommunicationModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Communication; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Communication);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return Header != null ? new OrgSalesCallCollection((OrgHeader)Header) : new OrgSalesCallCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CommunicationFilterControl(GridCollection, (CommunicationFilterBusinessObject)FilterBusinessObject, (OrgHeader)Header);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CommunicationFilterBusinessObject((OrgHeader)Header);
		}

		protected override bool ShowRecentItemsCore()
		{
			return base.ShowRecentItemsCore() && Header == null;
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.CommunicationWorkflowDescriptorCode; }
		}

		#region Security Checkpoint

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CommunicationManager; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.CommunicationManager; }
		}

		#endregion

		#region ICommunicationModule Members

		public IOrgHeader Header
		{
			get;
			set;
		}

		#endregion

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new CommunicationActionSupporter(); }
		}

		#endregion
	}
}
