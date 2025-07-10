using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.OutwardReport.Testing
{
	using Enterprise.Customs.Common.NZ;
	using Enterprise.Customs.NZ.Business.Express.Testing;
	using Enterprise.Messaging.Business;
	using NUnit.Framework;

	public class MessageBuilderTest : MessageBuilders.Testing.MessageBuilderTest
	{
		public void Test2LegOutwardReport()
		{
			SetUpValidSEAConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40009914H");
			shippingLine.OH_FullName = "SUNSHINE CRUISE";
			consol.SetDefaultShippingLineAddress(shippingLine);

			consol.JK_RL_NKLoadPort = "NZCHC";
			consol.JK_RL_NKDischargePort = "USLAX";

			Transport domesticPreCarriageLeg = transport;
			domesticPreCarriageLeg.JW_Vessel = "WRONG VESSEL";
			domesticPreCarriageLeg.JW_VoyageFlight = "V333";
			domesticPreCarriageLeg.JW_RL_NKLoadPort = "NZCHC";
			domesticPreCarriageLeg.JW_RL_NKDiscPort = "NZWLG";
			domesticPreCarriageLeg.IsDomestic = true;
			domesticPreCarriageLeg.JW_ETD = new ZDateTime(2003, 10, 12);

			Transport internationalMainLeg = consol.Transports.AddNew();
			internationalMainLeg.JW_Vessel = "TESTVESL";
			internationalMainLeg.JW_VoyageFlight = "V12345O";
			internationalMainLeg.JW_RL_NKLoadPort = "NZWLG";
			internationalMainLeg.JW_RL_NKDiscPort = "USLAX";
			internationalMainLeg.JW_ETD = new ZDateTime(2003, 10, 14);
			internationalMainLeg.CarrierPK = shippingLine.PK;

			shipment.CustomsEntryNumber = "89012345";
			shipment.JS_HouseBill = "SOC76543WLG";
			shipment.JS_UniqueConsignRef = "SA0001000";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.CustomsEntryNumberType = "ENT";
			shipment2.CustomsEntryNumber = "23456789";
			shipment2.JS_HouseBill = "SOC00009876";
			shipment2.JS_UniqueConsignRef = "SA0001001";

			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.JC_ContainerNum = "CRUX12345";

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2.JC_ContainerNum = "CRUX12346";

			CommonContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container3.JC_ContainerNum = "CRUX12347";
			container3.JC_IsEmptyContainer = true;

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertMultilineEquals("Sea ORN report", ExpectedOriginalForSea.Replace("\r\n", ""), generatedMessage, '\'');
		}

		public void TestOutwardReportWithExpressClearances()
		{
			CusMAWB mawb = Factory.New<CusMAWB>();
			TestCusMAWBCreator testMAWBCreator = new TestCusMAWBCreator(mawb, "08122222222", "QF344", "NZAKL", "USLAX", new ZDateTime(2007, 8, 21), new ZDateTime(2007, 8, 22));
			mawb.CM_MessageReference = "X00003434";
			mawb.ECINumber = "89898989";

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00004444";
			TestJobConsolCreator testConsolCreator = new TestJobConsolCreator(consol, "08122222222", "QF344", "NZAKL", "USLAX", new ZDateTime(2007, 8, 21), new ZDateTime(2007, 8, 22), testMAWBCreator.Carrier);

			ForwardingShipment shipment0 = testConsolCreator.AddShipment(0, "NZCHC", "USDNQ", testMAWBCreator.Supplier2.PK, testMAWBCreator.Importer2.PK);
			shipment0.CustomsEntryNumberType = "CUS";
			shipment0.CustomsEntryNumber = "11111111";

			ForwardingShipment shipment1 = testConsolCreator.AddShipment(1, "NZAKL", "USLAX", testMAWBCreator.Supplier1.PK, testMAWBCreator.Importer1.PK);
			JobDeclaration declarationInOtherCountry = Factory.New<JobDeclaration>();
			declarationInOtherCountry.JE_GB = ZGuid.NewZGuid();
			declarationInOtherCountry.JE_JS = shipment1.PK;

			ForwardingShipment shipmentWithDeclaration = testConsolCreator.AddShipment(6, "NZDUD", "USDFW", "C", "B");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipmentWithDeclaration.PK;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			declaration.DeclarationNumber = "33333333";

			CusHAWB hawb1 = testMAWBCreator.AddHAWB(testMAWBCreator.Supplier1, testMAWBCreator.Importer1, "HOUSEBILLX1", "NZAKL", "USLAX", "GOODS 0", 0.0m, 0, 00m, removeImporterAndSupplierGuids: true);
			hawb1.CS_JS = shipment1.PK;
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			CusHAWB hawb2 = testMAWBCreator.AddHAWB(testMAWBCreator.Supplier2, testMAWBCreator.Importer1, "HOUSEBILLX2", "NZAKL", "USSFO", "GOODS 1", 0.1m, 1, 10m, removeImporterAndSupplierGuids: true);
			hawb2.CS_JS = shipment1.PK;
			hawb2.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;

			NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			TestMessageBuilderResultAgainstString(ExpectedOutwardReportWithExpressClearances);
		}
		#region ExpectedOutwardReportWithExpressClearances
		const string ExpectedOutwardReportWithExpressClearances = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03A:UN
BGM+833:::DEPART+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CS+00009917B:ZZZ:143
NAD+CH++TEST CARRIER
TDT+20++4+++++:::QF344
LOC+5+NZAKL
LOC+8+US
DTM+136:20070821:102
CNT+2:4
CNI+1+11111111
RFF+HWB:H0
CNI+2+89898989
RFF+HWB:HOUSEBILLX1
CNI+3+89898989
RFF+HWB:HOUSEBILLX2
CNI+4+33333333
RFF+HWB:H6
UNT+18+<<MSGNO PLACEHOLDER>>
";
		#endregion

		[ExpectNoExceptions()]
		public void TestEmptyFreightConsolThrowsNoExceptions()
		{
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			messageBuilder.GetMessageText();
		}

		public void TestPopulateMessages()
		{
			NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			SetUpValidAIRConsol();

			transport.JW_VoyageFlight = "QF2";
			transport.JW_RL_NKLoadPort = "NZWLG";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2003, 10, 14);
			shippingLine.OH_FullName = "Qantas";

			shipment.CustomsEntryNumber = "89012345";
			shipment.JS_HouseBill = "SOC76543WLG";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.CustomsEntryNumber = "23456789";
			shipment2.JS_HouseBill = "SOC00009876";

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			messageBuilder.PopulateMessages();
			AssertEquals("MessageBuilder.MessageBusinessObject.IsInDatabase", true, messageBuilder.MessageBusinessObject.IsInDatabase);

			ZQuery filter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, consol.PK);
			filter.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, OutwardReportMessage.MessageTypes.OutwardReport.MessageType);

			var oRMessage = Factory.LoadTop1<EDIMessage>(filter);
			AssertNotNull("EDIMessage is generated", oRMessage);
			AssertEquals("The message is in the database", true, oRMessage.IsInDatabase);
			oRMessage.Factory.Save();
			AssertEquals("EM_MessageText is populated", false, oRMessage.EM_MessageText.IsEmpty);
			AssertNotNull("Message log is there for declaration original", oRMessage.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeclarationQueued.Code)));
			AssertEquals("EM_TestMessage Incorrectly Set", false, oRMessage.EM_IsTestMessage);
			Assert("Sender reference number is replaced with Consol ID", oRMessage.EM_MessageText.IndexOf("BGM+833:::DEPART+" + consol.JK_UniqueConsignRef + "+9") > 0);
			Assert("Message Number is replaced", oRMessage.EM_MessageText.IndexOf("<<MSGNO PLACEHOLDER>>") < 0);

			consol.CusEntryNums.Load();
			AssertEquals("One entrynum only", 1, consol.CusEntryNums.Count);
			Common.CusEntryNumber entryNum = consol.CusEntryNums[0];
			AssertNotNull("Entry num is generated to store status", entryNum);
			AssertEquals("Status is set", OutwardReportStatusList.Codes.AwaitingResponse, entryNum.CE_EntryStatus);
		}

		public override void TestGenerateTestMessage()
		{
			NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			SetUpValidAIRConsol();

			transport.JW_VoyageFlight = "QF2";
			transport.JW_RL_NKLoadPort = "NZWLG";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2003, 10, 14);
			shippingLine.OH_FullName = "Qantas";

			shipment.CustomsEntryNumber = "89012345";
			shipment.JS_HouseBill = "SOC76543WLG";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.CustomsEntryNumber = "23456789";
			shipment2.JS_HouseBill = "SOC00009876";

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			messageBuilder.GenerateMessage();
			AssertEquals("MessageBuilder.MessageBusinessObject.IsInDatabase", false, messageBuilder.MessageBusinessObject.IsInDatabase);
			messageBuilder.MessageBusinessObject.Factory.Save();

			ZQuery filter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, consol.PK);
			filter.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, OutwardReportMessage.MessageTypes.OutwardReport.MessageType);

			var oRMessage = Factory.LoadTop1<EDIMessage>(filter);
			AssertNotNull("EDIMessage is generated", oRMessage);
			AssertEquals("The message is in the database", true, oRMessage.IsInDatabase);
			AssertEquals("EM_MessageText is populated", false, oRMessage.EM_MessageText.IsEmpty);
			AssertNotNull("Message log is there for declaration original", oRMessage.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeclarationQueued.Code)));
			AssertEquals("EM_TestMessage Incorrectly Set", true, oRMessage.EM_IsTestMessage);
			Assert("Sender reference number is replaced with Consol ID", oRMessage.EM_MessageText.IndexOf("BGM+833:::DEPART+" + consol.JK_UniqueConsignRef + "+9") > 0);
			Assert("Message Number is replaced", oRMessage.EM_MessageText.IndexOf("<<MSGNO PLACEHOLDER>>") < 0);

			consol.CusEntryNums.Load();
			AssertEquals("One entrynum only", 1, consol.CusEntryNums.Count);
			Common.CusEntryNumber entryNum = consol.CusEntryNums[0];
			AssertNotNull("Entry num is generated to store status", entryNum);
			AssertEquals("Status is set", OutwardReportStatusList.Codes.AwaitingResponse, entryNum.CE_EntryStatus);
		}

		public override void TestGenerateLiveMessage()
		{
			NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			SetUpValidAIRConsol();

			transport.JW_VoyageFlight = "QF2";
			transport.JW_RL_NKLoadPort = "NZWLG";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2003, 10, 14);
			shippingLine.OH_FullName = "Qantas";

			shipment.CustomsEntryNumber = "89012345";
			shipment.JS_HouseBill = "SOC76543WLG";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.CustomsEntryNumber = "23456789";
			shipment2.JS_HouseBill = "SOC00009876";

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			messageBuilder.GenerateMessage();
			AssertEquals("messageBuilder.MessageBusinessObject.IsInDatabase", false, messageBuilder.MessageBusinessObject.IsInDatabase);
			messageBuilder.MessageBusinessObject.Factory.Save();

			ZQuery filter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, consol.PK);
			filter.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, OutwardReportMessage.MessageTypes.OutwardReport.MessageType);

			var oRMessage = Factory.LoadTop1<EDIMessage>(filter);
			AssertNotNull("EDIMessage is generated", oRMessage);
			AssertEquals("The message is in the database", true, oRMessage.IsInDatabase);
			oRMessage.Factory.Save();
			AssertEquals("EM_MessageText is populated", false, oRMessage.EM_MessageText.IsEmpty);
			AssertNotNull("Message log is there for declaration original", oRMessage.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeclarationQueued.Code)));
			AssertEquals("EM_TestMessage Incorrectly Set", false, oRMessage.EM_IsTestMessage);
			Assert("Sender reference number is replaced with Consol ID", oRMessage.EM_MessageText.IndexOf("BGM+833:::DEPART+" + consol.JK_UniqueConsignRef + "+9") > 0);
			Assert("Message Number is replaced", oRMessage.EM_MessageText.IndexOf("<<MSGNO PLACEHOLDER>>") < 0);
			AssertEquals("EM_MessageType should be set", Declaration.NZCMessage.MessageTypes.OutwardReport.MessageType, oRMessage.EM_MessageType);

			consol.CusEntryNums.Load();
			AssertEquals("One entrynum only", 1, consol.CusEntryNums.Count);
			Common.CusEntryNumber entryNum = consol.CusEntryNums[0];
			AssertNotNull("Entry num is generated to store status", entryNum);
			AssertEquals("Status is set", OutwardReportStatusList.Codes.AwaitingResponse, entryNum.CE_EntryStatus);
		}

		public void TestUNH()
		{
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("UNH generated incorrectly", "UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03A:UN", GetSegment(generatedMessage, "UNH"));
		}

		public void TestBGM()
		{
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("BGM generated incorrectly", "BGM+833:::DEPART+<<SENDERS REFERENCE PLACE HOLDER>>+9", GetSegment(generatedMessage, "BGM"));
		}

		public void TestNADForConsolidator()
		{
			SetUpValidAIRConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40009914H");
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("NAD generated incorrectly", "NAD+CS+40009914H:ZZZ:143", GetSegment(generatedMessage, "NAD+CS"));
		}

		public void TestNADForVesselOperator()
		{
			SetUpValidAIRConsol();
			shippingLine.OH_FullName = "STAR OCEAN CARRIERS";
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("NAD generated incorrectly", "NAD+CH++STAR OCEAN CARRIERS", GetSegment(generatedMessage, "NAD+CH"));
		}

		public void TestTDTForSEA()
		{
			SetUpValidSEAConsol();
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "WELLINGTON STAR";

			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "V123N";
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("TDT generated incorrectly", "TDT+20+V123N+1+++++:::WELLINGTON STAR", GetSegment(generatedMessage, "TDT"));
		}

		public void TestTDTForAIR()
		{
			SetUpValidAIRConsol();
			transport.JW_VoyageFlight = "QF2";
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("TDT generated incorrectly", "TDT+20++4+++++:::QF2", GetSegment(generatedMessage, "TDT"));
		}

		public void TestLOCDeparture()
		{
			SetUpValidAIRConsol();

			transport.JW_RL_NKLoadPort = "NZWLG";

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("LOC generated incorrectly", "LOC+5+NZWLG", GetSegment(generatedMessage, "LOC+5"));
		}

		public void TestLOCDestination()
		{
			SetUpValidAIRConsol();
			consol.JK_RL_NKDischargePort = "USLAX";
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("LOC generated incorrectly", "LOC+8+US", GetSegment(generatedMessage, "LOC+8"));
		}

		public void TestDTM()
		{
			SetUpValidAIRConsol();
			transport.JW_ETD = new ZDateTime(2003, 10, 16);
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("DTM generated incorrectly", "DTM+136:20031016:102", GetSegment(generatedMessage, "DTM"));
		}

		public void TestEQD()
		{
			SetUpValidSEAConsol();
			container.JC_ContainerNum = "ABCU1234560";
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("EQD generated incorrectly", "EQD+CN+ABCU1234560++++7", GetSegment(generatedMessage, "EQD"));
		}

		public void TestCNT()
		{
			SetUpValidAIRConsol();
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("CNT generated incorrectly", "CNT+2:1", GetSegment(generatedMessage, "CNT"));
		}

		public void TestCNI()
		{
			SetUpValidAIRConsol();
			shipment.CustomsEntryNumber = "89012345";
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("CNI generated incorrectly", "CNI+1+89012345", GetSegment(generatedMessage, "CNI"));
		}

		public void TestCNIForSecondShipments()
		{
			SetUpValidAIRConsol();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.CustomsEntryNumberType = "ENT";
			shipment2.CustomsEntryNumber = "23456789";
			shipment2.JS_UniqueConsignRef = "SB0001000";
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("CNI for second shipment generated incorrectly", "CNI+2+23456789", GetSegment(generatedMessage, "CNI+2"));
		}

		public void TestRFF()
		{
			SetUpValidAIRConsol();
			shipment.JS_HouseBill = "SOC76543WLG";
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("RFF generated incorrectly", "RFF+HWB:SOC76543WLG", GetSegment(generatedMessage, "RFF+HWB"));
		}

		public void TestUNT()
		{
			SetUpValidAIRConsol();
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			string[] segments = generatedMessage.Split('\'');
			Assert("UNT generated", !string.IsNullOrEmpty(GetSegment(generatedMessage, "UNT")));
		}

		public void TestGenerateOriginalMessageForAir()
		{
			SetUpValidAIRConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40009914H");

			transport.JW_VoyageFlight = "QF2";
			transport.JW_RL_NKLoadPort = "NZWLG";
			transport.JW_RL_NKDiscPort = "USLAX";

			consol.JK_RL_NKLoadPort = "NZWLG";
			consol.JK_RL_NKDischargePort = "USLAX";

			transport.JW_ETD = new ZDateTime(2003, 10, 14);
			shippingLine.OH_FullName = "Qantas";

			shipment.CustomsEntryNumber = "89012345";
			shipment.JS_HouseBill = "SOC76543WLG";
			shipment.JS_UniqueConsignRef = "SA0001000";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.CustomsEntryNumberType = "ENT";
			shipment2.CustomsEntryNumber = "23456789";
			shipment2.JS_HouseBill = "SOC00009876";
			shipment2.JS_UniqueConsignRef = "SB0001000";

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertMultilineEquals("Air ORN report", ExpectedOriginalForAir.Replace("\r\n", ""), generatedMessage, '\'');
		}
		#region ExpectedOriginalForAir
		const string ExpectedOriginalForAir = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03A:UN'
