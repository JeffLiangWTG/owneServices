using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestsSubclassesOf(typeof(BCDGoodsShipment))]
	abstract class BCDGoodsShipmentTest<T, S> : TestCaseWithFactory
		where T : IBCDGoodsShipment
		where S : MessageSendingObject
	{
		public void TestBCDGoodsShipmentData()
		{
			(var goodsShipment, _, _, var bill) = SetupData();
			bill.ABL_GrossWeight = 50000;
			bill.ABL_GrossWeightUQ = "G";
			bill.ABL_LocationInformation = "DESC P";
			bill.ABL_CustomsValue = 2m;
			CombineAssertions(() =>
			{
				AssertEquals("SequenceNumeric", bill.ABL_SequenceNumber, goodsShipment.SequenceNumeric);
				AssertEquals("TotalGrossMassMeasure", 50m, goodsShipment.TotalGrossMassMeasure);
				AssertEquals("DeliveryDestinationName", "DESC P", goodsShipment.DeliveryDestinationName);
				AssertEquals("ItemChargeAmount", 2m, goodsShipment.ItemChargeAmount);
			});
		}

		public void TestBuyer()
		{
			(var goodsShipment, _, _, _) = SetupData();
			AssertType<BCDConsignee>(goodsShipment.Buyer);
		}

		public void TestTradeTermsConditionCode()
		{
			(var goodsShipment, _, _, var bill) = SetupData();
			bill.ABL_Incoterm = "CIF";
			AssertEquals("CIF", goodsShipment.TradeTermsConditionCode);
		}

		public void TestGovernmentAgencyGoodsItem()
		{
			(var goodsShipment, _, _, var bill) = SetupData();
			bill.PackedItems.AddNew();
			bill.PackedItems.AddNew();
			var goodsItems = goodsShipment.GovernmentAgencyGoodsItems;
			CombineAssertions(() =>
			{
				AssertType<GovernmentAgencyGoodsItem>(goodsItems.FirstOrDefault());
				AssertEquals("Should generate 2 GovernmentAgencyGoodsItems", 2, goodsItems.Count());
				AssertContainsExactElementsInExactOrder(new ZInt[] { 1, 2 }, goodsItems.Select(x => x.SequenceNumeric));
			});
		}

		public void TestAdditionalDocuments()
		{
			(_, var messageSendingObject, var header, var bill) = SetupData();
			bill.ABL_BillNumber = $"B0001001";
			header.Bills.AddNew().ABL_BillNumber = $"B0001002";
			header.Bills.AddNew().ABL_BillNumber = $"B0001003";
			header.Bills.AddNew().ABL_BillNumber = ZString.Empty;

			var document = messageSendingObject.SupportingDocuments.AddNew();
			document.BillNumber = "B0001001";
			document = messageSendingObject.SupportingDocuments.AddNew();
			document.BillNumber = "B0001001";
			document = messageSendingObject.SupportingDocuments.AddNew();
			document.BillNumber = "B0001002";

			var goodsShipments = GetAdditionalDocuments(messageSendingObject).Cast<IGoodsShipment>();
			CombineAssertions(() =>
			{
				AssertEquals("2 documents assigned to Bill B0001001", 2, goodsShipments.ElementAt(0).AdditionalDocuments.Count());
				AssertEquals("1 document assigned to Bill B0001002", 1, goodsShipments.ElementAt(1).AdditionalDocuments.Count());
				AssertEquals("No document assigned to Bill B0001003", 0, goodsShipments.ElementAt(2).AdditionalDocuments.Count());
				AssertEquals("Bill.BillNumber is empty, so no document assigned to this GoodsShipment", 0, goodsShipments.ElementAt(3).AdditionalDocuments.Count());
			});
		}

		public void TestConsignee()
		{
			(var goodsShipment, _, _, _) = SetupData();
			AssertType<BCDConsignee>(goodsShipment.Consignee);
		}

		public void TestExporter()
		{
			(var goodsShipment, _, _, _) = SetupData();
			AssertType<BCDExporter>(goodsShipment.Exporter);
		}

		protected abstract List<BCDGoodsShipment> GetAdditionalDocuments(S messageSendingObject);

		protected abstract T GetGoodsShipment(S messageSendingObject);

		protected abstract S GetSendingObject(AsycudaManifestHeader header);

		protected (T goodsShipment, S messageSendingObject, AsycudaManifestHeader header, AsycudaBill bill) SetupData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			var bill = header.Bills.AddNew();
			var messageSendingObject = GetSendingObject(header);
			return (GetGoodsShipment(messageSendingObject), messageSendingObject, header, bill);
		}
	}
}
