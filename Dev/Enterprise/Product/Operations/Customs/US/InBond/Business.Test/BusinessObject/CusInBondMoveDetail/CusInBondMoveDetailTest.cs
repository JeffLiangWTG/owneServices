using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.InBond.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Workflow.ProcessTasks.Milestones;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetail))]
	sealed class CusInBondMoveDetailTest : CusInBondMoveDetailTest<CusInBondMoveDetail>
	{
		public void TestNoExceptionWhenHeaderIsNull()
		{
			var moveDetail = Factory.New<CusInBondMoveDetail>();
			AssertNoExceptionThrown("When Header is null, get_DispositionCodeDescriptionList is no exception thrown", () => _ = moveDetail.DispositionCodeDescriptionList);
			AssertNoExceptionThrown("When Header is null, get_LatestDispositionForInBondClosedDate is no exception thrown", () => _ = moveDetail.LatestDispositionForInBondClosedDate);
			AssertNoExceptionThrown("When Header is null, get_TopLevelBizObjReferenceNumber is no exception thrown", () => _ = ((IMessageAttachee)moveDetail).TopLevelBizObjReferenceNumber);
			AssertNoExceptionThrown("When Header is null, get_Branch is no exception thrown", () => _ = ((IMessageAttachee)moveDetail).Branch);
			AssertNoExceptionThrown("When Header is null, get_TopLevelBusinessObjectLogs is no exception thrown", () => _ = ((IMessageAttachee)moveDetail).TopLevelBusinessObjectLogs);
			AssertNoExceptionThrown("When Header is null, get_JobHeaderCompany is no exception thrown", () => _ = ((IWorkflowTriggerEventSource)moveDetail).JobHeaderCompany);
		}

		public void TestMilestoneFromHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_SeqNo = "0001";
			moveDetail.B9_B0 = bill.PK;

			var milestone = header.WorkflowItems.Milestones.AddNew();
			var condition = milestone as ITriggerConditions;
			condition.TriggerEventCode = AutoEvents.MessageStatusChange.Code;
			condition.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			condition.TriggerConditionValue = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			milestone.P9_ActualDateUpdateType = ActualDateUpdateTypeCodeList.Codes.AD1;

			var outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
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

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse,
				"10A62040414371   RLSN2304971230035443376-055469900 N                            ",
				"30A 0001                                                                        ",
				"9502220 ACCEPTED                                                                ",
				"Y  5201EJEQP00001                                                               ");
			AssertEquals(ZDateTime.Empty, milestone.P9_ActualDate);
			processor.Process();

			AssertEquals(true, moveDetail.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageStatusChange.Code && x.SL_Table == CusInBondMoveDetailSchema.Constants.TableName));
			AssertEquals(false, milestone.P9_ActualDate.IsEmpty);
		}

		public void TestIWorkflowTriggerEventSource()
		{
			var source = MoveDetail as IWorkflowTriggerEventSource;
			AssertEquals(MoveHeader.Company.PK, source.JobHeaderCompany.PK);
			AssertEquals(true, source.ParentWorkflowProviders.Contains(MoveHeader));
		}

		public void TestCusInBondContainerType()
		{
			var supporter = MoveDetail as ICusInBondContainerTypeSupporter;
			AssertEquals(typeof(CusInBondContainer), supporter.ContainerType);
			AssertEquals(typeof(CusInBondContainer), MoveDetail.ContainerType);
		}

		public void TestUniversalCopy()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])typeof(CusInBondMoveDetail).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false);
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertEquals(5, ignoreElementAttributes[0].ElementNames.Count);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusInBondBill, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusInBondContainers, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.InBondMoveDetail, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondMoveDetail.Schema.B9_CustomsStatus, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondMoveDetail.Schema.B9_MessageStatus, ignoreElementAttributes[0].ElementNames);

			var containers = typeof(CusInBondMoveDetail).GetProperty(nameof(CusInBondMoveDetail.Containers), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(containers, typeof(UniversalCopyCollectionEntityAttribute)));

			var cBP7512Lines = typeof(CusInBondMoveDetail).GetProperty(nameof(CusInBondMoveDetail.CBP7512Lines), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(cBP7512Lines, typeof(UniversalCopyCollectionEntityAttribute)));

			var warehouseDetails = typeof(CusInBondMoveDetail).GetProperty(nameof(CusInBondMoveDetail.WarehouseDetails), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(warehouseDetails, typeof(UniversalCopyCollectionEntityAttribute)));
		}

		public override void TestProperties()
		{
			AssertEquals("Move Detail ", MoveDetail.HumanReadableName);
			MoveHeader.InBondNumber = "964397663";
			AssertEquals("Move Detail 964397663 / ", MoveDetail.HumanReadableName);
			AssertEquals("964397663-", MoveDetail.InbondNumberAndBillNumber);
			Bill.B0_MasterBillNumber = "MB234422";
			AssertEquals("Move Detail 964397663 / MB234422", MoveDetail.HumanReadableName);
			AssertEquals("MB234422", MoveDetail.MasterBillNumber);
			AssertEquals("964397663-MB234422", MoveDetail.InbondNumberAndBillNumber);
			MoveHeader.InBondNumber = ZString.Empty;
			AssertEquals("Move Detail MB234422", MoveDetail.HumanReadableName);
			AssertEquals("-MB234422", MoveDetail.InbondNumberAndBillNumber);
		}

		public void TestNotToSetClosedDateInMessageStatusBCBDBEBGBH191S()
		{
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(Factory);
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
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "51", "51 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "BD", "1C DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1J", "1J DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "4E", "92 DESC", startDate, endDate);
			var attribute11 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName2.ZXE_Name, "Y");
			var attribute21 = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName3.ZXE_Name, "Y");
			var attribute41 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName3.ZXE_Name, "Y");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "267";
			var inbond = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond.BH_ParentID = declaration.PK;
			inbond.BH_ParentTableCode = declaration.TablePrefix;
			var bill1 = (Customs.Business.CusInBondBill)inbond.Bills.AddNew();
			bill1.B0_MasterBillNumber = "130851952006";
			bill1.B0_IssuerCode = "CPRS";
			var moveHeader = inbond.MovementHeader;
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			var moveDetail1 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			moveDetail1.B9_SeqNo = "1";
			Factory.Save();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			response.EM_Status = EDIMessage.Status.Queued;
			var processor = new NotificationOfStatusProcessor();
			processor.Message = response;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",

				"3051CPRS130851952006                                0000000624 1303280831       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("InBond Closed", new ZDateTime(2013, 3, 28, 8, 31, 0), moveHeader.BM_InBondClosedDate);
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"30BDCPRS130851952006                                0000000624 1303290841       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("InBond closed date should not be clear.", new ZDateTime(2013, 3, 28, 8, 31, 0), moveHeader.BM_InBondClosedDate);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("InBond closed date should not be clear.", new ZDateTime(2013, 3, 28, 8, 31, 0), moveHeader.BM_InBondClosedDate);
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"304ECPRS130851952006                                0000000624 1303280821       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			AssertEquals("InBond closed date should not be clear.", new ZDateTime(2013, 3, 28, 8, 31, 0), moveHeader.BM_InBondClosedDate);
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"301JCPRS130851952006                                0000000624 1303290851       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			Assert("InBond closed date should be clear.", moveHeader.BM_InBondClosedDate.IsEmpty);
			mock.Protected().Verify("GetNumberFountainNumbersAndFillInPlaceHolders", Times.Never());
		}

		public void TestCloseMoveHeader()
		{
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(Factory);
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
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "51", "51 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1C", "1C DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1J", "1J DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "92", "92 DESC", startDate, endDate);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "93", "93 DESC", startDate, endDate);
			var code6 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "11", "11 DESC", startDate, endDate);
			var attribute11 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName2.ZXE_Name, "Y");
			var attribute21 = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName3.ZXE_Name, "Y");
			var attribute41 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName2.ZXE_Name, "Y");
			var attribute51 = helper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, attributeName2.ZXE_Name, "Y");
			var attribute61 = helper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, attributeName1.ZXE_Name, "Y");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "267";
			var inbond = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond.BH_ParentID = declaration.PK;
			inbond.BH_ParentTableCode = declaration.TablePrefix;
			var bill1 = (Customs.Business.CusInBondBill)inbond.Bills.AddNew();
			bill1.B0_MasterBillNumber = "130851952006";
			bill1.B0_IssuerCode = "CPRS";
			var moveHeader = inbond.MovementHeader;
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			var moveDetail1 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			moveDetail1.B9_SeqNo = "1";
			Factory.Save();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			response.EM_Status = EDIMessage.Status.Queued;
			var processor = new NotificationOfStatusProcessor();
			processor.Message = response;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"3051CPRS130851952006                                0000000624 1303280841       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("InBond Closed", new ZDateTime(2013, 3, 28, 8, 41, 0), moveHeader.BM_InBondClosedDate);
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"301JCPRS130851952006                                0000000624 1303290841       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			Assert("InBond closed date should be clear.", moveHeader.BM_InBondClosedDate.IsEmpty);
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"301CCPRS130851952006                                0000000624 1303291341       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			Assert("InBond closed date should be clear.", moveHeader.BM_InBondClosedDate.IsEmpty);
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"3011CPRS130851952006                                0000000624 1303290941       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("InBond Closed", new ZDateTime(2013, 3, 29, 9, 41, 0), moveHeader.BM_InBondClosedDate);
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"301JCPRS130851952006                                0000000624 1303291541       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			Assert("InBond closed date should be clear.", moveHeader.BM_InBondClosedDate.IsEmpty);
			var bill2 = (Customs.Business.CusInBondBill)inbond.Bills.AddNew();
			bill2.B0_MasterBillNumber = "130851952007";
			bill2.B0_IssuerCode = "CPRS";
			var moveDetail2 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill2.PK;
			moveDetail2.B9_SeqNo = "2";
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"30922PRS130851952006                                0000000624 1304290841       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			Assert("InBond should not be closed, because there is a move detail not be closed.", moveHeader.BM_InBondClosedDate.IsEmpty);
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"30933PRS130851952007                                0000000624 1304290841       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("InBond Closed, set the max datetime", new ZDateTime(2013, 4, 29, 8, 41, 0), moveHeader.BM_InBondClosedDate);
			mock.Protected().Verify("GetNumberFountainNumbersAndFillInPlaceHolders", Times.Never());
		}

		public void TestAddEventForInbondStatusNotificationMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "267";
			var inbond = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond.BH_ParentID = declaration.PK;
			inbond.BH_ParentTableCode = declaration.TablePrefix;
			var bill1 = (Customs.Business.CusInBondBill)inbond.Bills.AddNew();
			bill1.B0_MasterBillNumber = "130851952006";
			bill1.B0_IssuerCode = "CPRS";
			var moveHeader = inbond.MovementHeader;
			var moveDetail1 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			moveDetail1.B9_SeqNo = "1";
			Factory.Save();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			response.EM_Status = EDIMessage.Status.Queued;
			var processor = new NotificationOfStatusProcessor();
			processor.Message = response;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"3096CPRS130851952006                                0000000624 1303280841       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var reLoadMoveDetail = Factory.Load<CusInBondMoveDetail>(moveDetail1.PK);
			var mscLog = reLoadMoveDetail.Logs.MostRecentLogByEventTime(Events.MessageStatusChange);
			AssertNotNull(mscLog);
			Assert(mscLog.SL_Reference.Contains("96"));
			mock.Protected().Verify("GetNumberFountainNumbersAndFillInPlaceHolders", Times.Never());
		}

		public void TestIControllerIDProviderMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			IControllerIDProvider provider = moveDetail;
			AssertEquals("ControllerID", ControllerIDs.Customs.US.InBond, provider.ControllerID);
			AssertEquals("BusinessObjectPK", header.PK.ToGuid(), provider.BusinessObjectPK);
			var shipment = Factory.New<ForwardingShipment>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			AssertEquals("ControllerID", ControllerIDs.JobShipment, provider.ControllerID);
			AssertEquals("BusinessObjectPK", shipment.PK.ToGuid(), provider.BusinessObjectPK);
		}

		[TestDate(2015, 1, 21, 21, 52, 0)]
		public void TestConcurrency()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			Factory.RefreshEnabled = false;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var moveDetailInDiffFactory = newFactory.Load<CusInBondMoveDetail>(moveDetail.PK);
			moveDetailInDiffFactory.B9_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			moveDetailInDiffFactory.B9_ExportDate = new ZDateTime(2015, 1, 24);
			newFactory.Save();
			AssertEquals(ZString.Empty, moveDetail.B9_CustomsStatus);
			AssertEquals(ZDateTime.Empty, moveDetail.B9_ExportDate);
			moveDetail.B9_ExportLadenOn = "HELLO";
			var handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}

			AssertEquals(ZString.Empty, moveDetail.B9_CustomsStatus);
			AssertEquals(ZDateTime.Empty, moveDetail.B9_ExportDate);
			AssertEquals("HELLO", moveDetail.B9_ExportLadenOn);
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			var lastEdited = ZDataUtils.GetUsernameAndTimeOfLastModification(((INeedRow)moveDetail).Row, false);
			var lastEditedInMessage = string.IsNullOrWhiteSpace(lastEdited) ? string.Empty : $" ({lastEdited})";
			var expectedMessage = $@"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