BGM+833:::DEPART+<<SENDERS REFERENCE PLACE HOLDER>>+9'
NAD+CS+40009914H:ZZZ:143'
NAD+CH++QANTAS'
TDT+20++4+++++:::QF2'
LOC+5+NZWLG'
LOC+8+US'
DTM+136:20031014:102'
CNT+2:2'
CNI+1+89012345'
RFF+HWB:SOC76543WLG'
CNI+2+23456789'
RFF+HWB:SOC00009876'
UNT+14+<<MSGNO PLACEHOLDER>>'
";
		#endregion

		public void TestWithInvalidCharacters()
		{
			SetUpValidAIRConsol();
			shippingLine.OH_FullName = "STAR OCEAN? CARRIERS";
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			string nADSegment = GetSegment(generatedMessage, "NAD+CH");
			AssertEquals("NAD generated incorrectly", "NAD+CH++STAR OCEAN  CARRIERS", nADSegment);
		}

		public void TestGenerateOrignalMessageForSubShipments()
		{
			SetUpValidAIRConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40009914H");

			transport.JW_VoyageFlight = "QF2";
			transport.JW_RL_NKLoadPort = "NZWLG";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2003, 10, 14);
			shippingLine.OH_FullName = "Qantas";

			consol.JK_RL_NKLoadPort = "NZWLG";
			consol.JK_RL_NKDischargePort = "USLAX";

			shipment.CustomsEntryNumber = "";
			shipment.JS_HouseBill = "SOC76543WLG";
			shipment.JS_UniqueConsignRef = "SA0001000";

			ForwardingShipment sub1 = shipment.CoLoadShipments.AddNew() as ForwardingShipment;
			sub1.JS_HouseBill = "SOC00009876";
			sub1.CustomsEntryNumberType = "ENT";
			sub1.CustomsEntryNumber = "23456789";
			sub1.JS_UniqueConsignRef = "SA0001001";
			consol.Shipments.Add(sub1);

			ForwardingShipment sub2 = shipment.CoLoadShipments.AddNew() as ForwardingShipment;
			sub2.JS_HouseBill = "SOC00009877";
			sub2.CustomsEntryNumberType = "ENT";
			sub2.CustomsEntryNumber = "23456788";
			sub2.JS_UniqueConsignRef = "SA0001002";
			consol.Shipments.Add(sub2);

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertMultilineEquals("Air ORN report", ExpectedOriginalForSubShipments.Replace("\r\n", ""), generatedMessage, '\'');
		}
		#region ExpectedOriginalForSubShipments
		const string ExpectedOriginalForSubShipments = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03A:UN'
