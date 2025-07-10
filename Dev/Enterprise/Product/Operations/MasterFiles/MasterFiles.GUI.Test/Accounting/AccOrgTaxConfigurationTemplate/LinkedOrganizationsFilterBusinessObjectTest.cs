using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(LinkedOrganizationsFilterBusinessObject))]
	class LinkedOrganizationsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilter_Code()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Code"];
			filter.Property = "XXXYYY";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org2));

			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));

			filter.Property = "XY";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection3.Contains(org2));
		}

		public void TestFilter_FullName()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_FullName = "XXXYYY";
			org2.OH_FullName = "XXXXXX";
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Name"];
			AssertEquals("Prefix", null, filter.Prefix);
			filter.Property = "XXXYYY";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));

			filter.Property = "XY";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection3.Contains(org2));
		}

		public void TestFilter_MainAddressCountryCodes()
		{
			Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed = true;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "Test1";

			var address1 = org1.Addresses.AddNew();
			address1.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address1.OA_RN_NKCountryCode = "AU";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "Test2";
			org2.OH_IsGlobalAccount = true;

			var address2 = org2.Addresses.AddNew();
			address2.OA_Language = "EN-US";
			address2.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address2.OA_RN_NKCountryCode = "US";

			var address3 = org2.Addresses.AddNew();
			address3.OA_Language = "EN";
			address3.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address3.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address3.OA_RN_NKCountryCode = "AU";
			Factory.Save();

			var filter1 = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.OrgAddress.Country];
			filter1.Property = "AU";
			filter1.IsActive = true;

			var filter2 = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDetails.Code];
			filter2.Property = "Test";
			filter2.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter2.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1, org2 }, orgCollection);

			filter1.Property = "US";
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org2 }, orgCollection);

			filter1.Property = "CN";
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals(0, orgCollection.Count);
		}

		public void TestFilter_MainAddressCity()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_City = "Paris";

			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";
			orgAddress2.OA_City = "Sydney";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["City"];
			filter.Property = "Paris";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "Par";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "s";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestFilter_MainAddressState()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_State = "France";

			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";
			orgAddress2.OA_State = "Australia";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["State"];
			filter.Property = "France";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "Fra";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "ra";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		[StressTest]
		public void TestFilter_MainAddressCategory()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Category = "BUS";
			org2.OH_Category = "GOV";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Category"];
			filter.Property = CategoryList["BUS"].Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = CategoryList["GOV"].Code;
			OrgHeaderCollection orgCollection1 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection1.Load();

			Assert("Expect collection to contain org2", orgCollection1.Contains(org2));
			Assert("Expect collection not to contain org1", !orgCollection1.Contains(org1));
		}

		public void TestFilter_UNLOCO()
		{
			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort];
			Assert("Zones should be allowed if there is no Country search restriction", ((LocationCollection)filter.List).AllowZones);
			Assert("Empty default filter value if there is no Country search restriction", filter.DefaultProperty.IsEmpty);
			AssertEquals("Filter should have default visibility if there is no Country search restriction", FilterVisibility.Visible, filter.Visibility);
			AssertNull("PropertyValidation should be null if search is unrestricted", filter.PropertyValidation);

			Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed = false;

			FilterStripBusinessObject newFilterStripBizO = GetNewFilterStripBusinessObject();
			filter = (ModuleNkFilter)newFilterStripBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort];
			Assert("Zones should be disallowed if there is Country search restriction", !((LocationCollection)filter.List).AllowZones);
			AssertEquals("If there is UNLOCO search restriction the filter should be defaulted to CurrentCompany.CountryCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, filter.DefaultProperty);
			AssertEquals("Filter should be always visible if there is Country search restriction", FilterVisibility.AlwaysVisible, filter.Visibility);
			AssertNotNull("PropertyValidation should not be null if search is restricted", filter.PropertyValidation);
			AssertNoErrors(filter.PropertyInfo);

			filter.Property = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "ADL";
			AssertNoErrors(filter.PropertyInfo);

			filter.Property = "AA";
			var expectedError = $@"Your current security rights only allow you to view organizations based in your current login country/region ({GlbCompany.CurrentCompany.GC_RN_NKCountryCode}).
If you think this is incorrect, please contact your system administrator.";
			AssertHasError(filter.PropertyInfo, expectedError);

			filter.Property = "AABOS";
			AssertHasError(filter.PropertyInfo, expectedError);

			filter.Property = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "MEL";
			AssertNoErrors(filter.PropertyInfo);

			filter.Property = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertNoErrors(filter.PropertyInfo);
		}

		CodeDescriptionPairList CategoryList
		{
			get
			{
				if (categoryList == null)
				{
					categoryList = new CodeDescriptionPairList();
					categoryList.AddRange(new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory));
					return categoryList;
				}
				return categoryList;
			}
		}
		CodeDescriptionPairList categoryList;

		FilterStripBusinessObject FilterStripBizO => fFilterStripBizO ?? (fFilterStripBizO = GetNewFilterStripBusinessObject());
		FilterStripBusinessObject fFilterStripBizO;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new LinkedOrganizationsFilterBusinessObject();
		}
	}
}
