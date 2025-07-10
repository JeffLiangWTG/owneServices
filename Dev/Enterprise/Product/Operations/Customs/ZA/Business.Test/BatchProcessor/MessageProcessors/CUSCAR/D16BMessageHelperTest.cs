using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Edifact.D16A.Elements;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageProcessor.Testing
{
	[TestedType(typeof(D16AMessageHelper))]
	sealed class D16BMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageSender_MasterTransportDocumentNumber()
		{
			var message = Factory.New<CUSCAREDIMessage>();
			message.EM_MessageText = @"UNH+360+CUSCAR:D:16A:UN:RCG001'BGM+85:::ALM+4702A6AD067441149AAF7809692B4C2C+9'RFF+LO:MAN0000014'NAD+MS+12342342'NAD+DEG'TDT+20++++:172:20'LOC+60'CNI+1+1234345:BOL:123434'RFF+BM:11111'LOC+8'LOC+9'GID+1+0'FTX+AAA++9'MEA+AAE+AAB+KGM:0'PCI+24'UNT+16+360'";
			AssertEquals("12342342", message.CUSCARD16AHelper.MessageSender);
			AssertEquals("1234345", message.CUSCARD16AHelper.MasterTransportDocumentNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<ZAMessage>();
			testMessage.EM_MessageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			return D16AMessageHelper.New(testMessage);
		}

		const string TestD16BMessage1Bill2Containers = @"UNH+6292+CUSCAR:D:16A:UN:RCG01'
BGM+85:::AND+E9FEB87636BC47CBB+9'
DTM+137:20180226:102'
DTM+136:20180206:102'
RFF+LO:OGM0000202'
RFF+ACL:S123'
NAD+RL+MSC'
NAD+FZ+00101566'
NAD+MS+00505655TST'
TDT+20+S123+1++MSC:172:20+++3FRF8:103:::ZA'
LOC+60+ZADUR'
DTM+132:20180223:102'
GEI+5+16:176:ZZZ'
EQD+CN+MSCU2443581+2000:102:5++3+7'
SEL+SEAL1+CU'
EQD+CN+MSCU3248888+2000:102:5++3+7'
MEA+AAE+VGM+KGM:3'
SEL+SEAL2+CU'
CNT+16:2'
CNI++098475::::20180206'
CNT+16:2'
RFF+BM:BILL1'
LOC+8+ZADUR'
LOC+9+DEHAM'
LOC+104+ZADUR'
LOC+65+DEHAM'
NAD+CN++GLENN CORP:3 NEW ROAD:MIDRAND::1682++3 NEW ROAD:ERAND AH:MIDRAND:1682  ZA'
NAD+CZ++TIM EXPORT CO:1 BAYERN STREET:HAMBURG:HH:22765++1 BAYERN STREET:HAMBURG:HAMBURG:22765 HAMBURG (HANSESTADT) DE'
GID+1+1:KG'
FTX+AAA+++PACKITEM1'
MEA+AAE+AAW+MTQ:1'
MEA+AAE+AAB+KGM:1'
SGP+MSCU2443581+1'
PCI+24+MANDNPACK1'
GID+2+2:KG'
FTX+AAA+++PACKITEM2'
MEA+AAE+AAW+MTQ:2'
MEA+AAE+AAB+KGM:3'
SGP+MSCU3248888+2'
PCI+24+MANDN:PACK:2'
UNT+41+6292'
";
		public void TestUniqueReferenceNumber()
		{
			var testMessage = Factory.NewWithValidTestData<ZAMessage>();
			testMessage.EM_MessageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			var testHelper = D16AMessageHelper.New(testMessage);
			AssertEquals("E9FEB87636BC47CBB", testHelper.UniqueReferenceNumber);
		}

		public void TestValidMessage()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			var testHelper = D16AMessageHelper.New(testMessage);
			AssertNotNull("Helper created", testHelper);
			CombineAssertions("Message Body", () =>
			{
				AssertEquals("DocumentNumber", "098475", testHelper.DocumentNumber);
				AssertEquals("DocumentDate", new ZDateTime(2018, 02, 06), testHelper.DocumentDate);
				AssertEquals("ManifestType", "AND", testHelper.ManifestType);
				AssertEquals("MessageFunction", MessageFunctionCodeList.Original, testHelper.MessageFunction);
				AssertEquals("MessageType", MessageFunctionCodeList.Original.ToString(), testHelper.MessageType);
				AssertEquals("MessageFunctionDescription", "Original", testHelper.MessageFunctionDescription);
				AssertEquals("VoyageFlightNo", "S123", testHelper.VoyageFlightNo);
				AssertEquals("CarrierCode", "MSC", testHelper.CarrierCode);
				AssertEquals("TransportCode", "1", testHelper.TransportCode);
				AssertEquals("RadioCallSign", "3FRF8", testHelper.RadioCallSign);
			});
			var containers = testHelper.GetContainers();
			AssertNotNull("Has Containers collection", containers);
			var containerDetail = containers[0];
			AssertNotNull("Has Container", containerDetail);
			CombineAssertions("Container", () =>
			{
				AssertEquals("SealNumber", "SEAL1", containerDetail.SealNumber);
				AssertEquals("ContainerNumber", "MSCU2443581", containerDetail.ContainerNumber);
				AssertEquals("GoodsWeight", "", containerDetail.GoodsWeight);
				AssertEquals("GoodsWeightUQ", "", containerDetail.GoodsWeightUQ);
				AssertEquals("Container Count", 2, containers.Length);
				containerDetail = containers[1];
				AssertEquals("SealNumber", "SEAL2", containerDetail.SealNumber);
				AssertEquals("ContainerNumber", "MSCU3248888", containerDetail.ContainerNumber);
				AssertEquals("GoodsWeight", "3", containerDetail.GoodsWeight);
				AssertEquals("GoodsWeightUQ", "KGM", containerDetail.GoodsWeightUQ);
			});
			var bills = testHelper.GetBills();
			AssertNotNull("Has Bills collection", bills);
			var bill = bills[0];
			AssertNotNull("Has Bill", bill);
			CombineAssertions("Bill", () =>
			{
				AssertEquals("BillNumber", "BILL1", bill.BillNumber);
				AssertEquals("TerminalOfDischarge", "DEHAM", bill.TerminalOfDischarge);
				AssertEquals("PlaceOfDeconsolidation", "ZADUR", bill.PlaceOfDeconsolidation);
				AssertEquals("TotalNumberOfPackages", 3, bill.TotalNumberOfPackages);
				AssertEquals("TypeOfPackages", "KG", bill.TypeOfPackages);
				AssertEquals("BillGoodsDescription", "", bill.BillGoodsDescription);
			});
			var packs = bill.GetPackLines();
			AssertNotNull("Has Packs collection", packs);
			var pack = packs[0];
			AssertNotNull("Has Pack", pack);
			CombineAssertions("Pack", () =>
			{
				AssertEquals("GoodsLineNumber", 1, pack.GoodsLineNumber);
				AssertEquals("NumberOfPackages", "1", pack.NumberOfPackages);
				AssertEquals("TypeOfPackages", "KG", pack.TypeOfPackages);
				AssertEquals("ContainerPackageLink", "MSCU2443581", pack.ContainerPackageLink);
				AssertEquals("GoodsDescription", "PACKITEM1", pack.GoodsDescription);
				AssertEquals("MarksAndNumbers", "MANDNPACK1", pack.MarksAndNumbers);
				AssertEquals("PackWeight", "1", pack.PackWeight);
				AssertEquals("PackWeightUQ", "KGM", pack.PackWeightUQ);
				AssertEquals("Pack Count", 2, packs.Length);
				pack = packs[1];
				AssertEquals("GoodsLineNumber", 2, pack.GoodsLineNumber);
				AssertEquals("NumberOfPackages", "2", pack.NumberOfPackages);
				AssertEquals("TypeOfPackages", "KG", pack.TypeOfPackages);
				AssertEquals("ContainerPackageLink", "MSCU3248888", pack.ContainerPackageLink);
				AssertEquals("GoodsDescription", "PACKITEM2", pack.GoodsDescription);
				AssertEquals("MarksAndNumbers", "MANDNPACK2", pack.MarksAndNumbers);
				AssertEquals("PackWeight", "3", pack.PackWeight);
				AssertEquals("PackWeightUQ", "KGM", pack.PackWeightUQ);
			});
		}

		const string TestD16BMessageEmptyComplete = @"UNH+6292+CUSCAR:D:16A:UN:RCG01'
BGM+85:::++'
DTM+137::'
TDT+20++++::+++::::'
EQD+CN++::+++'
MEA+AAE+VGM+:'
SEL++CU'
CNI++::::'
RFF+BM:'
LOC+104+'
LOC+65+'
GID++:'
FTX+AAA+++'
MEA+AAE+AAB+:'
SGP++'
PCI+24+'
UNT+17+6292'
";
		public void TestEmptyCompleteMessage()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = TestD16BMessageEmptyComplete.Replace("\r\n", "");
			var testHelper = D16AMessageHelper.New(testMessage);
			AssertNotNull("Helper created", testHelper);
			AssertEmptyMessageBody(testHelper);
			var containers = testHelper.GetContainers();
			AssertNotNull("Has Containers collection", containers);
			var container = containers[0];
			AssertNotNull("Has Container", container);
			AssertEmptyContainer(container);
			var bills = testHelper.GetBills();
			AssertNotNull("Has Bills collection", bills);
			var bill = bills[0];
			AssertNotNull("Has Bill", bill);
			AssertEmptyBill(bill);
			var packs = bill.GetPackLines();
			AssertNotNull("Has Packs collection", packs);
			var pack = packs[0];
			AssertNotNull("Has Pack", pack);
			AssertEmptyPack(pack);
			AssertEquals("validation", "BGM.DocumentName, BGM.MessageFunctionCode, TDT.TransportModeNameCode, TDT.MeansOfTransportJourneyIdentifier, CNI.DocumentIdentifier, CNI.VersionIdentifier, RFF.ReferenceIdentifier.", testHelper.ValidateMessage());
		}

		const string TestD16BMessageEmptyBasic = @"UNH+6292+CUSCAR:D:16A:UN:RCG01'