BGM+833:::DEPART+<<SENDERS REFERENCE PLACE HOLDER>>+9'
NAD+CS+40009914H:ZZZ:143'
NAD+CH++QANTAS'
TDT+20++4+++++:::QF2'
LOC+5+NZWLG'
LOC+8+US'
DTM+136:20031014:102'
CNT+2:2'
CNI+1+23456789'
RFF+HWB:SOC00009876'
CNI+2+23456788'
RFF+HWB:SOC00009877'
UNT+14+<<MSGNO PLACEHOLDER>>'
";
		#endregion

		public void TestGenerateOriginalMessageForSEA()
		{
			SetUpValidSEAConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40009914H");
			shippingLine.OH_FullName = "SUNSHINE CRUISE";
			consol.SetDefaultShippingLineAddress(shippingLine);

			transport.JW_VoyageFlight = "V12345O";
			transport.JW_RL_NKLoadPort = "NZWLG";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2003, 10, 14);

			consol.JK_RL_NKLoadPort = "NZWLG";
			consol.JK_RL_NKDischargePort = "USLAX";

			shipment.CustomsEntryNumber = "89012345";
			shipment.JS_HouseBill = "SOC76543WLG";
			shipment.JS_UniqueConsignRef = "SA0001000";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.CustomsEntryNumberType = "ENT";
			shipment2.CustomsEntryNumber = "23456789";
			shipment2.JS_HouseBill = "SOC00009876";
			shipment2.JS_UniqueConsignRef = "SA0001001";

			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.JC_ContainerNum = "CRUX12345";

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2.JC_ContainerNum = "CRUX12346";

			CommonContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container3.JC_ContainerNum = "CRUX12347";
			container3.JC_IsEmptyContainer = true;

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertMultilineEquals("Sea ORN report", ExpectedOriginalForSea.Replace("\r\n", ""), generatedMessage, '\'');
		}
		#region ExpectedOriginalForSea
		const string ExpectedOriginalForSea = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03A:UN'
