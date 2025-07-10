using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestExistanceAndValueOfEntryStatusColums()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			var columnsList = new string[] { "EntryStatus", "EntryStatusDescription" };
			using (var form = new ZForm())
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._40;
				var invHeader = declaration.Invoices.AddNew();
				var invLine = invHeader.JobComInvoiceLines.AddNew();
				invLine.JI_CEI = instruction.PK;
				invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + UniversalReferenceConstants.ProcedureCodes._00;
				Factory.Save();
				new LineMerger(declaration).DoMerge();
				Factory.Save();
				var header = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
				userControl.SetDataBinding(declaration, "");
				form.Show();
				attachCUSDEC(header);
				attachCUSRES(header);
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				CombineAssertions("No Values", () =>
				{
					messagesGrid.SelectAllElements();
					var item = messagesGrid.SelectedElements[0] as ZAMessage;
					AssertEquals("Entry Status should be empty", ZString.Empty, item.EntryStatus);
					AssertEquals("Entry Status Description should be empty", ZString.Empty, item.EntryStatusDescription);
					item = messagesGrid.SelectedElements[1] as ZAMessage;
					AssertNotEquals("Entry Status should not be empty", ZString.Empty, item.EntryStatus);
					AssertNotEquals("Entry Status Description should not be empty", ZString.Empty, item.EntryStatusDescription);
				});
			}
		}

		void attachCUSDEC(CusEntryHeader header)
		{
			var testMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = cusdecTestMessage.Replace("\r\n", "");
			testMessage.EM_MessageNum = "IN1";
			header.Messages.Add(testMessage);
		}

		void attachCUSRES(CusEntryHeader header)
		{
			var testMessage = Factory.NewWithValidTestData<CUSRESEDIMessageForTest>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = cusresTestMessage.Replace("\r\n", "");
			testMessage.EM_MessageNum = "IN1";
			header.Messages.Add(testMessage);
		}

		const string cusresTestMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166JSA20160331008480:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++:::'
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+1:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:202'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";
		const string cusdecTestMessage = @"UNH+66+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+01020304JSA20160708000064::00002+9'
CST++A:117:ZZZ'
LOC+14+G5::ZZZ'
LOC+18+JHB0001::ZZZ'
LOC+35+DE::5'
LOC+36+ZA::5'
LOC+96+JSA::ZZZ'
LOC+9+DEBER::5'
GIS+V:134:ZZZ'
FTX+LIN+++1::N'
RFF+AAS:083-00000000'
DTM+137:20160607:102'
RFF+ABI:0123456789'
RFF+ACD:66'
TDT+20'
NAD+IM+00010005++SCOOBY DOO+THE MYSTERY MACHINE'
RFF+VA:4123546789'
NAD+AG+01020304'
NAD+SU+++SCOOBY DOO+THE MYSTERY MACHINE'
NAD+MS+51051342'
NAD+BY+00010005'
UNS+D'
CST+0001+853610007:108:ZZZ+100'
FTX+AAA+++FUSES'
FTX+ACB+++NUIN'
FTX+CCI+++11:40'
LOC+27+DE'
MEA+AAR++KG:100.00'
MEA+AAF++KG:100'
MOA+38:100'
MOA+40:100'
RFF+WE:DBN201501035000012'
TAX+1+VAT:107:ZZZ'
MOA+161:15.40'
UNS+S'
TAX+3+TVD:107:ZZZ'
MOA+161:15.40'
TAX+3+CUS:107:ZZZ'
MOA+161:100'
UNT+41+66'";
	}
}
