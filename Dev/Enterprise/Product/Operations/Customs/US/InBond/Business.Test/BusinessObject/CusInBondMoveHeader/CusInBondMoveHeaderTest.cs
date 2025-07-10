using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.InBond.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Workflow.ProcessTasks.Milestones;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using IInBondWarehouseIntegrationSupporter = Enterprise.Customs.US.InBond.Business.WarehouseExtensions.IInBondWarehouseIntegrationSupporter;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeader))]
	sealed class CusInBondMoveHeaderTest : CusInBondMoveHeaderTest<CusInBondMoveHeader>
	{
		public void TestMilestoneFromHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(moveHeader.BM_DestinationPortCode, ZString.Empty);
			AssertEquals(moveHeader.BM_InBondCarrierID, ZString.Empty);
			AssertEquals(moveHeader.BM_InBondCarrierSCAC, ZString.Empty);

			var milestone = header.WorkflowItems.Milestones.AddNew();
			var condition = milestone as ITriggerConditions;
			condition.TriggerEventCode = AutoEvents.MessageStatusChange.Code;
			condition.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			condition.TriggerConditionValue = ImportMessageStatusList.Codes.ClearArrival;
			milestone.P9_ActualDateUpdateType = ActualDateUpdateTypeCodeList.Codes.AD1;

			var outgoingMessage = Factory.NewWithValidTestData<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondArrival;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_119086";
			outgoingMessage.EM_MessageText = @"B011101SV9WP                                               HYEDUSCMT_193717     10Z001003015                                                                    201902182012131401AQOE20-064032900                                              Y  1101SV9WP00002";
			var receiveMessage = Factory.NewWithValidTestData<MQEDIMessage>();
			receiveMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receiveMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			receiveMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse;
			receiveMessage.EM_Status = EDIMessage.Status.Queued;
			receiveMessage.EM_MessageNum = "HYEDUSCMT_119086";
			receiveMessage.EM_MessageText = @"B013910SV9WT                                               HYEDUSCMT_118892     101333210054                                                                    9502271 DATA ADDED AS REQUESTED                                                 201206141642003901XXXW11-765432100LONDON             CT                         Y  3910SV9WT00003";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_119086";
			Factory.Save();

			AssertEquals(ZDateTime.Empty, milestone.P9_ActualDate);
			var processor = new InBondArrivalOrFDAPriorNoticeProcessor();
			processor.Message = receiveMessage;
			var block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, new[]
			{
				"B013910SV9WT                                               HYEDUSCMT_118892     ",
				"101333210054                                                                    ",
				"9502271 DATA ADDED AS REQUESTED                                                 ",
				"201206141642003901XXXW11-765432100LONDON             CT                         ",
				"Y  3910SV9WT00003"
			});
			block.MessageBlocks.ForEach(x => processor.AddMessageBlock(x));
			processor.Process();

			AssertEquals(true, moveHeader.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageStatusChange.Code && x.SL_Table == CusInBondMoveHeaderSchema.Constants.TableName));
			AssertEquals(false, milestone.P9_ActualDate.IsEmpty);
		}

		public void TestIWorkflowTriggerEventSource()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var source = moveHeader as IWorkflowTriggerEventSource;
			AssertEquals(1, source.ParentWorkflowProviders.Count);
			AssertEquals(header.PK, source.ParentWorkflowProviders[0].PK);
			AssertEquals(header.Company.PK, source.JobHeaderCompany.PK);
		}

		public void TestUniversalCopy()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])typeof(CusInBondMoveHeader).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false);
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertEquals(7, ignoreElementAttributes[0].ElementNames.Count);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusInBondEDIMessages, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.ReferenceNumbers, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusInBondMoveDetails, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondMoveHeader.Schema.BM_CustomsStatus, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondMoveHeader.Schema.BM_WarehouseTransactionStatus, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondMoveHeader.Schema.BM_InBondClosedDate, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondMoveHeader.Schema.BM_MessageStatus, ignoreElementAttributes[0].ElementNames);

			var movementDetails = typeof(CusInBondMoveHeader).GetProperty(nameof(CusInBondMoveHeader.MovementDetails), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(movementDetails, typeof(UniversalCopyCollectionEntityAttribute)));
		}

		public void TestIResetToOriginal()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_CustomsStatus = "ADA";
			var iResetToOriginal = moveHeader as IResetToOriginal;
			AssertEquals("QP:ADA", iResetToOriginal.CustomsStatus);
			AssertEquals("", iResetToOriginal.BillNumber);
			AssertEquals("", iResetToOriginal.ContainerNumber);
			AssertEquals("In-Bond", iResetToOriginal.Level);

			moveHeader.BM_CustomsStatus = "";
			moveHeader.BM_MessageStatus = "AAV";
			AssertEquals("WP:AAV", iResetToOriginal.CustomsStatus);

			moveHeader.BM_CustomsStatus = "ADA";
			AssertEquals("QP:ADA WP:AAV", iResetToOriginal.CustomsStatus);
		}

		public void TestWarehouseBondedQuantityIsCorrectlyCalculated()
		{
			var helper = new WhsDataTestHelper(Factory);
			helper.WhsWarehouse.WW_IsVirtualWarehouse = false;
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B00002132", "XJ5", "ENT324", 110m);
				Factory.Save();
				var result1 = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result1.ResultType);
				var whsReceive = (IWhsReceive)result1.FindJobIfExists();
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				whsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
				whsReceive = Factory.Load<IWhsReceive>(whsReceive.PK);
				whsReceive.WD_ArrivalDate = ZDateTimeOffset.Now;
				whsHelper.FinaliseDocketWithoutUserConfirmation(whsReceive.PK);
				Factory.Save();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 110m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardDeclaration.WarehouseTransactionStatus);
				var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = helper.Importer.MainAddress.PK;
				header.BH_FTZMove = true;
				header.BH_JobReference = "B00002133";
				header.MessageInitiator = messageInitiator;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENT324";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var productLine = container.Commodities.AddNew();
				productLine.BY_PartNumber = helper.Part.OP_PartNum;
				productLine.BY_WarehouseEntryNumber = "XJ5-ENT324";
				productLine.BY_WarehouseEntryLineNo = 1;
				productLine.BY_InvoiceQuantity = 66m;
				Factory.Save();
				var result = moveHeader.PublishShipmentForWHSOutward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENT324-1", 44m);
				moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				var shipment = moveHeader.GetLastClearedUniversalShipment(RecipientRoleType.BWR);
				AssertNotNull(shipment);
				var commercialInvoiceLine = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				AssertEquals(66m, commercialInvoiceLine.BondedWarehouseQuantity.GetValueOrDefault());
				AssertEquals("moveDetail.WarehouseDetails.Count", 1, moveDetail.WarehouseDetails.Count);
				var warehouseDetail = moveDetail.WarehouseDetails[0];
				AssertEquals("warehouseDetail.US_WarehouseBondedQuantity", 10m, warehouseDetail.US_WarehouseBondedQuantity);
				AssertEquals("warehouseDetail.US_WarehouseWithdrawQuantity", 6m, warehouseDetail.US_WarehouseWithdrawQuantity);
				AssertEquals("warehouseDetail.WarehouseBondedQuantityBalance", 4m, warehouseDetail.WarehouseBondedQuantityBalance);
			}
		}

		public void TestUpdateLinkBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(moveHeader.BM_DestinationPortCode, ZString.Empty);
			AssertEquals(moveHeader.BM_InBondCarrierID, ZString.Empty);
			AssertEquals(moveHeader.BM_InBondCarrierSCAC, ZString.Empty);
			var outgoingMessage = Factory.NewWithValidTestData<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDiversionRequest;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_119086";
			outgoingMessage.EM_MessageText = @"B011101SV9WP                                               HYEDUSCMT_193717     10Z001003015                                                                    201902182012131401AQOE20-064032900                                              Y  1101SV9WP00002";
			var receiveMessage = Factory.NewWithValidTestData<MQEDIMessage>();
			receiveMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receiveMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			receiveMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse;
			receiveMessage.EM_Status = EDIMessage.Status.Queued;
			receiveMessage.EM_MessageNum = "HYEDUSCMT_119086";
			receiveMessage.EM_MessageText = @"B013910SV9WT                                               HYEDUSCMT_118892     101333210054                                                                    9502271 DATA ADDED AS REQUESTED                                                 201206141642003901XXXW11-765432100LONDON             CT                         Y  3910SV9WT00003";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_119086";
			Factory.Save();
			var iheader = moveHeader as IInBondArriveExportTOLHeader;
			iheader.UpdateLinkBusinessObject(receiveMessage);
			AssertEquals(moveHeader.BM_DestinationPortCode, "1401");
			AssertEquals(moveHeader.BM_InBondCarrierID, "20-064032900");
			AssertEquals(moveHeader.BM_InBondCarrierSCAC, "AQOE");
		}

		public void TestWPStatus()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(ZString.Empty, moveHeader.BM_CustomsStatus);
			AssertEquals(ZString.Empty, moveHeader.BM_MessageStatus);

			var outgoingMessage = Factory.NewWithValidTestData<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondArrival;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_119086";
			outgoingMessage.EM_MessageText = @"B011101SV9WP                                               HYEDUSCMT_193717     10Z001003015                                                                    201902182012131401AQOE20-064032900                                              Y  1101SV9WP00002";
			var receiveMessage = Factory.NewWithValidTestData<MQEDIMessage>();
			receiveMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receiveMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			receiveMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse;
			receiveMessage.EM_Status = EDIMessage.Status.Queued;
			receiveMessage.EM_MessageNum = "HYEDUSCMT_119086";
			receiveMessage.EM_MessageText = @"B013910SV9WT                                               HYEDUSCMT_118892     101333210054                                                                    9502271 DATA ADDED AS REQUESTED                                                 201206141642003901XXXW11-765432100LONDON             CT                         Y  3910SV9WT00003";
			Factory.Save();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_119086";
			Factory.Save();

			var processor = new InBondArrivalOrFDAPriorNoticeProcessor();
			processor.Message = receiveMessage;
			var block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, new[]
			{
				"B013910SV9WT                                               HYEDUSCMT_118892     ",
				"101333210054                                                                    ",
				"9502271 DATA ADDED AS REQUESTED                                                 ",
				"201206141642003901XXXW11-765432100LONDON             CT                         ",
				"Y  3910SV9WT00003"
			});
			block.MessageBlocks.ForEach(x => processor.AddMessageBlock(x));
			processor.Process();
			AssertEquals(ZString.Empty, moveHeader.BM_CustomsStatus);
			AssertEquals(ImportMessageStatusList.Codes.ClearArrival, moveHeader.BM_MessageStatus);
		}

		public void TestBM_InBondCarrierSCACChanged()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FIRMS = "W235";
			header.BH_FTZMove = true;
			var bill1 = header.Bills.AddNew();
			AssertEquals(bill1.B0_IssuerCode, "");
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondCarrierSCAC = "ABCD";
			AssertEquals(bill1.B0_IssuerCode, "ABCD");
			AssertEquals(bill1.B0_PortOfLadingKCode, "99999");
		}

		public void TestCanOpenInBond()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, "IMakeBondCloseDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, "IMakeBondCloseDisposition6263", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, "INeutralInBondDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, "IsExamDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName5 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, "IsHoldDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName6 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, "IsHoldExamRemovedDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName7 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, "HoldRemovedExamCompletedMapCode", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "11", "11 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1C", "1C DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "14", "14 DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "15", "15 DESC", startDate, endDate);
			var attribute11 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName1.ZXE_Name, "Y");
			var attribute21 = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName4.ZXE_Name, "Y");
			var attribute31 = helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, attributeName1.ZXE_Name, "Y");
			var attribute32 = helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, attributeName3.ZXE_Name, "Y");
			var attribute41 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName5.ZXE_Name, "Y");
			Factory.Save();
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FIRMS = "W235";
			header.BH_FTZMove = true;
			header.BH_ETA = new ZDateTime(2019, 2, 12);
			var bill1 = header.Bills.AddNew();
			AssertEquals(bill1.B0_IssuerCode, "");
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals(true, moveHeader1.CanCloseInBond(code1.ZZD_Code));
			AssertEquals(false, moveHeader1.CanCloseInBond(code2.ZZD_Code));
			AssertEquals(false, moveHeader1.CanOpenInBond(code3.ZZD_Code));
			AssertEquals(true, moveHeader1.CanOpenInBond(code4.ZZD_Code));
		}

		public override void TestHumanReadableName()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.AllocateInBondNumber("1234567");
			AssertEquals("In-Bond Movement Header 1234567", moveHeader.HumanReadableName);
		}

		public void TestEffectiveProperties()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = "QF";
			header.BH_VoyageNumber = "1234";
			header.BH_ETA = ZDateTime.BrettsBirthday;
			AssertEquals(header.BH_CarrierSCAC, moveHeader.EffectiveSplitCarrierSCAC);
			AssertEquals(header.BH_VoyageNumber, moveHeader.EffectiveSplitFlightNo);
			AssertEquals(header.BH_ETA, moveHeader.EffectiveArrivalDate);
			moveHeader.BM_SplitCarrierSCAC = "BF";
			moveHeader.BM_SplitFlightNo = "111";
			moveHeader.BM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(-5);
			AssertEquals("BF", moveHeader.EffectiveSplitCarrierSCAC);
			AssertEquals("111", moveHeader.EffectiveSplitFlightNo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-5), moveHeader.EffectiveArrivalDate);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.RailContainer;
			AssertEquals(header.BH_CarrierSCAC, moveHeader.EffectiveSplitCarrierSCAC);
			AssertEquals(header.BH_VoyageNumber, moveHeader.EffectiveSplitFlightNo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-5), moveHeader.EffectiveArrivalDate);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = "A2";
			header.ThreeLetterAirCarrierCode = "WDF";
			AssertEquals(header.ThreeLetterAirCarrierCode, moveHeader.EffectiveSplitCarrierSCAC);
			moveHeader.BM_SplitCarrierSCAC = "A3";
			moveHeader.ThreeLetterSplitAirCarrierCode = "YUN";
			AssertEquals(moveHeader.ThreeLetterSplitAirCarrierCode, moveHeader.EffectiveSplitCarrierSCAC);
		}

		public void TestSetDefaultValueToAirCarrierCode()
		{
			var refAirline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "A2"));
			if (refAirline == null)
			{
				refAirline = Factory.NewWithValidTestData<RefAirline>();
				refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "A8";
				refAirline.RM_TwoCharacterCode = "A2";
			}

			refAirline.RM_ThreeLetterCode = "A00";
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondCarrierSCAC = "A2";
			AssertEquals(moveHeader.ThreeLetterInBondAirCarrierCode, refAirline.RM_ThreeLetterCode);
			AssertEquals(false, moveHeader.ThreeLetterInBondAirCarrierCode_ReadOnly);
			moveHeader.BM_SplitCarrierSCAC = "A2";
			AssertEquals(moveHeader.ThreeLetterSplitAirCarrierCode, refAirline.RM_ThreeLetterCode);
			AssertEquals(false, moveHeader.ThreeLetterSplitAirCarrierCode_ReadOnly);
			Factory.Save();
			var addOnCarrierCodeQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, moveHeader.PK);
			addOnCarrierCodeQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, moveHeader.TablePrefix);
			addOnCarrierCodeQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, "ThreeLetterAirCarrierCode");
			var addOn = Factory.LoadTop1<GenAddOnColumn>(addOnCarrierCodeQuery);
			AssertNull("Do not create GenAddOnColumn", addOn);
			addOnCarrierCodeQuery.Clear();
			addOnCarrierCodeQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, moveHeader.PK);
			addOnCarrierCodeQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, moveHeader.TablePrefix);
			addOnCarrierCodeQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, "ThreeLetterSplitAirCarrierCode");
			addOn = Factory.LoadTop1<GenAddOnColumn>(addOnCarrierCodeQuery);
			AssertNull("Do not create GenAddOnColumn", addOn);
		}

		[TestDate(2015, 1, 21, 21, 52, 0)]
		public void TestConcurrency()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			Factory.RefreshEnabled = false;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var moveHeaderInDiffFactory = newFactory.Load<CusInBondMoveHeader>(moveHeader.PK);
			moveHeaderInDiffFactory.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			moveHeaderInDiffFactory.BM_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			moveHeaderInDiffFactory.BM_ExportDate = new ZDateTime(2015, 1, 24);
			newFactory.Save();
			AssertEquals(ZString.Empty, moveHeader.BM_CustomsStatus);
			AssertEquals(ZString.Empty, moveHeader.BM_MessageStatus);
			AssertEquals(ZDateTime.Empty, moveHeader.BM_ExportDate);
			moveHeader.BM_ExportLadenOn = "HELLO";
			var handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}

			AssertEquals(ZString.Empty, moveHeader.BM_CustomsStatus);
			AssertEquals(ZString.Empty, moveHeader.BM_MessageStatus);
			AssertEquals(ZDateTime.Empty, moveHeader.BM_ExportDate);
			AssertEquals("HELLO", moveHeader.BM_ExportLadenOn);
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			AssertMultilineASCIIEquals("ReportInformationMessage", @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
In-Bond Movement Header NOT YET SPECIFIED 1 (CargoWise Support @ 21 Jan 2015 21:52:00)
	Export Date
	QP Message Status (Critical change)
	WP Message Status (Critical change)", handler.ReportInformationMessage);
			moveHeader.Delete();
			handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}

			AssertEquals(true, moveHeader.IsDeleted);
			AssertEquals(true, moveHeader.HasChanges);
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			AssertMultilineASCIIEquals("ReportInformationMessage", @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
In-Bond Movement Header NOT YET SPECIFIED (CargoWise Support @ 21 Jan 2015 21:52:00) (pending delete)
	Export Date
	QP Message Status (Critical change)
	WP Message Status (Critical change)", handler.ReportInformationMessage);
		}

		public void TestResetInBondNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			Assert(moveHeader.IsInBondNumberResetable);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			Assert(!moveHeader.IsInBondNumberResetable);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			Assert(!moveHeader.IsInBondNumberResetable);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			Assert(moveHeader.IsInBondNumberResetable);
			moveHeader.AllocateInBondNumber("1234");
			Factory.Save();
			Assert(!moveHeader.InBondNumber.IsEmpty);
			moveHeader.ResetInBondNumber("TEST RESET");
			Factory.Save();
			Assert(moveHeader.InBondNumber.IsEmpty);
			AssertEquals(Events.ResetEntryMessageItemFunction.Code, moveHeader.Logs.MostRecentLog.Event.SE_Code);
			AssertContains("TEST RESET", moveHeader.Logs.MostRecentLog.SL_Reference);
		}

		public void TestIInBondMessagingHeaderMutexLock()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			IInBondMessagingHeader msgheader = moveHeader;
			AssertEquals(false, msgheader.InBondNumberAllocationMutexHasLock());
			msgheader.LockInBondNumberAllocationMutex();
			AssertEquals(true, msgheader.InBondNumberAllocationMutexHasLock());
			msgheader.UnLockInBondNumberAllocationMutex();
			AssertEquals(false, msgheader.InBondNumberAllocationMutexHasLock());
		}

		public void TestIInBondMessagingHeaderTransportMode()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			IInBondMessagingHeader msgheader = moveHeader;
			AssertEquals(TransportModeCodes.Codes.VesselContainer, msgheader.TransportMode);
		}

		public void TestIsBondedWarehousingDisabled()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals("IsBondedWarehousingDisabled", false, moveHeader.IsBondedWarehousingDisabled);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
			AssertEquals("IsBondedWarehousingDisabled", true, moveHeader.IsBondedWarehousingDisabled);
		}

		public void TestIInBondWarehouseIntegrationSupporterMembers()
		{
			var helper = new WhsDataTestHelper(Factory);
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "W#@33";
			var whsWarehouse = helper.GetNewWhsWarehouse(warehouse.MainAddress.PK, true, "W#@");
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IM#@#";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DS@";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "DK#";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = branch.PK;
			header.BH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-2);
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;
			var moveHeader = header.MovementHeaders.AddNew();
			IInBondWarehouseIntegrationSupporter supporter = moveHeader;
			AssertEquals(false, supporter.ShouldUpdate(false, false));
			AssertEquals(company, supporter.Company);
			AssertEquals(ZGuid.Empty, supporter.ClientPK);
			AssertEquals(true, supporter.IsActive);
			AssertEquals(false, supporter.IsInDatabase);
			AssertEquals(messageInitiator, supporter.MessageInitiator);
			AssertNull(supporter.WarehouseAddress);
			AssertEquals(ZString.Empty, supporter.WarehouseTransactionStatus);
			header.BH_OA_Importer = importer.MainAddress.PK;
			AssertEquals(importer.PK, supporter.ClientPK);
			moveHeader.BM_OA_WarehouseAddress = warehouse.MainAddress.PK;
			AssertEquals(warehouse.MainAddress, supporter.WarehouseAddress);
			header.BH_SystemCreateTimeUtc = ZDateTime.Now;
			AssertEquals(true, supporter.IsActive);
			header.BH_FTZMove = true;
			AssertEquals(true, supporter.IsExBondAutomationEnabled);
			moveHeader.BM_WarehouseTransactionStatus = "DSK";
			AssertEquals("DSK", supporter.WarehouseTransactionStatus);
			Factory.Save();
			AssertEquals(true, supporter.IsInDatabase);
		}

		public void TestBM_WarehouseTransactionStatusDesc()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals("", moveHeader.BM_WarehouseTransactionStatusDesc);
			moveHeader.BM_WarehouseTransactionStatus = "SD";
			AssertEquals("", moveHeader.BM_WarehouseTransactionStatusDesc);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			AssertEquals(WarehouseTransactionStatusList.Descriptions.OutwardCreated, moveHeader.BM_WarehouseTransactionStatusDesc);
		}

		public void TestIsExBondAutomationEnabled()
		{
			var helper = new WhsDataTestHelper(Factory);
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "W#@33";
			var whsWarehouse = helper.GetNewWhsWarehouse(warehouse.MainAddress.PK, true, "W#@");
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = importer.MainAddress.PK;
			AssertEquals(false, moveHeader.IsExBondAutomationEnabled);
			moveHeader.BM_OA_WarehouseAddress = warehouse.MainAddress.PK;
			AssertEquals(true, moveHeader.IsExBondAutomationEnabled);
			header.BH_FTZMove = false;
			AssertEquals(false, moveHeader.IsExBondAutomationEnabled);
		}

		public void TestSupportsBondedWarehousing()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
			header.BH_FTZMove = false;
			AssertEquals(false, moveHeader.SupportsBondedWarehousing);
			header.BH_FTZMove = true;
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
			header.BH_OA_Importer = ZGuid.Empty;
			AssertEquals(false, moveHeader.SupportsBondedWarehousing);
			header.BH_OA_Importer = importer.MainAddress.PK;
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
			header.BH_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
			header.BH_SystemCreateTimeUtc = ZDateTime.Today;
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			header.BH_FTZMove = false;
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
			header.BH_OA_Importer = ZGuid.Empty;
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
		}

		public void TestHasWHSTransaction()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(false, moveHeader.HasWHSTransaction);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			AssertEquals(true, moveHeader.HasWHSTransaction);
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCanceled;
			AssertEquals(false, moveHeader.HasWHSTransaction);
		}

		public void TestWhsWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "W#@33";
			var address2 = warehouse.Addresses.AddNew();
			var whsWarehouse = helper.GetNewWhsWarehouse(address2.PK, true, "W#@", Warehouse.Integration.CodeLists.WarehouseTypes.Codes.FreeTradeZone);
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = warehouse.MainAddress.PK;
			AssertNull(moveHeader.WhsWarehouse);
			moveHeader.BM_OA_WarehouseAddress = address2.PK;
			AssertEquals(whsWarehouse, moveHeader.WhsWarehouse);
			Assert(moveHeader.IsFTZWarehouse);
			moveHeader.WarehouseAddressOrgPK = ZGuid.Empty;
			AssertNull(moveHeader.WhsWarehouse);
			moveHeader.WarehouseAddressOrgPK = warehouse.PK;
			AssertNull(moveHeader.WhsWarehouse);
			moveHeader.BM_OA_WarehouseAddress = address2.PK;
			AssertEquals(whsWarehouse, moveHeader.WhsWarehouse);
			moveHeader.BM_OA_WarehouseAddress = ZGuid.Empty;
			AssertNull(moveHeader.WhsWarehouse);
			Assert(!moveHeader.IsFTZWarehouse);
		}

		public void TestInbondTransactionResponseWithMix95AndEBBlock()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "K1";
			staff.GS_LoginName = "K1";
			staff.GS_EmailAddress = "kevin@pretend.email.com";
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			var outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			Factory.Save();
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageNum = outgoingMessage.EM_MessageNum;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
			var processor = new InBondProcessor();
			processor.Message = message;
			outgoingMessage.EM_MessageText = @"B015201EJEQP                                01             SEIMIAHAM_1134       