BGM+85:::++'
EQD+CN++::+++'
CNI++::::'
RFF+BM:'
GID++:'
UNT+7+6292'
";
		public void TestEmptyBasicMessage()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = TestD16BMessageEmptyBasic.Replace("\r\n", "");
			var testHelper = D16AMessageHelper.New(testMessage);
			AssertNotNull("Helper created", testHelper);
			AssertEmptyMessageBody(testHelper);
			var containers = testHelper.GetContainers();
			AssertNotNull("Has Containers collection", containers);
			var container = containers[0];
			AssertNotNull("Has Container", container);
			AssertEmptyContainer(container);
			var bills = testHelper.GetBills();
			AssertNotNull("Has Bills collection", bills);
			var bill = bills[0];
			AssertNotNull("Has Bill", bill);
			AssertEmptyBill(bill);
			var packs = bill.GetPackLines();
			AssertNotNull("Has Packs collection", packs);
			var pack = packs[0];
			AssertNotNull("Has Pack", pack);
			AssertEmptyPack(pack);
			AssertEquals("validation", "BGM.DocumentName, BGM.MessageFunctionCode, TDT, CNI.DocumentIdentifier, CNI.VersionIdentifier, RFF.ReferenceIdentifier.", testHelper.ValidateMessage());
		}

		const string TestD16BMessageEmptyInvalid = @"UNH+6292+CUSCAR:D:16A:UN:RCG01'