Move Detail {lastEditedInMessage}
	Export Date
	Customs Status (Critical change)";
			AssertMultilineASCIIEquals("ReportInformationMessage", expectedMessage, handler.ReportInformationMessage);
			moveDetail.Delete();
			handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}

			AssertEquals(true, moveDetail.IsDeleted);
			AssertEquals(true, moveDetail.HasChanges);
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			var deletedLastEdited = ZDataUtils.GetUsernameAndTimeOfLastModification(((INeedRow)moveDetail).Row, false);
			var deletedLastEditedInMessage = string.IsNullOrWhiteSpace(deletedLastEdited) ? string.Empty : $" ({deletedLastEdited})";
			var deletedExpectedMessage = $@"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
Move Detail {deletedLastEditedInMessage} (pending delete)
	Export Date
	Customs Status (Critical change)";
			AssertMultilineASCIIEquals("ReportInformationMessage", deletedExpectedMessage, handler.ReportInformationMessage);
		}

		public void TestShouldSynchronise()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			AssertEquals(false, moveDetail.ShouldSynchronise);
			header.BH_ParentID = shipment.PK;
			AssertEquals(true, moveDetail.ShouldSynchronise);
			header.BH_OverrideFreightDefaults = ZBool.True;
			AssertEquals(false, moveDetail.ShouldSynchronise);
			header.BH_OverrideFreightDefaults = ZBool.False;
			AssertEquals(true, moveDetail.ShouldSynchronise);
			var message = Factory.New<MQEDIMessage>();
			header.MovementHeader.Messages.Add(message);
			AssertEquals(false, moveDetail.ShouldSynchronise);
		}

		public override void TestLookups()
		{
			base.TestLookups();
			AssertEquals(typeof(CusInBondMoveDetailLookups), MoveDetail.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusInBondMoveDetailValidation), MoveDetail.Validation.GetType());
		}

		public void TestUpdateQty()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			bill1.B0_ManifestQty = 100;
			CusInBondBill bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MB2";
			bill2.B0_ManifestQty = 150;
			CusInBondMoveHeader moveHeader1 = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail1 = moveHeader1.MovementDetails.AddNew();
			AssertEquals(0, moveDetail1.B9_InBoundQty);
			moveDetail1.B9_B0 = bill1.PK;
			AssertEquals(100, moveDetail1.B9_InBoundQty);
			moveDetail1.B9_B0 = bill2.PK;
			AssertEquals(150, moveDetail1.B9_InBoundQty);
			moveDetail1.B9_InBoundQty = 70;
			CusInBondMoveHeader moveHeader2 = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail2 = moveHeader2.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill1.PK;
			AssertEquals(70, moveDetail1.B9_InBoundQty);
			AssertEquals(100, moveDetail2.B9_InBoundQty);
			moveDetail2.B9_B0 = bill2.PK;
			AssertEquals(70, moveDetail1.B9_InBoundQty);
			AssertEquals(80, moveDetail2.B9_InBoundQty);
			CusInBondMoveHeader moveHeader3 = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail3 = moveHeader3.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill1.PK;
			AssertEquals(70, moveDetail1.B9_InBoundQty);
			AssertEquals(80, moveDetail2.B9_InBoundQty);
			AssertEquals(100, moveDetail3.B9_InBoundQty);
			moveDetail3.B9_B0 = bill2.PK;
			AssertEquals(70, moveDetail1.B9_InBoundQty);
			AssertEquals(80, moveDetail2.B9_InBoundQty);
			AssertEquals(0, moveDetail3.B9_InBoundQty);
		}

		public void TestIInBondBillDetailsMembers()
		{
			IInBondBillDetails billDetails = MoveDetail;
			AssertEquals(Bill.Consignee, billDetails.ConsigneeAddress);
			Bill.B0_PortOfLadingKCode = "41231";
			AssertEquals("41231", billDetails.ForeignLadingPortLocalCode);
			AssertEquals(Bill.ForeignShipper, billDetails.ForeignShipperAddress);
			AssertEquals(ZDecimal.Zero, billDetails.GoodsValueInLocalCurrency);
			MoveDetail.B9_InBoundQty = 350;
			AssertEquals(350, billDetails.InBondQuantity);
			AssertEquals(false, billDetails.IsDetailedInBond);
			Header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			AssertEquals(true, billDetails.IsDetailedInBond);
			CusInBondContainer container1 = MoveDetail.Containers.AddNew();
			CusInBondContainer container2 = MoveDetail.Containers.AddNew();
			List<IInBondLineDetailsHeader> lineDetailsHeaders = new List<IInBondLineDetailsHeader>(billDetails.LineDetailsHeaders);
			AssertEquals(2, lineDetailsHeaders.Count);
			AssertEquals(container1, lineDetailsHeaders[0]);
			AssertEquals(container2, lineDetailsHeaders[1]);
			Bill.B0_ManifestQty = 630;
			AssertEquals(630, billDetails.ManifestQuantity);
			Bill.B0_ManifestUQ = "PC";
			AssertEquals("PC", billDetails.ManifestUQ);
			Bill.B0_IssuerCode = "APCD";
			AssertEquals("APCD", billDetails.MasterBillIssuerSCAC);
			Bill.B0_MasterBillNumber = "HB22342344";
			AssertEquals("HB22342344", billDetails.MasterBillNumber);
			AssertEquals(Bill.NotifyParty, billDetails.NotifyPartyAddress);
			Bill.B0_PlaceOfReceiptDCode = "2705";
			AssertEquals("2705", billDetails.PlaceOfPreReceipt);
			MoveDetail.B9_PreviousITNumber = "86594327";
			AssertEquals("86594327", billDetails.PreviousITNumber);
			CusInbondBillAddRef ref1 = Bill.AdditionalReferences.AddNew();
			CusInbondBillAddRef ref2 = Bill.AdditionalReferences.AddNew();
			List<IInBondBillReferenceNumber> refNumbers = new List<IInBondBillReferenceNumber>(billDetails.RefNumbers);
			AssertEquals(2, refNumbers.Count);
			AssertEquals(ref1, refNumbers[0]);
			AssertEquals(ref2, refNumbers[1]);
			billDetails.SequenceNumber = "2";
			AssertEquals("2", billDetails.SequenceNumber);
			Bill.B0_Volume = 6935;
			Bill.B0_VolumeUQ = Core.Constants.Volume.CubicCentimeters;
			AssertEquals(6935m, billDetails.VolumeInWholeNumber);
			AssertEquals(Core.Constants.Volume.CubicCentimeters, billDetails.VolumeUQ);
			Bill.B0_Weight = 7854;
			Bill.B0_WeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals(Core.Constants.Weight.Pounds, billDetails.WeightUQ);
			Bill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(Core.Constants.Weight.Kilograms, billDetails.WeightUQ);
			Bill.B0_WeightUQ = "Z!";
			AssertEquals("Z!", billDetails.WeightUQ);
			Bill.B0_WeightUQ = Core.Constants.Weight.Kilotonnes;
			AssertEquals(Core.Constants.Weight.Kilograms, billDetails.WeightUQ);
			AssertEquals(7854000000m, billDetails.WeightInWholeNumber);
			Bill.B0_Weight = 999999999;
			AssertEquals(ZDecimal.Zero, billDetails.WeightInWholeNumber);
			Bill.B0_WeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals(999999999m, billDetails.WeightInWholeNumber);
			Bill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(999999999m, billDetails.WeightInWholeNumber);
			Bill.B0_WeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(1000000m, billDetails.WeightInWholeNumber);
			Header.BH_FTZMove = ZBool.True;
			Header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			Bill.B0_MasterBillNumber = "XJ512345678";
			AssertEquals("12345678", billDetails.MasterBillNumber);
			Bill.B0_MasterBillNumber = "XJ5-123456789";
			Header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselNonContainer;
			AssertEquals("XJ5123456789", billDetails.MasterBillNumber);
			Header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			MoveDetail.B9_InBoundQty = 350;
			AssertEquals(0, billDetails.InBondQuantity);
			Bill.B0_MasterBillNumber = "XJ512345678";
			AssertEquals("XJ5", billDetails.MasterBillIssuerSCAC);
			Bill.B0_HouseBillNumber = "HB000141";
			AssertEquals("HB000141", billDetails.HouseBillNumber);
			Header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.RailContainer;
			AssertEquals(ZString.Empty, billDetails.HouseBillNumber);
		}

		public void TestIInBondQXBillDetailsMember()
		{
			IInBondQXBillDetails billDetails = MoveDetail;
			Bill.B0_MasterBillNumber = "01234567890";
			AssertEquals("01234567890", billDetails.MasterBillNumber);
			Bill.B0_HouseBillNumber = "012345678901";
			AssertEquals("012345678901", billDetails.HouseBillNumber);
			MoveDetail.B9_PreviousITNumber = "01234567890";
			AssertEquals("01234567890", billDetails.PreviousInBondNumber);
		}

		public void TestSecondaryNotifyParties()
		{
			MoveDetail.B9_FirstSecondaryNotifyParty = "SCA1";
			SecondaryNotifyParty snp1 = MoveDetail.FirstSecondaryNotifyParty;
			AssertEquals("SCA1", snp1.CY_Data);
			AssertEquals(SecondaryNotifyPartyCodeList.Codes.First, snp1.CY_Code);
			snp1.CY_Data = "SCA1234567890";
			AssertEquals("SCA123456", MoveDetail.B9_FirstSecondaryNotifyParty);
			MoveDetail.B9_FirstSecondaryNotifyParty = ZString.Empty;
			AssertEquals(ZString.Empty, snp1.CY_Data);
			AssertEquals(false, snp1.IsDeleted);
			MoveDetail.B9_SecondSecondaryNotifyParty = "SCA2";
			SecondaryNotifyParty snp2 = MoveDetail.SecondSecondaryNotifyParty;
			AssertEquals("SCA2", snp2.CY_Data);
			AssertEquals(SecondaryNotifyPartyCodeList.Codes.Second, snp2.CY_Code);
			snp2.CY_Data = "SCA2234567890";
			AssertEquals("SCA223456", MoveDetail.B9_SecondSecondaryNotifyParty);
			MoveDetail.B9_SecondSecondaryNotifyParty = ZString.Empty;
			AssertEquals(ZString.Empty, snp2.CY_Data);
			AssertEquals(false, snp2.IsDeleted);
			MoveDetail.B9_ThirdSecondaryNotifyParty = "SCA3";
			SecondaryNotifyParty snp3 = MoveDetail.ThirdSecondaryNotifyParty;
			AssertEquals("SCA3", snp3.CY_Data);
			AssertEquals(SecondaryNotifyPartyCodeList.Codes.Third, snp3.CY_Code);
			snp3.CY_Data = "SCA3234567890";
			AssertEquals("SCA323456", MoveDetail.B9_ThirdSecondaryNotifyParty);
			MoveDetail.B9_ThirdSecondaryNotifyParty = ZString.Empty;
			AssertEquals(ZString.Empty, snp3.CY_Data);
			AssertEquals(false, snp3.IsDeleted);
			MoveDetail.B9_FourthSecondaryNotifyParty = "SCA4";
			SecondaryNotifyParty snp4 = MoveDetail.FourthSecondaryNotifyParty;
			AssertEquals("SCA4", snp4.CY_Data);
			AssertEquals(SecondaryNotifyPartyCodeList.Codes.Fourth, snp4.CY_Code);
			snp4.CY_Data = "SCA4234567890";
			AssertEquals("SCA423456", MoveDetail.B9_FourthSecondaryNotifyParty);
			MoveDetail.B9_FourthSecondaryNotifyParty = ZString.Empty;
			AssertEquals(ZString.Empty, snp4.CY_Data);
			AssertEquals(false, snp4.IsDeleted);
			Factory.Save();
			AssertEquals(true, snp1.IsDeleted);
			AssertEquals(true, snp2.IsDeleted);
			AssertEquals(true, snp3.IsDeleted);
			AssertEquals(true, snp4.IsDeleted);
			MoveDetail.B9_FirstSecondaryNotifyParty = "SCA1";
			AssertNotEquals(snp1, MoveDetail.FirstSecondaryNotifyParty);
			AssertEquals("SCA1", MoveDetail.FirstSecondaryNotifyParty.CY_Data);
			MoveDetail.B9_SecondSecondaryNotifyParty = "SCA2";
			AssertNotEquals(snp2, MoveDetail.SecondSecondaryNotifyParty);
			AssertEquals("SCA2", MoveDetail.SecondSecondaryNotifyParty.CY_Data);
			MoveDetail.B9_ThirdSecondaryNotifyParty = "SCA3";
			AssertNotEquals(snp3, MoveDetail.ThirdSecondaryNotifyParty);
			AssertEquals("SCA3", MoveDetail.ThirdSecondaryNotifyParty.CY_Data);
			MoveDetail.B9_FourthSecondaryNotifyParty = "SCA4";
			AssertNotEquals(snp4, MoveDetail.FourthSecondaryNotifyParty);
			AssertEquals("SCA4", MoveDetail.FourthSecondaryNotifyParty.CY_Data);
		}

		public void TestDoNotAllowBillChangesIfMessagingHasBeenDone()
		{
			AssertEquals(false, MoveDetail.B9_B0Info.ReadOnly);
			MoveDetail.B9_SeqNo = "1";
			AssertEquals(true, MoveDetail.B9_B0Info.ReadOnly);
		}

		public void TestIMsgAttacheeWithDispositions()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			bill1.B0_ManifestQty = 100;
			CusInBondMoveHeader moveHeader1 = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			IIMessageAttacheeWithDisposition dispositionData = moveDetail1;
			dispositionData.UpdateDispositionInformation("06", ZDateTime.Today.AddHours(-5));
			Factory.Save();
			AssertEquals(1, moveDetail1.DispositionCodes.Count);
			AssertEquals("06", moveDetail1.DispositionCodes[0].US_Code);
			AssertEquals(ZDateTime.Today.AddHours(-5), moveDetail1.DispositionCodes[0].US_DispositionDate);
		}

		public void TestDispositionsAddedToGenAddOnColumn()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			bill1.B0_ManifestQty = 100;
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			Factory.Save();
			IIMessageAttacheeWithDisposition dispositionData = moveDetail1;
			dispositionData.UpdateDispositionInformation("06", ZDateTime.Today.AddHours(-5));
			Factory.Save();
			AssertEquals(1, moveDetail1.DispositionCodes.Count);
			AssertEquals("06", moveDetail1.DispositionCodes[0].US_Code);
			AssertEquals(previousCount + 1, Factory.GetDatabaseCount(typeof(GenAddOnColumn), dispositionDataFilter));
		}

		public void TestPreviousDispositionsAreDeletedFromGenAddOnColumn()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			bill1.B0_ManifestQty = 100;
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			var data1 = moveDetail1.DispositionCodes.AddNewIfNotExist("XX", ZDateTime.BrettsBirthday);
			var data2 = moveDetail1.DispositionCodes.AddNewIfNotExist("1K", ZDateTime.BrettsBirthday);
			data2.B7_ParentID = moveDetail1.PK;
			data2.B7_ParentTableCode = moveDetail1.TablePrefix;
			var data3 = moveDetail1.DispositionCodes.AddNewIfNotExist("62", ZDateTime.BrettsBirthday);
			data3.B7_ParentID = moveDetail1.PK;
			data3.B7_ParentTableCode = moveDetail1.TablePrefix;
			var data4 = moveDetail1.DispositionCodes.AddNewIfNotExist("ZZ", ZDateTime.BrettsBirthday);
			var data5 = moveDetail1.DispositionCodes.AddNewIfNotExist("24", ZDateTime.BrettsBirthday);
			data5.B7_ParentID = moveDetail1.PK;
			data5.B7_ParentTableCode = moveDetail1.TablePrefix;
			Factory.Save();
			AssertEquals("Pre-condition - 5 test rows established", previousCount + 5, Factory.GetDatabaseCount(typeof(GenAddOnColumn), dispositionDataFilter));
			IIMessageAttacheeWithDisposition dispositionData = moveDetail1;
			dispositionData.MarkPreviousDispositionsInactive(ZDateTime.Today);
			dispositionData.UpdateDispositionInformation("06", ZDateTime.Today);
			Factory.Save();
			AssertEquals("Should now only be 1 disposition row in GenAddOnColumn: 1 new disposition replacing 5 deleted dispositions for this movement", previousCount + 1, Factory.GetDatabaseCount(typeof(GenAddOnColumn), dispositionDataFilter));
		}

		public void TestDispositionsSuperseededEndToEnd()
		{
			processor = new NotificationOfStatusProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.Message = message;
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "001821004";
			bill1.B0_ManifestQty = 100;
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "999999996";
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"1061999999996   390100000                                                       ",
				"3054APLU001821004                                   0000000000 0612041631APLU   ",
				"4061568741095      2704                                                         ",
				"50INBOND DELETE                                                                 ",
				"60 APZU4145927   4936655                                                        ",
				"60 NOSU2405592   4936009                                                        ");
			processor.SetBAndYBlock(new AABIOutputB()
			{ FilerCode = "XJ5" }, new AABIOutputY()
			{ FilerCode = "XJ5" });
			processor.Process();
			Factory.Save();
			AssertEquals(1, moveDetail1.DispositionCodes.Count);
			AssertEquals("54", moveDetail1.DispositionCodes[0].US_Code);
			AssertEquals("First disposition status", previousCount + 1, Factory.GetDatabaseCount(typeof(GenAddOnColumn), dispositionDataFilter));
			var filterDispositionQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "US_Code");
			var currentGenAddOnValue = Factory.LoadTop1<GenAddOnColumn>(filterDispositionQuery);
			AssertEquals("Current Disposition status stored in GenAddOnColumn", "54", currentGenAddOnValue.XA_Data);
			processor = new NotificationOfStatusProcessor();
			processor.Message = message;
			processor.SetBAndYBlock(new AABIOutputB()
			{ FilerCode = "XJ5" }, new AABIOutputY()
			{ FilerCode = "XJ5" });
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"1061999999996   390100000                                                       ",
				"30BGAPLU001821004                                   0000002234 1008021944APLU   ",
				"4000               2833                                                         ",
				"50FTZ ADMISSION ADVISORY                                                        ",
				"50ADMISSION NUMBER: 12600031000000525                                           ",
				"60 TTNU5288182   APA6018263                                                     ");
			processor.Process();
			Factory.Save();
			AssertEquals(2, moveDetail1.DispositionCodes.Count);
			AssertEquals("BG", moveDetail1.DispositionCodes[1].US_Code);
			AssertEquals("Second disposition status: previous GenAddOnColumn record should have been deleted, so should still be only 1 GenAddOnColumn record", previousCount + 1, Factory.GetDatabaseCount(typeof(GenAddOnColumn), dispositionDataFilter));
			currentGenAddOnValue = Factory.LoadTop1<GenAddOnColumn>(filterDispositionQuery);
			AssertEquals("Current Disposition status stored in GenAddOnColumn", "BG", currentGenAddOnValue.XA_Data);
		}

		public void TestAttachStatusMessageByMasterBill()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "INB001";
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "999999996";
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "130851952006";
			bill1.B0_IssuerCode = "CPRS";
			bill1.B0_ManifestQty = 100;
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			var processor = new NotificationOfStatusProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			processor.Message = message;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"3096CPRS130851952006                                0000000624 1303280841       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Status Notification Response for INB001 / 999999996", email.Subject);
			AssertEquals(@"NS message shoud be attached to Move Header found by SCAC + Master Bill Number, 
				because no InBond number or Entry Number provided by Customs", moveHeader1.PK, message.EM_LinkUniqueID);
		}

		public void TestAttachStatusMessageByMasterBillWhenMoreThanOneMovementHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "999999996";
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "000001010";
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "130851952006";
			bill1.B0_IssuerCode = "CPRS";
			bill1.B0_ManifestQty = 100;
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			var moveDetail2 = moveHeader2.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill1.PK;
			var processor = new NotificationOfStatusProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			processor.Message = message;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"05CPRS0241273375739213087130873801130326000000                                  ",
				"3096CPRS130851952006                                0000000624 1303280841       ",
				"4000               3801                                                         ",
				"50TRAIN CONSIST AT 3801                                                         ",
				"50CPRS0241273375739213087                                                       ",
				"60 EMHU00660766  0790930                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Status Notification Response for Unknown/CPRS130851952006", email.Subject);
			AssertEquals(@"NS message cannot be attached to object, becuase no InBond number or Entry Number provided by Customs.
InBond header cannot be found by SCAC + Master Bill Number, because more than one Movement Header assosiated with Master Bill provided by Customs.", ZGuid.Empty, message.EM_LinkUniqueID);
		}

		public void TestAttachStatusMessageByMasterBillWhenSecondJobIsInactive_CS00217346()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "INB0000002";
			header.BH_CarrierSCAC = "EGLV";
			header.BH_PortUnladingDCode = "2704";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.InBondNumber = "693000140";
			var moveHeaderPK = moveHeader.PK;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "020300060810";
			bill1.B0_IssuerCode = "EGLV";
			bill1.B0_ManifestQty = 50;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill1.PK;
			moveDetail.B9_InBoundQty = 50;
			var query = new ZQuery(CusEntryNumSchema.CE_EntryNum, "693000140");
			var inbondNum = Factory.Load<CusEntryNumber>(query);
			AssertEquals("Precondition: CusEntryNum for moveHeader exists", 1, inbondNum.Length);
			AssertEquals("Precondition: CusEntryNum for moveHeader exists", inbondNum[0].CE_ParentID, moveHeader.PK);
			var declaration = Factory.New<JobDeclaration>();
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_JobReference = "BMPK00015990";
			header2.BH_CarrierSCAC = "EGLV";
			header2.BH_PortUnladingDCode = "2704";
			header2.BH_ParentID = declaration.PK;
			header2.BH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			var moveHeader2 = header2.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "693000140";
			var bill2 = header2.Bills.AddNew();
			bill2.B0_MasterBillNumber = "020300060810";
			bill2.B0_IssuerCode = "EGLV";
			bill2.B0_ManifestQty = 50;
			var moveDetail2 = moveHeader2.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill2.PK;
			moveDetail2.B9_InBoundQty = 50;
			inbondNum = Factory.Load<CusEntryNumber>(query);
			AssertEquals("Precondition: CusEntryNum for moveHeader exists", 2, inbondNum.Length);
			header.BH_IsActive = false;
			header.MovementHeader.Delete();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedMoveHeader = newFactory.Load<CusInBondMoveHeader>(moveHeaderPK);
			AssertNull(reloadedMoveHeader);
			inbondNum = Factory.Load<CusEntryNumber>(query);
			AssertEquals("Only one CusEntryNum should exists - MoveHeader was deleted with his own inBond Number", 1, inbondNum.Length);
			var processor = new NotificationOfStatusProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			processor.Message = message;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"1061693000140   260800000                                                       ",
				"3019EGLV020300060810                                0000000050 1304181143ODFL   ",
				"4061693000140      2709                                                         ",
				"50ACTUAL ARRIVAL AT 2709 130417                                                 ",
				"60 EMCU6097560   EMCFGW0681                                                     ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Status Notification Response for BMPK00015990 / 693000140", email.Subject);
			AssertEquals(@"NS message should be attached to active job", moveHeader2.PK, message.EM_LinkUniqueID);
		}

		/// <summary>
		/// Re-process an old disposition message.
		/// Ensure current dispostion status is not superseeded by old disposition re-processed.
		/// </summary>
		public void TestMarkingPreviousDispositionsInactive()
		{
			processor = new NotificationOfStatusProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.Message = message;
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "J304895890";
			bill1.B0_ManifestQty = 100;
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "005449721";
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"1062005449721   230455976                                                       ",
				"301JJTWRJ304895890                                  0000000100 1103071956JTWR   ",
				"4062005449721      2704                                                         ",
				"50MARDEL COAST TRUCKING (MCT), INC                                              ",
				"60 MOLU0038948   SJ093984X                                                      ");
			processor.SetBAndYBlock(new AABIOutputB()
			{ FilerCode = "XJ5" }, new AABIOutputY()
			{ FilerCode = "XJ5" });
			processor.Process();
			Factory.Save();
			AssertEquals(1, moveDetail1.DispositionCodes.Count);
			AssertEquals("1J", moveDetail1.DispositionCodes[0].US_Code);
			AssertEquals("First disposition status", previousCount + 1, Factory.GetDatabaseCount(typeof(GenAddOnColumn), dispositionDataFilter));
			var filterDispositionQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "US_Code");
			var currentGenAddOnValue = Factory.LoadTop1<GenAddOnColumn>(filterDispositionQuery);
			AssertEquals("Current Disposition status stored in GenAddOnColumn", "1J", currentGenAddOnValue.XA_Data);
			processor = new NotificationOfStatusProcessor();
			processor.Message = message;
			processor.SetBAndYBlock(new AABIOutputB()
			{ FilerCode = "XJ5" }, new AABIOutputY()
			{ FilerCode = "XJ5" });
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"1062005449721   230455976                                                       ",
				"306HJTWRJ304895890                                  0000000100 1103072118JTWR   ",
				"4000               2709                                                         ",
				"50DO NOT LOAD. INADEQUATE CARGO                                                 ",
				"50DESCRIPTION. AMMEND AND RESUBM                                                ",
				"60 MOLU0038948   SJ093984X                                                      ");
			processor.Process();
			Factory.Save();
			AssertEquals(2, moveDetail1.DispositionCodes.Count);
			AssertEquals("6H", moveDetail1.DispositionCodes[1].US_Code);
			AssertEquals("Second disposition status: previous GenAddOnColumn record should have been deleted, so should still be only 1 GenAddOnColumn record", previousCount + 1, Factory.GetDatabaseCount(typeof(GenAddOnColumn), dispositionDataFilter));
			currentGenAddOnValue = Factory.LoadTop1<GenAddOnColumn>(filterDispositionQuery);
			AssertEquals("Current Disposition status stored in GenAddOnColumn", "6H", currentGenAddOnValue.XA_Data);
			processor = new NotificationOfStatusProcessor();
			processor.Message = message;
			processor.SetBAndYBlock(new AABIOutputB()
			{ FilerCode = "XJ5" }, new AABIOutputY()
			{ FilerCode = "XJ5" });
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"1062005449721   230455976                                                       ",
				"301KJTWRJ304895890                                  0000000000 1104010030JTWR   ",
				"4000005449721      9900                                                         ",
				"60 MOLU0038948   SJ093984X                                                      ");
			processor.Process();
			Factory.Save();
			AssertEquals(3, moveDetail1.DispositionCodes.Count);
			AssertEquals("1K", moveDetail1.DispositionCodes[2].US_Code);
			AssertEquals("Third disposition status: previous GenAddOnColumn record should have been deleted, so should still be only 1 GenAddOnColumn record", previousCount + 1, Factory.GetDatabaseCount(typeof(GenAddOnColumn), dispositionDataFilter));
			currentGenAddOnValue = Factory.LoadTop1<GenAddOnColumn>(filterDispositionQuery);
			AssertEquals("Current Disposition status stored in GenAddOnColumn", "1K", currentGenAddOnValue.XA_Data);
			//Now re-process the first message again...
			processor = new NotificationOfStatusProcessor();
			processor.Message = message;
			processor.SetBAndYBlock(new AABIOutputB()
			{ FilerCode = "XJ5" }, new AABIOutputY()
			{ FilerCode = "XJ5" });
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"1062005449721   230455976                                                       ",
				"301JJTWRJ304895890                                  0000000100 1103071956JTWR   ",
				"4062005449721      2704                                                         ",
				"50MARDEL COAST TRUCKING (MCT), INC                                              ",
				"60 MOLU0038948   SJ093984X                                                      ");
			processor.SetBAndYBlock(new AABIOutputB()
			{ FilerCode = "XJ5" }, new AABIOutputY()
			{ FilerCode = "XJ5" });
			processor.Process();
			Factory.Save();
			AssertEquals("Should still be only 3 dispositions", 3, moveDetail1.DispositionCodes.Count);
			AssertEquals("Re-process of first disposition status should not affect GenAddOnColumn record, so should still be only 1 GenAddOnColumn record", previousCount + 1, Factory.GetDatabaseCount(typeof(GenAddOnColumn), dispositionDataFilter));
			currentGenAddOnValue = Factory.LoadTop1<GenAddOnColumn>(filterDispositionQuery);
			AssertEquals("Current Disposition status stored in GenAddOnColumn should not have been affected by reprocessing of first status (1J)", "1K", currentGenAddOnValue.XA_Data);
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			AssertEquals(ZString.Empty, moveDetail.B9_FirstSecondaryNotifyParty);
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			USCustomsDataRegistry.Instance.BRecordOfficeCode.SetValue(header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12");
			header = Factory.New<CusInBondHeader>();
			bill = header.Bills.AddNew();
			header.MovementHeaders.AddNew();
			moveDetail = header.MovementHeaders[0].MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			AssertEquals("8888XJ512", moveDetail.B9_FirstSecondaryNotifyParty);
		}

		public void TestCanChangeMasterBillWhenWithdrawn()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "INB0000002";
			header.BH_CarrierSCAC = "EGLV";
			header.BH_PortUnladingDCode = "2704";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.InBondNumber = "693000140";
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "020300060810";
			bill1.B0_IssuerCode = "EGLV";
			bill1.B0_ManifestQty = 50;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill1.PK;
			moveDetail.B9_InBoundQty = 50;
			Factory.Save();
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			moveDetail.B9_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			Factory.Save();
			AssertEquals("moveHeader.LogManager.HasAClearLog", true, moveHeader.LogManager.HasAClearLog);
			AssertEquals("moveHeader.LogManager.HasAWithdrawnLog", false, moveHeader.LogManager.HasAWithdrawnLog);
			AssertEquals("moveDetail.ActiveInMessaging", true, moveDetail.ActiveInMessaging);
			AssertEquals("moveDetail.LogManager.HasAClearLog", true, moveDetail.LogManager.HasAClearLog);
			AssertEquals("moveDetail.LogManager.HasAWithdrawnLog", false, moveDetail.LogManager.HasAWithdrawnLog);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			Factory.Save();
			AssertEquals("moveHeader.LogManager.HasAClearLog", true, moveHeader.LogManager.HasAClearLog);
			AssertEquals("moveHeader.LogManager.HasAWithdrawnLog", false, moveHeader.LogManager.HasAWithdrawnLog);
			AssertEquals("moveDetail.ActiveInMessaging", true, moveDetail.ActiveInMessaging);
			AssertEquals("moveDetail.LogManager.HasAClearLog", true, moveDetail.LogManager.HasAClearLog);
			AssertEquals("moveDetail.LogManager.HasAWithdrawnLog", false, moveDetail.LogManager.HasAWithdrawnLog);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			Factory.Save();
			AssertEquals("moveHeader.LogManager.HasAClearLog", false, moveHeader.LogManager.HasAClearLog);
			AssertEquals("moveHeader.LogManager.HasAWithdrawnLog", true, moveHeader.LogManager.HasAWithdrawnLog);
			AssertEquals("moveDetail.ActiveInMessaging", false, moveDetail.ActiveInMessaging);
			AssertEquals("moveDetail.LogManager.HasAClearLog", false, moveDetail.LogManager.HasAClearLog);
			AssertEquals("moveDetail.LogManager.HasAWithdrawnLog", true, moveDetail.LogManager.HasAWithdrawnLog);
		}

		public void TestDeleteMoveDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MBTEST1";
			var package0 = declaration.Packages.AddNew();
			package0.CW_HouseBill = masterBill.CU_BillUniqueCode;
			var header0 = Factory.New<CusInBondHeader>();
			header0.BH_ParentID = declaration.PK;
			header0.BH_ParentTableCode = declaration.TablePrefix;
			var synchronizer = (CusInBondHeaderDeclarationSynchronizer)header0.Synchroniser;
			synchronizer.Synchronise(true);
			AssertEquals(1, header0.Bills.Count);
			AssertEquals(1, header0.Bills[0].MoveDetails.Count);
			var moveDetail0 = header0.Bills[0].MoveDetails[0];
			Assert("Movement Detail can't be deleted.", !moveDetail0.CanDelete);
			AssertEquals(string.Format("This Movement Detail cannot be deleted {0}", ValidationConstants.Synchronize.SynchronizedFromParent(JobDeclarationSchema.Constants.TableName)), moveDetail0.ReasonForNotAbleToDelete);
			Factory.Save();
			header0.BH_OverrideFreightDefaults = true;
			var moveHeader = header0.MovementHeader;
			var moveHeaderMessage = Factory.New<MQEDIMessage>();
			moveHeader.Messages.Add(moveHeaderMessage);
			moveHeaderMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Assert("moveDetail3 can be deleted.", moveDetail0.CanDelete);
			AssertEquals(@"", moveDetail0.ReasonForNotAbleToDelete);
			moveDetail0.B9_SeqNo = "1";
			Assert("Movement Detail can't be deleted.", !moveDetail0.CanDelete);
			AssertEquals(@"This Movement Detail cannot be deleted as it has been submitted to Customs.
Please send an In-Bond Delete message first before deleting this Movement Detail.", moveDetail0.ReasonForNotAbleToDelete);
			var header1 = Factory.New<CusInBondHeader>();
			var bill = header1.Bills.AddNew();
			var moveDetail1 = bill.MoveDetails.AddNew();
			var moveDetail2 = bill.MoveDetails.AddNew();
			Assert("Movement Detail can be deleted.", moveDetail1.CanDelete);
			Assert("Movement Detail can be deleted.", moveDetail2.CanDelete);
		}

		public void TestLinkInBondHeaderByNS10()
		{
			processor = new NotificationOfStatusProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.Message = message;
			var header = Factory.New<CusInBondHeader>();
			header.BH_PostDepartureOnly = true;
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "APLU";
			bill1.B0_MasterBillNumber = "001821004";
			bill1.B0_ManifestQty = 100;
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "000000012";
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"1061999999996   390100000                                                       ",
				"3054APLU001821004                                   0000000000 0612041631APLU   ",
				"4061568741095      2704                                                         ",
				"50INBOND DELETE                                                                 ",
				"60 APZU4145927   4936655                                                        ",
				"60 NOSU2405592   4936009                                                        ");
			processor.SetBAndYBlock(new AABIOutputB()
			{ FilerCode = "XJ5" }, new AABIOutputY()
			{ FilerCode = "XJ5" });
			processor.Process();
			Factory.Save();
			AssertEquals(@"If exist NS10 block, the message should be linked by NS10 block rather than NS30 block.", ZGuid.Empty, message.EM_LinkUniqueID);
			processor = new NotificationOfStatusProcessor();
			processor.Message = message;
			processor.SetBAndYBlock(new AABIOutputB()
			{ FilerCode = "XJ5" }, new AABIOutputY()
			{ FilerCode = "XJ5" });
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
				"3054APLU001821004                                   0000000000 0612041631APLU   ",
				"4061568741095      2704                                                         ",
				"50INBOND DELETE                                                                 ",
				"60 APZU4145927   4936655                                                        ",
				"60 NOSU2405592   4936009                                                        ");
			processor.Process();
			Factory.Save();
			AssertEquals(@"If not exist NS10 block, the message should be linked by NS30 block.", moveHeader1.PK, message.EM_LinkUniqueID);
		}

		NotificationOfStatusProcessor processor;
		MQEDIMessage message;
		protected override void SetUp()
		{
			base.SetUp();
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			var staffZ2 = groupZZ1.Staff.AddNew();
			staffZ2.GS_Code = "Z2";
			staffZ2.GS_LoginName = "z2";
			staffZ2.GS_EmailAddress = "dong2@pretend.email.com";
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "Z~";
			var abiMessageStaff = group.Staff.AddNew();
			abiMessageStaff.GS_Code = "ZAC";
			abiMessageStaff.GS_LoginName = "~2";
			abiMessageStaff.GS_EmailAddress = "postmaster@pretendemail.com";
			Factory.Save();
			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new ManifestGroupNotification(Enterprise.Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK, false));

			dispositionDataFilter = new ZQuery();
			dispositionDataFilter.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusAddInfoSchema.Constants.Prefix);
			dispositionDataFilter.AddToFilter(GenAddOnColumnSchema.XA_Name, DispositionData.Schema.US_Code);

			previousCount = Factory.GetDatabaseCount(typeof(GenAddOnColumn), dispositionDataFilter);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetMovementDetails(factory);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetMovementDetails(Factory);

		protected override US.Business.CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header) => ((CusInBondHeader)header).MovementHeaders.AddNew();

		protected override Customs.Business.CusInBondHeader CreateNewCusInBondHeader() => Factory.New<CusInBondHeader>();

		protected override Customs.Business.CusInBondBill CreateNewCusInBondBill(Customs.Business.CusInBondHeader header) => ((CusInBondHeader)header).Bills.AddNew();

		protected override CusInBondMoveDetail CreateNewCusInBondMoveDetail(US.Business.CusInBondMoveHeader moveHeader) => ((CusInBondMoveHeader)moveHeader).MovementDetails.AddNew();

		CusInBondHeader Header => (CusInBondHeader)header;

		CusInBondBill Bill => (CusInBondBill)bill;

		CusInBondMoveHeader MoveHeader => (CusInBondMoveHeader)moveHeader;

		CusInBondMoveDetail MoveDetail => (CusInBondMoveDetail)moveDetail;

		ZQuery dispositionDataFilter;

		int previousCount;

		CusInBondMoveDetail GetMovementDetails(BusinessObjectFactory factory)
		{
			header = factory.New<CusInBondHeader>();
			bill = Header.Bills.AddNew();
			moveHeader = Header.MovementHeaders.AddNew();
			var moveDetails = MoveHeader.MovementDetails.AddNew(Bill.PK);
			moveDetails.B9_FirstSecondaryNotifyParty = "!!";
			return moveDetails;
		}

		void AddMessageBlocksToProcessor(ACEABIProcessor processor, string applicationReference, params string[] messageBlocks)
		{
			var block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(applicationReference, messageBlocks);
			foreach (MessageBlock messageBlock in block.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}
		}
	}
}
