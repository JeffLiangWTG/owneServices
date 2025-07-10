using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ECBM = Enterprise.Customs.Business.MessageManagers;

namespace Enterprise.Customs.ZA.Business.MessageManagers.Testing
{
	[TestedType(typeof(CUSDECMessageManager))]
	sealed class CUSDECMessageManagerTest : EDIFACTMessageManagerTestCase
	{
		public override void TestIsWaitingForResponse()
		{
			dataWrapper.MessageStatus = ZAMessageStatusList.Codes.AwaitingResponse;
			Assert("IsWaitingForResponse", messageManager.IsWaitingForResponse);
			dataWrapper.MessageStatus = ZAMessageStatusList.Codes.Error;
			Assert("Not IsWaitingForResponse", !messageManager.IsWaitingForResponse);
			dataWrapper.MessageStatus = ZString.Empty;
			Assert("Not IsWaitingForResponse", !messageManager.IsWaitingForResponse);
		}

		public void TestPopulateHeldUntilDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "66";
			invoiceLine.JI_CEI = instruction.PK;

			new LineMerger(declaration).DoMerge();
			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			var testWrapper = new MessageSendingObject(entryHeader);
			testWrapper.MessageType = Customs.Common.Shared.MessageSubTypeCodes.Codes.Original;

			var manager = new CUSDECMessageManagerForTest(testWrapper, notification);
			manager.SendMessage(MessageSubTypes.Create);
			var message = entryHeader.Messages[0];
			AssertEquals(ZDateTime.Empty, message.EM_HeldUntilDate);

