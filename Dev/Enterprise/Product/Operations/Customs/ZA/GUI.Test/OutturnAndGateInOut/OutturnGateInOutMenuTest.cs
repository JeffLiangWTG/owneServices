using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class OutturnGateInOutMenuTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var header = GOVGIOExamples.CreateSeaDepotGateIn(Factory);
			Factory.Save();
			using (var menu = new OutturnGateInOutMenuForTest(header))
			{
				AssertEquals("pre-condition", 0, header.Messages.Count);
				menu.SendOriginalMenuItem_Exposed.PerformClick();
				AssertEquals(1, header.Messages.Count);
				var ediMessage = header.Messages[0];
				AssertEquals(SARSEDIMessage.MessageTypes.GOVGIO, ediMessage.EM_MessageType);
				AssertEquals(MessageSubTypeCodes.Codes.Original, ediMessage.EM_MessageSubType);
				AssertContains("message should look like GOVGIO message", "+GOVCBR:D:", ediMessage.EM_MessageText);
				AssertEquals(EDIMessage.ApplicationCodes.SouthAfricanCustoms, ediMessage.EM_ApplicationCode);
				AssertEquals(EDIMessage.Status.Queued, ediMessage.EM_Status);
				AssertEquals("Create GOVGIO message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestValidateManifestHeader()
		{
			var header = GOVGIOExamples.CreateSeaDepotGateIn(Factory);
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.TerminalGateIn;
			header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
			header.AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
			Factory.Save();
			using (var menu = new OutturnGateInOutMenuForTest(header))
			{
				AssertEquals("pre-condition", 0, header.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.SendOriginalMenuItem_Exposed.PerformClick();
				AssertEquals("For Gate In/Out Manifests of Type 'TGI' De-consolidation address is Mandatory.\r\nFor Gate In/Out Manifests of Type 'TGI' Terminal address is Mandatory.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);
			}
		}

		public void TestSendOriginalMenuItemVisible()
		{
			OutturnTestHelper.SetupZZ(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			using (var menu = new OutturnGateInOutMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(true, menu.SendOriginalMenuItem_Exposed.Visible);
			}

			var costco = Factory.New<GOVGIOEDIMessage>();
			costco.EM_MessageNum = "123";
			costco.EM_MessageText = GOVGIOTestMessage.Replace("\r\n", "");
			costco.EM_LinkUniqueID = header.PK;
			costco.EM_LinkTable = header.TablePrefix;
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cusres.EM_MessageText = CUSRESTestMessage_GOVGIO.Replace("\r\n", "");
			cusres.EM_LinkUniqueID = header.PK;
			cusres.EM_LinkTable = header.TablePrefix;
			header.Messages.AddRange(costco, cusres);
			using (var menu = new OutturnGateInOutMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(false, menu.SendOriginalMenuItem_Exposed.Visible);
			}
		}

		public void TestSendAmendemntMenuItemVisible()
		{
			OutturnTestHelper.SetupZZ(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			using (var menu = new OutturnGateInOutMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(false, menu.SendAmendemntMenuItem_Exposed.Visible);
			}

			var costco = Factory.New<GOVGIOEDIMessage>();
			costco.EM_MessageNum = "123";
			costco.EM_MessageText = GOVGIOTestMessage.Replace("\r\n", "");
			costco.EM_LinkUniqueID = header.PK;
			costco.EM_LinkTable = header.TablePrefix;
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cusres.EM_MessageText = CUSRESTestMessage_GOVGIO.Replace("\r\n", "");
			cusres.EM_LinkUniqueID = header.PK;
			cusres.EM_LinkTable = header.TablePrefix;
			header.Messages.AddRange(costco, cusres);
			using (var menu = new OutturnGateInOutMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(true, menu.SendAmendemntMenuItem_Exposed.Visible);
			}
		}

		public void TestSendCancellationMenuItemVisible()
		{
			OutturnTestHelper.SetupZZ(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			using (var menu = new OutturnGateInOutMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(false, menu.SendCancellationMenuItem_Exposed.Visible);
			}

			var costco = Factory.New<GOVGIOEDIMessage>();
			costco.EM_MessageNum = "123";
			costco.EM_MessageText = GOVGIOTestMessage.Replace("\r\n", "");
			costco.EM_LinkUniqueID = header.PK;
			costco.EM_LinkTable = header.TablePrefix;
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cusres.EM_MessageText = CUSRESTestMessage_GOVGIO.Replace("\r\n", "");
			cusres.EM_LinkUniqueID = header.PK;
			cusres.EM_LinkTable = header.TablePrefix;
			header.Messages.AddRange(costco, cusres);
			using (var menu = new OutturnGateInOutMenuForTest(header))
			{
				menu.ShowPopupMenu();
				AssertEquals(true, menu.SendCancellationMenuItem_Exposed.Visible);
			}
		}

		const string GOVGIOTestMessage = @"UNH+449+GOVCBR:D:16A:UN:RCG001+GOVGIO'
BGM+655:::ADI+55E8E3293886428DB5DE455857C358F8+9'
LOC+11'
LOC+34'
NAD+TB'
IFD+1'
NAD+CA'
NAD+DC'
DOC+706'
TDT+20++4'
DTM+133::102'
QTY+264:0'
POC'
UNS+D'
HYN+3'
CNI+1'
RFF+ACD:OGM0000007'
DOC+704'
DOC+703+111'
EQD+CN'
SEQ++1'
SEL'
SEQ++1'
TDT+20'
DTM+6::203'
GDS+BB:ZZZ'
LIN+1'
DOC+914'
UNS+S'
UNT+30+449'";

		const string CUSRESTestMessage_GOVGIO = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+55E8E3293886428DB5DE455857C358F8:0'
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

	sealed class OutturnGateInOutMenuForTest : OutturnGateInOutMenu
	{
		public OutturnGateInOutMenuForTest(AsycudaManifestHeader outturn) : base(outturn)
		{
		}

		public ZMenuItem SendOriginalMenuItem_Exposed => SendOriginalMenuItem;
		public ZMenuItem SendAmendemntMenuItem_Exposed => SendAmendemntMenuItem;
		public ZMenuItem SendCancellationMenuItem_Exposed => SendCancellationMenuItem;
	}
}
