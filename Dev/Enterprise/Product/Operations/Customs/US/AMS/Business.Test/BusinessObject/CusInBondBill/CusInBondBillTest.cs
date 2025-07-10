using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondBill))]
	class CusInBondBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMessages()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill1 = header.Bills.AddNew();

			using (header.SuspendMarkingAsNeedingValidation())
			using (bill1.SuspendMarkingAsNeedingValidation())
			{
				var action = new MessageSendingAction(header, ActionCode.Creating);
				AssertEquals(1, action.CreateAMSMessages());
				var bill2 = header.Bills.AddNew();
				using (bill2.SuspendMarkingAsNeedingValidation())
				{
					action = new MessageSendingAction(header, ActionCode.Creating);
					AssertEquals(2, action.CreateAMSMessages());
					action = new MessageSendingAction(header, ActionCode.AmendingAdd);
					AssertEquals(2, action.CreateAMSMessages());
				}
				AssertEquals(5, moveHeader.Messages.Count);
				var bill1Messages = bill1.Messages;
				AssertType<AMSBillEDIMessageCollection>(bill1Messages);
				AssertEquals(bill1Messages.Count, 3);
				Assert(bill1Messages.Cast<AMSEDIMessage>().All(m => m.EM_ApplicationReference == bill1.PK.ToString()));

				var bill2Messages = bill2.Messages;
				AssertType<AMSBillEDIMessageCollection>(bill2Messages);
				AssertEquals(bill2Messages.Count, 2);
				Assert(bill2Messages.Cast<AMSEDIMessage>().All(m => m.EM_ApplicationReference == bill2.PK.ToString()));
				bill2.Messages.Reload(true);
				AssertEquals(bill2.Messages.Count, 2);
			}
		}

		public void TestGetMessagesFromDifferentMoveHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var pttMoveHeader = header.PTTMovements.AddNew();
			var inbondMoveHeader = header.InBondMovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			var message1 = Factory.New<AMSEDIMessage>();
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.USAMS;
			message1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message1.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			message1.EM_LinkUniqueID = moveHeader.PK;
			message1.EM_ApplicationReference = bill.PK.ToString();

			var message2 = Factory.New<AMSEDIMessage>();
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.USAMS;
			message2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message2.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			message2.EM_LinkUniqueID = pttMoveHeader.PK;
			message2.EM_ApplicationReference = bill.PK.ToString();

			var message3 = Factory.New<AMSEDIMessage>();
			message3.EM_ApplicationCode = ApplicationCodeList.Codes.USAMS;
			message3.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message3.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			message3.EM_LinkUniqueID = inbondMoveHeader.PK;
			message3.EM_ApplicationReference = bill.PK.ToString();
			AssertEquals(3, bill.Messages.Count);
			AssertCollectionContains(message1, bill.Messages);
			AssertCollectionContains(message2, bill.Messages);
			AssertCollectionContains(message3, bill.Messages);
		}

		public void TestICusInbondBillAddRefTypeSupporter()
		{
			var supporter = bill as ICusInbondBillAddRefTypeSupporter;
			AssertEquals(typeof(CusInbondBillAddRef), supporter.AddRefType);
		}

		public void TestOceanBillManifestQtyIsTheSumOfAllBillsManifestQty()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = Enterprise.Customs.US.AMS.Business.DirectionTypeList.Codes.NVOCC;
			var oceanBill = header.OceanBill;
			AssertEquals(ZInt.Zero, oceanBill.B0_ManifestQty);
			var bill1 = header.Bills.AddNew();
			bill1.B0_ManifestQty = 10;
			AssertEquals(10, oceanBill.B0_ManifestQty);
			var bill12 = header.Bills.AddNew();
			bill12.B0_ManifestQty = 33;
			AssertEquals(43, oceanBill.B0_ManifestQty);
		}

		public void TestIsNVOCCHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("IsNVOCCHeader", false, bill.IsNVOCCHeader);
			header.BH_TransitDirection = Enterprise.Customs.US.AMS.Business.DirectionTypeList.Codes.NVOCC;
			AssertEquals("IsNVOCCHeader", true, bill.IsNVOCCHeader);
		}

		public void TestHasDifferentDischargePort()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_PortUnladingDCode = "0012";
			var bill1 = header.Bills.AddNew();
			bill1.B0_InBondPortOfDestDCode = "0012";
			AssertEquals(false, bill1.HasDifferentDischargePort);
			bill1.B0_InBondPortOfDestDCode = "0013";
			AssertEquals(true, bill1.HasDifferentDischargePort);
		}

		public void TestInBondStatus()
		{
			AssertInBondStatus(false);
			AssertInBondStatus(true);
		}

		void AssertInBondStatus(bool isNVOCCHeader)
		{
			var header = Factory.New<CusInBondHeader>();
			if (isNVOCCHeader)
			{
				header.BH_TransitDirection = Enterprise.Customs.US.AMS.Business.DirectionTypeList.Codes.NVOCC;
			}
			var bill = header.Bills.AddNew();
			AssertEquals("B0_InBondStatus", ZString.Empty, bill.B0_InBondStatus);
			AssertEquals("B0_InBondStatusDescription", ZString.Empty, bill.B0_InBondStatusDescription);
			AssertEquals("B0_InBondMessageStatus", ZString.Empty, bill.B0_InBondMessageStatus);
			AssertEquals("B0_InBondMessageStatusDescription", ZString.Empty, bill.B0_InBondMessageStatusDescription);
			var inbond1 = header.InBondMovementHeaders.AddNew();
			var moveDetail1 = isNVOCCHeader ? inbond1.OceanBillNVOCCMoveDetail : inbond1.MovementDetails.AddNew(bill.PK);
			AssertEquals("B0_InBondStatus", ZString.Empty, bill.B0_InBondStatus);
			AssertEquals("B0_InBondStatusDescription", ZString.Empty, bill.B0_InBondStatusDescription);
			AssertEquals("B0_InBondMessageStatus", ZString.Empty, bill.B0_InBondMessageStatus);
			AssertEquals("B0_InBondMessageStatusDescription", ZString.Empty, bill.B0_InBondMessageStatusDescription);
			moveDetail1.B9_CustomsStatus = AMSBillMessageStatusList.Codes.ClearArrival;
			moveDetail1.B9_MessageStatus = AMSBillMessageStatusList.Codes.AwaitingExportation;
			AssertEquals("B0_InBondStatus", AMSBillMessageStatusList.Codes.ClearArrival, bill.B0_InBondStatus);
			AssertEquals("B0_InBondStatusDescription", AMSBillMessageStatusList.Descriptions.ClearArrival, bill.B0_InBondStatusDescription);
			AssertEquals("B0_InBondMessageStatus", AMSBillMessageStatusList.Codes.AwaitingExportation, bill.B0_InBondMessageStatus);
			AssertEquals("B0_InBondMessageStatusDescription", AMSBillMessageStatusList.Descriptions.AwaitingExportation, bill.B0_InBondMessageStatusDescription);
			moveDetail1.B9_CustomsStatus = "Z#";
			moveDetail1.B9_MessageStatus = "Y#";
			AssertEquals("B0_InBondStatus", "Z#", bill.B0_InBondStatus);
			AssertEquals("B0_InBondStatusDescription", ZString.Empty, bill.B0_InBondStatusDescription);
			AssertEquals("B0_InBondMessageStatus", "Y#", bill.B0_InBondMessageStatus);
			AssertEquals("B0_InBondMessageStatusDescription", ZString.Empty, bill.B0_InBondMessageStatusDescription);
			moveDetail1.B9_CustomsStatus = AMSBillMessageStatusList.Codes.ClearExportation;
			moveDetail1.B9_MessageStatus = AMSBillMessageStatusList.Codes.ClearTransferOfLiability;
			AssertEquals("B0_InBondStatus", AMSBillMessageStatusList.Codes.ClearExportation, bill.B0_InBondStatus);
			AssertEquals("B0_InBondStatusDescription", AMSBillMessageStatusList.Descriptions.ClearExportation, bill.B0_InBondStatusDescription);
			AssertEquals("B0_InBondMessageStatus", AMSBillMessageStatusList.Codes.ClearTransferOfLiability, bill.B0_InBondMessageStatus);
			AssertEquals("B0_InBondMessageStatusDescription", AMSBillMessageStatusList.Descriptions.ClearTransferOfLiability, bill.B0_InBondMessageStatusDescription);
			var inbond2 = header.InBondMovementHeaders.AddNew();
			var moveDetail2 = inbond2.MovementDetails.AddNew(bill.PK);
			AssertEquals("B0_InBondStatus", AMSBillMessageStatusList.Codes.Multiple, bill.B0_InBondStatus);
			AssertEquals("B0_InBondStatusDescription", AMSBillMessageStatusList.Descriptions.Multiple, bill.B0_InBondStatusDescription);
			AssertEquals("B0_InBondMessageStatus", AMSBillMessageStatusList.Codes.Multiple, bill.B0_InBondMessageStatus);
			AssertEquals("B0_InBondMessageStatusDescription", AMSBillMessageStatusList.Descriptions.Multiple, bill.B0_InBondMessageStatusDescription);
		}

		public void TestLatestPTTDisposition()
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
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1W", "Within port transfer authorized:  Bill of lading remains open", startDate, endDate);
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "83", "PTT cancelled", startDate, endDate);
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var code1 = bill.DispositionCodes.AddNew();
			code1.US_DispositionDate = new ZDateTime(2012, 10, 1);
			var code2 = bill.DispositionCodes.AddNew();
			code2.US_DispositionDate = new ZDateTime(2012, 11, 1);
			var code3 = bill.DispositionCodes.AddNew();
			code3.US_DispositionDate = new ZDateTime(2012, 12, 1);
			code3.US_Code = "!!";
			var list = DispositionCodeListLoader.GetCachedPTTCodesForAMS(Factory);
			for (var i = 0; i < list.Count; i++)
			{
				code1.US_Code = ((ICodeDescription)list[i == 0 ? list.Count - 1 : 0]).Code;
				var pair = ((ICodeDescription)list[i]);
				code2.US_Code = pair.Code;
				AssertEquals("B0_LatestPTTDispositionCode", pair.Code, bill.B0_LatestPTTDispositionCode);
				AssertEquals("B0_LatestPTTDispositionCodeDescription", pair.Description, bill.B0_LatestPTTDispositionCodeDescription);
				var disposition = bill.LatestPTTDisposition;
				AssertEquals("US_DispositionDate", new ZDateTime(2012, 11, 1), disposition.US_DispositionDate);
			}
		}

		public void TestLatestISFDisposition()
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
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3U", "Importer Security Filing Removed", startDate, endDate);
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3Z", "Importer Security Filing on File", startDate, endDate);
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var code1 = bill.DispositionCodes.AddNew();
			code1.US_DispositionDate = new ZDateTime(2012, 10, 1);
			var code2 = bill.DispositionCodes.AddNew();
			code2.US_DispositionDate = new ZDateTime(2012, 11, 1);
			var code3 = bill.DispositionCodes.AddNew();
			code3.US_DispositionDate = new ZDateTime(2012, 12, 1);
			code3.US_Code = "!!";
			var list = DispositionCodeListLoader.GetCachedISFCodesForAMS(Factory);
			for (var i = 0; i < list.Count; i++)
			{
				code1.US_Code = ((ICodeDescription)list[i == 0 ? list.Count - 1 : 0]).Code;
				var pair = ((ICodeDescription)list[i]);
				code2.US_Code = pair.Code;
				AssertEquals("B0_LatestISFDispositionCode", pair.Code, bill.B0_LatestISFDispositionCode);
				AssertEquals("B0_LatestISFDispositionCodeDescription", pair.Description, bill.B0_LatestISFDispositionCodeDescription);
				var disposition = bill.LatestISFDisposition;
				AssertEquals("US_DispositionDate", new ZDateTime(2012, 11, 1), disposition.US_DispositionDate);
			}
		}

		public void TestIsOceanBillType()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("IsOceanBillType", false, bill.IsOceanBillType);
			bill.B0_ShipmentType = CusInBondBill.OceanBillType;
			AssertEquals("IsOceanBillType", true, bill.IsOceanBillType);
		}

		public void TestDeleteOcaenBillMoveDetailIfExists()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = Enterprise.Customs.US.AMS.Business.DirectionTypeList.Codes.NVOCC;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			AssertNotNull(moveDetail);
			AssertEquals("moveDetail.IsDeleted", false, moveDetail.IsDeleted);
			bill.B0_ShipmentType = CusInBondBill.OceanBillType;
			AssertEquals("moveDetail.IsDeleted", true, moveDetail.IsDeleted);
			AssertNull("MovementDetail should not be created for OceanBillType", bill.MovementDetail);
		}

		public void TestICanDeleteMembers()
		{
			AssertEquals(true, ((ICanDelete)bill).CanDelete);
			var consol = Factory.New<ForwardingConsol>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			AssertEquals(false, ((ICanDelete)bill).CanDelete);
			AssertEquals(CusInBondBill.ReasonForCannotDeleteStillBeingSynchronise, ((ICanDelete)bill).ReasonForNotAbleToDelete);
			header.BH_OverrideFreightDefaults = true;
			AssertEquals(true, ((ICanDelete)bill).CanDelete);
			bill.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			AssertEquals(false, ((ICanDelete)bill).CanDelete);
			AssertEquals(CusInBondBill.ReasonForCannotDeleteAlreadyOnFile, ((ICanDelete)bill).ReasonForNotAbleToDelete);
			header.BH_OverrideFreightDefaults = false;
			bill = header.Bills.AddNew();
			bill.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			AssertEquals(false, ((ICanDelete)bill).CanDelete);
			AssertEquals(CusInBondBill.ReasonForCannotDeleteAlreadyOnFile, ((ICanDelete)bill).ReasonForNotAbleToDelete);
		}

		public void TestIsInventoryRecordValidationMode()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			AssertEquals(ValidationModes.InventoryRecord, bill.ValidationModes);
			AssertEquals(true, bill.IsInventoryRecordValidationMode);
			header.ValidationModes = ValidationModes.None;
			AssertEquals(ValidationModes.None, bill.ValidationModes);
			AssertEquals(false, bill.IsInventoryRecordValidationMode);
			bill.ValidationModes = ValidationModes.InventoryRecord;
			AssertEquals(ValidationModes.None, header.ValidationModes);
			AssertEquals(true, bill.IsInventoryRecordValidationMode);
		}

		public void TestReadOnlyFields()
		{
			var consol = Factory.New<ForwardingConsol>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			AssertEquals(true, bill.ShouldSynchronise);
			AssertEquals(true, bill.B0_PortOfLadingKCodeInfo.ReadOnly);
			AssertEquals(true, bill.B0_LastForeignPortKCodeInfo.ReadOnly);
			header.BH_OverrideFreightDefaults = true;
			AssertEquals(false, bill.ShouldSynchronise);
			AssertEquals(false, bill.B0_PortOfLadingKCodeInfo.ReadOnly);
			AssertEquals(false, bill.B0_LastForeignPortKCodeInfo.ReadOnly);
		}

		public void TestB0_MasterBillNumberInfoHumanReadableName()
		{
			AssertEquals("Bill Of Lading", bill.B0_MasterBillNumberInfo.HumanReadableName);
		}

		public void TestIssuerCodeAndMasterBillNumber()
		{
			bill.B0_IssuerCode = "SC34";
			bill.B0_MasterBillNumber = "MB23432342";
			AssertEquals("SC34 MB23432342", bill.IssuerCodeAndMasterBillNumber);
			bill.B0_MasterBillNumber = "";
			AssertEquals("SC34", bill.IssuerCodeAndMasterBillNumber);
			bill.B0_IssuerCode = "";
			bill.B0_MasterBillNumber = "MB23432342";
			AssertEquals("MB23432342", bill.IssuerCodeAndMasterBillNumber);
		}

		public void TestHumanReadableName()
		{
			bill.B0_IssuerCode = "SC34";
			bill.B0_MasterBillNumber = "MB23432342";
			AssertEquals("AMS Bill SC34 MB23432342", bill.HumanReadableName);
			bill.B0_MasterBillNumber = "";
			AssertEquals("AMS Bill SC34", bill.HumanReadableName);
			bill.B0_IssuerCode = "";
			bill.B0_MasterBillNumber = "MB23432342";
			AssertEquals("AMS Bill MB23432342", bill.HumanReadableName);
		}

		public void TestPortDefaulting()
		{
			bill.B0_RL_NKPortOfLading = "AUSYD";
			AssertEquals("60267", bill.B0_PortOfLadingKCode);
			AssertEquals("AUSYD", bill.B0_RL_NKLastForeignPort);
			AssertEquals("60267", bill.B0_LastForeignPortKCode);
			AssertEquals("AUSYD", bill.B0_RL_NKForeignPortOfContract);
			AssertEquals("60267", bill.B0_ForeignPortOfContractKCode);
			bill.B0_PortOfLadingKCode = "60268";
			AssertEquals("60268", bill.B0_PortOfLadingKCode);
			AssertEquals("AUSYD", bill.B0_RL_NKLastForeignPort);
			AssertEquals("60268", bill.B0_LastForeignPortKCode);
			AssertEquals("AUSYD", bill.B0_RL_NKForeignPortOfContract);
			AssertEquals("60268", bill.B0_ForeignPortOfContractKCode);
			bill.B0_RL_NKLastForeignPort = "AUMEL";
			bill.B0_RL_NKForeignPortOfContract = "AUBNE";
			bill.B0_PortOfLadingKCode = "60269";
			AssertEquals("60269", bill.B0_PortOfLadingKCode);
			AssertEquals("AUMEL", bill.B0_RL_NKLastForeignPort);
			AssertEquals("60237", bill.B0_LastForeignPortKCode);
			AssertEquals("AUBNE", bill.B0_RL_NKForeignPortOfContract);
			AssertEquals("60210", bill.B0_ForeignPortOfContractKCode);
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CusInBondBillLookups), bill.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusInBondBillValidation), bill.Validation.GetType());
			bill.ValidationModes = ValidationModes.PermitToTransfer;
			Assert(bill.IsPTTValidationMode);
			bill.ValidationModes = ValidationModes.InventoryRecord;
			Assert(!bill.IsPTTValidationMode);
			Assert(!bill.IsChangeEstDateOfArrivalValidationMode);
			Assert(!bill.IsVesselDepartureValidationMode);
			bill.ValidationModes = ValidationModes.ChangeEstDateOfArrival;
			Assert(bill.IsChangeEstDateOfArrivalValidationMode);
			bill.ValidationModes = ValidationModes.VesselDeparture;
			Assert(bill.IsVesselDepartureValidationMode);
			bill.B0_ShipmentType = CusInBondBill.OceanBillType;
			AssertEquals(typeof(OceanBillCusInBondBillValidation), bill.Validation.GetType());
		}

		public void TestDispositionsAddedToGenAddOnColumn()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			bill1.B0_ManifestQty = 100;
			Factory.Save();
			bill.DispositionCodes.AddNewIfNotExist("06", ZDateTime.Today.AddHours(-5));
			Factory.Save();
			AssertEquals(1, bill.DispositionCodes.Count);
			AssertEquals("06", bill.DispositionCodes[0].US_Code);
			AssertEquals(1, Factory.GetDatabaseCount(typeof(GenAddOnColumn)));
		}

		public void TestMasterInBondMovement()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var subInbond = header.InBondMovementHeaders.AddNew();
			subInbond.BM_SubApplicationCode = SubApplicationCodeList.Codes.SubsequentInBond;
			subInbond.BM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMinutes(1);
			var subInbondDetail = subInbond.MovementDetails.AddNew(bill.PK);
			var inbond1 = header.InBondMovementHeaders.AddNew();
			inbond1.BM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMinutes(2);
			var inbondDetail = inbond1.MovementDetails.AddNew(bill.PK);
			var inbond2 = header.InBondMovementHeaders.AddNew();
			inbond2.BM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMinutes(3);
			AssertEquals(false, bill.B0_MasterInBondIndicator);
			AssertNull("Bill should not have a MasterInBondMovement since it's not flag as MasterInBond", bill.MasterInBondMovement);
			bill.B0_MasterInBondIndicator = true;
			AssertEquals(inbond1, bill.MasterInBondMovement);
			inbondDetail.B9_BM = inbond2.PK;
			AssertEquals(inbond2, bill.MasterInBondMovement);
			var inbondDetail2 = inbond1.MovementDetails.AddNew(bill.PK);
			AssertEquals(inbond1, bill.MasterInBondMovement);
		}

		public void TestDispositionCodeDescriptionList()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(true, Object.ReferenceEquals(DispositionCodeListLoader.GetDispositionCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode), bill.DispositionCodeDescriptionList));
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.HouseBill, bill.B0_BillStatus);
		}

		public void TestB0_BillStatusWhenQRSTTariffRequired()
		{
			var header = Factory.New<CusInBondHeader>();
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF;
			AssertEquals("Tariff Required", true, bill.IsTariffRequired);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF;
			AssertEquals("Tariff Required", true, bill.IsTariffRequired);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF;
			AssertEquals("Tariff Required", true, bill.IsTariffRequired);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond;
			AssertEquals("Tariff Required", true, bill.IsTariffRequired);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.InternationalMailDirectDischargeAtMailFacility;
			AssertEquals("Tariff is not equired", false, bill.IsTariffRequired);
		}

		public void TestDefaultingEstUnload()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.B0_InBondPortOfDestDCode = "1111";
			bill1.B0_DateOfDischarge = new ZDate(2015, 3, 11);
			var bill2 = header.Bills.AddNew();
			bill2.B0_InBondPortOfDestDCode = "1111";
			AssertEquals("B0_DateOfDischarge should be set the default value", new ZDate(2015, 3, 11), bill2.B0_DateOfDischarge);
			var bill3 = header.Bills.AddNew();
			bill3.B0_DateOfDischarge = new ZDate(2015, 3, 12);
			bill3.B0_InBondPortOfDestDCode = "1111";
			AssertEquals("B0_DateOfDischarge should not be set the default value", new ZDate(2015, 3, 12), bill3.B0_DateOfDischarge);
		}

		public void TestGetEffectiveValue_B0_InBondPortOfDestDCode()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_PortUnladingDCode = "1111";
			var bill = header.Bills.AddNew();
			AssertEquals("1111", bill.B0_InBondPortOfDestDCode);
			bill.B0_InBondPortOfDestDCode = "2222";
			AssertEquals("2222", bill.B0_InBondPortOfDestDCode);
			header.BH_PortUnladingDCode = "2222";
			AssertEquals("2222", bill.B0_InBondPortOfDestDCode);
			header.BH_PortUnladingDCode = "3333";
			AssertEquals("3333", bill.B0_InBondPortOfDestDCode);
		}

		public void TestGetEffectiveValue_B0_RL_NKInBondPortOfDest()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_RL_NKPortUnlading = "AAAAA";
			var bill = header.Bills.AddNew();
			AssertEquals("AAAAA", bill.B0_RL_NKInBondPortOfDest);
			bill.B0_RL_NKInBondPortOfDest = "BBBBB";
			AssertEquals("BBBBB", bill.B0_RL_NKInBondPortOfDest);
			header.BH_RL_NKPortUnlading = "BBBBB";
			AssertEquals("BBBBB", bill.B0_RL_NKInBondPortOfDest);
			header.BH_RL_NKPortUnlading = "CCCCC";
			AssertEquals("CCCCC", bill.B0_RL_NKInBondPortOfDest);
		}

		public void TestGetEffectiveValue_B0_DateOfDischarge()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ETA = new ZDate(2015, 3, 11);
			var bill = header.Bills.AddNew();
			AssertEquals(new ZDate(2015, 3, 11), bill.B0_DateOfDischarge);
			bill.B0_DateOfDischarge = new ZDate(2015, 3, 12);
			AssertEquals(new ZDate(2015, 3, 12), bill.B0_DateOfDischarge);
			header.BH_ETA = new ZDate(2015, 3, 12);
			AssertEquals(new ZDate(2015, 3, 12), bill.B0_DateOfDischarge);
			header.BH_ETA = new ZDate(2015, 3, 13);
			AssertEquals(new ZDate(2015, 3, 13), bill.B0_DateOfDischarge);
		}

		[TestDate(2016, 7, 29)]
		public void TestB0_A_ARVClearedWhenPortChanged()
		{
			var header = Factory.New<CusInBondHeader>();
			header.PortArrivalDetails.AddNewIfNotExist("1101", ZDateTime.Today);
			header.PortArrivalDetails.AddNewIfNotExist("1103", ZDateTime.Today.AddDays(1));
			header.PortArrivalDetails.AddNewIfNotExist("1104", ZDateTime.Today.AddDays(2));
			var bill = header.Bills.AddNew();
			bill.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			bill.B0_InBondPortOfDestDCode = "1101";
			AssertEquals("1101", bill.B0_InBondPortOfDestDCode);
			AssertEquals(ZDateTime.Today, bill.B0_A_ARV);
			bill.B0_InBondPortOfDestDCode = "1102";
			AssertEquals("1102", bill.B0_InBondPortOfDestDCode);
			AssertEquals(ZDateTime.Empty, bill.B0_A_ARV);
			bill.OnPortOfLadingChangedEvent += delegate(ZString oldPort, ZString newPort, ZDateTime arrivalTime)
			{
				return false;
			};
			bill.B0_InBondPortOfDestDCode = "1103";
			AssertEquals("1102", bill.B0_InBondPortOfDestDCode);
			AssertEquals(ZDateTime.Empty, bill.B0_A_ARV);
			bill.OnPortOfLadingChangedEvent = null;
			bill.OnPortOfLadingChangedEvent += delegate(ZString oldPort, ZString newPort, ZDateTime arrivalTime)
			{
				return true;
			};
			bill.B0_InBondPortOfDestDCode = "1104";
			AssertEquals("1104", bill.B0_InBondPortOfDestDCode);
			AssertEquals(ZDateTime.Today.AddDays(2), bill.B0_A_ARV);
		}

		public void TestGetActualArrivalDateFromLatestVesselMessage()
		{
			bill.B0_IssuerCode = "OTT1";
			bill.B0_MasterBillNumber = "MST061516A";
			bill.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			bill.B0_InBondPortOfDestDCode = "1001";
			AssertEquals(ZDateTime.Empty, bill.B0_A_ARV);
			var originalMessage0 = header.MovementHeader.Messages.AddNew();
			originalMessage0.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival;
			originalMessage0.EM_MessageSubType = AMSMessageSubTypeList.Codes.VesselArrival;
			originalMessage0.EM_MessageNum = "EDIEDIDAT1";
			originalMessage0.EM_MessageText = "ACR          HI                                                                 M01OTT111USTITANIC                06156     000001                              M02MST061516A_EDIEDIDAT1                                                        P011001062516                                                                   H014              1606241001    0000                                            ZCR                               00000";
			originalMessage0.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage0.EM_MessageOwner = Constants.ACE;
			originalMessage0.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			var responseMessage0 = header.MovementHeader.Messages.AddNew();
			responseMessage0.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse;
			responseMessage0.EM_MessageSubType = AMSMessageSubTypeList.Codes.VesselArrival;
			responseMessage0.EM_MessageNum = "EDIEDIDAT1";
			responseMessage0.EM_MessageText = "ACR          HR16061512134253414                                                M01OTT111USTITANIC                06156     000001                              M02MST061516A_EDIEDIDAT1                                                        P011001062516                                                                   W02OTT11606151213410100100000000000000000001000000000100004                     ZCR          HR                   00004";
			responseMessage0.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage0.EM_MessageOwner = Constants.ACE;
			responseMessage0.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-8);
			header.PortArrivalDetails.InitialisePortArrival();
			AssertEquals(1, header.PortArrivalDetails.Count);
			AssertEquals("Vessel arrival message is accepted", new ZDateTime(2016, 6, 24), bill.B0_A_ARV);
			var originalMessage1 = header.MovementHeader.Messages.AddNew();
			originalMessage1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival;
			originalMessage1.EM_MessageSubType = AMSMessageSubTypeList.Codes.VesselArrival;
			originalMessage1.EM_MessageNum = "EDIEDIDAT2";
			originalMessage1.EM_MessageText = "ACR          HI                                                                 M01OTT111USTITANIC                06156     000001                              M02MST061516A_EDIEDIDAT2                                                        P011001062516                                                                   H014              1606281001    1202                                            ZCR                               00000";
			originalMessage1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage1.EM_MessageOwner = Constants.ACE;
			originalMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-5);
			var responseMessage1 = header.MovementHeader.Messages.AddNew();
			responseMessage1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse;
			responseMessage1.EM_MessageSubType = AMSMessageSubTypeList.Codes.VesselArrival;
			responseMessage1.EM_MessageNum = "EDIEDIDAT2";
			responseMessage1.EM_MessageText = "ACR          HR16061512134253414                                                M01OTT111USTITANIC                06156     000001                              M02MST061516A_EDIEDIDAT2                                                        P011001062516                                                                   W02OTT11606151213410100100000000000000000001000000000100004                     ZCR          HR                   00004";
			responseMessage1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage1.EM_MessageOwner = Constants.ACE;
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-4);
			header.PortArrivalDetails.InitialisePortArrival();
			AssertEquals(1, header.PortArrivalDetails.Count);
			bill.B0_InBondPortOfDestDCode = "1101";
			AssertEquals(ZDateTime.Empty, bill.B0_A_ARV);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var billReload = newFactory.Load<CusInBondBill>(bill.PK);
			billReload.B0_InBondPortOfDestDCode = "1001";
			AssertEquals("New vessel arrival message is accepted", new ZDateTime(2016, 6, 28, 12, 2, 0), billReload.B0_A_ARV);
			var originalMessage2 = header.MovementHeader.Messages.AddNew();
			originalMessage2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival;
			originalMessage2.EM_MessageSubType = AMSMessageSubTypeList.Codes.VesselArrival;
			originalMessage2.EM_MessageNum = "EDIEDIDAT3";
			originalMessage2.EM_MessageText = "ACR          HI                                                                 M01OTT111USTITANIC                06156     000001                              M02MST061516A_EDIEDIDAT3                                                        P011001062516                                                                   H014              1606281001    1202                                            ZCR                               00000";
			originalMessage2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage2.EM_MessageOwner = Constants.ACE;
			originalMessage2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			var responseMessage2 = header.MovementHeader.Messages.AddNew();
			responseMessage2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse;
			responseMessage2.EM_MessageSubType = AMSMessageSubTypeList.Codes.VesselArrival;
			responseMessage2.EM_MessageNum = "EDIEDIDAT3";
			responseMessage2.EM_MessageText = "ACR          HR16061512134253414                                                M01OTT111USTITANIC                06156     000001                              M02MST061516A_EDIEDIDAT3                                                        P011001062516                                                                   W01                          1101000001143 DDPP NOT ON MANIFEST                 W02OTT11606151213410100100000000000000000001000000000100004                     ZCR          HR                   00004";
			responseMessage2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage2.EM_MessageOwner = Constants.ACE;
			responseMessage2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			header.PortArrivalDetails.InitialisePortArrival();
			AssertEquals(0, header.PortArrivalDetails.Count);
			bill.B0_InBondPortOfDestDCode = "1101";
			AssertEquals(ZDateTime.Empty, bill.B0_A_ARV);
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			billReload = newFactory.Load<CusInBondBill>(bill.PK);
			billReload.B0_InBondPortOfDestDCode = "1001";
			AssertEquals("New vessel arrival message is rejected", ZDateTime.Empty, bill.B0_A_ARV);
		}

		public void TestIWorkflowTriggerEventSource()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var bill = (IWorkflowTriggerEventSource)header.Bills.AddNew();
			AssertEquals("JobHeaderCompany", header.Company, bill.JobHeaderCompany);
			AssertEquals("ParentWorkflowProviders", header, bill.ParentWorkflowProviders[0]);
		}

		public void TestMasterAndHouseBillNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = Enterprise.Customs.US.AMS.Business.DirectionTypeList.Codes.NVOCC;
			header.OceanBill.B0_MasterBillNumber = "OCU111111";
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "HSB1111111";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "HSB2222222";
			AssertEquals("OCU111111", header.OceanBill.MasterBillNumber);
			AssertEquals(ZString.Empty, header.OceanBill.HouseBillNumber);
			AssertEquals("OCU111111", bill1.MasterBillNumber);
			AssertEquals("HSB1111111", bill1.HouseBillNumber);
			AssertEquals("OCU111111", bill2.MasterBillNumber);
			AssertEquals("HSB2222222", bill2.HouseBillNumber);
		}

		CusInBondHeader header;
		CusInBondBill bill;

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = Enterprise.Customs.US.AMS.Business.DirectionTypeList.Codes.NVOCC;
			return header.Bills.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.NumberFountains.EDIFACTNumberFountain("M", GlbCompany.CurrentCompany.LicenceKeyIdentifier, CBPEDIInterchange.InterchangePartyIDs.AMSMailbox).SetNext(Factory, 1);
			header = Factory.New<CusInBondHeader>();
			bill = header.Bills.AddNew();
		}
	}
}
