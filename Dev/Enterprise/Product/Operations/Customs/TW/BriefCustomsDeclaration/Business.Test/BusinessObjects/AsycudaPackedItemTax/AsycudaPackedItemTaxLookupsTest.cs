using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemTaxLookups))]
	sealed class AsycudaPackedItemTaxLookupsTest : BusinessObjectValidationTestCase
	{
		public void TestChargeTypeList()
		{
			AssertSame(Factory.GetCachedValue<ChargeTypeOtherList>(), Lookups.ChargeTypeList);
		}

		public void TestTariffCollection()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			Tax.AET_ChargeType = "SS";
			var tariffCollection = Lookups.TariffCollection;
			tariffCollection.Load();
			AssertContainsExactElementsInAnyOrder(new string[] { "SSTariff", "SSDoesNotBelongToMainTariff", "SEDAN", "ASUS", "OPPO" }, tariffCollection.Select(c => c.ZZ1_TariffCode));

			Tax.AET_ChargeType = "CT";
			tariffCollection = Lookups.TariffCollection;
			tariffCollection.Load();
			AssertContainsExactElementsInAnyOrder(new string[] { "CTTariff", "CTDoesNotBelongToMainTariff" }, tariffCollection.Select(c => c.ZZ1_TariffCode));
		}

		public void TestMethodOfCalculationList()
		{
			AssertEquals(TWRefCusCodeListTypes.GetMethodOfCalculationList(Factory), Lookups.MethodOfCalculationList);
		}

		public void TestMethodOfPaymentList()
		{
			AssertSame(Factory.GetCachedValue<TaxFeePaymentMethodList>(), Lookups.MethodOfPaymentList);
		}

		AsycudaManifestHeader header;
		AsycudaManifestHeader Header => header ?? (header = Factory.NewWithValidTestData<AsycudaManifestHeader>());

		AsycudaPackedItemTax tax;
		AsycudaPackedItemTax Tax => tax ?? (tax = Header.Bills.AddNew().PackedItems.AddNew().AsycudaTaxes.AddNew());

		AsycudaPackedItemTaxLookups Lookups => Tax.Lookups;
	}
}
