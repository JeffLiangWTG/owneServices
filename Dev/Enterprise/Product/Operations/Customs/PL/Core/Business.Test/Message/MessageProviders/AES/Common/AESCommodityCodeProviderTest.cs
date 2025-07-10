using System;
using System.Linq;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESCommodityCodeProviderTest : Customs.Business.Testing.DataProviderTestCase<AESCommodityCodeProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null InvoiceLine", "Value cannot be null.\r\nParameter name: invoiceLine",
				() => new AESCommodityCodeProvider(null));
		});
	}

	public void TestHarmonizedSystemSubHeadingCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty tariff", string.Empty, GetProvider().HarmonizedSystemSubHeadingCode);
			invoiceLine.JI_Tariff = "123";
			AssertEquals("3 char tariff", "123", GetProvider().HarmonizedSystemSubHeadingCode);
			invoiceLine.JI_Tariff = "12345678";
			AssertEquals("8 char tariff", "123456", GetProvider().HarmonizedSystemSubHeadingCode);
		});
	}

	public void TestCombinedNomenclatureCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty tariff", string.Empty, GetProvider().CombinedNomenclatureCode);
			invoiceLine.JI_Tariff = "123";
			AssertEquals("3 char tariff", string.Empty, GetProvider().CombinedNomenclatureCode);
			invoiceLine.JI_Tariff = "1234567";
			AssertEquals("7 char tariff", "7", GetProvider().CombinedNomenclatureCode);
			invoiceLine.JI_Tariff = "12345678";
			AssertEquals("8 char tariff", "78", GetProvider().CombinedNomenclatureCode);
		});
	}

	public void TestTARICAdditionalCodes()
	{
		var taricCodes = Provider.TARICAdditionalCodes.ToArray();
		CombineAssertions(() =>
		{
			AssertEquals(3, taricCodes.Length);
			TestHelper.TestCollectionHasCorrectIndexOrder(GetProvider().TARICAdditionalCodes, (item) => item.SequenceNumber);
		});
	}

	public void TestNationalAdditionalCodes()
	{
		var supplementaryCode2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode2.CY_Code = "J";
		var nationalCodes = GetProvider().NationalAdditionalCodes.ToArray();
		CombineAssertions(() =>
		{
			AssertEquals(2, nationalCodes.Length);
			TestHelper.TestCollectionHasCorrectIndexOrder(GetProvider().NationalAdditionalCodes, (item) => item.SequenceNumber);
		});
	}

	protected override AESCommodityCodeProvider GetProvider() => new AESCommodityCodeProvider(invoiceLine);

	protected override void SetUp()
	{
		base.SetUp();
		invoiceLine = Factory.New<JobComInvoiceLine>();

		var supDoc1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supDoc1.CY_Code = "ABC";
		var supDoc2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supDoc2.CY_Code = "DCD";
		var supDoc3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supDoc3.CY_Code = "JKL";
		invoiceLine.AdditionalSupplementaryCodes.AddNew();
	}

	JobComInvoiceLine invoiceLine;
}
