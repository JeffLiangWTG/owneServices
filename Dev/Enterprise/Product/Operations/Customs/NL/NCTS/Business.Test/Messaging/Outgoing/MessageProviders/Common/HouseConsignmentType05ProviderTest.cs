using System;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(HouseConsignmentType05Provider))]
sealed class HouseConsignmentType05ProviderTest : Customs.Business.Testing.DataProviderTestCase<HouseConsignmentType05Provider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new HouseConsignmentType05Provider(null, "1"));

	public void TestSequenceNumeric() => AssertEquals(1, Provider.SequenceNumeric);

	public void TestGrossMass() => CombineAssertions(() =>
	{
		bill.B0_Weight = 1.2m;
		bill.B0_WeightUQ = "KG";
		bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals(1.2m, Provider.GrossMass);

		bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 10;
		goodsItem1.BY_GrossWeightUnit = "KG";

		var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
		goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		goodsItem2.BY_GrossWeight = 2;
		goodsItem2.BY_GrossWeightUnit = "KG";
		var unloadedGoodsItem = goodsItem2.UnloadedGoodsItem;
		unloadedGoodsItem.BY_GrossWeight = 10;
		unloadedGoodsItem.BY_GrossWeightUnit = "KG";

		AssertEquals(20m, Provider.GrossMass);

		bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertNull(Provider.GrossMass);
	});

	public void TestDepartureTransportMeans()
	{
		var transportMeans1 = bill.ArrivalTransportInfos.AddNew();
		transportMeans1.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
		var transportMeans2 = bill.ArrivalTransportInfos.AddNew();
		transportMeans2.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
		var transportMeans3 = bill.ArrivalTransportInfos.AddNew();
		transportMeans3.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;

		AssertEquals(2, Provider.DepartureTransportMeans.Count);
	}

	public void TestSupportingDocuments()
	{
		var supportingDocument1 = bill.SupportingDocuments.AddNew();
		supportingDocument1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		var supportingDocument2 = bill.SupportingDocuments.AddNew();
		supportingDocument2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
		var supportingDocument3 = bill.SupportingDocuments.AddNew();
		supportingDocument3.CSI_Status = NctsUnloadedStateList.Codes.DEC;

		AssertEquals(2, Provider.SupportingDocuments.Count);
	}

	public void TestAdditionalReferences()
	{
		var additionalReference1 = bill.AdditionalDocuments.AddNew();
		additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		additionalReference1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		var additionalReference2 = bill.AdditionalDocuments.AddNew();
		additionalReference2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		additionalReference2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
		var additionalReference3 = bill.AdditionalDocuments.AddNew();
		additionalReference3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		additionalReference3.CSI_Status = NctsUnloadedStateList.Codes.DEC;

		AssertEquals(2, Provider.AdditionalReferences.Count);
	}

	public void TestConsignmentItems() => CombineAssertions(() =>
	{
		var item = bill.ArrivalGoodsItems.AddNew();

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		provider = new HouseConsignmentType05Provider(bill, "1");
		AssertEquals("DEC", 0, provider.ConsignmentItems.Count);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DAM;
		provider = new HouseConsignmentType05Provider(bill, "1");
		AssertEquals("DAM", 0, provider.ConsignmentItems.Count);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		provider = new HouseConsignmentType05Provider(bill, "1");
		AssertEquals("NEW", 1, provider.ConsignmentItems.Count);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		provider = new HouseConsignmentType05Provider(bill, "1");
		AssertEquals("MIS", 1, provider.ConsignmentItems.Count);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		provider = new HouseConsignmentType05Provider(bill, "1");
		AssertEquals("DIF", 1, provider.ConsignmentItems.Count);
	});

	public void TestConsignmentItems_Order()
	{
		var item1 = bill.ArrivalGoodsItems.AddNew();
		item1.BY_LineNo = 2;
		var item2 = bill.ArrivalGoodsItems.AddNew();
		item2.BY_LineNo = 1;
		var item3 = bill.ArrivalGoodsItems.AddNew();
		item3.BY_LineNo = 3;

		AssertContainsExactElementsInExactOrder(new[] { 1, 2, 3 }, provider.ConsignmentItems.Select(x => x.GoodsItemNumber));
	}

	public void TestTransportDocuments()
	{
		var additionalReference1 = bill.AdditionalDocuments.AddNew();
		additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		additionalReference1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		var additionalReference2 = bill.AdditionalDocuments.AddNew();
		additionalReference2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		additionalReference2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
		var additionalReference3 = bill.AdditionalDocuments.AddNew();
		additionalReference3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		additionalReference3.CSI_Status = NctsUnloadedStateList.Codes.DEC;

		AssertEquals(2, Provider.TransportDocuments.Count);
	}

	protected override HouseConsignmentType05Provider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		bill = nctsHeader.Bills.AddNew();

		provider = new HouseConsignmentType05Provider(bill, "1");
	}
	NctsBill bill;
	HouseConsignmentType05Provider provider;
}