10A62040414371   RLSN2304971230035443376-055469900 N                            
Y  5201EJEQP00001";
			Factory.Save();
			AddMessageBlocksToProcessor(processor,
				"10A62040414371   RLSN2304971230035443376-055469900 N                            ",
				"30                                                                              ",
				"9501001 QP30 RECORD MISSING                                                     ",
				"9501270 TRANSACTION DATA REJECTED                                               ",
				"EYDETAIL REC COUNT NOT EQUAL 'Y' COUNT                                          ",
				"EBTRANSACTION DATA REJECTED                                                     ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("Should be error departure original", ImportMessageStatusList.Codes.ErrorDepartureOriginal, moveHeader.BM_CustomsStatus);
			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var emailBody = Env.OutgoingCustomsMailManager.EmailsCreated[0].Body;
			Assert(!emailBody.Contains("Error Narrative Message"));
		}

		public void TestInbondTransactionResponseWithEBBlock()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "K1";
			staff.GS_LoginName = "K1";
			staff.GS_EmailAddress = "kevin@pretend.email.com";
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			var outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			Factory.Save();
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageNum = outgoingMessage.EM_MessageNum;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
			var processor = new InBondProcessor();
			processor.Message = message;
			outgoingMessage.EM_MessageText = @"B015201EJEQP                                01             SEIMIAHAM_1134       
