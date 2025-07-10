using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobDeclarationSetupPreMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestActionForAutomationDisabled()
		{
			var inwardDeclaration = CreateTestDeclaration(Factory, "B00000001", "WH1", EntryTypeList.Codes.Warehouse, "XJ5", "ENT323", 10m);
			inwardDeclaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
			Factory.Save();

			Func<PublishToUniversalResult> preMessagingAction = null;
			Action restoreToPreMessagingState = null;
			var preMessagingActionResult = inwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Original);
			AssertEquals("isBondedWarehouse", false, preMessagingActionResult.IsBondedWarehouse);
			AssertNull(preMessagingAction);
			AssertNull(restoreToPreMessagingState);

			preMessagingAction = null;
			restoreToPreMessagingState = null;
			preMessagingActionResult = inwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Replacement);
			AssertEquals("isBondedWarehouse", false, preMessagingActionResult.IsBondedWarehouse);
			AssertNull(preMessagingAction);
			AssertNull(restoreToPreMessagingState);

			preMessagingAction = null;
			restoreToPreMessagingState = null;
			preMessagingActionResult = inwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Deletion);
			AssertEquals("isBondedWarehouse", false, preMessagingActionResult.IsBondedWarehouse);
			AssertNull(preMessagingAction);
			AssertNull(restoreToPreMessagingState);

			inwardDeclaration.WarehouseTransactionStatus = ZString.Empty;
			Factory.Save();
			preMessagingAction = null;
			restoreToPreMessagingState = null;
			preMessagingActionResult = inwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Original);
			AssertEquals("isBondedWarehouse", true, preMessagingActionResult.IsBondedWarehouse);
			AssertNotNull(preMessagingAction);
			AssertNotNull(restoreToPreMessagingState);

			var outwardDeclaration = CreateTestDeclaration(Factory, "B000000002", "WH2", EntryTypeList.Codes.WarehouseWithdrawalConsumption, "XJ5", "ENT326", 60m);
			outwardDeclaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
			Factory.Save();
			preMessagingAction = null;
			restoreToPreMessagingState = null;
			preMessagingActionResult = outwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Original);
			AssertEquals("isBondedWarehouse", false, preMessagingActionResult.IsBondedWarehouse);
			AssertNull(preMessagingAction);
			AssertNull(restoreToPreMessagingState);

			preMessagingAction = null;
			restoreToPreMessagingState = null;
			preMessagingActionResult = outwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Replacement);
			AssertEquals("isBondedWarehouse", false, preMessagingActionResult.IsBondedWarehouse);
			AssertNull(preMessagingAction);
			AssertNull(restoreToPreMessagingState);

			preMessagingAction = null;
			restoreToPreMessagingState = null;
			preMessagingActionResult = outwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Deletion);
			AssertEquals("isBondedWarehouse", false, preMessagingActionResult.IsBondedWarehouse);
			AssertNull(preMessagingAction);
			AssertNull(restoreToPreMessagingState);

			outwardDeclaration.WarehouseTransactionStatus = ZString.Empty;
			Factory.Save();
			preMessagingAction = null;
			restoreToPreMessagingState = null;
			preMessagingActionResult = outwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Original);
			AssertEquals("isBondedWarehouse", true, preMessagingActionResult.IsBondedWarehouse);
			AssertNotNull(preMessagingAction);
			AssertNotNull(restoreToPreMessagingState);
		}

		public void TestActionForInward()
		{
			var inwardDeclaration = CreateTestDeclaration(Factory, "B00000001", "WH1", EntryTypeList.Codes.Warehouse, "XJ5", "ENT323", 10m);
			Factory.Save();

			((IWarehouseIntegrationSupporter)inwardDeclaration).WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			Assert("Precondition: IsOutwardBondedWarehousingEnabled = false", !inwardDeclaration.IsOutwardBondedWarehousingEnabled);
			Func<PublishToUniversalResult> preMessagingAction = null;
			Action restoreToPreMessagingState = null;
			var preMessagingActionResult = inwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Original);
			Assert("isBondedWarehouse", preMessagingActionResult.IsBondedWarehouse);
			AssertNotNull(preMessagingAction);
			AssertNotNull(restoreToPreMessagingState);
		}

		public void TestNoExceptionThrownWhenRestoreToPreMessageState()
		{
			var inwardDeclaration = CreateTestDeclaration(Factory, "B00000003", "WH1", EntryTypeList.Codes.Warehouse, "XJ5", "ENT456", 10m);
			Factory.Save();

			Func<PublishToUniversalResult> preMessagingAction = null;
			Action restoreToPreMessagingState = null;
			var preMessagingActionResult = inwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Original);
			Assert("isBondedWarehouse", preMessagingActionResult.IsBondedWarehouse);
			AssertNotNull(preMessagingAction);
			AssertNotNull(restoreToPreMessagingState);
			((IWarehouseIntegrationSupporter)inwardDeclaration).WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;

			var newBusinessObjectFactory = new BusinessObjectFactory();
			newBusinessObjectFactory.RefreshEnabled = false;
			var inwardDeclarationLoaded = (IWarehouseIntegrationSupporter)newBusinessObjectFactory.Load<JobDeclaration>(inwardDeclaration.PK);
			inwardDeclarationLoaded.PublishCancelEventForWHSInwardAndSaveIfNeeded(false, ZString.Empty, isManualWhsUpdate: false);
			newBusinessObjectFactory.Save();
			AssertEquals("WarehouseTransactionStatus is empty", ZString.Empty, inwardDeclarationLoaded.WarehouseTransactionStatus);

			AssertNoExceptionThrown(() => restoreToPreMessagingState());
		}

		public void TestActionForOutward()
		{
			var outwardDeclaration = CreateTestDeclaration(Factory, "B000000002", "WH1", EntryTypeList.Codes.WarehouseWithdrawalConsumption, "", "", 60m);
			outwardDeclaration.US_WHSEntryFilerCode = "XJ5";
			outwardDeclaration.US_WHSEntryNumber = "ENT326";
			Factory.Save();

			Assert("Precondition: IsOutwardBondedWarehousingEnabled = true", outwardDeclaration.IsOutwardBondedWarehousingEnabled);

			Func<PublishToUniversalResult> preMessagingAction = null;
			Action restoreToPreMessagingState = null;
			var preMessagingActionResult = outwardDeclaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Original);
			Assert("isBondedWarehouse", preMessagingActionResult.IsBondedWarehouse);
			AssertNotNull(preMessagingAction);
			AssertNotNull(restoreToPreMessagingState);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@test.com.au";
		}

		JobDeclaration CreateTestDeclaration(BusinessObjectFactory factory, ZString declarationReference, string whsCode, ZString entryType, ZString entryFilerCode, ZString entryNumber, ZDecimal quantity)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			declaration.US_EntryFilerCode = entryFilerCode;

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

			var isFTZAdmission = declaration.IsFTZAdmission;
			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			SetupQuantity(quantity, invoice, invoiceLine);
			if (isFTZAdmission)
			{
				declaration.FTZZoneID = "1530100";
				declaration.FTZYear = "15";
				declaration.FTZControlNumber = entryNumber;
			}
			else
			{
				declaration.US_EnableENS = true;
				declaration.US_EntryType = entryType;
				if (declaration.IsExWarehouseEntryType)
				{
					declaration.US_WHSEntryFilerCode = entryFilerCode;
					declaration.US_WHSEntryNumber = entryNumber;
					invoiceLine.US_WHSEntryLineNo = 1;
				}
				else if (declaration.IsENSFormalImportAndConsumptionFTZ)
				{
					invoiceLine.US_WHSEntryNumber = entryFilerCode + "-" + entryNumber;
					invoiceLine.US_WHSEntryLineNo = 1;
				}
				else
				{
					declaration.ImportEntryNumber = entryNumber;
				}
			}
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			return declaration;
		}

		void SetupQuantity(ZDecimal quantity, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine)
		{
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			invoiceLine.JI_BondedWhsQuantity = quantity;
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
					warehouse.OH_RL_NKClosestPort = "USCHI";
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
					CusClassPartPivot importPivot = part.PivotsForBinding.AddNew();
					importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
					importPivot.CI_CC = Classification.PK;
				}
				return part;
			}
		}
		OrgSupplierPart part;

		CusClassification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.New<CusClassification>();
					classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
					classification.CC_LookupCode = "~~1L";
					classification.CC_TariffNum = "4901.10.00 01";
				}
				return classification;
			}
		}
		CusClassification classification;
	}
}
