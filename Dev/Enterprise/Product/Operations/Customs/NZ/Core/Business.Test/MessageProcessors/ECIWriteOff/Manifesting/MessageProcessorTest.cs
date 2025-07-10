using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using JobDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration;
using NZCMessage = Enterprise.Customs.NZ.Business.Declaration.NZCMessage;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.ECIWriteOff.Manifesting.Testing
{
	public class MessageProcessorTest : TestCaseWithFactory
	{
		#region Test3Consignments2WrittenOff1InError
		public void Test3Consignments2WrittenOff1InError()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader, "081-11111111", "QF254", "USDNQ", "NZCHC", new ZDateTime(2005, 11, 27), new ZDateTime(2005, 12, 4));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "PINGPONG", "BALLS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer2, "RUBBER", "BALLS", 14.3m, 3, 36.63m);
			JobDeclaration declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer1, "TENNIS", "BALLS", 13.4m, 2, 27.72m);
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration3.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Factory.Save();

			NZCMessage message = entryHeader.Messages.AddNew();
			message.EM_MessageText = Message3Consignments2WrittenOff1InError.Replace("\r", "").Replace("\n", "");
			message.IsTransmitMessage = false;

			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);

			processor.ProcessMessage(message);
			AssertMultilineASCIIEquals("Message Results Should Match", EmailECIManifest3Consignments2WrittenOff1InError, message.EM_MessageInterpretation);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.ManifestInError, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration2.JE_EntryStatus);
			AssertEquals("Declaration3.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration3.JE_EntryStatus);
			AssertEquals("EntryHeader.Messages.Count", 1, entryHeader.Messages.Count);
		}
		#endregion
		#region Message3Consignments2WrittenOff1InError
		const string Message3Consignments2WrittenOff1InError = @"
UNH+3143+CUSRES:D:96B:UN+M01010101'
BGM+932+10926889'
GIS+843:120:143'
DOC+WOF:148:143+1::PINGPONG'
DOC+ERR:148:143+2::RUBBER'
ERP+1::430'
ERC+153::143'
DOC+WOF:148:143+3::TENNIS'
CNT+10:3'
UNT+8+3143'
";
		#endregion
		#region EmailECIManifest3Consignments2WrittenOff1InError
		const string EmailECIManifest3Consignments2WrittenOff1InError = @"[ICR/CRE in Error, Check Consignments for Status] Response for ECI Manifest: M01010101

ECI Write-Off Report
----------------------------------------------------------------------
ECI Manifest   : M01010101
Entry Number   : 10926889
Master Bill    : 081-11111111
Message No     : 3143

Message Status : (843) ECI Received With Errors.
                 Some Consignments May have been Written Off.

Summary        : 2 out of 3 Jobs have been Written Off.

Job Responses
----------------------------------------------------------------------
Job Number: M01010101-1   House Bill: PINGPONG
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: M01010101-2   House Bill: RUBBER
--- Clearance Status: ERR-Consignment In Error ---
**Error** in Job Details, Country/Region of Origin:-
  Country/Region of Origin : Not specified or invalid.

Job Number: M01010101-3   House Bill: TENNIS
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";
		#endregion

		#region Test4Consignments3WrittenOffOutOf4
		public void Test4Consignments3WrittenOffOutOf4()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader, "081-11111111", "QF254", "USDNQ", "NZCHC", new ZDateTime(2005, 11, 27), new ZDateTime(2005, 12, 4));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "PINGPONG", "BALLS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer2, "RUBBER", "BALLS", 14.3m, 3, 36.63m);
			JobDeclaration declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer1, "TENNIS", "BALLS", 13.4m, 2, 27.72m);
			JobDeclaration declaration4 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "BEACH", "BALLS", 12.5m, 2, 18.81m);
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration3.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration4.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Factory.Save();

			NZCMessage message = entryHeader.Messages.AddNew();
			message.EM_MessageText = Message3ConsignmentsWrittenOffOutOf4.Replace("\r", "").Replace("\n", "");
			message.IsTransmitMessage = false;

			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);

			processor.ProcessMessage(message);
			AssertMultilineASCIIEquals("Message Results Should Match", Email3ConsignmentsWrittenOffOutOf4, message.EM_MessageInterpretation);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.ManifestAccepted, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration2.JE_EntryStatus);
			AssertEquals("Declaration3.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration3.JE_EntryStatus);
			AssertEquals("Declaration4.JE_EntryStatus", LowValueConsignmentStatusList.Codes.FormalDeclarationRequired, declaration4.JE_EntryStatus);
			AssertEquals("EntryHeader.Messages.Count", 1, entryHeader.Messages.Count);
		}
		#endregion
		#region Message3ConsignmentsWrittenOffOutOf4
		const string Message3ConsignmentsWrittenOffOutOf4 = @"
