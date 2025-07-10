using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;

namespace Enterprise.Customs.NZ.Business.MasterFiles.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.ZArchitecture.Core;

	public class CusClassPartPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffs()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			var lookups = pivot.Lookups;
			Assert("Tariffs is NonDependentNZCClassificationCollection", lookups.Tariffs is NonDependentNZCClassificationCollection);
		}

		public void TestConcessionList()
		{
			var pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			var lookups = pivot.Lookups;
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert("Concessions is NonDependentNZCClassificationCollection", lookups.ConcessionList is NonDependentNZCConcessionCollection);
			}
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);
				pivot.CI_TariffNum = "123456789";

				AssertEquals("ConcessionList.Count", 2, lookups.ConcessionList.Count);
				pivot.CI_RN_NKCountryOfOrigin = "AU";
				AssertEquals("ConcessionList.Count", 1, lookups.ConcessionList.Count);
				Assert("Concessions is CodeDescriptionPairList", lookups.ConcessionList is CodeDescriptionPairList);
			}
		}
	}
}
