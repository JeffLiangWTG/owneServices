using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class OrgConsigneeModule : OrganisationModule
	{
		public OrgConsigneeModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.OrgConsigneeTracking; }
		}

		protected override ZBool MatchConsignors
		{
			get { return ZBool.False; }
		}
	}
}
