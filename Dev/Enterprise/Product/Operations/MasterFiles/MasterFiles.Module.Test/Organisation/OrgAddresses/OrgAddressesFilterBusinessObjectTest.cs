using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgAddressesFilterBusinessObject))]
	sealed class OrgAddressesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLayoutContext()
		{
			AssertEquals("OrgAddresses", ((IFilterStripBusinessObjectInternals)GetNewFilterStripBusinessObject()).LayoutContext);
		}

		public void TestAddressType_List()
		{
			var filterBO = new OrgAddressesFilterBusinessObject();
			var list = filterBO.AddressType_List;
			var codes = list.GetAllCodes();
			var types = Enum.GetNames(typeof(AddressType));
			foreach (var type in types)
			{
				AssertCollectionContains(type, codes);
			}
			AssertEquals("AddressType matches ZArchitecture.Business.AddressType", codes.Length, types.Length);
		}

		#region Filters

		public void TestTypeFilter()
		{
			//Query = new ZDBOnlyQuery(typeof(OrgAddress));
			//Query.DefaultJoinCondition = JoinCondition.And;

			//ZDBOnlySubQuery SubQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
			//SubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, Value);

			//Query.AddSubQuery(SubQuery, JoinCondition.And);

			OrgAddress address1 = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress address2 = Factory.NewWithValidTestData<OrgAddress>();

			OrgAddressCapability addressCapability1 = Factory.NewWithValidTestData<OrgAddressCapability>();
			OrgAddressCapability addressCapability2 = Factory.NewWithValidTestData<OrgAddressCapability>();

			addressCapability1.PZ_AddressType = "TP1";
			addressCapability2.PZ_AddressType = "ANT";

			addressCapability1.PZ_OA = address1.PK;
			addressCapability2.PZ_OA = address2.PK;

			Factory.Save();

			OrgAddressesFilterBusinessObject filter = new OrgAddressesFilterBusinessObject();
			((ModuleTextFilter)filter[OrgAddressesFilterBusinessObject.Schema.Type]).Property = "TP1";
			((ModuleTextFilter)filter[OrgAddressesFilterBusinessObject.Schema.Type]).IsActive = true;

			OrgAddressCollection collection = new OrgAddressCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(address1, collection);
			AssertCollectionNotContains(address2, collection);
		}

		public void TestPostCodeFilter()
		{
			OrgAddressesFilterBusinessObject filter = new OrgAddressesFilterBusinessObject();
			var postcodeFilter = (ModuleTextFilter)filter[OrgAddressesFilterBusinessObject.Schema.PostCode];
			AssertEquals(FilterCategories.Locations, postcodeFilter.Category);
			AssertEquals(OrgAddressSchema.OA_PostCode, postcodeFilter.FilterColumn);
		}

		public void TestCountryFilter()
		{
			OrgAddressesFilterBusinessObject filter = new OrgAddressesFilterBusinessObject();
			var countryFilter = (ModuleNkFilter)filter[OrgAddressesFilterBusinessObject.Schema.Country];
			AssertEquals(FilterCategories.Locations, countryFilter.Category);
			AssertEquals(OrgAddressSchema.OA_RN_NKCountryCode, countryFilter.FilterColumn);
		}

		public void TestOrganisationFilter()
		{
			OrgAddressesFilterBusinessObject filter = new OrgAddressesFilterBusinessObject();
			ModuleGuidFilter organisationFilter = (ModuleGuidFilter)filter[OrgAddressesFilterBusinessObject.Schema.Organisation];
			AssertEquals(FilterCategories.Organisations, organisationFilter.Category);
			AssertEquals(ModuleIDs.Organisation, organisationFilter.ModuleId);
			AssertEquals(OrgAddressSchema.OA_OH, organisationFilter.FilterColumn);
		}

		public void TestRelatedPortFilter()
		{
			OrgAddressesFilterBusinessObject filter = new OrgAddressesFilterBusinessObject();
			ModuleNkFilter relatedPortFilter = (ModuleNkFilter)filter[OrgAddressesFilterBusinessObject.Schema.RelatedPort];
			AssertEquals(FilterCategories.Locations, relatedPortFilter.Category);
			AssertEquals(ModuleIDs.RefUNLOCO, relatedPortFilter.ModuleId);
			AssertEquals(OrgAddressSchema.OA_RL_NKRelatedPortCode, relatedPortFilter.FilterColumn);
		}

		public void TestAddress1Filter()
		{
			OrgAddressesFilterBusinessObject filter = new OrgAddressesFilterBusinessObject();
			ModuleTextFilter address1Filter = (ModuleTextFilter)filter[OrgAddressesFilterBusinessObject.Schema.Address1];
			AssertEquals(FilterCategories.Locations, address1Filter.Category);
			AssertEquals(OrgAddressSchema.OA_Address1, address1Filter.FilterColumn);
		}

		public void TestCityFilter()
		{
			OrgAddressesFilterBusinessObject filter = new OrgAddressesFilterBusinessObject();
			ModuleTextFilter cityFilter = (ModuleTextFilter)filter[OrgAddressesFilterBusinessObject.Schema.City];
			AssertEquals(FilterCategories.Locations, cityFilter.Category);
			AssertEquals(OrgAddressSchema.OA_City, cityFilter.FilterColumn);
		}

		public void TestCodeFilter()
		{
			OrgAddressesFilterBusinessObject filter = new OrgAddressesFilterBusinessObject();
			ModuleTextFilter codeFilter = (ModuleTextFilter)filter[OrgAddressesFilterBusinessObject.Schema.Code];
			AssertEquals(FilterCategories.TextSearch, codeFilter.Category);
			AssertEquals(OrgAddressSchema.OA_Code, codeFilter.FilterColumn);
		}

		public void TestStateFilter()
		{
			OrgAddressesFilterBusinessObject filter = new OrgAddressesFilterBusinessObject();
			ModuleTextFilter stateFilter = (ModuleTextFilter)filter[OrgAddressesFilterBusinessObject.Schema.State];
			AssertEquals(FilterCategories.Locations, stateFilter.Category);
			AssertEquals(OrgAddressSchema.OA_State, stateFilter.FilterColumn);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgAddressesFilterBusinessObject();
		}

		#endregion
	}
}
