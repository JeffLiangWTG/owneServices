using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	sealed class AIMInboundMessageTest : TestCaseWithFactory
	{
		[TestDate(2020, 12, 27)]
		public void TestMessageProperties()
		{
			var messageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/24DEC-A
CSN/1C-20/12DEC1430/0158232873876/HLD1234";

			var message = Factory.New<AIMInboundMessageForTest>();
			message.EM_MessageText = messageText;

			CombineAssertions(() =>
			{
				AssertEquals("FSN", message.ComponentIdentifier);
				AssertEquals("123456", message.PackageTrackingIdentifier);
				AssertEquals("081", message.AirWaybillPrefix);
				AssertEquals("11223344", message.AirWaybillSerialNumber);
				AssertEquals("HAWB123", message.HAWBNumber);
				AssertEquals("08111223344", message.MAWBNumber);
				AssertEquals("081-11223344", message.MAWBNumberFormatted);
			});

			AssertEndsWith("Last Line has CRLF added", "\r\n", message.LinesExposed.Last());
		}
	}

	sealed class AIMInboundMessageForTest : AIMInboundMessage
	{
		public AIMInboundMessageForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString MessageTypeCode => "TST";
		public override ZString MessageTypeDescription => "AIM Inbound Message For Test";

		public IList<ZString> LinesExposed => Lines;
	}
}
