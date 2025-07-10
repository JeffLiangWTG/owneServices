
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class ClientIntelligenceController : OrganisationController
	{
		public ClientIntelligenceController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ClientIntelligence; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ClientIntelligence; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ZClientIntelligenceForm((OrgHeader)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrganisationDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ClientIntelligenceModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ClientIntelligenceModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ClientIntelligenceView; }
		}
	}
}
