using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestRelatedIndicatorList()
		{
			AssertEquals(Factory.GetCachedValue<MasterFiles.Business.Customs.RelatedIndicatorList>(), invoiceHeader.Lookups.RelatedIndicatorList);
		}

		public void TestValuationCodeList()
		{
			AssertEquals(Factory.GetCachedValue<MasterFiles.Business.Customs.ZA.ValuationCodeList>(), invoiceHeader.Lookups.ValuationCodeList);
		}

		public void TestROOTypesList()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROOType, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.CountryCodes.SouthAfrica);
			var codeList1 = testHelper.CreateAdditionalInformationCusCodeEntry("XX");
			codeList1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty);
			var codeList2 = testHelper.CreateAdditionalInformationCusCodeEntry("YY");
			codeList2.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty);
			Factory.Save();
			var parent1 = Factory.New<JobComInvoiceHeader>();
			var parent2 = Factory.New<JobComInvoiceHeader>();
			var testLookups = parent1.Lookups;
			CombineAssertions("Test ROOTypesList", () =>
			{
				Assert("Test 1", testLookups.ROOTypesList.ContainsCode("XX"));
				Assert("Test 2", testLookups.ROOTypesList.ContainsCode("YY"));
				var list1 = parent1.Lookups.ROOTypesList;
				var list2 = parent2.Lookups.ROOTypesList;
				AssertSame("IsCached", list1, list2);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
	}
}