			var selection = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, selection))
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(2);

				manager.SendMessage(MessageSubTypes.Create);
				message = entryHeader.Messages[1];
				AssertEquals(ZDateTime.Empty, message.EM_HeldUntilDate);

				testWrapper.SubmissionDate = ZDateTime.Today.AddDays(1);

				manager.SendMessage(MessageSubTypes.Create);
				message = entryHeader.Messages[2];
				AssertEquals(ZDateTime.Today.AddDays(1), message.EM_HeldUntilDate);
			}
		}

		public override void TestShouldSendMessagesInTestMode()
		{
			SetTestMode(true);
			Assert(messageManager.ShouldSendMessagesInTestModeForTesting);
			SetTestMode(false);
			Assert(!messageManager.ShouldSendMessagesInTestModeForTesting);
		}

		public override void TestCanSendThisMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}00";

			new LineMerger(declaration).DoMerge();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			var testWrapper = new MessageSendingObject(entryHeader);
			testWrapper.MessageType = Customs.Common.Shared.MessageSubTypeCodes.Codes.Original;
			var manager = new CUSDECMessageManagerForTest(testWrapper, notification);
			AssertEquals(expected: false, manager.CanSendThisMessage_Exposed(out ZString reason));
			AssertEquals("Job not yet saved, Please save before sending.", reason);

			Factory.Save();
			manager = new CUSDECMessageManagerForTest(testWrapper, notification);
			AssertEquals(expected: true, manager.CanSendThisMessage_Exposed(out reason));
			AssertEquals(expected: string.Empty, reason);
		}

		public override void TestGetMessageBuilder()
		{
			AssertType(typeof(CUSDECMessageBuilder), (messageManager as CUSDECMessageManagerForTest).GetMessageBuilder_Exposed(MessageSubTypes.Create));
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("CUSDEC", messageManager.MessageFriendlyName);
		}

		[TestDate(2016, 09, 15)]
		public override void TestPopulateMessages()
		{
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "IMP");
			helper.CreateTaxOrFee(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			var testAgent = CreateTestAgent();
			Factory.Save();

			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_CarrierCode = "ABCD";
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "6AUCINVINV001S";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			CreateInvoiceLineWithPrimaryReference(invoice, instruction, UniversalReferenceConstants.PrimaryPreference.Standard);
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFees(entryHeader);
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesOriginal, result[0].EM_MessageText.Replace("'", "\r\n"));
			entryHeader.MovementReferenceNumberSetter("JSA201607041234567", new ZDateTime(2016, 7, 4));
			result = PopulateCancelationMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesCancelation, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		public void TestGrossWeightPerCUSDEC()
		{
			AssertGrossWeight(100m, 120m);
		}

		public void TestGrossWeightDecimalPlaces()
		{
			AssertGrossWeight(100.10654m, 120.33999m, decimals: true);
		}

		void AssertGrossWeight(decimal totalWeight, decimal weight, bool decimals = false)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TotalWeight = totalWeight;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "66";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			invoiceLine.JI_Weight = weight;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			var result = PopulateOriginalMessagesExposed(entryHeader);
			var grossWeight = invoiceLine.GrossWeightInKG;
			AssertEquals(1, result.Length);
			AssertContains("Should use the total gross weight from the Invoice Lines", ZString.Format("MEA+AAE+AAD+KGM:{0}", decimals ? grossWeight.Round(2) : grossWeight), result[0].EM_MessageText);
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_IMP()
		{
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "IMP");
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);

			var testAgent = CreateTestAgent();
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var testSupplier = CreateOrganisation("SUPPLIERADDR1");
			testSupplier.CustomsCodes.AddNew("CSC", "CSC", Core.Constants.CountryCodes.SouthAfrica);
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_CarrierCode = "ABCD";
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "6AUCINVINV001S";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			invoice.JZ_ValuationCode = "1";
			invoice.JZ_RelatedIndicator = "Y";
			invoice.JZ_VDN = "223344";
			CreateInvoiceLineWithPrimaryReference(invoice, instruction, UniversalReferenceConstants.PrimaryPreference.PreferentialRate);
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFees(entryHeader);
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesIMPOriginal, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_IMP_ZeroTotals()
		{
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "IMP");
			var testAgent = CreateTestAgent();
			Factory.Save();

			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 02, 28);
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_CustomsOffice = "BFN";
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "6CINVINV001S";
			var invoice = CreateInvoice(declaration, ZDateTime.Today, "INV001", 50000);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}00";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "F";
			_ = entryHeader.MergedLines[0];

			var result = PopulateChangeMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesIMPZeroTotalsChange, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_EXP()
		{
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H", "60", "00", "", "", "EXP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H", "60", "", "", "", "EXP");
			var testAgent = CreateTestAgent();
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var testSupplier = CreateOrganisation("SUPPLIERADDR1");
			testSupplier.CustomsCodes.AddNew("CSC", "CSC", Core.Constants.CountryCodes.SouthAfrica);
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Export, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_CarrierCode = "ABCD";
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			declaration.JE_RL_NKOrigin = "ZAJNB";
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "60";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			var invoiceLine = CreateInvoiceLine(invoice, instruction, "U", "VAT");
			invoiceLine.JI_TakeUpInTradeStatistics = false;
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = entryHeader.MergedLines[0];
			entryLine.CL_CustomsValue = 50000;
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesEXP, result[0].EM_MessageText.Replace("'", "\r\n"));
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ExportInvoiceNumber, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now, value: true))
			{
				var resultExp = PopulateMessagesExposed(entryHeader);
				AssertEquals(1, resultExp.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesEXP, resultExp[0].EM_MessageText.Replace("'", "\r\n"));
			}
		}

		[TestDate(2016, 09, 15)]
		public void TestPopupMessage_IMP_CPC_20()
		{
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.OH_Code = "TESTOH";
			var code = testAgent.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPW123", Core.Constants.CountryCodes.SouthAfrica);
			var warehouseAddress = testAgent.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			code.OK_OA_PremisesAddress = warehouseAddress.PK;
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "TESTVESSEL";
			testVessel.RV_CarrierCode = "AANG";
			testVessel.RV_RadioCallSign = "3FKT9";
			Factory.Save();
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Air, "BWBBK", "00626126HB001");
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "20";
			instruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), "INV001");
			invoice.JZ_ValuationCode = "1";
			invoice.JZ_RelatedIndicator = "Y";
			CreateInvoiceLine(invoice, instruction, "U", "VAT");
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			entryHeader.CH_Packages = 15;
			var entryLine = CreateEnryLineWithFees(entryHeader);
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertContains("LOC+122+", @"LOC+122+CPW123::ZZZ", result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_IMP_AirNoContainer_NoVesselName()
		{
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "IMP");
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);

			var testAgent = CreateTestAgent();
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "TESTVESSEL";
			testVessel.RV_CarrierCode = "AANG";
			testVessel.RV_RadioCallSign = "3FKT9";
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Air, "CAOTT", "HB001");
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "6AUCINVINV001S";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			CreateInvoiceLineWithPrimaryReference(invoice, instruction, UniversalReferenceConstants.PrimaryPreference.Standard);
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			entryHeader.CH_Packages = 15;
			var entryLine = CreateEnryLineWithFees(entryHeader);
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesIMPAirNoContainerNoVesselNameOriginal, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 05, 18)]
		public void TestPopulateMessages_UnregisteredTraderExport()
		{
			var entryHeader = SetupPopulateMessagesUnregisteredTraderExport(out OrgHeader testSupplier);
			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesUnregisteredTraderExportOriginal, result[0].EM_MessageText.Replace("'", "\r\n"));

			testSupplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "4770181941", Core.Constants.CountryCodes.SouthAfrica);
			result = PopulateMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesUnregisteredTraderExportOriginalUpdated, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 05, 18)]
		public void TestPopulateMessages_UnregisteredTraderExportWithEXPINVCode()
		{
			var entryHeader = SetupPopulateMessagesUnregisteredTraderExport(out OrgHeader testSupplier);
			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesUnregisteredTraderExportWithEXPINVCodeOriginal, result[0].EM_MessageText.Replace("'", "\r\n"));

			testSupplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "4770181941", Core.Constants.CountryCodes.SouthAfrica);
			result = PopulateMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesUnregisteredTraderExportWithEXPINVCodeUpdated, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		CusEntryHeader SetupPopulateMessagesUnregisteredTraderExport(out OrgHeader testSupplier)
		{
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H", "60", "00", "", "", "EXP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H", "60", "", "", "", "EXP");
			var testAgent = CreateTestAgent();
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			testSupplier = CreateOrganisation("SUPPLIERADDR1");
			testSupplier.OH_FullName = "IMPORTERFULLNAME";
			testSupplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, ValidationConstants.Declaration.UnregisteredTraderCustomsCode, Core.Constants.CountryCodes.SouthAfrica);
			testSupplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "1234567890128", Core.Constants.CountryCodes.SouthAfrica);
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Export, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_CarrierCode = "ABCD";
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			declaration.JE_RL_NKOrigin = "ZAJNB";
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "60";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			var invoiceLine = CreateInvoiceLine(invoice, instruction, "U", "VAT");
			invoiceLine.JI_TakeUpInTradeStatistics = false;
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = entryHeader.MergedLines[0];
			entryLine.CL_CustomsValue = 50000;
			CreateAdditionalInfo(entryLine);
			return entryHeader;
		}

		[TestDate(2016, 05, 18)]
		public void TestPopulateMessages_UnregisteredTraderImport()
		{
			var testImporter = SetupPopulateMessagesUnregisteredTraderImport(addImporter: true, out CusEntryHeader entryHeader);
			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesUnregisteredTraderImportoriginal, result[0].EM_MessageText.Replace("'", "\r\n"));
			testImporter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "4770181941", Core.Constants.CountryCodes.SouthAfrica);
			result = PopulateMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesUnregisteredTraderImportoriginalUpdated, result[0].EM_MessageText.Replace("'", "\r\n"));
			testImporter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "", Core.Constants.CountryCodes.SouthAfrica);
			testImporter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "", Core.Constants.CountryCodes.SouthAfrica);
			testImporter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "ABC12345678", Core.Constants.CountryCodes.SouthAfrica);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAUSEPASSPORT, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: false))
			{
				result = PopulateMessagesExposed(entryHeader);
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesUnregisteredTraderImportoriginalUpdated1, result[0].EM_MessageText.Replace("'", "\r\n"));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAUSEPASSPORT, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: true))
			{
				result = PopulateMessagesExposed(entryHeader);
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesUnregisteredTraderImportoriginalUpdated2, result[0].EM_MessageText.Replace("'", "\r\n"));
			}
		}

		[TestDate(2016, 05, 18)]
		public void TestPopulateMessages_UnregisteredTraderImportNull()
		{
			SetupPopulateMessagesUnregisteredTraderImport(addImporter: false, out CusEntryHeader entryHeader);
			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", TestPopulateMessagesUnregisteredTraderImportNull, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		OrgHeader SetupPopulateMessagesUnregisteredTraderImport(bool addImporter, out CusEntryHeader entryHeader)
		{
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "IMP");
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);

			var testImporter = default(OrgHeader);
			var testAgent = CreateTestAgent();
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "TESTVESSEL";
			testVessel.RV_CarrierCode = "AANG";
			testVessel.RV_RadioCallSign = "3FKT9";
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Air, "CAOTT", "HB001");
			if (addImporter)
			{
				testImporter = CreateOrganisation("IMPORTERADDR1");
				testImporter.OH_FullName = "IMPORTERFULLNAME";
				testImporter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, ValidationConstants.Declaration.UnregisteredTraderCustomsCode, Core.Constants.CountryCodes.SouthAfrica);
				testImporter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "1234567890128", Core.Constants.CountryCodes.SouthAfrica);
				declaration.JE_OH_Importer = testImporter.PK;
			}
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_OH_AgentOverride = testAgent.PK;
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "6AUCINVINV001S";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			CreateInvoiceLine(invoice, instruction, "U", "VAT");
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFees(entryHeader);
			CreateAdditionalInfo(entryLine);
			return testImporter;
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_IMP_SeaContainer_VesselName()
		{
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "IMP");
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);

			var testAgent = CreateTestAgent();
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "TESTVESSEL";
			testVessel.RV_CarrierCode = "AANG";
			testVessel.RV_RadioCallSign = "3FKT9";
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_CarrierCode = "ABCD";
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "6AUCINVINV001S";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			CreateInvoiceLineWithPrimaryReference(invoice, instruction, UniversalReferenceConstants.PrimaryPreference.Standard);
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFees(entryHeader);
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesIMPSeaContainerVesselName, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_CPC_11_40()
		{
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "40", "", "", "EXW");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "EXW");
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);

			var testAgent = CreateTestAgent();
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			var testWarehouseAddr = Factory.NewWithValidTestData<OrgAddress>();
			testWarehouseAddr.OA_OH = testWarehouse.PK;
			testWarehouseAddr.Address1 = "WAREWOLF01";
			testWarehouseAddr.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Botswana;
			testWarehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "100213", Core.Constants.CountryCodes.SouthAfrica).OK_OA_PremisesAddress = testWarehouseAddr.PK;
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			testImporter.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "00000001", Core.Constants.CountryCodes.SouthAfrica);
			testImporter.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "4123456789", Core.Constants.CountryCodes.SouthAfrica);
			var testSupplier = CreateOrganisation("SUPPLIERADDR1");
			testSupplier.CustomsCodes.AddNew("CSC", "CSC", Core.Constants.CountryCodes.SouthAfrica);
			var testOwner = Factory.NewWithValidTestData<OrgHeader>();
			testOwner.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "MEHHE", Core.Constants.CountryCodes.SouthAfrica);
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.ExBond, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_CarrierCode = "DHLP";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "6CSCCINVINV001S";
			instruction.CEI_OA_Warehouse = testWarehouseAddr.PK;
			instruction.CEI_OA_Warehouse2 = testWarehouseAddr.PK;
			instruction.CEI_PreviousMRN = "DBN201501035000012";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			var invoiceLine = CreateInvoiceLine(invoice, instruction, "U", "VAT", "40");
			var tariff1 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DTY", "8999610", taxOrFeeCode: "VAT");
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "7", tariff1);
			helper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KG");
			invoiceLine.JI_Tariff = "8999610";
			invoiceLine.JI_CustomsQuantity = 1.005;
			invoiceLine.JI_CustomsSecondQuantity = 1.505;
			invoiceLine.JI_CustomsSecondUnitQty = "NO";
			invoiceLine.JI_CustomsThirdQuantity = 2.005;
			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			invoiceLine.JI_BondedWhsQuantity = 2.5;
			invoiceLine.JI_BondedWhsUnitQty = "NO";
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			new LineMerger(declaration).DoMerge();
			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFees(entryHeader);
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesCPC1140, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_PaymentTypeNoFAN()
		{
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "IMP");
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);

			var testAgent = CreateTestAgent();
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var testSupplier = CreateOrganisation("SUPPLIERADDR1");
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_CarrierCode = "ABCD";
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "6AUCINVINV001S";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			CreateInvoiceLineWithPrimaryReference(invoice, instruction, UniversalReferenceConstants.PrimaryPreference.Standard);
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "C";
			entryHeader.CH_Packages = 15;
			var entryLine = CreateEnryLineWithFees(entryHeader);
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesPaymentTypeNoFAN, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 01, 01)]
		public void TestPopulateMessages_CaseNumber_ChangeAcknowledgementIndicator()
		{
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "IMP");
			var testAgent = CreateTestAgent();
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var testSupplier = CreateOrganisation("SUPPLIERADDR1");
			testSupplier.CustomsCodes.AddNew("CSC", "CSC", Core.Constants.CountryCodes.SouthAfrica);
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_CarrierCode = "ABCD";
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "6AUCINVINV001S";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			invoice.JZ_VDN = "223344";
			CreateInvoiceLineWithPrimaryReference(invoice, instruction, UniversalReferenceConstants.PrimaryPreference.Standard);
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFees(entryHeader);
			CreateAdditionalInfo(entryLine);

			AssertPopulateMessagesCaseNumberChangeAcknowldgementIndicatorORGNoCaseNumber(entryHeader);
			AssertPopulateMessagesCaseNumberChangeAcknowldgementIndicatorORGWithCaseNumber(entryHeader);
			AssertPopulateMessagesCaseNumberChangeAcknowldgementIndicatorCHGNoCaseNumber(entryHeader);
			AssertPopulateMessagesCaseNumberChangeAcknowldgementIndicatorCHGWithCaseNumber(entryHeader);
		}

		void AssertPopulateMessagesCaseNumberChangeAcknowldgementIndicatorORGNoCaseNumber(CusEntryHeader entryHeader)
		{
			CombineAssertions("ORG - No CaseNumber", () =>
			{
				var result = PopulateOriginalMessagesExposed(entryHeader);
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesCaseNumberChangeAcknowldgementIndicator, result[0].EM_MessageText.Replace("'", "\r\n"));
			});
		}

		void AssertPopulateMessagesCaseNumberChangeAcknowldgementIndicatorORGWithCaseNumber(CusEntryHeader entryHeader)
		{
			CombineAssertions("ORG - With CaseNumber", () =>
			{
				var testWrapper = new MessageSendingObject(entryHeader);
				testWrapper.MessageType = Customs.Common.Shared.MessageSubTypeCodes.Codes.Original;
				testWrapper.CaseNumber = "123321";
				testWrapper.ChangeAcknowledgementIndicator = "5";
				var manager = new CUSDECMessageManagerForTest(testWrapper, notification);
				var result = manager.PopulateMessages_Exposed();
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesCaseNumberChangeAcknowldgementIndicator, result[0].EM_MessageText.Replace("'", "\r\n"));
			});
		}

		void AssertPopulateMessagesCaseNumberChangeAcknowldgementIndicatorCHGNoCaseNumber(CusEntryHeader entryHeader)
		{
			CombineAssertions("CHG - No CaseNumber", () =>
			{
				var result = PopulateChangeMessagesExposed(entryHeader);
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesCaseNumberChangeAcknowldgementIndicatorChange, result[0].EM_MessageText.Replace("'", "\r\n"));
			});
		}

		void AssertPopulateMessagesCaseNumberChangeAcknowldgementIndicatorCHGWithCaseNumber(CusEntryHeader entryHeader)
		{
			CombineAssertions("CHG - With CaseNumber", () =>
			{
				var testWrapper = new MessageSendingObject(entryHeader);
				testWrapper.MessageType = Customs.Common.Shared.MessageSubTypeCodes.Codes.Change;
				testWrapper.CaseNumber = "123321";
				testWrapper.ChangeAcknowledgementIndicator = "5";
				var manager = new CUSDECMessageManagerForTest(testWrapper, notification);
				var result = manager.PopulateMessages_Exposed();
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesCaseNumberChangeAcknowldgementIndicatorChange1, result[0].EM_MessageText.Replace("'", "\r\n"));
			});
		}

		[TestDate(2016, 01, 01)]
		public void TestPopulateEntrySubmittedDateAndLogCommenced()
		{
			helper.CreateCustomsOfficeCusCodeEntry("BBR");
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica);
			var testAgent = CreateTestAgent();
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK, "BBR");
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var testSupplier = CreateOrganisation("SUPPLIERADDR1");
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CustomsOffice = "BBR";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), "INV001");
			invoice.JZ_ValuationCode = "1";
			invoice.JZ_RelatedIndicator = "Y";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}00";
			invoiceLine.JI_NewUsed = "U";
			var messageTypes = new string[] { MessageSubTypeCodes.Codes.Original, MessageSubTypeCodes.Codes.Change };
			foreach (var messageType in messageTypes)
			{
				new LineMerger(declaration).DoMerge();
				var entryHeader = declaration.ActiveEntryHeaders[0];
				entryHeader.CH_PaymentMethod = "D";
				var entryLine = CreateEnryLineWithFees(entryHeader);
				CreateAdditionalInfo(entryLine);
				Factory.Save();

				AssertPopulateEntrySubmittedDateAndLogCommencedPreCheck(declaration, entryHeader);

				var testWrapper = new MessageSendingObject(entryHeader);
				testWrapper.MessageType = Customs.Common.Shared.MessageSubTypeCodes.Codes.Original;
				var manager = new CUSDECMessageManagerForTest(testWrapper, notification);
				manager.SendMessage(MessageSubTypes.Create);

				AssertPopulateEntrySubmittedDateAndLogCommencedPostCheck(declaration, entryHeader);

				declaration.ThrowAwayMerge();
				Factory.Save();
				declaration.JE_EntrySubmittedDate = ZDateTime.Empty;
			}
		}

		void AssertPopulateEntrySubmittedDateAndLogCommencedPreCheck(JobDeclaration declaration, CusEntryHeader entryHeader)
		{
			CombineAssertions("Pre-Check", () =>
			{
				AssertEquals(0, entryHeader.Messages.Count);
				AssertEquals(ZDateTime.Empty, entryHeader.CH_EntrySubmittedDate);
				AssertEquals(ZDateTime.Empty, declaration.JE_EntrySubmittedDate);
				AssertNull(declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsCommenced));
				AssertNull(declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.ExportCustomsCommenced));
			});
		}

		void AssertPopulateEntrySubmittedDateAndLogCommencedPostCheck(JobDeclaration declaration, CusEntryHeader entryHeader)
		{
			CombineAssertions("Post-Check", () =>
			{
				AssertEquals(1, entryHeader.Messages.Count);
				var result = entryHeader.Messages[0];
				AssertEquals(EDIMessage.Status.Queued, result.EM_Status);
				AssertEquals(new ZDateTime(2016, 01, 01), entryHeader.CH_EntrySubmittedDate);
				AssertEquals(new ZDateTime(2016, 01, 01), declaration.JE_EntrySubmittedDate);
				AssertNotNull(declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsCommenced));
				AssertNull(declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(Events.ExportCustomsCommenced));
			});
		}

		[TestDate(2016, 02, 01)]
		public void TestUpdateLRNWhenSending()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry("ZA");
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew("AGT", "00626126", "ZA");
			testAgent.CustomsCodes.AddNew("CDP", "CDP", "ZA");
			Factory.Save();

			var declaration = CreateDeclarationWithMessageInitiator();
			declaration.JE_CustomsOffice = "JSA";
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "11";
			testInst.CEI_DateForDuty = new ZDateTime(2016, 01, 01);
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			declaration.DoMerge();
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			AssertNotEquals("", entryHeader.CH_BGMReference);
			AssertNotEquals("00505655JHB20160202000002", entryHeader.CH_BGMReference);
			AssertEquals(0, entryHeader.Messages.Count);

			var testWrapper = new MessageSendingObject(entryHeader);
			testWrapper.LocalReferenceNumber = "00505655JHB20160202000002";
			var manager = new CUSDECMessageManagerForTest(testWrapper, notification);
			manager.SendMessage(MessageSubTypes.Change);
			AssertEquals("00505655JHB20160202000002", testWrapper.Header.CH_BGMReference);
			AssertEquals(1, entryHeader.Messages.Count);
			var lastMessage_1 = entryHeader.Messages.LastMessage;
			AssertEquals(expected: true, lastMessage_1.EM_MessageText.Contains("00505655JHB20160202000002"));

			Factory.Save();
			testWrapper = new MessageSendingObject(entryHeader);
			testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
			testWrapper.LocalReferenceNumber = "00505655JHB20160202000003";
			testWrapper.MessageType = MessageSubTypeCodes.Codes.Original;
			manager = new CUSDECMessageManagerForTest(testWrapper, notification);
			manager.SendMessage(MessageSubTypes.Change);
			AssertEquals("00505655JHB20160202000002", testWrapper.Header.CH_BGMReference);
			AssertEquals(2, entryHeader.Messages.Count);
			var lastMessage_2 = entryHeader.Messages.LastMessage;
			AssertNotEquals(lastMessage_1.PK, lastMessage_2);
			AssertEquals(expected: true, lastMessage_2.EM_MessageText.Contains("00505655JHB20160202000002"));
			AssertEquals(expected: false, lastMessage_2.EM_MessageText.Contains("00505655JHB20160202000003"));
			AssertEquals("00505655JHB20160202000002", testWrapper.LocalReferenceNumber);
			testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
			AssertEquals("00505655JHB20160202000002", testWrapper.LocalReferenceNumber);
			ErrorReporter.Clear();
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_CPC_11_20()
		{
			helper.CreateCustomsOfficeCusCodeEntry("BBR");
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "IMP");
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica);
			var testAgent = CreateTestAgent();
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK, "BBR");

			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea);
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "6CINVINV001S";
			instruction.CEI_PreviousMRN = "BBR201603221234567";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), "INV001", 50000);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_LinePrice = 50000;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}20";
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			new LineMerger(declaration).DoMerge();
			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesCPC1120, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_CPC40and42()
		{
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "40", "41", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "40", "", "", "", "IMP");
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			helper.CreateTaxOrFee(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "S!", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			var testAgent = CreateTestAgent();
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var testSupplier = CreateOrganisation("SUPPLIERADDR1");
			testSupplier.CustomsCodes.AddNew("CSC", "CSC", Core.Constants.CountryCodes.SouthAfrica);
			var testWarehouse = CreateOrganisation("WAREHOUSEADDR1");
			testWarehouse.CustomsCodes.AddNew("CPW", "PTAVMP11111", Core.Constants.CountryCodes.SouthAfrica);
			var testRemover = CreateOrganisation("REMOVERADDR1");
			testRemover.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "00111110", Core.Constants.CountryCodes.SouthAfrica);
			testRemover.OH_RL_NKClosestPort = "ZACPT";
			var testBondHolder = CreateOrganisation("BONDHOLDERADDR1");
			testBondHolder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "00281124", Core.Constants.CountryCodes.SouthAfrica);

			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea, "ZAJNB", "HB001");
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_CarrierCode = "DHLP";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_RemovalTransportCode = Enterprise.Core.Constants.TransportModes.Road;
			declaration.JE_OH_AgentOverride = testAgent.PK;

			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "40";
			instruction.CEI_UCROverride = "6AUCINVINV001S";
			instruction.CEI_OA_Warehouse2 = testWarehouse.MainAddress.PK;
			instruction.CEI_OH_Carrier = testRemover.PK;
			instruction.CEI_OH_BondHolder = testBondHolder.PK;
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			invoice.JZ_VDN = "223344";
			CreateInvoiceLineWithPrimaryReference(invoice, instruction, UniversalReferenceConstants.PrimaryPreference.Standard, "41");
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFeesWithLandedCostOnly(entryHeader);
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesCPC40and42, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateRemovalTransportMode_CPC11_WithAndWithoutBLNSCountry()
		{
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			helper.CreateTaxOrFee(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "S!", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			var testAgent = CreateTestAgent();
			Factory.Save();

			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var testSupplier = CreateOrganisation("SUPPLIERADDR1");
			testSupplier.CustomsCodes.AddNew("CSC", "CSC", Core.Constants.CountryCodes.SouthAfrica);
			var testWarehouse = CreateOrganisation("WAREHOUSEADDR1");
			testWarehouse.CustomsCodes.AddNew("CPW", "PTAVMP11111", Core.Constants.CountryCodes.SouthAfrica);
			var testRemover = CreateOrganisation("REMOVERADDR1");
			testRemover.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "00111110", Core.Constants.CountryCodes.SouthAfrica);
			testRemover.OH_RL_NKClosestPort = "ZACPT";
			var testBondHolder = CreateOrganisation("BONDHOLDERADDR1");
			testBondHolder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "00281124", Core.Constants.CountryCodes.SouthAfrica);

			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea, "LSMSU", "HB001");
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_CarrierCode = "DHLP";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_RemovalTransportCode = Enterprise.Core.Constants.TransportModes.Road;
			declaration.JE_OH_AgentOverride = testAgent.PK;

			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_OA_Warehouse2 = testWarehouse.MainAddress.PK;
			instruction.CEI_OH_Carrier = testRemover.PK;
			instruction.CEI_OH_BondHolder = testBondHolder.PK;
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			invoice.JZ_VDN = "223344";
			CreateInvoiceLineWithPrimaryReference(invoice, instruction, UniversalReferenceConstants.PrimaryPreference.Standard, "00");
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFeesWithLandedCostOnly(entryHeader);
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertContains("When CPS = 11 and final destination is a BLNS country/region then removal code 3 is output in third element", "TDT+20+VOY+1:3+", result[0].EM_MessageText);
			declaration.JE_RL_NKFinalDestination = "ZAJNB"; // Non BLNS Country
			result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertContains("When CPC = 11 and final destination is NOT a BLNS country/region then removal code 3 is NOT output in third element", "TDT+20+VOY+1:3+++++", result[0].EM_MessageText);
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_CPC66and66and67()
		{
			var entryHeader = SetupPopulateMessagesCPC66and66and67();
			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesCPC66and66and67, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_CPC66and66and67WithEXPINVCode()
		{
			var entryHeader = SetupPopulateMessagesCPC66and66and67();
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ExportInvoiceNumber, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now, value: true))
			{
				var result = PopulateOriginalMessagesExposed(entryHeader);
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesCPC66and66and67WithEXPINVCode, result[0].EM_MessageText.Replace("'", "\r\n"));
			}
		}

		public CusEntryHeader SetupPopulateMessagesCPC66and66and67()
		{
			helper.CreateCustomsOfficeCusCodeEntry("KFN");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H", "66", "12", "", "", "EXP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H", "66", "", "", "", "EXP");
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "00505655", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			SetupFinancialAccountNumberPortMaps(testAgent.PK, "KFN", "8120073229");

			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var testSupplier = CreateOrganisation("SUPPLIERADDR1");
			testSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "01143233", Core.Constants.CountryCodes.SouthAfrica);
			testSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "4420116552", Core.Constants.CountryCodes.SouthAfrica);

			var declaration = CreateDeclarationCPC66and66and67();
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction = CreateInstructionCPC66and66and67(declaration);

			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 06, 02), "JPG125", 20);
			invoice.JZ_OH_Supplier = testSupplier.PK;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RN_NKDefaultOrigin = "NA";
			var invoiceLine = CreateInvoiceLine(invoice, instruction, "12");
			invoiceLine.JI_InvoiceQuantity = 20m;
			invoiceLine.JI_Weight = 100m;

			var tariff1 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DTY", "999910");
			Factory.Save();
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "9", tariff1);
			helper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, Core.Constants.Weight.Kilograms);

			invoiceLine.JI_Tariff = "999910";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_LinePrice = 20m;
			invoiceLine.JI_Description = "CARROTS";
			invoiceLine.JI_CountryOfOrigin = "NA";
			invoiceLine.UnitPrice = 1;
			invoiceLine.JI_PreviousEntryNumber = "KFN201606025000110";
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			new LineMerger(declaration).DoMerge();
			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_PaymentMethod = "F";
			entryHeader.CH_BGMReference = "00505655KFN20160602000001";
			return entryHeader;
		}

		JobDeclaration CreateDeclarationCPC66and66and67()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.JE_CustomsOffice = "KFN";
			declaration.JE_MasterBill = "BOL126";
			declaration.JE_RL_NKMasterBillIssuedAt = "ZAJNB";
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 06, 02);
			declaration.JE_VoyageFlightNo = "DE12EEGP";
			declaration.JE_CarrierCode = "DHLP";
			declaration.JE_RL_NKPortOfLoading = "ZAJNB";
			declaration.JE_RL_NKPortOfArrival = "NAWDH";
			declaration.JE_RL_NKOrigin = "ZAJNB";
			declaration.JE_RL_NKFinalDestination = "NAWDH";
			declaration.JE_GoodsOrigin = "NA";
			declaration.JE_TotalWeight = 100m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_ShipmentIncoTerm = "FOB";
			return declaration;
		}

		CusEntryInstruction CreateInstructionCPC66and66and67(JobDeclaration declaration)
		{
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "66";
			instruction.CEI_PreviousMRN = "KFN201606025000110";
			instruction.CEI_CreditTerms = "ADV";
			instruction.CEI_TransactionValue = 200m;
			instruction.CEI_RX_NKTransactionValueCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			instruction.CEI_RefType = RefTypeList.Codes.Other;
			instruction.CEI_UCROrderNumber = "000000000003";
			instruction.CEI_PortOfExit = "KFN";
			return instruction;
		}

		public void TestPopulateEntryLineUZ_VOBAmountForSAD()
		{
			var declaration = CreateDeclarationWithMessageInitiator();
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testInvoice1 = declaration.Invoices.AddNew();
			testInvoice1.JZ_InvoiceNumber = "INV3";
			testInvoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine11 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = testInst.PK;
			invoiceLine11.JI_Tariff = "1";
			var invoiceLine12 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = testInst.PK;
			invoiceLine12.JI_Tariff = "1";
			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var header = declaration.ActiveEntryHeaders[0];
			AssertEquals(2, header.MergedLines.Count);
			var entryLine1 = header.MergedLines[0];
			var entryLine2 = header.MergedLines[1];
			entryLine1.CL_VPBAmount = 1m;
			entryLine2.CL_VPBAmount = 2m;

			AssertPopulateEntryLineUZVOBAmountForSADSendingFailures(header, entryLine1, entryLine2);
			AssertPopulateEntryLineUZVOBAmountForSADSendingChangeNoUpdate(header, entryLine1, entryLine2);
			AssertPopulateEntryLineUZVOBAmountForSADSendingOriginalUpdate(header, entryLine1, entryLine2);
		}

		void AssertPopulateEntryLineUZVOBAmountForSADSendingFailures(CusEntryHeader header, CusEntryLine entryLine1, CusEntryLine entryLine2)
		{
			CombineAssertions("Sending Failures", () =>
			{
				var messageSendingObject = new MessageSendingObject(header);
				messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
				new CUSDECMessageManager(messageSendingObject, notification).SendMessage();

				AssertEquals(0, header.Messages.Count);
				AssertEquals(1m, entryLine1.CL_VPBAmount);
				AssertEquals(2m, entryLine2.CL_VPBAmount);
			});
		}

		void AssertPopulateEntryLineUZVOBAmountForSADSendingChangeNoUpdate(CusEntryHeader header, CusEntryLine entryLine1, CusEntryLine entryLine2)
		{
			CombineAssertions("Sending Change, no Update", () =>
			{
				Factory.Save();
				notification = new MessageNotificationCollector_ForTest();
				var messageSendingObject = new MessageSendingObject(header);
				messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
				new CUSDECMessageManager(messageSendingObject, notification).SendMessage();

				AssertEquals(1, header.Messages.Count);
				AssertEquals(1m, entryLine1.CL_VPBAmount);
				AssertEquals(2m, entryLine2.CL_VPBAmount);
			});
		}

		void AssertPopulateEntryLineUZVOBAmountForSADSendingOriginalUpdate(CusEntryHeader header, CusEntryLine entryLine1, CusEntryLine entryLine2)
		{
			CombineAssertions("Sending Original, Update", () =>
			{
				Factory.Save();
				var messageSendingObject = new MessageSendingObject(header);
				messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
				new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
				AssertEquals(2, header.Messages.Count);
				AssertEquals(1m, entryLine1.CL_VPBAmount);
				AssertEquals(2m, entryLine2.CL_VPBAmount);

				entryLine1.CL_VPBAmount = 3m;
				entryLine2.CL_VPBAmount = 4m;
				Factory.Save();
				messageSendingObject = new MessageSendingObject(header);
				messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Replace;
				new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
				AssertEquals(3, header.Messages.Count);
				AssertEquals(3m, entryLine1.CL_VPBAmount);
				AssertEquals(4m, entryLine2.CL_VPBAmount);
			});
		}

		public void TestBackpopulateTargetEntryLineNumberOnMessageSending()
		{
			var declaration = CreateDeclarationWithMessageInitiator();
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_DateForDuty = new ZDateTime(2019, 1, 1);
			var testInvoice1 = declaration.Invoices.AddNew();
			testInvoice1.JZ_InvoiceNumber = "INV3";
			testInvoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine11 = CreateInvoiceLine(testInvoice1, testInst, "1", 5);
			var invoiceLine12 = CreateInvoiceLine(testInvoice1, testInst, "1", 0);
			var invoiceLine13 = CreateInvoiceLine(testInvoice1, testInst, "1", 5);

			var testInvoice2 = declaration.Invoices.AddNew();
			testInvoice2.JZ_InvoiceNumber = "INV1";
			testInvoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine21 = CreateInvoiceLine(testInvoice2, testInst, "2", 0);
			var invoiceLine22 = CreateInvoiceLine(testInvoice2, testInst, "2", 5);
			var invoiceLine23 = CreateInvoiceLine(testInvoice2, testInst, "2", 6);

			var testInvoice3 = declaration.Invoices.AddNew();
			testInvoice3.JZ_InvoiceNumber = "INV2";
			testInvoice3.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine31 = CreateInvoiceLine(testInvoice3, testInst, "2", 0);
			invoiceLine31.JI_LineNo = 2;
			var invoiceLine32 = CreateInvoiceLine(testInvoice3, testInst, "2", 0);
			invoiceLine32.JI_LineNo = 3;
			var invoiceLine33 = CreateInvoiceLine(testInvoice3, testInst, "2", 0);
			invoiceLine33.JI_LineNo = 1;

			declaration.DoMerge();
			var header = declaration.CustomsEntryHeaders[0];
			AssertEquals(9, header.MergedLines.Count);
			CombineAssertions("Post-Merge - BeforeSending", () =>
			{
				AssertEquals(0, header.Messages.Count);
				AssertEquals("linked_11", new ZShort(11), invoiceLine11.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_12", new ZShort(12), invoiceLine12.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_13", new ZShort(13), invoiceLine13.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_21", new ZShort(07), invoiceLine21.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_22", new ZShort(05), invoiceLine22.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_23", new ZShort(06), invoiceLine23.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_31", new ZShort(09), invoiceLine31.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_32", new ZShort(10), invoiceLine32.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_33", new ZShort(08), invoiceLine33.CusEntryLine.CL_LineNumber);

				AssertEquals("backpop_11", new ZShort(5), invoiceLine11.JI_TargetEntryLineNumber);
				AssertEquals("backpop_12", new ZShort(0), invoiceLine12.JI_TargetEntryLineNumber);
				AssertEquals("backpop_13", new ZShort(5), invoiceLine13.JI_TargetEntryLineNumber);
				AssertEquals("backpop_21", new ZShort(0), invoiceLine21.JI_TargetEntryLineNumber);
				AssertEquals("backpop_22", new ZShort(5), invoiceLine22.JI_TargetEntryLineNumber);
				AssertEquals("backpop_23", new ZShort(6), invoiceLine23.JI_TargetEntryLineNumber);
				AssertEquals("backpop_31", new ZShort(0), invoiceLine31.JI_TargetEntryLineNumber);
				AssertEquals("backpop_32", new ZShort(0), invoiceLine32.JI_TargetEntryLineNumber);
				AssertEquals("backpop_33", new ZShort(0), invoiceLine33.JI_TargetEntryLineNumber);
			});
			var messageSendingObject = new MessageSendingObject(header);
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			CombineAssertions("Fail Sending", () =>
			{
				AssertEquals(0, header.Messages.Count);
				AssertEquals("linked_11", new ZShort(11), invoiceLine11.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_12", new ZShort(12), invoiceLine12.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_13", new ZShort(13), invoiceLine13.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_21", new ZShort(07), invoiceLine21.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_22", new ZShort(05), invoiceLine22.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_23", new ZShort(06), invoiceLine23.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_31", new ZShort(09), invoiceLine31.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_32", new ZShort(10), invoiceLine32.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_33", new ZShort(08), invoiceLine33.CusEntryLine.CL_LineNumber);

				AssertEquals("backpop_11", new ZShort(5), invoiceLine11.JI_TargetEntryLineNumber);
				AssertEquals("backpop_12", new ZShort(0), invoiceLine12.JI_TargetEntryLineNumber);
				AssertEquals("backpop_13", new ZShort(5), invoiceLine13.JI_TargetEntryLineNumber);
				AssertEquals("backpop_21", new ZShort(0), invoiceLine21.JI_TargetEntryLineNumber);
				AssertEquals("backpop_22", new ZShort(5), invoiceLine22.JI_TargetEntryLineNumber);
				AssertEquals("backpop_23", new ZShort(6), invoiceLine23.JI_TargetEntryLineNumber);
				AssertEquals("backpop_31", new ZShort(0), invoiceLine31.JI_TargetEntryLineNumber);
				AssertEquals("backpop_32", new ZShort(0), invoiceLine32.JI_TargetEntryLineNumber);
				AssertEquals("backpop_33", new ZShort(0), invoiceLine33.JI_TargetEntryLineNumber);
			});
			Factory.Save();
			notification = new MessageNotificationCollector_ForTest();
			messageSendingObject = new MessageSendingObject(header);
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			CombineAssertions("Success Sending", () =>
			{
				AssertEquals(1, header.Messages.Count);
				AssertEquals("linked_11", new ZShort(11), invoiceLine11.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_12", new ZShort(12), invoiceLine12.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_13", new ZShort(13), invoiceLine13.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_21", new ZShort(07), invoiceLine21.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_22", new ZShort(05), invoiceLine22.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_23", new ZShort(06), invoiceLine23.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_31", new ZShort(09), invoiceLine31.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_32", new ZShort(10), invoiceLine32.CusEntryLine.CL_LineNumber);
				AssertEquals("linked_33", new ZShort(08), invoiceLine33.CusEntryLine.CL_LineNumber);

				AssertEquals("backpop_11", new ZShort(05), invoiceLine11.JI_TargetEntryLineNumber);
				AssertEquals("backpop_12", new ZShort(12), invoiceLine12.JI_TargetEntryLineNumber);
				AssertEquals("backpop_13", new ZShort(05), invoiceLine13.JI_TargetEntryLineNumber);
				AssertEquals("backpop_21", new ZShort(07), invoiceLine21.JI_TargetEntryLineNumber);
				AssertEquals("backpop_22", new ZShort(05), invoiceLine22.JI_TargetEntryLineNumber);
				AssertEquals("backpop_23", new ZShort(06), invoiceLine23.JI_TargetEntryLineNumber);
				AssertEquals("backpop_31", new ZShort(09), invoiceLine31.JI_TargetEntryLineNumber);
				AssertEquals("backpop_32", new ZShort(10), invoiceLine32.JI_TargetEntryLineNumber);
				AssertEquals("backpop_33", new ZShort(08), invoiceLine33.JI_TargetEntryLineNumber);
			});
		}

		[TestDate(2016, 6, 01)]
		public void TestUpdatingOfPermits_ValidateWithLastSentPermitRecord()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);
			SetupUpdatingOfPermits(startDate, endDate, out OrgHeader importer, out TariffView tariff);
			var permitHelper = new PermitTestDataHelper(Factory);
			permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, ZString.Empty, 1000m, 1000m, "NO");
			Factory.Save();
			var declaration = CreateDeclarationWithMessageInitiator();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsOffice = "KFN";
			declaration.JE_AGTCode = "00626126";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = CreateInvoiceLocalCurrencyConstantCode(declaration);
			CreateInvoiceLineUpdatingOfPermits(invoice, instruction, tariff.ZZ1_TariffCode, 2000m, 2000);
			declaration.DoMerge();

			AssertEquals("One header generated", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("One line generated", 1, entryHeader.MergedLines.Count);
			var entryLine = entryHeader.MergedLines[0];
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, "DTI2014/7656");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, "2000");
			Factory.Save();

			var messageSendingObject = new MessageSendingObject(entryHeader);
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertContains("Greater than Permit balance"
				, "The declared customs value, 4000.00, for permit DTI2014/7656 is greater than the permit value balance of 1000.00."
				, notification.ErrorNotificationsAsString);

			var testMessage = (CUSDECEDIMessage)entryHeader.Messages.AddNew(typeof(CUSDECEDIMessage));
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			testMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			testMessage.EM_MessageText = TestLastSentPermitRecordMessage.Replace("\r\n", "'");
			testMessage.EM_MessageNum = "202";

			messageSendingObject = new MessageSendingObject(entryHeader);
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			notification.Clear();
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertNotContains("Greater than Permit balance"
				, "The declared customs value, 4000.00, for permit DTI2014/7656 is greater than the permit value balance of 1000.00."
				, notification.ErrorNotificationsAsString);
		}

		[TestDate(2016, 6, 01)]
		public void TestUpdatingOfPermits()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);
			SetupUpdatingOfPermits(startDate, endDate, out OrgHeader importer, out TariffView tariff);
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m, "NO");
			var permit2 = permitHelper.CreatePermitHeader(importer.PK, "RCC00001", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, PermitSubTypeList.Codes.ACO, 1000m, 1000m);
			var permit3 = permitHelper.CreatePermitHeader(importer.PK, "RCC00002", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.VALA, PermitSubTypeList.Codes.LVE, 1000m, 1000m);
			var permit4 = permitHelper.CreatePermitHeader(importer.PK, "RCC00003", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.PRC, PermitSubTypeList.Codes.LVE, 1000m, 1000m);
			Factory.Save();
			var declaration = CreateDeclarationWithMessageInitiator();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsOffice = "KFN";
			declaration.JE_AGTCode = "00626126";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = CreateInvoiceLocalCurrencyConstantCode(declaration);
			var invoiceLine1 = CreateInvoiceLineUpdatingOfPermits(invoice, instruction, tariff.ZZ1_TariffCode, 2000.00m, 2000);
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("One header generated", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var messageSendingObject = new MessageSendingObject(entryHeader);
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			Assert("Greater than Permit balance", ((IMessageNotificationCollector)notification).Notifications.ContainsError(@"Can not send Create for 00626126KFN20160601000004 due to error:
The declared customs value, 2000.00, for permit DTI2014/7656 is greater than the permit value balance of 1000.00.
The declared customs quantity, 2000.00, for permit DTI2014/7656 is greater than the permit quantity balance of 1000.00."));

			invoiceLine1.JI_LinePrice = 100.00m;
			invoiceLine1.JI_CustomsQuantity = 150;
			declaration.DoMerge();
			AssertEquals("One line generated", 1, entryHeader.MergedLines.Count);
			var entryLine = entryHeader.MergedLines[0];
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, "RCC00001");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, "100");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.ProductionRebateCertificate, "LVERCC00002");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue, "300");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, "RCC00001");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, "100");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.ProductionRebateCertificate, "LVERCC00003");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue, "40000");
			Factory.Save();

			notification = new MessageNotificationCollector_ForTest();
			messageSendingObject = new MessageSendingObject(entryHeader);
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertEquals("One message created", 1, entryHeader.Messages.Count);
			var ediMessage = entryHeader.Messages[0];

			AssertUpdatingOfPermitsTransactionsMustBeCreatedForOriginal(permitHelper, entryHeader, ediMessage);

			invoiceLine1.JI_LinePrice = 50.00m;
			invoiceLine1.JI_CustomsQuantity = 100;
			declaration.DoMerge();
			AssertEquals("One line generated", 1, entryHeader.MergedLines.Count);
			entryLine = entryHeader.MergedLines[0];
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, "RCC00001");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, "200");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.ProductionRebateCertificate, "LVERCC00002");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue, "250");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, "RCC00001");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, "100");
			Factory.Save();

			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertEquals("Two message created", 2, entryHeader.Messages.Count);
			ediMessage = entryHeader.Messages[1];
			AssertUpdatingOfPermitsTransactionsMustBeCreatedForChangeWhenSubmitted(permitHelper, entryHeader, ediMessage);

			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = CreateUpdatingOfPermitsTestMessage(entryHeader, ediMessage, "IN1", "1"); //Response 1 RELEASE
			var proc = new MessageProcessor.CUSRESMessageProcessor(logger);
			proc.PreProcessMessage(testMessage);
			proc.ProcessMessage(testMessage);
			Factory.Save();
			AssertUpdatingOfPermitsTransactionsStatusShouldChangeWhenCleared(permitHelper, entryHeader, ediMessage, permit4);

			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertEquals("Three message created", 4, entryHeader.Messages.Count);
			ediMessage = entryHeader.Messages[3];

			CombineAssertions("Permit Transaction must be created for Cancellation only when Accepted", () =>
			{
				var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, ediMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("No adjustment transactions for CNL message", 0, transactions.Length);
			});

			testMessage = CreateUpdatingOfPermitsTestMessage(entryHeader, ediMessage, "IN2", "28"); //Response 28 CANCELLATION ACCEPTED
			proc.PreProcessMessage(testMessage);
			proc.ProcessMessage(testMessage);
			Factory.Save();
			AssertUpdatingOfPermitsTransactionsMustBeCreatedWhenCancelationAccepted(permitHelper, testMessage, entryHeader, permit1, permit2, permit3);
			ErrorReporter.Clear();
		}

		void AssertUpdatingOfPermitsTransactionsMustBeCreatedForOriginal(PermitTestDataHelper permitHelper, CusEntryHeader entryHeader, ZAMessage ediMessage)
		{
			CombineAssertions("Permit Transactions must be created for Original", () =>
			{
				var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, ediMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("Four (new) transactions for message (IMP, RCC, VALA and PRC)", 4, transactions.Length);
				AssertEquals("RCC00001: Transaction Value", -200.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00001").CPL_TranValue);
				AssertEquals("RCC00002: Transaction Value", -300.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00002").CPL_TranValue);
				AssertEquals("RCC00003: Transaction Value", -400.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00003").CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Value", -100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Quantity", -150m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranQty);
			});
		}

		void AssertUpdatingOfPermitsTransactionsMustBeCreatedForChangeWhenSubmitted(PermitTestDataHelper permitHelper, CusEntryHeader entryHeader, ZAMessage ediMessage)
		{
			CombineAssertions("Permit Transactions must be created for Change when Submitted", () =>
			{
				var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, ediMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("Four adjustment transactions for message (RCC)", 4, transactions.Length);
				AssertEquals("DTI2014/7656: Transaction Value decreased", 50.00m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Quantity decreased", 50m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranQty);
				AssertEquals("RCC00001: Transaction Value increased", -100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00001").CPL_TranValue);
				AssertEquals("RCC00002: Transaction Value decreased", 50.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00002").CPL_TranValue);
				AssertEquals("RCC00003: Transaction Value vanished", 400.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00003").CPL_TranValue);
			});
		}

		void AssertUpdatingOfPermitsTransactionsStatusShouldChangeWhenCleared(PermitTestDataHelper permitHelper, CusEntryHeader entryHeader, ZAMessage ediMessage, CusPermitHeader permit)
		{
			CombineAssertions("Permit Transactions status should change when Change is Cleared", () =>
			{
				var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, ediMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("Four transactions for message (1x IMP, 2x new RCC, 1x existing RCC)", 4, transactions.Length);
				AssertEquals("RCC00002: Transaction Value", 50.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00002").CPL_TranValue);
				AssertEquals("RCC00003: Transaction Value", 400.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00003").CPL_TranValue);
				AssertEquals("RCC00003: No effect on Opening Balance (VAL)", 1000m, permit.ValueBalance);
				AssertEquals("DTI2014/7656: Transaction Value", 50.00m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Quantity", 50m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranQty);
			});
		}

		void AssertUpdatingOfPermitsTransactionsMustBeCreatedWhenCancelationAccepted(PermitTestDataHelper permitHelper, CUSRESEDIMessageForTest testMessage, CusEntryHeader entryHeader, CusPermitHeader permit1, CusPermitHeader permit2, CusPermitHeader permit3)
		{
			CombineAssertions("Permit Transaction must be created when Cancellation is Accepted", () =>
			{
				var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, testMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("RCC00001: Transaction Value", 300.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00001").CPL_TranValue);
				AssertEquals("RCC00001: No effect on Opening Balance (VAL)", 1000m, permit2.ValueBalance);
				AssertEquals("RCC00002: Transaction Value", 250.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00002").CPL_TranValue);
				AssertEquals("RCC00002: No effect on Opening Balance (VAL)", 1000m, permit3.ValueBalance);
				AssertEquals("DTI2014/7656: Transaction Value", 50.00m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Quantity", 100m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranQty);
				AssertEquals("DTI2014/7656: No effect on Opening Balance (VAL)", 1000m, permit1.ValueBalance);
				AssertEquals("DTI2014/7656: No effect on Opening Balance (QTY)", 1000m, permit1.QuantityBalance);
			});
		}

		[TestDate(2016, 6, 01)]
		public void TestUpdatingOfPermits_ValidationShouldBeBasedOnQtyValIndicator()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);
			SetupUpdatingOfPermits(startDate, endDate, out OrgHeader importer, out TariffView tariff);
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m, "NO");
			Factory.Save();
			var declaration = CreateDeclarationWithMessageInitiator();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsOffice = "KFN";
			declaration.JE_AGTCode = "00626126";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = CreateInvoiceLocalCurrencyConstantCode(declaration);
			CreateInvoiceLineUpdatingOfPermits(invoice, instruction, tariff.ZZ1_TariffCode, 2000.00m, 2000);
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("One header generated", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];

			var messageSendingObject = new MessageSendingObject(entryHeader);
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertEquals("Greater than Permit balance"
				, @"Can not send Create for 00626126KFN20160601000004 due to error:
The declared customs value, 2000.00, for permit DTI2014/7656 is greater than the permit value balance of 1000.00.
The declared customs quantity, 2000.00, for permit DTI2014/7656 is greater than the permit quantity balance of 1000.00."
				, notification.ErrorNotificationsAsString);

			permit1.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			notification.Clear();
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertEquals("Greater than Permit balance"
				, @"Can not send Create for 00626126KFN20160601000004 due to error:
The declared customs value, 2000.00, for permit DTI2014/7656 is greater than the permit value balance of 1000.00."
				, notification.ErrorNotificationsAsString);

			permit1.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
			notification.Clear();
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertEquals("Greater than Permit balance"
				, @"Can not send Create for 00626126KFN20160601000004 due to error:
The declared customs quantity, 2000.00, for permit DTI2014/7656 is greater than the permit quantity balance of 1000.00."
				, notification.ErrorNotificationsAsString);
		}

		[TestDate(2016, 5, 31)]
		public void TestUpdatingOfPermits_AssessmentDateChangePermit()
		{
			var helper2 = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var tariffType1P1 = helper2.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var rateType_ZA_DTY = helper2.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Enterprise.Customs.Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper2.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			Factory.Save();
			helper.CreateCustomsOfficeCusCodeEntry("KFN");
			helper.CreateCustomsStatusCusCodeEntry("1");
			var importer = CreateOrganisation("IMPORTERADDR1");
			helper2.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "", "DESC", ZAJobMessageTypeList.Codes.Import);
			var startDate1 = new ZDateTime(2016, 01, 01);
			var endDate1 = new ZDateTime(2016, 05, 31);
			var startDate2 = new ZDateTime(2016, 06, 01);
			var endDate2 = new ZDateTime(2016, 12, 31);
			var tariff = helper2.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "8888999900", startDate1, endDate2);
			helper2.CreateRate(tariff, rateCode_ZA_DTY_D.PK, startDate1, endDate2, "0.01 * VFD");
			var permitHelper = new PermitTestDataHelper(Factory);
			permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate1.Date, endDate1.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m, "NO");
			permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate2.Date, endDate2.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m, "NO");
			permitHelper.CreatePermitHeader(importer.PK, "RCC00001", startDate1.Date, endDate1.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, PermitSubTypeList.Codes.LVE, 1000m, 1000m);
			permitHelper.CreatePermitHeader(importer.PK, "RCC00001", startDate2.Date, endDate2.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, PermitSubTypeList.Codes.LVE, 1000m, 1000m);
			Factory.Save();

			var declaration = CreateDeclarationWithMessageInitiator();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsOffice = "KFN";
			declaration.JE_AGTCode = "00626126";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = CreateInvoiceLocalCurrencyConstantCode(declaration);
			CreateInvoiceLineUpdatingOfPermits(invoice, instruction, tariff.ZZ1_TariffCode, 100.00m, 150);
			declaration.DoMerge();

			AssertEquals("One header generated", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626166CLP20160426000199";

			AssertEquals("One line generated", 1, entryHeader.MergedLines.Count);
			var entryLine = entryHeader.MergedLines[0];
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, "RCC00001");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, "100");
			Factory.Save();

			var messageSendingObject = new MessageSendingObject(entryHeader);
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertEquals("One message created", 1, entryHeader.Messages.Count);
			var ediMessage = entryHeader.Messages[0];
			AssertUpdatingOfPermitsAssessmentDateChangePermitTransactionsMustBeCreatedForOriginal(permitHelper, entryHeader, ediMessage);

			instruction.CEI_DateForDuty = new ZDateTime(2016, 06, 01);
			declaration.DoMerge();
			AssertEquals("One line generated", 1, entryHeader.MergedLines.Count);
			entryLine = entryHeader.MergedLines[0];
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, "RCC00001");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, "100");
			Factory.Save();
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertEquals("Two message created", 2, entryHeader.Messages.Count);
			ediMessage = entryHeader.Messages[1];
			AssertUpdatingOfPermitsAssessmentDateChangePermitTransactionsThatDecreaseAvailableMustBeCreatedForChangeWhenSubmitted(permitHelper, entryHeader, ediMessage, startDate1, startDate2);

			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = CreateUpdatingOfPermitsTestMessage(entryHeader, ediMessage, "IN1", "1"); //Response 1 RELEASE
			new MessageProcessor.CUSRESMessageProcessor(logger).ProcessMessage(testMessage);
			Factory.Save();
			AssertUpdatingOfPermitsAssessmentDateChangePermitTransactionsThatIncreaseMustBeCreatedWhenCleared(permitHelper, entryHeader, ediMessage);
			ErrorReporter.Clear();
		}

		void AssertUpdatingOfPermitsAssessmentDateChangePermitTransactionsMustBeCreatedForOriginal(PermitTestDataHelper permitHelper, CusEntryHeader entryHeader, ZAMessage ediMessage)
		{
			CombineAssertions("Permit Transactions must be created for Original", () =>
			{
				var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, ediMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("Two (new) transactions for message (IMP, and RCC)", 2, transactions.Length);
				AssertEquals("RCC00001: Transaction Value", -100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00001").CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Value", -100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Quantity", -150m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranQty);
			});
		}

		void AssertUpdatingOfPermitsAssessmentDateChangePermitTransactionsThatDecreaseAvailableMustBeCreatedForChangeWhenSubmitted(PermitTestDataHelper permitHelper, CusEntryHeader entryHeader, ZAMessage ediMessage, ZDateTime startDate1, ZDateTime startDate2)
		{
			CombineAssertions("Permit Transactions that decrease available must be created for Change when Submitted", () =>
			{
				var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, ediMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("Four adjustment transactions for message (1x IMP, and 1x RCC)", 4, transactions.Length);
				AssertEquals("RCC00001: Transaction Value", 100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00001" && x.PermitHeader.CPH_StartDate == startDate1).CPL_TranValue);
				AssertEquals("RCC00001: Transaction Value", -100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00001" && x.PermitHeader.CPH_StartDate == startDate2).CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Value", 100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656" && x.PermitHeader.CPH_StartDate == startDate1).CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Quantity", 150m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656" && x.PermitHeader.CPH_StartDate == startDate1).CPL_TranQty);
				AssertEquals("DTI2014/7656: Transaction Value", -100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656" && x.PermitHeader.CPH_StartDate == startDate2).CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Quantity", -150m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656" && x.PermitHeader.CPH_StartDate == startDate2).CPL_TranQty);
			});
		}

		void AssertUpdatingOfPermitsAssessmentDateChangePermitTransactionsThatIncreaseMustBeCreatedWhenCleared(PermitTestDataHelper permitHelper, CusEntryHeader entryHeader, ZAMessage ediMessage)
		{
			CombineAssertions("Permit Transactions that increase available must be created when Change is Cleared", () =>
			{
				var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, ediMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("Four adjustment transactions for message (1x IMP, 1x new IMP, 1x RCC, 1x new RCC)", 4, transactions.Length);
				AssertEquals("RCC00001: Transaction Value", -100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00001").CPL_TranValue);
				AssertEquals("RCC00001: Transaction Value", 100.00m, transactions.Where(x => x.PermitHeader.CPH_Number == "RCC00001").ToArray()[1].CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Value", -100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Quantity", -150m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranQty);
				AssertEquals("DTI2014/7656: Transaction Value", 100.00m, transactions.Where(x => x.PermitHeader.CPH_Number == "DTI2014/7656").ToArray()[1].CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Quantity", 150m, transactions.Where(x => x.PermitHeader.CPH_Number == "DTI2014/7656").ToArray()[1].CPL_TranQty);
			});
		}

		[TestDate(2016, 6, 01)]
		public void TestUpdatingOfPermits_DuplicateOriginal()
		{
			var helper2 = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var tariffType1P1 = helper2.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var rateType_ZA_DTY = helper2.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Enterprise.Customs.Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper2.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			Factory.Save();
			helper.CreateCustomsOfficeCusCodeEntry("KFN");
			var importer = CreateOrganisation("IMPORTERADDR1");
			helper2.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "", "DESC", ZAJobMessageTypeList.Codes.Import);
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);
			var tariff = helper2.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "8888999900", startDate, endDate);
			helper2.CreateRate(tariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.01 * VFD");
			var permitHelper = new PermitTestDataHelper(Factory);
			permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m, "NO");
			permitHelper.CreatePermitHeader(importer.PK, "RCC00001", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, PermitSubTypeList.Codes.LVE, 1000m, 1000m);
			Factory.Save();

			var declaration = CreateDeclarationWithMessageInitiator();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsOffice = "KFN";
			declaration.JE_AGTCode = "00626126";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = CreateInvoiceLocalCurrencyConstantCode(declaration);
			var invoiceLine1 = CreateInvoiceLineUpdatingOfPermits(invoice, instruction, tariff.ZZ1_TariffCode, 100.00m, 150);
			declaration.DoMerge();

			AssertEquals("One header generated", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("One line generated", 1, entryHeader.MergedLines.Count);
			var entryLine = entryHeader.MergedLines[0];
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, "RCC00001");
			entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, "100");
			Factory.Save();

			var messageSendingObject = new MessageSendingObject(entryHeader);
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();
			AssertEquals("One message created", 1, entryHeader.Messages.Count);
			var ediMessage = entryHeader.Messages[0];
			AssertUpdatingOfPermitsDuplicateOriginalTransactionMustBeCreatedForOriginal(permitHelper, entryHeader, ediMessage);

			invoiceLine1.JI_LinePrice = 150.00m;
			declaration.DoMerge();
			Factory.Save();
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			new CUSDECMessageManager(messageSendingObject, notification).SendMessage();

			AssertEquals("Two message created", 2, entryHeader.Messages.Count);
			ediMessage = entryHeader.Messages[1];
			AssertUpdatingOfPermitsDuplicateOriginalTransactionMustBeCreatedForChange(permitHelper, entryHeader, ediMessage);
		}

		void AssertUpdatingOfPermitsDuplicateOriginalTransactionMustBeCreatedForOriginal(PermitTestDataHelper permitHelper, CusEntryHeader entryHeader, ZAMessage ediMessage)
		{
			CombineAssertions("Permit Transaction must be created for Original", () =>
			{
				var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, ediMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("Three (new) transactions for message (IMP, and RCC)", 2, transactions.Length);
				AssertEquals("RCC00001: Transaction Value", -100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00001").CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Value", -100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranValue);
				AssertEquals("DTI2014/7656: Transaction Quantity", -150m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranQty);
			});
		}

		void AssertUpdatingOfPermitsDuplicateOriginalTransactionMustBeCreatedForChange(PermitTestDataHelper permitHelper, CusEntryHeader entryHeader, ZAMessage ediMessage)
		{
			CombineAssertions("Permit Transaction must be created for Change", () =>
			{
				var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, ediMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("Adjustment transactions for message (1x IMP, 1xRCC)", 2, transactions.Length);
				AssertEquals("DTI2014/7656: Transaction Value", -50.00m, transactions.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").CPL_TranValue);
				AssertEquals("RCC00001: Transaction Value", 100.00m, transactions.First(x => x.PermitHeader.CPH_Number == "RCC00001").CPL_TranValue);
			});
		}

		void SetupUpdatingOfPermits(ZDateTime startDate, ZDateTime endDate, out OrgHeader importer, out TariffView tariff)
		{
			var helper2 = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var tariffType1P1 = helper2.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var rateType_ZA_DTY = helper2.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Enterprise.Customs.Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper2.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			Factory.Save();
			helper.CreateCustomsOfficeCusCodeEntry("KFN");
			helper.CreateCustomsStatusCusCodeEntry("1");
			helper.CreateCustomsStatusCusCodeEntry("28");
			importer = CreateOrganisation("IMPORTERADDR1");
			helper2.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "", "DESC", ZAJobMessageTypeList.Codes.Import);
			tariff = helper2.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "8888999900", startDate, endDate);
			helper2.CreateRate(tariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.01 * VFD");
		}

		JobComInvoiceLine CreateInvoiceLineUpdatingOfPermits(JobComInvoiceHeader invoice, CusEntryInstruction instruction, ZString tariffCode, ZDecimal linePrice, ZDecimal customsQty)
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = linePrice;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = tariffCode;
			invoiceLine.JI_CustomsQuantity = customsQty;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_PermitNumber = "DTI2014/7656";
			return invoiceLine;
		}

		CUSRESEDIMessageForTest CreateUpdatingOfPermitsTestMessage(CusEntryHeader entryHeader, ZAMessage ediMessage, ZString messageNum, ZString responseType)
		{
			var message = CUSRESMessageProcessorTest.GetTestMessageResNo(responseType).Replace("00626166CLP20160426000199", entryHeader.CH_BGMReference);
			message = message.Replace("RFF+ACD:202", $"RFF+ACD:{ediMessage.EM_MessageNum}");
			var testMessage = Factory.NewWithValidTestData<CUSRESEDIMessageForTest>();
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = EDIMessage.Status.Queued;
			testMessage.EM_MessageText = message;
			testMessage.EM_SystemCreateTimeUtc = new ZDateTime(2016, 9, 29, 1, 5, 0);
			testMessage.EM_MessageNum = messageNum;
			Factory.Save();
			return testMessage;
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_RCCCertificates()
		{
			var declaration = SetupPopulateMessagesRCCCertificates(excessPermits: false, out CusLineTariffDetail lineDetail);
			AssertHasRowMessageError(lineDetail, ValidationConstants.CusLineTariffDetail.InsufficientPermitValue);

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFees(entryHeader);
			CreateAdditionalInfo(entryLine);

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesRCCCertificates, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_RCCCertificates_TooManyPermits()
		{
			var declaration = SetupPopulateMessagesRCCCertificates(excessPermits: true, out CusLineTariffDetail lineDetail);
			AssertHasRowMessageError(lineDetail, ValidationConstants.CusLineTariffDetail.InsufficientPermitValue);

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			CreateEnryLineWithFees(entryHeader);
			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesRCCCertificatesTooManyPermits, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		[TestDate(2016, 09, 15)]
		public void TestPopulateMessages_RCCCertificates_PermitsWrapAcrossSegments()
		{
			var declaration = SetupPopulateMessagesRCCCertificates(excessPermits: true, out CusLineTariffDetail lineDetail);
			AssertHasRowMessageError(lineDetail, ValidationConstants.CusLineTariffDetail.InsufficientPermitValue);

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFees(entryHeader);
			var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			additionalInfo.CY_Code = "VIN";

			var result = PopulateOriginalMessagesExposed(entryHeader);
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", PopulateMessagesRCCCertificatesPermitsWrapAccrossSegments, result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		JobDeclaration SetupPopulateMessagesRCCCertificates(bool excessPermits, out CusLineTariffDetail lineDetail)
		{
			var universalHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "00", "13A,13B,13C,13D,4", "", "IMP");
			universalHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "", "", "", "IMP");
			var tariffType1P1 = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType4P1 = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P1");
			Factory.Save();
			var testAgent = CreateTestAgent();
			Factory.Save();
			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var testSupplier = CreateOrganisation("SUPPLIERADDR1");
			testSupplier.CustomsCodes.AddNew("CSC", "CSC", Core.Constants.CountryCodes.SouthAfrica);
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(testImporter.PK, "PERMIT1", ZDate.Today, ZDate.Today, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, "", 0m, 40m);
			var permit2 = permitHelper.CreatePermitHeader(testImporter.PK, "PERMIT2", ZDate.Today, ZDate.Today, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, "", 0m, 2m);
			var permit3 = permitHelper.CreatePermitHeader(testImporter.PK, "PERMIT3", ZDate.Today, ZDate.Today, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, "", 0m, 3m);
			var permit4 = default(CusPermitHeader);
			var permit5 = default(CusPermitHeader);
			if (excessPermits)
			{
				permit4 = permitHelper.CreatePermitHeader(testImporter.PK, "PERMIT4", ZDate.Today, ZDate.Today, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, "", 0m, 4m);
				permit5 = permitHelper.CreatePermitHeader(testImporter.PK, "PERMIT5", ZDate.Today, ZDate.Today, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, "", 0m, 4m);
			}
			var tariff = universalHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010102030", ZDateTime.Today, ZDateTime.Today);
			universalHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.SpecifiedMotorVehicle, "true", tariff);
			var tariff4P1 = universalHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P1.PK, "4100101010", ZDateTime.Today, ZDateTime.Today);
			universalHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.PRCC, "true", tariff4P1);
			universalHelper.CreateTariffRelationship(tariff4P1.PK, tariff.CusTariffType.PK, "1010102030");
			Factory.Save();
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_CarrierCode = "ABCD";
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "12";
			instruction.CEI_UCROverride = "6AUCINVINV001S";
			var rcc1 = instruction.RCCCertificates.AddNew();
			rcc1.CY_Code = permit1.CPH_Number;
			var rcc2 = instruction.RCCCertificates.AddNew();
			rcc2.CY_Code = permit2.CPH_Number;
			var rcc3 = instruction.RCCCertificates.AddNew();
			rcc3.CY_Code = permit3.CPH_Number;
			if (excessPermits)
			{
				var rcc4 = instruction.RCCCertificates.AddNew();
				rcc4.CY_Code = permit4.CPH_Number;
				var rcc5 = instruction.RCCCertificates.AddNew();
				rcc5.CY_Code = permit5.CPH_Number;
			}
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 43.75m);
			invoice.JZ_VDN = "223344";

			var invoiceLine1 = CreateInvoiceLine(invoice, instruction, "U", "VAT");
			invoiceLine1.JI_LinePrice = 43.75;
			invoiceLine1.JI_PrimaryPreference = "SADC";
			invoiceLine1.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine1.CusLineTariffDetails.RemoveAndDeleteAll();
			lineDetail = invoiceLine1.CusLineTariffDetails.AddNew(tariff4P1.ZZ1_ZZI_TariffTypeCode, tariff4P1.ZZ1_TariffCode);
			new LineMerger(declaration).DoMerge();
			return declaration;
		}

		[TestDate(2018, 09, 05)]
		public void TestPopulateMessages_AdditionalInfo()
		{
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory, helper);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", "IMP");
			helper.CreateTaxOrFee(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			var testAgent = CreateTestAgent();
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignImporterOrExporter);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount);
			Factory.Save();

			SetupFinancialAccountNumberPortMaps(testAgent.PK);
			var testImporter = CreateOrganisation("IMPORTERADDR1");
			var declaration = CreateDeclaration(ZAJobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Sea, "CAOTT", "HB001");
			declaration.JE_CarrierCode = "ABCD";
			declaration.JE_RL_NKMasterBillIssuedAt = "AUSYD";
			declaration.JE_CargoCarrier = "00626127";
			declaration.JE_OH_Importer = testImporter.PK;
			declaration.JE_OH_AgentOverride = testAgent.PK;

			CreateCusContainer(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_UCROverride = "8AUCINVINV001S";
			var invoice = CreateInvoice(declaration, new ZDateTime(2016, 04, 01), 50000);
			CreateInvoiceLineWithPrimaryReference(invoice, instruction, UniversalReferenceConstants.PrimaryPreference.Standard, "00");
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00626126BFN20160101000001";
			entryHeader.CH_PaymentMethod = "D";
			var entryLine = CreateEnryLineWithFees(entryHeader);
			var additionalInfo = CreateAdditionalInfo(entryLine);
			additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignImporterOrExporter;
			additionalInfo.CY_Data = "20269812";
			additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			additionalInfo.CY_Code = UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount;
			additionalInfo.CY_Data = "2036897";
			{
				var result = PopulateOriginalMessagesExposed(entryHeader);
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesAdditionalInfoOriginal, result[0].EM_MessageText.Replace("'", "\r\n"));
			}
			{
				entryHeader.MovementReferenceNumberSetter("JSA201607041234567", new ZDateTime(2016, 7, 4));
				var result = PopulateCancelationMessagesExposed(entryHeader);
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesAdditionalInfocancelation, result[0].EM_MessageText.Replace("'", "\r\n"));
			}
		}

		[TestDate(2024, 8, 21)]
		public void TestPopulateMessages_InvoiceDetails()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateCusMapType(RefCusMapTypeList.Codes.ChargeCode, MapDirectionList.Codes.OUT, "CW1 Charge Codes to Customs codes", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.AdditionCharge, "160", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.LandingCharges, "78", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.OverseasFreight, "64", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.OverseasInsurance, "67", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.OtherCharges, "160", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SUPPLIER FULL NAME";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			supplier.MainAddress.OA_Address1 = "SUP ADDRESS 1";
			supplier.MainAddress.OA_Address2 = "SUP ADDRESS 2";
			supplier.MainAddress.OA_City = "SUP CITY";

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			usdCurrency.SetCustomsRate(new ZDateTime(2024, 8, 1), new ZDateTime(2024, 8, 31), 0.05602m);

			var declaration = CreateDeclarationWithMessageInitiator();
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2024, 8, 1);
			declaration.JE_OH_Supplier = supplier.PK;

			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "11";
			entryInstruction1.CEI_DateForDuty = new ZDateTime(2024, 8, 1);
			entryInstruction1.CEI_CreditTerms = "23";

			var groupCharge1 = declaration.TopGroupInvoice.Charges.AddNew();
			groupCharge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.OverseasFreight;
			groupCharge1.J7_Amount = 99m;
			groupCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			groupCharge1.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;

			var invoice1 = CreateInvoice(declaration, new ZDateTime(2024, 8, 1), "INV-1234-1", 123.45m);
			invoice1.JZ_InvoiceCurrExRate = 0.1234m;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_PaymentAmount = 12.34m;
			invoice1.JZ_PaymentTerms = "23";

			var invoice1Charge1 = invoice1.Charges.AddNew();
			invoice1Charge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.AdditionCharge;
			invoice1Charge1.J7_Amount = 1.23m;
			invoice1Charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			var invoice1Charge2 = invoice1.Charges.AddNew();
			invoice1Charge2.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.OtherCharges;
			invoice1Charge2.J7_ChargeDescription = "SPECIAL CHARGE FOR BRAND A";
			invoice1Charge2.J7_Amount = 2.34m;
			invoice1Charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoice1Line1 = CreateInvoiceLine(invoice1, entryInstruction1);
			invoice1Line1.JI_Description = "ITEM A";
			invoice1Line1.JI_PartNo = "PART A";
			invoice1Line1.JI_InvoiceQuantity = 11m;
			invoice1Line1.JI_InvoiceUQ = "KG";
			invoice1Line1.JI_LinePrice = 123m;
			invoice1Line1.JI_ValuationMarkup = 11.1m;
			invoice1Line1.JI_BrandName = "BRAND A";

			var invoice1Line1Charge1 = invoice1Line1.Charges.AddNew();
			invoice1Line1Charge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.Discount;
			invoice1Line1Charge1.J7_Amount = 1.23m;
			invoice1Line1Charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			var invoice2 = CreateInvoice(declaration, new ZDateTime(2024, 8, 2), "INV-1234-2", 234.56m);
			invoice2.JZ_InvoiceCurrExRate = 0.2345m;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			invoice2.JZ_PaymentAmount = 23.45m;
			invoice2.JZ_PaymentTerms = "45";

			var invoice2Charge1 = invoice2.Charges.AddNew();
			invoice2Charge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.LandingCharges;
			invoice2Charge1.J7_Amount = 3.45m;
			invoice2Charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			var invoice2Charge2 = invoice2.Charges.AddNew();
			invoice2Charge2.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.OtherCharges;
			invoice2Charge2.J7_ChargeDescription = "SPECIAL CHARGE FOR INVOICE 2";
			invoice2Charge2.J7_Amount = 4.56m;
			invoice2Charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			var invoice2Line1 = CreateInvoiceLine(invoice2, entryInstruction1);
			invoice2Line1.JI_Description = "VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG ITEM B";
			invoice2Line1.JI_InvoiceQuantity = 22m;
			invoice2Line1.JI_InvoiceUQ = "T";
			invoice2Line1.JI_LinePrice = 234m;
			invoice2Line1.JI_ValuationMarkup = 22.2m;
			invoice2Line1.JI_BrandName = "BRAND B";
			invoice2Line1.JI_AdvancePaymentNo = "APN-2-1";

			var invoice2Line2 = CreateInvoiceLine(invoice2, entryInstruction1);
			invoice2Line2.JI_Description = "ITEM C";
			invoice2Line2.JI_InvoiceQuantity = 33m;
			invoice2Line2.JI_InvoiceUQ = "G";
			invoice2Line2.JI_LinePrice = 345m;
			invoice2Line2.JI_ValuationMarkup = 32.9m;
			invoice2Line2.JI_BrandName = "BRAND C";
			invoice2Line2.JI_AdvancePaymentNo = "APN-2-2";

			var invoice2Line2Charge1 = invoice2Line2.Charges.AddNew();
			invoice2Line2Charge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.Discount;
			invoice2Line2Charge1.J7_Amount = 3.33m;
			invoice2Line2Charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			declaration.ResumeApportionment();
			declaration.DoMerge();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
			{
				var result = PopulateOriginalMessagesExposed(declaration.CustomsEntryHeaders[0]);
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesInvoiceDetails, result[0].EM_MessageText.Replace("'", "\r\n"));
			}
		}

		[TestDate(2024, 8, 21)]
		public void TestPopulateMessages_InvoiceDetailsForExbond()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateCusMapType(RefCusMapTypeList.Codes.ChargeCode, MapDirectionList.Codes.OUT, "CW1 Charge Codes to Customs codes", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.AdditionCharge, "160", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.LandingCharges, "78", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.OverseasFreight, "64", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.OverseasInsurance, "67", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.OtherCharges, "160", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SUPPLIER FULL NAME";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			supplier.MainAddress.OA_Address1 = "SUP ADDRESS 1";
			supplier.MainAddress.OA_Address2 = "SUP ADDRESS 2";
			supplier.MainAddress.OA_City = "SUP CITY";

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			usdCurrency.SetCustomsRate(new ZDateTime(2024, 8, 1), new ZDateTime(2024, 8, 31), 0.05602m);

			var declaration = CreateDeclarationWithMessageInitiator(Customs.Business.JobMessageTypeList.Codes.ExWarehouse);
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2024, 8, 1);
			declaration.JE_OH_Supplier = supplier.PK;

			declaration.Invoices.RemoveAll();

			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "46";
			entryInstruction1.CEI_DateForDuty = new ZDateTime(2024, 8, 1);
			entryInstruction1.CEI_CreditTerms = "23";

			var groupCharge1 = declaration.TopGroupInvoice.Charges.AddNew();
			groupCharge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.OverseasFreight;
			groupCharge1.J7_Amount = 99m;
			groupCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			groupCharge1.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;

			var invoice1 = CreateInvoice(declaration, new ZDateTime(2024, 8, 1), "INV-1234-1", 123.45m);
			invoice1.JZ_InvoiceCurrExRate = 0.1234m;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_PaymentAmount = 12.34m;
			invoice1.JZ_PaymentTerms = "23";

			var invoice1Charge1 = invoice1.Charges.AddNew();
			invoice1Charge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.AdditionCharge;
			invoice1Charge1.J7_Amount = 1.23m;
			invoice1Charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			var invoice1Charge2 = invoice1.Charges.AddNew();
			invoice1Charge2.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.OtherCharges;
			invoice1Charge2.J7_ChargeDescription = "SPECIAL CHARGE FOR BRAND A";
			invoice1Charge2.J7_Amount = 2.34m;
			invoice1Charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoice1Line1 = CreateInvoiceLine(invoice1, entryInstruction1);
			invoice1Line1.JI_Description = "ITEM A";
			invoice1Line1.JI_PartNo = "PART A";
			invoice1Line1.JI_InvoiceQuantity = 11m;
			invoice1Line1.JI_InvoiceUQ = "KG";
			invoice1Line1.JI_LinePrice = 123m;
			invoice1Line1.JI_ValuationMarkup = 11.1m;
			invoice1Line1.JI_BrandName = "BRAND A";

			var invoice1Line1Charge1 = invoice1Line1.Charges.AddNew();
			invoice1Line1Charge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.Discount;
			invoice1Line1Charge1.J7_Amount = 1.23m;
			invoice1Line1Charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			var invoice2 = CreateInvoice(declaration, new ZDateTime(2024, 8, 2), "INV-1234-2", 234.56m);
			invoice2.JZ_InvoiceCurrExRate = 0.2345m;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			invoice2.JZ_PaymentAmount = 23.45m;

			var invoice2Charge1 = invoice2.Charges.AddNew();
			invoice2Charge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.LandingCharges;
			invoice2Charge1.J7_Amount = 3.45m;
			invoice2Charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			var invoice2Charge2 = invoice2.Charges.AddNew();
			invoice2Charge2.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.OtherCharges;
			invoice2Charge2.J7_ChargeDescription = "SPECIAL CHARGE FOR INVOICE 2";
			invoice2Charge2.J7_Amount = 4.56m;
			invoice2Charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			var invoice2Line1 = CreateInvoiceLine(invoice2, entryInstruction1);
			invoice2Line1.JI_Description = "VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG ITEM B";
			invoice2Line1.JI_InvoiceQuantity = 22m;
			invoice2Line1.JI_InvoiceUQ = "T";
			invoice2Line1.JI_LinePrice = 234m;
			invoice2Line1.JI_ValuationMarkup = 22.2m;
			invoice2Line1.JI_BrandName = "BRAND B";
			invoice2Line1.JI_AdvancePaymentNo = "APN-2-1";

			var invoice2Line2 = CreateInvoiceLine(invoice2, entryInstruction1);
			invoice2Line2.JI_Description = "ITEM C";
			invoice2Line2.JI_InvoiceQuantity = 33m;
			invoice2Line2.JI_InvoiceUQ = "G";
			invoice2Line2.JI_LinePrice = 345m;
			invoice2Line2.JI_ValuationMarkup = 32.9m;
			invoice2Line2.JI_BrandName = "BRAND C";
			invoice2Line2.JI_AdvancePaymentNo = "APN-2-2";

			var invoice2Line2Charge1 = invoice2Line2.Charges.AddNew();
			invoice2Line2Charge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.Discount;
			invoice2Line2Charge1.J7_Amount = 3.33m;
			invoice2Line2Charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			declaration.ResumeApportionment();
			declaration.DoMerge();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
			{
				var result = PopulateOriginalMessagesExposed(declaration.CustomsEntryHeaders[0]);
				AssertEquals(1, result.Length);
				AssertMultilineASCIIEquals("MessageBody", PopulateMessagesInvoiceDetailsExbond, result[0].EM_MessageText.Replace("'", "\r\n"));
			}
		}

		public override void SetTestMode(bool testMode)
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, testMode);
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			helper.CreateAdditionalInformationCusCodeEntry("BHR");
			helper.CreateAdditionalInformationCusCodeEntry("BND");
			helper.CreateAdditionalInformationCusCodeEntry("IPC");
			helper.CreateAdditionalInformationCusCodeEntry("NUI");
			helper.CreateAdditionalInformationCusCodeEntry("RCC");
			helper.CreateAdditionalInformationCusCodeEntry("RCV");
			helper.CreateAdditionalInformationCusCodeEntry("ROO");
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.AdvancePaymentNo);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}00";

			new LineMerger(declaration).DoMerge();
			return declaration.ActiveEntryHeaders[0];
		}

		protected override ECBM.EDIFACTMessageManager GetMessageManager()
		{
			return new CUSDECMessageManagerForTest(new MessageSendingObject(dataWrapper as CusEntryHeader), new MessageNotificationCollector_ForTest());
		}

		protected override void SetUp()
		{
			helper = new ZAUniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsOfficeCusCodeEntry("BFN");

			base.SetUp();
		}
		ZAUniversalReferenceTestDataHelper helper;
		new MessageNotificationCollector_ForTest notification = new MessageNotificationCollector_ForTest();

		const string TestLastSentPermitRecordMessage = @"UNH+202+CUSDEC:D:96B:UN:ZZZ01