BGM+85:::++'
UNT+3+6292'
";
		public void TestEmptyInvalidMessage()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = TestD16BMessageEmptyInvalid.Replace("\r\n", "");
			var testHelper = D16AMessageHelper.New(testMessage);
			AssertNotNull("Helper created", testHelper);
			AssertEmptyMessageBody(testHelper);
			var containers = testHelper.GetContainers();
			AssertNotNull("Has Containers collection", containers);
			AssertEquals("Containers Count", 0, containers.Length);
			var bills = testHelper.GetBills();
			AssertNotNull("Has Bills collection", bills);
			AssertEquals("Bills Count", 0, bills.Length);
			AssertEquals("validation", "BGM.DocumentName, BGM.MessageFunctionCode, TDT, CNI, Group 8.", testHelper.ValidateMessage());
		}

		void AssertEmptyMessageBody(D16AMessageHelper testHelper)
		{
			CombineAssertions("Message Body", () =>
			{
				AssertEquals("DocumentNumber", "", testHelper.DocumentNumber);
				AssertEquals("DocumentDate", ZDate.Empty, testHelper.DocumentDate);
				AssertEquals("ManifestType", "", testHelper.ManifestType);
				AssertEquals("MessageFunction", "", testHelper.MessageFunction.ToString());
				AssertEquals("MessageType", "", testHelper.MessageType);
				AssertEquals("MessageFunctionDescription", "", testHelper.MessageFunctionDescription);
				AssertEquals("VoyageFlightNo", "", testHelper.VoyageFlightNo);
				AssertEquals("CarrierCode", "", testHelper.CarrierCode);
				AssertEquals("TransportCode", "", testHelper.TransportCode);
				AssertEquals("RadioCallSign", "", testHelper.RadioCallSign);
			});
		}

		void AssertEmptyContainer(D16AMessageContainerDetail container)
		{
			CombineAssertions("Container", () =>
			{
				AssertEquals("SealNumber", "", container.SealNumber);
				AssertEquals("ContainerNumber", "", container.ContainerNumber);
				AssertEquals("GoodsWeight", "", container.GoodsWeight);
				AssertEquals("GoodsWeightUQ", "", container.GoodsWeightUQ);
			});
		}

		void AssertEmptyBill(D16AMessageHouseBillDetail bill)
		{
			CombineAssertions("Bill", () =>
			{
				AssertEquals("BillNumber", "", bill.BillNumber);
				AssertEquals("TerminalOfDischarge", "", bill.TerminalOfDischarge);
				AssertEquals("PlaceOfDeconsolidation", "", bill.PlaceOfDeconsolidation);
				AssertEquals("TotalNumberOfPackages", 0, bill.TotalNumberOfPackages);
				AssertEquals("TypeOfPackages", "", bill.TypeOfPackages);
				AssertEquals("BillGoodsDescription", "", bill.BillGoodsDescription);
			});
		}

		void AssertEmptyPack(D16AMessagePackLineDetail pack)
		{
			CombineAssertions("Pack", () =>
			{
				AssertEquals("GoodsLineNumber", 0, pack.GoodsLineNumber);
				AssertEquals("NumberOfPackages", "", pack.NumberOfPackages);
				AssertEquals("TypeOfPackages", "", pack.TypeOfPackages);
				AssertEquals("ContainerPackageLink", "", pack.ContainerPackageLink);
				AssertEquals("GoodsDescription", "", pack.GoodsDescription);
				AssertEquals("MarksAndNumbers", "", pack.MarksAndNumbers);
				AssertEquals("PackWeight", "", pack.PackWeight);
				AssertEquals("PackWeightUQ", "", pack.PackWeightUQ);
			});
		}
	}
}