UNH+3143+CUSRES:D:96B:UN+M01010101'
BGM+932+10926889'
GIS+842:120:143'
DOC+WOF:148:143+1::PINGPONG'
DOC+WOF:148:143+2::RUBBER'
DOC+WOF:148:143+3::TENNIS'
CNT+10:3'
UNT+8+3143'
";
		#endregion
		#region Email3ConsignmentsWrittenOffOutOf4
		const string Email3ConsignmentsWrittenOffOutOf4 = @"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Manifest: M01010101

ECI Write-Off Report
----------------------------------------------------------------------
ECI Manifest   : M01010101
Entry Number   : 10926889
Master Bill    : 081-11111111
Message No     : 3143

Message Status : (842) ECI Received OK.
                 Consignments have been Written Off.

Summary        : 3 out of 4 Jobs have been Written Off.

Job Responses
----------------------------------------------------------------------
Job Number: M01010101-1   House Bill: PINGPONG
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: M01010101-2   House Bill: RUBBER
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: M01010101-3   House Bill: TENNIS
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: M01010101-4   House Bill: BEACH
--- Clearance Status: FEQ-Formal Declaration Required ---
";
		#endregion

		#region Test2ConsignmentsWithErrors
		public void Test2ConsignmentsWithErrors()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader, "081-11111111", "QF254", "USDNQ", "NZCHC", new ZDateTime(2005, 11, 27), new ZDateTime(2005, 12, 4));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "PINGPONG", "BALLS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer2, "RUBBER", "BALLS", 14.3m, 3, 36.63m);
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Factory.Save();

			NZCMessage message = entryHeader.Messages.AddNew();
			message.EM_MessageText = Message2ConsignmentsWithErrors.Replace("\r", "").Replace("\n", "");
			message.IsTransmitMessage = false;

			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);

			processor.ProcessMessage(message);
			AssertMultilineASCIIEquals("Message Results Should Match", Email2ConsignmentsWithErrors, message.EM_MessageInterpretation);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.ManifestInError, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration2.JE_EntryStatus);
			AssertEquals("EntryHeader.Messages.Count", 1, entryHeader.Messages.Count);
		}
		#endregion
		#region Message2ConsignmentsWithErrors
		const string Message2ConsignmentsWithErrors = @"
UNH+3143+CUSRES:D:96B:UN+M01010101'
BGM+932+10926889'
GIS+843:120:143'
DOC+ERR:148:143+1::PINGPONG'
ERP+1::430'
ERC+153::143'
DOC+ERR:148:143+2::RUBBER'
ERP+1::430'
ERC+153::143'
CNT+10:3'
UNT+8+3143'
";
		#endregion
		#region Email2ConsignmentsWithErrors
		const string Email2ConsignmentsWithErrors = @"[ICR/CRE in Error, Check Consignments for Status] Response for ECI Manifest: M01010101

ECI Write-Off Report
----------------------------------------------------------------------
ECI Manifest   : M01010101
Entry Number   : 10926889
Master Bill    : 081-11111111
Message No     : 3143

Message Status : (843) ECI Received With Errors.
                 Some Consignments May have been Written Off.

Summary        : 0 out of 2 Jobs have been Written Off.

