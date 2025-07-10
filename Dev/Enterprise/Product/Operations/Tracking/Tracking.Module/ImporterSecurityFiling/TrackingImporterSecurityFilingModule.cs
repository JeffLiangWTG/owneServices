using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.Tracking.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class TrackingImporterSecurityFilingModule : ZFilterStripGridModule
	{
		public TrackingImporterSecurityFilingModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID => WebModuleIDs.TrackingImporterSecurityFiling;

		public override Type GridCollectionType => typeof(TrackingCusISFHeaderCollection);

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutISF;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ISFFilterBusinessObject();

		public override bool CacheCollectionPKs => false;

		#region GetCurrentLoggenInUserFilter

		ISFFilterBusinessObject ISFFilterBizO => iSFFilterBizO ?? (iSFFilterBizO = FilterStripBizO as ISFFilterBusinessObject ?? new ISFFilterBusinessObject());

		ISFFilterBusinessObject iSFFilterBizO;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionToFilter", Justification = "Baseline")]
		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			if (Page.SiteUser.IsLoggedIn)
			{
				var result = new ZDBOnlyQuery(typeof(TrackingCusISFHeader));

				var siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
				var relatedOrgPKs = siteUser != null ? siteUser.OrganisationRelatedOrgPKs : ((OrgContactWebUser)Page.SiteUser).OrganisationRelatedOrgPKs;

				foreach (var role in WebDataRegistry.Instance.ISFOrgsRolePropertySuppression.RolesWithAccess())
				{
					switch (role)
					{
						case ISFAccessRules.Roles.ISFBuyingParty:
							result.AddToFilter(GetBuyingPartyQuery(relatedOrgPKs), JoinCondition.Or);
							break;

						case ISFAccessRules.Roles.ISFConsolidator:
							result.AddToFilter(GetConsolidatorQuery(relatedOrgPKs), JoinCondition.Or);
							break;

						case ISFAccessRules.Roles.ISFImporter:
							result.AddToFilter(GetImporterQuery(relatedOrgPKs), JoinCondition.Or);
							break;

						case ISFAccessRules.Roles.ISFManufacturer:
							result.AddToFilter(GetManufacturerQuery(relatedOrgPKs), JoinCondition.Or);
							break;

						case ISFAccessRules.Roles.ISFSellingParty:
							result.AddToFilter(GetSellingPartyQuery(relatedOrgPKs), JoinCondition.Or);
							break;

						case ISFAccessRules.Roles.ISFShipToLocation:
							result.AddToFilter(GetShipToPartyQuery(relatedOrgPKs), JoinCondition.Or);
							break;

						case ISFAccessRules.Roles.ISFStuffingLocation:
							result.AddToFilter(GetStuffingLocationQuery(relatedOrgPKs), JoinCondition.Or);
							break;

						case ISFAccessRules.Roles.SendingAgent:
							result.AddToFilter(GetSendingAgentQuery(relatedOrgPKs), JoinCondition.Or);
							break;

						case ISFAccessRules.Roles.ISFBookingParty:
							result.AddToFilter(GetBookingPartyQuery(relatedOrgPKs), JoinCondition.Or);
							break;

						default:
							throw new NotImplementedException("Registry Access roles don't correspond TrackingImporterSecurityFilingModule's: " + role);
					}
				}

				return result;
			}

			return ZQuery.NoResultQuery;
		}

		#region SendingAgentQuery

		public ZDBOnlyQuery GetSendingAgentQuery(ZGuid[] relatedOrgPK)
		{
			var oSBLTM = new ZDBOnlySubQuery(typeof(OrgSupBuyLinkTrnMode), OrgSupBuyLinkTrnModeSchema.PF_OL);
			oSBLTM.AddToFilter(OrgSupBuyLinkTrnModeSchema.PF_OH_SendingAgent, relatedOrgPK);

			var buyOSBL = new ZDBOnlySubQuery(typeof(OrgSupplierBuyerLink), OrgSupplierBuyerLinkSchema.OL_OH_Buyer);
			buyOSBL.AddSubQuery(oSBLTM, JoinCondition.And);
			var supOSBL = new ZDBOnlySubQuery(typeof(OrgSupplierBuyerLink), OrgSupplierBuyerLinkSchema.OL_OH_Supplier);
			supOSBL.AddSubQuery(oSBLTM, JoinCondition.And);

			var buyOA = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			buyOA.AddSubQuery(OrgAddressSchema.OA_OH, buyOSBL, JoinCondition.And);
			var supOA = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			supOA.AddSubQuery(OrgAddressSchema.OA_OH, supOSBL, JoinCondition.And);

			var buyJDA = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			buyJDA.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusISFHeaderSchema.Constants.Prefix);
			buyJDA.AddSubQuery(JobDocAddressSchema.E2_OA_Address, buyOA, JoinCondition.And);
			ISFFilterBizO.AddBuyers(buyJDA);
			var supJDA = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			supJDA.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusISFHeaderSchema.Constants.Prefix);
			supJDA.AddSubQuery(JobDocAddressSchema.E2_OA_Address, supOA, JoinCondition.And);
			ISFFilterBizO.AddSuppliers(supJDA);

			var allBuyers = new ZDBOnlySubQuery(typeof(CusISFHeader), CusISFHeaderSchema.PK);
			allBuyers.AddSubQuery(buyJDA, JoinCondition.Or);
			allBuyers.AddSubQuery(CusISFHeaderSchema.BF_OH_Importer, buyOSBL, JoinCondition.Or);

			var result = new ZDBOnlyQuery(typeof(CusISFHeader));
			result.AddSubQuery(allBuyers, JoinCondition.And);
			result.AddSubQuery(supJDA, JoinCondition.And);

			return result;
		}

		#endregion

		ZQuery GetSellingPartyQuery(ZGuid[] relatedOrgPKs) => GetOrganisationQuery(DocAddressType.SellingParty, relatedOrgPKs);

		ZQuery GetBuyingPartyQuery(ZGuid[] relatedOrgPKs) => GetOrganisationQuery(DocAddressType.BuyingParty, relatedOrgPKs);

		ZQuery GetShipToPartyQuery(ZGuid[] relatedOrgPKs) => GetOrganisationQuery(DocAddressType.ShipToParty, relatedOrgPKs);

		ZQuery GetManufacturerQuery(ZGuid[] relatedOrgPKs) => GetOrganisationQuery(DocAddressType.Manufacturer, relatedOrgPKs);

		ZQuery GetBookingPartyQuery(ZGuid[] relatedOrgPKs) => GetOrganisationQuery(DocAddressType.BookingPartyDocumentaryAddress, relatedOrgPKs);

		ZQuery GetStuffingLocationQuery(ZGuid[] relatedOrgPKs) => GetOrganisationQuery(DocAddressType.ScheduledContainerStuffingLocation, relatedOrgPKs);

		ZQuery GetConsolidatorQuery(ZGuid[] relatedOrgPKs) => GetOrganisationQuery(DocAddressType.Consolidator, relatedOrgPKs);

		ZQuery GetImporterQuery(ZGuid[] relatedOrgPKs) => new ZQuery(CusISFHeaderSchema.BF_OH_Importer, relatedOrgPKs);

		public ZDBOnlyQuery GetOrganisationQuery(DocAddressType addressType, ZGuid[] relatedOrgPK)
		{
			var result = new ZDBOnlyQuery(typeof(CusISFHeader));
			var docAddressSubQuery = GetDocAddressSubQuery(addressType, relatedOrgPK);
			result.AddSubQuery(docAddressSubQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlySubQuery GetDocAddressSubQuery(DocAddressType addressType, ZGuid[] relatedOrgPK)
		{
			var docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);

			orgHeaderSubQuery.AddToFilter(OrgAddressSchema.OA_OH, relatedOrgPK);
			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusISFHeaderSchema.Constants.Prefix);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, addressType));
			docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			return docAddressSubQuery;
		}

		#endregion

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(CusISFHeaderSchema.BF_JobReference.Name, DefaultSortOrder) };

		protected override GridColumnProvider GetColumnProvider() => new ISFModuleColumnProvider();

		protected override SchemaPKColumn RelevantPersistantPKColumn => CusISFHeaderSchema.PK;
	}
}
