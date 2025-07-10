using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class OrgConsignorModule : OrganisationModule
	{
		public OrgConsignorModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ModuleIdentifier ID => WebModuleIDs.OrgConsignorTracking;

		protected override ZBool MatchConsignees => ZBool.False;
	}
}
