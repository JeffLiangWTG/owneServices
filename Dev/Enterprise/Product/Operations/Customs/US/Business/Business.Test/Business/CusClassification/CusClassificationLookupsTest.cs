using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusClassificationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffs()
		{
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			CusClassificationLookups lookups = classification.Lookups;
			AssertEquals("Tariffs", typeof(USCTariffCollection), lookups.Tariffs.GetType());
			classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			AssertEquals("Tariffs", typeof(Universal.TariffViewCollection), lookups.Tariffs.GetType());
		}
	}
}
