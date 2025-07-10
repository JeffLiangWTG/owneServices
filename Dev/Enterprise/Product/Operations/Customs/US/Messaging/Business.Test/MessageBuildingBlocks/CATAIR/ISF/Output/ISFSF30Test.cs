using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Testing
{
	sealed class ISFSF30Test : NUnit.Framework.TestCase
	{
		public void TestMaskSSNOnISFSF30()
		{
			var message = "SF30IM                                    EI 123-45-6789                        ";
			var sf30 = new ISFSF30();
			sf30.Deserialise(message);
			AssertEquals("123-45-6789", sf30.EntityIdentifier);

			Environment.Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			sf30.Deserialise(message);
			AssertEquals("***-**-****", sf30.EntityIdentifier);
		}
	}
}
