using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobServiceTypeProviderTest : TestCaseWithFactory
	{
		public void TestServiceTypeNeedsToBeUnique()
		{
			var jobServiceTypeProvider = ObjectFactory.Get<Integration.Customs.TW.IJobServiceTypeProvider>();
			Assert("ICI can have more than one.", !jobServiceTypeProvider.ServiceTypeNeedsToBeUnique(ServiceTypes.CommodityInspection));
			Assert("XX1 can't have more than one.", jobServiceTypeProvider.ServiceTypeNeedsToBeUnique("XX1"));
		}
	}
}
