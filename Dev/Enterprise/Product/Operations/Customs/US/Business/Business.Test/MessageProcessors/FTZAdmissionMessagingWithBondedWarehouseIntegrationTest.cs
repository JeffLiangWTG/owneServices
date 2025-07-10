using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class FTZAdmissionMessagingWithBondedWarehouseIntegrationTest : TestCaseWithFactory
	{
		class BondedWarehouseFTZMessageProcessorForTest : BondedWarehouseFTZMessageProcessor
		{
			internal BondedWarehouseFTZMessageProcessorForTest(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<JobDeclaration, EmailDef, bool> sendMail)
				: base(messagePK, emailReportThatHasBeenDelayed, sendMail)
			{
			}

			public bool HasBeenWithdrawnForTest => HasBeenWithdrawn;

			public bool IsAmendmentErrorForTest => IsAmendmentError;

			public bool IsAmendmentClearForTest => IsAmendmentClear;

			public bool IsOriginalErrorForTest => IsOriginalError;

			public bool IsWithdrawalErrorForTest => IsWithdrawalError;

			public void SendEmailForTest(EmailDef email)
			{
				SendEmailCore(email);
			}

			public string GetReferenceDetailForTest()
			{
				return GetReferenceDetail();
			}
		}

		void SendEmailForTest(JobDeclaration declaration, EmailDef email, bool isFailure)
		{
		}

		public void TestWhenDeclarationIsNull()
		{
			var message = Factory.New<MQEDIMessage>();
			Factory.Save();
			var processor = new BondedWarehouseFTZMessageProcessorForTest(message.PK, null, SendEmailForTest);
			Assert(!processor.IsAmendmentErrorForTest);
			Assert(!processor.IsAmendmentClearForTest);
			Assert(!processor.IsOriginalErrorForTest);
			Assert(!processor.IsWithdrawalErrorForTest);
			AssertNoExceptionThrown(() =>
			{
				processor.SendEmailForTest(new EmailDef());
			});
			AssertNoExceptionThrown(() =>
			{
				var reference = processor.GetReferenceDetailForTest();
			});
			ErrorReporter.Clear();
		}

		#region Implementation

		protected override void SetUp()
		{
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			base.SetUp();
			DeclarationTestHelper.SetupForSendMessage();
		}

		JobDeclaration SetupFTZMessages(JobDeclaration declaration, out MQEDIMessage incomingMessage, ZString messageNumber, ZString messageText, string messageSubType = null)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoingMessage = mock.Object;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			outgoingMessage.EM_MessageNum = messageNumber;
			declaration.Messages.Add(outgoingMessage);

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			incomingMessage.EM_Status = MQEDIMessage.Status.Queued;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = messageNumber;
			incomingMessage.EM_MessageText = messageText;
			if (messageSubType != null)
			{
				outgoingMessage.EM_MessageSubType = messageSubType;
				incomingMessage.EM_MessageSubType = messageSubType;
			}
			return declaration;
		}

		JobDeclaration GetNewDeclaration(ZString declarationReference, ZString controlNumber, string whsCode, ZDecimal quantity)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = declarationReference;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZControlNumber = controlNumber;
			declaration.US_EnableENS = true;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;

			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WHS"));
			if (warehouse == null)
			{
				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				warehouse = (IWhsWarehouse)helper.CreateWarehouse(Warehouse.MainAddress.OA_Address1, whsCode, "BOND");
				warehouse.WW_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
				warehouse.WW_AutoPrintPackingSlip = false;
			}

			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;

			var whsPack = declaration.WHSPacks.AddNew();
			whsPack.US_PackageQty = 10;
			var whsPackLine = declaration.WHSPackLines.AddNew(whsPack);
			whsPackLine.US_JI_InvoiceLine = invoiceLine.PK;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			return declaration;
		}

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.New<OrgHeader>();
					importer.OH_Code = "IMP";
					importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
					importer.MiscServ.OM_IMPartAttrib1Type = "NON";
					importer.CompanyData.OB_IMUsedBondedWhs = true;
					importer.OH_IsWarehouseClient = true;
				}
				return importer;
			}
		}
		OrgHeader importer;

		OrgHeader Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Factory.New<OrgHeader>();
					warehouse.OH_Code = "W1";
					warehouse.OH_RL_NKClosestPort = "USLAX";
					warehouse.OH_FullName = "WAREHOUSE ORG";
					warehouse.MainAddress.OA_Address1 = "ADDRESS 1";
					warehouse.MainAddress.LocalControlledPremisesID = "23423";
				}
				return warehouse;
			}
		}
		OrgHeader warehouse;

		OrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Factory.New<OrgSupplierPart>();
					part.OP_PartNum = "~~1";
					part.OP_StockKeepingUnit = "NO";
					part.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
					var pivot = Pivot; // Force the creation of the pivot
				}
				return part;
			}
		}
		OrgSupplierPart part;

		OrgSupplierPart Part2
		{
			get
			{
				if (part2 == null)
				{
					part2 = Factory.New<OrgSupplierPart>();
					part2.OP_PartNum = "~~2";
					part2.OP_StockKeepingUnit = "NO";
					part2.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
					var pivot = Pivot2; // Force the creation of the pivot
				}
				return part2;
			}
		}
		OrgSupplierPart part2;

		CusClassification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.New<CusClassification>();
					classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
					classification.CC_LookupCode = "@#$34";
					classification.CC_TariffNum = "1010101010";
				}
				return classification;
			}
		}
		CusClassification classification;

		CusClassPartPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					pivot = Part.PivotsForBinding.AddNew();
					pivot.CI_UsageComment = "U1";
					pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
					pivot.CI_OH = Part.RelatedOrganisations[0].OU_OH;
					pivot.CI_TariffNum = "1010101010";
					pivot.CI_CC = Classification.PK;
					pivot.CD_LicenceNo = "13";
				}
				return pivot;
			}
		}
		CusClassPartPivot pivot;

		CusClassPartPivot Pivot2
		{
			get
			{
				if (pivot2 == null)
				{
					pivot2 = Part2.PivotsForBinding.AddNew();
					pivot2.CI_UsageComment = "U1";
					pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
					pivot2.CI_OH = Part.RelatedOrganisations[0].OU_OH;
					pivot2.CI_TariffNum = "2010101010";
					pivot2.CI_CC = Classification.PK;
					pivot2.CD_LicenceNo = "13";
				}
				return pivot2;
			}
		}
		CusClassPartPivot pivot2;

		public void TestProcessInwardOriginalClear()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				MQEDIMessage incomingMessage;
				var declaration = SetupFTZMessages(GetNewDeclaration("BINW000001", "40000007", "WH1", 100m), out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90A153000114790090842501N                                                       " +
"91B1 1  15300011479009084                  1404132150                           " +
"95  083 ZONE ADMISSION DATA ACCEPTED                                            " +
"900000040950000000000000  00000000000000000000000000000315000000015000          " +
"Y  3901SV9NF00001000000409500                                                   ",
					EM_MessageSubTypeList.Codes.FTZAdmissionAdd);

				Factory.Save();
				declaration.FTZYear = ZString.Empty;
				Factory.Save();
				declaration.PublishShipmentForWHSInward(true);
				var customsReference = FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(declaration.FTZAdmissionNumber) + "-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, declaration.AdmissionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "FTZ Admission Response for BINW000001"; }));
				AssertContains("Stock Levels have been updated. (WHS Receipt: ", email.Body);
			}
		}

		public void TestProcessUnknownError()
		{
			MQEDIMessage incomingMessage;
			var declaration = SetupFTZMessages(GetNewDeclaration("BINW000001", "40000007", "WH1", 100m), out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90A153000114790090842501N                                                       " +
"91B1 1  15300011479009084                  1404132150                           " +
"9500001E31FT FORMAT OR PROCESSING ERROR                                         " +
"Y  3901SV9NF00001000000409500                                                   ",
				EM_MessageSubTypeList.Codes.FTZAdmissionAdd);

			Factory.Save();
			AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);

			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("AdmissionStatus", FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd, declaration.AdmissionStatus);
		}

		public void TestProcessInwardOriginalClearInwardCreatedPending()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				MQEDIMessage incomingMessage;
				var declaration = SetupFTZMessages(GetNewDeclaration("BINW000001", "40000007", "WH1", 100m), out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90A153000114790090842501N                                                       " +
"91B1 1  15300011479009084                  1404132150                           " +
"95  083 ZONE ADMISSION DATA ACCEPTED                                            " +
"900000040950000000000000  00000000000000000000000000000315000000015000          " +
"Y  3901SV9NF00001000000409500                                                   ",
					EM_MessageSubTypeList.Codes.FTZAdmissionAdd);

				Factory.Save();

				declaration.FTZYear = ZString.Empty;
				var customsReference = FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(declaration.FTZAdmissionNumber) + "-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);
				declaration.PublishShipmentForWHSInward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertNotNull(declaration.GetLastHoldUniversalShipmentFromNote());
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, declaration.AdmissionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "FTZ Admission Response for BINW000001"; }));
				AssertContains("Stock Levels have been updated. (WHS Receipt: ", email.Body);
			}
		}

		public void TestProcessInwardOriginalClearWithWHSTransaction()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				MQEDIMessage incomingMessage;
				var declaration = SetupFTZMessages(GetNewDeclaration("BINW000001", "40000007", "WH1", 100m), out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90A153000114790090842501N                                                       " +
"91B1 1  15300011479009084                  1404132150                           " +
"95  083 ZONE ADMISSION DATA ACCEPTED                                            " +
"900000040950000000000000  00000000000000000000000000000315000000015000          " +
"Y  3901SV9NF00001000000409500                                                   ",
					EM_MessageSubTypeList.Codes.FTZAdmissionAdd);
				Factory.Save();
				var customsReference = FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(declaration.FTZAdmissionNumber) + "-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);
				var result = declaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);
				declaration.PublishShipmentForWHSInward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, declaration.AdmissionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "FTZ Admission Response for BINW000001"; }));
				AssertContains("Stock Levels have been updated. (WHS Receipt: ", email.Body);
			}
		}

		public void TestProcessInwardAmendmentClear()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				MQEDIMessage incomingMessage;
				var declaration = SetupFTZMessages(GetNewDeclaration("BINW000001", "40000007", "WH1", 100m), out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90A153000114790090842501N                                                       " +
"91B1 1  15300011479009084                  1404132150                           " +
"95  083 ZONE ADMISSION DATA ACCEPTED                                            " +
"900000040950000000000000  00000000000000000000000000000315000000015000          " +
"Y  3901SV9NF00001000000409500                                                   ",
					EM_MessageSubTypeList.Codes.FTZAdmissionReplace);
				Factory.Save();
				var customsReference = FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(declaration.FTZAdmissionNumber) + "-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);
				var result = declaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);
				declaration.PublishShipmentForWHSInward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", FTZMessageStatusList.Codes.ClearFTZAdmissionAmend, declaration.AdmissionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "FTZ Admission Response for BINW000001"; }));
				AssertContains("Stock Levels have been updated. (WHS Receipt: ", email.Body);
			}
		}

		public void TestProcessInwardWithdrawalClear()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				MQEDIMessage incomingMessage;
				var declaration = SetupFTZMessages(GetNewDeclaration("BINW000001", "40000007", "WH1", 100m), out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90 153000114790090842501N                                                       " +
"91B1 1  15300011479009084                  1404160052                           " +
"95  084 ZONE ADMISSION DELETED                                                  " +
"Y  3901SV9NF00001",
					EM_MessageSubTypeList.Codes.FTZAdmissionDelete);
				Factory.Save();
				declaration.FTZYear = ZString.Empty;
				var customsReference = FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(declaration.FTZAdmissionNumber) + "-1";
				var result = declaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);

				declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("AdmissionStatus", FTZMessageStatusList.Codes.ClearFTZAdmissionDelete, declaration.AdmissionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceled, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "FTZ Admission Response for BINW000001"; }));
				AssertContains("Stock Levels Update has been canceled.", email.Body);
			}
		}

		public void TestProcessInwardOriginalError()
		{
			using (Culture.SetTemporarily(System.Globalization.CultureInfo.InvariantCulture))
			{
				MQEDIMessage incomingMessage;
				var declaration = SetupFTZMessages(GetNewDeclaration("BINW000001", "40000007", "WH1", 100m), out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90A15300011479009084    N                                                       " +
"91B3 1  15300011479009084                  1404160045                           " +
"10R15300011479009084    NSV9         13-147927000                               " +
"9501006 INVALID PORT CODE                                                       " +
"9501082 ZONE ADMISSION DATA REJECTED                                            " +
"Y  3901SV9NF00003",
					EM_MessageSubTypeList.Codes.FTZAdmissionAdd);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(true);
				var customsReference = declaration.FTZAdmissionNumber + "-1";
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);
				AssertNotNull(declaration.GetLastHoldUniversalShipmentFromNote());

				var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("AdmissionStatus", FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd, declaration.AdmissionStatus);

				var query = new ZQuery(StmALogSchema.SL_Parent, declaration.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "DCR");
				var rejectedLogs = declaration.Logs.Find(query);
				AssertEquals("One Rejected Log exists", 1, rejectedLogs.Length);

				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "FTZ Admission Response (Failure) for BINW000001"; }));
				AssertContains("<tr><td>B3</td><td>FT Data Rejected</td><td>FTZ Admission Number</td><td>16-Apr-14 00:45:00</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>", email.Body);
				AssertNotContains("(WHS Receipt: ", email.Body);
			}
		}

		public void TestProcessInwardOriginalErrorWhenStatusIsEmpty()
		{
			using (Culture.SetTemporarily(System.Globalization.CultureInfo.InvariantCulture))
			{
				MQEDIMessage incomingMessage;
				var declaration = SetupFTZMessages(GetNewDeclaration("BINW000001", "40000007", "WH1", 100m), out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90A15300011479009084    N                                                       " +
"91B3 1  15300011479009084                  1404160045                           " +
"10R15300011479009084    NSV9         13-147927000                               " +
"9501006 INVALID PORT CODE                                                       " +
"9501082 ZONE ADMISSION DATA REJECTED                                            " +
"Y  3901SV9NF00003",
					EM_MessageSubTypeList.Codes.FTZAdmissionAdd);

				MQEDIMessage incomingMessage2;
				SetupFTZMessages(declaration, out incomingMessage2, "402232",
"B013901SV9NF                                               402232               " +
"90A153000114790090842501N                                                       " +
"91B1 1  15300011479009084                  1404132150                           " +
"95  083 ZONE ADMISSION DATA ACCEPTED                                            " +
"900000040950000000000000  00000000000000000000000000000315000000015000          " +
"Y  3901SV9NF00001000000409500                                                   ",
	EM_MessageSubTypeList.Codes.FTZAdmissionAdd);

				Factory.Save();
				declaration.PublishShipmentForWHSInward(true);
				var customsReference = declaration.FTZAdmissionNumber + "-1";

				var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage2);
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				Factory.Save();
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "FTZ Admission Response (Failure) for BINW000001"; }));
				AssertNotNull(email);
			}
		}

		public void TestProcessInwardOriginalErrorWithWHSTransaction()
		{
			MQEDIMessage incomingMessage;
			var declaration = SetupFTZMessages(GetNewDeclaration("BINW000001", "40000007", "WH1", 100m), out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90A15300011479009084    N                                                       " +
"91B3 1  15300011479009084                  1404160045                           " +
"10R15300011479009084    NSV9         13-147927000                               " +
"9501006 INVALID PORT CODE                                                       " +
"9501082 ZONE ADMISSION DATA REJECTED                                            " +
"Y  3901SV9NF00003",
				EM_MessageSubTypeList.Codes.FTZAdmissionAdd);
			Factory.Save();
			declaration.FTZYear = ZString.Empty;
			var customsReference = FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(declaration.FTZAdmissionNumber) + "-1";
			var result = declaration.PublishShipmentForWHSInward(true);
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
			AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
			AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);
			AssertNotNull(declaration.GetLastHoldUniversalShipmentFromNote());

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("AdmissionStatus", FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd, declaration.AdmissionStatus);
			Factory.Save();
			AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
			AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == "FTZ Admission Response (Failure) for BINW000001"; }));
			AssertContains("<td>INVALID PORT CODE</td></tr><tr><td>082</td><td>ZONE ADMISSION DATA REJECTED</td></tr></table><br>", email.Body);
			AssertNotContains("(WHS Receipt: <", email.Body);
		}

		public void TestProcessInwardAmendmentError()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				MQEDIMessage incomingMessage;
				var declaration = SetupFTZMessages(GetNewDeclaration("BINW000001", "40000007", "WH1", 100m), out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90A15300011479009084    N                                                       " +
"91B3 1  15300011479009084                  1404160045                           " +
"10R15300011479009084    NSV9         13-147927000                               " +
"9501006 INVALID PORT CODE                                                       " +
"9501082 ZONE ADMISSION DATA REJECTED                                            " +
"Y  3901SV9NF00003", EM_MessageSubTypeList.Codes.FTZAdmissionReplace);
				Factory.Save();
				declaration.FTZYear = ZString.Empty;
				var customsReference = FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(declaration.FTZAdmissionNumber) + "-1";
				var result = declaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);

				result = declaration.PublishShipmentForWHSInward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", FTZMessageStatusList.Codes.ErrorFTZAdmissionAmend, declaration.AdmissionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "FTZ Admission Response (Failure) for BINW000001"; }));
				AssertContains("Stock Levels Update can be finalized. (WHS Receipt: ", email.Body);
			}
		}

		public void TestProcessInwardWithdrawalError()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewDeclaration("BINW000001", "40000007", "WH1", 100m);
				MQEDIMessage incomingMessage;
				var entryHeader = SetupFTZMessages(declaration, out incomingMessage, "402232",
"B013901SV9NF                                               402232               " +
"90A15300011479009084    N                                                       " +
"91B3 1  15300011479009084                  1404160045                           " +
"10R15300011479009084    NSV9         13-147927000                               " +
"9501006 INVALID PORT CODE                                                       " +
"9501082 ZONE ADMISSION DATA REJECTED                                            " +
"Y  3901SV9NF00003", EM_MessageSubTypeList.Codes.FTZAdmissionDelete);
				Factory.Save();
				declaration.FTZYear = ZString.Empty;
				var customsReference = FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(declaration.FTZAdmissionNumber) + "-1";
				var result = declaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);

				declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				AssertEquals("AdmissionStatus", ZString.Empty, declaration.AdmissionStatus);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("AdmissionStatus", FTZMessageStatusList.Codes.ErrorFTZAdmissionDelete, declaration.AdmissionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(customsReference, 100m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "FTZ Admission Response (Failure) for BINW000001"; }));
				AssertContains("Stock Levels Update can be finalized. (WHS Receipt: ", email.Body);
			}
		}

		#endregion
	}
}
