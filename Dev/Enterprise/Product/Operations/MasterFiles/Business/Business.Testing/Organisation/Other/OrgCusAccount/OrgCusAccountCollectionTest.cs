using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCusAccountCollection))]
	sealed class OrgCusAccountCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNewSetsCorrectOrgHeaderAndCountryCode()
		{
			var collectionEritrea = new OrgCusAccountCollection(Factory.New<OrgHeader>(), Core.Constants.CountryCodes.Eritrea);
			var orgCusAccount = collectionEritrea.AddNew();
			AssertEquals("OrgCusAccountCollection.Count", 1, collectionEritrea.Count);
			AssertEquals("Correct OrgHeader", collectionEritrea.Master.PK, orgCusAccount.CZ_OH);
			AssertEquals("Correct CountryCode", Core.Constants.CountryCodes.Eritrea, orgCusAccount.Country.Code);
		}

		public void TestGetOrgCusAccountForCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var cd1 = Factory.New<OrgCusAccount>();
			cd1.CZ_Code = "CD1";
			cd1.CZ_Account = "1234567890";
			cd1.CZ_OH = orgHeader.PK;
			cd1.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var cd2 = Factory.New<OrgCusAccount>();
			cd2.CZ_Code = "CD2";
			cd2.CZ_Account = "1234567891";
			cd2.CZ_OH = orgHeader.PK;
			cd2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var cdUS = Factory.New<OrgCusAccount>();
			cdUS.CZ_Code = "CD1";
			cdUS.CZ_Account = "1234567890";
			cdUS.CZ_OH = orgHeader.PK;
			cdUS.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var eritreaCollection = new OrgCusAccountCollection(orgHeader, Core.Constants.CountryCodes.Eritrea);
			eritreaCollection.Load();
			AssertSame("Should return the targeted item.", cd1, eritreaCollection.GetOrgCusAccountForCode("CD1"));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new OrgCusAccountCollection(Factory.New<OrgHeader>(), GlbCompany.CurrentCompany.Country.Code);
	}
}
