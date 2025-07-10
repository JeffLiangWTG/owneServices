using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CUSCARMessageProcessorTest : TestCaseWithFactory
	{
		internal const string TestD16BMessage1Bill2Containers = @"UNH+6292+CUSCAR:D:16A:UN:RCG01'
BGM+85:::{0}+E9FEB87636BC47CBB+{1}'
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
MEA+AAE+AAB+KGM:1'
SGP+MSCU3248888+2'
PCI+24+MANDNPACK2'
UNT+41+6292'
";
		public void TestProcessCUSCARMessageIfHelperIsNull()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var cuscarMessage = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			cuscarMessage.EM_ReceiveTransmit = "RCV";
			cuscarMessage.EM_ApplicationCode = "ZAC";
			cuscarMessage.EM_Status = "QUE";
			var messageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			cuscarMessage.EM_MessageText = string.Format(messageText, "AND", "9");
			cuscarMessage.EM_MessageNum = "IN0";
			cuscarMessage.EM_SystemCreateTimeUtc = new ZDateTime(2018, 2, 14, 1, 5, 0);
			cuscarMessage.EM_LinkedObject = manifestHeader;
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var proc = new CUSCARMessageProcessor(logger);
			AssertNoExceptionThrown(() => proc.ProcessMessage(cuscarMessage));
		}

		public void TestProcessCUSCARMessage2CCreatesEntries()
		{
			var cuscarMessage = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			cuscarMessage.EM_ReceiveTransmit = "RCV";
			cuscarMessage.EM_ApplicationCode = "ZAC";
			cuscarMessage.EM_Status = "QUE";
			var messageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			cuscarMessage.EM_MessageText = string.Format(messageText, "AND", "9");
			cuscarMessage.EM_MessageNum = "IN0";
			cuscarMessage.EM_SystemCreateTimeUtc = new ZDateTime(2018, 2, 14, 1, 5, 0);
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			PreProcessAndProcess(logger, cuscarMessage);
			Factory.Save();
			AssertEquals("PRS", cuscarMessage.EM_Status);
			AssertEquals("AsycudaManifestHeader", cuscarMessage.EM_LinkTable);
			AssertNotNull(cuscarMessage.EM_LinkUniqueID);
			var query = new ZQuery();
			query.AddToFilter(AsycudaBillSchema.ABL_BillNumber, "098475");
			query.AddToFilter(AsycudaBillSchema.ABL_BillIssueDate, new ZDate(2018, 02, 06));
			query.AddToFilter(AsycudaBillSchema.ABL_BolType, "BOL");
			AsycudaBill[] documents = Factory.Load<AsycudaBill>(query);
			AssertEquals(1, documents.Length);
			AsycudaBill document = documents[0];
			AssertEquals(true, document.ABL_IsActive);
			AssertEquals(0, document.ABL_ManifestQty);
			AssertEquals("", document.ABL_ManifestUQ);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, document.CountryCode);
			AsycudaManifestHeader header = document.Header;
			AssertEquals(Core.Constants.AgentType.Agent, header.AMA_AgentType);
			AssertEquals(true, header.AMA_IsActive);
			AssertEquals("OUT", header.AMA_ApplicationCode);
			AssertEquals("OGM0000001", header.AMA_JobReference);
			AssertEquals("S123", header.AMA_Voyage);
			AssertEquals("SEA", header.AMA_TransportMode);
			AssertEquals("3FRF8", header.AMA_RadioCallSign);
			AssertEquals("SONG YUN HE", header.AMA_VesselName);
			AssertEquals("AND", header.AMA_ManifestType);
			AssertEquals(2, header.Containers.Count);
			var container1 = header.Containers[0];
			AssertEquals("MSCU2443581", container1.ACN_ContainerNumber);
			AssertEquals("SEAL1", container1.ACN_Seal1);
			AssertEquals(0m, container1.ACN_GoodsWeight);
			AssertEquals("", container1.ACN_GoodsWeightUQ);
			AssertEquals(1, container1.ACN_NumberOfPackages);
			var container2 = header.Containers[1];
			AssertEquals("MSCU3248888", container2.ACN_ContainerNumber);
			AssertEquals("SEAL2", container2.ACN_Seal1);
			AssertEquals(3m, container2.ACN_GoodsWeight);
			AssertEquals("KGM", container2.ACN_GoodsWeightUQ);
			AssertEquals(1, container2.ACN_NumberOfPackages);
			AssertEquals(1, header.Bills.AsEnumerable().Count());
			var houseBill = header.Bills[0];
			AssertEquals("HWB", houseBill.ABL_BolType);
			AssertEquals(true, houseBill.ABL_IsActive);
			AssertEquals("BILL1", houseBill.ABL_BillNumber);
			AssertEquals("", houseBill.ABL_GoodsDescription);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, houseBill.CountryCode);
			AssertEquals("ZADUR", houseBill.ABL_GoodsLocation);
			AssertEquals(2, houseBill.Packs.Count);
			var pack1 = houseBill.Packs[0];
			AssertEquals("PACKITEM1", pack1.APA_GoodsDescription);
			AssertEquals("MANDNPACK1", pack1.APA_MarksAndNumbers);
			AssertEquals("", pack1.APA_VINNumber);
			AssertEquals(1m, pack1.APA_Weight);
			AssertEquals("KG", pack1.APA_WeightUQ);
			AssertEquals(container1.PK, pack1.ContainerPK);
			var pack2 = houseBill.Packs[1];
			AssertEquals("PACKITEM2", pack2.APA_GoodsDescription);
			AssertEquals("MANDNPACK2", pack2.APA_MarksAndNumbers);
			AssertEquals("", pack2.APA_VINNumber);
			AssertEquals(1m, pack2.APA_Weight);
			AssertEquals("KG", pack2.APA_WeightUQ);
			AssertEquals(container2.PK, pack2.ContainerPK);
		}

		public void TestProcessExistingCUSCARMessage2CUpdatesEntries()
		{
			const string BillNumber = "098475";
			ZDate billIssueDate = new ZDate(2018, 02, 06);
			// create existing entries
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_AgentType = Core.Constants.AgentType.Agent;
			manifestHeader.AMA_IsActive = true;
			manifestHeader.AMA_Voyage = "S123";
			manifestHeader.AMA_RadioCallSign = "2CDEF";
			manifestHeader.AMA_VesselName = "ARGONAUT";
			manifestHeader.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var masterBill = manifestHeader.MasterBill;
			masterBill.ABL_BillNumber = BillNumber;
			masterBill.ABL_BillIssueDate = billIssueDate;
			masterBill.ABL_IsActive = true;
			// containers
			var containerA = manifestHeader.Containers.AddNew();
			containerA.ACN_ContainerNumber = "MSCU2443581";
			containerA.ACN_IsActive = true;
			containerA.ACN_GoodsWeight = 110m;
			containerA.ACN_GoodsWeightUQ = "L";
			containerA.ACN_Seal1 = "Original Seal";
			// bills
			var houseBillA = manifestHeader.Bills.AddNew();
			houseBillA.ABL_BolType = "HWB";
			houseBillA.ABL_BillNumber = "BILL1";
			houseBillA.ABL_IsActive = true;
			houseBillA.ABL_GoodsDescription = "Stuff to be shipped";
			houseBillA.ABL_ManifestQty = 4;
			houseBillA.ABL_ManifestUQ = "BX";
			houseBillA.ABL_GoodsLocation = "ZZZ";
			// packs
			var packA = houseBillA.Packs.AddNew();
			packA.APA_Weight = 99m;
			packA.APA_WeightUQ = "L";
			packA.APA_GoodsDescription = "Cola";
			packA.APA_MarksAndNumbers = "BillAPackA";
			packA.APA_VINNumber = "";
			packA.APA_LineNo = 1;
			packA.ContainerPK = containerA.PK;
			Factory.Save();
			var testMessage0 = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			testMessage0.EM_ReceiveTransmit = "RCV";
			testMessage0.EM_ApplicationCode = "ZAC";
			testMessage0.EM_Status = "QUE";
			testMessage0.EM_MessageNum = "IN0";
			testMessage0.EM_SystemCreateTimeUtc = new ZDateTime(2018, 2, 14, 1, 5, 0);
			var messageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			testMessage0.EM_MessageText = string.Format(messageText, "AND", "4");
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			PreProcessAndProcess(logger, testMessage0);
			Factory.Save();
			AssertEquals("PRS", testMessage0.EM_Status);
			var query = new ZQuery();
			query.AddToFilter(AsycudaBillSchema.ABL_BillNumber, BillNumber);
			query.AddToFilter(AsycudaBillSchema.ABL_BillIssueDate, billIssueDate);
			query.AddToFilter(AsycudaBillSchema.ABL_BolType, "BOL");
			AsycudaBill[] documents = Factory.Load<AsycudaBill>(query);
			AssertEquals(1, documents.Length);
			AsycudaBill document = documents[0];
			AssertEquals(true, document.ABL_IsActive);
			AssertEquals(0, document.ABL_ManifestQty);
			AssertEquals("", document.ABL_ManifestUQ);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, document.CountryCode);
			AsycudaManifestHeader header = document.Header;
			AssertEquals(Core.Constants.AgentType.Agent, header.AMA_AgentType);
			AssertEquals(true, header.AMA_IsActive);
			AssertEquals("OUT", header.AMA_ApplicationCode);
			AssertEquals("OUT", header.AMA_ApplicationCode);
			AssertEquals("OGM0000001", header.AMA_JobReference);
			AssertEquals("S123", header.AMA_Voyage);
			AssertEquals("SEA", header.AMA_TransportMode);
			AssertEquals("3FRF8", header.AMA_RadioCallSign);
			AssertEquals("SONG YUN HE", header.AMA_VesselName);
			AssertEquals("AND", header.AMA_ManifestType);
			AssertEquals(2, header.Containers.Count);
			var container1 = header.Containers[0];
			AssertEquals("MSCU2443581", container1.ACN_ContainerNumber);
			AssertEquals("SEAL1", container1.ACN_Seal1);
			AssertEquals(0m, container1.ACN_GoodsWeight);
			AssertEquals("", container1.ACN_GoodsWeightUQ);
			AssertEquals(1, container1.ACN_NumberOfPackages);
			var container2 = header.Containers[1];
			AssertEquals("MSCU3248888", container2.ACN_ContainerNumber);
			AssertEquals("SEAL2", container2.ACN_Seal1);
			AssertEquals(3m, container2.ACN_GoodsWeight);
			AssertEquals("KGM", container2.ACN_GoodsWeightUQ);
			AssertEquals(1, container2.ACN_NumberOfPackages);
			var validBills = header.Bills.Cast<AsycudaBill>().ToArray();
			AssertEquals(1, validBills.Length);
			var houseBill = validBills[0];
			AssertEquals("HWB", houseBill.ABL_BolType);
			AssertEquals(true, houseBill.ABL_IsActive);
			AssertEquals("BILL1", houseBill.ABL_BillNumber);
			AssertEquals("", houseBill.ABL_GoodsDescription);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, houseBill.CountryCode);
			AssertEquals("ZADUR", houseBill.ABL_GoodsLocation);
			AssertEquals(2, houseBill.Packs.Count);
			var pack1 = houseBill.Packs[0];
			AssertEquals("PACKITEM1", pack1.APA_GoodsDescription);
			AssertEquals("MANDNPACK1", pack1.APA_MarksAndNumbers);
			AssertEquals("", pack1.APA_VINNumber);
			AssertEquals(1m, pack1.APA_Weight);
			AssertEquals("KG", pack1.APA_WeightUQ);
			AssertEquals(container1.PK, pack1.ContainerPK);
			var pack2 = houseBill.Packs[1];
			AssertEquals("PACKITEM2", pack2.APA_GoodsDescription);
			AssertEquals("MANDNPACK2", pack2.APA_MarksAndNumbers);
			AssertEquals("", pack2.APA_VINNumber);
			AssertEquals(1m, pack2.APA_Weight);
			AssertEquals("KG", pack2.APA_WeightUQ);
			AssertEquals(container2.PK, pack2.ContainerPK);
		}

		public void TestProcessExistingCUSCARMessage2CCreatesEntriesNotUpdates()
		{
			// create existing entries
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_AgentType = Core.Constants.AgentType.Agent;
			manifestHeader.AMA_IsActive = true;
			manifestHeader.AMA_TransportMode = "SEA";
			manifestHeader.AMA_Voyage = "S123";
			manifestHeader.AMA_VesselName = "ARGONAUT";
			manifestHeader.AMA_RadioCallSign = "2CDEF";
			manifestHeader.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var masterBill = manifestHeader.MasterBill;
			masterBill.ABL_BillNumber = "098475";
			masterBill.ABL_BillIssueDate = new ZDate(2018, 02, 06);
			masterBill.ABL_IsActive = true;
			// containers
			var containerA = manifestHeader.Containers.AddNew();
			containerA.ACN_ContainerNumber = "MSCU2443581";
			containerA.ACN_IsActive = true;
			containerA.ACN_GoodsWeight = 110m;
			containerA.ACN_GoodsWeightUQ = "KG";
			containerA.ACN_Seal1 = "Original Seal";
			// bills
			var houseBillA = manifestHeader.Bills.AddNew();
			houseBillA.ABL_BolType = "HWB";
			houseBillA.ABL_BillNumber = "BILL1";
			houseBillA.ABL_IsActive = true;
			houseBillA.ABL_GoodsDescription = "Stuff to be shipped";
			houseBillA.ABL_ManifestQty = 4;
			houseBillA.ABL_ManifestUQ = "BX";
			houseBillA.ABL_GoodsLocation = "ZZZ";
			// packs
			var packA = houseBillA.Packs.AddNew();
			packA.APA_Weight = 99m;
			packA.APA_WeightUQ = "KG";
			packA.APA_GoodsDescription = "Cola";
			packA.APA_MarksAndNumbers = "BillAPackA";
			packA.APA_VINNumber = "";
			packA.APA_LineNo = 1;
			packA.ContainerPK = containerA.PK;
			Factory.Save();
			var testMessage0 = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			testMessage0.EM_ReceiveTransmit = "RCV";
			testMessage0.EM_ApplicationCode = "ZAC";
			testMessage0.EM_Status = "QUE";
			testMessage0.EM_MessageNum = "IN0";
			testMessage0.EM_SystemCreateTimeUtc = new ZDateTime(2018, 2, 14, 1, 5, 0);
			var messageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			testMessage0.EM_MessageText = string.Format(messageText, "AND", "9");
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			PreProcessAndProcess(logger, testMessage0);
			Factory.Save();
			AssertEquals("PRS", testMessage0.EM_Status);
			var query = new ZQuery();
			query.AddToFilter(AsycudaBillSchema.ABL_BillNumber, "098475");
			query.AddToFilter(AsycudaBillSchema.ABL_BillIssueDate, new ZDate(2018, 02, 06));
			query.AddToFilter(AsycudaBillSchema.ABL_BolType, "BOL");
			AsycudaBill[] documents = Factory.Load<AsycudaBill>(query);
			AssertEquals(1, documents.Length);
			AsycudaBill document = documents[0];
			AssertEquals(true, document.ABL_IsActive);
			AssertEquals(0, document.ABL_ManifestQty);
			AssertEquals("", document.ABL_ManifestUQ);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, document.CountryCode);
			AsycudaManifestHeader header = document.Header;
			AssertEquals(Core.Constants.AgentType.Agent, header.AMA_AgentType);
			AssertEquals(true, header.AMA_IsActive);
			AssertEquals("OUT", header.AMA_ApplicationCode);
			AssertEquals("OGM0000001", header.AMA_JobReference);
			AssertEquals("S123", header.AMA_Voyage);
			AssertEquals("SEA", header.AMA_TransportMode);
			AssertEquals("2CDEF", header.AMA_RadioCallSign);
			AssertEquals("ARGONAUT", header.AMA_VesselName);
			AssertEquals("COH", header.AMA_ManifestType);
			AssertEquals(2, header.Containers.Count);
			var container1 = header.Containers[0];
			AssertEquals("MSCU2443581", container1.ACN_ContainerNumber);
			AssertEquals("Original Seal", container1.ACN_Seal1);
			AssertEquals(110m, container1.ACN_GoodsWeight);
			AssertEquals("KG", container1.ACN_GoodsWeightUQ);
			AssertEquals(1, container1.ACN_NumberOfPackages);
			var container2 = header.Containers[1];
			AssertEquals("MSCU3248888", container2.ACN_ContainerNumber);
			AssertEquals("SEAL2", container2.ACN_Seal1);
			AssertEquals(3m, container2.ACN_GoodsWeight);
			AssertEquals("KGM", container2.ACN_GoodsWeightUQ);
			AssertEquals(1, container2.ACN_NumberOfPackages);
			AssertEquals(1, header.Bills.AsEnumerable().Count());
			var houseBill = header.Bills[0];
			AssertEquals("HWB", houseBill.ABL_BolType);
			AssertEquals(true, houseBill.ABL_IsActive);
			AssertEquals("BILL1", houseBill.ABL_BillNumber);
			AssertEquals("Stuff to be shipped", houseBill.ABL_GoodsDescription);
			AssertEquals(4, houseBill.ABL_ManifestQty);
			AssertEquals("BX", houseBill.ABL_ManifestUQ);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, houseBill.CountryCode);
			AssertEquals("ZZZ", houseBill.ABL_GoodsLocation);
			AssertEquals("Creates an additional pack", 2, houseBill.Packs.Count);
			var pack1 = houseBill.Packs[0];
			AssertEquals("Leaves existing pack unchanged", "Cola", pack1.APA_GoodsDescription);
			AssertEquals("BillAPackA", pack1.APA_MarksAndNumbers);
			AssertEquals("", pack1.APA_VINNumber);
			AssertEquals(99m, pack1.APA_Weight);
			AssertEquals("KG", pack1.APA_WeightUQ);
			AssertEquals(container1.PK, pack1.ContainerPK);
			var pack2 = houseBill.Packs[1];
			AssertEquals("PACKITEM2", pack2.APA_GoodsDescription);
			AssertEquals("MANDNPACK2", pack2.APA_MarksAndNumbers);
			AssertEquals("", pack2.APA_VINNumber);
			AssertEquals(1m, pack2.APA_Weight);
			AssertEquals("KG", pack2.APA_WeightUQ);
			AssertEquals(container2.PK, pack2.ContainerPK);
		}

		public void TestProcessExistingCUSCARMessageAirUpdatesFlight()
		{
			// create existing entries
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_AgentType = Core.Constants.AgentType.Agent;
			manifestHeader.AMA_IsActive = true;
			manifestHeader.AMA_Voyage = "S001";
			manifestHeader.AMA_TransportMode = "AIR";
			manifestHeader.AMA_RadioCallSign = "3FRF8";
			manifestHeader.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var masterBill = manifestHeader.MasterBill;
			masterBill.ABL_BillNumber = "MSCU09874ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			masterBill.ABL_BillIssueDate = new ZDate(2018, 02, 16);
			masterBill.ABL_IsActive = true;
			masterBill.ABL_ManifestQty = 3;
			masterBill.ABL_ManifestUQ = "BX";
			Factory.Save();
			var testMessage0 = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			testMessage0.EM_ReceiveTransmit = "RCV";
			testMessage0.EM_ApplicationCode = "ZAC";
			testMessage0.EM_Status = "QUE";
			testMessage0.EM_MessageText = @"UNH+6266+CUSCAR:D:16A:UN:RCG001'
BGM+85:::AND+669E4647053C4C18+4'
DTM+136:20180120:102'
DTM+137:20180219:102'
RFF+LO:OGM0000194'
NAD+RL+MSC:172:ZZZ'
NAD+FZ+00101566::ZZZ'
NAD+MS+00505655TST::ZZZ'
TDT+20+S123+4++MSC:172:20+++3FRF8:103:::ZA'
LOC+60+ZADUR:139:6'
DTM+132:20180216:102'
EQD+CN+MSCU1234566+2000:102:5++3+5'
MEA+AAE+VGM+KGM:100'
SEL+SEAL1+TO'
SEL+SEAL2+TO'
CNT+16:1'
CNI+1+MSCU09874ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ:BOL:::20180216'
CNT+16:1'
RFF+BM:HB2'
LOC+8+ZADUR:139:6'
LOC+9+DEHAM:139:6'
NAD+CN++SHELDONS IMPORTER ZA WITH A LONG NA:198 WEST STREET:JOHANNESBURG::1619'
NAD+CZ++TIM EXPORT CO:1 BAYERN STREET:HAMBURG:HH:22765'
GID+1+12:NO'
FTX+AAA+++DESC2'
MEA+AAE+AAW+MTQ:0'
MEA+AAE+AAB+KGM:300'
SGP+MSCU1234566+12'
PCI+24+MAND2'
CST++COMM2'
UNT+31+6266'
".Replace("\r\n", "");
			testMessage0.EM_MessageNum = "IN0";
			testMessage0.EM_SystemCreateTimeUtc = new ZDateTime(2018, 2, 14, 1, 5, 0);
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			PreProcessAndProcess(logger, testMessage0);
			Factory.Save();
			AssertEquals("PRS", testMessage0.EM_Status);
			var query = new ZQuery();
			query.AddToFilter(AsycudaBillSchema.ABL_BillNumber, "MSCU09874ABCDEFGHIJKLMNOPQRSTUVWXYZ");
			query.AddToFilter(AsycudaBillSchema.ABL_BillIssueDate, new ZDate(2018, 02, 16));
			query.AddToFilter(AsycudaBillSchema.ABL_BolType, "BOL");
			AsycudaBill[] documents = Factory.Load<AsycudaBill>(query);
			AssertEquals(1, documents.Length);
			AsycudaBill document = documents[0];
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, document.CountryCode);
			AssertEquals(true, document.ABL_IsActive);
			AssertEquals("Document Number is trimmed to BillNumber length", "MSCU09874ABCDEFGHIJKLMNOPQRSTUVWXYZ", document.ABL_BillNumber);
			AssertEquals(3, document.ABL_ManifestQty);
			AssertEquals("BX", document.ABL_ManifestUQ);
			AsycudaManifestHeader header = document.Header;
			AssertEquals(Core.Constants.AgentType.Agent, header.AMA_AgentType);
			AssertEquals(true, header.AMA_IsActive);
			AssertEquals("OUT", header.AMA_ApplicationCode);
			AssertEquals("OGM0000001", header.AMA_JobReference);
			AssertEquals("AIR", header.AMA_TransportMode);
			AssertEquals("3FRF8", header.AMA_RadioCallSign);
			AssertEquals("S123", header.AMA_Voyage);
			AssertEquals("AND", header.AMA_ManifestType);
			AssertEquals(1, header.Containers.Count);
			var container1 = header.Containers[0];
			AssertEquals("MSCU1234566", container1.ACN_ContainerNumber);
			AssertEquals("SEAL1", container1.ACN_Seal1);
			AssertEquals(100m, container1.ACN_GoodsWeight);
			AssertEquals("KGM", container1.ACN_GoodsWeightUQ);
			AssertEquals(1, container1.ACN_NumberOfPackages);
			AssertEquals(1, header.Bills.AsEnumerable().Count());
			var houseBill = header.Bills[0];
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, houseBill.CountryCode);
			AssertEquals(true, houseBill.ABL_IsActive);
			AssertEquals("HB2", houseBill.ABL_BillNumber);
			AssertEquals("", houseBill.ABL_GoodsDescription);
			AssertEquals(1, houseBill.Packs.Count);
			var pack1 = houseBill.Packs[0];
			AssertEquals("DESC2", pack1.APA_GoodsDescription);
			AssertEquals("MAND2", pack1.APA_MarksAndNumbers);
			AssertEquals("", pack1.APA_VINNumber);
			AssertEquals(300m, pack1.APA_Weight);
			AssertEquals("KG", pack1.APA_WeightUQ);
			AssertEquals(container1.PK, pack1.ContainerPK);
		}

		public void TestInvalidMessageType()
		{
			var cusdecMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			PreProcessAndProcess(logger, cusdecMessage);
			Factory.Save();
			AssertContains("CUSCAR Message Processor cannot process message type CUSDECEDIMessage", logger.LogMessages.ToString());
		}

		public void TestInvalidManifestType()
		{
			var cusdecMessage = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			cusdecMessage.EM_ReceiveTransmit = "RCV";
			cusdecMessage.EM_ApplicationCode = "ZAC";
			cusdecMessage.EM_Status = "QUE";
			cusdecMessage.EM_MessageText = @"UNH+6292+CUSCAR:D:16A:UN:RCG01'
BGM+85:::XXX++9'
TDT+20+S123+1++MSC:172:20+++3FRF8:103:::ZA'
EQD+CN++::+++'
CNI++098475::::20180206'
RFF+BM:123'
GID++:'
UNT+7+6292'
".Replace("\r\n", "");
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			PreProcessAndProcess(logger, cusdecMessage);
			Factory.Save();
			AssertContains("CUSCAR Message Processor cannot process a XXX manifest type", logger.LogMessages.ToString());
		}

		public void TestSentManifest()
		{
			// create existing entries
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_AgentType = Core.Constants.AgentType.Agent;
			manifestHeader.AMA_IsActive = true;
			manifestHeader.AMA_Voyage = "S123";
			manifestHeader.AMA_TransportMode = "AIR";
			manifestHeader.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var masterBill = manifestHeader.MasterBill;
			masterBill.ABL_BillNumber = "098475";
			masterBill.ABL_BillIssueDate = new ZDate(2018, 02, 06);
			masterBill.ABL_IsActive = true;
			var houseBillA = manifestHeader.Bills.AddNew();
			houseBillA.ABL_BolType = "HWB";
			houseBillA.ABL_BillNumber = "BILL1";
			houseBillA.ABL_IsActive = true;
			houseBillA.ABL_MessageStatus = ZAMessageStatusList.Codes.Acknowledged;
			Factory.Save();
			var cusdecMessage = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			cusdecMessage.EM_ReceiveTransmit = "RCV";
			cusdecMessage.EM_ApplicationCode = "ZAC";
			cusdecMessage.EM_Status = "QUE";
			cusdecMessage.EM_MessageText = @"UNH+6292+CUSCAR:D:16A:UN:RCG01'
BGM+85:::AND++4'
TDT+20+S123+1++MSC:172:20+++3FRF8:103:::ZA'
EQD+CN++::+++'
CNI++098475::::20180206'
RFF+BM:BILL1'
GID++:'
UNT+7+6292'
".Replace("\r\n", "");
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			PreProcessAndProcess(logger, cusdecMessage);
			Factory.Save();
			AssertContains("CUSCAR Message Processor cannot update Manifest 098475.  Bill BILL1 on the Manifest has been sent to Customs. Message Status: ACK", logger.LogMessages.ToString());
		}

		public void TestInvalidMessage()
		{
			var manifests = Factory.Load<AsycudaManifestHeader>(ManifestQuery);
			AssertEquals("Precondition", 0, manifests.Length);
			var masterbills = Factory.Load<AsycudaBill>(MasterBillQuery);
			AssertEquals("Precondition", 0, masterbills.Length);
			var cuscarMessage = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			cuscarMessage.EM_ReceiveTransmit = "RCV";
			cuscarMessage.EM_ApplicationCode = "ZAC";
			cuscarMessage.EM_Status = "QUE";
			cuscarMessage.EM_MessageText = @"UNH+6308+CUSCAR:D:16A:UN:RCG001'
DTM+136:20180228:102'
DTM+137:20180302:102'
RFF+LO:MAN0001204'
NAD+RL+057:172:3'
NAD+FZ+20449470::ZZZ'
NAD+MS+00505655TST::ZZZ'
LOC+60+JNB:145:3'
DTM+232:201803020100:203'
GIS+23:71'
CNI+1+057-55222622::20180228'
RFF+BM:057-55655622'
LOC+8+JNB:145:6'
LOC+9+BRU:145:6'
NAD+CN++GLENN CORP:3 NEW ROAD:MIDRAND::1682'
NAD+CZ++TIM EXPORT CO:1 BAYERN STREET:HAMBURG:HH:22765'
NAD+N1++GLENN CORP:3 NEW ROAD:MIDRAND::1682'
GID+1+7:PLT'
FTX+AAA+++MEDICAL'
MEA+AAE+AAW+MTQ:6.018'
MEA+AAE+G+KGM:546'
PCI+24+ILEX SOUTH FRIC'
UNT+25+6308'
".Replace("\r\n", "");
			AssertInvalidMessageIsDiscarded(cuscarMessage, "Message 1", "CUSCAR Message Processor cannot process a message with Invalid or Missing parts: BGM, TDT, CNI, Group 8.");
			var cuscarMessage2 = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			cuscarMessage2.EM_ReceiveTransmit = "RCV";
			cuscarMessage2.EM_ApplicationCode = "ZAC";
			cuscarMessage2.EM_Status = "QUE";
			cuscarMessage2.EM_MessageText = @"UNH+6308+CUSCAR:D:16A:UN:RCG001'
BGM+85:::+E15934515E1444B1B94A8A726D64C9B9+'
DTM+136:20180228:102'
DTM+137:20180302:102'
RFF+LO:MAN0001204'
NAD+RL+057:172:3'
NAD+FZ+20449470::ZZZ'
NAD+MS+00505655TST::ZZZ'
TDT+20++++057:172:3'
LOC+60+JNB:145:3'
DTM+232:201803020100:203'
GIS+23:71'
CNI++098475::::20180206'
RFF+BM:057-55655622'
LOC+8+JNB:145:6'
LOC+9+BRU:145:6'
LOC+104+ZADUR'
LOC+65+DEHAM'
NAD+CN++GLENN CORP:3 NEW ROAD:MIDRAND::1682'
NAD+CZ++TIM EXPORT CO:1 BAYERN STREET:HAMBURG:HH:22765'
NAD+N1++GLENN CORP:3 NEW ROAD:MIDRAND::1682'
GID+1+7:PLT'
FTX+AAA+++MEDICAL'
MEA+AAE+AAW+MTQ:6.018'
MEA+AAE+G+KGM:546'
PCI+24+ILEX SOUTH FRIC'
UNT+25+6308'
".Replace("\r\n", "");
			AssertInvalidMessageIsDiscarded(cuscarMessage2, "Message 2", "CUSCAR Message Processor cannot process a message with Invalid or Missing parts: BGM.DocumentName, BGM.MessageFunctionCode, TDT.TransportModeNameCode, TDT.MeansOfTransportJourneyIdentifier, CNI, Group 8.");
			var cuscarMessage3 = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			cuscarMessage3.EM_ReceiveTransmit = "RCV";
			cuscarMessage3.EM_ApplicationCode = "ZAC";
			cuscarMessage3.EM_Status = "QUE";
			cuscarMessage3.EM_MessageText = @"UNH+6308+CUSCAR:D:16A:UN:RCG001'
BGM+85:::AND+E15934515E1444B1B94A8A726D64C9B9+4'
DTM+136:20180228:102'
DTM+137:20180302:102'
RFF+LO:MAN0001204'
RFF+ACL:S123'
NAD+RL+057:172:3'
NAD+FZ+20449470::ZZZ'
NAD+MS+00505655TST::ZZZ'
TDT+20+AF990+4++057:172:3'
LOC+60+JNB:145:3'
DTM+232:201803020100:203'
GEI+5+16:176:ZZZ'
CNT+16:2'
CNI++::::'
CNT+16:2'
RFF+BM:'
LOC+104+'
LOC+65+'
UNT+41+6292'
".Replace("\r\n", "");
			AssertInvalidMessageIsDiscarded(cuscarMessage3, "Message 3", "CUSCAR Message Processor cannot process a message with Invalid or Missing parts: CNI.DocumentIdentifier, CNI.VersionIdentifier, RFF.ReferenceIdentifier, Group 14 in Bill ''.");
		}

		public void TestPreProcessMessage_Matched()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			header.AMA_JobReference = "MAN0001000";
			header.AMA_ApplicationCode = AsycudaManifestHeader.ApplicationCode_Out;
			header.AMA_Voyage = "S123";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "098475";
			bill.ABL_BillIssueDate = new ZDate(2018, 02, 06);
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			var testMessage = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "IN0";
			testMessage.EM_SystemCreateTimeUtc = new ZDateTime(2018, 2, 14, 1, 5, 0);
			var messageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			testMessage.EM_MessageText = string.Format(messageText, "AND", "4");
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			new CUSCARMessageProcessor(logger).PreProcessMessage(testMessage);
			Factory.Save();
			header.Reload();
			testMessage.Reload();
			AssertEquals(1, header.Messages.Count);
			AssertEquals(testMessage.PK, header.Messages[0].PK);
			AssertEquals(header.PK, testMessage.EM_LinkedObject.PK);
			AssertEquals("PPS", testMessage.EM_Status);
		}

		public void TestPreProcessMessage_NoMatch()
		{
			var testMessage = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "IN0";
			testMessage.EM_SystemCreateTimeUtc = new ZDateTime(2018, 2, 14, 1, 5, 0);
			var messageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			testMessage.EM_MessageText = string.Format(messageText, "AND", "4");
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			new CUSCARMessageProcessor(logger).PreProcessMessage(testMessage);
			Factory.Save();
			testMessage.Reload();
			var header = testMessage.EM_LinkedObject as AsycudaManifestHeader;
			AssertNull(header);
			AssertEquals("PPS", testMessage.EM_Status);
		}

		public void TestPreProcessingRequired()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var processor = new CUSCARMessageProcessor(logger);
			AssertEquals(true, processor.RequiresPreProcessing);
		}

		public void TestProcessBatch()
		{
			var cuscarMessage = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			cuscarMessage.EM_ReceiveTransmit = "RCV";
			cuscarMessage.EM_ApplicationCode = "ZAC";
			cuscarMessage.EM_Status = "QUE";
			cuscarMessage.EM_LinkTable = ZString.Empty;
			cuscarMessage.EM_LinkUniqueID = ZGuid.Empty;
			var messageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			cuscarMessage.EM_MessageText = string.Format(messageText, "AND", "9");
			cuscarMessage.EM_MessageNum = "IN0";
			cuscarMessage.EM_SystemCreateTimeUtc = new ZDateTime(2018, 2, 14, 1, 5, 0);
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var proc = new ZACIncomingMessageProcessor(logger);
			proc.ExecuteBatch();
			cuscarMessage.Reload();
			AssertEquals("PRS", cuscarMessage.EM_Status);
			AssertEquals("AsycudaManifestHeader", cuscarMessage.EM_LinkTable);
			AssertNotNull(cuscarMessage.EM_LinkUniqueID);
		}

		public void TestProcessBatch_NoMatch()
		{
			var cuscarMessage = Factory.NewWithValidTestData<CUSCAREDIMessage>();
			cuscarMessage.EM_ReceiveTransmit = "RCV";
			cuscarMessage.EM_ApplicationCode = "ZAC";
			cuscarMessage.EM_Status = "QUE";
			cuscarMessage.EM_LinkTable = ZString.Empty;
			cuscarMessage.EM_LinkUniqueID = ZGuid.Empty;
			var messageText = TestD16BMessage1Bill2Containers.Replace("\r\n", "");
			cuscarMessage.EM_MessageText = string.Format(messageText, "AND", "4");
			cuscarMessage.EM_SystemCreateTimeUtc = new ZDateTime(2018, 2, 14, 1, 5, 0);
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var proc = new ZACIncomingMessageProcessor(logger);
			proc.ExecuteBatch();
			cuscarMessage.Reload();
			AssertEquals("PRS", cuscarMessage.EM_Status);
			AssertEquals("AsycudaManifestHeader", cuscarMessage.EM_LinkTable);
			AssertNotNull(cuscarMessage.EM_LinkUniqueID);
		}

		void AssertInvalidMessageIsDiscarded(CUSCAREDIMessage cuscarMessage, string message, string expectedErrorText)
		{
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			PreProcessAndProcess(logger, cuscarMessage);
			Factory.Save();
			var manifests = Factory.Load<AsycudaManifestHeader>(ManifestQuery);
			AssertEquals(message + "Manifest not created", 0, manifests.Length);
			var masterbills = Factory.Load<AsycudaBill>(MasterBillQuery);
			AssertEquals(message + "Master Bill not created", 0, masterbills.Length);
			AssertEquals(message, ZAMessage.Status.Discarded, cuscarMessage.EM_Status);
			AssertContains(message, expectedErrorText, logger.LogMessages.ToString());
		}

		void PreProcessAndProcess(LoggingInformation logger, EDIMessage cuscarMessage)
		{
			var proc = new CUSCARMessageProcessor(logger);
			proc.PreProcessMessage(cuscarMessage);

			if (cuscarMessage.EM_Status.EqualsIgnoringCase(EDIMessage.Status.PreProcessedOK))
			{
				proc.ProcessMessage(cuscarMessage);
			}
		}

		ZQuery ManifestQuery => manifestQuery ??= new ZQuery(AsycudaManifestHeaderSchema.AMA_ApplicationCode, AsycudaManifestHeader.ApplicationCode_Out);
		ZQuery manifestQuery;
		ZQuery MasterBillQuery => masterbillQuery ??= new ZQuery(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
		ZQuery masterbillQuery;
	}
}
