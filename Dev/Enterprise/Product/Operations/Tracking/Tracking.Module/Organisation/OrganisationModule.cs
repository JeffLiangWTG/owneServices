using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using ZWebModules = Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class OrganisationModule : ZWebModules.OrganisationModule
	{
		public OrganisationModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			var fFilterBusinessObjectDefaults = base.GetNewFilterBusinessObjectDefaults();
			var currentOrgGuid = (Page != null && Page.SiteUser != null && Page.SiteUser.IsLoggedIn) ? ((OrgContactWebUser)Page.SiteUser).CurrentOrg : ZGuid.NewZGuid();
			fFilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ZWebModules.AutoOrganisationFilterBusinessObject.Schema.OH_RelatedConsign, currentOrgGuid));
			fFilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ZWebModules.AutoOrganisationFilterBusinessObject.Schema.OH_IsConsignee, MatchConsignees));
			fFilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ZWebModules.AutoOrganisationFilterBusinessObject.Schema.OH_IsConsignor, MatchConsignors));

			return fFilterBusinessObjectDefaults;
		}

		protected virtual ZBool MatchConsignees => ZBool.True;

		protected virtual ZBool MatchConsignors => ZBool.True;

		public override Type FilterBusinessObjectType => typeof(ZWebModules.OrganisationFilterBusinessObject);
	}
}
