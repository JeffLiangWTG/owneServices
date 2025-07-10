
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class CompetitorIntelligenceController : OrganisationController
	{
		public CompetitorIntelligenceController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CompetitorIntelligence; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CompetitorIntelligence; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ZCompetitorIntelligenceForm((OrgHeader)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CompetitorIntelligenceModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CompetitorIntelligenceModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CompetitorIntelligenceModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CompetitorIntelligenceView; }
		}
	}
}
