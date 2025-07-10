using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class ACEEntrySummaryProcessorTest : ABIProcessorTest<ACEEntrySummaryProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new System.Drawing.Bitmap(1, 2);
			var image2 = new System.Drawing.Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, image2);

			JobDeclaration declaration = GetMergedDeclaration("00000063");
			declaration.JE_GB = newBranch.PK;

			MQEDIMessage outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var invoiceLine1 = declaration.InvoiceLines[0];

			var invoiceLine2 = declaration.InvoiceLines[1];
			invoiceLine2.CusEntryLine.US_CWOs = "27D";//reported in the last response

			MQEDIMessage message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
"B003902SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022098 B00153984    123                          " +
"E1 F198   INITIAL PAY TYP CANNOT BE INDIVD PAYMENTSV9  70022098     B00153984   " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E1 F577   SOLD TO PARTY MISSING-REQ'D FOR TYPE    SV9  70022098     B00153984   " +
"E0 TARIFF 000001 REF ID: 8471704065                                             " +
"E1 F492   FCC 740 MAY BE REQUIRED                 SV9  70022098     B00153984   " +
"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022098     B00153984   " +
"E1RF998   TRANSACTION DATA REJECTED               SV9  70022098     B00153984   " +
"Y  3902SV9AX00008";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);
			AssertEquals("Status changed", "EEO", declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status);

			var factory = new BusinessObjectFactory();
			var entryLine = factory.Load<CusEntryLine>(invoiceLine1.JI_CL);
			AssertEquals("27C", entryLine.US_CWOs);

			var entryLine2 = factory.Load<CusEntryLine>(invoiceLine2.JI_CL);
			AssertEquals("Only Outstanding CWs are recorded", "", entryLine2.US_CWOs);

			var dec = factory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("123", dec.US_TeamNo);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<ZArchitecture.Environment.AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(2, image.Width);
		}

		public void TestHasEmailWarningMessageOnSubTitle()
		{
			JobDeclaration declaration = GetMergedDeclaration("00000088");

			MQEDIMessage outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~18000";

			var invoiceLine1 = declaration.InvoiceLines[0];
			var invoiceLine2 = declaration.InvoiceLines[1];
			invoiceLine2.CusEntryLine.US_CWOs = "27D";

			MQEDIMessage message = GetMessageToProcess();
			message.EM_MessageNum = "~18000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
"B003902SV9AX                                               ~18000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022098 B00153984    123                          " +
"E1 F198   INITIAL PAY TYP CANNOT BE INDIVD PAYMENTSV9  70022098     B00153984   " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E1 F577   SOLD TO PARTY MISSING-REQ'D FOR TYPE    SV9  70022098     B00153984   " +
"E0 TARIFF 000001 REF ID: 8471704065                                             " +
"E1 F492   FCC 740 MAY BE REQUIRED                 SV9  70022098     B00153984   " +
"E1 W27J   *CENSUS* OR-AGR CHARGES/VALUE           SV9  70022098     B00153984   " +
"E1RF998   TRANSACTION DATA REJECTED               SV9  70022098     B00153984   " +
"Y  3902SV9AX00008";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals(true, email.Subject.Contains(ACEEntrySummaryProcessor.CensusWarning));

			var mailQueryText = $"SELECT * FROM dbo.MailDBItems WHERE MI_Subject LIKE '%{ACEEntrySummaryProcessor.CensusWarning}%'";
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(mailQueryText);
			AssertEquals("Email created and saved when bonded warehouse integration not enabled", 1, collection.Count);
		}

		public void TestHasEmailWarningMessageOnSubTitleWhenSupportWarehouse()
		{
			var warehouse = Factory.New<OrgHeader>();
			warehouse.FillWithValidTestData();
			warehouse.OH_FullName = "JPDuminy Bond Stores";
			warehouse.OH_IsWarehouseClient = true;
			warehouse.CompanyData.OB_IMUsedBondedWhs = true;

			var declaration = GetMergedDeclaration("00000088");
			var helper = new WhsDataTestHelper(Factory);
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;

			MQEDIMessage outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~18000";

			var invoiceLine1 = declaration.InvoiceLines[0];
			var invoiceLine2 = declaration.InvoiceLines[1];
			invoiceLine2.CusEntryLine.US_CWOs = "27D";

			MQEDIMessage message = GetMessageToProcess();
			message.EM_MessageNum = "~18000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
"B003902SV9AX                                               ~18000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022098 B00153984    123                          " +
"E1 F198   INITIAL PAY TYP CANNOT BE INDIVD PAYMENTSV9  70022098     B00153984   " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E1 F577   SOLD TO PARTY MISSING-REQ'D FOR TYPE    SV9  70022098     B00153984   " +
"E0 TARIFF 000001 REF ID: 8471704065                                             " +
"E1 F492   FCC 740 MAY BE REQUIRED                 SV9  70022098     B00153984   " +
"E1 W27J   *CENSUS* OR-AGR CHARGES/VALUE           SV9  70022098     B00153984   " +
"E1RF998   TRANSACTION DATA REJECTED               SV9  70022098     B00153984   " +
"Y  3902SV9AX00008";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals(true, email.Subject.Contains(ACEEntrySummaryProcessor.CensusWarning));

			var mailQueryText = $"SELECT * FROM dbo.MailDBItems WHERE MI_Subject LIKE '%{ACEEntrySummaryProcessor.CensusWarning}%'";
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(mailQueryText);
			AssertEquals("Email created and saved when bonded warehouse integration enabled", 1, collection.Count);
		}

		public void TestClearDutyAndMPFCalcDateForRejectOriginalMessage()
		{
			var declaration = CreateTestDataForClearDutyAndMPFCalcDate("71005415");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			declaration.ImportEntryNumber = "71005415";
			Factory.Save();

			message.EM_MessageNum = "HYEDUSCMT_151199";
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();

			var ensResponse = GetMessageToProcess();
			ensResponse.EM_MessageNum = "HYEDUSCMT_151199";
			ensResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			ensResponse.EM_MessageText =
"B001101SV9AX                                               HYEDUSCMT_151199     " +
"E0 SUMMRY 000001 REF ID: SV9 71005415 B00162063                                 " +
"E1RF998   TRANSACTION DATA REJECTED               SV9  70022098     B00153984   " +
"Y  1101SV9AX00002";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var entry2 = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			Assert(entry2.US_DutyCalcDate.IsEmpty);
			Assert(entry2.US_MPFCalcDate.IsEmpty);
			AssertNotNull(entry2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ResetEntryMessageItemFunction.Code)).FirstOrDefault());
		}

		public void TestClearDutyAndMPFCalcDateForRejectReplaceMessage()
		{
			var declaration = CreateTestDataForClearDutyAndMPFCalcDate("71005415");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			declaration.ImportEntryNumber = "71005415";
			Factory.Save();

			message.EM_MessageNum = "HYEDUSCMT_151199";
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace;
			Factory.Save();

			var ensResponse = GetMessageToProcess();
			ensResponse.EM_MessageNum = "HYEDUSCMT_151199";
			ensResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			ensResponse.EM_MessageText =
"B001101SV9AX                                               HYEDUSCMT_151199     " +
"E0 SUMMRY 000001 REF ID: SV9 71005415 B00162063                                 " +
"E1RF998   TRANSACTION DATA REJECTED               SV9  70022098     B00153984   " +
"Y  1101SV9AX00002";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var entry2 = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			Assert(entry2.US_DutyCalcDate.IsEmpty);
			Assert(entry2.US_MPFCalcDate.IsEmpty);
			AssertNotNull(entry2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ResetEntryMessageItemFunction.Code)).FirstOrDefault());
		}

		public void TestClearDutyAndMPFCalcDateForDeletionMessage()
		{
			var declaration = CreateTestDataForClearDutyAndMPFCalcDate("71005477");
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			outgoingmessage.EM_MessageText =
"B  3901SV9AE                                  1101SV9  1   ~15000               " +
"10DSV9  71005415 3901            01                                             " +
"Y  3901SV9AE";

			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";
			var message2 = GetMessageToProcess();
			message2.EM_MessageNum = "~15000";
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message2.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			message2.EM_MessageText =
"B003901SV9AX                                  1101SV9  1   ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 71005415                                           " +
"E1A 997   SUMMARY HAS BEEN DELETED                SV9  71025041                 " +
"Y  3901SV9AX00002";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var entry2 = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			Assert(entry2.US_DutyCalcDate.IsEmpty);
			Assert(entry2.US_MPFCalcDate.IsEmpty);
			AssertNotNull(entry2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ResetEntryMessageItemFunction.Code)).FirstOrDefault());
		}

		JobDeclaration CreateTestDataForClearDutyAndMPFCalcDate(ZString entryNumber)
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff[0].GS_EmailAddress = "test@cargowise.com";

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1020304050";
			tariff1.UE_Unit1 = "KG";
			tariff1.UE_DateFrom = new ZDateTime(2017, 1, 1);
			tariff1.UE_DateTo = new ZDateTime(2017, 12, 31);
			tariff1.UE_Unit1 = Core.Constants.Weight.Kilograms;
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff1.UE_Column1RateAdValorem = 0.162m;
			tariff1.UE_Column2RateAdValorem = 0.370m;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1020304050";
			tariff2.UE_Unit1 = "KG";
			tariff2.UE_DateFrom = new ZDateTime(2018, 1, 1);
			tariff2.UE_DateTo = new ZDateTime(2018, 12, 31);
			tariff2.UE_Unit1 = Core.Constants.Weight.Kilograms;
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff2.UE_Column1RateAdValorem = 0.255m;
			tariff2.UE_Column2RateAdValorem = 0.451m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_ExportDate = new ZDateTime(2017, 12, 11);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2018, 1, 1);
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.ImportEntryNumber = entryNumber;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_Tariff = "1020304050";
			invoiceLine.JI_CustomsQuantity = 10m;
			declaration.DoMerge();

			var mainSender = new MainMessageSender(declaration, ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			mainSender.OnPrepare += new MainMessageSender.PrepareEventHandler(delegate
			{ return true; });
			mainSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate
			{ return true; });
			mainSender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ Factory.Save(); });

			var actions = mainSender.Actions.OfType<EntryHeaderMessageSendingAction>().ToArray();
			mainSender.SendMessage();
			Factory.Save();

			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(new ZDateTime(2018, 1, 1), entrySummaryEntry.US_DutyCalcDate);
			AssertEquals(new ZDateTime(2018, 1, 1), entrySummaryEntry.US_MPFCalcDate);
			declaration.UnlockImportEntryNumberAllocationMutex();
			return declaration;
		}

		public void TestSetTIBExpiryDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.JE_DateOfArrival = ZDateTime.BrettsBirthday;
			declaration.ImportEntryNumber = "00000063";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.US_CertReqDate = ZDateTime.Now;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var invoiceLine1 = declaration.InvoiceLines[0];

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
"B001101SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E1 IB63   CARGO CERT IGNORED/PROCESS ALREADY DONE SV9  70022270     B00155595   " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 8536100020                                             " +
"E1 W27D   *CENSUS* OR-HI VAL/QTY (1)              SV9  70022270     B00155595   " +
"E0 LINITM 000002 REF ID: 002                                                    " +
"E0 TARIFF 000001 REF ID: 4820102010                                             " +
"E1 P489   PGA DISCLAIMER UNKNOWN                  SV9  70022270     B00155595   " +
"E1AI996   SUMMARY HAS BEEN REPLACED               SV9  7002227000100B00155595   " +
"Y  1101SV9AX00009";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.US_TIBExpiryDate, ZDateTime.BrettsBirthday.AddYears(1));
		}

		public void TestUpdateTrackingStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_PGAExpeditedRelease = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "71002867";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var ttb = invoiceLine.TTBLines.AddNew();
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			var lacey = invoiceLine.LaceyActLines.AddNew();
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			var nmfs = invoiceLine.NMFSLines.AddNew();
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var nhtsa = invoiceLine.NHTSALines.AddNew();
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var fws = invoiceLine.FWSHeaders.AddNew();
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var fsis = invoiceLine.FSISLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vne = invoiceLine.VehicleLines.AddNew();
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var pst = invoiceLine.PSTLines.AddNew();
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			var atf = invoiceLine.ATFLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var ams = invoiceLine.AMSLines.AddNew();
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;

			invoiceLine.US_DDTCExemptionCode = "1";
			invoiceLine.US_TSCACertification = "1";
			invoiceLine.US_FDAContactName = "1";
			ttb.US_PermitNumber = "1";
			lacey.US_PGACommercialDescription = "1";
			nmfs.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfs.US_AMLRPermitNumber = "1";
			nhtsa.US_CertifyingIndividual = "1";
			fws.US_CartonQty = 1;
			fsis.US_ProductID = "ADF";
			fda.US_BrandName = "AAA";
			vne.US_BodyCode = "12";
			pst.US_BrandName = "PST";
			atf.US_AECAExemptionCode = "AAA";
			ams.US_IntendedUseCode = "AAA";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Deleted;
			ttb.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			lacey.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			nmfs.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			nhtsa.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;

			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = (EntryHeaderMessageSendingAction)actions[0];

			var message = new ACEEntrySummaryMessageBuilder(entryHeader, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Add).PopulateMessage();
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			declaration.US_PGAReplaceUpdateNeeded = YesNoList.Codes.Yes;
			Factory.Save();
			message.EM_MessageNum = "HYEDUSCMT_148588";

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148588";

			Factory.Save();

			responseMessage.EM_MessageText =
					"B001101SV9AX                                               HYEDUSCMT_148588     " +
					"E0 SUMMRY 000001 REF ID: SV9 71002867 B00155595                                 " +
					"E0 LINITM 000001 REF ID: 001                                                    " +
					"E0 TARIFF 000001 REF ID: 4703110000                                             " +
					"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  71002867     B00155595   " +
					"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  71002867     B00155595   " +
					"Y  1101SV9AX00005";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			ttb = newFactory.Load<TTBLine>(ttb.PK);
			lacey = newFactory.Load<PGA>(lacey.PK);
			nmfs = newFactory.Load<NMFSLine>(nmfs.PK);
			nhtsa = newFactory.Load<NHTSAHeader>(nhtsa.PK);
			fws = newFactory.Load<FWSHeader>(fws.PK);
			fsis = newFactory.Load<USInvoiceLineFSISLine>(fsis.PK);
			fda = newFactory.Load<ACEFDA>(fda.PK);
			vne = newFactory.Load<Vehicle>(vne.PK);
			pst = newFactory.Load<Pesticide>(pst.PK);
			atf = newFactory.Load<ATF>(atf.PK);
			ams = newFactory.Load<AMS>(ams.PK);

			CombineAssertions(() =>
			{
				AssertEquals("US_PGAReplaceUpdateNeeded", ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
				AssertEquals("TTB", PGATrackingStatusList.Codes.Deleted, ttb.US_TrackingStatus);
				AssertEquals("LACEY", PGATrackingStatusList.Codes.Added, lacey.US_TrackingStatus);
				AssertEquals("NMFS", PGATrackingStatusList.Codes.Added, nmfs.US_TrackingStatus);
				AssertEquals("NHTSA", PGATrackingStatusList.Codes.Deleted, nhtsa.US_TrackingStatus);
				AssertEquals("FWS", PGATrackingStatusList.Codes.Added, fws.US_TrackingStatus);
				AssertEquals("FSIS", PGATrackingStatusList.Codes.Added, fsis.US_TrackingStatus);
				AssertEquals("FDA", PGATrackingStatusList.Codes.Added, fda.US_TrackingStatus);
				AssertEquals("VNE", PGATrackingStatusList.Codes.Added, vne.US_TrackingStatus);
				AssertEquals("PST", PGATrackingStatusList.Codes.Added, pst.US_TrackingStatus);
				AssertEquals("ATF", PGATrackingStatusList.Codes.Added, atf.US_TrackingStatus);
				AssertEquals("AMS", PGATrackingStatusList.Codes.Added, ams.US_TrackingStatus);
				AssertEquals("DDTC", PGATrackingStatusList.Codes.Added, invoiceLine.US_DDTCTrackingStatus);
				AssertEquals("TSCA", ZString.Empty, invoiceLine.US_TSCATrackingStatus);
				AssertEquals("ODS", PGATrackingStatusList.Codes.Deleted, invoiceLine.US_ODSTrackingStatus);
			});
		}

		public void TestUpdateTrackingStatusForDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_PGAExpeditedRelease = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "71002867";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var ttb = invoiceLine.TTBLines.AddNew();
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			var lacey = invoiceLine.LaceyActLines.AddNew();
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			var nmfs = invoiceLine.NMFSLines.AddNew();
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var nhtsa = invoiceLine.NHTSALines.AddNew();
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var fws = invoiceLine.FWSHeaders.AddNew();
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var fsis = invoiceLine.FSISLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vne = invoiceLine.VehicleLines.AddNew();
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var pst = invoiceLine.PSTLines.AddNew();
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			var atf = invoiceLine.ATFLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var ams = invoiceLine.AMSLines.AddNew();
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;

			invoiceLine.US_DDTCExemptionCode = "1";
			invoiceLine.US_TSCACertification = "1";
			invoiceLine.US_FDAContactName = "1";

			ttb.US_PermitNumber = "1";
			lacey.US_PGACommercialDescription = "1";
			nmfs.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfs.US_AMLRPermitNumber = "1";
			nhtsa.US_CertifyingIndividual = "1";
			fws.US_CartonQty = 1;
			fsis.US_ProductID = "ADF";
			fda.US_BrandName = "AAA";
			vne.US_BodyCode = "12";
			pst.US_BrandName = "PST";
			atf.US_AECAExemptionCode = "AAA";
			ams.US_IntendedUseCode = "AAA";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Deleted;
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Deleted;
			ttb.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			lacey.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			nmfs.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			nhtsa.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			fws.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			fsis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			fda.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			vne.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			pst.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			atf.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			ams.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var action = (EntryHeaderMessageSendingAction)actions[0];

			var message = new ACEEntrySummaryMessageBuilder(entryHeader, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Delete).PopulateMessage();
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete;
			declaration.US_PGAReplaceUpdateNeeded = YesNoList.Codes.Yes;
			Factory.Save();
			message.EM_MessageNum = "HYEDUSCMT_148588";

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148588";

			Factory.Save();

			responseMessage.EM_MessageText =
					"B001101SV9AX                                               HYEDUSCMT_148588     " +
					"E0 SUMMRY 000001 REF ID: SV9 71002867 B00155595                                 " +
					"E0 LINITM 000001 REF ID: 001                                                    " +
					"E0 TARIFF 000001 REF ID: 4703110000                                             " +
					"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  71002867     B00155595   " +
					"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  71002867     B00155595   " +
					"Y  1101SV9AX00005";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			ttb = newFactory.Load<TTBLine>(ttb.PK);
			lacey = newFactory.Load<PGA>(lacey.PK);
			nmfs = newFactory.Load<NMFSLine>(nmfs.PK);
			nhtsa = newFactory.Load<NHTSAHeader>(nhtsa.PK);
			fws = newFactory.Load<FWSHeader>(fws.PK);
			fsis = newFactory.Load<USInvoiceLineFSISLine>(fsis.PK);
			fda = newFactory.Load<ACEFDA>(fda.PK);
			vne = newFactory.Load<Vehicle>(vne.PK);
			pst = newFactory.Load<Pesticide>(pst.PK);
			atf = newFactory.Load<ATF>(atf.PK);
			ams = newFactory.Load<AMS>(ams.PK);

			CombineAssertions(() =>
			{
				AssertEquals("US_PGAReplaceUpdateNeeded", ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
				AssertEquals("TTB", ZString.Empty, ttb.US_TrackingStatus);
				AssertEquals("LACEY", ZString.Empty, lacey.US_TrackingStatus);
				AssertEquals("NMFS", ZString.Empty, nmfs.US_TrackingStatus);
				AssertEquals("NHTSA", ZString.Empty, nhtsa.US_TrackingStatus);
				AssertEquals("FWS", ZString.Empty, fws.US_TrackingStatus);
				AssertEquals("FSIS", ZString.Empty, fsis.US_TrackingStatus);
				AssertEquals("FDA", ZString.Empty, fda.US_TrackingStatus);
				AssertEquals("VNE", ZString.Empty, vne.US_TrackingStatus);
				AssertEquals("PST", ZString.Empty, pst.US_TrackingStatus);
				AssertEquals("ATF", ZString.Empty, atf.US_TrackingStatus);
				AssertEquals("AMS", ZString.Empty, ams.US_TrackingStatus);
				AssertEquals("DDTC", ZString.Empty, invoiceLine.US_DDTCTrackingStatus);
				AssertEquals("TSCA", ZString.Empty, invoiceLine.US_TSCATrackingStatus);
				AssertEquals("ODS", ZString.Empty, invoiceLine.US_ODSTrackingStatus);
			});

			ttb.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			lacey.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			nmfs.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			nhtsa.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			fws.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			fsis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			fda.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			vne.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			pst.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			atf.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			ams.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Deleted;
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
			invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.Deleted;
			newFactory.Load<MQEDIMessage>(responseMessage.PK).EM_Status = EDIMessage.Status.Queued;
			declaration.US_PGAReplaceUpdateNeeded = YesNoList.Codes.Yes;
			declaration.US_PGAExpeditedRelease = false;
			newFactory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			ttb = newFactory.Load<TTBLine>(ttb.PK);
			lacey = newFactory.Load<PGA>(lacey.PK);
			nmfs = newFactory.Load<NMFSLine>(nmfs.PK);
			nhtsa = newFactory.Load<NHTSAHeader>(nhtsa.PK);
			fws = newFactory.Load<FWSHeader>(fws.PK);
			fsis = newFactory.Load<USInvoiceLineFSISLine>(fsis.PK);
			fda = newFactory.Load<ACEFDA>(fda.PK);
			vne = newFactory.Load<Vehicle>(vne.PK);
			pst = newFactory.Load<Pesticide>(pst.PK);
			atf = newFactory.Load<ATF>(atf.PK);
			ams = newFactory.Load<AMS>(ams.PK);

			CombineAssertions(() =>
			{
				AssertEquals("US_PGAReplaceUpdateNeeded", YesNoList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
				AssertEquals("TTB", PGATrackingStatusList.Codes.ToBeDeleted, ttb.US_TrackingStatus);
				AssertEquals("LACEY", PGATrackingStatusList.Codes.ToBeUpdated, lacey.US_TrackingStatus);
				AssertEquals("NMFS", PGATrackingStatusList.Codes.Added, nmfs.US_TrackingStatus);
				AssertEquals("NHTSA", PGATrackingStatusList.Codes.Deleted, nhtsa.US_TrackingStatus);
				AssertEquals("FWS", PGATrackingStatusList.Codes.Added, fws.US_TrackingStatus);
				AssertEquals("FSIS", PGATrackingStatusList.Codes.Added, fsis.US_TrackingStatus);
				AssertEquals("FDA", PGATrackingStatusList.Codes.Added, fda.US_TrackingStatus);
				AssertEquals("VNE", PGATrackingStatusList.Codes.Added, vne.US_TrackingStatus);
				AssertEquals("PST", PGATrackingStatusList.Codes.Added, pst.US_TrackingStatus);
				AssertEquals("ATF", PGATrackingStatusList.Codes.Added, atf.US_TrackingStatus);
				AssertEquals("AMS", PGATrackingStatusList.Codes.Added, ams.US_TrackingStatus);
				AssertEquals("DDTC", PGATrackingStatusList.Codes.Deleted, invoiceLine.US_DDTCTrackingStatus);
				AssertEquals("TSCA", PGATrackingStatusList.Codes.Added, invoiceLine.US_TSCATrackingStatus);
				AssertEquals("ODS", PGATrackingStatusList.Codes.Deleted, invoiceLine.US_ODSTrackingStatus);
			});
		}

		public void TestE0TARQTYWithDecimalPoint()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.ImportEntryNumber = "70165183";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.US_CertReqDate = ZDateTime.Now;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "HYEDUSCMT_145679";

			var invoiceLine1 = declaration.InvoiceLines[0];

			var message = GetMessageToProcess();
			message.EM_MessageNum = "HYEDUSCMT_145679";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
			@"B001101XJ5AX                                  3910XJ5  1   HYEDUSCMT_145679     " +
"E0 SUMMRY 000001 REF ID: SV9 70165183 B00165183    171                          " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000002 REF ID: 4414000000                                             " +
"E0 TARQTY 000002 REF ID: 000000123.45 DOZ                                       " +
"E1 F442 UOM MISMATCH SV9  70165183     B00165183                                " +
"E0 TARQTY 000002 REF ID: 000000016.78 KG                                        " +
"E1 F442 UOM MISMATCH SV9  70165183     B00165183                                " +
"E1RF998   TRANSACTION DATA REJECTED SV9  70165183     B00165183                 " +
"Y  1101XJ5AX00008                                                               ";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.StartsWith("Entry Summary"));
			foreach (ICodeDescription pair in new EntrySummaryReferenceDataList())
			{
				string expectedEmailData = null;
				switch (pair.Code)
				{
					case EntrySummaryReferenceDataList.Codes.SUMMRY:
						expectedEmailData = "<tr><td>SUMMRY</td><td>&nbsp;</td><td>&nbsp;</td><td>CBP Team Number: 171</td></tr>";
						AssertContains("SUMMARY: " + pair.Code, expectedEmailData, email.Body);
						break;
					case EntrySummaryReferenceDataList.Codes.LINITM:
						expectedEmailData = "<tr><td>LINITM</td><td>&nbsp;</td><td>&nbsp;</td><td>Line Item Identifier: 001</td></tr>";
						AssertContains("Linitm: " + pair.Code, expectedEmailData, email.Body);
						break;
					case EntrySummaryReferenceDataList.Codes.TARIFF:
						expectedEmailData = "<tr><td>TARIFF</td><td>&nbsp;</td><td>&nbsp;</td><td>HTS Number: 4414000000</td></tr>";
						AssertContains("TARIFF: " + pair.Code, expectedEmailData, email.Body);
						break;
					case EntrySummaryReferenceDataList.Codes.TARQTY:
						expectedEmailData = "<tr><td>TARQTY</td><td>&nbsp;</td><td>&nbsp;</td><td>HTS (Qty: 123.45, UQ: DOZ)</td></tr>";
						AssertContains("TARQTY: 123.45 ", expectedEmailData, email.Body);
						expectedEmailData = "<tr><td>TARQTY</td><td>&nbsp;</td><td>&nbsp;</td><td>HTS (Qty: 16.78, UQ: KG)</td></tr>";
						AssertContains("TARQTY: 16.78 ", expectedEmailData, email.Body);
						break;
				}
			}
		}

		public void TestE0TARQTYWithOutDecimalPoint()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.ImportEntryNumber = "70165184";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.US_CertReqDate = ZDateTime.Now;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "HYEDUSCMT_145679";

			var invoiceLine1 = declaration.InvoiceLines[0];

			var message = GetMessageToProcess();
			message.EM_MessageNum = "HYEDUSCMT_145679";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
			@"B001101XJ5AX                                  3910XJ5  1   HYEDUSCMT_145679     " +
