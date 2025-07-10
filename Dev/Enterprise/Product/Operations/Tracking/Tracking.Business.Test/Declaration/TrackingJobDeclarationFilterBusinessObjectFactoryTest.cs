using Enterprise.Customs.Module;
using Enterprise.Customs.Module.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingJobDeclarationFilterBusinessObjectFactoryTest : CountrySpecificJobDeclarationFilterBusinessObjectFactoryTest
	{
		public void TestGetJobDeclarationFilterBusinessObjectForCountry()
		{
			var jDFactory = new TrackingJobDeclarationFilterBusinessObjectFactory();
			Assert(jDFactory.GetJobDeclarationFilterBusinessObject("US") is JobDeclarationFilterBusinessObject);
			AssertEquals(jDFactory.GetJobDeclarationFilterBusinessObject("US").CountryCode, "US");
		}
	}
}
