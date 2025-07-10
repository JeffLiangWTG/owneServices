using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FSIMessage))]
	sealed class FSIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadFSIMessage()
		{
			var message = Factory.New<FSIMessage>();
			AssertEquals(ApplicationCodeList.Codes.USAMA, message.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals(Constants.AIMMessageSubTypes.FSI, message.EM_MessageSubType);
			Factory.Save();

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertType<FSIMessage>(reloadedMessage);

			AssertType<FSIMessage>(GetNewBusinessObject());
		}

		[TestDate(2020, 12, 27)]
		public void TestFSIMessageProperties()
		{
			var messageText = @"FSI
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/24DEC-A
CSN/1C-20/12DEC1430/0158232873876/HLD1234";

			var message = Factory.New<FSIMessage>();
			message.EM_MessageText = messageText;

			CombineAssertions(() =>
			{
				AssertEquals("FSI", message.ComponentIdentifier);

				AssertEquals("MIA", message.AirportOfArrival);
				AssertEquals("KLM", message.CargoTerminalOperator);

				AssertEquals("081", message.AirWaybillPrefix);
				AssertEquals("11223344", message.AirWaybillSerialNumber);
				AssertEquals("HAWB123", message.HAWBNumber);
				AssertEquals("123456", message.PackageTrackingIdentifier);

				AssertEquals("KL325", message.FlightNumber);
				AssertEquals(new ZDate(2020, 12, 24), message.CalculatedScheduledArrivalDate);
				AssertEquals("A", message.PartArrivalReference);

				AssertEquals("1C", message.ActionCode);
				AssertEquals("HLD1234", message.Remarks);
			});
		}

		[TestDate(2020, 12, 27)]
		public void TestScheduledArrivalDate_NextYear()
		{
			var messageText = @"FSI
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/02JAN-A
CSN/1C-20/12DEC1430/0158232873876/HLD1234
";

			var message = Factory.New<FSIMessage>();
			message.EM_MessageText = messageText;
			AssertEquals(new ZDate(2021, 1, 2), message.CalculatedScheduledArrivalDate);
		}
	}
}
