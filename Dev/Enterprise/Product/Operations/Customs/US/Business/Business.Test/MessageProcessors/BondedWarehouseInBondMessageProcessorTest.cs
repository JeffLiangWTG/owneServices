using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class BondedWarehouseInBondMessageProcessorTest : TestCaseWithFactory
	{
		#region Implementation

		protected override void SetUp()
		{
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			base.SetUp();
			DeclarationTestHelper.SetupForSendMessage();
		}

		Integration.Customs.US.InBond.ICusInBondMoveHeader GetNewCusInBondMoveHeader(ZGuid headerPK, ZString inBondNumber, ZString warehouseTransactionStatus)
		{
			var result = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			result.BM_BH = headerPK;
			result.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			result.InBondNumber = inBondNumber;
			result.BM_WarehouseTransactionStatus = warehouseTransactionStatus;
			return result;
		}

		Integration.Customs.US.InBond.ICusInBondBill GetNewCusInBondBill(ZGuid headerPK, ZString billNumber)
		{
			var result = Factory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			result.B0_BH = headerPK;
			result.B0_MasterBillNumber = billNumber;
			return result;
		}

		Integration.Customs.US.InBond.ICusInBondMoveDetail GetNewCusInBondMoveDetail(ZGuid headerMovePK, ZGuid billPK, ZString sequenceNumber)
		{
			var result = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			result.B9_BM = headerMovePK;
			result.B9_B0 = billPK;
			result.B9_SeqNo = sequenceNumber;
			return result;
		}

		Integration.Customs.US.InBond.ICusInBondHeader GetNewCusInBondHeader(ZString jobReference)
		{
			var result = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			result.BH_JobReference = jobReference;
			result.BH_OA_Importer = Importer.MainAddress.PK;
			result.BH_FTZMove = ZBool.True;
			return result;
		}

		Integration.Customs.US.InBond.ICusInBondContainer GetNewCusInBondContainer(ZGuid containerPK, ZString containerNumber)
		{
			var result = Factory.New<Integration.Customs.US.InBond.ICusInBondContainer>();
			result.BC_ParentID = containerPK;
			result.BC_ParentTableCode = CusInBondMoveDetailSchema.Constants.Prefix;
			result.BC_ContainerNum = containerNumber;
			result.BC_RC = ContainerType.PK;
			return result;
		}

		Integration.Customs.US.InBond.ICusInBondCargoDesc GetNewCusInBondCargoDesc(ZGuid parentPK, ZString parentTableCode, ZGuid supplierPK, ZString partNumber, ZDecimal invoiceQuantity, ZString warehouseEntryNumber, ZShort warehouseEntryLineNo)
		{
			var result = Factory.New<Integration.Customs.US.InBond.ICusInBondCargoDesc>();
			result.BY_ParentID = parentPK;
			result.BY_ParentTableCode = parentTableCode;
			result.BY_OH_Supplier = supplierPK;
			result.BY_PartNumber = partNumber;
			result.BY_InvoiceQuantity = invoiceQuantity;
			result.BY_WarehouseEntryNumber = warehouseEntryNumber;
			result.BY_WarehouseEntryLineNo = warehouseEntryLineNo;
			return result;
		}

		IInBondWarehouseIntegrationSupporter SetupInBondMessages(Integration.Customs.US.InBond.ICusInBondMoveHeader moveHeader, out MQEDIMessage incomingMessage, ZString messageNumber, ZString messageText, ZString messageSubType)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoingMessage = mock.Object;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;
			outgoingMessage.EM_MessageSubType = messageSubType;
			outgoingMessage.EM_MessageNum = messageNumber;
			outgoingMessage.EM_LinkUniqueID = moveHeader.PK;
			outgoingMessage.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
			incomingMessage.EM_Status = MQEDIMessage.Status.Queued;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = messageNumber;
			incomingMessage.EM_MessageText = messageText;
			incomingMessage.EM_MessageSubType = messageSubType;
			return (IInBondWarehouseIntegrationSupporter)moveHeader;
		}

		JobDeclaration GetNewDeclaration(ZString declarationReference, ZString entryFilerCode, string whsCode, ZString entryNumber, ZDecimal quantity)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = declarationReference;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
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

			declaration.US_EntryFilerCode = entryFilerCode;
			declaration.ImportEntryNumber = entryNumber;
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
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
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
					pivot2.CD_LicenceNo = "13";
				}
				return pivot2;
			}
		}
		CusClassPartPivot pivot2;

		RefContainer ContainerType
		{
			get
			{
				if (containerType == null)
				{
					containerType = Factory.New<RefContainer>();
					containerType.RC_Code = "40#@";
					containerType.SetCountrySpecificContainerCode("40", Enterprise.Core.Constants.CountryCodes.UnitedStates);
				}
				return containerType;
			}
		}
		RefContainer containerType;

		public void TestProcessOutwardDepartureAddClear()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration("BINW000001", "XJ5", "WH1", "40000007", 100m);
				var header = GetNewCusInBondHeader("INB32423");
				var moveHeader = GetNewCusInBondMoveHeader(header.PK, "IN2342", "");
				var bill = GetNewCusInBondBill(header.PK, "XJ540000007");
				var moveDetail = GetNewCusInBondMoveDetail(moveHeader.PK, bill.PK, "0001");
				var container = GetNewCusInBondContainer(moveDetail.PK, "JMON1407071");
				var commodity = GetNewCusInBondCargoDesc(container.PK, CusInBondContainerSchema.Constants.Prefix, ZGuid.Empty, Part.OP_PartNum, 60m, "XJ5-40000007", 1);
				MQEDIMessage incomingMessage;
				var supporter = SetupInBondMessages(moveHeader, out incomingMessage, "402232",
"B018888XJ5QT                                               402232               " +
"10A63001001151   MDRL1101413800001000020-064032900YN                            " +
"20W23511  QP FTZ WITHDRAWAL      FDSF        1101      W235                     " +
"30A 0001XXXWORGIN1                                                  0000000100  " +
"9502220 BILL ACCEPTED FOR INBOND                                                " +
"Y  8888XJ5QT00004", EM_MessageSubTypeList.Codes.InBondDepartureOriginal);
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);

				result = supporter.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				AssertEquals("moveHeader.BM_CustomsStatus", ZString.Empty, moveHeader.BM_CustomsStatus);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, moveHeader.BM_WarehouseTransactionStatus);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("moveHeader.BM_CustomsStatus", ImportMessageStatusList.Codes.ClearDepartureOriginal, moveHeader.BM_CustomsStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, moveHeader.BM_WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "InBond Departure Response for INB32423 / IN2342"; }));
				AssertContains("Stock Release can be finalized. (WHS Order: ", email.Body);
			}
		}

		public void TestProcessOutwardDepartureAmendmentClear()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration("BINW000001", "XJ5", "WH1", "40000007", 100m);
				var inwardDeclarationInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardDeclarationInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				inwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardDeclarationInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardDeclarationInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardDeclarationInvoiceLine2.JI_CustomsQuantity = 2000m;
				inwardDeclarationInvoiceLine2.JI_LinePrice = 20000m;
				var whsPack = inwardDeclaration.WHSPacks.AddNew();
				whsPack.US_PackageQty = 20;
				var whsPackLine = inwardDeclaration.WHSPackLines.AddNew(whsPack);
				whsPackLine.US_JI_InvoiceLine = inwardDeclarationInvoiceLine2.PK;
				inwardDeclaration.DoMerge();
				var header = GetNewCusInBondHeader("INB32423");
				var moveHeader = GetNewCusInBondMoveHeader(header.PK, "IN2342", "");
				var bill = GetNewCusInBondBill(header.PK, "XJ540000007");
				var moveDetail = GetNewCusInBondMoveDetail(moveHeader.PK, bill.PK, "0001");
				var container = GetNewCusInBondContainer(moveDetail.PK, "JMON1407071");
				var commodity1 = GetNewCusInBondCargoDesc(container.PK, CusInBondContainerSchema.Constants.Prefix, ZGuid.Empty, Part.OP_PartNum, 60m, "XJ5-40000007", 1);
				var commodity2 = GetNewCusInBondCargoDesc(container.PK, CusInBondContainerSchema.Constants.Prefix, ZGuid.Empty, Part2.OP_PartNum, 150m, "XJ5-40000007", 2);

				MQEDIMessage incomingMessage;
				var supporter = SetupInBondMessages(moveHeader, out incomingMessage, "402232",
"B018888XJ5QT                                               402232               " +
"10A63001001151   MDRL1101413800001000020-064032900YN                            " +
"20W23511  QP FTZ WITHDRAWAL      FDSF        1101      W235                     " +
"30A 0001XXXWORGIN1                                                  0000000100  " +
"9502220 BILL ACCEPTED FOR INBOND                                                " +
"Y  8888XJ5QT00004", EM_MessageSubTypeList.Codes.InBondDepartureReplacement);
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 200m);

				result = supporter.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("moveHeader.BM_CustomsStatus", ZString.Empty, moveHeader.BM_CustomsStatus);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, moveHeader.BM_WarehouseTransactionStatus);
				supporter.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				commodity1.BY_InvoiceQuantity = 70m;
				commodity2.BY_InvoiceQuantity = 140m;
				Factory.Save();
				supporter.PublishShipmentForWHSOutwardWithPreAmendmentData();
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("moveHeader.BM_CustomsStatus", ImportMessageStatusList.Codes.ClearDepartureAmendment, moveHeader.BM_CustomsStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 60m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "InBond Departure Response for INB32423 / IN2342"; }));
				AssertContains("Stock Release has been updated. (WHS Order: ", email.Body);
			}
		}

		public void TestProcessOutwardDepartureWithdrawalClear()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration("BINW000001", "XJ5", "WH1", "40000007", 100m);
				var inwardDeclarationInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardDeclarationInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				inwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardDeclarationInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardDeclarationInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardDeclarationInvoiceLine2.JI_CustomsQuantity = 2000m;
				inwardDeclarationInvoiceLine2.JI_LinePrice = 20000m;
				var whsPack = inwardDeclaration.WHSPacks.AddNew();
				whsPack.US_PackageQty = 20;
				var whsPackLine = inwardDeclaration.WHSPackLines.AddNew(whsPack);
				whsPackLine.US_JI_InvoiceLine = inwardDeclarationInvoiceLine2.PK;
				inwardDeclaration.DoMerge();
				var header = GetNewCusInBondHeader("INB32423");
				var moveHeader = GetNewCusInBondMoveHeader(header.PK, "IN2342", "");
				var bill = GetNewCusInBondBill(header.PK, "XJ540000007");
				var moveDetail = GetNewCusInBondMoveDetail(moveHeader.PK, bill.PK, "0001");
				var container = GetNewCusInBondContainer(moveDetail.PK, "JMON1407071");
				var commodity1 = GetNewCusInBondCargoDesc(container.PK, CusInBondContainerSchema.Constants.Prefix, ZGuid.Empty, Part.OP_PartNum, 60m, "XJ5-40000007", 1);
				var commodity2 = GetNewCusInBondCargoDesc(container.PK, CusInBondContainerSchema.Constants.Prefix, ZGuid.Empty, Part2.OP_PartNum, 150m, "XJ5-40000007", 2);

				MQEDIMessage incomingMessage;
				var supporter = SetupInBondMessages(moveHeader, out incomingMessage, "402232",
"B018888XJ5QT                                               402232               " +
"10A63001001151   MDRL1101413800001000020-064032900YN                            " +
"20W23511  QP FTZ WITHDRAWAL      FDSF        1101      W235                     " +
"30A 0001XXXWORGIN1                                                  0000000100  " +
"9502220 BILL ACCEPTED FOR INBOND                                                " +
"Y  8888XJ5QT00004", EM_MessageSubTypeList.Codes.InBondDepartureDelete);
				Factory.Save();

				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 200m);

				result = supporter.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("moveHeader.BM_CustomsStatus", ZString.Empty, moveHeader.BM_CustomsStatus);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, moveHeader.BM_WarehouseTransactionStatus);
				supporter.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				commodity1.BY_InvoiceQuantity = 70m;
				commodity2.BY_InvoiceQuantity = 140m;
				Factory.Save();
				supporter.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("moveHeader.BM_CustomsStatus", ImportMessageStatusList.Codes.ClearDepartureWithdraw, moveHeader.BM_CustomsStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, moveHeader.BM_WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 200m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "InBond Departure Response for INB32423 / IN2342"; }));
				AssertContains("Stock Release has been canceled. (WHS Order: ", email.Body);
			}
		}
		#endregion
	}
}
