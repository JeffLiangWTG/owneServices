using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ServiceLevelInfoTest : TestCaseWithFactory
	{
		public void TestServiceLevelInfo()
		{
			var info = new ServiceLevelRatingInformation(
				new ServiceLevelInfo("ABC", ServiceLevelType.Client),
				new ServiceLevelInfo("XYZ", ServiceLevelType.Carrier));
			AssertEquals("ABC", info.GetServiceLevel(ServiceLevelType.Client));
			AssertEquals("XYZ", info.GetServiceLevel(ServiceLevelType.Carrier));

			info = new ServiceLevelRatingInformation();
			AssertEquals("STD", info.GetServiceLevel(ServiceLevelType.Client));
			AssertEquals("STD", info.GetServiceLevel(ServiceLevelType.Carrier));

			info = new ServiceLevelRatingInformation(
				new ServiceLevelInfo("ABC", ServiceLevelType.Client),
				new ServiceLevelInfo("", ServiceLevelType.Carrier));
			AssertEquals("ABC", info.GetServiceLevel(ServiceLevelType.Client));
			AssertEquals("STD", info.GetServiceLevel(ServiceLevelType.Carrier));
		}
	}
}
