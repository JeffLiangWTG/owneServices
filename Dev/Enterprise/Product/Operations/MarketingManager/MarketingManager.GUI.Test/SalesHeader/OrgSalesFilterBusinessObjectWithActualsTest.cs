using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(OrgSalesFilterBusinessObjectWithActuals))]
	public class OrgSalesFilterBusinessObjectWithActualsTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		[TestDate(2022, 2, 1)]
		public void TestStatus()
		{
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var us = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var gb = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);
			var salesCollection = salesHeader.EntitySalesCollectionProductView;
			var prospectSalesAuAu = salesCollection.AddNew();
			prospectSalesAuAu.OW_OriginID = au.PK;
			prospectSalesAuAu.OW_OriginTableCode = au.TablePrefix;
			prospectSalesAuAu.OW_DestinationID = au.PK;
			prospectSalesAuAu.OW_DestinationTableCode = au.TablePrefix;
			var prospectSalesAuUs = salesCollection.AddNew();
			prospectSalesAuUs.OW_OriginID = au.PK;
			prospectSalesAuUs.OW_OriginTableCode = au.TablePrefix;
			prospectSalesAuUs.OW_DestinationID = us.PK;
			prospectSalesAuUs.OW_DestinationTableCode = us.TablePrefix;
			var prospectSalesUsAu = salesCollection.AddNew();
			prospectSalesUsAu.OW_OriginID = us.PK;
			prospectSalesUsAu.OW_OriginTableCode = us.TablePrefix;
			prospectSalesUsAu.OW_DestinationID = au.PK;
			prospectSalesUsAu.OW_DestinationTableCode = au.TablePrefix;
			var prospectSalesUsUs = salesCollection.AddNew();
			prospectSalesUsUs.OW_OriginID = us.PK;
			prospectSalesUsUs.OW_OriginTableCode = us.TablePrefix;
			prospectSalesUsUs.OW_DestinationID = us.PK;
			prospectSalesUsUs.OW_DestinationTableCode = us.TablePrefix;
			var prospectSalesGbGb = salesCollection.AddNew();
			prospectSalesGbGb.OW_OriginID = gb.PK;
			prospectSalesGbGb.OW_OriginTableCode = gb.TablePrefix;
			prospectSalesGbGb.OW_DestinationID = gb.PK;
			prospectSalesGbGb.OW_DestinationTableCode = gb.TablePrefix;

			var actualSalesNswVic = salesCollection.AddNew();
			actualSalesNswVic.OW_IsTraded = true;
			actualSalesNswVic.OW_OriginID = Factory.LoadTop1<RefCountryStates>(new ZQuery(new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, au.RN_Code), new ZQuery(RefCountryStatesSchema.RW_Code, "NSW"))).PK;
			actualSalesNswVic.OW_OriginTableCode = RefCountryStatesSchema.Constants.Prefix;
			actualSalesNswVic.OW_DestinationID = Factory.LoadTop1<RefCountryStates>(new ZQuery(new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, au.RN_Code), new ZQuery(RefCountryStatesSchema.RW_Code, "VIC"))).PK;
			actualSalesNswVic.OW_DestinationTableCode = RefCountryStatesSchema.Constants.Prefix;
			var detail1 = actualSalesNswVic.TradeDetails.AddNew();
			var period1 = detail1.TradedPeriods.AddNew();
			period1.PAS_Period = new ZDate(2022, 2, 1);
			period1.PAS_LastTraded = new ZDate(2022, 2, 1);
			period1.PAS_OH_Client = org.PK;

			var actualSalesGbGb = salesCollection.AddNew();
			actualSalesGbGb.OW_IsTraded = true;
			actualSalesGbGb.OW_OriginID = gb.PK;
			actualSalesGbGb.OW_OriginTableCode = gb.TablePrefix;
			actualSalesGbGb.OW_DestinationID = gb.PK;
			actualSalesGbGb.OW_DestinationTableCode = gb.TablePrefix;
			var detail2 = actualSalesGbGb.TradeDetails.AddNew();
			var period2 = detail2.TradedPeriods.AddNew();
			period2.PAS_Period = new ZDate(2020, 2, 1);
			period2.PAS_LastTraded = new ZDate(2020, 2, 1);
			period2.PAS_OH_Client = org.PK;

			Factory.Save();

			var actualsInfoPopulater = new OrgSalesActualsInformationBulkPopulater(Factory, org.PK);
			actualsInfoPopulater.Execute(salesCollection.Cast<EntitySalesWrapper>(), false);

			var filterBizObj = new OrgSalesFilterBusinessObjectWithActuals();
			filterBizObj.AllProspectSales = salesCollection.Cast<EntitySalesWrapper>();

			var statusFilter = (ModuleTextFilter)filterBizObj[OrgSalesFilterBusinessObject.FilterDescription.Status];
			AssertEquals("Status", statusFilter.MultilingualDescription);

			statusFilter.IsActive = true;

			statusFilter.Property = OrgSalesActualsStatusList.Codes.Traded;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					prospectSalesAuAu
				},
				Factory.Load<EntitySalesWrapper>(new ZQuery(filterBizObj.Filter, new ZQuery(OrgSalesSchema.OW_IsTraded, ZBool.False))));

			statusFilter.Property = OrgSalesActualsStatusList.Codes.Prospective;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					prospectSalesAuUs,
					prospectSalesUsAu,
					prospectSalesUsUs
				},
				Factory.Load<EntitySalesWrapper>(new ZQuery(filterBizObj.Filter, new ZQuery(OrgSalesSchema.OW_IsTraded, ZBool.False))));

			statusFilter.Property = OrgSalesActualsStatusList.Codes.Lost;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					prospectSalesGbGb
				},
				Factory.Load<EntitySalesWrapper>(new ZQuery(filterBizObj.Filter, new ZQuery(OrgSalesSchema.OW_IsTraded, ZBool.False))));
		}

		[TestDate(2022, 2, 1)]
		public void TestLastTraded()
		{
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var us = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var gb = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);
			var salesCollection = salesHeader.EntitySalesCollectionProductView;
			var prospectSalesAuAu = salesCollection.AddNew();
			prospectSalesAuAu.OW_MP_Product = product.PK;
			prospectSalesAuAu.OW_OriginID = au.PK;
			prospectSalesAuAu.OW_OriginTableCode = au.TablePrefix;
			prospectSalesAuAu.OW_DestinationID = au.PK;
			prospectSalesAuAu.OW_DestinationTableCode = au.TablePrefix;
			var prospectSalesAuUs = salesCollection.AddNew();
			prospectSalesAuUs.OW_MP_Product = product.PK;
			prospectSalesAuUs.OW_OriginID = au.PK;
			prospectSalesAuUs.OW_OriginTableCode = au.TablePrefix;
			prospectSalesAuUs.OW_DestinationID = us.PK;
			prospectSalesAuUs.OW_DestinationTableCode = us.TablePrefix;
			var prospectSalesUsAu = salesCollection.AddNew();
			prospectSalesUsAu.OW_MP_Product = product.PK;
			prospectSalesUsAu.OW_OriginID = us.PK;
			prospectSalesUsAu.OW_OriginTableCode = us.TablePrefix;
			prospectSalesUsAu.OW_DestinationID = au.PK;
			prospectSalesUsAu.OW_DestinationTableCode = au.TablePrefix;
			var prospectSalesUsUs = salesCollection.AddNew();
			prospectSalesUsUs.OW_MP_Product = product.PK;
			prospectSalesUsUs.OW_OriginID = us.PK;
			prospectSalesUsUs.OW_OriginTableCode = us.TablePrefix;
			prospectSalesUsUs.OW_DestinationID = us.PK;
			prospectSalesUsUs.OW_DestinationTableCode = us.TablePrefix;

			var actualSalesNswVic = salesCollection.AddNew();
			actualSalesNswVic.OW_IsTraded = true;
			actualSalesNswVic.OW_MP_Product = product.PK;
			actualSalesNswVic.OW_OriginID = Factory.LoadTop1<RefCountryStates>(new ZQuery(new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, au.RN_Code), new ZQuery(RefCountryStatesSchema.RW_Code, "NSW"))).PK;
			actualSalesNswVic.OW_OriginTableCode = RefCountryStatesSchema.Constants.Prefix;
			actualSalesNswVic.OW_DestinationID = Factory.LoadTop1<RefCountryStates>(new ZQuery(new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, au.RN_Code), new ZQuery(RefCountryStatesSchema.RW_Code, "VIC"))).PK;
			actualSalesNswVic.OW_DestinationTableCode = RefCountryStatesSchema.Constants.Prefix;
			var detail = actualSalesNswVic.TradeDetails.AddNew();
			var period = detail.TradedPeriods.AddNew();
			period.PAS_Period = new ZDate(2022, 2, 1);
			period.PAS_LastTraded = new ZDate(2022, 2, 1);
			period.PAS_OH_Client = org.PK;

			Factory.Save();

			var actualsInfoPopulater = new OrgSalesActualsInformationBulkPopulater(Factory, org.PK);
			actualsInfoPopulater.Execute(salesCollection.Cast<EntitySalesWrapper>(), false);

			var filterBizObj = new OrgSalesFilterBusinessObjectWithActuals();
			filterBizObj.AllProspectSales = salesCollection.Cast<EntitySalesWrapper>();
			var lastTradedFilter = (ModuleDateFilter)filterBizObj[OrgSalesFilterBusinessObject.FilterDescription.ActualsLastTraded];
			AssertEquals("Last Traded", lastTradedFilter.MultilingualDescription);

			lastTradedFilter.IsActive = true;

			lastTradedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			lastTradedFilter.Property1 = new ZDateTime(2022, 2, 1);
			lastTradedFilter.Property2 = new ZDateTime(2022, 2, 3);
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					prospectSalesAuAu
				},
				Factory.Load<EntitySalesWrapper>(new ZQuery(filterBizObj.Filter, new ZQuery(OrgSalesSchema.OW_IsTraded, ZBool.False))));

			lastTradedFilter.Property1 = new ZDateTime(2022, 1, 1);
			lastTradedFilter.Property2 = new ZDateTime(2022, 1, 3);
			AssertContainsExactElementsInAnyOrder(
				System.Array.Empty<OrgSales>(),
				Factory.Load<EntitySalesWrapper>(new ZQuery(filterBizObj.Filter, new ZQuery(OrgSalesSchema.OW_IsTraded, ZBool.False))));

			lastTradedFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					prospectSalesAuAu
				},
				Factory.Load<EntitySalesWrapper>(new ZQuery(filterBizObj.Filter, new ZQuery(OrgSalesSchema.OW_IsTraded, ZBool.False))));

			lastTradedFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					prospectSalesAuUs,
					prospectSalesUsAu,
					prospectSalesUsUs
				},
				Factory.Load<EntitySalesWrapper>(new ZQuery(filterBizObj.Filter, new ZQuery(OrgSalesSchema.OW_IsTraded, ZBool.False))));
		}

		#endregion

		#region Overrides

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgSalesFilterBusinessObjectWithActuals();
		}

		#endregion
	}
}