BGM+833:::DEPART+<<SENDERS REFERENCE PLACE HOLDER>>+9'
NAD+CS+40009914H:ZZZ:143'
NAD+CH++SUNSHINE CRUISE'
TDT+20+V12345O+1+++++:::TESTVESL'
LOC+5+NZWLG'
LOC+8+US'
DTM+136:20031014:102'
EQD+CN+CRUX12345++++7'
EQD+CN+CRUX12346++++5'
EQD+CN+CRUX12347++++4'
CNT+2:2'
CNI+1+89012345'
RFF+HWB:SOC76543WLG'
CNI+2+23456789'
RFF+HWB:SOC00009876'
UNT+17+<<MSGNO PLACEHOLDER>>'
";
		#endregion

		public void TestGenerateReplacementMessage()
		{
			SetUpValidSEAConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40009914H");
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = consol.PK;
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = "45678910";

			shippingLine.OH_FullName = "STAR OCEAN CARRIERS";
			consol.SetDefaultShippingLineAddress(shippingLine);
			transport.JW_VoyageFlight = "V123N";
			shipment.JS_HouseBill = "SOC00009876";
			shipment.CustomsEntryNumber = "87032732";
			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Replacement, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertMultilineEquals("Sea ORN replacement report", ExpectedReplacementCarrierReport.Replace("\r\n", ""), generatedMessage, '\'');
		}
		#region ExpectedReplacementCarrierReport
		const string ExpectedReplacementCarrierReport = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03A:UN'
