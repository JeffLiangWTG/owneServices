using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using CodeDescriptionPairForTesting = Enterprise.Customs.DataTransfer.Universal.Testing.CodeDescriptionPairForTesting;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectWriterTest
	{
		void AssertInBondMoveDetailContentsForAir(InBondMoveDetail moveDetailData, ZString? sequenceNumber, ZInt? additionalBillLink, ZString? previousITNumber)
		{
			AssertInBondMoveDetailContents(moveDetailData, sequenceNumber, additionalBillLink, previousITNumber, null, null, null, null, CodeDescriptionPairForTesting.New(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Descriptions.AwaitingDepartureOriginal));
			AssertNull("moveDetailData.ContainerLinkCollection", moveDetailData.ContainerLinkCollection);
			AssertNull("moveDetailData.CustomsReferenceCollection", moveDetailData.CustomsReferenceCollection);
			AssertNotNull("moveDetailData.AddInfoGroupCollection", moveDetailData.AddInfoGroupCollection);
			var dispositionDataCollection = moveDetailData.AddInfoGroupCollection.Where(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USDisposition).ToArray();
			AssertEquals("dispositionDataCollection.Length", 3, dispositionDataCollection.Length);
			AssertInBondDisposition(dispositionDataCollection[0], DispositionList.Codes._1I, new ZDateTime(2014, 2, 6), 1);
			AssertInBondDisposition(dispositionDataCollection[1], DispositionList.Codes._1F, new ZDateTime(2014, 2, 5), 2);
			AssertInBondDisposition(dispositionDataCollection[2], DispositionList.Codes._22, new ZDateTime(2014, 2, 7), 3);
		}

		void AssertInBondMoveDetailContents(InBondMoveDetail moveDetailData, ZString? sequenceNumber, ZInt? additionalBillLink, ZString? previousITNumber, ContainerLink[] containerLinks,
			ZString? snp1 = null, ZString? snp2 = null, ZString? snp3 = null, ZString? snp4 = null)
		{
			AssertInBondMoveDetailContents(moveDetailData, sequenceNumber, additionalBillLink, previousITNumber, new ZDateTime(2014, 2, 4),
				CodeDescriptionPairForTesting.New(InbondCommonTypeList.Codes._2TransportandExport, InbondCommonTypeList.Descriptions._2TransportandExport),
				CodeDescriptionPairForTesting.New(SeaLocalPort3ScheduleD.ZZD_Code, SeaLocalPort3ScheduleD.ZZD_Description), 110,
				CodeDescriptionPairForTesting.New(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Descriptions.AwaitingDepartureOriginal));
			AssertNotNull("moveDetailData.ContainerLinkCollection", moveDetailData.ContainerLinkCollection);
			AssertEquals("moveDetailData.ContainerLinkCollection.Count", containerLinks.Length, moveDetailData.ContainerLinkCollection.Count);
			for (var i = 0; i < containerLinks.Length; i++)
			{
				AssertEquals(string.Format("moveDetailData.ContainerLinkCollection[{0}].Link", i), containerLinks[i].Link, moveDetailData.ContainerLinkCollection[i].Link);
				AssertEquals(string.Format("moveDetailData.ContainerLinkCollection[{0}].ContainerNumber", i), containerLinks[i].ContainerNumber, moveDetailData.ContainerLinkCollection[i].ContainerNumber);
			}

			AssertNotNull("moveDetailData.AddInfoGroupCollection", moveDetailData.AddInfoGroupCollection);
			var dispositionDataCollection = moveDetailData.AddInfoGroupCollection.Where(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USDisposition).ToArray();
			AssertEquals("dispositionDataCollection.Length", 3, dispositionDataCollection.Length);
			AssertInBondDisposition(dispositionDataCollection[0], DispositionList.Codes._1I, new ZDateTime(2014, 2, 6), 1);
			AssertInBondDisposition(dispositionDataCollection[1], DispositionList.Codes._1F, new ZDateTime(2014, 2, 5), 2);
			AssertInBondDisposition(dispositionDataCollection[2], DispositionList.Codes._22, new ZDateTime(2014, 2, 7), 3);
			AssertNotNull("moveDetailData.CustomsReferenceCollection", moveDetailData.CustomsReferenceCollection);
			if (snp1.HasValue)
			{
				AssertCustomsReferenceContents(moveDetailData.CustomsReferenceCollection.First(x => x.Type.GetCodeAsUpperCase() == Constants.SecondaryNotifyParty.Type && x.SubType.GetCodeAsUpperCase() == SecondaryNotifyPartyCodeList.Codes.First), Constants.SecondaryNotifyParty.Type, SecondaryNotifyPartyCodeList.Codes.First, snp1.Value);
			}

			if (snp2.HasValue)
			{
				AssertCustomsReferenceContents(moveDetailData.CustomsReferenceCollection.First(x => x.Type.GetCodeAsUpperCase() == Constants.SecondaryNotifyParty.Type && x.SubType.GetCodeAsUpperCase() == SecondaryNotifyPartyCodeList.Codes.Second), Constants.SecondaryNotifyParty.Type, SecondaryNotifyPartyCodeList.Codes.Second, snp2.Value);
			}

			if (snp3.HasValue)
			{
				AssertCustomsReferenceContents(moveDetailData.CustomsReferenceCollection.First(x => x.Type.GetCodeAsUpperCase() == Constants.SecondaryNotifyParty.Type && x.SubType.GetCodeAsUpperCase() == SecondaryNotifyPartyCodeList.Codes.Third), Constants.SecondaryNotifyParty.Type, SecondaryNotifyPartyCodeList.Codes.Third, snp3.Value);
			}

			if (snp4.HasValue)
			{
				AssertCustomsReferenceContents(moveDetailData.CustomsReferenceCollection.First(x => x.Type.GetCodeAsUpperCase() == Constants.SecondaryNotifyParty.Type && x.SubType.GetCodeAsUpperCase() == SecondaryNotifyPartyCodeList.Codes.Fourth), Constants.SecondaryNotifyParty.Type, SecondaryNotifyPartyCodeList.Codes.Fourth, snp4.Value);
			}
		}

		void AssertInBondMoveDetailContents(InBondMoveDetail moveDetailData, ZString? sequenceNumber, ZInt? additionalBillLink, ZString? previousITNumber, ZDateTime? previousITDate, ICodeDescription previousITType, ICodeDescription previousITPortDCode, ZInt? inBoundQty, ICodeDescription customsStatus)
		{
			AssertNotNull("Precondition: moveDetailData", moveDetailData);
			CombineAssertions(delegate
			{
				AssertEquals("moveDetailData.SequenceNumber", sequenceNumber, moveDetailData.SequenceNumber);
				AssertEquals("moveDetailData.AdditionalBillLink", additionalBillLink, moveDetailData.AdditionalBillLink);
				AssertEquals("moveDetailData.PreviousInBondTransitDate", previousITDate, moveDetailData.PreviousInBondTransitDate);
				if (previousITType == null)
				{
					AssertNull("moveDetailData.PreviousInBondTransitType", moveDetailData.PreviousInBondTransitType);
				}
				else
				{
					AssertNotNull("moveDetailData.PreviousInBondTransitType", moveDetailData.PreviousInBondTransitType);
					AssertEquals("moveDetailData.PreviousInBondTransitType.Code", previousITType.Code, moveDetailData.PreviousInBondTransitType.Code);
					AssertEquals("moveDetailData.PreviousInBondTransitType.Description", previousITType.Description, moveDetailData.PreviousInBondTransitType.Description);
				}

				if (previousITPortDCode == null)
				{
					AssertNull("moveDetailData.PreviousInBondTransitPortScheduleD", moveDetailData.PreviousInBondTransitPortScheduleD);
				}
				else
				{
					AssertNotNull("moveDetailData.PreviousInBondTransitPortScheduleD", moveDetailData.PreviousInBondTransitPortScheduleD);
					AssertEquals("moveDetailData.PreviousInBondTransitPortScheduleD.Code", previousITPortDCode.Code, moveDetailData.PreviousInBondTransitPortScheduleD.Code);
					AssertEquals("moveDetailData.PreviousInBondTransitPortScheduleD.Description", previousITPortDCode.Description, moveDetailData.PreviousInBondTransitPortScheduleD.Description);
				}

				AssertEquals("moveDetailData.InBondQuantity", inBoundQty, moveDetailData.InBondQuantity);
				if (customsStatus == null)
				{
					AssertNull("moveDetailData.CustomsStatus", moveDetailData.CustomsStatus);
				}
				else
				{
					AssertNotNull("moveDetailData.CustomsStatus", moveDetailData.CustomsStatus);
					AssertEquals("moveDetailData.CustomsStatus.Code", customsStatus.Code, moveDetailData.CustomsStatus.Code);
					AssertEquals("moveDetailData.CustomsStatus.Description", customsStatus.Description, moveDetailData.CustomsStatus.Description);
				}

				AssertNotNull("moveDetailData.CustomsStatus", moveDetailData.CustomsStatus);
				if (previousITNumber.HasValue)
				{
					AssertNotNull("moveDetailData.EntryNumberCollection", moveDetailData.EntryNumberCollection);
					AssertContainEntryNumber(moveDetailData.EntryNumberCollection, CodeDescriptionPairForTesting.New(Constants.MovementDetail.NumberTypes.PreviousInBondNumber, Constants.MovementDetail.NumberTypes.PreviousInBondNumberDescription), previousITNumber);
				}
			});
			AssertInBondMoveLineItemContents(moveDetailData);
			AssertInBondWarehouseDetailsContents(moveDetailData);
		}

		void AssertInBondMoveLineItemContents(InBondMoveDetail moveDetailData)
		{
			AssertNotNull("Precondition: moveDetailData", moveDetailData);
			var inbondMoveLineItemCollection = moveDetailData.InBondMoveLineItemCollection;
			AssertNotNull("Precondition: InBondMoveLineItemCollection", inbondMoveLineItemCollection);
			AssertEquals("moveDetailData.InBondMoveLineItemCollection", 1, inbondMoveLineItemCollection.Count);
			var inbondMoveLineItem = inbondMoveLineItemCollection[0];
			CombineAssertions(delegate
			{
				AssertEquals("inbondMoveLineItem.PrintingSequenceNo", (short)1, inbondMoveLineItem.PrintingSequenceNo);
				AssertEquals("inbondMoveLineItem.MarksAndNumbers", "MARKS AND NUMBERS", inbondMoveLineItem.MarksAndNumbers);
				AssertEquals("inbondMoveLineItem.Description", "DESCRIPTION", inbondMoveLineItem.DescriptionAndQuantityOfMerchandise);
				AssertEquals("inbondMoveLineItem.Weight", 10m, inbondMoveLineItem.Weight);
				AssertEquals("inbondMoveLineItem.WeightUnit", "KG", inbondMoveLineItem.WeightUnit.Code);
				AssertEquals("inbondMoveLineItem.MonetaryValue", 50m, inbondMoveLineItem.MonetaryValue);
				AssertEquals("inbondMoveLineItem.RateComment", "RATE COMMENT", inbondMoveLineItem.RateComment);
				AssertEquals("inbondMoveLineItem.DutyComment", "DUTY COMMENT", inbondMoveLineItem.DutyComment);
				AssertEquals("inbondMoveLineItem.IsMonetaryValueEstimated", true, inbondMoveLineItem.IsMonetaryValueEstimated);
			});
		}

		void AssertInBondWarehouseDetailsContents(InBondMoveDetail moveDetailData)
		{
			AssertNotNull("Precondition: moveDetailData", moveDetailData);
			var inbondWarehouseDetailsCollection = moveDetailData.WarehouseDetailCollection;
			AssertNotNull("Precondition: WareHouseDetailsCollection", inbondWarehouseDetailsCollection);
			AssertEquals("moveDetailData.WareHouseDetailsCollection", 1, inbondWarehouseDetailsCollection.Count);
			var inbondWarehouseDetails = inbondWarehouseDetailsCollection[0];
			CombineAssertions(delegate
			{
				AssertEquals("inbondMoveLineItem.WarehouseNumber", "1234", inbondWarehouseDetails.EntryNumber);
				AssertEquals("inbondMoveLineItem.WarehouseBondedQuantity", 2m, inbondWarehouseDetails.BondedQuantity);
				AssertEquals("inbondMoveLineItem.WarehouseWithdrawQuantity", 1m, inbondWarehouseDetails.WithdrawQuantity);
			});
		}

		CusInBondMoveDetail SetupCusInBondMoveDetail(CusInBondMoveDetail moveDetail, ZString previousITNumber, ZString? warehouseEntryNumber = null)
		{
			SetupCusInBondMoveDetail(moveDetail, previousITNumber, new ZDateTime(2014, 2, 4), InbondCommonTypeList.Codes._2TransportandExport, SeaLocalPort3ScheduleD.ZZD_Code, 110, ImportMessageStatusList.Codes.AwaitingDepartureOriginal, "SNP1", "SNP2", "SNP3", "SNP4");
			moveDetail.Containers.DeleteAll();
			var container1 = SetupCusInBondContainer(moveDetail.Containers.AddNew(), "CNT212", warehouseEntryNumber);
			var container2 = SetupCusInBondContainer(moveDetail.Containers.AddNew(), "CNT121", warehouseEntryNumber);
			SetupDisposition(moveDetail.DispositionCodes.AddNew(), DispositionList.Codes._1F, new ZDateTime(2014, 2, 5), 2);
			SetupDisposition(moveDetail.DispositionCodes.AddNew(), DispositionList.Codes._1I, new ZDateTime(2014, 2, 6), 1);
			SetupDisposition(moveDetail.DispositionCodes.AddNew(), DispositionList.Codes._22, new ZDateTime(2014, 2, 7), 3);
			SetupCusInBondMoveLineItem(moveDetail);
			SetupInBondWareHouseDetails(moveDetail);
			return moveDetail;
		}

		CusInBondMoveDetail SetupCusInBondMoveDetail(CusInBondMoveDetail moveDetail, ZString previousITNumber, ZDateTime previousITDate, ZString previousITType, ZString previousITPortDCode, ZInt inBoundQty,
			ZString customsStatus, ZString snp1, ZString snp2, ZString snp3, ZString snp4)
		{
			moveDetail.B9_PreviousITNumber = previousITNumber;
			moveDetail.B9_PreviousITDate = previousITDate;
			moveDetail.B9_PreviousITType = previousITType;
			moveDetail.B9_PreviousITPortDCode = previousITPortDCode;
			moveDetail.B9_InBoundQty = inBoundQty;
			moveDetail.B9_CustomsStatus = customsStatus;
			moveDetail.B9_FirstSecondaryNotifyParty = snp1;
			moveDetail.B9_SecondSecondaryNotifyParty = snp2;
			moveDetail.B9_ThirdSecondaryNotifyParty = snp3;
			moveDetail.B9_FourthSecondaryNotifyParty = snp4;
			return moveDetail;
		}

		void SetupInBondWareHouseDetails(CusInBondMoveDetail moveDetail)
		{
			var warehouseDetails = moveDetail.WarehouseDetails.AddNew();
			warehouseDetails.US_WarehouseNumber = "1234";
			warehouseDetails.US_WarehouseBondedQuantity = 2;
			warehouseDetails.US_WarehouseWithdrawQuantity = 1;
		}

		void SetupCusInBondMoveLineItem(CusInBondMoveDetail moveDetail)
		{
			var line = moveDetail.CBP7512Lines.AddNew();
			line.BI_PrintingSequenceNo = 1;
			line.BI_MarksAndNumbers = "MARKS AND NUMBERS";
			line.BI_Description = "DESCRIPTION";
			line.BI_Weight = 10m;
			line.BI_WeightUnit = "KG";
			line.BI_MonetaryValue = 50m;
			line.BI_RateComment = "RATE COMMENT";
			line.BI_DutyComment = "DUTY COMMENT";
			line.BI_IsMonetaryValueEstimated = true;
		}
	}
}