Job Responses
----------------------------------------------------------------------
Job Number: M01010101-1   House Bill: PINGPONG
--- Clearance Status: ERR-Consignment In Error ---
**Error** in Job Details, Country/Region of Origin:-
  Country/Region of Origin : Not specified or invalid.

Job Number: M01010101-2   House Bill: RUBBER
--- Clearance Status: ERR-Consignment In Error ---
**Error** in Job Details, Country/Region of Origin:-
  Country/Region of Origin : Not specified or invalid.
";
		#endregion

		#region Test3Consignments2WrittenOff1Held
		public void Test3Consignments2WrittenOff1Held()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var manifestCreator = new TestManifestCreator(entryHeader, "081-11111111", "QF254", "USDNQ", "NZCHC", new ZDateTime(2005, 11, 27), new ZDateTime(2005, 12, 4));
			var declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "254763132", "BALLS", 15.2m, 4, 45.54m);
			var declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer2, "255125147", "BALLS", 14.3m, 3, 36.63m);
			var declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer1, "263923820", "BALLS", 13.4m, 2, 27.72m);
			var declaration4 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "254763137", "BALLS", 15.2m, 4, 1500m);
			var declaration5 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "254763139", "BALLS", 15.2m, 4, 2000m);
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration3.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration4.JE_EntryStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			declaration5.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			Factory.Save();

			var message = entryHeader.Messages.AddNew();
			message.EM_MessageText = Message3Consignments2WrittenOff1Held.Replace("\r", "").Replace("\n", "");
			message.IsTransmitMessage = false;

			var logger = new LoggingInformation();
			var processor = new MessageProcessor(logger);

			processor.ProcessMessage(message);
			AssertMultilineASCIIEquals("Message Results Should Match", EmailECIManifest3Consignments2WrittenOff1Held, message.EM_MessageInterpretation);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.InspectionsAuditRequirements, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentHeld, declaration2.JE_EntryStatus);
			AssertEquals("Declaration3.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration3.JE_EntryStatus);
			AssertEquals("EntryHeader.Messages.Count", 1, entryHeader.Messages.Count);

			// Now process the single write-off response
			var singleEntryHeader = declaration2.CusEntryHeader;
			var wofMessage = singleEntryHeader.Messages.AddNew();
			wofMessage.IsTransmitMessage = false;
			wofMessage.EM_MessageText = HeldConsignmentNowWrittenOff.Replace("\r", "").Replace("\n", "");
			wofMessage.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			wofMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			Factory.Save();

			processor.ProcessMessage(wofMessage);
			AssertEquals("Declaration2.JE_EntryStatus - this consignment status should now show it has been written off", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration2.JE_EntryStatus);
		}
		#endregion
		#region Message3Consignments2WrittenOff1Held
		const string Message3Consignments2WrittenOff1Held = @"
UNH+3143+CUSRES:D:96B:UN+M01010101'
BGM+932+10926889'
GIS+843:120:143'
DOC+WOF:148:143+1::254763132'
DOC+HLD:148:143+2::255125147'
ERP+1::430'
ERC+153::143'
DOC+WOF:148:143+3::263923820'
CNT+10:3'
UNT+8+3143'
";
		const string HeldConsignmentNowWrittenOff = @"
UNH+1+CUSRES:D:98A:UN+M01010101'
BGM+932+10926889'
GIS+842:120:143'
DOC+WOF:148:143+2::255125147'
CNT+10:1'
UNT+6+1'
";
		#endregion
		#region EmailECIManifest3Consignments2WrittenOff1Held
		const string EmailECIManifest3Consignments2WrittenOff1Held = @"[Inspections/Audit Requirements] Response for ECI Manifest: M01010101

ECI Write-Off Report
----------------------------------------------------------------------
ECI Manifest   : M01010101
Entry Number   : 10926889
Master Bill    : 081-11111111
Message No     : 3143

Message Status : (843) ECI Received With Errors.
                 Some Consignments May have been Written Off.

Summary        : 2 out of 5 Jobs have been Written Off.