BGM+833:::DEPART+<<SENDERS REFERENCE PLACE HOLDER>>+5'
RFF+RF:45678910'
NAD+CS+40009914H:ZZZ:143'
NAD+CH++STAR OCEAN CARRIERS'
TDT+20+V123N+1+++++:::TESTVESL'
LOC+5+NZAKL'
LOC+8+AU'
DTM+136:20050101:102'
EQD+CN+CRUX12345++++7'
CNT+2:1'
CNI+1+87032732'
RFF+HWB:SOC00009876'
UNT+14+<<MSGNO PLACEHOLDER>>'";
		#endregion

		public void TestSendMasterShipmentOnlyWhenMasterHasClearanceNo()
		{
			SetUpValidAIRConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40009914H");

			shipment.CustomsEntryNumber = "89012345";
			shipment.JS_HouseBill = "SOC76543WLG";
			shipment.JS_UniqueConsignRef = "SA0001000";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.CustomsEntryNumber = "23456789";
			shipment2.JS_HouseBill = "SOC00009876";
			shipment2.JS_UniqueConsignRef = "SA0001001";
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Replacement, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			string cNTSegment = GetSegment(generatedMessage, "CNT+2:");
			AssertEquals("Only one bill is reported", "1", cNTSegment.Substring(6, 1));
		}

		public void TestSendSubShipmentWhenMasterDoesntHaveClearanceNo()
		{
			SetUpValidAIRConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40009914H");

			shipment.CustomsEntryNumber = "";
			shipment.JS_HouseBill = "SOC76543WLG";
			shipment.JS_UniqueConsignRef = "SA0001000";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.CustomsEntryNumber = "23456789";
			shipment2.JS_HouseBill = "SOC00009876";
			shipment2.JS_UniqueConsignRef = "SA0001001";
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Replacement, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			string cNTSegment = GetSegment(generatedMessage, "CNT+2:");
			AssertEquals("Only one bill is reported", "1", cNTSegment.Substring(6, 1));
		}

		public void TestCancelReport()
		{
			SetUpValidSEAConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40009914H");
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = consol.PK;
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = "45678910";

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Cancellation, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertMultilineEquals("Sea ORN Cancel report", ExpectedCancelReport.Replace("\r\n", ""), generatedMessage, '\'');
		}
		#region ExpectedCancelReport
		const string ExpectedCancelReport = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03A:UN'
