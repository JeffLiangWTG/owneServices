using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class TRInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestMessagesPopulateNewEdiMessage()
		{
			var bodyText = TRMessageTestHelper.GetFileText("T3OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var interchange = MessageTestHelper.CreateInterchange(Factory, bodyText, "T30 receive message", TRMessageTypes.Codes.TRE, "TRECustoms", "TRECustomsTest");

			var processor = new TRInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.TRCustoms });

			processor.ExecuteBatch();
			interchange.Reload();

			CombineAssertions("Below test is just to prove necessary EDIMessage created correctly!", () =>
			{
				var messagesCreated = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
				AssertEquals("NumberOfediMessages", 1, messagesCreated.Length);

				var message = messagesCreated[0];

				AssertEquals(message.EM_IsActive, ZBool.True);
				AssertEquals(message.EM_IsTestMessage, ZBool.False);
				AssertEquals(message.EM_MessageOwner, string.Empty);
				AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.TRCustoms);
				AssertEquals(message.EM_MessageType, interchange.EI_InterchangeType);
				AssertEquals(message.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				AssertEquals(message.EM_Status, EDIMessage.Status.Queued);
				AssertEquals(message.EM_MessageText, interchange.EI_BodyText);
				AssertEquals(message.EM_GB, interchange.EI_GB);
				AssertEquals(message.EM_GE, GlbDepartment.CurrentDepartment.PK);
				AssertEquals(message.EM_EI, interchange.PK);
			});
		}
	}
}
