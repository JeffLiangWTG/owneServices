using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(UEMEDIInterchange))]
	public class UEMEDIInterchangeTest : EDIInterchangeTest
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(ApplicationCodeList.Codes.USExportManifest, Interchange.EI_ApplicationCode);
		}

		public void TestShouldSendViaEHub()
		{
			AssertEquals(true, Interchange.ShouldSendViaEHub);
		}

		public void TestCreateMessageFromInterchange()
		{
			var interchange = Interchange as UEMEDIInterchange;
			AssertNotNull(interchange);

			interchange.EI_GB = Env.CurrentBranchPK;
			interchange.EI_BodyText = "Test Body";

			var message = interchange.CreateMessageFromInterchange();

			CombineAssertions("Create UEMEDIMessage through UEMEDIInterchange.", () =>
			{
				AssertNotNull("UEMEDIMessage", message);
				AssertEquals("ContainedMessages count", 1, interchange.ContainedMessages.Count);
				AssertEquals("Message was added to EDIInterchange ContainedMessages.", interchange.ContainedMessages[0].PK, message.PK);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.ExportManifestResponse, message.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_GB", Env.CurrentBranchPK, message.EM_GB);
				AssertEquals("EM_MessageText", "Test Body", message.EM_MessageText);
				AssertEquals("EM_MessageNum", "00001", message.EM_MessageNum);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var businessObjectFactory = new BusinessObjectFactory();
			Interchange = businessObjectFactory.New<UEMEDIInterchange>();
			Interchange.EI_From = "CW1";
			Interchange.EI_To = "USCustoms";
		}
	}
}
