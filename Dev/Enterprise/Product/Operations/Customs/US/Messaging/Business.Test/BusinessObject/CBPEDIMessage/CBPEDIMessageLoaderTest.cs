using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	[TestedType(typeof(CBPEDIMessage.Loader))]
	sealed class CBPEDIMessageLoaderTest : LoaderTestCase
	{
		public void TestLoadTop1WithDirectionAndDate()
		{
			var message1 = Factory.New<CBPMessageForTesting>();
			message1.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			message1.EM_MessageNum = "123";

			var message2 = Factory.New<CBPMessageForTesting>();
			message2.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			message2.EM_MessageNum = "123";
			var loader = new CBPEDIMessage.Loader(Factory);
			AssertEquals(message1, loader.LoadTop1WithDirectionOrderByCreatTime(CBPEDIInterchange.ApplicationCodeForTesting, "123", CBPEDIMessage.Direction.Receive));
			AssertEquals(message2, loader.LoadTop1WithDirectionOrderByCreatTime(CBPEDIInterchange.ApplicationCodeForTesting, "123", CBPEDIMessage.Direction.Transmit));
			AssertNull(loader.LoadTop1WithDirectionOrderByCreatTime(CBPEDIInterchange.ApplicationCodeForTesting, "123", CBPEDIMessage.Direction.Receive, extraFilter: ZQuery.NoResultQuery));

			message1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse;
			message2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendment;
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2013, 5, 20);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2013, 5, 20);

			var message3 = Factory.New<CBPMessageForTesting>();
			message3.EM_ApplicationCode = CBPEDIMessage.ApplicationCodes.AMS;
			message3.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			message3.EM_MessageNum = "123";
			message3.EM_SystemCreateTimeUtc = new ZDateTime(2013, 10, 21);

			var message4 = Factory.New<CBPMessageForTesting>();
			message4.EM_ApplicationCode = CBPEDIMessage.ApplicationCodes.AMS;
			message4.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			message4.EM_MessageNum = "123";
			message4.EM_SystemCreateTimeUtc = new ZDateTime(2013, 10, 19);

			AssertEquals(message3.PK, loader.LoadTop1WithDirectionOrderByCreatTime(CBPEDIMessage.ApplicationCodes.AMS, "123", CBPEDIMessage.Direction.Receive).PK);
			AssertEquals(message4.PK, loader.LoadTop1WithDirectionOrderByCreatTime(CBPEDIMessage.ApplicationCodes.AMS, "123", CBPEDIMessage.Direction.Transmit).PK);
		}

		public void TestLoad()
		{
			CBPMessageForTesting message1 = Factory.New<CBPMessageForTesting>();
			message1.EM_MessageType = "TTT";
			message1.EM_MessageNum = "123";

			CBPMessageForTesting message2 = Factory.New<CBPMessageForTesting>();
			message2.EM_MessageType = "TTT";
			message2.EM_MessageNum = "234";

			AssertEquals(message1, new CBPEDIMessage.Loader(Factory).LoadTop1(CBPEDIInterchange.ApplicationCodeForTesting, "TTT", "123"));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CBPEDIMessage.Loader(Factory);
	}
}