BGM+929+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+ACD:202
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+SU+CSC+++SUPPLIERADDR1
NAD+MS+CDP
UNS+D
CST+0001+1010102030:108:ZZZ
FTX+AAA+++DEFAULT DESCRIPTION
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+AAI+++RCCDTI2014/7656:RCV00000000000000000000000000003040:RCCPERMIT2:RCV00000000000000000000000000000002
FTX+CCI+++12:00:4100101010
LOC+27+CN
MOA+38:44
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:44
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+55+202";
		const string PopulateMessagesMarksAndNumbers = @"TEST1
TEST2TEST3TEST4TEST5TEST6TEST7TEST8TEST9TEST0";
		const string PopulateMessagesOriginal = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+100
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+58+<<MSGNO PLACEHOLDER>>
";
		const string PopulateMessagesCancelation = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+1
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+141:20160915:102
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+F:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++0::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+ABT:JSA201607041234567
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
UNS+D
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:0.00
TAX+3+TVD:107:ZZZ
MOA+161:0.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+44+<<MSGNO PLACEHOLDER>>
";
		const string PopulateMessagesIMPOriginal = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+SU+CSC+++SUPPLIERADDR1
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+200
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:ROO:VINVINDATA
FTX+AAI+++VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+60+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesIMPZeroTotalsChange = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+4
CST++A:117:ZZZ
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+141:20160915:102
GIS+F:134:ZZZ
FTX+LIN+++1::N
RFF+UCN:6CINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+0
TDT+20++1+++++:::             
DOC+380+INV001
DTM+3:20160915:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++NUIN
FTX+CCI+++11:00
MOA+40:1
UNS+S
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:0.00
TAX+3+TVD:107:ZZZ
MOA+161:0.00
TAX+3+CUS:107:ZZZ
MOA+161:1
UNT+32+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesEXP = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+830:::RCD+00626126BFN20160101000001+9
CST++H:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+ZA::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+178:20160102:102
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6ZACSCCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+AG+00626126
NAD+EX+CSC+++SUPPLIERADDR1
NAD+CN++++IMPORTERADDR1
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++60:00:::2
LOC+27+CN
MOA+40:50000
UNS+S
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+41+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesIMPAirNoContainerNoVesselNameOriginal = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+178:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:MB0-01
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+4
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+100
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+57+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesUnregisteredTraderExport = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+830:::RCD+00626126BFN20160101000001+9
CST++H:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+ZA::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+178:20160102:102
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6ZA70707070CINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+AG+00626126
NAD+EX+";
		const string PopulateMessagesUnregisteredTraderExportOriginal = PopulateMessagesUnregisteredTraderExport + ValidationConstants.Declaration.UnregisteredTraderCustomsCode + @"++IMPORTERFULLNAME+SUPPLIERADDR1