10A62040414371   RLSN2304971230035443376-055469900 N                            
Y  5201EJEQP00001";
			Factory.Save();
			AddMessageBlocksToProcessor(processor,
				"EYDETAIL REC COUNT NOT EQUAL 'Y' COUNT                                          ",
				"EBTRANSACTION DATA REJECTED                                                     ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("Should be error departure original", ImportMessageStatusList.Codes.ErrorDepartureOriginal, moveHeader.BM_CustomsStatus);
			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var emailBody = Env.OutgoingCustomsMailManager.EmailsCreated[0].Body;
			Assert(emailBody.Contains("Error Narrative Message"));
			Assert(emailBody.Contains("DETAIL REC COUNT NOT EQUAL"));
			Assert(emailBody.Contains("TRANSACTION DATA REJECTED"));
		}

		public void TestHandleNoMasterBillNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			MQEDIMessage outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			Factory.Save();
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_MessageNum = outgoingMessage.EM_MessageNum;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
			var processor = new InBondProcessor();
			processor.Message = message;
			outgoingMessage.EM_MessageText = @"B015201EJEQP                                01             SEIMIAHAM_1134       
10A62040414371   RLSN2304971230035443376-055469900 N                            
Y  5201EJEQP00001";
			Factory.Save();
			AddMessageBlocksToProcessor(processor,
				"10A62040414371   RLSN2304971230035443376-055469900 N                            ",
				"30                                                                              ",
				"9501001 QP30 RECORD MISSING                                                     ",
				"9501270 TRANSACTION DATA REJECTED                                               ");
			processor.Process();
			AssertEquals("Should be error departure original", ImportMessageStatusList.Codes.ErrorDepartureOriginal, moveHeader.BM_CustomsStatus);
		}

		public void TestDefaultBM_GS_NKCusAgent()
		{
			GlbStaff.CurrentUser.GS_Code = "TV";
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			var header = Factory.New<CusInBondHeader>();
			var movementHeader = header.MovementHeaders.AddNew();
			AssertEquals("TV", movementHeader.BM_GS_NKCusAgent);
		}

		public void TestDefaultingFromForeignPortCodeAndUNLOCO()
		{
			var foreignDest = Factory.New<RefUNLOCO>();
			foreignDest.RL_Code = "!ZZ22";
			foreignDest.RL_PortName = "Crystal Lawns";
			var locoMapping = Factory.New<RefLocoMap>();
			locoMapping.RY_LocalPortCode = "!ZZ11";
			locoMapping.RY_RL_NKLocoPort = "!ZZ22";
			locoMapping.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			locoMapping.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping.RY_IsSystem = true;
			var locoMapping2 = Factory.New<RefLocoMap>();
			locoMapping2.RY_LocalPortCode = "!ZZFF";
			locoMapping2.RY_RL_NKLocoPort = "!ZZ22";
			locoMapping2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			locoMapping2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping2.RY_IsSystem = false;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "!ZZ11", "!ZZ11 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "!ZZFF", "!ZZFF Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_ForeignDestPortKCode = "!ZZ11";
			AssertEquals("Defaulting", "!ZZ22", moveHeader.BM_RL_NKForeignDestPort);
			moveHeader.BM_ForeignDestPortKCode = ZString.Empty;
			moveHeader.BM_RL_NKForeignDestPort = ZString.Empty;
			moveHeader.BM_RL_NKForeignDestPort = "!ZZ22";
			AssertEquals("Defaulting user definied", "!ZZFF", moveHeader.BM_ForeignDestPortKCode);
			locoMapping2.Delete();
			moveHeader.BM_ForeignDestPortKCode = ZString.Empty;
			moveHeader.BM_RL_NKForeignDestPort = ZString.Empty;
			moveHeader.BM_RL_NKForeignDestPort = "!ZZ22";
			AssertEquals("Defaulting system definied", "!ZZ11", moveHeader.BM_ForeignDestPortKCode);
		}

		public void TestIControllerIDProviderMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			IControllerIDProvider provider = moveHeader;
			AssertEquals("ControllerID", ControllerIDs.Customs.US.InBond, provider.ControllerID);
			AssertEquals("BusinessObjectPK", header.PK.ToGuid(), provider.BusinessObjectPK);
			var shipment = Factory.New<ForwardingShipment>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			AssertEquals("ControllerID", ControllerIDs.JobShipment, provider.ControllerID);
			AssertEquals("BusinessObjectPK", shipment.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestShouldSynchronise()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			var moveHeader = header.MovementHeader;
			AssertEquals(false, moveHeader.ShouldSynchronise);
			header.BH_ParentID = shipment.PK;
			AssertEquals(true, moveHeader.ShouldSynchronise);
			header.BH_OverrideFreightDefaults = ZBool.True;
			AssertEquals(false, moveHeader.ShouldSynchronise);
			header.BH_OverrideFreightDefaults = ZBool.False;
			AssertEquals(true, moveHeader.ShouldSynchronise);
			var message = Factory.New<MQEDIMessage>();
			moveHeader.Messages.Add(message);
			AssertEquals(false, moveHeader.ShouldSynchronise);
		}

		public void TestIInBondQPHeaderMembers()
		{
			IInBondQPHeader inBondHeader = moveHeader;
			moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
			AssertEquals("BTAIndicator", ZBool.True, inBondHeader.BTAIndicator);
			moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.No;
			AssertEquals("BTAIndicator", ZBool.False, inBondHeader.BTAIndicator);
			CusInBondBill bill1 = Header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "BM123982323";
			CusInBondBill bill2 = Header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "BM68542";
			CusInBondBill bill3 = Header.Bills.AddNew();
			bill3.B0_MasterBillNumber = "BM35845";
			CusInBondMoveDetail moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill2.PK;
			CusInBondMoveDetail moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill1.PK;
			CusInBondMoveDetail moveDetail3 = moveHeader.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill3.PK;
			List<IInBondBillDetails> bills = new List<IInBondBillDetails>(inBondHeader.Bills);
			AssertEquals(3, bills.Count);
			AssertEquals(moveDetail2, bills[0]);
			AssertEquals(moveDetail3, bills[1]);
			AssertEquals(moveDetail1, bills[2]);
			Header.BH_ETA = new ZDateTime(2009, 2, 12);
			moveHeader.BM_ArrivalDate = new ZDateTime(2015, 6, 11);
			AssertEquals("ETAatUnlading", new ZDateTime(2015, 6, 11), inBondHeader.ETAatUnlading);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("EntryType", InbondCommonTypeList.Codes._2TransportandExport, inBondHeader.EntryType);
			moveHeader.BM_InBondCarrierSCAC = "1234";
			AssertEquals("InbondCarrierSCACOrFirms", "1234", inBondHeader.InbondCarrierSCACOrFirms);
			Header.BH_FTZMove = ZBool.True;
			AssertEquals("InbondCarrierSCACOrFirms", "1234", inBondHeader.InbondCarrierSCACOrFirms);
			Header.BH_FIRMS = "0186";
			AssertEquals("FTZFirmsCode", "0186", inBondHeader.FTZFirmsCode);
			AssertEquals("InbondCarrierSCACOrFirms", "1234", inBondHeader.InbondCarrierSCACOrFirms);
			Header.BH_FTZMove = ZBool.True;
			AssertEquals("InbondCarrierSCACOrFirms", "1234", inBondHeader.InbondCarrierSCACOrFirms);
			moveHeader.BM_InBondCarrierSCAC = ZString.Empty;
			AssertEquals("InbondCarrierSCACOrFirms", "0186", inBondHeader.InbondCarrierSCACOrFirms);
			Header.BH_FTZMove = ZBool.False;
			Header.BH_FTZMove = ZBool.True;
			AssertEquals("FTZIndicator", ZBool.True, inBondHeader.FTZIndicator);
			Header.BH_FTZMove = ZBool.False;
			AssertEquals("FTZIndicator", ZBool.False, inBondHeader.FTZIndicator);
			moveHeader.BM_ForeignDestPortKCode = "2085";
			AssertEquals("ForeignDestination", "2085", inBondHeader.ForeignDestination);
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			AssertEquals("ImportTransportMode", TransportModeCodes.Codes.VesselContainer, inBondHeader.ImportTransportMode);
			Header.BH_ImportConveyanceCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals("ImportingCarrierCountryCode", Core.Constants.CountryCodes.Australia, inBondHeader.ImportingCarrierCountryCode);
			Header.BH_CarrierSCAC = "APPD";
			AssertEquals("ImportingCarrierSCAC", "APPD", inBondHeader.ImportingCarrierSCAC);
			Header.BH_FTZMove = ZBool.True;
			AssertEquals("ImportingCarrierSCAC", "0186", inBondHeader.ImportingCarrierSCAC);
			moveHeader.BM_InBondCarrierSCAC = "1234";
			AssertEquals("ImportingCarrierSCAC", "1234", inBondHeader.ImportingCarrierSCAC);
			moveHeader.ThreeLetterInBondAirCarrierCode = "CCC";
			AssertEquals("ImportingCarrierSCAC", "1234", inBondHeader.ImportingCarrierSCAC);
			Header.BH_FTZMove = ZBool.False;
			Header.BH_VoyageNumber = "V323";
			AssertEquals("ImportingCarrierVoyageNumber", "V323", inBondHeader.ImportingCarrierVoyageNumber);
			Header.BH_ImportConveyanceName = "APL VESSEL DUMMY";
			AssertEquals("ImportingConveyanceName", "APL VESSEL DUMMY", inBondHeader.ImportingConveyanceName);
			moveHeader.BM_InBondCarrierID = "9563584652";
			AssertEquals("InBondCarrierID", "9563584652", inBondHeader.InBondCarrierID);
			moveHeader.BM_InBondCarrierSCAC = "APDE";
			AssertEquals("InbondCarrierSCAC", "APDE", inBondHeader.InbondCarrierSCAC);
			Header.BH_FTZMove = ZBool.True;
			AssertEquals("InbondCarrierSCAC", "APDE", inBondHeader.InbondCarrierSCAC);
			moveHeader.BM_InBondCarrierSCAC = "1234";
			AssertEquals("InbondCarrierSCAC", "1234", inBondHeader.InbondCarrierSCAC);
			moveHeader.ThreeLetterInBondAirCarrierCode = "CCC";
			AssertEquals("InbondCarrierSCAC", "1234", inBondHeader.InbondCarrierSCAC);
			Header.BH_FTZMove = ZBool.False;
			Header.BH_PortUnladingDCode = "9635";
			AssertEquals("PortOfUnlading", "9635", inBondHeader.PortOfUnlading);
			moveHeader.BM_DestinationPortCode = "9683";
			AssertEquals("USDestination", "9683", inBondHeader.USDestination);
			moveHeader.BM_MonetaryValue = 1402m;
			AssertEquals("Value", 1402m, inBondHeader.Value);
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals("ImportingConveyanceName", ZString.Empty, inBondHeader.ImportingConveyanceName);
			Header.BH_FTZMove = ZBool.False;
			Header.BH_CarrierSCAC = "AA";
			moveHeader.ThreeLetterInBondAirCarrierCode = "";
			AssertEquals("ImportingCarrierSCAC", "AA", inBondHeader.ImportingCarrierSCAC);
			Header.BH_FTZMove = ZBool.True;
			AssertEquals("ImportingCarrierSCAC", "1234", inBondHeader.ImportingCarrierSCAC);
			moveHeader.BM_InBondCarrierSCAC = "BB";
			AssertEquals("ImportingCarrierSCAC", "BB", inBondHeader.ImportingCarrierSCAC);
			moveHeader.ThreeLetterInBondAirCarrierCode = "CCC";
			AssertEquals("ImportingCarrierSCAC", "CCC", inBondHeader.ImportingCarrierSCAC);
			Header.BH_FTZMove = ZBool.False;
			moveHeader.BM_InBondCarrierSCAC = "AA";
			moveHeader.ThreeLetterInBondAirCarrierCode = "CCC";
			AssertEquals("InbondCarrierSCAC", "CCC", inBondHeader.InbondCarrierSCAC);
			Header.BH_FTZMove = ZBool.True;
			AssertEquals("InbondCarrierSCAC", "CCC", inBondHeader.InbondCarrierSCAC);
			moveHeader.BM_InBondCarrierSCAC = "BB";
			AssertEquals("InbondCarrierSCAC", "CCC", inBondHeader.InbondCarrierSCAC);
		}

		public void TestDestinationPortDCodeAndName()
		{
			var inBondHeader = moveHeader;
			inBondHeader.BM_DestinationPortCode = "A1A1";

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "A1A1", "BEACHFRONT AVENUE", startDate, endDate);
			newFactory.Save();

			AssertEquals("A1A1 BEACHFRONT AVENUE", inBondHeader.DestinationPortDCodeAndName);
		}

		[ExpectNoExceptions]
		public void TestPedimentoNumber()
		{
			var inBondHeader = moveHeader;
			var bill = Header.Bills.AddNew();
			var movementDetail = inBondHeader.MovementDetails.AddNew();
			movementDetail.B9_B0 = bill.PK;
			var reference = bill.AdditionalReferences.AddNew();
			reference.BR_Qualifier = ReferenceQualifierList.Codes.BL;
			reference.BR_ReferenceNum = "123456";
			AssertEquals(ZString.Empty, inBondHeader.PedimentoNumber);
			var reference2 = bill.AdditionalReferences.AddNew();
			reference2.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			AssertEquals(ZString.Empty, inBondHeader.PedimentoNumber);
			reference2.BR_ReferenceNum = "123456";
			AssertEquals("123456", inBondHeader.PedimentoNumber);
			movementDetail.B9_B0 = ZGuid.Empty;
			AssertEquals(ZString.Empty, inBondHeader.PedimentoNumber);
		}

		public void TestForeignDestinationRefLocoMappings()
		{
			var foreignDest = Factory.New<RefUNLOCO>();
			foreignDest.RL_Code = "!ZZ22";
			foreignDest.RL_PortName = "Crystal Lawns";
			var locoMapping = Factory.New<RefLocoMap>();
			locoMapping.RY_LocalPortCode = "!ZZ11";
			locoMapping.RY_RL_NKLocoPort = "!ZZ22";
			locoMapping.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			locoMapping.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping.RY_IsSystem = false;
			var locoMapping2 = Factory.New<RefLocoMap>();
			locoMapping2.RY_LocalPortCode = "!ZZFF";
			locoMapping2.RY_RL_NKLocoPort = "!ZZ22";
			locoMapping2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			locoMapping2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping2.RY_IsSystem = false;
			var locoMapping3 = Factory.New<RefLocoMap>();
			locoMapping3.RY_LocalPortCode = "!ZZDD";
			locoMapping3.RY_RL_NKLocoPort = "!ZZ22";
			locoMapping3.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			locoMapping3.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping3.RY_IsSystem = true;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "!ZZ11", "!ZZ11 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "!ZZFF", "!ZZFF Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var inBondHeader = moveHeader;
			AssertEquals(nameof(ZArchitecture.FieldType.TextCodeFindBox), inBondHeader.BM_ForeignDestPortKCodeType);
			AssertEquals(0, inBondHeader.ForeignDestinationRefLocoMappings.Count);

			inBondHeader.BM_RL_NKForeignDestPort = "!ZZ22";
			AssertEquals(nameof(ZArchitecture.FieldType.TextDropEdit), inBondHeader.BM_ForeignDestPortKCodeType);
			AssertEquals(2, inBondHeader.ForeignDestinationRefLocoMappings.Count);
			Assert(inBondHeader.ForeignDestinationRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "!ZZ11"));
			Assert(inBondHeader.ForeignDestinationRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "!ZZFF"));
		}

		public void TestProcessX0X1ForInBond()
		{
			var inBondHeader = moveHeader;
			var bill1 = Header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "BM123982323";
			Factory.Save();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var sendingMessage = mock.Object;
			sendingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;
			sendingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			sendingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			sendingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sendingMessage.EM_MessageNum = "~15000";
			sendingMessage.EM_LinkedObject = inBondHeader;
			new InBondMessageStatusCalculator(inBondHeader).CalculateStatus(sendingMessage, ABIResponseStatus.Undefined);
			Assert(inBondHeader.IsWaitingForResponse);
			var generator = new ImportOutputBlockControlGenerator<AABIOutputB, AABIOutputY>();
			generator.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;
			generator.B.ProcessingDistrictPortCode = "8888";
			generator.B.FilerPreparersUserDataText = "~15000";
			generator.AddMessageBlock(CreateAABIX0("X0 BLOCK       1 REF ID:      286    AE YASYUSPRD_70018"));
			generator.AddMessageBlock(CreateAABIX1("X1 FX18   PROC PORT/FLR NOT AUTHRZD FOR SENDR/RCVR"));
			generator.AddMessageBlock(CreateAABIX1("X1RF999   BATCH REJECTED"));
			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			interchange.EI_From = "USC";
			interchange.EI_To = "SV9";
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Receive;
			interchange.EI_HeaderText = "A3901SV9      05140701   051407014539                                00000000039";
			interchange.EI_BodyText = generator.Serialise();
			interchange.EI_FooterText = "Z3901SV9      05140701   051407014539                                00000000039";
			MQEDIMessage message = (MQEDIMessage)interchange.ContainedMessages.AddNew(typeof(MQEDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;
			message.EM_MessageText = generator.Serialise();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "~15000";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			inBondHeader.Reload();
			Assert(!inBondHeader.IsWaitingForResponse);
			mock.VerifyAll();
		}

		public void TestIInBondQXHeaderMember()
		{
			IInBondQXHeader inBondHeader = moveHeader;
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("EntryType", InbondCommonTypeList.Codes._2TransportandExport, inBondHeader.EntryType);
			CusInBondBill bill1 = Header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "BM123982323";
			CusInBondBill bill2 = Header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "BM68542";
			CusInBondBill bill3 = Header.Bills.AddNew();
			bill3.B0_MasterBillNumber = "BM35845";
			CusInBondMoveDetail moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill2.PK;
			CusInBondMoveDetail moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill1.PK;
			CusInBondMoveDetail moveDetail3 = moveHeader.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill3.PK;
			List<IInBondQXBillDetails> bills = new List<IInBondQXBillDetails>(inBondHeader.Bills);
			AssertEquals(3, bills.Count);
			AssertEquals(moveDetail2, bills[0]);
			AssertEquals(moveDetail3, bills[1]);
			AssertEquals(moveDetail1, bills[2]);
			moveHeader.BM_InBondCarrierSCAC = "ABCD";
			AssertEquals("Carrier Code", "ABCD", inBondHeader.CarrierCode);
			moveHeader.BM_DestinationPortCode = "1234";
			AssertEquals("US Port of Destination", "1234", inBondHeader.USPortOfDestination);
			moveHeader.BM_ForeignDestPortKCode = "12345";
			AssertEquals("Foreignt Destination", "12345", inBondHeader.ForeignDestination);
			moveHeader.BM_MonetaryValue = 12345678;
			AssertEquals("Value", 12345678, inBondHeader.Value);
			moveHeader.BM_MonetaryValue = 1000000000;
			AssertEquals("Value", ZInt.Zero, inBondHeader.Value);
			moveHeader.BM_MonetaryValue = 999.54;
			AssertEquals("Value", 999, inBondHeader.Value);
			moveHeader.BM_MonetaryValue = -24;
			AssertEquals("Value", ZInt.Zero, inBondHeader.Value);
			moveHeader.BM_InBondCarrierID = "012345678901";
			AssertEquals("Carrier ID", "012345678901", inBondHeader.InBondCarrierID);
			Header.BH_CarrierSCAC = "123";
			AssertEquals("Importing Carrier Code", "123", inBondHeader.ImportingCarrierCode);
			Header.BH_VoyageNumber = "12345";
			AssertEquals("Flight Number", "12345", inBondHeader.ImportingCarrierFlightNumber);
			Header.BH_PortUnladingDCode = "1234";
			AssertEquals("District Port of Importing Conveyance Arrival", "1234", inBondHeader.DistrictPortOfImportingConveyanceArrival);
			Header.BH_ETA = new ZDateTime(2011, 09, 02);
			moveHeader.BM_ArrivalDate = new ZDateTime(2015, 06, 02);
			AssertEquals("Estimate Date Of Arrival", new ZDateTime(2011, 09, 02), inBondHeader.EstimatedDateOfArrival);
			Header.BH_JobReference = "INB000001";
			moveHeader.InBondNumber = "123456789";
			AssertEquals("Job Number", "INB000001 / 123456789", inBondHeader.JobNumber);
		}

		public void TestIInBondWXHeaderMemeber()
		{
			IInBondWXHeader inBondHeader = moveHeader;
			AssertEquals("Master Bill Number", ZString.Empty, inBondHeader.MasterBillNumber);
			AssertEquals("House Bill Number", ZString.Empty, inBondHeader.HouseBillNumber);
			moveHeader.BM_ArrivalDate = new ZDateTime(2011, 09, 01, 14, 10, 00);
			AssertEquals("Arrival Date Time", new ZDateTime(2011, 09, 01, 14, 10, 00), inBondHeader.ArrivalDateTime);
			moveHeader.BM_ExportDate = new ZDateTime(2011, 08, 31, 13, 00, 59);
			AssertEquals("In Bond Number", new ZDateTime(2011, 08, 31, 13, 00, 59), inBondHeader.ExportDateTime);
			moveHeader.BM_DestinationPortCode = "ABCD";
			AssertEquals("Port Of Arrival", "ABCD", inBondHeader.ScheduleDPortOfArrival);
			moveHeader.BM_DestinationPortCode = "DCBA";
			AssertEquals("Port of Departure Or Export", "DCBA", inBondHeader.PortOfExport);
			moveHeader.BM_ExportTransportMode = "10";
			AssertEquals("Export MOT", "10", inBondHeader.ExportMOT);
			Header.BH_CarrierSCAC = "ABC";
			AssertEquals("Import Carrier Code", "ABC", inBondHeader.ImportingCarrierCode);
			Header.BH_VoyageNumber = "12345";
			AssertEquals("Importing Carrier Flight Number", "12345", inBondHeader.ImportingCarrierFlightNumber);
			Header.BH_ETA = new DateTime(2011, 09, 02);
			AssertEquals("Estimate Date Of Arrival", moveHeader.BM_ArrivalDate, inBondHeader.ImportingCarrierScheduleArrivalDate);
			Header.BH_JobReference = "INB000001";
			moveHeader.InBondNumber = "123456789";
			AssertEquals("Job Number", "INB000001 / 123456789", inBondHeader.JobNumber);
		}

		public void TestGeneratePendingMessage()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.GeneratePendingOriginalForAmendment(InBondMessageType.DepartureAmend);
			AssertEquals("Original message should have been generated", 1, moveHeader.Messages.Count);
			MQEDIMessage generatedMessage = (MQEDIMessage)moveHeader.Messages[0];
			AssertEquals("Message should be in pending state", EDIMessage.Status.Pending, generatedMessage.EM_Status);
			AssertEquals("Message should be QP", "QP", generatedMessage.EM_FormattedMessageText.Substring(10, 2));
			moveHeader.Messages.RemoveAndDeleteAll();
			moveHeader.GeneratePendingOriginalForAmendment(InBondMessageType.AirInBondAmend);
			AssertEquals("Original message should have been generated", 1, moveHeader.Messages.Count);
			generatedMessage = (MQEDIMessage)moveHeader.Messages[0];
			AssertEquals("Message should be in pending state", EDIMessage.Status.Pending, generatedMessage.EM_Status);
			AssertEquals("Message should be QP", "QP", generatedMessage.EM_FormattedMessageText.Substring(10, 2));
		}

		public void TestIMessageAttacheeWithCBPSenderReferenceMembers()
		{
			IMessageAttacheeWithCBPSenderReference messageAttachee = moveHeader;
			AssertEquals("", messageAttachee.EntryFilerCode);
			AssertEquals("", messageAttachee.ProcessingDistrictPort);
			AssertEquals("", messageAttachee.ProcessingOfficeCode);
			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), messageAttachee.Branch);
			AssertEquals(Factory, messageAttachee.Factory);
			AssertEquals(moveHeader.Messages, messageAttachee.Messages);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			AssertEquals(Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearDepartureOriginal, messageAttachee.MessageStatus);
			AssertEquals(Header, messageAttachee.TopLevelBusinessObject);
		}

		public void TestICBPEDIMessageMessageTextNumberPlaceHolderFillerMembers()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DbConnection connection = ((IDbConnected)Factory).Connection;
			try
			{
				connection.BeginTransaction();
				CusInBondHeader header = Factory.New<CusInBondHeader>();
				CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
				MQEDIMessage message = Factory.New<MQEDIMessage>();
				string messageData = "START{0}END";
				string defaultMessageText = string.Format(messageData, MQEDIMessage.InBondNumberPlaceHolder);
				message.EM_MessageText = defaultMessageText;
				ICBPEDIMessageMessageTextNumberPlaceHolderFiller filler = moveHeader;
				AssertEquals("", moveHeader.InBondNumber);
				filler.Fill(message);
				AssertNotEquals("", moveHeader.InBondNumber);
				AssertEquals(string.Format(messageData, moveHeader.InBondNumber.PadRight(MQEDIMessage.InBondNumberPlaceHolder.Length)), message.EM_MessageText);
				header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
				moveHeader.InBondNumber = ZString.Empty;
				filler.Fill(message);
				AssertEquals("", moveHeader.InBondNumber);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestAutomaticallyAllocateNumber()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DbConnection connection = ((IDbConnected)Factory).Connection;
			try
			{
				connection.BeginTransaction();
				var header = Factory.New<CusInBondHeader>();
				header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
				AssertEquals(Enterprise.Customs.US.Business.TransportTypeList.Codes.Air, ((ICargoManifestStatusQueryHeader)header).TransportMode);
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				header.BH_OA_Importer = importer.MainAddress.PK;
				var moveHeader = header.MovementHeader;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "001234578";
				bill.B0_HouseBillNumber = "HouseBill1";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var commodity = container.Commodities.AddNew();
				commodity.BY_InvoiceQuantity = 10m;
				Factory.Save();
				var sendingObject = new CargoManifestStatusQueryHeaderObject(header);
				sendingObject.ActionCode = CargoManifestStatusQueryActionList.Codes.HAWB;
				sendingObject.SendingObjects[0].ShouldSendMessage = true;
				AssertEquals("1 message was created.", 1, sendingObject.SendQueryMessage());
				Factory.Save();
				AssertEquals("", moveHeader.InBondNumber);
				sendingObject = new CargoManifestStatusQueryHeaderObject(header);
				sendingObject.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;
				sendingObject.SendingObjects[0].ShouldSendMessage = true;
				AssertEquals("1 message was created.", 1, sendingObject.SendQueryMessage());
				Factory.Save();
				AssertEquals("", moveHeader.InBondNumber);
				var sendingObject2 = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
				var message2 = sendingObject2.Send();
				Factory.Save();
				AssertNotEquals("", moveHeader.InBondNumber);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public override void TestLookups()
		{
			base.TestLookups();
			AssertEquals(typeof(CusInBondMoveHeaderLookups), moveHeader.Lookups.GetType());
		}

		public override void TestValidation()
		{
			base.TestValidation();
			AssertEquals(typeof(CusInBondMoveHeaderValidation), moveHeader.Validation.GetType());
		}

		[ExpectNoExceptions]
		public void TestMajorMarkAsNeedingValidationCore()
		{
			var mockHeader = Factory.NewMoq<CusInBondHeader>();
			mockHeader.Protected().Setup("MarkAsNeedingValidationCore");
			CusInBondHeader header = mockHeader.Object;
			var mockMoveHeader = Factory.NewMoq<CusInBondMoveHeader>();
			mockMoveHeader.Protected().Setup("MarkAsNeedingValidationCore");
			CusInBondMoveHeader moveHeader = mockMoveHeader.Object;
			header.MovementHeaders.Add(moveHeader);
			var mockMoveDetail = Factory.NewMoq<CusInBondMoveDetail>();
			mockMoveDetail.Protected().Setup("MarkAsNeedingValidationCore");
			CusInBondMoveDetail moveDetail = mockMoveDetail.Object;
			moveHeader.MovementDetails.Add(moveDetail);
			moveHeader.BM_BH = ZGuid.Empty;
			moveHeader.BM_BH = header.PK;
			mockHeader.Protected().Verify("MarkAsNeedingValidationCore", Times.Exactly(2));
			mockMoveHeader.Protected().Verify("MarkAsNeedingValidationCore", Times.Exactly(3));
			mockMoveDetail.Protected().Verify("MarkAsNeedingValidationCore", Times.Exactly(2));
		}

		public void TestIWorkflowTriggerFieldChangeSourceMember()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			IWorkflowTriggerFieldChangeSource triggerSource = moveHeader;
			AssertCollectionContains(header, triggerSource.ParentWorkflowProviders);
		}

		public void TestIsFTZWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "W#@33";
			var whsWarehouse = helper.GetNewWhsWarehouse(warehouse.MainAddress.PK, true, "W#@");
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals("moveHeader.IsFTZWarehouse", false, moveHeader.IsFTZWarehouse);
			moveHeader.BM_OA_WarehouseAddress = warehouse.MainAddress.PK;
			AssertEquals("moveHeader.IsFTZWarehouse", false, moveHeader.IsFTZWarehouse);
			whsWarehouse.WW_WarehouseType = Warehouse.Integration.CodeLists.WarehouseTypes.Codes.FreeTradeZone;
			AssertEquals("moveHeader.IsFTZWarehouse", true, moveHeader.IsFTZWarehouse);
		}

		public void TestHasAtLeastOneCommodityWithProductWithoutProperEntryDetails()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "ENT1";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity1 = container.Commodities.AddNew();
			var commodity2 = container.Commodities.AddNew();
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			commodity2.BY_PartNumber = "PART1";
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			commodity2.BY_WarehouseEntryLineNo = 1;
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			commodity2.BY_WarehouseEntryNumber = "ENT2";
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			commodity2.BY_WarehouseEntryNumber = "";
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			commodity2.BY_WarehouseEntryNumber = "ENT2";
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			commodity2.BY_WarehouseEntryLineNo = 0;
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			commodity2.BY_WarehouseEntryLineNo = 1;
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			var childCommodity = commodity1.ChildCommodities.AddNew();
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			childCommodity.BY_PartNumber = "PART2";
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			childCommodity.BY_WarehouseEntryLineNo = 2;
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
			childCommodity.BY_WarehouseEntryNumber = "ENT1";
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails);
		}

		public void TestHasAtLeastOneCommodityWithProductWithoutInvoiceQuantity()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutInvoiceQuantity);
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity1 = container.Commodities.AddNew();
			var commodity2 = container.Commodities.AddNew();
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutInvoiceQuantity);
			commodity2.BY_PartNumber = "PART1";
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithProductWithoutInvoiceQuantity);
			commodity2.BY_InvoiceQuantity = 10m;
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutInvoiceQuantity);
			var childCommodity = commodity1.ChildCommodities.AddNew();
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutInvoiceQuantity);
			childCommodity.BY_PartNumber = "PART2";
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithProductWithoutInvoiceQuantity);
			childCommodity.BY_InvoiceQuantity = 30m;
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProductWithoutInvoiceQuantity);
		}

		public void TestHasAtLeastOneCommodityWithProduct()
		{
			var helper = new WhsDataTestHelper(Factory);
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProduct);
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity1 = container.Commodities.AddNew();
			var commodity2 = container.Commodities.AddNew();
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProduct);
			commodity2.BY_PartNumber = helper.Part.OP_PartNum;
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithProduct);
			commodity2.BY_PartNumber = "SD2!2";
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProduct);
			var childCommodity = commodity1.ChildCommodities.AddNew();
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProduct);
			childCommodity.BY_PartNumber = "JD$%@#";
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithProduct);
			childCommodity.BY_PartNumber = helper.Part2.OP_PartNum;
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithProduct);
		}

		public void TestHasAtLeastOneCommodityWithoutProduct()
		{
			var helper = new WhsDataTestHelper(Factory);
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithoutProduct);
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity1 = container.Commodities.AddNew();
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithoutProduct);
			commodity1.BY_PartNumber = "JD$%@#";
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithoutProduct);
			commodity1.BY_PartNumber = helper.Part2.OP_PartNum;
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithoutProduct);
			var commodity2 = container.Commodities.AddNew();
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithoutProduct);
			var childCommodity = commodity2.ChildCommodities.AddNew();
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithoutProduct);
			childCommodity.BY_PartNumber = "JD$%@#";
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithoutProduct);
			childCommodity.BY_PartNumber = helper.Part.OP_PartNum;
			AssertEquals(false, moveHeader.HasAtLeastOneCommodityWithoutProduct);
			childCommodity.Delete();
			AssertEquals(true, moveHeader.HasAtLeastOneCommodityWithoutProduct);
		}

		public void TestHasAtLeastOneDetail()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			Assert(!moveHeader.HasAtLeastOneDetail);
			moveHeader.MovementDetails.AddNew();
			Assert(moveHeader.HasAtLeastOneDetail);
		}

		public void TestInBondNumberAndJobReference()
		{
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			AssertEquals("-", moveHeader.InBondNumberAndJobReference);
			moveHeader.InBondNumber = "INB3242";
			AssertEquals("INB3242-", moveHeader.InBondNumberAndJobReference);
			var header = Factory.New<CusInBondHeader>();
			header.MovementHeaders.Add(moveHeader);
			AssertEquals("INB3242-", moveHeader.InBondNumberAndJobReference);
			header.BH_JobReference = "JD32342";
			AssertEquals("INB3242-JD32342", moveHeader.InBondNumberAndJobReference);
		}

		public void TestHasBeenDeleted()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.RefreshEnabled = false;
			var moveHeader = header.MovementHeaders.AddNew();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var header2 = newFactory.Load<CusInBondHeader>(header.PK);
			var moveHeader2 = newFactory.Load<CusInBondMoveHeader>(moveHeader.PK);
			moveHeader.Delete();
			Factory.Save();
			Assert(moveHeader2.HasBeenDeleted);
		}

		public void TestDeleteMoveHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var header0 = Factory.New<CusInBondHeader>();
			header0.BH_ParentID = declaration.PK;
			header0.BH_ParentTableCode = declaration.TablePrefix;
			var moveHeader0 = header0.MovementHeaders.AddNew();
			Assert("Movement Header can't be deleted.", !moveHeader0.CanDelete);
			AssertEquals(string.Format("This Movement cannot be deleted {0}.", ValidationConstants.Synchronize.SynchronizedFromParent(JobDeclarationSchema.Constants.TableName)), moveHeader0.ReasonForNotAbleToDelete);
			header0.BH_OverrideFreightDefaults = true;
			Assert("Movement Header can be deleted.", moveHeader0.CanDelete);
			var header1 = Factory.New<CusInBondHeader>();
			var moveHeader1 = header1.MovementHeaders.AddNew();
			Assert("Movement Header can be deleted.", moveHeader1.CanDelete);
		}

		public void TestFirmsCode()
		{
			var header = Factory.New<CusInBondMoveHeader>();
			header.BM_FIRMS = "1234";
			IInBondArriveExportTOLHeader iheader = header;
			AssertEquals("Return the firms code should be 1234", "1234", iheader.ArrivalFirmsCode);
		}

		public void TestFindBillMatchingNumber()
		{
			var truckHeader = Factory.New<CusInBondHeader>();
			truckHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			var truckMovement = truckHeader.MovementHeader;
			var truckBill = truckHeader.Bills.AddNew();
			truckBill.B0_IssuerSCAC = "SAIA";
			truckBill.B0_MasterBillNumber = "10-4 .655718805";
			truckBill.B0_HouseBillNumber = "1042/1 18*0143";
			var truckMoveDetail = truckMovement.MovementDetails.AddNew();
			truckMoveDetail.B9_B0 = truckBill.PK;
			var truckQPHeader = (IInBondQPHeader)truckMovement;
			var matchedMoveDetail = (CusInBondMoveDetail)truckQPHeader.FindBillMatchingNumber("104655718805", ZString.Empty);
			AssertEquals(truckMoveDetail.PK, matchedMoveDetail.PK);
			matchedMoveDetail = (CusInBondMoveDetail)truckQPHeader.FindBillMatchingNumber("104655718805", "10421180143");
			AssertEquals(truckMoveDetail.PK, matchedMoveDetail.PK);
			var railHeader = Factory.New<CusInBondHeader>();
			truckHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.RailContainer;
			var railMovement = railHeader.MovementHeader;
			var railQPHeader = (IInBondQPHeader)railMovement;
			var railBill = railHeader.Bills.AddNew();
			railBill.B0_IssuerSCAC = "SAIA";
			railBill.B0_MasterBillNumber = "104655718806";
			railBill.B0_HouseBillNumber = "10421180146";
			var railMoveDetail = railMovement.MovementDetails.AddNew();
			railMoveDetail.B9_B0 = railBill.PK;
			matchedMoveDetail = (CusInBondMoveDetail)railQPHeader.FindBillMatchingNumber("104655718806", ZString.Empty);
			AssertEquals(railMoveDetail.PK, matchedMoveDetail.PK);
			matchedMoveDetail = (CusInBondMoveDetail)railQPHeader.FindBillMatchingNumber("104655718806", "10421180146");
			AssertNull(matchedMoveDetail);
			var airHeader = Factory.New<CusInBondHeader>();
			airHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var airMovement = airHeader.MovementHeader;
			var airBill0 = airHeader.Bills.AddNew();
			airBill0.B0_IssuerSCAC = "CI";
			airBill0.B0_MasterBillNumber = "29714581571";
			airBill0.B0_HouseBillNumber = "13321140225";
			var airBill1 = airHeader.Bills.AddNew();
			airBill1.B0_IssuerSCAC = "CI";
			airBill1.B0_MasterBillNumber = "29714581571";
			airBill1.B0_HouseBillNumber = "13321140342";
			var airMoveDetail0 = airMovement.MovementDetails.AddNew();
			airMoveDetail0.B9_B0 = airBill0.PK;
			var airMoveDetail1 = airMovement.MovementDetails.AddNew();
			airMoveDetail1.B9_B0 = airBill1.PK;
			var airQPHeader = (IInBondQPHeader)airMovement;
			matchedMoveDetail = (CusInBondMoveDetail)airQPHeader.FindBillMatchingNumber("14581571", "13321140225");
			AssertEquals(airMoveDetail0.PK, matchedMoveDetail.PK);
			matchedMoveDetail = (CusInBondMoveDetail)airQPHeader.FindBillMatchingNumber("14581571", "013321140225");
			AssertEquals(airMoveDetail0.PK, matchedMoveDetail.PK);
			matchedMoveDetail = (CusInBondMoveDetail)airQPHeader.FindBillMatchingNumber("14581571", "13321140342");
			AssertEquals(airMoveDetail1.PK, matchedMoveDetail.PK);
			matchedMoveDetail = (CusInBondMoveDetail)airQPHeader.FindBillMatchingNumber("14581571", "13321140223");
			AssertNull(matchedMoveDetail);
		}

		protected override CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header) => ((CusInBondHeader)header).MovementHeaders.AddNew();

		protected override Customs.Business.CusInBondHeader CreateNewCusInBondHeader() => Factory.New<CusInBondHeader>();

		CusInBondHeader Header => (CusInBondHeader)header;

		AABIX0 CreateAABIX0(ZString data)
		{
			var response = new AABIX0();
			response.Deserialise(BlockPadder.Pad(data));
			return response;
		}

		AABIOutputX1 CreateAABIX1(ZString data)
		{
			var response = new AABIOutputX1();
			response.Deserialise(BlockPadder.Pad(data));
			return response;
		}

		void AddMessageBlocksToProcessor(InBondProcessor processor, params string[] messageBlocks)
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, messageBlocks);
			foreach (MessageBlock messageBlock in block.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}
		}
	}
}
