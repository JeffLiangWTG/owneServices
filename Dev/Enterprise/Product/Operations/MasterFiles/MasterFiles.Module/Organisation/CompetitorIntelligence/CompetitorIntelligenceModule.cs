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
	public class CompetitorIntelligenceModule : ZFilterGridModule
	{
		#region Construction

		public CompetitorIntelligenceModule()
		{
		}

		#endregion

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CompetitorIntelligence; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.Organisation }; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CompetitorIntelligence);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrganisationFilterControl(GridCollection, (OrganisationFilterBusinessObject)FilterBusinessObject, OrgModuleType.CompetitorIntelligence);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_IsCompetitor, ZBool.True));
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CompetitorIntelligenceFilterBusinessObject();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CompetitorIntelligence; }
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
