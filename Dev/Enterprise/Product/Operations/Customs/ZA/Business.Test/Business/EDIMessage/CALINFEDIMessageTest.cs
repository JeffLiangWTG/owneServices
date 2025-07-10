using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CALINFEDIMessage))]
	sealed class CALINFEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLocalReferenceNumber()
		{
			var calinf = Factory.New<CALINFEDIMessage>();
			calinf.EM_MessageText = CALINFTestMessage.Replace("\r\n", "");
			AssertEquals("B9C73560F3A54797909A498BE37010B5", calinf.LocalReferenceNumber);
		}

		public void TestParentMessageNumber()
		{
			var calinf = Factory.New<CALINFEDIMessage>();
			calinf.EM_MessageText = CALINFTestMessage.Replace("\r\n", "");
			AssertEquals("B9C73560F3A54797909A498BE37010B5", calinf.ParentMessageNumber);
		}

		const string CALINFTestMessage = @"UNH+316+CALINF:D:16A:UN:RCG001'
BGM+788+B9C73560F3A54797909A498BE37010B5+9'
FTX+ADI'
TDT+20++++:172:20+++:103'
RFF+ACL'
LOC+11+:139:6+::ZZZ'
NAD+MS+::ZZZ'
NAD+RL+::ZZZ'
EQD+BB'
SEL+NO SEAL NO'
CNT+8:0'";
	}
}
