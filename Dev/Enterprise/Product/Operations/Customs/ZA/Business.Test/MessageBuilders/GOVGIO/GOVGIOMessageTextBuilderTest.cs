using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Edifact.D16A.Messages.GOVCBR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class GOVGIOMessageTextBuilderTest : TestCaseWithFactory
	{
		public void TestEmpty()
		{
			foreach (var codeDescription in new GateInOutMessageTypeCodeList().Cast<ICodeDescription>())
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.GateInOutMessageType = codeDescription.Code;
				{
					var builder = new GOVGIOMessageTextBuilderForTest(header);
					var result = builder.GenerateMessageBody();
					AssertContains("+GOVCBR:D:", result);
				}

				header.Bills.AddNew();
				header.Bills.AddNew().Packs.AddNew();
				header.Containers.AddNew();
				header.Bills.AddNew().Packs.AddNew().ContainerPK = header.Containers.AddNew().PK;
				{
					var builder = new GOVGIOMessageTextBuilderForTest(header);
					var result = builder.GenerateMessageBody();
					AssertContains("+GOVCBR:D:", result);
				}
			}
		}

		public void TestCreateAirDepotGateIn()
		{
			var header = GOVGIOExamples.CreateAirDepotGateIn(Factory);

			var builder = new GOVGIOMessageTextBuilderForTest(header);
			var result = builder.GenerateMessageBody();
			AssertMultilineASCIIEquals("Message Text", GOVGIOExamples.GetExpectedAirDepotGateInText(), result.Replace("'", "'\n"));
		}

		public void TestCreateSeaDepotGateIn()
		{
			var header = GOVGIOExamples.CreateSeaDepotGateIn(Factory);

			var builder = new GOVGIOMessageTextBuilderForTest(header);
			var result = builder.GenerateMessageBody();
			AssertMultilineASCIIEquals("Message Text", GOVGIOExamples.GetExpectedSeaDepotGateInText(), result.Replace("'", "'\n"));
		}

		public void TestCreateSeaDepotGateOut()
		{
			var header = GOVGIOExamples.CreateSeaDepotGateOut(Factory);

			var builder = new GOVGIOMessageTextBuilderForTest(header);
			var result = builder.GenerateMessageBody();
			AssertMultilineASCIIEquals("Message Text", GOVGIOExamples.GetExpectedSeaDepotGateOutText(), result.Replace("'", "'\n"));
		}

		public void TestCreateSeaDepotConsignmentGateIn()
		{
			var header = GOVGIOExamples.CreateSeaDepotConsignmentGateIn(Factory);

			var builder = new GOVGIOMessageTextBuilderForTest(header);
			var result = builder.GenerateMessageBody();
			AssertMultilineASCIIEquals("Message Text", GOVGIOExamples.GetExpectedSeaDepotConsignmentGateInText(), result.Replace("'", "'\n"));
		}

		public void TestCreateAirTerminalGateIn()
		{
			var header = GOVGIOExamples.CreateAirTerminalGateIn(Factory);

			var builder = new GOVGIOMessageTextBuilderForTest(header);
			var result = builder.GenerateMessageBody();
			AssertMultilineASCIIEquals("Message Text", GOVGIOExamples.GetExpectedAirTerminalGateInText(), result.Replace("'", "'\n"));
		}

		public void TestCreateSeaBreakBulkGateIn()
		{
			var header = GOVGIOExamples.CreateSeaBreakBulkGateIn(Factory);

			var builder = new GOVGIOMessageTextBuilderForTest(header);
			var result = builder.GenerateMessageBody();
			AssertMultilineASCIIEquals("Message Text", GOVGIOExamples.GetExpectedSeaBreakBulkGateInText(), result.Replace("'", "'\n"));
		}

		public void TestCreateSeaTerminalGateIn()
		{
			var header = GOVGIOExamples.CreateSeaTerminalGateIn(Factory);

			var builder = new GOVGIOMessageTextBuilderForTest(header);
			var result = builder.GenerateMessageBody();
			AssertMultilineASCIIEquals("Message Text", GOVGIOExamples.GetExpectedSeaTerminalGateInText(), result.Replace("'", "'\n"));
		}

		public void TestCreateSeaTerminalGateOut()
		{
			var header = GOVGIOExamples.CreateSeaTerminalGateOut(Factory);

			var builder = new GOVGIOMessageTextBuilderForTest(header);
			var result = builder.GenerateMessageBody();
			AssertMultilineASCIIEquals("Message Text", GOVGIOExamples.GetExpectedSeaTerminalGateOutText(), result.Replace("'", "'\n"));
		}

		public void TestBGM_BeginningOfMessage()
		{
			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var codeDescription in new GateInOutMessageTypeCodeList().Cast<ICodeDescription>())
			{
				foreach (var testCase in new[]
				{
					new { SubType = MessageSubTypes.Create, FuntionCode = 9 },
					new { SubType = MessageSubTypes.Change, FuntionCode = 4 },
					new { SubType = MessageSubTypes.Withdraw, FuntionCode = 1 }
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.GateInOutMessageType = codeDescription.Code;
					var builder = new GOVGIOMessageTextBuilderForTest(header);
					var result = builder.GenerateMessageBody(testCase.SubType);
					assertDelegate += () => AssertContains($"BGM+655:::{codeDescription.Code}+<<SYSCAR>>+{testCase.FuntionCode}'", result);
				}
			}
			CombineAssertions(assertDelegate);
		}

		public void TestDOC_DocumentToBeAmended()
		{
			OutturnTestHelper.SetupZZ(Factory);

			VoidParameterlessDelegate assertDelegate = () => { };
			foreach (var codeDescription in new GateInOutMessageTypeCodeList().Cast<ICodeDescription>())
			{
				foreach (var testCase in new[]
				{
					new { SubType = MessageSubTypes.Create, Snippet = "DOC+998+55E8E3293886428DB5DE455857C358F8'", Contains = false },
					new { SubType = MessageSubTypes.Change, Snippet = "DOC+998+55E8E3293886428DB5DE455857C358F8'", Contains = true },
					new { SubType = MessageSubTypes.Withdraw, Snippet = "DOC+998+55E8E3293886428DB5DE455857C358F8'", Contains = true },
				})
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.GateInOutMessageType = codeDescription.Code;
					var govgio = Factory.New<GOVGIOEDIMessage>();
					govgio.EM_MessageNum = "123";
					govgio.EM_MessageText = GOVGIOTestMessage.Replace("\r\n", "");
					govgio.EM_LinkUniqueID = header.PK;
					govgio.EM_LinkTable = header.TablePrefix;
					var cusres = Factory.New<CUSRESEDIMessage>();
					cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					cusres.EM_MessageText = CUSRESTestMessage_GOVGIO.Replace("\r\n", "");
					cusres.EM_LinkUniqueID = header.PK;
					cusres.EM_LinkTable = header.TablePrefix;
					header.Messages.AddRange(govgio, cusres);
					var builder = new GOVGIOMessageTextBuilderForTest(header);
					var result = builder.GenerateMessageBody(testCase.SubType);

					assertDelegate += () => { AssertEquals($"{codeDescription.Code}-{testCase.SubType}", testCase.Contains, result.Contains(testCase.Snippet)); };
				}
			}

			CombineAssertions(assertDelegate);
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

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "11111111", "ZA");
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "22222222", "ZA");
		}
	}

	sealed class GOVGIOMessageTextBuilderForTest
	{
		public GOVGIOMessageTextBuilderForTest(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		public string GenerateMessageBody(MessageSubTypes subType = MessageSubTypes.Create)
		{
			var result = new GOVCBRMessage();
			var textBuilder = new GOVGIOMessageTextBuilder(result, header, subType);
			textBuilder.Create();
			return result.ToString(new BatchProcessor.ZACharacterSetNoCasing());
		}
	}
}
