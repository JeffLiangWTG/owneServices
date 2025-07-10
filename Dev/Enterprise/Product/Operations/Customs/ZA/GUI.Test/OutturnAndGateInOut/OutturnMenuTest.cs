using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class OutturnMenuTest : TestCaseWithFactory
	{
		public void TestSendOutturnCostcoMenuItemVisible()
		{
			OutturnTestHelper.SetupZZ(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			using (var menu = new OutturnMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(true, menu.sendOutturnCostcoMenuItem_Exposed.Visible);
			}

			var costco = Factory.New<COSTCOEDIMessage>();
			costco.EM_MessageNum = "123";
			costco.EM_MessageText = COSTCOTestMessage.Replace("\r\n", "");
			costco.EM_LinkUniqueID = header.PK;
			costco.EM_LinkTable = header.TablePrefix;
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cusres.EM_MessageText = CUSRESTestMessage.Replace("\r\n", "");
			cusres.EM_LinkUniqueID = header.PK;
			cusres.EM_LinkTable = header.TablePrefix;
			header.Messages.AddRange(costco, cusres);
			using (var menu = new OutturnMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(false, menu.sendOutturnCostcoMenuItem_Exposed.Visible);
			}
		}

		public void TestSendAmendmentOutturnMenuItemVisible()
		{
			OutturnTestHelper.SetupZZ(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			using (var menu = new OutturnMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(false, menu.sendAmendmentOutturnMenuItem_Exposed.Visible);
			}

			var costco = Factory.New<COSTCOEDIMessage>();
			costco.EM_MessageNum = "123";
			costco.EM_MessageText = COSTCOTestMessage.Replace("\r\n", "");
			costco.EM_LinkUniqueID = header.PK;
			costco.EM_LinkTable = header.TablePrefix;
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cusres.EM_MessageText = CUSRESTestMessage.Replace("\r\n", "");
			cusres.EM_LinkUniqueID = header.PK;
			cusres.EM_LinkTable = header.TablePrefix;
			header.Messages.AddRange(costco, cusres);
			using (var menu = new OutturnMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(true, menu.sendAmendmentOutturnMenuItem_Exposed.Visible);
			}
		}

		public void TestSendCancellationOutturnMenuItemVisible()
		{
			OutturnTestHelper.SetupZZ(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			using (var menu = new OutturnMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(false, menu.sendCancellationOutturnMenuItem_Exposed.Visible);
			}

			var costco = Factory.New<COSTCOEDIMessage>();
			costco.EM_MessageNum = "123";
			costco.EM_MessageText = COSTCOTestMessage.Replace("\r\n", "");
			costco.EM_LinkUniqueID = header.PK;
			costco.EM_LinkTable = header.TablePrefix;
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cusres.EM_MessageText = CUSRESTestMessage.Replace("\r\n", "");
			cusres.EM_LinkUniqueID = header.PK;
			cusres.EM_LinkTable = header.TablePrefix;
			header.Messages.AddRange(costco, cusres);
			using (var menu = new OutturnMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(true, menu.sendCancellationOutturnMenuItem_Exposed.Visible);
			}
		}

		const string COSTCOTestMessage = @"UNH+316+COSTCO:D:16A:UN:RCG001'
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

		const string CUSRESTestMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+B9C73560F3A54797909A498BE37010B5:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+8:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:123'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";
	}

	sealed class OutturnMenuForTest : OutturnMenu
	{
		public OutturnMenuForTest(AsycudaManifestHeader header) : base(header)
		{
		}

		public ZMenuItem sendOutturnCostcoMenuItem_Exposed => sendOutturnCostcoMenuItem;
		public ZMenuItem sendAmendmentOutturnMenuItem_Exposed => sendAmendmentOutturnMenuItem;
		public ZMenuItem sendCancellationOutturnMenuItem_Exposed => sendCancellationOutturnMenuItem;
	}
}
