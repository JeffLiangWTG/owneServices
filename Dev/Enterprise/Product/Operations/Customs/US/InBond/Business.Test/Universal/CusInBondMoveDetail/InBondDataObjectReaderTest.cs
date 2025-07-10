using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectReaderTest
	{
		public void TestImportingInBondMoveDetailData()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB1";
			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { SetupContainer2("CONT1", 1), SetupContainer("CONT2", 2) }));
			var helper = new InBondDataObjectReaderHelper(Factory);
			helper.CollectContainerPackingLineAndCommercialInvoiceLineDetails(shipmentDataObject);
			helper.CollectBillLink(bill, SetupInBondBill("MB1", WayBillTypeList.Codes.Master, ZString.Empty, 1));
			var inBondMoveDetailDataObject = SetupInBondMoveDetail(1, "0002");
			inBondMoveDetailDataObject.ContainerLinkCollection = new List<ContainerLink>(new[] { new ContainerLink()
			{ Link = (ZInt)1, ContainerNumber = "CONT1" }, new ContainerLink()
			{ Link = (ZInt)2, ContainerNumber = "CONT2" } });
			Factory.SaveForTesting();
			var reader = new CusInBondMoveDetailDataObjectReader(inBondMoveDetailDataObject, logger, helper, bill.MovementDetail.MoveHeader.PK, bill.PK, "");
			var moveDetailBO1 = reader.ReadIntoBusinessObject();
			AssertNotNull(moveDetailBO1);
			CombineAssertions(delegate
			{
				AssertCusInBondMoveDetailContents(moveDetailBO1, "0002");
				AssertEquals("moveDetailBO1.Containers.Count", 2, moveDetailBO1.Containers.Count);
				AssertNotNull(moveDetailBO1.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1"));
				AssertNotNull(moveDetailBO1.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT2"));
				AssertMultilineASCIIEquals("logger.Logs", @" 
Information - No matching CusInBondMoveDetail found, creating new CusInBondMoveDetail.
Information - Populating CusInBondMoveDetail...
Information - No matching CusInBondContainer found, creating new CusInBondContainer.
Information - Populating CusInBondContainer...
Information - Successfully loaded matching Container Type.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching CusInBondContainer found, creating new CusInBondContainer.
Information - Populating CusInBondContainer...
Information - Successfully loaded matching Container Type.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...".Trim(), logger.Logs);
			});
		}

		protected override InBondMoveDetail SetupInBondMoveDetail(ZInt additionalBillLink, ZInt inBondQty)
		{
			var result = SetupInBondMoveDetail(additionalBillLink, ZString.Empty, "IT323", new ZDateTime(2014, 2, 4), InbondCommonTypeList.Codes._2TransportandExport, SeaLocalPort3ScheduleD.ZZD_Code, inBondQty, "SNP1", "SNP2", "SNP3", "SNP4");
			result.AddInfoGroupCollection = new List<AddInfoGroup>(new[] { SetupDisposition() });
			return result;
		}

		InBondMoveDetail SetupInBondMoveDetail(ZInt additionalBillLink, ZString seqNo)
		{
			var result = SetupInBondMoveDetail(additionalBillLink, seqNo, "IT323", new ZDateTime(2014, 2, 4), InbondCommonTypeList.Codes._2TransportandExport, SeaLocalPort3ScheduleD.ZZD_Code, 110, "SNP1", "SNP2", "SNP3", "SNP4");
			result.AddInfoGroupCollection = new List<AddInfoGroup>(new[] { SetupDisposition() });
			return result;
		}

		InBondMoveDetail SetupInBondMoveDetail(ZInt additionalBillLink, ZString seqNo, ZString previousITNumber, ZDateTime previousITDate, ZString previousITType, ZString previousITPortDCode,
			ZInt inBondQty, ZString? firstSecondaryNotifyParty = null, ZString? secondSecondaryNotifyParty = null, ZString? thirdSecondaryNotifyParty = null, ZString? fourthSecondaryNotifyParty = null)
		{
			var result = SetupInBondMoveDetail(additionalBillLink, previousITNumber, previousITDate, previousITType, previousITPortDCode, inBondQty);
			result.SequenceNumber = seqNo;
			var customsReferenceCollection = new List<CustomsReference>();
			if (firstSecondaryNotifyParty.HasValue)
			{
				customsReferenceCollection.Add(SetupSecondaryNotifyParty(SecondaryNotifyPartyCodeList.Codes.First, firstSecondaryNotifyParty.Value));
			}

			if (secondSecondaryNotifyParty.HasValue)
			{
				customsReferenceCollection.Add(SetupSecondaryNotifyParty(SecondaryNotifyPartyCodeList.Codes.Second, secondSecondaryNotifyParty.Value));
			}

			if (thirdSecondaryNotifyParty.HasValue)
			{
				customsReferenceCollection.Add(SetupSecondaryNotifyParty(SecondaryNotifyPartyCodeList.Codes.Third, thirdSecondaryNotifyParty.Value));
			}

			if (fourthSecondaryNotifyParty.HasValue)
			{
				customsReferenceCollection.Add(SetupSecondaryNotifyParty(SecondaryNotifyPartyCodeList.Codes.Fourth, fourthSecondaryNotifyParty.Value));
			}

			result.CustomsReferenceCollection = customsReferenceCollection;
			result.InBondMoveLineItemCollection = SetupInBondMoveLineItems();
			result.WarehouseDetailCollection = SetupWarehouseDetails();
			return result;
		}

		CustomsReference SetupSecondaryNotifyParty(ZString code, ZString reference)
		{
			return new CustomsReference()
			{
				Type = new CodeDescriptionPair()
				{ Code = Constants.SecondaryNotifyParty.Type, Description = Constants.SecondaryNotifyParty.TypeDescription },
				SubType = new CodeDescriptionPair35Char()
				{ Code = code },
				Reference = reference
			};
		}

		List<InBondMoveLineItem> SetupInBondMoveLineItems()
		{
			return new List<InBondMoveLineItem>()
			{ new InBondMoveLineItem()
			{ PrintingSequenceNo = 1, MarksAndNumbers = "MARKS AND NUMBERS", DescriptionAndQuantityOfMerchandise = "DESCRIPTION", Weight = 10m, WeightUnit = new CodeDescriptionPair()
			{ Code = "KG", Description = "Kilogrammes" }, MonetaryValue = 50m, RateComment = "RATE COMMENT", DutyComment = "DUTY COMMENT", IsMonetaryValueEstimated = true } };
		}

		List<InBondWarehouseDetail> SetupWarehouseDetails()
		{
			return new List<InBondWarehouseDetail>()
			{ new InBondWarehouseDetail()
			{ EntryNumber = "1234", BondedQuantity = 2, WithdrawQuantity = 1 } };
		}

		void AssertCusInBondMoveDetailContents(CusInBondMoveDetail moveDetailBO, ZString seqNo)
		{
			AssertCusInBondMoveDetailContents(moveDetailBO, seqNo, "IT323", new ZDateTime(2014, 2, 4), InbondCommonTypeList.Codes._2TransportandExport, SeaLocalPort3ScheduleD.ZZD_Code, 110, "SNP1", "SNP2", "SNP3", "SNP4");
			AssertEquals("moveDetailBO.DispositionCodes.Count", 0, moveDetailBO.DispositionCodes.Count);
			AssertEquals("moveDetailBO.CBP7512Lines.Count", 1, moveDetailBO.CBP7512Lines.Count);
			AssertCusInBondMoveLineItemContents(moveDetailBO.CBP7512Lines.FirstOrDefault());
			AssertEquals("moveDetailBO.WarehouseDetails.Count", 1, moveDetailBO.WarehouseDetails.Count);
			AssertWarehouseDetailContents((WarehouseDetail)moveDetailBO.WarehouseDetails.FirstOrDefault());
		}

		void AssertCusInBondMoveDetailContents(CusInBondMoveDetail moveDetailBO, ZString seqNo, ZString previousITNumber, ZDateTime previousITDate, ZString previousITType, ZString previousITPortDCode,
			ZInt inBoundQty, ZString firstSecondaryNotifyParty, ZString secondSecondaryNotifyParty, ZString thirdSecondaryNotifyParty, ZString fourthSecondaryNotifyParty)
		{
			AssertEquals("moveDetailBO.B9_SeqNo", seqNo, moveDetailBO.B9_SeqNo);
			AssertEquals("moveDetailBO.B9_PreviousITNumber", previousITNumber, moveDetailBO.B9_PreviousITNumber);
			AssertEquals("moveDetailBO.previousITDate", previousITDate, moveDetailBO.B9_PreviousITDate);
			AssertEquals("moveDetailBO.B9_PreviousITType", previousITType, moveDetailBO.B9_PreviousITType);
			AssertEquals("moveDetailBO.B9_PreviousITPortDCode", previousITPortDCode, moveDetailBO.B9_PreviousITPortDCode);
			AssertEquals("moveDetailBO.B9_InBoundQty", inBoundQty, moveDetailBO.B9_InBoundQty);
			AssertEquals("moveDetailBO.B9_FirstSecondaryNotifyParty", firstSecondaryNotifyParty, moveDetailBO.B9_FirstSecondaryNotifyParty);
			AssertEquals("moveDetailBO.B9_SecondSecondaryNotifyParty", secondSecondaryNotifyParty, moveDetailBO.B9_SecondSecondaryNotifyParty);
			AssertEquals("moveDetailBO.B9_ThirdSecondaryNotifyParty", thirdSecondaryNotifyParty, moveDetailBO.B9_ThirdSecondaryNotifyParty);
			AssertEquals("moveDetailBO.B9_FourthSecondaryNotifyParty", fourthSecondaryNotifyParty, moveDetailBO.B9_FourthSecondaryNotifyParty);
		}

		void AssertCusInBondMoveLineItemContents(CusInBondMoveLineItem moveDetailLineItem)
		{
			AssertEquals("moveDetailLineItem.BI_PrintingSequenceNo", (short)1, moveDetailLineItem.BI_PrintingSequenceNo);
			AssertEquals("moveDetailLineItem.BI_MarksAndNumbers", "MARKS AND NUMBERS", moveDetailLineItem.BI_MarksAndNumbers);
			AssertEquals("moveDetailLineItem.BI_Description", "DESCRIPTION", moveDetailLineItem.BI_Description);
			AssertEquals("moveDetailLineItem.BI_Weight", 10m, moveDetailLineItem.BI_Weight);
			AssertEquals("moveDetailLineItem.BI_WeightUnit", "KG", moveDetailLineItem.BI_WeightUnit);
			AssertEquals("moveDetailLineItem.BI_MonetaryValue", 50m, moveDetailLineItem.BI_MonetaryValue);
			AssertEquals("moveDetailLineItem.BI_RateComment", "RATE COMMENT", moveDetailLineItem.BI_RateComment);
			AssertEquals("moveDetailLineItem.BI_DutyComment", "DUTY COMMENT", moveDetailLineItem.BI_DutyComment);
			AssertEquals("moveDetailLineItem.BI_IsMonetaryValueEstimated", true, moveDetailLineItem.BI_IsMonetaryValueEstimated);
		}

		void AssertWarehouseDetailContents(WarehouseDetail warehouseDetail)
		{
			AssertEquals("warehouseDetail.US_WarehouseNumber", "1234", warehouseDetail.US_WarehouseNumber);
			AssertEquals("warehouseDetail.US_WarehouseBondedQuantity", 2m, warehouseDetail.US_WarehouseBondedQuantity);
			AssertEquals("warehouseDetail.US_WarehouseWithdrawQuantity", 1m, warehouseDetail.US_WarehouseWithdrawQuantity);
		}
	}
}
