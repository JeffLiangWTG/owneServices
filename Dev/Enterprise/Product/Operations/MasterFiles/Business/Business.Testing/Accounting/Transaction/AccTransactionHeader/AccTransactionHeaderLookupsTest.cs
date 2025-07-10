using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTransactionHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions()]
		public void TestHeaderLookup()
		{
			AccTransactionHeader parent = Factory.New<AccTransactionHeader>();
			AccTransactionHeaderLookups lookups = new AccTransactionHeaderLookups(parent);
			lookups.Headers.Load();
		}

		public void TestDepartmentFilter()
		{
			GlbDepartmentCollection allDeptCollection = new GlbDepartmentCollection(Factory);

			allDeptCollection[0].GE_IsActive = false;
			allDeptCollection.AdditionalFilter = (new ZQuery(GlbDepartmentSchema.GE_IsActive, true));
			int activeDeptCount = allDeptCollection.Count;

			AccTransactionHeader parent = Factory.New<AccTransactionHeader>();
			AccTransactionHeaderLookups lookups = new AccTransactionHeaderLookups(parent);
			GlbDepartmentCollection collection = lookups.Departments;

			AssertEquals(activeDeptCount, collection.Count);
		}

		public void TestTaxBranches()
		{
			var parent = Factory.New<AccTransactionHeader>();
			var lookups = new AccTransactionHeaderLookups(parent);
			var collection = lookups.TaxBranches;

			AssertEquals(GlbCompany.CurrentCompany.ActiveBranches.Count(), collection.Count);
		}

		public void TestCompanies()
		{
			GlbCompany company1 = Factory.LoadFromUniqueKey<GlbCompany>(GlbCompanySchema.GC_Code, new ZString("DEM"));
			AssertEquals("PreCondition: Expected that test database contains DEM company", "DEM", company1.GC_Code);

			GlbCompany company2 = Factory.LoadFromUniqueKey<GlbCompany>(GlbCompanySchema.GC_Code, new ZString("EDI"));
			AssertEquals("PreCondition: Expected that test database contains DEM company", "EDI", company2.GC_Code);

			GlbCompany company3 = Factory.LoadFromUniqueKey<GlbCompany>(GlbCompanySchema.GC_Code, new ZString("SIN"));
			AssertEquals("PreCondition: Expected that test database contains DEM company", "SIN", company3.GC_Code);

			AccTransactionHeader parent = Factory.New<AccTransactionHeader>();
			AccTransactionHeaderLookups lookups = new AccTransactionHeaderLookups(parent);
			GlbCompanyCollection collection = lookups.Companies;

			AssertEquals("Expected 3 companies in collection", 3, collection.Count);
			Assert("First company should be in collection", collection.Contains(company1));
			Assert("Second company should be in collection", collection.Contains(company2));
			Assert("Third company should be in collection", collection.Contains(company3));
		}

		public void TestPlacesOfSupplyLookup()
		{
			foreach (var country in new[] { Core.Constants.CountryCodes.India, Core.Constants.CountryCodes.Australia })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var placesOfSupply = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany);

					var parent = Factory.NewWithValidTestData<AccTransactionHeader>();
					var lookups = new AccTransactionHeaderLookups(parent);
					AssertContainsExactElementsInAnyOrder(placesOfSupply, lookups.PlacesOfSupply);
				}
			}
		}

		public void TestPlaceOfSupplyTypesLookup()
		{
			foreach (var country in new[] { Core.Constants.CountryCodes.India, Core.Constants.CountryCodes.Australia })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var placeOfSupplyTypes = PlaceOfSupplyListProvider.GetPlaceOfSupplyTypeList(GlbCompany.CurrentCompany);

					var parent = Factory.NewWithValidTestData<AccTransactionHeader>();
					var lookups = new AccTransactionHeaderLookups(parent);
					AssertContainsExactElementsInAnyOrder(placeOfSupplyTypes, lookups.PlaceOfSupplyTypes);
				}
			}
		}
	}
}
