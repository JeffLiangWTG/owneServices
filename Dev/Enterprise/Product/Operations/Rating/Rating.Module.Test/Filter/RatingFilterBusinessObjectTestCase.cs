using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(RatingFilterBusinessObject))]
	public abstract class RatingFilterBusinessObjectTestCase<T> : FilterStripBusinessObjectTestCase where T : RatingFilterBusinessObject, new()
	{
		#region Dates

		public void TestDateFilter()
		{
			var rate1 = NewRatingHeader(Helper.NewOrgHeader());
			var entry1 = AddRateEntry(rate1, "AIR", "LSE", "AUSYD", "USLAX");
			entry1.TI_RateStartDate = new ZDate(2009, 5, 2);
			entry1.TI_RateEndDate = new ZDate(2009, 6, 1);

			var rate2 = NewRatingHeader(Helper.NewOrgHeader());
			var entry2 = AddRateEntry(rate2, "AIR", "LSE", "AUSYD", "USLAX");
			entry2.TI_RateStartDate = new ZDate(2009, 5, 18);
			entry2.TI_RateEndDate = new ZDate(2009, 8, 1);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();

			AssertLastUpdatedFilter(filterBizo);

			var startDateFilter = (ModuleDateFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.StartDate];
			if (startDateFilter != null)
			{
				var endDateFilter = (ModuleDateFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.EndDate];
				startDateFilter.IsActive = true;
				endDateFilter.IsActive = true;
				var rates = GetRatingHeaderCollection();

				startDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				startDateFilter.Property1 = new ZDateTime(2009, 5, 1);
				startDateFilter.Property2 = new ZDateTime(2009, 5, 20);
				rates.Load(filterBizo.Filter);
				AssertCollection(rates, rate1, rate2);

				startDateFilter.Property1 = new ZDateTime(2009, 5, 16);
				startDateFilter.Property2 = ZDateTime.Empty;
				rates.Load(filterBizo.Filter);
				AssertCollection(rates, rate2);

				startDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
				rates.Load(filterBizo.Filter);
				AssertCollection(rates, rate1, rate2);
			}
			else
			{
				Assert("Module does not have start date filter - test is not necessary", true);
			}
		}

		protected virtual void AssertLastUpdatedFilter(FilterStripBusinessObject filterBizo)
		{
			CombineAssertions("LastUpdated filter should be superceded by LastEdit filter", () =>
			{
				AssertNull("'Last Updated' filter should NOT exist", filterBizo[RateFilterHelper.Constants.LastUpdated]);
				AssertNull("'Last Edit' filter should exist", filterBizo["Last Edit"]);
			});
		}

		#endregion

		#region Organisation filters

		public void TestCarrierFilter()
		{
			var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			carrier.OH_IsAirLine = true;

			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, carrier.PK));

			var rate1 = NewRatingHeader(Helper.NewOrgHeader());
			var rate1Entry = AddRateEntry(rate1, "AIR", "LSE", "AUSYD", "USLAX", "", "");
			rate1Entry.TI_ContractNumber = "CON1";
			rate1Entry.TI_OH_TransportProvider = carrier.PK;

			var rate2 = NewRatingHeader(Helper.NewOrgHeader());
			var rate2Entry = AddRateEntry(rate2, "ORG", "LSE", "AUSYD", "", "", "");
			rate2Entry.TI_RH_NKCommodityCode = "PAIN";
			rate2Entry.TI_ContractNumber = "CON2";
			rate2Entry.TI_OH_Consignee = consignee.PK;

			var rate3 = NewRatingHeader(Helper.NewOrgHeader());
			var rate3Entry = AddRateEntry(rate3, "DST", "ULD", "", "USLAX", "", "");
			rate3Entry.TI_RH_NKCommodityCode = "BANA";

			Factory.Save();

			ZQuery scope = new ZQuery(RatingHeaderSchema.PK, new ZGuid[] { rate1.PK, rate2.PK, rate3.PK });

			var filterBizo = GetNewFilterStripBusinessObject();
			ModuleGuidFilter carrierFilter = (ModuleGuidFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider];
			var rates = GetRatingHeaderCollection();

			rates.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Empty Filter",
				new RatingHeader[] { rate1, rate2, rate3 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));

			carrierFilter.IsActive = true;
			carrierFilter.Property = carrier.PK;

			AssertContainsExactElementsInAnyOrder("Filter on Carrier",
				new RatingHeader[] { rate1 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));
		}

		public virtual void TestSalesRepFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgStaffAssignments assignment1 = org.StaffAssignments.AddNew();
			assignment1.O8_GS_NKPersonResponsible = "TS1";
			assignment1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			assignment1.O8_Department = "ALL";
			assignment1.O8_GC = GlbCompany.CurrentCompany.PK;
			OrgStaffAssignments assignment2 = org.StaffAssignments.AddNew();
			assignment2.O8_GS_NKPersonResponsible = "TS2";
			assignment2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			assignment2.O8_Department = "ALL";
			assignment2.O8_GC = ZGuid.Empty;

			Factory.Save();

			var rate1 = NewRatingHeader(org);
			var rate2 = NewRatingHeader(Helper.NewOrgHeader());
			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			ModuleNkFilter salesRepFilter = (ModuleNkFilter)filterBizO[RateFilterHelper.Constants.StaffFilterSalesRep];
			var collection = GetRatingHeaderCollection();

			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);

			salesRepFilter.IsActive = true;
			salesRepFilter.Property = "TS1";
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(rate1, collection);

			salesRepFilter.Property = "TS2";
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(rate1, collection);
		}

		public void TestControllingCustomerFilter()
		{
			var controllingCustomer = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var rate1 = NewRatingHeader(Helper.NewOrgHeader());
			var rate1Entry = AddRateEntry(rate1, "AIR", "LSE", "AUSYD", "USLAX", "", "");
			rate1Entry.TI_OH_ControllingCustomer = controllingCustomer.PK;

			var rate2 = NewRatingHeader(Helper.NewOrgHeader());
			var rate2Entry = AddRateEntry(rate2, "ORG", "LSE", "AUSYD", "", "", "");
			rate2Entry.TI_RH_NKCommodityCode = "PAIN";

			Factory.Save();

			var scope = new ZQuery(RatingHeaderSchema.PK, new ZGuid[] { rate1.PK, rate2.PK });

			var filterBizo = GetNewFilterStripBusinessObject();
			var controllingCustomerFilter = (ModuleGuidFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.ControllingCustomer];
			var rates = GetRatingHeaderCollection();

			rates.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Empty Filter",
				new RatingHeader[] { rate1, rate2 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));

			controllingCustomerFilter.IsActive = true;
			controllingCustomerFilter.Property = controllingCustomer.PK;

			AssertContainsExactElementsInAnyOrder("Filter on ControllingCustomer",
				new RatingHeader[] { rate1 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));
		}

		#endregion

		#region Transport Modes Filter

		public void TestTransportModesFilter()
		{
			var airLSERate = CreateRatingHeader("ClientAirLse", Category.AIR, Mode.LSE, "AUSYD", "USLAX");
			var airULDRate = CreateRatingHeader("ClientAirUld", Category.AIR, Mode.ULD, "AUSYD", "USLAX");
			var orgLSERate = CreateRatingHeader("ClientOrgLse", Category.ORG, Mode.LSE, "AUSYD", "");
			var dstULDRate = CreateRatingHeader("ClientDstUld", Category.DST, Mode.ULD, "", "USLAX");
			var orgAIRRate = CreateRatingHeader("ClientOrgAir", Category.ORG, Mode.AIR, "AUSYD", "");
			var dstALLRate = CreateRatingHeader("ClientDstAll", Category.DST, Mode.ALL, "", "USLAX");
			var cstSEARate = CreateRatingHeader("ClientCstSea", Category.CST, Mode.SEA, "", "");

			// Customs
			var caiLSERate = CreateRatingHeader("ClientCaiLse", Category.CAI, Mode.LSE, "AUSYD", "USLAX");
			var caiULDRate = CreateRatingHeader("ClientCaiUld", Category.CAI, Mode.ULD, "AUSYD", "USLAX");
			var corLSERate = CreateRatingHeader("ClientCorLse", Category.COR, Mode.LSE, "AUSYD", "");
			var cdsULDRate = CreateRatingHeader("ClientCdsUld", Category.CDS, Mode.ULD, "", "USLAX");
			var corAIRRate = CreateRatingHeader("ClientCorAir", Category.COR, Mode.AIR, "AUSYD", "");
			var cdsALLRate = CreateRatingHeader("ClientCdsAll", Category.CDS, Mode.ALL, "", "USLAX");

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var rates = GetRatingHeaderCollection();

			AssertLoad(filterBizo, transportModeFilter: string.Empty, rates, expectedRatingHeaders: new[] { airLSERate, airULDRate, orgLSERate, dstULDRate, orgAIRRate, dstALLRate, cstSEARate, caiLSERate, caiULDRate, corLSERate, cdsULDRate, corAIRRate, cdsALLRate });

			((ModuleTextFilter)filterBizo["Transport Mode"]).IsActive = true;
			AssertLoad(filterBizo, transportModeFilter: Mode.LSE, rates, expectedRatingHeaders: new[] { airLSERate, orgLSERate, caiLSERate, corLSERate });
			AssertLoad(filterBizo, transportModeFilter: Mode.ULD, rates, expectedRatingHeaders: new[] { airULDRate, dstULDRate, caiULDRate, cdsULDRate });
			AssertLoad(filterBizo, transportModeFilter: Mode.AIR, rates, expectedRatingHeaders: new[] { airLSERate, airULDRate, orgLSERate, dstULDRate, orgAIRRate, caiLSERate, caiULDRate, corLSERate, cdsULDRate, corAIRRate });
			AssertLoad(filterBizo, transportModeFilter: Mode.SEA, rates, expectedRatingHeaders: new[] { cstSEARate });

			void AssertLoad(FilterStripBusinessObject filterStripBizO, string transportModeFilter, RatingHeaderCollection ratingHeaders, IEnumerable<IRatingHeader> expectedRatingHeaders, string message = default)
			{
				if (!string.IsNullOrEmpty(transportModeFilter))
				{
					((ModuleTextFilter)filterStripBizO["Transport Mode"]).Property = transportModeFilter;
				}
				ratingHeaders.Load(filterStripBizO.Filter);
				TestHelper.AssertRatingHeaders(expectedRatingHeaders, ratingHeaders.AsEnumerable().Cast<IRatingHeader>(), $"{message} Transport Mode Filter: {transportModeFilter}");
			}
		}

		RatingHeader CreateRatingHeader(string clientCode, string category, string mode, string origin, string destination)
		{
			var ratingHeader = NewRatingHeader(Helper.NewOrgHeader(clientCode));
			AddRateEntry(ratingHeader, category, mode, origin, destination);
			return ratingHeader;
		}

		void AssertCollection(RatingHeaderCollection collection, params RatingHeader[] ratingHeaders)
		{
			AssertEquals(ratingHeaders.Length, collection.Count);

			foreach (RatingHeader header in ratingHeaders)
			{
				Assert(collection.Contains(header.PK));
			}
		}

		#endregion

		#region Origin/Destination Filter

		public void TestOriginDestinationFilter()
		{
			var airLSERate = CreateRatingHeader("ClientAirLse", Category.AIR, Mode.LSE, "AUSYD", "USLAX");
			var orgLSERate = CreateRatingHeader("ClientOrgLse", Category.ORG, Mode.LSE, "AUSYD", "");
			var dstULDRate = CreateRatingHeader("ClientDstUld", Category.DST, Mode.ULD, "", "USLAX");

			var sorALLRate = CreateRatingHeader("ClientSorAll", Category.SOR, Mode.ALL, "AUSYD", "USLAX");
			var sdeALLRate = CreateRatingHeader("ClientSdeAll", Category.SDE, Mode.ALL, "AUSYD", "USLAX");
			var scoSEARate = CreateRatingHeader("ClientScoSea", Category.SCO, Mode.SEA, "AUSYD", "USLAX");
			var sncLCLRate = CreateRatingHeader("ClientSncLcl", Category.SNC, Mode.LCL, "AUSYD", "USLAX");
			var sedSEARate = CreateRatingHeader("ClientSedSea", Category.SED, Mode.SEA, "AUSYD", "USLAX");
			var sidSEARate = CreateRatingHeader("ClientSidSea", Category.SID, Mode.SEA, "AUSYD", "USORD");

			var caiLSERate = CreateRatingHeader("ClientCaiLse", Category.CAI, Mode.LSE, "AUSYD", "USLAX");
			var corLSERate = CreateRatingHeader("ClientCorLse", Category.COR, Mode.LSE, "AUSYD", "");
			var cdsULDRate = CreateRatingHeader("ClientCdsUld", Category.CDS, Mode.ULD, "", "USLAX");

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var rates = GetRatingHeaderCollection();

			rates.Load(filterBizo.Filter);
			TestHelper.AssertRatingHeaders
			(
				new[] { airLSERate, orgLSERate, dstULDRate, sorALLRate, sdeALLRate, scoSEARate, sncLCLRate, sedSEARate, sidSEARate, caiLSERate, corLSERate, cdsULDRate },
				rates.AsEnumerable().Cast<IRatingHeader>(),
				"No filter"
			);

			((ModuleLocationFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination]).IsActive = true;
			((ModuleLocationFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination]).Property1 = "AUSYD";
			rates.Load(filterBizo.Filter);
			TestHelper.AssertRatingHeaders
			(
				new[] { airLSERate, orgLSERate, sorALLRate, sdeALLRate, scoSEARate, sncLCLRate, sedSEARate, sidSEARate, caiLSERate, corLSERate },
				rates.AsEnumerable().Cast<IRatingHeader>(),
				"Origin Filter: USLAX"
			);

			((ModuleLocationFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination]).Property2 = "USLAX";
			rates.Load(filterBizo.Filter);
			TestHelper.AssertRatingHeaders
			(
				new[] { airLSERate, sorALLRate, sdeALLRate, scoSEARate, sncLCLRate, sedSEARate, caiLSERate },
				rates.AsEnumerable().Cast<IRatingHeader>(),
				"Destination Filter: USLAX"
			);
		}

		#endregion

		#region Organisation Caption

		public virtual void TestRatingHeaderOrganisationCaption()
		{
			var filterBizo = GetNewFilterStripBusinessObject();
			AssertNotNull(filterBizo[RateFilterHelper.Constants.Client]);
			AssertNull(filterBizo[RateFilterHelper.Constants.ServiceProvider]);
		}

		#endregion

		#region Multiple Sub Query Filter

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestMultipleSubQueryFilter()
		{
			var rate1 = NewRatingHeader(Helper.NewOrgHeader());
			AddRateEntry(rate1, "AIR", "ULD", "AUSYD", "USLAX", "", "");

			var rate2 = NewRatingHeader(Helper.NewOrgHeader());
			var rate2Entry = AddRateEntry(rate2, "ORG", "LSE", "AUSYD", "", "", "");
			rate2Entry.TI_RH_NKCommodityCode = "PAIN";

			var rate3 = NewRatingHeader(Helper.NewOrgHeader());
			var rate3Entry = AddRateEntry(rate3, "DST", "ULD", "", "USLAX", "", "");
			rate3Entry.TI_RH_NKCommodityCode = "BANA";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var rates = GetRatingHeaderCollection();

			rates.Load(filterBizo.Filter);
			AssertCollection(rates, rate1, rate2, rate3);

			((ModuleLocationFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination]).IsActive = true;
			((ModuleLocationFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.OriginDestination]).Property1 = "AUSYD";
			((ModuleNkFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.CommodityCode]).IsActive = true;
			((ModuleNkFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.CommodityCode]).Property = "BANA";
			rates.Load(filterBizo.Filter);
			Assert("No rates should show for AUSYD & BANA", rates.Count == 0);

			((ModuleNkFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.CommodityCode]).Property = "GEN";
			rates.Load(filterBizo.Filter);
			AssertCollection(rates, rate1);

			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.TransportMode]).Property = "LSE";
			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.TransportMode]).IsActive = true;
			rates.Load(filterBizo.Filter);
			Assert("No rates for AUSYD, GEN and CON2", rates.Count == 0);

			((ModuleNkFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.CommodityCode]).Property = "PAIN";
			rates.Load(filterBizo.Filter);
			AssertCollection(rates, rate2);
		}

		#endregion

		#region Organization Name

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestOrganizationNameFilter()
		{
			RatingHeader GetRatingHeaderForOrganization(string organizationName)
			{
				var org = Helper.NewOrgHeader();
				org.OH_FullName = organizationName;

				return NewRatingHeader(org);
			}

			var blueRate = GetRatingHeaderForOrganization("BlueOrgShouldNotBeInDB");
			var greenRate = GetRatingHeaderForOrganization("GreenOrgShouldNotBeInDB");
			var purpleRate = GetRatingHeaderForOrganization("PurpleOrgShouldNotBeInDB");
			var blobRate = GetRatingHeaderForOrganization("BlobOrgShouldNotBeInDB");

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var rates = GetRatingHeaderCollection();

			rates.Load(filterBizo.Filter);
			AssertCollection(rates, blueRate, greenRate, purpleRate, blobRate);

			((ModuleTextFilter)filterBizo[RateFilterHelper.Constants.OrganizationName]).IsActive = true;
			((ModuleTextFilter)filterBizo[RateFilterHelper.Constants.OrganizationName]).Property = "NothingWithThisNameInDB";
			rates.Load(filterBizo.Filter);
			Assert("No rates should be shown for NothingWithThisNameInDB", rates.Count == 0);

			((ModuleTextFilter)filterBizo[RateFilterHelper.Constants.OrganizationName]).Property = "BL";
			((ModuleTextFilter)filterBizo[RateFilterHelper.Constants.OrganizationName]).ComparisonOperator = "starts with";

			rates.Load(filterBizo.Filter);
			AssertCollection(rates, blueRate, blobRate);

			((ModuleTextFilter)filterBizo[RateFilterHelper.Constants.OrganizationName]).Property = "ShouldNotBeInDB";
			((ModuleTextFilter)filterBizo[RateFilterHelper.Constants.OrganizationName]).ComparisonOperator = "contains";

			rates.Load(filterBizo.Filter);
			AssertCollection(rates, blueRate, greenRate, purpleRate, blobRate);

			((ModuleTextFilter)filterBizo[RateFilterHelper.Constants.OrganizationName]).Property = "BLUE";
			((ModuleTextFilter)filterBizo[RateFilterHelper.Constants.OrganizationName]).ComparisonOperator = "not starting";

			rates.Load(filterBizo.Filter);
			AssertCollection(rates, greenRate, purpleRate, blobRate);
		}

		#endregion

		#region Global Rate Type Filter

		public virtual void TestGlobalRateTypeFilter()
		{
			var ratingHeaders = GetGlobalAndLocalRatingHeaders();
			if (ratingHeaders != null)
			{
				Factory.Save();

				var filterBizo = GetNewFilterStripBusinessObject();
				var rates = GetRatingHeaderCollection();
				rates.Load(filterBizo.Filter);
				AssertCollection(rates, ratingHeaders.ToArray());

				((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).IsActive = true;
				((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property0 = true;
				((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property1 = false;
				rates.Load(filterBizo.Filter);

				Assert("Should be only one global rate", rates.Count == 1 && rates[0].Company == null);

				((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property0 = true;
				((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property1 = true;
				rates.Load(filterBizo.Filter);

				Assert("Should be both global and local rate", rates.Count == (IsGlobalOnly ? 1 : 2));

				((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property0 = false;
				((ModuleFlagsFilter)filterBizo[RateFilterHelper.Constants.GlobalRateFilter]).Property1 = true;

				rates.Load(filterBizo.Filter);
				if (IsGlobalOnly)
				{
					Assert("Should be only one local rate", rates.Count == 0);
				}
				else
				{
					Assert("Should be only one local rate", rates.Count == 1 && rates[0].Company != null);
				}
			}
			else
			{
				Assert("Test not required", true);
			}
		}

		protected virtual bool IsGlobalOnly { get; }

		protected abstract List<RatingHeader> GetGlobalAndLocalRatingHeaders();

		protected abstract RatingHeaderCollection GetRatingHeaderCollection();

		protected abstract RatingHeader NewRatingHeader(OrgHeader client);

		protected virtual RateEntry AddRateEntry(RatingHeader rate, ZString category, ZString mode, ZString origin, ZString destination, ZString serviceLevel, ZString container) =>
			rate.AddRateEntry(category, mode, origin, destination, serviceLevel, container);

		protected virtual RateEntry AddRateEntry(RatingHeader rate, ZString category, ZString mode, ZString origin, ZString destination) =>
			rate.AddRateEntry(category, mode, origin, destination);

		#endregion

		#region Test Notifications for Locations and Containers

		public void TestAddNotificationForRatingLocations()
		{
			var zoneA = Factory.NewWithValidTestData<RefZoneHeader>();
			var zoneB = Factory.NewWithValidTestData<RefZoneHeader>();
			var zoneC = Factory.NewWithValidTestData<RefZoneHeader>();
			var country = Factory.NewWithValidTestData<RefCountry>();

			zoneA.FZ_Code = "AB";
			zoneA.FZ_Description = "Zone A Desc";
			zoneA.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			zoneA.FZ_IsActive = true;

			zoneB.FZ_Code = "XY";
			zoneB.FZ_Description = "Zone B Desc";
			zoneB.FZ_IsActive = true;
			zoneB.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.RatingExport;

			zoneC.FZ_Code = "ZY";
			zoneC.FZ_Description = "Zone C Desc";
			zoneC.FZ_IsActive = true;
			zoneC.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.TransitWarehouse;

			country.RN_Code = "AB";
			country.RN_Desc = "Desc";
			country.RN_IsActive = false;

			Factory.Save();
			RatingLocationCollection locations = new RatingLocationCollection(Factory);

			var filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Location Type"]).Property = "Int. Zone";
			((ModuleTextFilter)filter["Location Type"]).IsActive = true;
			locations.LoadZone(filter.Filter);

			AssertCollectionContains(zoneA, locations);
			AssertCollectionContains(zoneB, locations);
			AssertCollectionContains(zoneC, locations);
			AssertCollectionNotContains(country, locations);
			AssertEquals(false, zoneA.HasRowNotifications);

			var notification = locations.GetExtraNotification(zoneC);
			AssertContains("This Zone is not available for Rating purposes. The Zone type is Transit Warehouse.", notification.Message);

			filter = new LocationFilterBusinessObject();

			((ModuleTextFilter)filter["Location Type"]).Property = "Country";
			((ModuleTextFilter)filter["Location Type"]).IsActive = true;

			((ModuleTextFilter)filter["Active Status"]).Property = "Inactive";
			((ModuleTextFilter)filter["Active Status"]).IsActive = true;

			locations.LoadCountry(filter.Filter);

			AssertCollectionContains(country, locations);
			AssertCollectionNotContains(zoneA, locations);

			notification = locations.GetExtraNotification(country);
			AssertEquals("This Country/Region is inactive - it may not be used.", notification.Message);
		}

		public void TestAddNotificationForContainersBasedOnTransportMode()
		{
			var trasportMode1 = Factory.NewWithValidTestData<RefContainer>();
			trasportMode1.RC_ShippingMode = "AIR";
			trasportMode1.RC_IsActive = true;

			var trasportMode2 = Factory.NewWithValidTestData<RefContainer>();
			trasportMode2.RC_ShippingMode = "SEA";
			trasportMode2.RC_IsActive = true;

			var trasportMode3 = Factory.NewWithValidTestData<RefContainer>();
			trasportMode3.RC_ShippingMode = "SEA";
			trasportMode3.RC_IsActive = false;

			var trasportMode4 = Factory.NewWithValidTestData<RefContainer>();
			trasportMode4.RC_ShippingMode = "ROA";
			trasportMode4.RC_IsActive = true;

			Factory.Save();

			var containers = new RatingRefContainerCollection(Factory, "SEA");
			AssertCollectionContains(trasportMode2, containers);

			var filter = new RefContainerFilterBusinessObject();
			((ModuleTextFilter)filter["Transport Mode"]).Property = "Air";
			((ModuleTextFilter)filter["Transport Mode"]).IsActive = true;
			containers.AdditionalFilter = filter.Filter;

			AssertCollectionContains(trasportMode1, containers);
			AssertCollectionNotContains(trasportMode2, containers);

			var notification = containers.GetExtraNotification(trasportMode1);
			AssertContains("This is a Sea Freight entry - please choose a Sea Freight FCL Container.", notification.Message);

			containers = new RatingRefContainerCollection(Factory, "ROA");
			AssertCollectionContains(trasportMode4, containers);

			filter = new RefContainerFilterBusinessObject();
			((ModuleTextFilter)filter["Transport Mode"]).Property = "ROA";
			((ModuleTextFilter)filter["Transport Mode"]).IsActive = true;
			containers.AdditionalFilter = filter.Filter;

			AssertCollectionContains(trasportMode4, containers);
			AssertCollectionNotContains(trasportMode2, containers);
			AssertCollectionNotContains(trasportMode1, containers);

			filter = new RefContainerFilterBusinessObject();
			((ModuleTextFilter)filter["Transport Mode"]).Property = "SEA";
			((ModuleTextFilter)filter["Transport Mode"]).IsActive = true;
			containers.AdditionalFilter = filter.Filter;

			AssertCollectionContains(trasportMode2, containers);
			AssertCollectionNotContains(trasportMode4, containers);

			notification = containers.GetExtraNotification(trasportMode2);
			AssertNull(notification);

			notification = containers.GetExtraNotification(trasportMode4);
			AssertNull(notification);

			notification = containers.GetExtraNotification(trasportMode1);
			AssertContains("This is a Road Freight entry - please choose a Road or Sea Freight Container.", notification.Message);

			containers = new RatingRefContainerCollection(Factory, "AIR");
			AssertCollectionContains(trasportMode1, containers);

			using (var refContainerModule = new RefContainerModuleForTest())
			{
				var filterBusinessObject = (RefContainerFilterBusinessObject)refContainerModule.FilterBusinessObject;
				var statusFilter = (ModuleTextFilter)filterBusinessObject["Active Status"];
				statusFilter.Property = "All";
				statusFilter.IsActive = true;

				var transportMode = (ModuleTextFilter)filterBusinessObject["Transport Mode"];
				transportMode.Property = "Sea";
				transportMode.IsActive = true;

				containers.AdditionalFilter = filterBusinessObject.Filter;
				AssertCollectionContains(trasportMode3, containers);
				AssertCollectionContains(trasportMode2, containers);
				AssertCollectionNotContains(trasportMode1, containers);

				notification = containers.GetExtraNotification(trasportMode2);
				AssertContains("This is an Air Freight entry - please choose an Air Freight ULD Container.", notification.Message);

				notification = containers.GetExtraNotification(trasportMode3);
				AssertContains("This container is not active - it may not be used.", notification.Message);
				AssertContains("This is an Air Freight entry - please choose an Air Freight ULD Container", notification.Message);
			}
		}

		#endregion

		#region Filters

		public void TestFilters()
		{
			var expectedFilterDescriptions = GetExpectedFilterDescriptions();
			var actualFilterDescriptions = GetNewFilterStripBusinessObject()
				.ModuleFilters
				.ToSortedArrayWithIsExclusiveLast()
				.Select(f => f.Description.ToString())
				.ToArray();

			AssertContainsExactElementsInAnyOrder(expectedFilterDescriptions, actualFilterDescriptions);
		}

		protected abstract string[] GetExpectedFilterDescriptions();

		#endregion

		#region Implementation

		protected sealed override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new T();
		}

		protected TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;

		#endregion
	}
}
