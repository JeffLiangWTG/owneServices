using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Testing
{
	sealed class ISFSF10Test : NUnit.Framework.TestCase
	{
		public void TestMaskSSNOnISFSF10()
		{
			var message = "SF10101ACTEI 123-45-6789            11                   111-22-6789    018     ";
			var sf10 = new ISFSF10();
			sf10.Deserialise(message);
			AssertEquals("123-45-6789", sf10.ISFImporterNumber);
			AssertEquals("111-22-6789", sf10.BondHolder);

			Environment.Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			sf10.Deserialise(message);
			AssertEquals("***-**-****", sf10.ISFImporterNumber);
			AssertEquals("***-**-****", sf10.BondHolder);
		}
	}
}
