using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ServiceTypesTest : TestCaseWithFactory
	{
		public void TestServiceTypes()
		{
			var serviceTypes = new ServiceTypes();
			Assert(serviceTypes.ContainsCode(ServiceTypes.CommodityInspection));
		}
	}
}