NAD+CN++++IMPORTERADDR1
NAD+MS+CDP
NAD+DT+1234567890128:174:ZZZ++IMPORTERFULLNAME+SUPPLIERADDR1
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++60:00:::2
LOC+27+CN
MOA+40:50000
UNS+S
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+42+<<MSGNO PLACEHOLDER>>
";
		const string PopulateMessagesUnregisteredTraderExportOriginalUpdated = PopulateMessagesUnregisteredTraderExport + ValidationConstants.Declaration.UnregisteredTraderCustomsCode + @"++IMPORTERFULLNAME+SUPPLIERADDR1
RFF+VA:4770181941
NAD+CN++++IMPORTERADDR1
NAD+MS+CDP
NAD+DT+1234567890128:174:ZZZ++IMPORTERFULLNAME+SUPPLIERADDR1
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++60:00:::2
LOC+27+CN
MOA+40:50000
UNS+S
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+43+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesUnregisteredTraderExportWithEXPINVCodeOriginal = PopulateMessagesUnregisteredTraderExport + ValidationConstants.Declaration.UnregisteredTraderCustomsCode + @"++IMPORTERFULLNAME+SUPPLIERADDR1
NAD+CN++++IMPORTERADDR1
NAD+MS+CDP
NAD+DT+1234567890128:174:ZZZ++IMPORTERFULLNAME+SUPPLIERADDR1
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++60:00:::2
LOC+27+CN
MOA+40:50000
UNS+S
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+42+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesUnregisteredTraderExportWithEXPINVCodeUpdated = PopulateMessagesUnregisteredTraderExport + ValidationConstants.Declaration.UnregisteredTraderCustomsCode + @"++IMPORTERFULLNAME+SUPPLIERADDR1
RFF+VA:4770181941
NAD+CN++++IMPORTERADDR1
NAD+MS+CDP
NAD+DT+1234567890128:174:ZZZ++IMPORTERFULLNAME+SUPPLIERADDR1
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++60:00:::2
LOC+27+CN
MOA+40:50000
UNS+S
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+43+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesUnregisteredTraderImport = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+178:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:MB0-01
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+4
DOC+380+INV001
DTM+3:20160401:102
NAD+IM+";
		const string PopulateMessagesUnregisteredTraderImportoriginal = PopulateMessagesUnregisteredTraderImport + ValidationConstants.Declaration.UnregisteredTraderCustomsCode + @"++IMPORTERFULLNAME+IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