BGM+833:::DEPART+<<SENDERS REFERENCE PLACE HOLDER>>+1'
RFF+RF:45678910'
NAD+CS+40009914H:ZZZ:143'
UNT+5+<<MSGNO PLACEHOLDER>>'";
		#endregion

		public void TestBuildOnOrderedShipments()
		{
			SetUpValidSEAConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "40009914H");
			shipment.JS_UniqueConsignRef = "SA0001001";
			shipment.JS_HouseBill = "S1";

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "S2";
			shipment2.CustomsEntryNumberType = "ENT";
			shipment2.CustomsEntryNumber = "EntryNumber2";
			shipment2.JS_UniqueConsignRef = "SA0001000";

			messageBuilder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			string generatedMessage = messageBuilder.GetMessageText();

			string firstCNI = GetSegment(generatedMessage, "CNI+1");
			Assert("Shipments should be ordered on JS_UniqueConsignRef", firstCNI.IndexOf(shipment2.CustomsEntryNumber.ToUpper()) > 0);

			string secondCNI = GetSegment(generatedMessage, "CNI+2");
			Assert("Shipments should be ordered on JS_UniqueConsignRef", secondCNI.IndexOf(shipment.CustomsEntryNumber.ToUpper()) > 0);
		}

		//public void TestGenerateGroup0SetsCharacterSetToUNOA()
		//{
		//  SetUpValidSEAConsol();
		//  messageBuilder = new MessageBuilder(Consol, MessageBuilder.MessageTypes.Original, ManifestStatus);

		//  UNCharacterSet.SetInstance(new UNOBCharacterSet());
		//  AssertEquals("UNOA Character set", typeof(UNOBCharacterSet), UNCharacterSet.GetInstance().GetType());

		//  string GeneratedMessage = messageBuilder.GetMessageText();
		//  AssertEquals("UNOA Character set", typeof(UNOACharacterSet), UNCharacterSet.GetInstance().GetType());
		//}

		#region Implementation

		protected new MessageBuilder messageBuilder
		{
			get
			{
				return (MessageBuilder)base.messageBuilder;
			}
			set
			{
				base.messageBuilder = value;
			}
		}

		ForwardingConsol consol;
		Transport transport;
		OutwardReportManifestStatus manifestStatus;

		CommonShipment shipment;
		CommonContainer container;
		OrgHeader shippingLine;
		RefVessel vessel;

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345678A");
			consol = Factory.New<ForwardingConsol>();
			transport = consol.Transports[0];
			manifestStatus = new OutwardReportManifestStatus(consol);
		}

		protected void SetUpShipmentsAndCommonData()
		{
			shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Shipment1212";
			shipment.CustomsEntryNumberType = "ENT";
			shipment.CustomsEntryNumber = "EntryNumber";

			shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consol.SetDefaultShippingLineAddress(shippingLine);

			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2005, 1, 1);

			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
		}

		protected void SetUpValidAIRConsol()
		{
			SetUpShipmentsAndCommonData();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			transport.JW_VoyageFlight = "QF23";
		}

		protected void SetUpValidSEAConsol()
		{
			SetUpShipmentsAndCommonData();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			transport.JW_VoyageFlight = "12345678";

			vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_Code = "TESTVESL";
			transport.JW_Vessel = vessel.RV_Code;

			container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.JC_ContainerNum = "CRUX12345";
		}

		protected string GetSegment(string wholeMessage, string segmentTagDesired)
		{
			string[] segments = wholeMessage.Split('\'');
			foreach (string segment in segments)
			{
				if (segment.StartsWith(segmentTagDesired))
				{
					return segment;
				}
			}
			return string.Empty;
		}

		#endregion
	}

	public class ErrorCheckingTestClassForORNBuilder : TestCaseWithFactory
	{
		public void TestOutwardReportWithExpressClearances()
		{
			CusMAWB mawb = Factory.New<CusMAWB>();
			TestCusMAWBCreator testMAWBCreator = new TestCusMAWBCreator(mawb, "08122222222", "QF344", "NZAKL", "USLAX", new ZDateTime(2007, 8, 21), new ZDateTime(2007, 8, 22));
			mawb.CM_MessageReference = "X00003434";
			mawb.ECINumber = "89898989";

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00004444";
			TestJobConsolCreator testConsolCreator = new TestJobConsolCreator(consol, "08122222222", "QF344", "NZAKL", "USLAX", new ZDateTime(2007, 8, 21), new ZDateTime(2007, 8, 22), testMAWBCreator.Carrier);

			ForwardingShipment shipment0 = testConsolCreator.AddShipment(0, "NZCHC", "USDNQ", testMAWBCreator.Supplier2.PK, testMAWBCreator.Importer2.PK);

			ForwardingShipment shipment1 = testConsolCreator.AddShipment(1, "NZAKL", "USLAX", testMAWBCreator.Supplier1.PK, testMAWBCreator.Importer1.PK);
			JobDeclaration declarationInOtherCountry = Factory.New<JobDeclaration>();
			declarationInOtherCountry.JE_GB = ZGuid.NewZGuid();
			declarationInOtherCountry.JE_JS = shipment1.PK;

			ForwardingShipment shipmentWithDeclaration = testConsolCreator.AddShipment(6, "NZDUD", "USDFW", "C", "B");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipmentWithDeclaration.PK;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			declaration.DeclarationNumber = "33333333";

			CusHAWB hawb1 = testMAWBCreator.AddHAWB(testMAWBCreator.Supplier1, testMAWBCreator.Importer1, "HOUSEBILLX1", "NZAKL", "USLAX", "GOODS 0", 0.0m, 0, 00m, removeImporterAndSupplierGuids: true);
			hawb1.CS_JS = shipment0.PK;
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			CusHAWB hawb2 = testMAWBCreator.AddHAWB(testMAWBCreator.Supplier2, testMAWBCreator.Importer1, "HOUSEBILLX2", "NZAKL", "USSFO", "GOODS 1", 0.1m, 1, 10m, removeImporterAndSupplierGuids: true);
			hawb2.CS_JS = shipment0.PK;
			hawb2.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;

			NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var manifestStatus = new OutwardReportManifestStatus(consol);
			MessageBuilder builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);

			AssertEquals("builder.Errors", @"AirCargo ICR/CRE Consignment X00003434-2 is not written off and not converted to a standalone shipment.You have not entered a Customs Entry Number for shipment, S00090001", builder.Errors.Trim());

			shipment1.CustomsEntryNumberType = "ENT";
			shipment1.CustomsEntryNumber = "90909090";
			ForwardingShipment shipment2 = testConsolCreator.AddShipment(2, "NZAKL", "USLAX", testMAWBCreator.Supplier1.PK, testMAWBCreator.Importer1.PK);
			shipment2.JS_UniqueConsignRef = "S00004545";
			hawb2.CS_JS = shipment2.PK;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("builder.Errors", "", builder.Errors.Trim());

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
			declaration2.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			declaration2.DeclarationNumber = "44444444";
			shipment2.ResetCusEntryNumbers();
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("builder.Errors", "", builder.Errors.Trim());
		}

		public void TestNoErrorForValidConsol()
		{
			SetUpValidAIRConsol();
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("This is a valid consol", 0, builder.ErrorCount);
		}

		public void TestCheckClearanceNumberIfCancelationOrReplacement()
		{
			SetUpValidAIRConsol();
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Replacement, manifestStatus);
			AssertEquals("For replacement, Clearance number for Consol is necessary", 1, builder.ErrorCount);
		}

		public void TestCheckValidBrokerageID()
		{
			SetUpValidAIRConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Broker ID is necessary in Registry", 1, builder.ErrorCount);
		}

		public void TestCheckValidShippingLine()
		{
			SetUpValidAIRConsol();
			consol.SetDefaultShippingLineAddress(ZGuid.Empty);
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Shipping Line is empty", 1, builder.ErrorCount);
		}

		public void TestCheckTransportMode()
		{
			SetUpValidAIRConsol();
			consol.JK_TransportMode = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			Assert("Consol doesnt have transport mode entered", builder.ErrorCount > 0);
		}

		public void TestCheckFlightNumberForAir()
		{
			SetUpValidAIRConsol();
			transport.JW_VoyageFlight = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			Assert("Consol doesnt have flight no entered", builder.ErrorCount > 0);
		}

		public void TestCheckVoyageNumerForSea()
		{
			SetUpValidSEAConsol();
			transport.JW_VoyageFlight = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			Assert("Consol doesnt have voyage no entered", builder.ErrorCount > 0);
		}

		public void TestCheckVesselForSea()
		{
			SetUpValidSEAConsol();

			transport.JW_Vessel = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			Assert("Consol doesnt have vessel no entered", builder.ErrorCount > 0);

			transport.JW_Vessel = "Rubbish";
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			Assert("Consol has vessel no entered", builder.ErrorCount == 0);
		}

		public void TestCheckVesselForAir()
		{
			SetUpValidAIRConsol();

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;

			transport.Sailing.Voyage.JV_OH_Line = carrier.PK;
			transport.JW_Vessel = ZString.Empty;

			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Consol is air and empty vessel is not invalid", 0, builder.ErrorCount);
		}

		public void TestCheckLoadPortOfConsol()
		{
			SetUpValidAIRConsol();

			consol.JK_RL_NKLoadPort = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Port of loading may not be empty", 1, builder.ErrorCount);

			consol.JK_RL_NKLoadPort = "AUSYD";
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Port of loading may not be an overseas port", 1, builder.ErrorCount);
		}

		public void TestCheckDischargePortOfConsol()
		{
			SetUpValidAIRConsol();

			consol.JK_RL_NKDischargePort = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Port of discharge may not be empty", 1, builder.ErrorCount);

			consol.JK_RL_NKDischargePort = "NZAKL";
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Port of loading may not be a NZ port", 1, builder.ErrorCount);
		}

		public void TestCheckShipmentsCount()
		{
			SetUpValidAIRConsol();
			consol.Shipments.Remove(shipment);
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("There should be at least one shipment", 1, builder.ErrorCount);
		}

		public void TestCheckShipmentsHouseBill()
		{
			SetUpValidAIRConsol();
			shipment.JS_HouseBill = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("House Bill may not be empty", 1, builder.ErrorCount);
		}

		public void TestCheckShipmentsClearanceNumber()
		{
			SetUpValidAIRConsol();
			shipment.CustomsEntryNumber = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Shipment's clearance no may not be empty", 1, builder.ErrorCount);
		}

		public void TestCheckSubShipmentsClearanceNumber()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = ZString.Empty;
			CommonShipment subshipment = shipment.CoLoadShipments.AddNew();
			subshipment.CustomsEntryNumberType = "CUS";
			subshipment.CustomsEntryNumber = "C123";
			subshipment.JS_HouseBill = "S121212";
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Shipment's clearance empty, but its subshipment has one", 0, builder.ErrorCount);
		}

		public void TestCheckCustomsEntryNumberWhenMasterHasOneWhileTwoSubShipmentsDontHave()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = "123456";
			CommonShipment subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.CustomsEntryNumber = "";
			CommonShipment subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.CustomsEntryNumber = "";

			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Shipment's clearance not empty, but its subshipment has none", 0, builder.ErrorCount);
		}

		public void TestCheckCustomsEntryNumberWhenMasterHasOneWhileOneOfSubShipmentsDoesntHave()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = "123456";
			CommonShipment subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.CustomsEntryNumber = "";
			CommonShipment subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.CustomsEntryNumber = "1234";

			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Shipment's clearance not empty, but one subshipment has none", 0, builder.ErrorCount);
		}

		public void CheckCustomsEntryNumberWhenMasterDoesntHaveAndOneOfSubShipmentDoesHave()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = "";
			CommonShipment subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.CustomsEntryNumber = "";
			CommonShipment subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.CustomsEntryNumber = "1234";

			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Shipment's clearance not empty, but one subshipment has none", 1, builder.ErrorCount);
		}

		public void TestCheckSubShipmentsClearanceNumber2()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = ZString.Empty;
			CommonShipment subshipment = shipment.CoLoadShipments.AddNew();
			subshipment.CustomsEntryNumber = ZString.Empty;
			subshipment.JS_HouseBill = "S121212";
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("Shipment's clearance may not be empty", 1, builder.ErrorCount);
		}

		public void TestValidETD()
		{
			SetUpValidAIRConsol();

			transport.JW_ETD = ZDateTime.Empty;

			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			AssertEquals("ETD may not be empty", 1, builder.ErrorCount);
		}

		public void TestContainerNumber()
		{
			SetUpValidSEAConsol();
			container.JC_ContainerNum = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			Assert("Container number may not be empty", builder.ErrorCount > 0);
		}

		public void TestContainerMode()
		{
			SetUpValidSEAConsol();
			container.JC_ContainerMode = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			Assert("Container mode may not be empty", builder.ErrorCount > 0);
		}

		public void TestCheckErrorsAfterSaving()
		{
			SetUpValidAIRConsol();
			shipment.JS_HouseBill = ZString.Empty;
			builder = new MessageBuilder(consol, MessageBuilder.MessageTypes.Original, manifestStatus);
			Assert("House bill number may not be empty", builder.ErrorCount == 1);

			shipment.JS_HouseBill = "123456";
			Factory.Save();
			AssertEquals("House bill number is now there", 0, builder.ErrorCount);
		}

		#region Implementation
		ForwardingConsol consol;
		Transport transport;
		OutwardReportManifestStatus manifestStatus;
		MessageBuilder builder;
		CommonShipment shipment;
		CommonContainer container;

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345678A");
			consol = Factory.New<ForwardingConsol>();
			transport = consol.Transports[0];

			manifestStatus = new OutwardReportManifestStatus(consol);
		}

		protected void SetUpShipmentsAndCommonData()
		{
			shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Shipment1212";
			shipment.CustomsEntryNumberType = "ENT";
			shipment.CustomsEntryNumber = "EntryNumber";
			shipment.JS_UniqueConsignRef = "SA0001000";

			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consol.SetDefaultShippingLineAddress(shippingLine);
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2005, 1, 1);
		}

		protected void SetUpValidAIRConsol()
		{
			SetUpShipmentsAndCommonData();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			transport.JW_VoyageFlight = "QF23";
		}

		protected void SetUpValidSEAConsol()
		{
			SetUpShipmentsAndCommonData();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "APLU13102015";

			transport.JW_VoyageFlight = "12345678";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_Code = "TESTVESL";
			transport.JW_Vessel = vessel.RV_Code;

			container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.JC_ContainerNum = "CRUX12345";
		}
		#endregion
	}
}