"E0 SUMMRY 000001 REF ID: SV9 70165184 B00165183    171                          " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000002 REF ID: 4414000000                                             " +
"E0 TARQTY 000002 REF ID: 000000088899 KG                                        " +
"E1 F442 UOM MISMATCH SV9  70165184     B00165183                                " +
"E0 TARQTY 000002 REF ID: 000000006644 KG                                        " +
"E1 F442 UOM MISMATCH SV9  70165184     B00165183                                " +
"E1RF998   TRANSACTION DATA REJECTED SV9  70165184     B00165183                 " +
"Y  1101XJ5AX00008                                                               ";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.StartsWith("Entry Summary"));
			foreach (ICodeDescription pair in new EntrySummaryReferenceDataList())
			{
				string expectedEmailData = null;
				switch (pair.Code)
				{
					case EntrySummaryReferenceDataList.Codes.SUMMRY:
						expectedEmailData = "<tr><td>SUMMRY</td><td>&nbsp;</td><td>&nbsp;</td><td>CBP Team Number: 171</td></tr>";
						AssertContains("SUMMARY: " + pair.Code, expectedEmailData, email.Body);
						break;
					case EntrySummaryReferenceDataList.Codes.LINITM:
						expectedEmailData = "<tr><td>LINITM</td><td>&nbsp;</td><td>&nbsp;</td><td>Line Item Identifier: 001</td></tr>";
						AssertContains("Linitm: " + pair.Code, expectedEmailData, email.Body);
						break;
					case EntrySummaryReferenceDataList.Codes.TARIFF:
						expectedEmailData = "<tr><td>TARIFF</td><td>&nbsp;</td><td>&nbsp;</td><td>HTS Number: 4414000000</td></tr>";
						AssertContains("TARIFF: " + pair.Code, expectedEmailData, email.Body);
						break;
					case EntrySummaryReferenceDataList.Codes.TARQTY:
						expectedEmailData = "<tr><td>TARQTY</td><td>&nbsp;</td><td>&nbsp;</td><td>HTS (Qty: 888.99, UQ: KG)</td></tr>";
						expectedEmailData = "<tr><td>TARQTY</td><td>&nbsp;</td><td>&nbsp;</td><td>HTS (Qty: 66.44, UQ: KG)</td></tr>";
						break;
				}
			}
		}

		public void TestReferenceCodesAreIncludedInEmailSubject()
		{
			var declaration = GetMergedDeclaration("71001091");

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "HYEDUSCMT_145679";

			var invoiceLine1 = declaration.InvoiceLines[0];

			var message = GetMessageToProcess();
			message.EM_MessageNum = "HYEDUSCMT_145679";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =

			@"B001101XJ5AX                                  3910XJ5  1   HYEDUSCMT_145679     " +
"E0 SUMMRY 000001 REF ID: SV9 71016784 B00165183    171                          " +
"E0 PSTLIN 000000 REF ID: XJ5 71001091 B00159751                                 " +
"E0 ADDCVD 000001 REF ID: A475818001                                             " +
"E0 ARPART 000002 REF ID: C 55-555555800                                         " +
"E0 BNDDTL 000001 REF ID: 8 B 566                                                " +
"E0 BOLINB        REF ID: M MCI234322                                            " +
"E0 CARMAN 000001 REF ID: 00000000                                               " +
"E0 CENWRN        REF ID: CEN CW                                                 " +
"E0 COMDES        REF ID: BOB THE BUILDER                                        " +
"E0 CONREL        REF ID: XJ5 71001091                                           " +
"E0 DOTLIN 000001 REF ID: 001                                                    " +
"E0 DOTVEH        REF ID: VHID2342322                                            " +
"E0 EIPINV 000001 REF ID: JPMATELE288OSA  INVQ67                                 " +
"E0 FCCLIN 000001 REF ID: 001                                                    " +
"E0 FDAACT        REF ID: AOF AOF4685654                                         " +
"E0 FDALIN 000001 REF ID: 001                                                    " +
"E0 FEETOT 000001 REF ID: 053 00000165900                                        " +
"E0 HDRFEE        REF ID: 501 00089210                                           " +
"E0 INVLIN 000001 REF ID: 0001 0000                                              " +
"E0 LICNSE 000001 REF ID: 01 LVSYR1020                                           " +
"E0 LINFEE 000001 REF ID: 501 00000210                                           " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 MISDOC 000001 REF ID:                                                        " +
"E0 PG01          REF ID: FSIFSI                                                 " +
"E0 PG02          REF ID: PG02 TEST                                              " +
"E0 PG04          REF ID: PG04 TEST                                              " +
"E0 PG05          REF ID: PG05 TEST                                              " +
"E0 PG06          REF ID: 0 AU                                                   " +
"E0 PG07          REF ID: PG07 TEST                                              " +
"E0 PG08          REF ID: PG08 TEST                                              " +
"E0 PG10          REF ID: S1   1    RPI 1D                                       " +
"E0 PG13          REF ID:                                   ISONI                " +
"E0 PG14          REF ID: FS751455                                               " +
"E0 PG17          REF ID: PG17 TEST                                              " +
"E0 PG18          REF ID: PG18 TEST                                              " +
"E0 PG19          REF ID: B                   CRAIG SEELIG                       " +
"E0 PG20          REF ID: ERE                                                    " +
"E0 PG21          REF ID: B CRAIG SEELIG           2155551212                    " +
"E0 PG22          REF ID: 956         CI FS3 Y03192014                           " +
"E0 PG23          REF ID: PG23 TEST                                              " +
"E0 PG24          REF ID: PG24 TEST                                              " +
"E0 PG25          REF ID:           2                        0118201403192014    " +
"E0 PG26          REF ID: 000000000100PK                                         " +
"E0 PG27          REF ID: PG27 TEST                                              " +
"E0 PG28          REF ID: PG28 TEST                                              " +
"E0 PG29          REF ID: B 000000010000                                         " +
"E0 PG30          REF ID: PG30 TEST                                              " +
"E0 PG31          REF ID: PG31 TEST                                              " +
"E0 PG32          REF ID: PG32 TEST                                              " +
"E0 PG33          REF ID: PG33 TEST                                              " +
"E0 PG34          REF ID: PG34 TEST                                              " +
"E0 PG35          REF ID: PG35 TEST                                              " +
"E0 PG55          REF ID: PG55 TEST                                              " +
"E0 PG60          REF ID: PG60 TEST                                              " +
"E0 PGADIS 000001 REF ID:                                                        " +
"E0 PGOI          REF ID:   BONE IN                                              " +
"E0 PSCEXP        REF ID: PSCEXP TEST                                            " +
"E0 PSCHRE        REF ID: PSC                                                    " +
"E0 PSCLRE        REF ID: PSC                                                    " +
"E0 TARIFF 000001 REF ID: 2106909700                                             " +
"E0 TARQTY 000001 REF ID: 000053648945 KG                                        " +
"E0 TOTALS 000001 REF ID: XJ5 71001091 B00159751                                 " +
"E1 F706   AT LEAST ONE LINE REQUIRES AD/CVD CASE  XJ5  71001091     B00159751   " +
"E1RF998   TRANSACTION DATA REJECTED               XJ5  71001091     B00159751   " +
"Y  1101XJ5AX00063                                                               ";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.StartsWith("Entry Summary"));
			foreach (ICodeDescription pair in new EntrySummaryReferenceDataList())
			{
				string expectedEmailData = null;
				switch (pair.Code)
				{
					case EntrySummaryReferenceDataList.Codes.ADDCVD:
						expectedEmailData = "<tr><td>ADDCVD</td><td>&nbsp;</td><td>&nbsp;</td><td>Case Number: A475818001</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.ARPART:
						expectedEmailData = "<tr><td>ARPART</td><td>&nbsp;</td><td>&nbsp;</td><td>Article Party (Type: C, ID: 55-555555800)</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.BNDDTL:
						expectedEmailData = "<tr><td>BNDDTL</td><td>&nbsp;</td><td>&nbsp;</td><td>Bonde Type: 8, Bond Designation Type: B, Surety Company: 566</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.BOLINB:
						expectedEmailData = "<tr><td>BOLINB</td><td>&nbsp;</td><td>&nbsp;</td><td>Manifest Component (Type: M, ID: MCI234322)</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.CARMAN:
						expectedEmailData = "<tr><td>CARMAN</td><td>&nbsp;</td><td>&nbsp;</td><td>Manifest (Qty: 0, UQ: )</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.CENWRN:
						expectedEmailData = "<tr><td>CENWRN</td><td>&nbsp;</td><td>&nbsp;</td><td>Census Warning Condition (Code: CEN, Override: W)</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.COMDES:
						expectedEmailData = "<tr><td>COMDES</td><td>&nbsp;</td><td>&nbsp;</td><td>Commercial Description: BOB THE BUILDER</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.CONREL:
						expectedEmailData = "<tr><td>CONREL</td><td>&nbsp;</td><td>&nbsp;</td><td>Release Entry (Filer Code: XJ5, Number: 1001091)</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.DOTLIN:
						expectedEmailData = "<tr><td>DOTLIN</td><td>&nbsp;</td><td>&nbsp;</td><td>DOT Line Number: 001</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.DOTVEH:
						expectedEmailData = "<tr><td>DOTVEH</td><td>&nbsp;</td><td>&nbsp;</td><td>Vehicle Identification Number: VHID2342322</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.EIPINV:
						expectedEmailData = "<tr><td>EIPINV</td><td>&nbsp;</td><td>&nbsp;</td><td>Supplier ID: JPMATELE288OSA, Invoice Number: NVQ67</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.FCCLIN:
						expectedEmailData = "<tr><td>FCCLIN</td><td>&nbsp;</td><td>&nbsp;</td><td>FCC Line Number: 001</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.FDAACT:
						expectedEmailData = "<tr><td>FDAACT</td><td>&nbsp;</td><td>&nbsp;</td><td>Affirmation of Compliance (Code: AOF, Qualifier: OF4685654)</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.FDALIN:
						expectedEmailData = "<tr><td>FDALIN</td><td>&nbsp;</td><td>&nbsp;</td><td>FDA Line Number: 001</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.FEETOT:
						expectedEmailData = "<tr><td>FEETOT</td><td>&nbsp;</td><td>&nbsp;</td><td>Accounting Class: 053, Total Fee Amount: 1659</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.HDRFEE:
						expectedEmailData = "<tr><td>HDRFEE</td><td>&nbsp;</td><td>&nbsp;</td><td>Accounting Class: 501, Header Fee Amount: 892.1</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.INVLIN:
						expectedEmailData = "<tr><td>INVLIN</td><td>&nbsp;</td><td>&nbsp;</td><td>Invoice Line Range (Begin: 1, End: 0)</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.LICNSE:
						expectedEmailData = "<tr><td>LICNSE</td><td>&nbsp;</td><td>&nbsp;</td><td>License / Certificate / Permit (Type: 01, Number: LVSYR1020)</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.LINFEE:
						expectedEmailData = "<tr><td>LINFEE</td><td>&nbsp;</td><td>&nbsp;</td><td>Accounting Class: 501, User Fee Amount: 2.1</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.LINITM:
						expectedEmailData = "<tr><td>LINITM</td><td>&nbsp;</td><td>&nbsp;</td><td>Line Item Identifier: 001</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.MISDOC:
						expectedEmailData = "<tr><td>MISDOC</td><td>&nbsp;</td><td>&nbsp;</td><td>Missing Document Code: </td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG01:
						expectedEmailData = "<tr><td>PG01</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: FSIFSI</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG02:
						expectedEmailData = "<tr><td>PG02</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG02 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG04:
						expectedEmailData = "<tr><td>PG04</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG04 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG05:
						expectedEmailData = "<tr><td>PG05</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG05 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG06:
						expectedEmailData = "<tr><td>PG06</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: 0 AU</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG07:
						expectedEmailData = "<tr><td>PG07</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG07 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG08:
						expectedEmailData = "<tr><td>PG08</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG08 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG10:
						expectedEmailData = "<tr><td>PG10</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: S1   1    RPI 1D</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG13:
						expectedEmailData = "<tr><td>PG13</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: ISONI</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG14:
						expectedEmailData = "<tr><td>PG14</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: FS751455</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG17:
						expectedEmailData = "<tr><td>PG17</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG17 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG18:
						expectedEmailData = "<tr><td>PG18</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG18 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG19:
						expectedEmailData = "<tr><td>PG19</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: B                   CRAIG SEELIG</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG20:
						expectedEmailData = "<tr><td>PG20</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: ERE</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG21:
						expectedEmailData = "<tr><td>PG21</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: B CRAIG SEELIG           2155551212</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG22:
						expectedEmailData = "<tr><td>PG22</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: 956         CI FS3 Y03192014</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG23:
						expectedEmailData = "<tr><td>PG23</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG23 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG24:
						expectedEmailData = "<tr><td>PG24</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG24 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG25:
						expectedEmailData = "<tr><td>PG25</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: 2                        0118201403192014</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG26:
						expectedEmailData = "<tr><td>PG26</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: 000000000100PK</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG27:
						expectedEmailData = "<tr><td>PG27</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG27 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG28:
						expectedEmailData = "<tr><td>PG28</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG28 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG29:
						expectedEmailData = "<tr><td>PG29</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: B 000000010000</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG30:
						expectedEmailData = "<tr><td>PG30</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG30 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG31:
						expectedEmailData = "<tr><td>PG31</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG31 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG32:
						expectedEmailData = "<tr><td>PG32</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG32 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG33:
						expectedEmailData = "<tr><td>PG33</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG33 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG34:
						expectedEmailData = "<tr><td>PG34</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG34 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG35:
						expectedEmailData = "<tr><td>PG35</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG35 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG55:
						expectedEmailData = "<tr><td>PG55</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG55 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PG60:
						expectedEmailData = "<tr><td>PG60</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: PG60 TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PGOI:
						expectedEmailData = "<tr><td>PGOI</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Data: BONE IN</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PGADIS:
						expectedEmailData = "<tr><td>PGADIS</td><td>&nbsp;</td><td>&nbsp;</td><td>PGA Form Disclaimer Code: </td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PSCEXP:
						expectedEmailData = "<tr><td>PSCEXP</td><td>&nbsp;</td><td>&nbsp;</td><td>PSC Filing Explanation Text: PSCEXP TEST</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PSCHRE:
						expectedEmailData = "<tr><td>PSCHRE</td><td>&nbsp;</td><td>&nbsp;</td><td>Post Summary Correction Header Reason Code: PSC</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PSCLRE:
						expectedEmailData = "<tr><td>PSCLRE</td><td>&nbsp;</td><td>&nbsp;</td><td>Post Summary Correction Line Reason Code: PSC</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.SUMMRY:
						expectedEmailData = "<tr><td>SUMMRY</td><td>&nbsp;</td><td>&nbsp;</td><td>CBP Team Number: 171</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.TARIFF:
						expectedEmailData = "<tr><td>TARIFF</td><td>&nbsp;</td><td>&nbsp;</td><td>HTS Number: 2106909700</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.TARQTY:
						expectedEmailData = "<tr><td>TARQTY</td><td>&nbsp;</td><td>&nbsp;</td><td>HTS (Qty: 536489.45, UQ: KG)</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.TOTALS:
						expectedEmailData = "<tr><td>TOTALS</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>";
						break;
					case EntrySummaryReferenceDataList.Codes.PSTLIN:
						continue; // This code is ignored
					default:
						Fail("Code " + pair.Code + " need to be added to the test case");
						break;
				}
				AssertContains("Data should have code " + pair.Code, expectedEmailData, email.Body);
			}
		}

		public void TestSendPaidAfterPaymentFinalized()
		{
			var declaration = GetMergedDeclaration("00000063");
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_Paid = YesNoDefaultList.Codes.Yes;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			outgoingmessage.EM_MessageText =
"B  3901SV9AE                                  1101SV9  1   ~15000               " +
"10DSV9  71025041 3901            01                                             " +
"Y  3901SV9AE";

			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			message.EM_MessageText =
"B003901SV9AX                                  1101SV9  1   ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 71025041                                           " +
"E1A 997   SUMMARY HAS BEEN DELETED                SV9  71025041                 " +
"Y  3901SV9AX00002";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(ZString.Empty, declaration.US_Paid);
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryDelete, declaration.JE_MessageStatus);
		}

		public void TestUpdateCensusWarningStatus()
		{
			var declaration = GetMergedDeclaration("00000063");

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			outgoingmessage.EM_MessageText =
"B  1101SV9AE                                               ~15000               " +
"OI        SAUCE                                                                 " +
"PG01001FDAFOOPRO                                                                " +
"Y  1101SV9AE";
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var invoiceLine1 = declaration.InvoiceLines[0];

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
"B001101SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 4703110000                                             " +
"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022270     B00155595   " +
"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  70022270     B00155595   " +
"Y  1101SV9AX00005";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);
			AssertEquals("Status changed", ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status);
			invoiceLine1.CusEntryLine.Reload();
			AssertEquals("Only 1 Census warning code", 1, invoiceLine1.CusEntryLine.US_CWOs.Split(3).Length);
		}

		public void TestUpdateCensusWarningStatus2()
		{
			var declaration = GetMergedDeclaration("00000063");

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText =
"B  1101SV9AE                                               ~15000               " +
"10ASV9  70022270 1101B00155595   0110 XY          2110813                       " +
"1158-12345678958-12345678938-217802300         102813       PA                  " +
"20APLU1101102813B002TITANIC                                                     " +
"21F9923                                                                         " +
"2200000023CS                                                                    " +
"23MAPLUTESTCENSUS                                                               " +
"318B 891                                                                        " +
"40  001 MXGB102013        0000001429413800000000952    N                        " +
"47MJPTOYTSU149TOK                                                               " +
"47C58-123456789                                                                 " +
"47S58-123456789                                                                 " +
"508536100020 0000027000 0000010000 000000000100NO                               " +
"6250100001250                                                                   " +
"6249900003464                                                                   " +
"40  002 CNGB102013        0000000071413800000000048    N                        " +
"47MEGACETES100CAI                                                               " +
"47C58-123456789                                                                 " +
"47S58-123456789                                                                 " +
"504820102010 0000000000 0000000500 000000110000NO                               " +
"6250100000063                                                                   " +
"6249900000173                                                                   " +
"895010000000131349900000003637                                                  " +
"9000000027000 00000004950 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var invoiceLine1 = declaration.InvoiceLines[0];

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
"B001101SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E1 IB63   CARGO CERT IGNORED/PROCESS ALREADY DONE SV9  70022270     B00155595   " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 8536100020                                             " +
"E1 W27D   *CENSUS* OR-HI VAL/QTY (1)              SV9  70022270     B00155595   " +
"E0 LINITM 000002 REF ID: 002                                                    " +
"E0 TARIFF 000001 REF ID: 4820102010                                             " +
"E1 I465   ARTICLE MAY BE SUBJECT TO AD/CVD        SV9  70022270     B00155595   " +
"E1AI996   SUMMARY HAS BEEN REPLACED               SV9  7002227000100B00155595   " +
"Y  1101SV9AX00009";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);
			AssertEquals("Status changed", ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status);
			invoiceLine1.CusEntryLine.Reload();
			AssertEquals("Only 1 Census warning code", 1, invoiceLine1.CusEntryLine.US_CWOs.Split(3).Length);
		}

		public void TestUpdatePGAWarningStatus()
		{
			var declaration = GetMergedDeclaration("00000063");

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var invoiceLine1 = declaration.InvoiceLines[0];

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
"B001101SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E1 IB63   CARGO CERT IGNORED/PROCESS ALREADY DONE SV9  70022270     B00155595   " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 8536100020                                             " +
"E1 P489   PGA DISCLAIMER UNKNOWN                  SV9  70022270     B00155595   " +
"E0 LINITM 000002 REF ID: 002                                                    " +
"E0 TARIFF 000001 REF ID: 4820102010                                             " +
"E1 I465   ARTICLE MAY BE SUBJECT TO AD/CVD        SV9  70022270     B00155595   " +
"E1AI996   SUMMARY HAS BEEN REPLACED               SV9  7002227000100B00155595   " +
"Y  1101SV9AX00009";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);
			AssertEquals("Status changed", ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status);
		}

		public void TestCorrectStatusCalculated()
		{
			var declaration = GetMergedDeclaration("00000063");

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var invoiceLine1 = declaration.InvoiceLines[0];

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
"B001101SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E1 IB63   CARGO CERT IGNORED/PROCESS ALREADY DONE SV9  70022270     B00155595   " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 8536100020                                             " +
"E1 W27D   *CENSUS* OR-HI VAL/QTY (1)              SV9  70022270     B00155595   " +
"E0 LINITM 000002 REF ID: 002                                                    " +
"E0 TARIFF 000001 REF ID: 4820102010                                             " +
"E1 P489   PGA DISCLAIMER UNKNOWN                  SV9  70022270     B00155595   " +
"E1AI996   SUMMARY HAS BEEN REPLACED               SV9  7002227000100B00155595   " +
"Y  1101SV9AX00009";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);
			AssertEquals("Status is CENSUS warning not PGA warning", ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status);
			invoiceLine1.CusEntryLine.Reload();
			AssertEquals("Census warning code added", 1, invoiceLine1.CusEntryLine.US_CWOs.Split(3).Length);
		}

		public void TestInformationNoticeResponseConsieredAsWAWStatus()
		{
			var declaration = GetMergedDeclaration("00000063");

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var invoiceLine1 = declaration.InvoiceLines[0];

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
"B001101SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E1 I465   ARTICLE MAY BE SUBJECT TO AD/CVD        SV9  70022270     B00155595   " +
"E1AI996   SUMMARY HAS BEEN ADDED                  SV9  7002227000100B00155595   " +
"Y  1101SV9AX00009";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);
			AssertEquals("Status is considered WAW if Information notice is received", ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status);
		}

		[NUnit.Framework.TestDate(2013, 02, 11)]
		public void TestSendAutoENSQuery()
		{
			USCustomsDataRegistry.Instance.AutoQueryEntrySummaries.SetValue(Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);

			var declaration = GetDeclarationWithMessageToProcess("00000063", "~15000", "B001101SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: XJ5 00000063 B00155595                                 " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 4703110000                                             " +
"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022270     B00155595   " +
"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  70022270     B00155595   " +
"Y  1101SV9AX00005");
			declaration.US_PaymentDueDate = ZDateTime.Today.AddDays(-4);
			declaration.JE_DeclarationReference = "B00155595";

			var declaration2 = GetDeclarationWithMessageToProcess("00000064", "~15001", "B003902SV9AX                                               ~15001               " +
"E0 SUMMRY 000001 REF ID: SV9 70022098 B00153984                                 " +
"E1 F198   INITIAL PAY TYP CANNOT BE INDIVD PAYMENTSV9  70022098     B00153984   " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E1 F577   SOLD TO PARTY MISSING-REQ'D FOR TYPE    SV9  70022098     B00153984   " +
"E0 TARIFF 000001 REF ID: 8471704065                                             " +
"E1 F492   FCC 740 MAY BE REQUIRED                 SV9  70022098     B00153984   " +
"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022098     B00153984   " +
"E1RF998   TRANSACTION DATA REJECTED               SV9  70022098     B00153984   " +
"Y  3902SV9AX00008");
			declaration2.US_PaymentDueDate = ZDateTime.Today.AddDays(-1);

			var declaration3 = GetDeclarationWithMessageToProcess("00000065", "~15002", "B001101SV9AX                                               ~15002               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  70022270     B00155595   " +
"Y  1101SV9AX00005");
			declaration3.US_PaymentDueDate = ZDateTime.Today.AddDays(-3);

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery;
			message.EM_HeldUntilDate = ZDate.Today.AddHours(1);
			var entry = declaration3.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Messages.Add(message);

			var declaration4 = GetDeclarationWithMessageToProcess("00000066", "~15003", "B001101SV9AX                                               ~15003               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  70022270     B00155595   " +
"Y  1101SV9AX00005");
			declaration4.Liquidations.AddNew();
			declaration4.US_PaymentDueDate = ZDateTime.Today.AddDays(-7);

			var declaration5 = GetDeclarationWithMessageToProcess("00000067", "~15004", "B001101SV9AX                                               ~15004               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  70022270     B00155595   " +
"Y  1101SV9AX00005");

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			AssertEntrySummaryQueryMessageSent(declaration.ActiveEntryHeaders.EntrySummaryEntry, 1, declaration.US_PaymentDueDate.AddDays(3), "Entry Summary query shoud be sent");
			AssertEntrySummaryQueryMessageSent(declaration2.ActiveEntryHeaders.EntrySummaryEntry, 0, ZDateTime.Empty, "Entry Summary query should not be sent, because entry summary rejected");
			AssertEntrySummaryQueryMessageSent(declaration3.ActiveEntryHeaders.EntrySummaryEntry, 1, new ZDateTime(2013, 02, 11), "Another Entry Summary query for declaration3 should not be sent, because this job already has entry summary query, but Held Until Date should be changed");
			AssertEntrySummaryQueryMessageSent(declaration4.ActiveEntryHeaders.EntrySummaryEntry, 0, ZDateTime.Empty, "Entry Summary query should not be sent, because job already liquidated");
			AssertEntrySummaryQueryMessageSent(declaration5.ActiveEntryHeaders.EntrySummaryEntry, 1, ZDateTime.Today.AddDays(3), "Entry Summary query shoud be sent");
		}

		public void TestMessageOwner()
		{
			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = "B" + MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			Factory.Save();
			outgoingmessage.EM_MessageNum = "~15000";

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText =
"B001101SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 4703110000                                             " +
"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022270     B00155595   " +
"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  70022270     B00155595   " +
"Y  1101SV9AX00005";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();

			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals("It should still work fine without this getting set. Have a look at MQEDIMessage.GetMessageBlockApplicationCodeCore", ZString.Empty, message.EM_MessageOwner);
		}

		public void TestSendCRFromEntrySummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_CargoReleaseType = "ACE";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "71005415";
			declaration.JE_MasterBill = "FSDFS324234";
			Factory.Save();

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_Status = EDIMessage.Status.Received;
			outgoingmessage.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_151199     10ASV9  71005415 1101B00161771   0110 XA          2041714                       1123-45678901223-456789012               040714040714       CA                  20APLU1101040714A001TITANIC                                                     210098L                                                                         2200000015CS                                                                    23MAPLUFSDFS324234                                                              318B 422                                                                        SE30CN                                    EI 23-456789012                       SE30BY SIMPLIFIED ENTRY TEST IMPORTER                                           SE3515123 MAIN STREET                                                           SE36LOS ANGELES                                 60111          US               SE30SE PT KABEPE CHAKRA                                                         SE3515JL PASIRKALIKI 145                                                        SE36BANDUNG                                     40173          ID               40  001 GBJP040714                  588660000000019    N                        47MGB0UAFOR218ROA                                                               47C23-456789012                                                                 47S23-456789012                                                                 SE50MF 0UANGZHOU FOREIGN TRADE BAIYUN*                                          SE5515LTD                                154/F. NO.218 GUANGYUAN ZHONG 343      SE56ROAD GUANGZHOU CHINA                        21512          GB               501401100000 0000000000 0000000090             X                                6250100000011                                                                   6249900000031                                                                   894990000000250050100000000011                                                  9000000000000 00000002511 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			outgoingmessage.EM_LinkedObject = entry;
			Factory.Save();
			outgoingmessage.EM_MessageNum = "HYEDUSCMT_151199";

			var message = GetMessageToProcess();
			message.EM_MessageNum = "HYEDUSCMT_151199";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			message.EM_MessageText =
