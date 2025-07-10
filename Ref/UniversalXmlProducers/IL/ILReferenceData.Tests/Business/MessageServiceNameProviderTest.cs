using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	sealed class MessageServiceNameProviderTest
	{
		[Test]
		public void TestGetServiceName()
		{
			Assert.AreEqual("GetCD_8347_8348_Web01_02_CurrencyRateSearch", MessageServiceNameProvider.GetServiceName("347"));
			Assert.AreEqual("GetSYSTBL_MSG9000_9001_SystemTableRequest", MessageServiceNameProvider.GetServiceName("901"));
			Assert.AreEqual("", MessageServiceNameProvider.GetServiceName("NotInList"));
		}
	}
}