NAD+DT+1234567890128:174:ZZZ++IMPORTERFULLNAME+IMPORTERADDR1
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+58+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesUnregisteredTraderImportoriginalUpdated = PopulateMessagesUnregisteredTraderImport + ValidationConstants.Declaration.UnregisteredTraderCustomsCode + @"++IMPORTERFULLNAME+IMPORTERADDR1
RFF+VA:4770181941
NAD+AG+00626126
NAD+MS+CDP
NAD+DT+1234567890128:174:ZZZ++IMPORTERFULLNAME+IMPORTERADDR1
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+59+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesUnregisteredTraderImportoriginalUpdated1 = PopulateMessagesUnregisteredTraderImport + ValidationConstants.Declaration.UnregisteredTraderCustomsCode + @"++IMPORTERFULLNAME+IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
NAD+DT+ABC12345678:53:ZZZ++IMPORTERFULLNAME+IMPORTERADDR1
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+58+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesUnregisteredTraderImportoriginalUpdated2 = PopulateMessagesUnregisteredTraderImport + ValidationConstants.Declaration.UnregisteredTraderCustomsCode + @"++IMPORTERFULLNAME+IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
NAD+DT+ABC12345678:53:ZZZ++IMPORTERFULLNAME+IMPORTERADDR1
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+58+<<MSGNO PLACEHOLDER>>";
		const string TestPopulateMessagesUnregisteredTraderImportNull = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+178:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
