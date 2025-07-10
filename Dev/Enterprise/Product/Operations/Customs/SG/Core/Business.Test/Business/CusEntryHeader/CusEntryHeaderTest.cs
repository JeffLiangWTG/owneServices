using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		public void TestGetPreviousMessageContainerSequence()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			var sgCusdec = entryHeader as ISGCUSDEC;

			var containerSequence = sgCusdec.GetPreviousMessageContainerSequence();
			AssertEquals("[NO CONTAINER SEQUENCE DATA] 111 sequence number", -1, containerSequence.GetSequenceNumber("111"));
			AssertEquals("[NO CONTAINER SEQUENCE DATA] NEW sequence number", -1, containerSequence.GetSequenceNumber("NEW"));

			var testOriginalMessage = CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType);
			testOriginalMessage.EM_ApplicationCode = SGConstants.TradeNetVersion.Four;
			testOriginalMessage.EM_MessageSubType = Cuspmt09bMessageProcessor.MessageType;
			testOriginalMessage.EM_MessageText = "UNH+1+CUSPMT:0:1:RT:040+OUTPMT'EQD+CN+111:1+913:::FCL13'EQD+CN+222:2+517:::LCL77'UNT+2+1'";
			entryHeader.Messages.Add(testOriginalMessage);

			containerSequence = sgCusdec.GetPreviousMessageContainerSequence();
			AssertEquals("111 sequence number", 1, containerSequence.GetSequenceNumber("111"));
			AssertEquals("222 sequence number", 2, containerSequence.GetSequenceNumber("222"));
			AssertEquals("NEW sequence number", -1, containerSequence.GetSequenceNumber("NEW"));
			AssertEquals("HighestSequenceNumber", 2, containerSequence.HighestSequenceNumber);

			var testAmendmentMessage = CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType);
			testAmendmentMessage.EM_ApplicationCode = SGConstants.TradeNetVersion.Four;
			testAmendmentMessage.EM_MessageSubType = Cuspmt09bMessageProcessor.MessageType;
			testAmendmentMessage.EM_MessageText = "UNH+1+CUSPMT:0:1:RT:040+OUTPMT'EQD+CN+111:1+913:::FCL13'EQD+CN+333:3+517:::LCL77'UNT+2+1'";
			entryHeader.Messages.Add(testAmendmentMessage);

			containerSequence = sgCusdec.GetPreviousMessageContainerSequence();
			AssertEquals("111 sequence number", 1, containerSequence.GetSequenceNumber("111"));
			AssertEquals("222 sequence number", -1, containerSequence.GetSequenceNumber("222"));
			AssertEquals("222 sequence number", 3, containerSequence.GetSequenceNumber("333"));
			AssertEquals("NEW sequence number", -1, containerSequence.GetSequenceNumber("NEW"));
			AssertEquals("HighestSequenceNumber", 3, containerSequence.HighestSequenceNumber);
		}

		public new void TestRecoverFromUnsuccessfulSave()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_Status = "XXX";
			Assert(!entry.CH_Status.IsEmpty);
			entry.RecoverFromUnsuccessfulSave();
			Assert(entry.CH_Status.IsEmpty);
		}

		public void TestTypeDecider()
		{
			Assert(Factory.New<Customs.Business.CusEntryHeader>() is CusEntryHeader);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(Core.SGConstants.DeclarationStatus.JobOpenButNoMessageSent, EntryHeader.CH_Status);
		}

		public void TestEntryType()
		{
			Declaration.JE_MessageType = "";
			AssertEquals(CusEntryHeader.EntryTypes.Unspecified, EntryHeader.EntryType);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals(CusEntryHeader.EntryTypes.InPayment, EntryHeader.EntryType);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals(CusEntryHeader.EntryTypes.InNonPayment, EntryHeader.EntryType);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals(CusEntryHeader.EntryTypes.Outward, EntryHeader.EntryType);

			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			AssertEquals(CusEntryHeader.EntryTypes.OutwardWithCO, EntryHeader.EntryType);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals(CusEntryHeader.EntryTypes.Transhipment, EntryHeader.EntryType);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals(CusEntryHeader.EntryTypes.CertificateOfOrigin, EntryHeader.EntryType);

			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKO;
			AssertEquals("OUT with CO should only consider Application Product Type when NON Blanket dec", CusEntryHeader.EntryTypes.Outward, EntryHeader.EntryType);

			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCE;
			AssertEquals("This should now be OUT with CO dec", CusEntryHeader.EntryTypes.OutwardWithCO, EntryHeader.EntryType);
		}

		public void TestEntrySubType()
		{
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKP;
			AssertEquals(DeclarationTypeCodeList.Codes.BKP, EntryHeader.EntrySubType);
		}

		public void TestCH_BGMReference()
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "UEN";
			EDIMessage message = Factory.New<CUSDECEDIMessage>();
			message.EM_MessageNum = "TEST";
			EntryHeader.Messages.Add(message);

			AssertEquals("UEN                 " + "TEST", EntryHeader.CH_BGMReference);
		}

		public void TestInvoicesAreUnique()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-123";
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "44121000";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Singapore;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "44121000";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Malaysia;

			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("pre-condition - 2 merged lines", 2, declaration.MergedLinesCount);

			var entryHeader = Declaration.CusEntryHeader;
			var wrappedHeader = (ISGCUSDEC)entryHeader;
			AssertEquals("There should only be 1 Invoice for this entry", 1, wrappedHeader.Invoices.Count());
		}

		public void TestCH_Status()
		{
			EntryHeader.CH_Status = "ABC";
			AssertEquals("ABC", EntryHeader.CH_Status);
			AssertEquals("ABC", Declaration.JE_EntryStatus);

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today;

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals("ValuationDateOverride should not be affected", ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals("ValuationDateOverride should be cleared out", ZDateTime.Empty, invoiceHeader.JZ_ValuationDateOverride);

			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today;
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
			AssertEquals("ValuationDateOverride should be cleared out", ZDateTime.Empty, invoiceHeader.JZ_ValuationDateOverride);

			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today;
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals("ValuationDateOverride should not be affected", ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);

			ZDateTime entryDate = new ZDateTime(2009, 10, 03);
			invoiceHeader.JZ_ValuationDateOverride = entryDate;
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPending;
			AssertEquals("ValuationDateOverride should not be changed", entryDate, invoiceHeader.JZ_ValuationDateOverride);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			AssertEquals("ValuationDateOverride should not be changed", entryDate, invoiceHeader.JZ_ValuationDateOverride);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationPending;
			AssertEquals("ValuationDateOverride should not be changed", entryDate, invoiceHeader.JZ_ValuationDateOverride);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals("ValuationDateOverride should be cleared out on cancellation", ZDateTime.Empty, invoiceHeader.JZ_ValuationDateOverride);
		}

		public void TestAmendmentCount()
		{
			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, "INP"));
			AssertEquals("Default value - no update messages sent yet", 1, CusEntry.NumberOfRequestsForUpdate);

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType, "UNH+CWISEB00003391+CUSPMT:0:1:RT:041+INPPMT'BGM+962:::SFZ+199702247W       201207167099+11'CST++5'LOC+11+KZ'LOC+88+KZ'LOC+9+THBKK'DTM+178:20120716:102'DTM+416:20120716153057SST:304'GEI+5+:Y'MEA+ABK++BUN:10'MEA+AAH++TNE:0.990'FTX+AAI+++VENDOR TESTING'RFF+ABT:II8B986409H'DTM+148:20120716153057SST:304'DTM+273:2012071620120727:718'RFF+AEA:SC'FTX+CCI++Y99+SPECIMEN PERMIT ONLY'FTX+CCI++Z01+APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI++CJ+THIS PERMIT IS NOT TO BE USED FOR CARGO CLEARANCE.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'TDT+3+154N+1+++++:::JADE TRADER'DOC+704+OBI20939'"));
			EntryHeader.EntryNumber = "II8B986409H";
			AssertEquals("Default value - no update messages sent yet", 1, CusEntry.NumberOfRequestsForUpdate);

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, "INP"));
			AssertEquals("Default value - no update messages sent yet", 1, CusEntry.NumberOfRequestsForUpdate);

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType, "UNH+CWISEB00003391+CUSPMT:0:1:RT:041+INPPMT'BGM+962:::SFZ+199702247W       201207167099+11'CST++5'LOC+11+KZ'LOC+88+KZ'LOC+9+THBKK'DTM+178:20120716:102'DTM+416:20120716153057SST:304'GEI+5+:Y'MEA+ABK++BUN:10'MEA+AAH++TNE:0.990'FTX+AAI+++VENDOR TESTING'RFF+ABT:II8E017214H'DTM+148:20120716153057SST:304'DTM+273:2012071620120727:718'RFF+AEA:SC'FTX+CCI++Y99+SPECIMEN PERMIT ONLY'FTX+CCI++Z01+APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI++CJ+THIS PERMIT IS NOT TO BE USED FOR CARGO CLEARANCE.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'TDT+3+154N+1+++++:::JADE TRADER'DOC+704+OBI20939'"));
			EntryHeader.EntryNumber = "II8E017214H";

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, "INP"));
			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, "CAN", "UNH+1+CUSRES:D:09B:UN:041+STATUS'BGM+961:::CAC+198800784N       201111026817'FTX+ACD++C01+C01  - PERMIT NO. II1K006254W APPROVAL DATE - 02/11/2011 CANCELLATION APPROVED BY SINGAPORE CUSTOMS ON THE CONDITION THAT THE ABOVE CCP HAS NOT BEEN USED FOR CARGO CLEARANCE AND NO CERTIFICATE OF ORIGIN HAS BEEN ISSUED FOR THE CONSIGNMENT'RFF+MS:DCST.DCST401'DTM+416:20111102165720SST:304'RFF+MR:F01T.F01T010'RFF+ACW:II8E017214H'UNT+8+1'"));
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals(true, EntryHeader.HasBeenWithdrawn);

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, "INP"));
			AssertEquals("First amendment request", 1, CusEntry.NumberOfRequestsForUpdate);

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType, "UNH+CWISEB00003391+CUSPMT:0:1:RT:041+INPUPT'BGM+962:::SFZ+199702247W       201207167100+32'CST++5+AME'LOC+11+KZ'LOC+88+KZ'LOC+9+THBKK'DTM+178:20120716:102'DTM+416:20120716153255SST:304'GEI+5+:Y'MEA+ABK++BUN:10'MEA+AAH++TNE:0.990'FTX+ACF+++OCEAN BILL AND VESSEL TYPO'FTX+AAI+++VENDOR TESTING'RFF+ABT:II8B986409H'DTM+148:20120716153256SST:304'DTM+273:2012071620120727:718'DTM+56:20120716153057SST:304'FTX+CUS+++217000000000MF:218000000000MF:219000000000MF:206000000000MF'FTX+CUS+++045000000000MF:031000000000MF:027000000000MF'RFF+AEA:SC'FTX+CCI++Y99+SPECIMEN PERMIT ONLY'FTX+CCI++Z01+APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI++Z20+AMENDMENT APPROVED BY SINGAPORE CUSTOMS ON CONDITION THAT THE EARLIER    APPROVED PERMIT HAS NOT BEEN USED AND THIS SUPERCEDES THE PREVIOUS PERMIT.'FTX+CCI++CJ+THIS PERMIT IS NOT TO BE USED FOR CARGO CLEARANCE.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'TDT+3+154N+1+++++:::KALIMANIS'DOC+704+OBI20989'NAD+DT'CTA+IC+P213111:GARY O DEA'"));
			EntryHeader.EntryNumber = "II8B986409H";

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, "INP"));
			AssertEquals("Second amendment request - update count should ignore cancellation of alternative permit number", 2, CusEntry.NumberOfRequestsForUpdate);
		}

		public void TestSettingEntryNumberCreatesCMDDataForShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipmentWrapper = new CMDShipmentWrapper(shipment);
			shipment.JS_TransportMode = TransportTypeList.Codes.Sea;

			EntryHeader.EntryNumber = "OU0329849X";
			shipmentWrapper.CMDDataValues.Load();
			AssertEquals("Stand alone declaration should not create CMD permit number", 0, shipmentWrapper.CMDDataValues.Count);

			Declaration.JE_JS = shipment.PK;
			EntryHeader.EntryNumber = "TP0329849M";
			shipmentWrapper.CMDDataValues.Load();
			AssertEquals("Sea Freight Shipment linked declaration should not create CMD permit number", 0, shipmentWrapper.CMDDataValues.Count);

			shipment.JS_TransportMode = TransportTypeList.Codes.Air;
			EntryHeader.EntryNumber = "II0329849R";
			shipmentWrapper.CMDDataValues.Load();
			AssertEquals("Air Freight Shipment linked declaration should create CMD permit number for use in Shipment CMD messages", 1, shipmentWrapper.CMDDataValues.Count);
			AssertEquals("II0329849R", shipmentWrapper.CMDDataValues[0].CY_Data);

			EntryHeader.EntryNumber = "II0329849R";
			shipmentWrapper.CMDDataValues.Load();
			AssertEquals("Update of same entry number should not create another CMD permit number", 1, shipmentWrapper.CMDDataValues.Count);
			AssertEquals("II0329849R", shipmentWrapper.CMDDataValues[0].CY_Data);

			EntryHeader.EntryNumber = "II0549221G";
			shipmentWrapper.CMDDataValues.Load();
			AssertEquals("Update of another entry number should create another CMD permit number", 2, shipmentWrapper.CMDDataValues.Count);
			AssertEquals("II0329849R", shipmentWrapper.CMDDataValues[0].CY_Data);
			AssertEquals("II0549221G", shipmentWrapper.CMDDataValues[1].CY_Data);

			EntryHeader.EntryNumber = "II0329849R";
			shipmentWrapper.CMDDataValues.Load();
			AssertEquals("Update back to an existing entry number should not create another CMD permit number", 2, shipmentWrapper.CMDDataValues.Count);
			AssertEquals("II0329849R", shipmentWrapper.CMDDataValues[0].CY_Data);
			AssertEquals("II0549221G", shipmentWrapper.CMDDataValues[1].CY_Data);
		}

		public void TestIsFeePaidByBroker()
		{
			Declaration.JE_PaymentMethod = "";
			AssertEquals(false, EntryHeader.IsFeePaidByBroker("", "", null));

			Declaration.JE_PaymentMethod = BGIndicatorCodeList.Codes.D;
			AssertEquals(true, EntryHeader.IsFeePaidByBroker("", "", null));

			Declaration.JE_PaymentMethod = BGIndicatorCodeList.Codes.I;
			AssertEquals(false, EntryHeader.IsFeePaidByBroker("", "", null));
		}

		public void TestEntryChargeTypeList()
		{
			Assert(EntryHeader.EntryChargeTypeList is Enterprise.Registry.Business.Customs.EmptyEntryChargeTypeList);
		}

		public void TestEntryChargeTypeList1()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Assert(EntryHeader.EntryChargeTypeList is Registry.EntryChargeTypeList);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Assert(EntryHeader.EntryChargeTypeList is Registry.EntryChargeTypeList);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Assert(EntryHeader.EntryChargeTypeList is Registry.EntryChargeTypeList);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Assert(EntryHeader.EntryChargeTypeList is Enterprise.Registry.Business.Customs.EntryChargeTypeList);
		}

		public void TestCustomsCharges()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "44121000";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Singapore;

			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "44121000";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Malaysia;

			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, 10m);
			Declaration.ActiveEntryHeaders[0].MergedLines[1].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, 20m);

			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Duty, 110m);
			Declaration.ActiveEntryHeaders[0].MergedLines[1].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Duty, 120m);

			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 210m);
			Declaration.ActiveEntryHeaders[0].MergedLines[1].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 220m);

			AssertEquals(680m, GetCustomsCharges(Declaration.ActiveEntryHeaders[0]));

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Malaysia;
			AssertEquals(690m, GetCustomsCharges(Declaration.ActiveEntryHeaders[0]));

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Singapore;
			Declaration.SG_SupplyIndicator = SupplyIndicatorCodeList.Codes.Y;
			AssertEquals(690m, GetCustomsCharges(Declaration.ActiveEntryHeaders[0]));

			Declaration.SG_SupplyIndicator = "";
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			AssertEquals(690m, GetCustomsCharges(Declaration.ActiveEntryHeaders[0]));

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			AssertEquals(690m, GetCustomsCharges(Declaration.ActiveEntryHeaders[0]));

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			AssertEquals(680m, GetCustomsCharges(Declaration.ActiveEntryHeaders[0]));
		}

		ZDecimal GetCustomsCharges(Customs.Business.CusEntryHeader entryHeader)
		{
			ZDecimal result = 0m;
			foreach (CustomsCharge customsCharge in ServiceLocator.GetService<ICustomsCharges>(entryHeader).GetCustomsCharges(null))
			{
				result += customsCharge.Amount;
			}
			return result;
		}

		public void TestSG_PreviousEntryStatus()
		{
			EntryHeader.SG_PreviousEntryStatus = "ABC";
			AssertEquals("ABC", EntryHeader.SG_PreviousEntryStatus);

			EntryHeader.CH_Status = "PQR";
			EntryHeader.CH_Status = "XYZ";
			AssertEquals("PQR", EntryHeader.SG_PreviousEntryStatus);
		}

		public void TestDocumentWrapper()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var docWrappers = entryHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			AssertEquals("Enterprise.Customs.SG.V4.Business.DocCusEntryHeader", docWrappers.Single().GetType().FullName);
		}

		#region Entry/Message Status

		public void TestCanSendOriginal()
		{
			AssertEquals(true, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
			AssertEquals(true, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals(true, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPending;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationPending;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals(true, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms;
			AssertEquals(false, EntryHeader.CanSendOriginal);

			EntryHeader.Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(true, EntryHeader.CanSendOriginal);
		}

		public void TestIsWaitingForResponse()
		{
			AssertEquals(false, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(false, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationQuery;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			AssertEquals(false, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundQuery;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationPending;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals(false, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationQuery;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPending;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundPermitReceived;
			AssertEquals(false, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundSent;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundPending;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentQuery;
			AssertEquals(true, EntryHeader.IsWaitingForResponse);
		}

		public void TestCanSendWithdrawal()
		{
			AssertEquals(false, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			AssertEquals(false, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(false, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals(false, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals(false, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationPending;
			AssertEquals(false, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			AssertEquals(false, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPending;
			AssertEquals(false, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundSent;
			AssertEquals(false, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundPending;
			AssertEquals(false, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundPermitReceived;
			AssertEquals(true, EntryHeader.CanSendWithdrawal);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			EntryHeader.Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals(false, EntryHeader.CanSendWithdrawal);
		}

		public void TestIsOriginalPending()
		{
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(true, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			AssertEquals(true, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPending;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationPending;
			AssertEquals(false, EntryHeader.IsOriginalPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsOriginalPending);
		}

		public void TestIsAmendmentPending()
		{
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			AssertEquals(true, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPending;
			AssertEquals(true, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationPending;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsAmendmentPending);
		}

		public void TestIsRefundPending()
		{
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(false, EntryHeader.IsRefundPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsRefundPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(false, EntryHeader.IsRefundPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundSent;
			AssertEquals(true, EntryHeader.IsRefundPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundPending;
			AssertEquals(true, EntryHeader.IsRefundPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundPermitReceived;
			AssertEquals(false, EntryHeader.IsRefundPending);
		}

		public void TestIsCancellationPending()
		{
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPending;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			AssertEquals(true, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals(false, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationPending;
			AssertEquals(true, EntryHeader.IsCancellationPending);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsCancellationPending);
		}

		public void TestIsCancelled()
		{
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPending;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals(true, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationPending;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
			AssertEquals(false, EntryHeader.IsCancelledByStatus);
		}

		public void TestHasBeenWithdrawn()
		{
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(false, EntryHeader.HasBeenWithdrawn);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals(true, EntryHeader.HasBeenWithdrawn);
		}

		public void TestIsEntryQueried()
		{
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(false, EntryHeader.IsEntryQueried);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals(false, EntryHeader.IsEntryQueried);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(false, EntryHeader.IsEntryQueried);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationQuery;
			AssertEquals(true, EntryHeader.IsEntryQueried);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundSent;
			AssertEquals(false, EntryHeader.IsEntryQueried);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundPending;
			AssertEquals(false, EntryHeader.IsEntryQueried);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundPermitReceived;
			AssertEquals(false, EntryHeader.IsEntryQueried);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundQuery;
			AssertEquals(true, EntryHeader.IsEntryQueried);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			AssertEquals(false, EntryHeader.IsEntryQueried);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentQuery;
			AssertEquals(true, EntryHeader.IsEntryQueried);

			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			AssertEquals(false, EntryHeader.IsEntryQueried);
			EntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationQuery;
			AssertEquals(true, EntryHeader.IsEntryQueried);
		}

		#endregion

		#region Properties

		public void TestShouldCalculateGST()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals(true, EntryHeader.ShouldCalculateGST);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals(true, EntryHeader.ShouldCalculateGST);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals(false, EntryHeader.ShouldCalculateGST);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals(false, EntryHeader.ShouldCalculateGST);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals(false, EntryHeader.ShouldCalculateGST);
		}

		public void TestShouldCalculateDuty()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals(true, EntryHeader.ShouldCalculateDuty);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals(true, EntryHeader.ShouldCalculateDuty);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals(false, EntryHeader.ShouldCalculateDuty);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals(false, EntryHeader.ShouldCalculateDuty);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals(false, EntryHeader.ShouldCalculateDuty);
		}

		#endregion

		#region Interface Tests

		public void TestIsImport()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals("In Payment Declaration", true, CusEntry.IsImport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals("In Non Payment declaration", true, CusEntry.IsImport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals("Outward declaration", false, CusEntry.IsImport);
		}

		public void TestIsExport()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals("In Payment Declaration", false, CusEntry.IsExport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals("In Non Payment declaration", false, CusEntry.IsExport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals("Outward declaration", true, CusEntry.IsExport);
		}

		public void TestHasInwardTransport()
		{
			Declaration.JE_TransportMode = "";
			AssertEquals(false, CusEntry.HasInwardTransport);

			Declaration.JE_TransportMode = Core.Constants.TransportCodes.Air;
			AssertEquals(true, CusEntry.HasInwardTransport);
		}

		public void TestHasOutwardTransport()
		{
			Declaration.SG_OutwardTransportMode = "";
			AssertEquals(false, CusEntry.HasOutwardTransport);

			Declaration.SG_OutwardTransportMode = Core.Constants.TransportCodes.Road;
			AssertEquals(true, CusEntry.HasOutwardTransport);
		}

		public void TestIsContainerised()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType2;
			AssertEquals(false, CusEntry.IsContainerised);
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType3;
			AssertEquals(true, CusEntry.IsContainerised);

			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType5;
			AssertEquals(false, CusEntry.IsContainerised);
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType9;
			AssertEquals(true, CusEntry.IsContainerised);
		}

		public void TestIsSea()
		{
			AssertEquals("Empty Declaration", false, CusEntry.IsSea);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Inward Sea Declaration", true, CusEntry.IsSea);
			Declaration.JE_TransportMode = "";
			Declaration.SG_OutwardTransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Outward Sea Declaration", true, CusEntry.IsSea);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Declaration.SG_OutwardTransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Inward Air / Outward Sea Declaration", true, CusEntry.IsSea);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Declaration.SG_OutwardTransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("Inward Sea / Outward Air Declaration", true, CusEntry.IsSea);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Declaration.SG_OutwardTransportMode = Enterprise.Core.Constants.TransportModes.Road;
			AssertEquals("Inward Air / Outward Road Declaration", false, CusEntry.IsSea);
		}

		public void TestIsAir()
		{
			AssertEquals("Empty Declaration", false, CusEntry.IsAir);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("Inward Air Declaration", true, CusEntry.IsAir);
			Declaration.JE_TransportMode = "";
			Declaration.SG_OutwardTransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("Outward Air Declaration", true, CusEntry.IsAir);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Declaration.SG_OutwardTransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Inward Air / Outward Sea Declaration", true, CusEntry.IsAir);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Declaration.SG_OutwardTransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("Inward Sea / Outward Air Declaration", true, CusEntry.IsAir);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Declaration.SG_OutwardTransportMode = Enterprise.Core.Constants.TransportModes.Road;
			AssertEquals("Inward Sea / Outward Road Declaration", false, CusEntry.IsAir);
		}

		public void TestIsShortPayment()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();

			AssertEquals("Empty Declaration", false, CusEntry.IsShortPayment);
			Declaration.SG_US_NKPlaceOfReceipt = "SPNOSTK";
			AssertEquals(true, CusEntry.IsShortPayment);
			Declaration.SG_US_NKPlaceOfReceipt = "KW";
			AssertEquals(false, CusEntry.IsShortPayment);
			Declaration.SG_US_NKPlaceOfReceipt = "SPSTK";
			AssertEquals(true, CusEntry.IsShortPayment);
			Declaration.SG_US_NKPlaceOfReceipt = "IGDS";
			AssertEquals(false, CusEntry.IsShortPayment);
			Declaration.SG_US_NKPlaceOfReceipt = "SPIGDS";
			AssertEquals(true, CusEntry.IsShortPayment);
		}

		public void TestIsExempted()
		{
			AssertEquals("Empty Declaration", false, CusEntry.IsShortPayment);
			Declaration.SG_GoodsPreviouslyExemptedFromDuties = true;
			AssertEquals(true, CusEntry.IsExempted);
			Declaration.SG_GoodsPreviouslyExemptedFromDuties = false;
			Declaration.SG_US_NKPlaceOfCargoRelease = "KW";
			AssertEquals(false, CusEntry.IsExempted);
		}

		public void TestIsRecoveryPayment()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();

			AssertEquals("Empty Declaration", false, CusEntry.IsRecoveryPayment);
			Declaration.SG_US_NKPlaceOfReceipt = "RCNOSTK";
			AssertEquals(true, CusEntry.IsRecoveryPayment);
		}

		public void TestInwardTransportIdentifier()
		{
			AssertEquals("Empty Declaration", "", CusEntry.InwardTransportIdentifier);
			RefVessel testInwardVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.JE_VesselName = testInwardVessel.RV_Code;
			AssertEquals("Inward Transport ID - Vessel Name", testInwardVessel.RV_Code, CusEntry.InwardTransportIdentifier);

			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.JE_VoyageFlightNo = "C/F 201";
			Declaration.JE_Folio = "VGE-153";
			AssertEquals("Inward Transport ID - Charter Aircraft Rego", "VGE-153", CusEntry.InwardTransportIdentifier);

			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			Declaration.JE_VoyageFlightNo = "XJR 268";
			AssertEquals("Inward Transport ID - Vehicle Rego", "XJR 268", CusEntry.InwardTransportIdentifier);

			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals("Inward Transport ID", "", CusEntry.InwardTransportIdentifier);
		}

		public void TestInwardTransportIdentifierWhenNotARefFileVessel()
		{
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.JE_VesselName = "Any Vessel Name";
			AssertEquals("Inward Transport ID - Vessel Name", "Any Vessel Name", CusEntry.InwardTransportIdentifier);
			AssertEquals("CV is to default when a valid vessel is not found in the reference file", Core.Constants.VesselType.CargoVessel, CusEntry.InwardVesselType);
		}

		public void TestInwardTransportIdentifierWhenOutDec()
		{
			AssertEquals("Empty Declaration", "", CusEntry.InwardTransportIdentifier);

			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("Outdec with no 'Inward' transport required", "", CusEntry.InwardTransportIdentifier);
		}

		public void TestInwardTransportIdentifierOutDecWithInwardTransport()
		{
			AssertEquals("Empty Declaration", "", CusEntry.InwardTransportIdentifier);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.JE_Folio = "SQ218";
			AssertEquals("Inward Transport ID for OutDec when Inward mode required", "SQ218", CusEntry.InwardTransportIdentifier);
		}

		public void TestInwardJourneyIdentifier()
		{
			AssertEquals("Empty Declaration", "", CusEntry.InwardJourneyIdentifier);

			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.JE_VoyageFlightNo = "175E";
			AssertEquals("Inward Journey ID", "175E", CusEntry.InwardJourneyIdentifier);

			Declaration.JE_VoyageFlightNo = "";
			AssertEquals("Inward Journey ID", "NA", CusEntry.InwardJourneyIdentifier);

			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			Declaration.JE_VoyageFlightNo = "";
			AssertEquals("Inward Journey ID - Road vehicle registration is sent in Transport Id field, not here", "", CusEntry.InwardJourneyIdentifier);
		}

		public void TestInwardJourneyIdentifierWhenOutDec()
		{
			AssertEquals("Empty Declaration", "", CusEntry.InwardJourneyIdentifier);

			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("Outdec with no 'Inward' transport required", "", CusEntry.InwardJourneyIdentifier);
		}

		public void TestInwardJourneyIdentifierWhenOutDecWithInwardTransport()
		{
			AssertEquals("Empty Declaration", "", CusEntry.InwardJourneyIdentifier);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("Inward Journey ID when OutDec has inward mode", "NA", CusEntry.InwardJourneyIdentifier);
		}

		public void TestInwardVesselType()
		{
			AssertEquals("", CusEntry.InwardVesselType);

			RefVessel inwardVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Declaration.JE_VesselName = inwardVessel.RV_Code;

			inwardVessel.RV_VesselType = Core.Constants.VesselType.CargoVessel;
			AssertEquals(Core.Constants.VesselType.CargoVessel, CusEntry.InwardVesselType);

			inwardVessel.RV_VesselType = Core.Constants.VesselType.ContainerisedVessel;
			AssertEquals(Core.Constants.VesselType.CargoVessel, CusEntry.InwardVesselType);

			inwardVessel.RV_VesselType = Core.Constants.VesselType.NavalVessel;
			AssertEquals(Core.Constants.VesselType.NavalVessel, CusEntry.InwardVesselType);
		}

		public void TestPortOfLoading()
		{
			AssertEquals("Empty Declaration", "", CusEntry.PortOfLoading);

			Declaration.JE_RL_NKPortOfLoading = "JPTYO";
			AssertEquals("Port of Loading", "JPTYO", CusEntry.PortOfLoading);
		}

		public void TestPortOfDischarge()
		{
			AssertEquals("Empty Declaration", "", CusEntry.PortOfDischarge);

			Declaration.JE_RL_NKPortOfArrival = "SGSIN";
			AssertEquals("Port of Loading", "SGSIN", CusEntry.PortOfDischarge);
		}

		public void TestNextPortOfCall()
		{
			AssertEquals("Empty Declaration", "", CusEntry.NextPortOfCall);

			Declaration.SG_RL_NKNextPortOfCall = "HKHKG";
			AssertEquals("Next Port of Call", "HKHKG", CusEntry.NextPortOfCall);
		}

		public void TestFinalPortOfCall()
		{
			AssertEquals("Empty Declaration", "", CusEntry.FinalPortOfCall);

			Declaration.SG_RL_NKFinalPortOfCall = "CNCDO";
			AssertEquals("Final Port of Call", "CNCDO", CusEntry.FinalPortOfCall);
		}

		public void TestArrivalDate()
		{
			AssertEquals("Empty Declaration", ZDateTime.Empty, CusEntry.ArrivalDate);

			ZDateTime arrivalDate = new ZDateTime(2007, 04, 30);
			Declaration.JE_DateOfArrival = arrivalDate;
			AssertEquals("Arrival Date", arrivalDate, CusEntry.ArrivalDate);
		}

		public void TestDepartureDate()
		{
			AssertEquals("Empty Declaration", ZDateTime.Empty, CusEntry.DepartureDate);

			ZDate departureDate = new ZDate(2007, 04, 27);
			Declaration.JE_ExportDate = departureDate;
			AssertEquals("Departure Date", departureDate, CusEntry.DepartureDate);
		}

		public void TestIsStorageInFTZ()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();

			AssertEquals("Is Storage in Free Trade Zone", false, CusEntry.IsStorageInFTZ);

			Declaration.SG_US_NKPlaceOfStorage = "CZ";
			AssertEquals("Is Storage in Free Trade Zone", true, CusEntry.IsStorageInFTZ);

			Declaration.SG_US_NKPlaceOfStorage = "JZ";
			AssertEquals("Is Storage in Free Trade Zone", true, CusEntry.IsStorageInFTZ);

			Declaration.SG_US_NKPlaceOfStorage = "KZ";
			AssertEquals("Is Storage in Free Trade Zone", true, CusEntry.IsStorageInFTZ);

			Declaration.SG_US_NKPlaceOfStorage = "PPZ";
			AssertEquals("Is Storage in Free Trade Zone", true, CusEntry.IsStorageInFTZ);

			Declaration.SG_US_NKPlaceOfStorage = "SZ";
			AssertEquals("Is Storage in Free Trade Zone", true, CusEntry.IsStorageInFTZ);

			Declaration.SG_US_NKPlaceOfStorage = "AT1B";
			AssertEquals("Is Storage in Free Trade Zone", false, CusEntry.IsStorageInFTZ);
		}

		public void TestIsForStorage()
		{
			AssertEquals("Is for Storage", false, CusEntry.IsForStorage);
			Declaration.SG_US_NKPlaceOfStorage = "CZ";
			AssertEquals("Is for Storage", true, CusEntry.IsForStorage);
		}

		public void TestIsLiqourDeclaration()
		{
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			Declaration.DoMerge();
			AssertEquals("Empty Declaration", false, CusEntry.HasLiquorOrTobacco);

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();

			JobComInvoiceHeader testInvHead = Declaration.Invoices.AddNew();
			testInvHead.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine testInvLine = testInvHead.JobComInvoiceLines.AddNew();
			testInvLine.JI_Tariff = "21069030"; // Non-dairy creamer - same chapter as alcohol but not dutiable
			Declaration.DoMerge();
			AssertEquals("Same Chapter goods, but non alocohol tariff that does not attract duty", false, CusEntry.HasLiquorOrTobacco);

			testInvLine.JI_Tariff = "21069061";
			Declaration.DoMerge();
			AssertEquals("Liqour Declaration", true, CusEntry.HasLiquorOrTobacco);
		}

		public void TestIsTobaccoDeclaration()
		{
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			Declaration.DoMerge();

			AssertEquals("Empty Declaration", false, CusEntry.HasLiquorOrTobacco);

			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();

			JobComInvoiceHeader testInvHead = Declaration.Invoices.AddNew();
			testInvHead.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine testInvLine = testInvHead.JobComInvoiceLines.AddNew();
			testInvLine.JI_Tariff = "24012010";
			testInvLine.JI_CL = entryLine.PK;
			testInvLine.JI_LinePrice = 2500m;

			Declaration.DoMerge();
			entryLine.Fees.GetOrAddFeeByFeeType(Registry.EntryChargeTypeList.Codes.Duty).CF_ChargeAmount = 350m;
			entryLine.CL_DutyPercent = 10m;
			AssertEquals("Tobacco Declaration", true, CusEntry.HasLiquorOrTobacco);
		}

		public void TestReplacementPermitNumber()
		{
			AssertEquals("Blank Declaration", "", CusEntry.ReplacementPermitNumber);

			Declaration.SG_ReplacementPermitNo = "IG200089H";
			AssertEquals("Replacement Permit Number", "IG200089H", CusEntry.ReplacementPermitNumber);
		}

		#region Places Tests

		public void TestPlaceOfRelease()
		{
			Declaration.SG_US_NKPlaceOfCargoRelease = "CZ";
			AssertEquals("Place of Release", "CZ", CusEntry.PlaceOfRelease.Code);
		}

		public void TestPlaceOfStorage()
		{
			Declaration.SG_US_NKPlaceOfStorage = "EXAREA";
			AssertEquals("Place of Storage", "EXAREA", CusEntry.PlaceOfStorage);
		}

		public void TestPlaceOfReceipt()
		{
			Declaration.SG_US_NKPlaceOfReceipt = "AT1B";
			AssertEquals("Place of Receipt", "AT1B", CusEntry.PlaceOfReceipt.Code);
		}

		public void TestInwardVesselBerth()
		{
			Declaration.SG_US_NKInwardVesselBerth = "KW";
			AssertEquals("Inward Vessel Berth", "KW", CusEntry.InwardVesselBerth.Code);
		}

		public void TestOutwardVesselBerth()
		{
			Declaration.SG_US_NKOutwardVesselBerth = "PTGX";
			AssertEquals("Outward Vessel Berth", "PTGX", CusEntry.OutwardVesselBerth.Code);
		}

		#endregion

		public void TestCountryOfFinalDestination()
		{
			Declaration.SG_RN_NKFinalDestination = "JP";
			AssertEquals("Final Destination", "JP", CusEntry.CountryOfFinalDestination);
		}

		public void TestTonnageOfOutwardVessel()
		{
			AssertEquals("Outward Vessel Tonnage", 0, CusEntry.OutwardVesselNRT);

			RefVessel outwardVessel = Factory.New<RefVessel>();
			outwardVessel.RV_Code = "Hyogo Maru";
			outwardVessel.RV_VesselType = "CV";
			outwardVessel.RV_NetRegisterTon = 38000;
			Factory.Save();

			Declaration.SG_OutwardVesselName = "Hyogo Maru";

			AssertEquals("Outward Vessel Tonnage", 38000, CusEntry.OutwardVesselNRT);
		}

		public void TestOutwardTransportIdentifier()
		{
			AssertEquals("Empty Declaration", "", CusEntry.OutwardTransportIdentifier);
			RefVessel testOutwardVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.SG_OutwardVesselName = testOutwardVessel.RV_Code;
			AssertEquals("OutwardTransportIdentifier (Outward Vessel Name)", testOutwardVessel.RV_Code, CusEntry.OutwardTransportIdentifier);

			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.SG_OutwardFolio = "JJ-279";
			AssertEquals("OutwardTransportIdentifier (Charter Aircraft Registration)", "JJ-279", CusEntry.OutwardTransportIdentifier);

			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			Declaration.SG_OutwardVoyageFlightNo = "5J 294 S";
			AssertEquals("OutwardTransportIdentifier (Vehicle Registration)", "5J 294 S", CusEntry.OutwardTransportIdentifier);

			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals("OutwardTransportIdentifier", "", CusEntry.OutwardTransportIdentifier);
		}

		public void TestOutwardJourneyIdentifier()
		{
			AssertEquals("Empty Declaration", "", CusEntry.OutwardJourneyIdentifier);

			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.SG_OutwardVoyageFlightNo = "22S";
			AssertEquals("Outward JourneyIdentifier (Voyage No)", "22S", CusEntry.OutwardJourneyIdentifier);

			Declaration.SG_OutwardVoyageFlightNo = "";
			AssertEquals("Outward JourneyIdentifier", "NA", CusEntry.OutwardJourneyIdentifier);

			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			Declaration.SG_OutwardVoyageFlightNo = "";
			AssertEquals("Outward JourneyIdentifier", "", CusEntry.OutwardJourneyIdentifier);
		}

		public void TestIsInwardDeclaration()
		{
			Declaration.JE_MessageType = "";
			AssertEquals("Empty Declaration", false, CusEntry.IsInwardDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals("IsInwardDeclaration", false, CusEntry.IsInwardDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals("IsInwardDeclaration", true, CusEntry.IsInwardDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals("IsInwardDeclaration", false, CusEntry.IsInwardDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals("IsInwardDeclaration", true, CusEntry.IsInwardDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals("IsInwardDeclaration", false, CusEntry.IsInwardDeclaration);
		}

		public void TestIsOutwardDeclaration()
		{
			Declaration.JE_MessageType = "";
			AssertEquals("Empty Declaration", false, CusEntry.IsOutwardDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals("IsOutwardDeclaration", true, CusEntry.IsOutwardDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals("IsOutwardDeclaration", false, CusEntry.IsOutwardDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals("IsOutwardDeclaration", false, CusEntry.IsOutwardDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals("IsOutwardDeclaration", false, CusEntry.IsOutwardDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals("IsOutwardDeclaration", false, CusEntry.IsOutwardDeclaration);
		}

		public void TestIsTranshipmentDeclaration()
		{
			Declaration.JE_MessageType = "";
			AssertEquals("Empty Declaration", false, CusEntry.IsTranshipmentDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals("IsTranshipmentDeclaration", false, CusEntry.IsTranshipmentDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals("IsTranshipmentDeclaration", false, CusEntry.IsTranshipmentDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals("IsTranshipmentDeclaration", true, CusEntry.IsTranshipmentDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals("IsTranshipmentDeclaration", false, CusEntry.IsTranshipmentDeclaration);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals("IsTranshipmentDeclaration", false, CusEntry.IsTranshipmentDeclaration);
		}

		public void TestOutwardVesselType()
		{
			AssertEquals("", CusEntry.OutwardVesselType);

			RefVessel outwardVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Declaration.SG_OutwardVesselName = outwardVessel.RV_Code;
			outwardVessel.RV_VesselType = Core.Constants.VesselType.CargoVessel;

			AssertEquals(Core.Constants.VesselType.CargoVessel, CusEntry.OutwardVesselType);

			outwardVessel.RV_VesselType = Core.Constants.VesselType.ContainerisedVessel;
			AssertEquals(Core.Constants.VesselType.CargoVessel, CusEntry.OutwardVesselType);

			outwardVessel.RV_VesselType = Core.Constants.VesselType.NavalVessel;
			Declaration.SGE_OutwardVesselType = outwardVessel.RV_VesselType;
			AssertEquals(Core.Constants.VesselType.NavalVessel, CusEntry.OutwardVesselType);
		}

		public void TestOutwardVesselNationality()
		{
			AssertEquals("Empty Declaration", "", CusEntry.OutwardVesselNationality);

			RefVessel testOutwardVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Declaration.SG_OutwardVesselName = testOutwardVessel.RV_Code;
			AssertEquals("Vessel with no Country", "", CusEntry.OutwardVesselNationality);

			testOutwardVessel.RV_RN_NKCountryOfReg = Factory.LoadTop1<RefCountry>(new ZQuery()).Code;
			Declaration.SG_OutwardVesselName = testOutwardVessel.RV_Code;
			Declaration.SGE_RN_NKOutwardVesselNationality = testOutwardVessel.RV_RN_NKCountryOfReg;
			AssertEquals("Outward Vessel Nationality", testOutwardVessel.RV_RN_NKCountryOfReg, CusEntry.OutwardVesselNationality);
		}

		public void TestTowingVesselName()
		{
			AssertEquals("Empty Declaration", "", CusEntry.TowingVesselName);
			RefVessel towingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Declaration.SG_TowingVesselName = towingVessel.RV_Code;
			AssertEquals("Towing Vessel Name", towingVessel.RV_Code, CusEntry.TowingVesselName);
		}

		public void TestTowingVesselVoyageNo()
		{
			AssertEquals("Empty Declaration", "", CusEntry.TowingVesselVoyageNo);
			Declaration.SG_TowingVoyageNumber = "T00398";
			AssertEquals("Towing Voyage No", "T00398", CusEntry.TowingVesselVoyageNo);
		}

		public void TestDeclarationType()
		{
			Declaration.JE_MessageSubType = "";
			AssertEquals("Empty Declaration", "", CusEntry.DeclarationType);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BRE;
			AssertEquals("Declaration Type", DeclarationTypeCodeList.Codes.BRE, CusEntry.DeclarationType);
		}

		public void TestCargoPackingType()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType2;
			AssertEquals("Conventional", CargoPackingTypeCodeList.Codes.PackingType2, CusEntry.CargoPackingType);
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType4;
			AssertEquals("Packed to Bulk", CargoPackingTypeCodeList.Codes.PackingType4, CusEntry.CargoPackingType);

			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType5;
			AssertEquals("Other non-containerized", CargoPackingCodeList.Codes.PackingType5, CusEntry.CargoPackingType);
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType9;
			AssertEquals("Containerized", CargoPackingCodeList.Codes.PackingType9, CusEntry.CargoPackingType);
		}

		public void DeclarationlarationType()
		{
			AssertEquals("Default Declaration Type", "", CusEntry.DeclarationType);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			AssertEquals("Declaration Type", DeclarationTypeCodeList.Codes.GST, CusEntry.DeclarationType);
		}

		public void TestBGIndicator()
		{
			Declaration.JE_PaymentMethod = BGIndicatorCodeList.Codes.D;
			AssertEquals("Declaring Agent", BGIndicatorCodeList.Codes.D, CusEntry.BGIndicator);
			Declaration.JE_PaymentMethod = BGIndicatorCodeList.Codes.I;
			AssertEquals("Importer", BGIndicatorCodeList.Codes.I, CusEntry.BGIndicator);
		}

		public void TestPreviousPermitNumber()
		{
			AssertEquals("Previous Permit Number", "", CusEntry.PreviousPermitNumber);

			Declaration.SG_PreviousPermitNo = "BA94596288";
			AssertEquals("Previous Permit Number", "BA94596288", CusEntry.PreviousPermitNumber);
		}

		public void TestInwardTransportCode()
		{
			AssertEquals("Inward Transport", 0, CusEntry.InwardTransportCode);
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Inward Transport", SGConstants.TransportCodes.Air, CusEntry.InwardTransportCode);
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("Inward Transport", SGConstants.TransportCodes.Rail, CusEntry.InwardTransportCode);
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Inward Transport", SGConstants.TransportCodes.Sea, CusEntry.InwardTransportCode);
		}

		public void TestOutwardTransportCode()
		{
			AssertEquals("Outward Transport", 0, CusEntry.OutwardTransportCode);
			Declaration.SG_OutwardTransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Outward Transport - Air", SGConstants.TransportCodes.Air, CusEntry.OutwardTransportCode);
			Declaration.SG_OutwardTransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("Outward Transport - Rail", SGConstants.TransportCodes.Rail, CusEntry.OutwardTransportCode);
			Declaration.SG_OutwardTransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Outward Transport - Sea", SGConstants.TransportCodes.Sea, CusEntry.OutwardTransportCode);
			Declaration.SG_OutwardTransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("Outward Transport - Pipeline", SGConstants.TransportCodes.Pipeline, CusEntry.OutwardTransportCode);
			Declaration.SG_OutwardTransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("Outward Transport - Road", SGConstants.TransportCodes.Road, CusEntry.OutwardTransportCode);
			Declaration.SG_OutwardTransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals("Outward Transport - Mail", SGConstants.TransportCodes.Mail, CusEntry.OutwardTransportCode);
		}

		public void TestTradersRemarks()
		{
			var remark = Declaration.TradersRemarks.AddNew();
			remark.CSI_Description = "Test this Trader's remark";
			AssertEquals("Traders Remarks", "Test this Trader's remark", CusEntry.TradersRemarksForMessage.ToList().ElementAtOrDefault(0));
		}

		public void TestSupplyIndicator()
		{
			AssertEquals("Supply Indicator", "", CusEntry.SupplyIndicator);

			Declaration.SG_SupplyIndicator = SupplyIndicatorCodeList.Codes.Y;
			AssertEquals("Supply Indicator", "Y", CusEntry.SupplyIndicator);
		}

		public void TestTotalOuterPack()
		{
			AssertEquals("Total Outer Pack", 0m, CusEntry.TotalOuterPack);
			Declaration.JE_TotalNoOfPacks = 250;
			AssertEquals("Total Outer Pack", 250m, CusEntry.TotalOuterPack);

			Declaration.JE_TotalNoOfPacksDecimal = 10.3m;
			AssertEquals("Total Outer Pack", 10.3m, CusEntry.TotalOuterPack);
		}

		public void TestTotalOuterPackUnitOfQty()
		{
			AssertEquals("Total Outer Pack UQ", "PKG", CusEntry.TotalOuterPackUnitOfQty);
			Declaration.JE_TotalNoOfPacksPackType = "BOX";
			AssertEquals("Total Outer Pack UQ", "BOX", CusEntry.TotalOuterPackUnitOfQty);
		}

		public void TestTotalGrossWeight()
		{
			AssertEquals("Total Gross Weight", 0m, CusEntry.TotalGrossWeight);
			Declaration.JE_TotalWeight = 15.75m;
			AssertEquals("Total Gross Weight", 15.75m, CusEntry.TotalGrossWeight);
		}

		public void TestTotalGrossWeightUnitOfQty()
		{
			AssertEquals("Total Gross Weight UQ - default value", "KG", CusEntry.TotalGrossWeightUnitOfQty);
			Declaration.JE_TotalWeightUnit = "T";
			AssertEquals("Total Gross Weight UQ", "T", CusEntry.TotalGrossWeightUnitOfQty);
		}

		public void TestAdditionalRecipients()
		{
			AssertEquals(false, CusEntry.AdditionalRecipients.GetEnumerator().MoveNext());
			Declaration.SG_AdditionalRecipientID1 = "p17t010";
			foreach (ZString additionalRecipient in CusEntry.AdditionalRecipients)
			{
				AssertContains("1 Additional Recipient", Declaration.SG_AdditionalRecipientID1, additionalRecipient);
			}

			Declaration.SG_AdditionalRecipientID1 = "AddtlRecipient1";
			Declaration.SG_AdditionalRecipientID2 = "AddtlRecipient2";
			Declaration.SG_AdditionalRecipientID3 = "AddtlRecipient3";
			IEnumerator<ZString> recipientsEnumerator = CusEntry.AdditionalRecipients.GetEnumerator();
			recipientsEnumerator.MoveNext();
			AssertEquals("Additional Recipient 1", "AddtlRecipient1", recipientsEnumerator.Current.ToString());
			recipientsEnumerator.MoveNext();
			AssertEquals("Additional Recipient 2", "AddtlRecipient2", recipientsEnumerator.Current.ToString());
			recipientsEnumerator.MoveNext();
			AssertEquals("Additional Recipient 3", "AddtlRecipient3", recipientsEnumerator.Current.ToString());
		}

		public void TestIsTemporaryConsignment()
		{
			AssertEquals("Blank Dec - Is Temporary Consignment", false, ((IINPDEC)EntryHeader).IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCI;
			AssertEquals("Is Temporary Consignment", false, ((IINPDEC)EntryHeader).IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCE;
			AssertEquals("Is Temporary Consignment", true, ((IINPDEC)EntryHeader).IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCS;
			AssertEquals("Is Temporary Consignment", true, ((IINPDEC)EntryHeader).IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCR;
			AssertEquals("Is Temporary Consignment", true, ((IINPDEC)EntryHeader).IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCO;
			AssertEquals("Is Temporary Consignment", true, ((IINPDEC)EntryHeader).IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			AssertEquals("Is Temporary Consignment", false, ((IINPDEC)EntryHeader).IsTemporaryConsignment);
		}

		public void TestIs2bStoredBWCY()
		{
			AssertEquals("Is to be stored in BW class 2 yard", false, ((IINPDEC)EntryHeader).Is2bStoredBWCY);
			AssertEquals("Is to be stored in BW class 2 yard", false, ((ITNPDEC)EntryHeader).Is2bStoredBWCY);

			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "AT1B", "AT1B", "AIRPORT TERMINAL 1 BOND", null);
			Factory.Save();
			var airportTerminalBond = Universal.ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "AT1B", Core.Constants.CountryCodes.Singapore,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Now);
			Declaration.SG_US_NKPlaceOfStorage = airportTerminalBond.ZZD_Code; // "AT1B";
			AssertEquals("AirportTerminalBond", false, ((IINPDEC)EntryHeader).Is2bStoredBWCY);
			AssertEquals("AirportTerminalBond", false, ((ITNPDEC)EntryHeader).Is2bStoredBWCY);

			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "G9847TZ", "BWCY", "Dangerous Goods Store Wharf 19 Changi");
			Factory.Save();
			Declaration.SG_US_NKPlaceOfStorage = "G9847TZ";
			AssertEquals("Bonded Warehouse Class 2 Yard", true, ((IINPDEC)EntryHeader).Is2bStoredBWCY);
			AssertEquals("Bonded Warehouse Class 2 Yard", true, ((ITNPDEC)EntryHeader).Is2bStoredBWCY);
		}

		public void TestEndDateOfTemporaryImport()
		{
			AssertEquals("End date of temporary import", ZDate.Empty, ((IINPDEC)EntryHeader).EndDateOfTemporaryImport);
			ZDate testEndDate = new ZDate(2007, 10, 31);
			Declaration.SG_EndDateTempImport = testEndDate;
			AssertEquals("End date of temporary import", testEndDate, ((IINPDEC)EntryHeader).EndDateOfTemporaryImport);
		}

		public void TestNumberOfRequestsForUpdate()
		{
			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, Cuspmt09bMessageProcessor.MessageType));
			AssertEquals(1, CusEntry.NumberOfRequestsForUpdate);

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType, "RFF+ABT:IM2G002174Z"));
			AssertEquals(1, CusEntry.NumberOfRequestsForUpdate);

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType, "RFF+ABT:IM2G002174Z"));
			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType, "RFF+ABT:IM2G002175F"));

			EntryHeader.Messages.Add(CreateXmlEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType, "<cbc:PermitNumber>IM2G002174Z</cbc:PermitNumber>"));
			EntryHeader.Messages.Add(CreateXmlEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType, "<cbc:PermitNumber>IM2G002175F</cbc:PermitNumber>"));

			AssertEquals(3, CusEntry.NumberOfRequestsForUpdate);

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cusres09bMessageProcessor.MessageType, "RFF+ABT:IM2G002174Z"));
			System.Threading.Thread.Sleep(1000);

			EntryHeader.Messages.Add(CreateEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType, "RFF+ABT:IM2G002174Z"));
			AssertEquals(1, CusEntry.NumberOfRequestsForUpdate);

			EntryHeader.Messages.Add(CreateXmlEDIMessage(EDIMessage.Direction.Receive, EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType, "<cbc:PermitNumber>IM2G002174Z</cbc:PermitNumber>"));
			AssertEquals(2, CusEntry.NumberOfRequestsForUpdate);
		}

		EDIMessage CreateEDIMessage(string direction, string status, string messageType)
		{
			return CreateEDIMessageCore<EDIMessage>(direction, ApplicationCodeList.Codes.SGCustomsTradenet4, status, messageType, string.Empty);
		}

		EDIMessage CreateEDIMessage(string direction, string status, string messageType, string messageText)
		{
			return CreateEDIMessageCore<EDIMessage>(direction, ApplicationCodeList.Codes.SGCustomsTradenet4, status, messageType, messageText);
		}

		SGXmlEDIMessage CreateXmlEDIMessage(string direction, string status, string messageType, string messageText)
		{
			return CreateEDIMessageCore<SGXmlEDIMessage>(direction, ApplicationCodeList.Codes.SGCustomsTradenetXML, status, messageType, messageText);
		}

		T CreateEDIMessageCore<T>(string direction, string applicationCode, string status, string messageType, string messageText) where T : EDIMessage
		{
			var result = Factory.New<T>();

			result.EM_ApplicationCode = applicationCode;
			result.EM_ReceiveTransmit = direction;
			result.EM_Status = status;
			result.EM_MessageType = messageType;
			result.EM_SystemCreateTimeUtc = ZDateTime.Now;
			result.EM_MessageText = messageText;

			return result;
		}

		public void TestIsSeaStoreDeclaration()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			AssertEquals(false, CusEntry.IsSeaStoreDeclaration);
			Declaration.SG_IsSeaStore = true;
			AssertEquals(true, CusEntry.IsSeaStoreDeclaration);
		}

		public void TestIsSeaStoreDeclarationTN41()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "", "420", "", "3000", "SEASTORE (OUT APS)", "OUT", group: "APS");
			var procedure1Attribute1 = helper.CreateRefCusProcedureAttribute(procedure1.PK, Universal.AttributeNames.Codes.ISSEASTORE, "Y");

			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			AssertEquals(false, CusEntry.IsSeaStoreDeclaration);
			var testCPC = Factory.New<SGCPC>();
			testCPC.B7_ParentID = Declaration.PK;
			testCPC.B7_ParentTableCode = Declaration.TablePrefix;
			testCPC.SG_CPCCode = "4203000";
			testCPC.SG_PC1 = "12";
			testCPC.SG_PC2 = "7";
			IEnumerable<ICusCPC> cpcs = CusEntry.CPCs;
			AssertEquals("For TradeNet 4.1, SeaStores are now sent in the new CPC code values", true, CusEntry.IsSeaStoreDeclaration);
		}

		public void TestNumberOfCrew()
		{
			AssertEquals(0, CusEntry.NumberOfCrew);
			Declaration.SG_NoOfCrew = 12;
			AssertEquals(12, CusEntry.NumberOfCrew);
		}

		public void TestVoyageDuration()
		{
			AssertEquals(0, CusEntry.VoyageDuration);
			Declaration.SG_VoyageDuration = 12;
			AssertEquals(12, CusEntry.VoyageDuration);
		}

		public void TestInwardMasterBill()
		{
			AssertEquals("Inward MasterBill", "", CusEntry.InwardMasterBill);
			Declaration.JE_MasterBill = "081-003985793";
			AssertEquals("Inward MasterBill", "081-003985793", CusEntry.InwardMasterBill);

			Declaration.SG_IsInwardHandCarried = true;
			AssertEquals("Inward MasterBill", "081-003985793", CusEntry.InwardMasterBill);

			Declaration.JE_MasterBill = "";
			AssertEquals("Inward MasterBill", "HandCarried", CusEntry.InwardMasterBill);
		}

		public void TestInwardHouseBill()
		{
			AssertEquals("Inward HouseBill", "", CusEntry.InwardHouseBill);
			Declaration.JE_HouseBill = "TestHB";
			AssertEquals("Inward HouseBill", "TestHB", CusEntry.InwardHouseBill);
		}

		public void TestOutwardMasterBill()
		{
			AssertEquals("Outward MasterBill", "", CusEntry.OutwardMasterBill);
			Declaration.SG_OutwardMAWB = "OUT-MAWB";
			AssertEquals("Outward MasterBill - should strip out display formatting characters", "OUTMAWB", CusEntry.OutwardMasterBill);

			Declaration.SG_IsOutwardHandCarried = true;
			AssertEquals("Outward MasterBill", "OUTMAWB", CusEntry.OutwardMasterBill);

			Declaration.SG_OutwardMAWB = "";
			AssertEquals("Outward MasterBill", "HandCarried", CusEntry.OutwardMasterBill);
		}

		public void TestOutwardHouseBill()
		{
			AssertEquals("Outward HouseBill", "", CusEntry.OutwardHouseBill);
			Declaration.SG_OutwardHAWB = "OUT-HAWB";
			AssertEquals("Outward MasterBill", "OUT-HAWB", CusEntry.OutwardHouseBill);
		}

		public void TestIsDG()
		{
			AssertEquals("Has DG Indicator set", false, CusEntry.IsDG);

			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			CusEntryLine entryLine = EntryHeader.MergedLines.AddNew();
			JobComInvoiceHeader testInvHead = Declaration.Invoices.AddNew();
			JobComInvoiceLine testInvLine = testInvHead.JobComInvoiceLines.AddNew();
			testInvLine.JI_HazMatCodeQualifier = DGIndicatorCodeList.Codes.Y;
			Declaration.DoMerge();
			AssertEquals("Has DG Indicator set", true, CusEntry.IsDG);
		}

		public void TestIsReleasedInLicensedPremiseExclBWCY()
		{
			AssertEquals("Is Released in Licensed Premises", false, CusEntry.IsReleasedInLicensedPremiseExclBWCY);

			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "AT1B", "AT1B", "AIRPORT TERMINAL 1 BOND", null);
			Factory.Save();
			var airportTerminalBond = Factory.LoadTop1<Universal.ZZRefCusCodeListCombined>(new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, "AT1B"));
			Declaration.SG_US_NKPlaceOfCargoRelease = airportTerminalBond.ZZD_Code; // "AT1B";
			AssertEquals("AirportTerminalBond", false, CusEntry.IsReleasedInLicensedPremiseExclBWCY);

			var testLicensedWarehouse = SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "ABCD", "LW", "Alcohol Store 1 Changi");
			Factory.Save();

			Declaration.SG_US_NKPlaceOfCargoRelease = "ABCD";
			AssertEquals("Licensed Warehouse", true, CusEntry.IsReleasedInLicensedPremiseExclBWCY);

			var testBondedWarehouse = SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BW001", "BW", "Quon Lee Cigarettes Changi");
			Factory.Save();

			Declaration.SG_US_NKPlaceOfCargoRelease = "BW001";
			AssertEquals("Bonded Warehouse", true, CusEntry.IsReleasedInLicensedPremiseExclBWCY);

			var testClass2YardWarehouse = SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "G9847TZ", "BWCY", "Dangerous Goods Store Wharf 19 Changi", null);
			Declaration.SG_US_NKPlaceOfCargoRelease = "G9847TZ";
			AssertEquals("Bonded Warehouse Class 2 Yard", false, CusEntry.IsReleasedInLicensedPremiseExclBWCY);
		}

		public void TestIsReceiptInLicensedPremise()
		{
			AssertEquals("Is from Licensed Premises", false, ((IOUTDEC)EntryHeader).IsReceiptInLicensedPremise);

			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "AT1B", "AT1B", "AIRPORT TERMINAL 1 BOND", null);
			Factory.Save();

			var airportTerminalBond = Universal.ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "AT1B", Core.Constants.CountryCodes.Singapore,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Now);
			Declaration.SG_US_NKPlaceOfReceipt = airportTerminalBond.ZZD_Code;
			AssertEquals("AirportTerminalBond", false, ((IOUTDEC)EntryHeader).IsReceiptInLicensedPremise);

			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "ABCD", "LW", "Alcohol Store 1 Changi");
			Factory.Save();
			Declaration.SG_US_NKPlaceOfReceipt = "ABCD";
			AssertEquals("Is from Licensed Warehouse", true, ((IOUTDEC)EntryHeader).IsReceiptInLicensedPremise);
		}

		public void TestDeclarantId()
		{
			AssertEquals("Declarant ID", "", CusEntry.DeclarantId);
			GlbStaff declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			declarant.GS_Code = "GHW";
			SGGlbStaffWrapper.Get(declarant).Tradenetv4Password.GP_UserID = "v13t001";
			Factory.Save();
			Declaration.JE_GS_NKCusAgent = declarant.GS_Code;
			AssertEquals("Declarant ID", "v13t001", CusEntry.DeclarantId);
		}

		public void TestLicencesAndDocumentsWhenEmpty()
		{
			IEnumerable<ICusDocument> licences = CusEntry.LicencesAndDocuments;
			foreach (ICusDocument licence in licences)
			{
				Assert("should not get in here", false);
			}

			Assert("empty collection of licenses should not cause exception", true);
		}

		public void TestLicencesAndDocuments()
		{
			CusCodeData testLicence = Factory.NewWithValidTestData<CALicenceNumber>();
			testLicence.CY_Type = CusCodeDataTypeList.Codes.CALicenceNumber;
			testLicence.CY_Data = "LIC0001";
			testLicence.CY_ParentTableCode = "JE";
			testLicence.CY_ParentID = Declaration.PK;
			IEnumerable<ICusDocument> licences = CusEntry.LicencesAndDocuments;
			foreach (ICusDocument licence in licences)
			{
				AssertEquals("LIC0001", licence.LicenceNumber);
			}
		}

		public void TestMultipleLicencesAndDocuments()
		{
			CusCodeData testLicence1 = Factory.NewWithValidTestData<CALicenceNumber>();
			testLicence1.CY_Type = CusCodeDataTypeList.Codes.CALicenceNumber;
			testLicence1.CY_Data = "LIC0001";
			testLicence1.CY_ParentTableCode = "JE";
			testLicence1.CY_ParentID = Declaration.PK;

			CusCodeData testLicence2 = Factory.NewWithValidTestData<CALicenceNumber>();
			testLicence2.CY_Type = CusCodeDataTypeList.Codes.CALicenceNumber;
			testLicence2.CY_Data = "LIC0002";
			testLicence2.CY_ParentTableCode = "JE";
			testLicence2.CY_ParentID = Declaration.PK;

			CusCodeData testLicence3 = Factory.NewWithValidTestData<CALicenceNumber>();
			testLicence3.CY_Type = CusCodeDataTypeList.Codes.CALicenceNumber;
			testLicence3.CY_Data = "LIC0003";
			testLicence3.CY_ParentTableCode = "JE";
			testLicence3.CY_ParentID = Declaration.PK;

			int count = 0;
			string output = "";
			List<ICusDocument> licences = new List<ICusDocument>(CusEntry.LicencesAndDocuments);
			licences.Sort(delegate(ICusDocument lhs, ICusDocument rhs)
			{ return lhs.LicenceNumber.CompareTo(rhs.LicenceNumber); });
			foreach (ICusDocument licence in licences)
			{
				count++;
				output = output + licence.LicenceNumber;
			}

			AssertEquals("Should be 3 licences", 3, count);
			AssertEquals("3 licences", "LIC0001LIC0002LIC0003", output);
		}

		public void TestCPCsWhenEmpty()
		{
			IEnumerable<ICusCPC> cpcs = CusEntry.CPCs;
			foreach (ICusCPC cpc in cpcs)
			{
				Assert("should not get in here", false);
			}

			Assert("empty collection of CPCs should not cause exception", true);
		}

		public void TestCPCs()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "", "420", "", "3000", "SEASTORE (OUT APS)", "OUT", group: "APS");
			Factory.Save();

			Declaration.JE_MessageType = "OUT";
			Declaration.JE_MessageSubType = "APS";
			var testCPC = Factory.New<SGCPC>();
			testCPC.B7_ParentID = Declaration.PK;
			testCPC.B7_ParentTableCode = Declaration.TablePrefix;
			testCPC.SG_CPCCode = "4203000";
			testCPC.SG_PC1 = "12";
			testCPC.SG_PC2 = "7";
			IEnumerable<ICusCPC> cpcs = CusEntry.CPCs;
			foreach (ICusCPC cpc in cpcs)
			{
				AssertEquals("CPC Code", "4203000", cpc.APCCodeName);
				foreach (ICusProcessingCodes occurrence in cpc.PCOccurrences)
				{
					AssertEquals("SEASTORE - crew number", "12", occurrence.ProcessingCode1);
					AssertEquals("SEASTORE - voyage duration", "7", occurrence.ProcessingCode2);
				}
			}
		}

		public void TestMultipleCPCs()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "", "440", "", "2000", "CWC (OUT DRT)", "OUT", group: "DRT");
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "", "440", "", "1000", "AEO (OUT DRT)", "OUT", group: "DRT");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "", "440", "", "6000", "CNB (OUT DRT)", "OUT", group: "DRT");
			Factory.Save();

			Declaration.JE_MessageType = "OUT";
			Declaration.JE_MessageSubType = "DRT";
			var testCPC1 = Factory.New<SGCPC>();
			testCPC1.B7_ParentID = Declaration.PK;
			testCPC1.B7_ParentTableCode = Declaration.TablePrefix;
			testCPC1.SG_CPCCode = "4402000";
			testCPC1.SG_PC1 = "8";
			testCPC1.SG_PC2 = "23";

			var testCPC2 = Factory.New<SGCPC>();
			testCPC2.B7_ParentID = Declaration.PK;
			testCPC2.B7_ParentTableCode = Declaration.TablePrefix;
			testCPC2.SG_CPCCode = "4401000";
			testCPC2.SG_PC1 = "CN";

			var testCPC3 = Factory.New<SGCPC>();
			testCPC3.B7_ParentID = Declaration.PK;
			testCPC3.B7_ParentTableCode = Declaration.TablePrefix;
			testCPC3.SG_CPCCode = "4406000";
			testCPC3.SG_PC1 = "PCP-0394839";
			testCPC3.SG_PC2 = "38.9%";
			testCPC3.SG_PC3 = "DG-459983845";

			int count = 0;
			IEnumerable<ICusCPC> cpcs = CusEntry.CPCs;
			foreach (ICusCPC cpc in cpcs)
			{
				count++;
				if (count == 1)
				{
					AssertEquals("CPC Code", "4402000", cpc.APCCodeName);
					foreach (ICusProcessingCodes occurrence in cpc.PCOccurrences)
					{
						AssertEquals("SEASTORE - crew number", "8", occurrence.ProcessingCode1);
						AssertEquals("SEASTORE - voyage duration", "23", occurrence.ProcessingCode2);
					}
				}
				else if (count == 2)
				{
					AssertEquals("CPC Code", "4401000", cpc.APCCodeName);
					foreach (ICusProcessingCodes occurrence in cpc.PCOccurrences)
					{
						AssertEquals("AEO - country", "CN", occurrence.ProcessingCode1);
					}
				}
				else if (count == 3)
				{
					AssertEquals("CPC Code", "4406000", cpc.APCCodeName);
					foreach (ICusProcessingCodes occurrence in cpc.PCOccurrences)
					{
						AssertEquals("CWC - ", "PCP-0394839", occurrence.ProcessingCode1);
						AssertEquals("CWC - ", "38.9%", occurrence.ProcessingCode2);
						AssertEquals("CWC - ", "DG-459983845", occurrence.ProcessingCode3);
					}
				}
			}

			AssertEquals("Should be 3 Customs Procedure Codes", 3, count);
		}

		public void TestContainers()
		{
			RefContainer fortyFooter = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			CusContainer testContainer = Factory.NewWithValidTestData<CusContainer>();
			testContainer.CO_ContainerNumber = "HJMU03948571";
			testContainer.CO_FCL_LCL_AIR = "FCL";
			testContainer.CO_JE = Declaration.PK;
			testContainer.CO_Seal = "S-45983X";
			testContainer.CO_RC = fortyFooter.PK;
			testContainer.CO_Weight = 25m;
			testContainer.CO_WeightUQ = "T";
			Declaration.CusContainers.Add(testContainer);

			IEnumerable<ICusContainer> containers = CusEntry.Containers;
			foreach (ICusContainer container in containers)
			{
				AssertEquals("HJMU03948571", container.ContainerNumber);
				AssertEquals(40, container.ContainerSize);
				AssertEquals("FCL", container.ContainerType);
				AssertEquals("S-45983X", container.SealNumber);
				AssertEquals(25m, container.ContainerWeight);
				AssertEquals("T", container.ContainerWeightUnit);
			}
		}

		public void TestStartDateOfCargoRemoval()
		{
			AssertEquals("Start Date of Cargo Removal", ZDate.Empty, ((ITNPDEC)EntryHeader).StartDateOfCargoRemoval);
			ZDate testDate = new ZDate(2007, 05, 29);
			Declaration.SG_RemovalStartDate = testDate;
			AssertEquals("Start Date of Cargo Removal", testDate, ((ITNPDEC)EntryHeader).StartDateOfCargoRemoval);
		}

		#region Organisations Tests

		public void TestImporter()
		{
			AssertEquals("Importer is null", null, CusEntry.Importer);

			OrgHeader testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.OH_FullName = "Singapore Test Importer Pte. Ltd.";
			testImporter.MainAddress.OA_Address1 = "Changi Airport";
			testImporter.MainAddress.OA_Address2 = "Building 3C";
			testImporter.MainAddress.OA_City = "Singapore";
			testImporter.MainAddress.OA_PostCode = "59211";
			testImporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "198300969N");

			Declaration.JE_OH_Importer = testImporter.PK;
			AssertEquals("Importer Name", "Singapore Test Importer Pte. Ltd.", CusEntry.Importer.Name);
			AssertEquals("Importer Address", "CHANGI AIRPORT BUILDING 3C SINGAPORE 59211", CusEntry.Importer.Address.FullAddress);
			AssertEquals("Importer UEN should be returned", "198300969N", CusEntry.Importer.UEN);

			Declaration.SG_ImporterNameOverride = "Name Override";
			AssertEquals("Importer Name", "Name Override", CusEntry.Importer.Name);
		}

		[TestDate(2009, 1, 1)]
		public void TestImporterPostUENCutOver()
		{
			AssertEquals("Importer is null", null, CusEntry.Importer);

			OrgHeader testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.OH_FullName = "Singapore Test Importer Pte. Ltd.";
			testImporter.MainAddress.OA_Address1 = "Changi Airport";
			testImporter.MainAddress.OA_Address2 = "Building 3C";
			testImporter.MainAddress.OA_City = "Singapore";
			testImporter.MainAddress.OA_PostCode = "59211";
			testImporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "18938475932W");
			testImporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "198300969N");

			Declaration.JE_OH_Importer = testImporter.PK;
			AssertEquals("Importer Name", "Singapore Test Importer Pte. Ltd.", CusEntry.Importer.Name);
			AssertEquals("Importer Address", "CHANGI AIRPORT BUILDING 3C SINGAPORE 59211", CusEntry.Importer.Address.FullAddress);
			AssertEquals("Importer UEN should be returned", "198300969N", CusEntry.Importer.UEN);

			Declaration.SG_ImporterNameOverride = "Name Override";
			AssertEquals("Importer Name", "Name Override", CusEntry.Importer.Name);
		}

		public void TestExporter()
		{
			AssertEquals("Exporter s/b null", null, CusEntry.Exporter);

			OrgHeader testExporter = Factory.NewWithValidTestData<OrgHeader>();
			testExporter.OH_FullName = "Singapore Exporter (Pte.) Ltd.";
			testExporter.MainAddress.OA_Address1 = "1792 Raffles Ave";
			testExporter.MainAddress.OA_Address2 = "Central";
			testExporter.MainAddress.OA_City = "Singapore";
			testExporter.MainAddress.OA_PostCode = "59200";
			testExporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "199601129Z");

			Declaration.JE_OH_Exporter = testExporter.PK;
			AssertEquals("Exporter Name", "Singapore Exporter (Pte.) Ltd.", CusEntry.Exporter.Name);
			AssertEquals("Exporter Address", "1792 RAFFLES AVE CENTRAL SINGAPORE 59200", CusEntry.Exporter.Address.FullAddress);
			AssertEquals("Exporter UEN should be returned", "199601129Z", CusEntry.Exporter.UEN);
		}

		[TestDate(2009, 1, 1)]
		public void TestExporterPostUENCutOver()
		{
			AssertEquals("Exporter s/b null", null, CusEntry.Exporter);

			OrgHeader testExporter = Factory.NewWithValidTestData<OrgHeader>();
			testExporter.OH_FullName = "Singapore Exporter (Pte.) Ltd.";
			testExporter.MainAddress.OA_Address1 = "1792 Raffles Ave";
			testExporter.MainAddress.OA_Address2 = "Central";
			testExporter.MainAddress.OA_City = "Singapore";
			testExporter.MainAddress.OA_PostCode = "59200";
			testExporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "10088349583A");
			testExporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "199601129Z");

			Declaration.JE_OH_Exporter = testExporter.PK;
			AssertEquals("Exporter Name", "Singapore Exporter (Pte.) Ltd.", CusEntry.Exporter.Name);
			AssertEquals("Exporter Address", "1792 RAFFLES AVE CENTRAL SINGAPORE 59200", CusEntry.Exporter.Address.FullAddress);
			AssertEquals("Exporter UEN should be returned", "199601129Z", CusEntry.Exporter.UEN);
		}

		public void TestForwarder()
		{
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertEquals("Forwarder s/b null", null, CusEntry.FreightForwarder);

			OrgHeader forwarder = Factory.New<OrgHeader>();
			forwarder.OH_FullName = "Singapore Test Forwarder Pte. Ltd.";
			forwarder.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "198801674D");

			Declaration.JE_OH_Forwarder = forwarder.PK;
			AssertEquals("Singapore Test Forwarder Pte. Ltd.", CusEntry.FreightForwarder.Name);
			AssertEquals("Forwarder UEN should be returned", "198801674D", CusEntry.FreightForwarder.UEN);
		}

		[TestDate(2009, 1, 1)]
		public void TestForwarderPostUENCutOver()
		{
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertEquals("Forwarder s/b null", null, CusEntry.FreightForwarder);

			OrgHeader forwarder = Factory.New<OrgHeader>();
			forwarder.OH_FullName = "Singapore Test Forwarder Pte. Ltd.";
			forwarder.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "18932275932C");
			forwarder.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "198801674D");

			Declaration.JE_OH_Forwarder = forwarder.PK;
			AssertEquals("Singapore Test Forwarder Pte. Ltd.", CusEntry.FreightForwarder.Name);
			AssertEquals("Forwarder UEN should be returned", "198801674D", CusEntry.FreightForwarder.UEN);
		}

		public void TestInwardCarrierAgent()
		{
			AssertEquals("Inward Carrier Agent", null, CusEntry.InwardCarrierAgent);

			OrgHeader testInwardCarrierAgent = Factory.NewWithValidTestData<OrgHeader>();
			testInwardCarrierAgent.OH_FullName = "Asian Shipping Pte. Ltd.";
			testInwardCarrierAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "42432211132B");
			testInwardCarrierAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "199002561R");

			Declaration.JE_OH_InwardCarrierAgent = testInwardCarrierAgent.PK;
			AssertEquals("Inward Carrier Agent Name", "Asian Shipping Pte. Ltd.", CusEntry.InwardCarrierAgent.Name);
			AssertEquals("Inward Carrier Agent UEN should be returned", "199002561R", CusEntry.InwardCarrierAgent.UEN);
		}

		public void TestOutwardCarrierAgent()
		{
			AssertEquals("Outward Carrier Agent", null, CusEntry.OutwardCarrierAgent);

			OrgHeader testOutwardCarrierAgent = Factory.NewWithValidTestData<OrgHeader>();
			testOutwardCarrierAgent.OH_FullName = "P&O East Asia";
			testOutwardCarrierAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "198203607W");

			Declaration.OutwardShippingLineForwarderPK = testOutwardCarrierAgent.PK;
			AssertEquals("Outward Carrier Agent Name", "P&O East Asia", CusEntry.OutwardCarrierAgent.Name);
			AssertEquals("Outward Carrier Agent UEN should be returned", "198203607W", CusEntry.OutwardCarrierAgent.UEN);
		}

		[TestDate(2009, 1, 1)]
		public void TestOutwardCarrierAgentPostUENChange()
		{
			AssertEquals("Outward Carrier Agent", null, CusEntry.OutwardCarrierAgent);

			OrgHeader testOutwardCarrierAgent = Factory.NewWithValidTestData<OrgHeader>();
			testOutwardCarrierAgent.OH_FullName = "P&O East Asia";
			testOutwardCarrierAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "79332216922J");
			testOutwardCarrierAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "198203607W");

			Declaration.OutwardShippingLineForwarderPK = testOutwardCarrierAgent.PK;
			AssertEquals("Outward Carrier Agent Name", "P&O East Asia", CusEntry.OutwardCarrierAgent.Name);
			AssertEquals("Outward Carrier Agent UEN should be returned", "198203607W", CusEntry.OutwardCarrierAgent.UEN);
		}

		public void TestManufacturer()
		{
			AssertEquals("Manufacturer", null, CusEntry.Manufacturer);

			OrgHeader testManufacturer = Factory.NewWithValidTestData<OrgHeader>();
			testManufacturer.OH_FullName = "ABC Manufacturer";
			testManufacturer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "199604017Z");

			Declaration.JE_OH_Manufacturer = testManufacturer.PK;
			AssertEquals("Manufacturer", "ABC Manufacturer", CusEntry.Manufacturer.Name);
			AssertEquals("Manufacturer UEN should be returned", "199604017Z", CusEntry.Manufacturer.UEN);
		}

		[TestDate(2009, 1, 1)]
		public void TestManufacturerPostUENChange()
		{
			AssertEquals("Manufacturer", null, CusEntry.Manufacturer);

			OrgHeader testManufacturer = Factory.NewWithValidTestData<OrgHeader>();
			testManufacturer.OH_FullName = "ABC Manufacturer";
			testManufacturer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "55912216852X");
			testManufacturer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "199604017Z");

			Declaration.JE_OH_Manufacturer = testManufacturer.PK;
			AssertEquals("Manufacturer", "ABC Manufacturer", CusEntry.Manufacturer.Name);
			AssertEquals("Manufacturer UEN should be returned", "199604017Z", CusEntry.Manufacturer.UEN);
		}

		public void TestConsignee()
		{
			AssertEquals("Consignee", null, CusEntry.Consignee);

			OrgHeader testConsignee = Factory.NewWithValidTestData<OrgHeader>();
			testConsignee.OH_FullName = "Lee Qwon Yue";
			testConsignee.MainAddress.OA_Address1 = "Apartment 395, Lee Qwon Yue Complex";
			testConsignee.MainAddress.OA_Address2 = "2275 Qwon Yue St.";
			testConsignee.MainAddress.OA_City = "Singapore";
			testConsignee.MainAddress.OA_PostCode = "55999";

			Declaration.JE_OH_Consignee = testConsignee.PK;
			AssertEquals("Consignee Name", "Lee Qwon Yue", CusEntry.Consignee.Name);
			AssertEquals("Consignee Address", "APARTMENT 395, LEE QWON YUE COMPLEX 2275 QWON YUE ST. SINGAPORE 55999", CusEntry.Consignee.Address.FullAddress);
			AssertEquals("Consignee UEN - with no CCD code should not crash", "", CusEntry.Consignee.UEN);

			testConsignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "200000000C");
			AssertEquals("Consignee UEN", "200000000C", CusEntry.Consignee.UEN);
		}

		public void TestEndUser()
		{
			AssertEquals("EndUser", null, CusEntry.EndUser);

			OrgHeader testEndUser = Factory.NewWithValidTestData<OrgHeader>();
			testEndUser.OH_FullName = "Singapore EndUser (Pte.) Ltd.";
			testEndUser.MainAddress.OA_Address1 = "156 Malay Ave";
			testEndUser.MainAddress.OA_Address2 = "Downtown";
			testEndUser.MainAddress.OA_City = "Singapore";
			testEndUser.MainAddress.OA_PostCode = "54319";

			Declaration.JE_OH_Buyer = testEndUser.PK;
			AssertEquals("EndUser Name", "Singapore EndUser (Pte.) Ltd.", CusEntry.EndUser.Name);
			AssertEquals("EndUser Address", "156 MALAY AVE DOWNTOWN SINGAPORE 54319", CusEntry.EndUser.Address.FullAddress);
			AssertEquals("EndUser UEN - with no CCD code set should not crash", "", CusEntry.EndUser.UEN);

			testEndUser.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "22961349C");
			AssertEquals("EndUser UEN", "22961349C", CusEntry.EndUser.UEN);
		}

		public void TestHandlingAgent()
		{
			AssertEquals("Handling Agent", null, ((ITNPDEC)EntryHeader).HandlingAgent);

			OrgHeader testHandlingAgent = Factory.NewWithValidTestData<OrgHeader>();
			testHandlingAgent.OH_FullName = "Kenny Lee Handling Service";

			Declaration.JE_OH_HandlingAgent = testHandlingAgent.PK;
			AssertEquals("Handling Agent Name", "Kenny Lee Handling Service", ((ITNPDEC)EntryHeader).HandlingAgent.Name);
		}

		#endregion

		public void TestDeclarant()
		{
			GlbStaff declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			declarant.GS_Code = "GHW";
			declarant.GS_FullName = "Gwok Hue Wee";
			declarant.GS_WorkPhone = "88432984";
			var wrapper = SGGlbStaffWrapper.Get(declarant);
			wrapper.Tradenetv4Password.GP_UserID = "v13t001";
			wrapper.Tradenetv4Password.GP_MailBoxID = "89135675C";
			Factory.Save();

			Declaration.JE_GS_NKCusAgent = declarant.GS_Code;
			AssertEquals("Declarant Name", "Gwok Hue Wee", CusEntry.Declarant.Name);
			AssertEquals("Declarant Phone No", "88432984", CusEntry.Declarant.Phone);
			AssertEquals("Declarant ID", "v13t001", CusEntry.Declarant.EntityIdentifier);
			AssertEquals("Declarant Code", "89135675C", CusEntry.Declarant.Code);
			AssertEquals("Declarant Passport", "", CusEntry.Declarant.Passport);

			wrapper.Tradenetv4Password.GP_MailBoxID = "";
			declarant.GS_Passport = "10008437593M";
			Factory.Save();
			AssertEquals("Declarant Passport", "10008437593M", CusEntry.Declarant.Passport);
			AssertEquals("Declarant Code", "", CusEntry.Declarant.Code);
		}

		public void TestClaimantCompanyName()
		{
			OrgHeader claimant = Factory.New<OrgHeader>();
			claimant.OH_FullName = "Claimant Company SG P/L";
			Declaration.JE_OH_Claimant = claimant.PK;

			AssertEquals("Claimant Company Name", "Claimant Company SG P/L", CusEntry.Claimant.Name);
		}

		public void TestClaimantName()
		{
			AssertEquals("Claimant Name", "", CusEntry.ClaimantName);
			Declaration.SG_ClaimantName = "William Raffles";

			AssertEquals("Claimant Name", "William Raffles", CusEntry.ClaimantName);
		}

		public void TestClaimantCode()
		{
			AssertEquals("Claimant Code", "", CusEntry.ClaimantCode);
			Declaration.SG_ClaimantCode = "883747J";
			AssertEquals("Claimant Code ", "883747J", CusEntry.ClaimantCode);
		}

		#region Totals Tests

		public void TestTotalCustomsValue()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			invoiceHeader.JZ_InvoiceAmount = 1000m;

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.SG_LastSellingPrice = 100m;

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.SG_LastSellingPrice = 200m;

			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			AssertEquals(300m, ((ISGCUSDEC)Declaration.ActiveEntryHeaders[0]).TotalCustomsValue);
		}

		public void TestTotalDutyPayable()
		{
			InsertPayableAmounts(Registry.EntryChargeTypeList.Codes.Duty);
			AssertEquals(30m, ((ISGCUSDEC)declaration.ActiveEntryHeaders[0]).TotalDutyPayable);
		}

		public void TestTotalExcisePayable()
		{
			InsertPayableAmounts(Registry.EntryChargeTypeList.Codes.Excise);
			AssertEquals(30m, ((ISGCUSDEC)declaration.ActiveEntryHeaders[0]).TotalExcisePayable);
		}

		public void TestTotalOtherTaxPayable()
		{
			InsertPayableAmounts(Registry.EntryChargeTypeList.Codes.OtherTax);
			AssertEquals(30m, ((ISGCUSDEC)declaration.ActiveEntryHeaders[0]).TotalOtherTaxPayable);
		}

		public void TestTotalGSTPayable()
		{
			InsertPayableAmounts(Registry.EntryChargeTypeList.Codes.GST);
			AssertEquals(30m, ((ISGCUSDEC)declaration.ActiveEntryHeaders[0]).TotalGSTPayable);
		}

		CusEntryHeader InsertPayableAmounts(string chargeType)
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00001111";

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00002222";

			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(chargeType, 10m);
			Declaration.ActiveEntryHeaders[0].MergedLines[1].Fees.AddOrUpdate(chargeType, 20m);

			return (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
		}

		public void TestTotalPayable()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00001111";

			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Duty, 2m);
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 3m);
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, 4m);

			ISGCUSDEC sGCUSDEC = (ISGCUSDEC)Declaration.ActiveEntryHeaders[0];

			decimal expectedGSTAmount = 4m;
			decimal expectedDutyAmount = 5m;

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			AssertEquals(expectedGSTAmount, sGCUSDEC.TotalPayable);

			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DUT;
			AssertEquals(expectedDutyAmount, sGCUSDEC.TotalPayable);

			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			AssertEquals(expectedDutyAmount + expectedGSTAmount, sGCUSDEC.TotalPayable);

			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKP;
			AssertEquals(expectedDutyAmount + expectedGSTAmount, sGCUSDEC.TotalPayable);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals(0m, sGCUSDEC.TotalPayable);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals(0m, sGCUSDEC.TotalPayable);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals(0m, sGCUSDEC.TotalPayable);

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals(0m, sGCUSDEC.TotalPayable);
		}

		public void TestTotalPayableWhenSGOriginGoodsNotForSupply()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "27101112";
			invoiceLine.JI_CountryOfOrigin = "CN";

			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 7295.3m);
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, 275.82m);

			ISGCUSDEC sGCUSDEC = (ISGCUSDEC)Declaration.ActiveEntryHeaders[0];

			decimal expectedGSTAmount = 275.82m;
			decimal expectedExciseAmount = 7295.3m;

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			AssertEquals(expectedGSTAmount + expectedExciseAmount, sGCUSDEC.TotalPayable);

			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKP;
			Declaration.SG_SupplyIndicator = "Y";
			AssertEquals(expectedGSTAmount + expectedExciseAmount, sGCUSDEC.TotalPayable);

			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKP;
			Declaration.SG_SupplyIndicator = "";
			AssertEquals(expectedGSTAmount + expectedExciseAmount, sGCUSDEC.TotalPayable);

			invoiceLine.JI_CountryOfOrigin = "SG";
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKP;
			Declaration.SG_SupplyIndicator = "Y";
			AssertEquals(expectedGSTAmount + expectedExciseAmount, sGCUSDEC.TotalPayable);

			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKP;
			Declaration.SG_SupplyIndicator = " ";
			AssertEquals("GST is not applicable for dutiable goods of Singapore origin where supply indicator is blank", expectedExciseAmount, sGCUSDEC.TotalPayable);
		}

		public void TestTotalPayableWhenForExemptCodePresident()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "27101112";
			invoiceLine.JI_CountryOfOrigin = "CN";

			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 7295.30m);
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, 275.82m);

			ISGCUSDEC sGCUSDEC = (ISGCUSDEC)Declaration.ActiveEntryHeaders[0];

			decimal expectedGSTAmount = 275.82m;
			decimal expectedExciseAmount = 7295.30m;

			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			AssertEquals(expectedGSTAmount + expectedExciseAmount, sGCUSDEC.TotalPayable);

			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ExemptPlaceCodePresident;
			AssertEquals("For exempt place code of President, Total Payable is GST component only.", expectedGSTAmount, sGCUSDEC.TotalPayable);
		}

		#endregion

		public void TestGoodsImportedUnderMESorBWS()
		{
			AssertEquals(false, InwardNonPaymentDEC.GoodsImportedUnderMESorBWS);
			Declaration.SG_GoodsImportedUnderMESorBWS = true;
			AssertEquals(true, InwardNonPaymentDEC.GoodsImportedUnderMESorBWS);
		}

		public void TestEntryNumberType()
		{
			EntryHeader.EntryNumber = "PERMIT";
			AssertEquals("PERMIT", EntryHeader.CusEntryNumber.CE_EntryNum);
			AssertEquals("PMT", EntryHeader.CusEntryNumber.CE_EntryType);
		}

		public void TestLoadEntryNumber()
		{
			EntryHeader.EntryNumber = "PERMIT";

			CusEntryNumber cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_EntryNum = "CERT";
			cusEntryNum.CE_EntryIsSystemGenerated = true;
			cusEntryNum.CE_ParentID = EntryHeader.PK;
			cusEntryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusEntryNum.CE_EntryType = "CER";

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.Load<CusEntryHeader>(EntryHeader.PK);

			AssertEquals("PERMIT", EntryHeader.CusEntryNumber.CE_EntryNum);
			AssertEquals("PMT", EntryHeader.CusEntryNumber.CE_EntryType);
		}

		public void TestCertificateNumber()
		{
			EntryHeader.EntryNumber = "PERMIT";

			CusEntryNumber cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_EntryNum = "CERT";
			cusEntryNum.CE_EntryIsSystemGenerated = true;
			cusEntryNum.CE_ParentID = EntryHeader.PK;
			cusEntryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusEntryNum.CE_EntryType = "CER";

			AssertEquals("CERT", EntryHeader.CertificateNumber);

			EntryHeader.CertificateNumber = "NEW CERT";
			AssertEquals("NEW CERT", EntryHeader.CertificateNumber);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusEntryHeader reLoadedEntryHeader = newFactory.Load<CusEntryHeader>(EntryHeader.PK);
			AssertEquals("NEW CERT", reLoadedEntryHeader.CertificateNumber);

			reLoadedEntryHeader.CertificateNumber = "";
			newFactory.Save();

			//reaccess the property (like binding may do) and ensure that the cert number is not stored in the DB
			string s = reLoadedEntryHeader.CertificateNumber;
			newFactory.Save();

			ZQuery query = new ZQuery(CusEntryNumSchema.CE_ParentID, EntryHeader.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, "CER");
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertNull(Factory.LoadTop1<CusEntryNumber>(query));
		}

		#region IOUTDEC

		public void TestCO()
		{
			AssertEquals(EntryHeader, OutwardDec.CO);
		}

		#endregion

		#region ITCODEC

		public void TestApplicationProductType()
		{
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			AssertEquals(ApplicationProductTypeCodeList.Codes.NH, CusCertificate.ApplicationProductType);
		}

		public void TestAdditionalInformation()
		{
			Declaration.SG_CertAdditionalInformation = "TEST";
			AssertEquals("TEST", CusCertificate.AdditionalInformation);
		}

		public void TestTransportDetails()
		{
			AssertEquals("Transport Details", "", CusCertificate.TransportDetails1);
			Declaration.SG_Cert1TransportDetails = "Certificate of Origin Transport Details string";
			AssertEquals("Transport Details", "Certificate of Origin Transport Details string", CusCertificate.TransportDetails1);
		}

		public void TestYearofEntry()
		{
			Declaration.SG_EntryYear = 2000;
			AssertEquals(2000, CusCertificate.YearOfEntry);
		}

		public void TestDonorCountryCode()
		{
			Declaration.SG_RN_NKDonorCountry = Core.Constants.CountryCodes.SaintKittsAndNevis;
			AssertEquals(Core.Constants.CountryCodes.SaintKittsAndNevis, CusCertificate.DonorCountryCode);
		}

		#endregion

		#endregion

		#region Interface Objects

		ISGCUSDEC CusEntry
		{
			get { return EntryHeader; }
		}

		ITCODEC CusCertificate
		{
			get { return EntryHeader; }
		}

		IOUTDEC OutwardDec
		{
			get { return EntryHeader; }
		}

		IINPDEC InwardNonPaymentDEC
		{
			get { return EntryHeader; }
		}

		#endregion

		#region Declaration

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		#endregion

		#region EntryHeader

		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = Declaration.CustomsEntryHeaders.AddNew();
					fEntryHeader.Declaration.AdditionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
					fEntryHeader.EntryNumber = "IM2G002174Z";
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;

		#endregion

		public override void TestEntriesOfHouseBillsRefreshed()
		{
			var (expectedKey, entryCreationStrategy, line) = SetupNonPaymentDeclaration();
			AssertEquals("SG Customs does not use cusdechousebill", expectedKey, entryCreationStrategy.GetKeyForHeader(line));
		}

		public override void TestPackages()
		{
			var (expectedKey, entryCreationStrategy, line) = SetupNonPaymentDeclaration();
			AssertEquals("SG Customs does not use cusdechousebill", expectedKey, entryCreationStrategy.GetKeyForHeader(line));
		}

		protected override bool RatesAreReciprocal
		{
			get { return true; }
		}

		protected override string DefaultExportMessageType
		{
			get { return MessageTypeCodeList.Codes.OUT; }
		}

		protected override ZString ImportJobMessage
		{
			get { return MessageTypeCodeList.Codes.INP; }
		}

		protected override void OverrideValuationDate(BaseJobComInvoiceHeader invoice, ZDateTime date)
		{
			base.OverrideValuationDate(invoice, date);
			invoice.JobDeclaration.JE_EntryAuthorisationDate = date;
		}

		protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new ChargesCurrencyTestSetup();

		sealed class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
		{
			void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
			{
				declaration.AutoCreateChargesBasedOnIncoTerm = false;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 10000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;

				var nonDutiableCharge = invoiceHeader.Charges.AddNew();
				nonDutiableCharge.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight;
				nonDutiableCharge.J7_Amount = 200m;
				nonDutiableCharge.J7_IsDutiable = false;
				nonDutiableCharge.J7_IsIncludedInITOT = true;

				var oft = invoiceHeader.Charges.AddNew();
				oft.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
				oft.J7_Amount = 500m;
				oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
			}

			ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10000m;
			ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 10500m;
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "21069061");
			helper.CreateTariffUOM(tariff1, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.LTR);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Alcohol, tariff1);

			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "24012010");
			helper.CreateTariffUOM(tariff2, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.KGM);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff2);

			Factory.Save();
		}

		(MergeKey ExpectedMergeKey, EntryCreationStrategy EntryCreationStrategy, BaseJobComInvoiceLine line) SetupNonPaymentDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			var entryCreationStrategy = new EntryCreationStrategy(declaration);
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var expectedMergeKey = new MergeKey();
			expectedMergeKey.Add((ZString)MessageTypeCodeList.Codes.INP);
			return (expectedMergeKey, entryCreationStrategy, line);
		}
	}
}
