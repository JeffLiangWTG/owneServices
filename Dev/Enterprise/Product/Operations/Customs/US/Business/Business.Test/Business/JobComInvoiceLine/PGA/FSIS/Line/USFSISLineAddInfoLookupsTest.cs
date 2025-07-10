using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USFSISLineAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLists()
		{
			CombineAssertions(() =>
			{
				AssertType<USCCountryCollection>("USCountries", lookups.USCountries);
				AssertType<GlobalUniqueProductCodeQualifierList>("ProductIDQualifiers", lookups.ProductIDQualifiers);
				AssertType<ACEIntendedUseBaseCodeList>("ACEIntendedUseBaseCodes", lookups.ACEIntendedUseBaseCodes);
				AssertType<ZZRefCusCodeListCombinedCollection>("ImportEstablishments", lookups.ImportEstablishments);
				AssertContainsExactElementsInExactOrder("ImportEstablishments.DataGroupingCode", new[] { Core.Constants.CountryCodes.UnitedStates }, lookups.ImportEstablishments.DataGroupingCodes);
				AssertCollectionContains("ImportEstablishments.CodeTypes", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFSISEstablishmentNumbers, lookups.ImportEstablishments.CodeTypes);
			});
		}

		public void TestCertifyingIndividualList()
		{
			Assert(lookups.FSISCertifyingIndividualList.ContainsCode(PartyTypeList.Codes.CustomsBroker));
			Assert(lookups.FSISCertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Importer));
		}

		protected override void SetUp()
		{
			base.SetUp();
			fsisAddInfo = (USFSISLineAddInfo)((Customs.Business.IAddInfoManager)Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew()).AddInfo;
			lookups = fsisAddInfo.Lookups;
		}
		USFSISLineAddInfo fsisAddInfo;
		USFSISLineAddInfoLookups lookups;
	}
}