FTX+LIN+++1
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:MB0-01
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+4
DOC+380+INV001
DTM+3:20160401:102
NAD+AG+00626126
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+56+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesIMPSeaContainerVesselName = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::AANG3FKT9    TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+100
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+58+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesCPC1140 = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+18+100213::ZZZ
LOC+36+CA::5
LOC+96+BFN::ZZZ
DTM+178:20160102:102
GIS+D:134:ZZZ
FTX+LIN+++1::Y
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+ABI:3234002346
RFF+UCN:6CSCCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20
NAD+IM+00000001+++IMPORTERADDR1
RFF+VA:4123456789
NAD+AG+00626126
NAD+MS+CDP
NAD+BY+00000001
UNS+D
CST+0001+899961007:108:ZZZ+100
FTX+AAA+++DEFAULT DESCRIPTION
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:40
LOC+27+CN
MEA+AAR++KG:1.01
MEA+AAS++NO:1.51
MEA+AAT++NO:2.01
MEA+AAF++NO:3
MOA+38:50000
MOA+40:50000
RFF+WE:DBN201501035000012
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+54+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesPaymentTypeNoFAN = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+C:134:ZZZ
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+SU++++SUPPLIERADDR1
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+100
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+58+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesCaseNumberChangeAcknowldgementIndicator = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+SU+CSC+++SUPPLIERADDR1
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+100
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+59+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesCaseNumberChangeAcknowldgementIndicatorChange = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+4
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+141:20160101:102
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+SU+CSC+++SUPPLIERADDR1
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+100
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+60+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesCaseNumberChangeAcknowldgementIndicatorChange1 = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+4
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+141:20160101:102
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N::5
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
RFF+AAV:123321
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+SU+CSC+++SUPPLIERADDR1
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+100
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+61+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesCPC1120 = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+::00001+9
CST++A:117:ZZZ
GIS+F:134:ZZZ
FTX+LIN+++1::N
RFF+UCN:6CINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+0
TDT+20++1+++++:::             
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+100
FTX+ACB+++NUIN
FTX+CCI+++11:20
MOA+38:50000
MOA+40:50000
RFF+WE:BBR201603221234567
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+29+<<MSGNO PLACEHOLDER>>
";
		const string PopulateMessagesCPC40and42 = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++E:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+ZA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:DHLPMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1:3+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+AF+00111110