"B001101SV9SX                                               HYEDUSCMT_151199     " +
"SE10ASV9  71005415 01EI 23-45678901210800000000901101  1101                     " +
"SE11 040714A001    TITANIC             0098L                                    " +
"SE15RAPLUFSDFS324234                                       00000015CS           " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00000";

			var ensResponse = GetMessageToProcess();
			ensResponse.EM_MessageNum = "HYEDUSCMT_151199";
			ensResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			ensResponse.EM_MessageText =
"B001101SV9AX                                               HYEDUSCMT_151199     " +
"E0 SUMMRY 000001 REF ID: SV9 71005415 B00162063                                 " +
"E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7100541500100B00162063   " +
"Y  1101SV9AX00002";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals("Entry Summary Header is a linked object", declaration.ActiveEntryHeaders.SimplifiedEntry.PK, message.EM_LinkUniqueID);
			entry.Reload();
			AssertEquals("Entry Status should not be CSA, should be ENS Accepted", ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, entry.CH_Status);
		}

		public void TestCRLCertStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "SV9";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.ImportEntryNumber = "71005415";
			declaration.JE_MasterBill = "FSDFS324234";
			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var ensAction = actions.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			ensAction.US_SendMessage = true;
			actions.SendMessagesWithoutSaving(declaration.MessageInitiator);
			Factory.Save();

			var entrySummary = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var message = entrySummary.Messages[0];
			message.EM_MessageNum = "HYEDUSCMT_151199";

			AssertEquals("Precondition - entry should have US_CRLCertStatus = 'P'", CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending, entrySummary.US_CRLCertStatus);

			var ensResponse = GetMessageToProcess();
			ensResponse.EM_MessageNum = "HYEDUSCMT_151199";
			ensResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			ensResponse.EM_MessageText =
