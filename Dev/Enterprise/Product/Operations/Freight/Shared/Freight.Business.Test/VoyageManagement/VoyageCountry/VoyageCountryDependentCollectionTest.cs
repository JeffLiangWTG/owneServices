using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(VoyageCountryDependentCollection))]
	sealed class VoyageCountryDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTestingTheCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(VoyageCountryDependentCollection), GetCollectionToTest().GetType());
		}

		public void TestGetCountry()
		{
			VoyageCountryDependentCollection collection = (VoyageCountryDependentCollection)GetCollectionToTest();

			VoyageCountry aU = collection.GetCountry("AU", false);
			AssertNull("AU not added yet", aU);

			aU = collection.GetCountry("AU", true);
			AssertNotNull("AU added", aU);
			AssertEquals("Should have correct country code", "AU", aU.J0_RN_NKCountry);

			VoyageCountry sG1 = collection.GetCountry("SG", false);
			AssertNull("Dont just grab any country (A)", sG1);

			sG1 = collection.GetCountry("SG", true);
			Assert("Dont just grab any country (B)", sG1.PK != aU.PK);
			AssertEquals("Should have correct country code", "SG", sG1.J0_RN_NKCountry);
			AssertEquals("Should not have changes", false, sG1.HasChanges);

			VoyageCountry sG2 = collection.GetCountry("SG", true);
			AssertEquals("Should not have changes", false, sG2.HasChanges);

			sG2.J0_AllocationMethod = AllocationMethodList.Codes.Country;
			AssertEquals("Should have changes", true, sG2.HasChanges);
			VoyageCountry sG3 = collection.GetCountry("SG", false);
			AssertEquals("HasChanges should not have been reset", true, sG3.HasChanges);

			AssertEquals("should reuse existing objects if there (1-2)", sG1.PK, sG2.PK);
			AssertEquals("Should reuse existing objects if there (2-3)", sG2.PK, sG3.PK);

			VoyageCountry country = collection.GetCountry("FAKE", true);
			AssertNull("if the Country code is invalid then return null even when told to create one (FAKE)", country);

			country = collection.GetCountry("", true);
			AssertNull("if the Country code is invalid then return null even when told to create one ()", country);
		}

		public void TestGetCountryDuplicate()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageCountry auInDatabase = voyage.Countries.GetCountry("AU", true);

			Factory.Save();

			VoyageCountry newAU1 = voyage.Countries.AddNew();
			newAU1.J0_JV = voyage.PK;
			newAU1.J0_RN_NKCountry = "AU";

			VoyageCountry newAU2 = voyage.Countries.AddNew();
			newAU2.J0_JV = voyage.PK;
			newAU2.J0_RN_NKCountry = "AU";

			VoyageCountry selectedAU = voyage.Countries.GetCountry("AU", true);

			AssertEquals("should have selected the country in the database.", auInDatabase, selectedAU);
			AssertEquals("should have purged newAU1", true, newAU1.IsDeleted);
			AssertEquals("should have purged newAU2", true, newAU2.IsDeleted);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			return voyage.Countries;
		}

		#endregion
	}
}
