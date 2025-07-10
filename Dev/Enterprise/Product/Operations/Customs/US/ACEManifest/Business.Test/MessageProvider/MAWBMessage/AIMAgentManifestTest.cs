using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMAgentManifestTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var additionalMessageInformation = new AdditionalMessageInformation(manifestHeader);
			additionalMessageInformation.AM_Agent = "GAZA745";
			var aimAgent = new AIMAgent(additionalMessageInformation);
			AssertEquals("GAZA745", aimAgent.AirAMSParticipantCode);
		}
	}
}
