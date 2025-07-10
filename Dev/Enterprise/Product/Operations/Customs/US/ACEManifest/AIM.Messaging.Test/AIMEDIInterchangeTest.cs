using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(AIMEDIInterchange))]
	public class AIMEDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadInterchange()
		{
			var interchange = Factory.New<AIMEDIInterchange>();
			AssertEquals(ApplicationCodeList.Codes.USAMA, interchange.EI_ApplicationCode);

			interchange.EI_To = "USC";
			interchange.EI_From = "TEST";
			Factory.Save();

			var reloadedInterchange = new BusinessObjectFactory().Load<EDIInterchange>(interchange.PK);
			AssertType<AIMEDIInterchange>(reloadedInterchange);

			AssertType<AIMEDIInterchange>(GetNewBusinessObject());
		}

		public void TestCreateMessageFromInterchange()
		{
			var msgText = @"FER
QF101/12DEC
081-11223344-HAWB123/123456
ERR/001ERRORDESCRIPTION
ERR/002ANOTHERERROR
";

			var interchange = Factory.New<AIMEDIInterchange>();
			interchange.EI_HeaderText = "WASUCCR\x0D\x0A.BCBTSV9";
			interchange.EI_BodyText = msgText;

			var message = interchange.CreateMessageFromInterchange();

			AssertEquals("EM_ReceiveTransmit", message.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			AssertEquals("EM_Status", message.EM_Status, EDIMessage.Status.Queued);
			AssertEquals("EM_MessageType", message.EM_MessageType, EDIMessageTypeList.Codes.FHL);
			AssertEquals("EM_GB", message.EM_GB, interchange.EI_GB);
			AssertEquals("EM_MessageText", msgText, message.EM_MessageText);
			AssertEquals("EM_MessageSubType", message.EM_MessageSubType, Constants.AIMMessageSubTypes.FER);
		}
	}
}
