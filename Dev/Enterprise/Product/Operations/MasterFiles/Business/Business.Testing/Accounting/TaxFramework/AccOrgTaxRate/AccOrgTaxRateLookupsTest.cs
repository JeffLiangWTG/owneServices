using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.TaxFramework.Testing
{
	sealed class AccOrgTaxRateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSourceRateMethod()
		{
			AccOrgTaxRate orgTaxRate = Factory.New<AccOrgTaxRate>();
			AssertArrayEqualsByElements(new[] { "MOV", "MON", "QUA" }, orgTaxRate.Lookups.Source.GetAllCodes());
		}
	}
}
