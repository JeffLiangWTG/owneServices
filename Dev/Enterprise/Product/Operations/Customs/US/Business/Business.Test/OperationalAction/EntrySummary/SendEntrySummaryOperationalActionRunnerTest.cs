using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.OperationalAction.Testing
{
	sealed class SendEntrySummaryOperationalActionRunnerTest : TestCaseWithFactory
	{
		[TestDate(2012, 10, 30)]
		public void TestEndToEndTest()
		{
			var log = new DummyOperationalActionSectionLog();
			var targets = System.Array.Empty<BusinessObject>();
			var runner = new SendEntrySummaryOperationalActionRunner(log, ZString.Empty, ZString.Empty);
			var jobs = runner.PerformFunctionOperationalAction(true, targets);
			AssertEquals(ZGuid.Empty, jobs.FirstOrDefault());
			AssertEquals("Nothing to send", "INFO: 0 declaration selected and therefore there is nothing to send.", log.MessagesString());

			var declaration1 = Helper.GetMergedDutiableDeclarationForACE(Factory);
			declaration1.US_EnableENS = false;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.Invoices.AddNew();
			declaration2.Invoices[0].InvoiceLines.AddNew();
			declaration2.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var declaration3 = Helper.GetMergedDutiableDeclarationForACE(Factory);

			var declaration4 = GetNotMergedDutiableDeclaration();
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "TSB"));
			if (staff == null)
			{
				staff = Factory.New<GlbStaff>();
				staff.GS_FullName = "Test Broker";
				staff.GS_Code = "TSB";
			}
			declaration4.JE_GS_NKCusAgent = staff.GS_Code;

			var declaration5 = GetNotMergedDutiableDeclaration();
			declaration5.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration5.US_EntryFilerCode = "XJ5";
			declaration5.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			Factory.Save();

			AssertEquals("Precondition: no messages exists", 0, declaration1.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
			AssertEquals("Precondition: no messages exists", 0, declaration2.CustomsEntryHeaders[0].Messages.Count);

			var entry3 = declaration3.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Precondition: no messages exists", 0, entry3.Messages.Count);
			AssertEquals("Precondition: no entry exists", null, declaration4.ActiveEntryHeaders.EntrySummaryEntry);
			AssertEquals("Precondition: no entry exists", null, declaration5.ActiveEntryHeaders.EntrySummaryEntry);

			log = new DummyOperationalActionSectionLog();
			targets = new BusinessObject[] { declaration1, declaration2, declaration3, declaration4, declaration5 };
			runner = new SendEntrySummaryOperationalActionRunner(log, ZString.Empty, ZString.Empty);
			jobs = runner.PerformFunctionOperationalAction(true, targets);
			AssertEquals("3 jobs should be successfully sent", 2, jobs.Count());
			AssertEquals("Import declaration 1, but ENS is not enabled", 0, declaration1.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
			AssertEquals("Export declaration 2", 0, declaration2.CustomsEntryHeaders[0].Messages.Count);

			AssertEquals("Import declaration 3 with ENS enabled, should be sent", 1, entry3.Messages.Count);
			AssertMessageType(entry3.Messages[0].EM_MessageType);
			AssertEquals("Message Status", ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, entry3.CH_Status);
			AssertEquals("Entry Submitted Date", ZDateTime.Today, entry3.CH_EntrySubmittedDate);
			AssertEquals("Entry Submitted Date", ZDateTime.Today, declaration3.JE_EntrySubmittedDate);
			AssertEquals("Payment Due Date", new ZDateTime(2012, 11, 14), declaration3.US_PaymentDueDate);

			var entry4 = declaration4.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Import declaration 4, should be merged and send", 1, entry4.Messages.Count);
			var message = entry4.Messages[0];
			AssertMessageType(message.EM_MessageType);
			AssertEquals("Owner should be current logged user", GlbStaff.CurrentUser.GS_Code, message.EM_SystemCreateUser);
			AssertEquals("Message Status", ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, entry4.CH_Status);
			AssertEquals("Entry Submitted Date", ZDateTime.Today, entry4.CH_EntrySubmittedDate);
			AssertEquals("Entry Submitted Date", ZDateTime.Today, declaration4.JE_EntrySubmittedDate);
			AssertEquals("Payment Due Date", new ZDateTime(2012, 11, 14), declaration4.US_PaymentDueDate);

			var entry5 = declaration5.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(null, entry5);
		}

		public void TestJobsWithConcurrency()
		{
			var declaration1 = Helper.GetMergedDutiableDeclarationForACE(Factory);
			var declaration2 = Helper.GetMergedDutiableDeclarationForACE(Factory);
			var declaration3 = GetNotMergedDutiableDeclaration();

			var declaration4 = GetNotMergedDutiableDeclaration();
			declaration4.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration4.US_EntryFilerCode = "XJ5";
			declaration4.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration1, declaration2, declaration3, declaration4 };
			var runner = new SendEntrySummaryOperationalActionRunner(log, ZString.Empty, ZString.Empty);

			var targetsWithConcurrency = new BusinessObject[] { declaration2, declaration4 };
			runner.SetUpJobsWithConcurrency(targetsWithConcurrency.Cast<JobDeclaration>().Take(2));
			var jobs = runner.PerformFunctionOperationalAction(true, targets);

			AssertEquals("4 jobs should be successfully sent", 3, jobs.Count());
			var entry = declaration2.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Only one message should exists", 1, entry.Messages.Count);
			AssertMessageType(entry.Messages[0].EM_MessageType);

			entry = declaration4.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(null, entry);

			entry = declaration3.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("For other declaration(s) message should be sent also", 1, entry.Messages.Count);
			AssertMessageType(entry.Messages[0].EM_MessageType);
		}

		public void TestDeclarationCanRemergeWhenSendMessageHasMessageError()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Factory.Save();

			NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = false;
			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration };
			var runner = new SendEntrySummaryOperationalActionRunner(log, ZString.Empty, ZString.Empty);
			var jobs = runner.PerformFunctionOperationalAction(false, targets);
			AssertEquals(0, jobs.Count());
			AssertEquals(1, log.messages.Count);
			AssertContains("Cannot send the Entry Summary because the declaration has message errors", log.messages[0]);
			NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = true;

			AssertNoExceptionThrown(() => declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
		}

		public void TestUpdateWarehouseWithdrawalWhenSendMessage()
		{
			var whsDataHelper = new WhsDataTestHelper(Factory);
			var declaration = whsDataHelper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B000000002", "XJ5", "ENT326", 50m);
			declaration.US_QtyInWHBeforeWithdrawal = 150m;
			declaration.US_QtyBeingWithdrawn = 0m;
			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration };
			var runner = new SendEntrySummaryOperationalActionRunner(log, ZString.Empty, ZString.Empty);
			var jobs = runner.PerformFunctionOperationalAction(true, targets);
			AssertEquals(1, jobs.Count());
			declaration = runner.LastFactoryForTesting.Load<JobDeclaration>(declaration.PK);
			AssertEquals(150m, declaration.US_QtyInWHBeforeWithdrawal);
			AssertEquals(50m, declaration.US_QtyBeingWithdrawn);
			AssertEquals(100m, declaration.US_QtyInWHAfterWithdrawal);
			AssertEquals(false, declaration.US_IsFinalWHS);
		}

		[TestDate(2017, 10, 11)]
		public void TestFetchHintUsage()
		{
			GlbBranch.CurrentBranch.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var declaration = SetUpImportDeclaration();
			SetUpInBondData(declaration);
			Factory.Save();

			using (RowFactory.SetCachedTables())
			{
				var targets = new BusinessObject[] { declaration };
				var log = new DummyOperationalActionSectionLog();
				var runner = new SendEntrySummaryOperationalActionRunner(log, ZString.Empty, ZString.Empty);
				var jobs = runner.PerformFunctionOperationalAction(true, targets);

				AssertEquals("One job should be successfully sent", 1, jobs.Count());

				var expectedDBHits = new Dictionary<string, int>()
				{
					{ CusAddInfoSchema.Constants.TableName, 6 },
					{ CusClassPartPivot.Schema.TableName, 0 },
					{ CusCodeDataSchema.Constants.TableName, 1 },
					{ CusContainerInvoiceLinePivotSchema.Constants.TableName, 1 },
					{ CusContainerSchema.Constants.TableName, 1 },
					{ CusDecHouseBillSchema.Constants.TableName, 1 },
					{ CusDecHouseContainerPackSchema.Constants.TableName, 1 },
					{ CusDecHouseContainerPivotSchema.Constants.TableName, 1 },
					{ CusEntryHeaderChargesSchema.Constants.TableName, 1 },
					{ CusEntryHeaderSchema.Constants.TableName, 1 },
					{ CusEntryLineFeeSchema.Constants.TableName, 1 },
					{ CusDispositionSchema.Constants.TableName, 2 },
					{ CusEntryLineSchema.Constants.TableName, 2 },
					{ CusEntryNumSchema.Constants.TableName, 1 },
					{ CusEntryPayInfoSchema.Constants.TableName, 1 },
					{ CusInvPackSchema.Constants.TableName, 0 },
					{ CusLiquidationSchema.Constants.TableName, 1 },
					{ CusStatementLineSchema.Constants.TableName, 1 },
					{ CusUnderbondDecSchema.Constants.TableName, 1 },
					{ EDIMessageSchema.Constants.TableName, 2 },
					{ GenAddOnColumnSchema.Constants.TableName, 1 },
					{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
					{ GenPivotSchema.Constants.TableName, 3 },
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ GlbCompanySchema.Constants.TableName, 1 },
					{ JobComInvHeaderChargeSchema.Constants.TableName, 2 },
					{ JobComInvoiceHeaderRefsSchema.Constants.TableName, 1 },
					{ JobComInvoiceHeaderSchema.Constants.TableName, 1 },
					{ JobComInvoiceLineSchema.Constants.TableName, 1 },
					{ JobConsolTransportSchema.Constants.TableName, 2 },
					{ JobContainerSchema.Constants.TableName, 1 },
					{ JobDeclarationSchema.Constants.TableName, 3 },
					{ JobDocAddressSchema.Constants.TableName, 2 },
					{ JobDocsAndCartageSchema.Constants.TableName, 1 },
					{ JobHeaderSchema.Constants.TableName, 2 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 4 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgCountryDataSchema.Constants.TableName, 1 },
					{ OrgCusCodeSchema.Constants.TableName, 2 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 5 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
					{ ProcessTasksSchema.Constants.TableName, 3 },
					{ RefCountrySchema.Constants.TableName, 2 },
					{ RefCountryStatesSchema.Constants.TableName, 1 },
					{ RefCurrencySchema.Constants.TableName, 1 },
					{ RefLocoMapSchema.Constants.TableName, 2 },
					{ RefServiceLevelSchema.Constants.TableName, 1 },
					{ StmALogSchema.Constants.TableName, 3 },
					{ StmEventSchema.Constants.TableName, 2 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ UNDGDataItem.Schema.TableName, 1 },
					{ USCCountry.Schema.TableName, 2 },
				};
				AssertDbHits(expectedDBHits, runner.LastFactoryForTesting, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
			}
		}

		void AssertMessageType(ZString msgType)
		{
			AssertEquals("Should be ENS message", ACEApplicationIdentifierCodeList.Codes.EntrySummary, msgType);
		}

		JobDeclaration SetUpImportDeclaration()
		{
			var importer = Helper.CreateOrganisation("IMP", "MR Importer", Helper.USLAX.Code, "21 IMPORTER STREET", "IMPORTER", "18023434");
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			importer.MainAddress.OA_State = "IL";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MasterBill = "MB001";
			declaration.JE_HouseBill = "HB001";

			var supplier = Helper.CreateOrganisation("SUP", "MR SUPPLIER", Helper.AUSYD.Code, "21 SUPPLIER STREET", "SUPPLIER", "18023434");
			supplier.OH_IsConsignor = true;
			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "ECPEFCIA113MAN", Core.Constants.CountryCodes.UnitedStates);
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.FillWithValidTestData();
			manufacturer.OH_FullName = "Manufacturer";
			var manufacturerCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");
			var contact = manufacturer.Contacts.AddNew();
			contact.OC_ContactName = "Walter Berry";
			contact.OC_Phone = "+1 (847) 364 5600";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoice.US_FDAContactName = "Walter";
			invoice.US_FDAContactPhoneNo = "8473645600";
			invoice.US_FDAContactEmail = "walter.berry@company.com";

			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;

			var charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "ABCPART1";
			part.OP_Desc = "Product";
			var relationPart = part.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			relationPart.OU_LocalPartNumber = "SUPABCPART1";
			relationPart = part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			relationPart.OU_LocalPartNumber = "IMPABCPART1";
			var pivot = part.PivotsForBinding.AddNew();

			CreateInvoiceLines(invoice, manufacturer, part);

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var charge2_inv2 = invoice2.Charges.AddNew();
			charge2_inv2.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			charge2_inv2.J7_AdjustedCharge = true;

			CreateInvoiceLines(invoice2, manufacturer, part);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Mushroom, 12m);
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.OtherExcise, 13m);
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Raspberry, 14m);
			return declaration;
		}

		void CreateInvoiceLines(JobComInvoiceHeader invoice, OrgHeader manufacturer, OrgSupplierPart part)
		{
			for (int i = 0; i < 10; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.ShouldNotDefaultFDAForProductXMLImport = true;
				invoiceLine.JI_PartNo = part.OP_PartNum;
				Helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
				invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;

				for (int j = 0; j < 5; j++)
				{
					var dot = invoiceLine.DOTs.AddNew();
					dot.US_DOTBondSuretyCode = "891";
					dot.US_DOTBoxNo = "1";
					dot.US_DOTClarCode = "1";
					dot.US_DOTCommercialDesc = "DOT TEST";
					dot.US_DOTCountryOfOrigin = Core.Constants.CountryCodes.Burundi;
					dot.US_DOTTireID = "FR1";
				}

				for (int j = 0; j < 5; j++)
				{
					var pga = invoiceLine.LaceyActLines.AddNew();
					pga.US_PGACommercialDescription = "clone pga line";

					var constituentElement = pga.PG04ConstituentElements.AddNew();
					constituentElement.US_PGANameOfTheConstituentElement = "PINE";
					var scientificData = constituentElement.ScientificDataCollection.AddNew();
					scientificData.US_PGAScientificGenusName = "Genus Name";
				}

				invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
				for (int j = 0; j < 5; j++)
				{
					var fcc = invoiceLine.FCCs.AddNew();
					fcc.US_FCCImpCondNo = "03";
					fcc.US_FCCTradeName = "Shakespeare";
					fcc.US_FCCModel = "WD-1760";
					fcc.US_FCCQty = 1m;
				}

				invoiceLine.JI_InvoiceQuantity = 12000;
				invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
				invoiceLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.Packs;
				invoiceLine.JI_CustomsQuantity = 70m;
				invoiceLine.JI_Weight = 100m;
				invoiceLine.JI_WeightUQ = "KG";
				invoiceLine.UnitPrice = 0.11875m;

				var lineCharge = invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Commission, 100m);
				lineCharge.J7_IsNotIncludedInInvoice = true;
				lineCharge.J7_IsDutiable = true;

				var lineCharge1 = invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 123m, Core.Constants.CurrencyCodes.UnitedStates);
				lineCharge1.J7_IsIncludedInITOT = false;
				lineCharge1.J7_IsDutiable = true;
				var lineCharge2 = invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 45m, Core.Constants.CurrencyCodes.UnitedStates);
				lineCharge2.J7_IsIncludedInITOT = false;
				lineCharge2.J7_IsDutiable = false;
				invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 435m, Core.Constants.CurrencyCodes.UnitedStates);
				invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 587m, Core.Constants.CurrencyCodes.UnitedStates);
			}
		}

		void SetUpInBondData(JobDeclaration declaration)
		{
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX879489";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX879488";

			var container3 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU1234567";

			var container4 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OCLU8839485";

			var packingGroup = declaration.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = declaration.Bills[0].PK;
			packingGroup.CR_CO_Container = container1.PK;

			var package1 = packingGroup.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = ShippingOrPackingingUnitList.Codes.Bag;
			package1.CW_ShippingSymbol = "X Symbol";
			package1.CW_MarksAndNos = "1 Bag";

			var package2 = packingGroup.Packages.AddNew();
			package2.CW_PackQty = 1;
			package2.CW_PackType = ShippingOrPackingingUnitList.Codes.Box;
			package2.CW_ShippingSymbol = "Y Symbol";
			package2.CW_MarksAndNos = "1 Box";

			var invoice = declaration.Invoices[0];

			var inbond = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond.BH_ParentID = declaration.PK;
			inbond.BH_ParentTableCode = declaration.TablePrefix;

			var bill1 = (CusInBondBill)inbond.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB2";
			var bill2 = (CusInBondBill)inbond.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MB1";
			var bill3 = (CusInBondBill)inbond.Bills.AddNew();
			bill3.B0_MasterBillNumber = "MB3";

			var moveHeader = inbond.MovementHeader;
			var moveDetail1 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			moveDetail1.B9_SeqNo = "1";
			var moveDetail2 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill2.PK;
			moveDetail2.B9_SeqNo = "2";
			var moveDetail3 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill3.PK;
			moveDetail3.B9_SeqNo = "3";

			var inBondContainer1 = (CusInBondContainer)moveDetail3.Containers.AddNew();
			inBondContainer1.BC_ContainerNum = "CRUX879489";
			var commodity1 = inBondContainer1.Commodities.AddNew();
			commodity1.BY_Description = "Test";
			var tariff = declaration.InvoiceLines[0].JI_Tariff;
			commodity1.BY_HarmonisedTariff = tariff;

			var commodity2 = inBondContainer1.Commodities.AddNew();
			commodity2.BY_Description = "Test2";
			commodity2.BY_HarmonisedTariff = tariff;

			var commodity3 = inBondContainer1.Commodities.AddNew();
			commodity3.BY_Description = "Test3";
			commodity3.BY_HarmonisedTariff = tariff;

			var inBondContainer2 = (CusInBondContainer)moveDetail3.Containers.AddNew();
			inBondContainer2.BC_ContainerNum = "CRUX879488";
			var inBondContainer3 = (CusInBondContainer)moveDetail3.Containers.AddNew();
			inBondContainer3.BC_ContainerNum = "OCLU1234567";
		}

		JobDeclaration GetNotMergedDutiableDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.SanMarino;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Italy;
			invoice.Charges.AddNew("OFT", 20m, "USD");

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 1000m;
			return declaration;
		}

		DeclarationTestHelper helper;
		DeclarationTestHelper Helper => helper ?? (helper = new DeclarationTestHelper(Factory));
	}
}