NAD+SU+CSC+++SUPPLIERADDR1
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+100
FTX+ACB+++BHR00281124:BND00000000000000000000000000000000:DOLDOLDATA:NUIU:PPRPPRDATA
FTX+AAI+++VINVINDATA:VONVONDATA
FTX+CCI+++40:41
LOC+27+CN
MOA+38:50000
MOA+40:50000
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+47+<<MSGNO PLACEHOLDER>>
";
		const string PopulateMessagesCPC66and66and67 = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+830:::RCD+00505655KFN20160602000001+9
CST++H:117:ZZZ
LOC+45+KFN::ZZZ
LOC+35+ZA::5
LOC+36+NA::5
LOC+96+KFN::ZZZ
LOC+9+ZAJNB::5
GIS+F:134:ZZZ
MEA+AAE+AAD+KGM:100.00
FTX+LIN+++1:ADV:Y
RFF+AAS:BOL126
DTM+137:20160602:102
RFF+ABI:8120073229
RFF+UCN:6ZA01143233COTH000000000003S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+10
TDT+20++3+++++:::DE12EEGP                      
DOC+380+JPG125
DTM+3:20160602:102
NAD+AG+00505655
NAD+EX+01143233+++SUPPLIERADDR1
RFF+VA:4420116552
NAD+CN++++IMPORTERADDR1
UNS+D
CST+0001+999910009:108:ZZZ
FTX+AAA+++CARROTS
FTX+ACB+++NUIN
FTX+CCI+++66:12:::1
LOC+27+NA
MEA+AAR++KG:10.00
MOA+40:20
RFF+WE:KFN201606025000110:0001
UNS+S
TAX+3+TRN:107:ZZZ
MOA+161:200:ZAR
TAX+3+CUS:107:ZZZ
MOA+161:20
UNT+39+<<MSGNO PLACEHOLDER>>
";
		const string PopulateMessagesCPC66and66and67WithEXPINVCode = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+830:::RCD+00505655KFN20160602000001+9
CST++H:117:ZZZ
LOC+45+KFN::ZZZ
LOC+35+ZA::5
LOC+36+NA::5
LOC+96+KFN::ZZZ
LOC+9+ZAJNB::5
GIS+F:134:ZZZ
MEA+AAE+AAD+KGM:100.00
FTX+LIN+++1:ADV:Y
RFF+AAS:BOL126
DTM+137:20160602:102
RFF+ABI:8120073229
RFF+UCN:6ZA01143233COTH000000000003S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+10
TDT+20++3+++++:::DE12EEGP                      
DOC+380+JPG125
DTM+3:20160602:102
NAD+AG+00505655
NAD+EX+01143233+++SUPPLIERADDR1
RFF+VA:4420116552
NAD+CN++++IMPORTERADDR1
UNS+D
CST+0001+999910009:108:ZZZ
FTX+AAA+++CARROTS
FTX+ACB+++NUIN
FTX+CCI+++66:12:::1
LOC+27+NA
MEA+AAR++KG:10.00
MOA+40:20
RFF+WE:KFN201606025000110:0001
UNS+S
TAX+3+TRN:107:ZZZ
MOA+161:200:ZAR
TAX+3+CUS:107:ZZZ
MOA+161:20
UNT+39+<<MSGNO PLACEHOLDER>>
";
		const string PopulateMessagesRCCCertificates = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+SU+CSC+++SUPPLIERADDR1
NAD+MS+CDP
UNS+D
CST+0001+1010102030:108:ZZZ
FTX+AAA+++DEFAULT DESCRIPTION
FTX+ACB+++DOLDOLDATA:NUIU:PPRPPRDATA:VINVINDATA:VONVONDATA
FTX+AAI+++RCCPERMIT1:RCV00000000000000000000000000000040:RCCPERMIT2:RCV00000000000000000000000000000002
FTX+CCI+++12:00:4100101010
LOC+27+CN
MOA+38:44
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:44
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+58+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesRCCCertificatesTooManyPermits = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+SU+CSC+++SUPPLIERADDR1
NAD+MS+CDP
UNS+D
CST+0001+1010102030:108:ZZZ
FTX+AAA+++DEFAULT DESCRIPTION
FTX+ACB+++NUIU:RCCPERMIT1:RCV00000000000000000000000000000040:RCCPERMIT2:RCV00000000000000000000000000000002
FTX+AAI+++RCCPERMIT3:RCV00000000000000000000000000000003:RCCPERMIT4:RCV00000000000000000000000000000004
FTX+CCI+++12:00:4100101010
LOC+27+CN
MOA+38:44
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:44
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+58+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesRCCCertificatesPermitsWrapAccrossSegments = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:6AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+SU+CSC+++SUPPLIERADDR1
NAD+MS+CDP
UNS+D
CST+0001+1010102030:108:ZZZ
FTX+AAA+++DEFAULT DESCRIPTION
FTX+ACB+++NUIU:VIN:RCCPERMIT1:RCV00000000000000000000000000000040:RCCPERMIT2
FTX+AAI+++RCV00000000000000000000000000000002:RCCPERMIT3:RCV00000000000000000000000000000003:RCCPERMIT4:RCV00000000000000000000000000000004
FTX+CCI+++12:00:4100101010
LOC+27+CN
MOA+38:44
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:44
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+58+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesAdditionalInfoOriginal = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+9
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+D:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++1::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:8AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
UNS+D
CST+0001+:108:ZZZ+100
FTX+ACB+++AFT00000000000000000000000020269812:BND00000000000000000000000002036897:DOLDOLDATA:NUIU:PPRPPRDATA
FTX+AAI+++VINVINDATA:VONVONDATA
FTX+CCI+++11:00
LOC+27+CN
MOA+38:50000
MOA+40:50000
TAX+1+VAT:107:ZZZ
MOA+161:2000.00
TAX+1+1P1:107:ZZZ
MOA+161:1000.00
TAX+1+12A:107:ZZZ
MOA+161:1001.00
TAX+1+12B:107:ZZZ
MOA+161:1002.00
TAX+1+13A:107:ZZZ
MOA+161:1003.00
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:4006.00
TAX+3+TVD:107:ZZZ
MOA+161:2000.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+59+<<MSGNO PLACEHOLDER>>
";
		const string PopulateMessagesAdditionalInfocancelation = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+00626126BFN20160101000001::00001+1
CST++A:117:ZZZ
LOC+14+TE::ZZZ
LOC+35+AU::5
LOC+36+CA::5
LOC+96+BFN::ZZZ
LOC+9+AUSYD::5
DTM+141:20180905:102
DTM+132:20160201:102
GIS+R1:127:ZZZ
GIS+F:134:ZZZ
EQD+CN+NONU-CONTNUM+:::SEAL1
FTX+LIN+++0::N
RFF+BH:00626127HB001
DTM+137:20160301:102
RFF+ABT:JSA201607041234567
RFF+AAS:ABCDMB001
DTM+137:20160228:102
RFF+ABI:3234002346
RFF+UCN:8AUCINVINV001S
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+15
PCI++TEST1
PCI++TEST2TEST3TEST4TEST5TEST6TEST7TEST8:TEST9TEST0
TDT+20+VOY+1+++++:::             TESTVESSEL
DOC+380+INV001
DTM+3:20160401:102
NAD+IM++++IMPORTERADDR1
NAD+AG+00626126
NAD+MS+CDP
UNS+D
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:50000
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+TDD:107:ZZZ
MOA+161:0.00
TAX+3+TVD:107:ZZZ
MOA+161:0.00
TAX+3+CUS:107:ZZZ
MOA+161:50000
UNT+44+<<MSGNO PLACEHOLDER>>
";
		const string PopulateMessagesInvoiceDetails = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+::00001+9
