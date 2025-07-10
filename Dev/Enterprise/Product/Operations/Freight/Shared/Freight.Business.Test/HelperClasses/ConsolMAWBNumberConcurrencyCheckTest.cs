using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Business.Test
{
	sealed class ConsolMAWBNumberConcurrencyCheckTest : TestCaseWithFactory
	{
		public void TestRegister()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();
			consol.JK_SystemCreateTimeUtc = ZDateTime.Now;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "12345678901";

			Assert("concurrency check has been registered for consol",
				ConsolMAWBNumberConcurrencyCheck.IsRegistered(factory, consol.JK_MasterBillNum, consol.PK, consol.TransportMode, consol.IsCoLoad, consol.JK_SystemCreateTimeUtc));

			consol.JK_MasterBillNum = "123";
			Assert("concurrency check has been removed for consol",
				!ConsolMAWBNumberConcurrencyCheck.IsRegistered(factory, consol.JK_MasterBillNum, consol.PK, consol.TransportMode, consol.IsCoLoad, consol.JK_SystemCreateTimeUtc));
		}

		public void TestConcurrencyCheck_ForDuplicateMAWBNumber()
		{
			var testMasterBillNum = "12345678";
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolInFactory1 = factory1.New<ForwardingConsol>();
			consolInFactory1.JK_SystemCreateTimeUtc = ZDateTime.Now;
			consolInFactory1.JK_AgentType = Constants.AgentType.Agent;
			consolInFactory1.JK_TransportMode = Constants.TransportModes.Air;
			consolInFactory1.JK_MasterBillNum = testMasterBillNum;

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolInFactory2 = factory2.New<ForwardingConsol>();
			consolInFactory2.JK_SystemCreateTimeUtc = ZDateTime.Now;
			consolInFactory2.JK_AgentType = Constants.AgentType.Agent;
			consolInFactory2.JK_TransportMode = Constants.TransportModes.Air;
			consolInFactory2.JK_MasterBillNum = testMasterBillNum;

			factory1.Save();

			try
			{
				factory2.Save();
				Fail("Expected to throw ZCannotSaveException");
			}
			catch (ZCannotSaveException ex)
			{
				AssertEquals(ex.Heading, "MAWB number concurrency error, you must reopen this form again before saving");
				AssertEquals(ex.Message, "Another user has made changes that conflicts with your own changes. You must reopen this form again before saving.");
			}
		}
	}
}