"B001101SV9AX                                               HYEDUSCMT_151199     " +
"E0 SUMMRY 000001 REF ID: SV9 71005415 B00162063                                 " +
"E1RF998   TRANSACTION DATA REJECTED               SV9  70022098     B00153984   " +
"Y  1101SV9AX00002";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			entrySummary.Reload();
			AssertEquals("US_CRLCertStatus should be set to empty if ENS message rejected", ZString.Empty, entrySummary.US_CRLCertStatus);
		}

		JobDeclaration GetDeclarationWithMessageToProcess(string entryNum, string messageNum, string messageText)
		{
			var declaration = GetMergedDeclaration(entryNum);

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = "B  1234XJ5AE                                               EDIDUSDAT_6012893    10AXJ5  50000310 3902B00005607   11100XY          3012213  16995                1132-34234234232-342342342               011713011713       AK                  20    3902011713    ADMIRALENGRACHT                                             2134R                                                                           40  001 MXMP011713      MX          961650000000100    N                        47MMX6M9GUE69MEX                                                                47C32-342342342                                                                 47S32-342342342                                                                 508466100110 0000000000 0000000100             X                                9000000000000 00000000000 00000000000 00000000000 00000000000                   Y  1234XJ5AE";

			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = messageNum;

			var message = GetMessageToProcess();
			message.EM_MessageNum = messageNum;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			message.EM_MessageText = messageText;

			Factory.Save();
			return declaration;
		}

		JobDeclaration GetMergedDeclaration(ZString entryNumber)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "XJ5";
			dec.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			dec.ImportEntryNumber = entryNumber;

			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();
			dec.InvoiceLines.AddNew();
			dec.US_CertReqDate = ZDateTime.Now;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			return dec;
		}

		MQEDIMessage GetMessageToProcess()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}
	}
}