CST++:117:ZZZ
LOC+35+AU::5
DTM+111:20240801:102
GIS+F:134:ZZZ
FTX+LIN+++3
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+0
TDT+20++9
NAD+SU+++SUPPLIER FULL NAME+SUP ADDRESS 1 SUP ADDRESS 2 SUP CIT:Y NSW AUSTRALIA+SUP CITY
UNS+D
DMS+INV-1234-1+380
DTM+3:20240801:102
MOA+39:123.45:ZAR
CUX+++0.1234
DTM+134:20240801:102
MOA+259:61.58:ZAR
CUX++5+1.35312474
TOD+6++FOB
NAD+II++SUPPLIER FULL NAME:SUP ADDRESS 1:SUP ADDRESS 2:SUP CITY:NEW SOUTH WALES++++++AU
PAT+1++9::D:23
MOA+74:12.34:ZAR
ALC+C
MOA+160:1.23:ZAR
CUX+1:ZAR++1.0000
ALC+C++++:::SPECIAL CHARGE FOR BRAND A
MOA+160:2.34:USD
CUX+1:USD++0.0560
ALC+C
MOA+64:17.35:ZAR
CUX+1:ZAR++1.0000
LIN+1+++:1
PIA+5+PART A
QTY+47:11.0000:KG
PRI+INV:11.1818
MOA+38:123.00
ALC+A
RTE+1:11
MOA+52:1.2300:ZAR
IMD+A++:::BRAND A
FTX+IND+++ITEM A
DMS+INV-1234-2+380
DTM+3:20240802:102
MOA+39:234.56:ZAR
CUX+++0.2345
DTM+134:20240801:102
MOA+259:92.99:ZAR
CUX++5+1.0079212
TOD+6++EXW
NAD+II++SUPPLIER FULL NAME:SUP ADDRESS 1:SUP ADDRESS 2:SUP CITY:NEW SOUTH WALES++++++AU
PAT+1++9::D:45
MOA+74:23.45:ZAR
FTX+PMT+++APN-2-1:APN-2-2
ALC+C
MOA+78:3.45:ZAR
CUX+1:ZAR++1.0000
ALC+C++++:::SPECIAL CHARGE FOR INVOICE 2
MOA+160:4.56:ZAR
CUX+1:ZAR++1.0000
ALC+C
MOA+64:81.65:ZAR
CUX+1:ZAR++1.0000
LIN+1+++:2
PIA+5+N/A
QTY+47:22.0000:T
PRI+INV:10.6364
MOA+38:234.00
ALC+A
RTE+1:22
IMD+A++:::BRAND B
FTX+IND+++VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :ITEM B
LIN+2+++:3
PIA+5+N/A
QTY+47:33.0000:G
PRI+INV:10.4545
MOA+38:345.00
ALC+A
RTE+1:32
MOA+52:3.3300:ZAR
IMD+A++:::BRAND C
FTX+IND+++ITEM C
CST+0001+:108:ZZZ
FTX+AAA+++ITEM A
FTX+ACB+++NUIN
FTX+CCI+++11:00
MOA+38:165
MOA+40:183
CST+0002+:108:ZZZ
FTX+AAA+++VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :ITEM B
FTX+ACB+++APNAPN-2-1:NUIN
FTX+CCI+++11:00
MOA+38:236
MOA+40:288
CST+0003+:108:ZZZ
FTX+AAA+++ITEM C
FTX+ACB+++APNAPN-2-2:NUIN
FTX+CCI+++11:00
MOA+38:344
MOA+40:458
UNS+S
TAX+3+CIF:107:ZZZ
MOA+161:844
TAX+3+TRN:107:ZZZ
MOA+161:0
TAX+3+CUS:107:ZZZ
MOA+161:929
UNT+108+<<MSGNO PLACEHOLDER>>";
		const string PopulateMessagesInvoiceDetailsExbond = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01
BGM+929:::RCD+::00001+9
CST++:117:ZZZ
DTM+111:20240801:102
GIS+F:134:ZZZ
FTX+LIN+++3
RFF+ACD:<<MSGNO PLACEHOLDER>>
PAC+0
TDT+20
UNS+D
DMS+INV-1234-1+380
DTM+3:20240801:102
MOA+39:123.45:ZAR
CUX+++0.1234
DTM+134:20240821:102
MOA+259:61.58:ZAR
CUX++5+1.35312474
TOD+6++FOB
NAD+II++SUPPLIER FULL NAME:SUP ADDRESS 1:SUP ADDRESS 2:SUP CITY:NEW SOUTH WALES++++++AU
PAT+1++9::D:23
MOA+74:12.34:ZAR
ALC+C
MOA+160:1.23:ZAR
CUX+1:ZAR++1.0000
ALC+C++++:::SPECIAL CHARGE FOR BRAND A
MOA+160:2.34:USD
CUX+1:USD++0.0560
ALC+C
MOA+64:17.35:ZAR
CUX+1:ZAR++1.0000
LIN+1+++:1
PIA+5+PART A
QTY+47:11.0000:KG
PRI+INV:11.1818
MOA+38:123.00
ALC+A
RTE+1:11
MOA+52:1.2300:ZAR
IMD+A++:::BRAND A
FTX+IND+++ITEM A
DMS+INV-1234-2+380
DTM+3:20240802:102
MOA+39:234.56:ZAR
CUX+++0.2345
DTM+134:20240821:102
MOA+259:92.99:ZAR
CUX++5+1.0079212
TOD+6++EXW
NAD+II++SUPPLIER FULL NAME:SUP ADDRESS 1:SUP ADDRESS 2:SUP CITY:NEW SOUTH WALES++++++AU
PAT+1++9::D
MOA+74:23.45:ZAR
FTX+PMT+++APN-2-1:APN-2-2
ALC+C
MOA+78:3.45:ZAR
CUX+1:ZAR++1.0000
ALC+C++++:::SPECIAL CHARGE FOR INVOICE 2
MOA+160:4.56:ZAR
CUX+1:ZAR++1.0000
ALC+C
MOA+64:81.65:ZAR
CUX+1:ZAR++1.0000
LIN+1+++:2
PIA+5+N/A
QTY+47:22.0000:T
PRI+INV:10.6364
MOA+38:234.00
ALC+A
RTE+1:22
IMD+A++:::BRAND B
FTX+IND+++VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :ITEM B
LIN+2+++:3
PIA+5+N/A
QTY+47:33.0000:G
PRI+INV:10.4545
MOA+38:345.00
ALC+A
RTE+1:32
MOA+52:3.3300:ZAR
IMD+A++:::BRAND C
FTX+IND+++ITEM C
CST+0001+:108:ZZZ
FTX+AAA+++ITEM A
FTX+ACB+++NUIN
FTX+CCI+++46:00
MOA+38:165
MOA+40:183
CST+0002+:108:ZZZ
FTX+AAA+++VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :ITEM B
FTX+ACB+++APNAPN-2-1:NUIN
FTX+CCI+++46:00
MOA+38:236
MOA+40:288
CST+0003+:108:ZZZ
FTX+AAA+++ITEM C
FTX+ACB+++APNAPN-2-2:NUIN
FTX+CCI+++46:00
MOA+38:344
MOA+40:458
UNS+S
TAX+3+CUS:107:ZZZ
MOA+161:929
UNT+102+<<MSGNO PLACEHOLDER>>";

		void SetupFinancialAccountNumberPortMaps(ZGuid organizationPK, string officeCode = null, string accountNumber = null)
		{
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping = collection.AddNew();
			mapping.OrganizationPK = organizationPK;
			mapping.CustomsOfficeCode = officeCode ?? "BFN";
			mapping.FinancialAccountNumber = accountNumber ?? "3234002346";
			mapping.ImporterPays = true;
			mapping.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
		}

		OrgHeader CreateTestAgent()
		{
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "CDP", Core.Constants.CountryCodes.SouthAfrica);
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "00626126", Core.Constants.CountryCodes.SouthAfrica);
			return testAgent;
		}

		JobDeclaration CreateDeclaration(ZString messageType, ZString transportMode)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = messageType;
			declaration.JE_TransportMode = transportMode;
			return declaration;
		}

		JobDeclaration CreateDeclaration(ZString messageType, ZString transportMode, ZString nkFinalDestination, ZString houseBill)
		{
			var declaration = CreateDeclaration(messageType, transportMode);
			declaration.JE_ExportDate = new ZDateTime(2016, 01, 02);
			declaration.JE_LocationOfGoods = "TE";
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.China;
			declaration.JE_RL_NKFinalDestination = nkFinalDestination;
			declaration.JE_DateOfArrival = new ZDateTime(2016, 02, 01);
			declaration.JE_HouseBill = houseBill;
			declaration.JE_MasterBill = "MB001";
			declaration.JE_VesselName = "TESTVESSEL";
			declaration.HouseBillIssuedDate = new ZDateTime(2016, 03, 01);
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 02, 28);
			declaration.JE_TotalNoOfPacks = 15;
			declaration.JE_MarksAndNumbers = PopulateMessagesMarksAndNumbers;
			declaration.JE_VoyageFlightNo = "VOY";
			declaration.JE_CustomsOffice = "BFN";
			return declaration;
		}

		JobDeclaration CreateDeclarationWithMessageInitiator(string messageType = Customs.Business.JobMessageTypeList.Codes.Import)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			return declaration;
		}

		CusContainer CreateCusContainer(JobDeclaration declaration)
		{
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "CONTNUM";
			cusContainer.CO_Seal = "SEAL1";
			return cusContainer;
		}

		OrgHeader CreateOrganisation(ZString address)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.Address1 = address;
			return org;
		}

		JobComInvoiceHeader CreateInvoice(JobDeclaration declaration, ZDateTime dateTime, ZString invoiceNumber)
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceDate = dateTime;
			invoice.JZ_InvoiceNumber = invoiceNumber;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			return invoice;
		}

		JobComInvoiceHeader CreateInvoice(JobDeclaration declaration, ZDateTime dateTime, decimal amount)
		{
			var invoice = CreateInvoice(declaration, dateTime, "INV001", amount);
			invoice.JZ_ValuationCode = "1";
			invoice.JZ_RelatedIndicator = "Y";
			return invoice;
		}

		JobComInvoiceHeader CreateInvoice(JobDeclaration declaration, ZDateTime dateTime, ZString invoiceNumber, decimal amount)
		{
			var invoice = CreateInvoice(declaration, dateTime, invoiceNumber);
			invoice.JZ_InvoiceAmount = amount;
			return invoice;
		}

		JobComInvoiceHeader CreateInvoiceLocalCurrencyConstantCode(JobDeclaration declaration)
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_ValuationDateOverride = new ZDateTime(2016, 05, 01);
			return invoice;
		}

		JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader invoice, CusEntryInstruction instruction, string procedureCode = "00")
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_LinePrice = 50000;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}{procedureCode}";
			return invoiceLine;
		}

		JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader invoice, CusEntryInstruction instruction, ZString newUsed, ZString taxType, string procedureCode = "00")
		{
			var invoiceLine = CreateInvoiceLine(invoice, instruction, procedureCode);
			invoiceLine.JI_NewUsed = newUsed;
			invoiceLine.JI_ZZF_NKTaxType = taxType;
			return invoiceLine;
		}

		JobComInvoiceLine CreateInvoiceLineWithPrimaryReference(JobComInvoiceHeader invoice, CusEntryInstruction instruction, ZString primaryPreference)
		{
			var invoiceLine = CreateInvoiceLine(invoice, instruction, "U", "VAT");
			invoiceLine.JI_PrimaryPreference = primaryPreference;
			return invoiceLine;
		}

		JobComInvoiceLine CreateInvoiceLineWithPrimaryReference(JobComInvoiceHeader invoice, CusEntryInstruction instruction, ZString primaryPreference, string procedureCode)
		{
			var invoiceLine = CreateInvoiceLine(invoice, instruction, "U", "VAT", procedureCode);
			invoiceLine.JI_PrimaryPreference = primaryPreference;
			return invoiceLine;
		}

		JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader invoice, CusEntryInstruction instruction, ZString tariff, ZShort entryLineNum)
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.JI_TargetEntryLineNumber = entryLineNum;
			return invoiceLine;
		}

		AdditionalInformation CreateAdditionalInfo(CusEntryLine entryLine)
		{
			var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			additionalInfo.CY_Code = "VIN";
			additionalInfo.CY_Data = "VINData";
			additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			additionalInfo.CY_Code = "DOL";
			additionalInfo.CY_Data = "DOLData";
			additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			additionalInfo.CY_Code = "PPR";
			additionalInfo.CY_Data = "PPRData";
			additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			additionalInfo.CY_Code = "VON";
			additionalInfo.CY_Data = "VONData";
			return additionalInfo;
		}

		CusEntryLine CreateEnryLineWithFees(CusEntryHeader entryHeader, decimal customsValue = 50000)
		{
			var entryLine = entryHeader.MergedLines[0];
			entryLine.CL_CustomsValue = customsValue;
			entryLine.Fees.AddOrUpdate("1P1", 1000);
			entryLine.Fees.AddOrUpdate("12A", 1001);
			entryLine.Fees.AddOrUpdate("12B", 1002);
			entryLine.Fees.AddOrUpdate("13A", 1003);
			entryLine.Fees.AddOrUpdate("VAT", 2000);
			return entryLine;
		}

		CusEntryLine CreateEnryLineWithFeesWithLandedCostOnly(CusEntryHeader entryHeader, decimal customsValue = 50000)
		{
			var entryLine = entryHeader.MergedLines[0];
			entryLine.CL_CustomsValue = customsValue;
			entryLine.Fees.AddOrUpdate("1P1", 1000).CF_IsLandedCostOnly = true;
			entryLine.Fees.AddOrUpdate("12A", 1001).CF_IsLandedCostOnly = true;
			entryLine.Fees.AddOrUpdate("12B", 1002).CF_IsLandedCostOnly = true;
			entryLine.Fees.AddOrUpdate("13A", 1003).CF_IsLandedCostOnly = true;
			entryLine.Fees.AddOrUpdate("VAT", 2000).CF_IsLandedCostOnly = true;
			return entryLine;
		}

		EDIMessage[] PopulateOriginalMessagesExposed(CusEntryHeader entryHeader)
		{
			return PopulateMessagesExposed(entryHeader, Customs.Common.Shared.MessageSubTypeCodes.Codes.Original);
		}

		EDIMessage[] PopulateCancelationMessagesExposed(CusEntryHeader entryHeader)
		{
			return PopulateMessagesExposed(entryHeader, Customs.Common.Shared.MessageSubTypeCodes.Codes.Cancellation);
		}

		EDIMessage[] PopulateChangeMessagesExposed(CusEntryHeader entryHeader)
		{
			return PopulateMessagesExposed(entryHeader, Customs.Common.Shared.MessageSubTypeCodes.Codes.Change);
		}

		EDIMessage[] PopulateMessagesExposed(CusEntryHeader entryHeader, string messageType = "")
		{
			var testWrapper = new MessageSendingObject(entryHeader);
			if (!string.IsNullOrEmpty(messageType))
			{
				testWrapper.MessageType = messageType;
			}
			var manager = new CUSDECMessageManagerForTest(testWrapper, notification);
			return manager.PopulateMessages_Exposed();
		}
	}

	public class MessageNotificationCollector_ForTest : TestUserNotification, IMessageNotificationCollector
	{
		MessageSendingNotificationCollection IMessageNotificationCollector.Notifications => notificationCollection ?? (notificationCollection = new MessageSendingNotificationCollection());
		MessageSendingNotificationCollection notificationCollection;

		public void Add(INotification notification)
		{
			var notifications = (this as IMessageNotificationCollector).Notifications;
			if (notification.Type == NotificationType.Error)
			{
				notifications.AddError(notification.Message);
			}
			else if (notification.Type == NotificationType.Warning)
			{
				notifications.AddWarning(notification.Message);
			}
			else
			{
				notifications.AddInformation(notification.Message);
			}
		}

		public void Clear()
		{
			(this as IMessageNotificationCollector).Notifications.Clear();
		}

		public ZString ErrorNotificationsAsString => (this as IMessageNotificationCollector).Notifications.ErrorNotificationsAsString();
	}

	sealed class CUSDECMessageManagerForTest : CUSDECMessageManager
	{
		public CUSDECMessageManagerForTest(MessageSendingObject sendingObject, IMessageNotificationCollector notification) : base(sendingObject, notification) { }

		public IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode) => GetMessageBuilder(actionCode);

		public EDIMessage[] PopulateMessages_Exposed()
		{
			return PopulateMessage(MessageSubTypes.Create);
		}

		public bool CanSendThisMessage_Exposed(out ZString messageText)
		{
			return CanSendThisMessage(MessageSubTypes.Undefined, out messageText);
		}
	}
}
