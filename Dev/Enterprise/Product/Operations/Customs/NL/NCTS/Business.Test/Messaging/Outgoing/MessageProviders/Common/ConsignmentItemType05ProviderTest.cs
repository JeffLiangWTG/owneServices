using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(ConsignmentItemType05Provider))]
sealed class ConsignmentItemType05ProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentItemType05Provider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new ConsignmentItemType05Provider(null));

	public void TestGoodsItemNumber()
	{
		item.BY_LineNo = 2;
		AssertEquals(2, Provider.GoodsItemNumber);
	}

	public void TestDeclarationGoodsItemNumber() => CombineAssertions(() =>
	{
		item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		item.BY_DeclarationGoodsItemNumber = 3;
		AssertEquals("NEW", 3, Provider.DeclarationGoodsItemNumber);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals("DIF", 3, GetProvider().DeclarationGoodsItemNumber);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals("MIS", 3, GetProvider().DeclarationGoodsItemNumber);

		item.BY_DeclarationGoodsItemNumber = 3;
		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		AssertEquals(0, Provider.DeclarationGoodsItemNumber);
	});

	public void TestCommodity() => CombineAssertions(() =>
	{
		item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertNotNull("NEW", Provider.Commodity);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		provider = new ConsignmentItemType05Provider(item);
		AssertNotNull("DIF", GetProvider().Commodity);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		provider = new ConsignmentItemType05Provider(item);
		AssertNull(GetProvider().Commodity);
	});

	public void TestCommodity_UnloadedItem()
	{
		CombineAssertions(() =>
		{
			item.BY_NetWeight = 5;
			AssertEquals("should be 5 because it's a NEW item", 5m, Provider.Commodity.GoodsMeasure.NetNetWeightMeasure);

			item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var unloadedItem = item.UnloadedGoodsItem;
			unloadedItem.BY_NetWeight = 10;
			unloadedItem.BY_NetWeightUnit = "KG";
			item.BY_BY_Commodity = unloadedItem.PK;
			provider = new ConsignmentItemType05Provider(item);
			AssertEquals("should be new unloaded item's value", 10m, GetProvider().Commodity.GoodsMeasure.NetNetWeightMeasure);
		});
	}

	public void TestPackagings() => CombineAssertions(() =>
	{
		var package = item.Packages.AddNew();
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		AssertEquals(1, Provider.Packagings.Count);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		provider = new ConsignmentItemType05Provider(item);
		AssertEquals(0, GetProvider().Packagings.Count);
	});

	public void TestSupportingDocuments()
	{
		var supportingDocument1 = item.SupportingDocuments.AddNew();
		supportingDocument1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		var supportingDocument2 = item.SupportingDocuments.AddNew();
		supportingDocument2.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		var supportingDocument3 = item.SupportingDocuments.AddNew();
		supportingDocument3.CSI_Status = NctsUnloadedStateList.Codes.DEC;

		AssertEquals(2, Provider.SupportingDocuments.Count);
	}

	public void TestAdditionalReferences()
	{
		var ref1 = item.AdditionalInfos.AddNew();
		ref1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		ref1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		var ref2 = item.AdditionalInfos.AddNew();
		ref2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		ref2.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		var ref3 = item.AdditionalInfos.AddNew();
		ref3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		ref3.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		AssertEquals(2, Provider.AdditionalReferences.Count);
	}

	public void TestTransportDocuments()
	{
		var tra1 = item.AdditionalInfos.AddNew();
		tra1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		tra1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		var tra2 = item.AdditionalInfos.AddNew();
		tra2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		tra2.CSI_Status = NctsUnloadedStateList.Codes.NEW;
		var tra3 = item.AdditionalInfos.AddNew();
		tra3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		tra3.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		AssertEquals(2, Provider.TransportDocuments.Count);
	}

	protected override ConsignmentItemType05Provider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		item = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
		provider = new ConsignmentItemType05Provider(item);
	}

	ConsignmentItemType05Provider provider;
	NctsArrivalCargoDesc item;
}
