using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ClientIntelligenceModule : ZFilterGridModule
	{
		public ClientIntelligenceModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ClientIntelligence; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.Organisation }; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ClientIntelligence);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrganisationFilterControl(GridCollection, (OrganisationFilterBusinessObject)FilterBusinessObject, OrgModuleType.ClientIntelligence);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_IsSalesLead, ZBool.True));
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ClientIntelligenceFilterBusinessObject();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ClientIntelligence; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.RelationshipClientIntelligence; }
		}

		#endregion
	}
}
