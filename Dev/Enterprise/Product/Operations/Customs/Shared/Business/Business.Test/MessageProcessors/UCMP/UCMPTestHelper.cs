using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP.Testing
{
	internal class UCMPTestHelper
	{
		public static (BaseJobDeclaration declaration, EDIInterchange incomingInterchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage) CreateTestInterchange(BusinessObjectFactory factory, string applicationCode, string bodyText, string interchangeType, bool hasLinkedObject = true)
		{
			var declaration = factory.New<BaseJobDeclaration>();

			var outgoingMessage = factory.New<EDIMessageForTest>();
			if (hasLinkedObject)
			{
				outgoingMessage.EM_LinkTable = "JobDeclaration";
				outgoingMessage.EM_LinkUniqueID = declaration.PK;
			}
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_ApplicationCode = applicationCode;
			outgoingMessage.EM_MessageType = "ZZZ";

			var sessionGUID = ZGuid.NewZGuid();
			var outgoingInterchange = factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "OUT123456" + applicationCode;
			outgoingInterchange.EI_BodyText = "Outgoing Test Data";
			outgoingInterchange.EI_SessionGUID = sessionGUID;
			outgoingInterchange.EI_ReceiveTransmit = "TRX";
			outgoingInterchange.EI_Status = "SNT";
			outgoingInterchange.EI_ApplicationCode = applicationCode;
			outgoingInterchange.EI_InterchangeType = "ZZZ";
			outgoingMessage.EM_EI = outgoingInterchange.PK;

			var incomingInterchange = factory.New<EDIInterchange>();
			incomingInterchange.EI_InterchangeNum = "INT001INT001INT001INT001INT001" + applicationCode;
			incomingInterchange.EI_BodyText = bodyText;
			incomingInterchange.EI_SessionGUID = sessionGUID;
			incomingInterchange.EI_ReceiveTransmit = "RCV";
			incomingInterchange.EI_Status = "QUE";
			incomingInterchange.EI_ApplicationCode = applicationCode;
			incomingInterchange.EI_InterchangeType = interchangeType;
			factory.Save();

			return (declaration, incomingInterchange, outgoingInterchange, outgoingMessage);
		}

		public static string GetTestUniversalEvent(string eventType, string eventParamMessageType, string eventParamType, string reason)
		{
			return @$"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Event>
    <EventTime>2024-09-09 01:23:45.678</EventTime>
    <EventType>{eventType}</EventType>
    <EventParameters>
      <MessageType>{eventParamMessageType}</MessageType>
      <Type>{eventParamType}</Type>
      <Reason>{reason}</Reason>
    </EventParameters>
    <ContextCollection>
      <Context>
        <Type>TST</Type>
        <Value>OriginalMessage</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
		}
	}
	public class EDIMessageForTest(BusinessObjectFactory factory, DataRow row) : EDIMessage(factory, row)
	{
		protected override string GetMessageReferenceNumber() => "1234";
	}
}
