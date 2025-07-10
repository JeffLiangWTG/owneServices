using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class OrgAgentModule : OrganisationModule
	{
		public OrgAgentModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		protected override IBusinessObjectCollection LoadCollectionCore(FilterBusinessObject filterBizO, ZQuery query, IBusinessObjectCollection collection, bool ignoreCache)
		{
			query.AddToFilter(GetCurrentLoggedInUserFilter(), JoinCondition.Or);
			return base.LoadCollectionCore(filterBizO, query, collection, ignoreCache);
		}

		protected ZQuery GetCurrentLoggedInUserFilter()
		{
			ZQuery result = new ZQuery();

			if (Page.SiteUser.IsLoggedIn)
			{
				result = new ZDBOnlyQuery(typeof(OrgHeader));
				ZGuid currentOrgPK = ((OrgContactWebUser)Page.SiteUser).LoggedInOrganisation.PK;

				result = FilterByOrg(currentOrgPK);
			}
			return result;
		}

		public ZDBOnlyQuery FilterByOrg(ZGuid orgPK)
		{
			ZDBOnlySubQuery oSBLTMFilter = new ZDBOnlySubQuery(typeof(OrgSupBuyLinkTrnMode), OrgSupBuyLinkTrnModeSchema.PF_OL);
			oSBLTMFilter.AddToFilter(OrgSupBuyLinkTrnModeSchema.PF_OH_SendingAgent, orgPK);

			ZDBOnlySubQuery supOSBLFilter = new ZDBOnlySubQuery(typeof(OrgSupplierBuyerLink), OrgSupplierBuyerLinkSchema.OL_OH_Supplier);
			supOSBLFilter.AddSubQuery(OrgSupplierBuyerLinkSchema.PK, oSBLTMFilter, JoinCondition.And);

			ZDBOnlySubQuery buyOSBLFilter = new ZDBOnlySubQuery(typeof(OrgSupplierBuyerLink), OrgSupplierBuyerLinkSchema.OL_OH_Buyer);
			buyOSBLFilter.AddSubQuery(OrgSupplierBuyerLinkSchema.PK, oSBLTMFilter, JoinCondition.And);

			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(OrgHeader));
			filter.AddSubQuery(OrgHeaderSchema.PK, buyOSBLFilter, JoinCondition.Or);
			filter.AddSubQuery(OrgHeaderSchema.PK, supOSBLFilter, JoinCondition.Or);

			ZDBOnlyQuery result = filter;
			return result;
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.OrgAgentTracking; }
		}
	}
}