Job Responses
----------------------------------------------------------------------
Job Number: M01010101-1   House Bill: 254763132
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: M01010101-2   House Bill: 255125147
--- Clearance Status: HLD-Consignment Held ---
**Error** in Job Details, Country/Region of Origin:-
  Country/Region of Origin : Not specified or invalid.

Job Number: M01010101-3   House Bill: 263923820
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
Job Number: M01010101-4   House Bill: 254763137
--- Clearance Status: FEQ-Formal Declaration Required ---
Job Number: M01010101-5   House Bill: 254763139
--- Clearance Status: FEQ-Formal Declaration Required ---
";
		#endregion

		public void TestMultipleECIsAgainstShipmentsAllGetTheEntryNumberPublishedBackToTheShipmentIfTheyAreWrittenOff()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader, "081-11111111", "QF254", "USDNQ", "NZCHC", new ZDateTime(2005, 11, 27), new ZDateTime(2005, 12, 4));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "PINGPONG", "BALLS", 15.2m, 4, 45.54m);
			ForwardingShipment shipment1 = GetNewShipmentLinkedToDeclaration(declaration1);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer2, "RUBBER", "BALLS", 14.3m, 3, 36.63m);
			ForwardingShipment shipment2 = GetNewShipmentLinkedToDeclaration(declaration2);
			JobDeclaration declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer1, "TENNIS", "BALLS", 13.4m, 2, 27.72m);
			ForwardingShipment shipment3 = GetNewShipmentLinkedToDeclaration(declaration3);
			JobDeclaration declaration4 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "BEACH", "BALLS", 12.5m, 2, 18.81m);
			ForwardingShipment shipment4 = GetNewShipmentLinkedToDeclaration(declaration4);
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration3.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration4.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Factory.Save();

			NZCMessage message = entryHeader.Messages.AddNew();
			message.EM_MessageText = Message3ConsignmentsWrittenOffOutOf4.Replace("\r", "").Replace("\n", "");
			message.IsTransmitMessage = false;

			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);

			processor.ProcessMessage(message);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration2.JE_EntryStatus);
			AssertEquals("Declaration3.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration3.JE_EntryStatus);
			AssertEquals("Declaration4.JE_EntryStatus", LowValueConsignmentStatusList.Codes.FormalDeclarationRequired, declaration4.JE_EntryStatus);

			AssertEquals("Shipment1.CustomsEntryNumber", "10926889", shipment1.CustomsEntryNumber);
			AssertEquals("Shipment2.CustomsEntryNumber", "10926889", shipment2.CustomsEntryNumber);
			AssertEquals("Shipment3.CustomsEntryNumber", "10926889", shipment3.CustomsEntryNumber);
			AssertEquals("Shipment4.CustomsEntryNumber", "10926889", shipment4.CustomsEntryNumber);
			ErrorReporter.Clear();
		}

		ForwardingShipment GetNewShipmentLinkedToDeclaration(JobDeclaration declaration)
		{
			ForwardingShipment result = Factory.New<ForwardingShipment>();
			result.ConsigneePK = declaration.JE_OH_Importer;
			result.ConsignorPK = declaration.JE_OH_Supplier;
			result.JS_RL_NKOrigin = declaration.JE_RL_NKOrigin;
			result.JS_RL_NKDestination = declaration.JE_RL_NKFinalDestination;
			result.JS_HouseBill = declaration.JE_HouseBill;
			result.JS_GoodsDescription = declaration.JE_GoodsDescription;
			result.JS_OuterPacks = declaration.JE_TotalNoOfPacks;
			result.JS_F3_NKPackType = "PKG";
			result.JS_ActualWeight = declaration.JE_TotalWeight;
			result.JS_UnitOfWeight = declaration.JE_TotalWeightUnit;
			result.JS_GoodsValue = declaration.JE_ECI_InvoiceAmount;
			var currency = Factory.Load<RefCurrency>(declaration.JE_ECI_InvoiceCurrency);
			result.JS_RX_NKGoodsValueCurr = currency.RX_Code;
			declaration.JE_JS = result.PK;
			ErrorReporter.Clear();
			return result;
		}
	}
}
