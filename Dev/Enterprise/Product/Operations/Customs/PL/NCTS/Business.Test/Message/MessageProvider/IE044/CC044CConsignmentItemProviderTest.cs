using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC044CConsignmentItemProviderTest : DataProviderTestCase<CC044CConsignmentItemProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsArrivalCargoDesc", "Value cannot be null.\r\nParameter name: goodsItem", () => new CC044CConsignmentItemProvider(null));
		});
	}

	public void TestGoodsItemNumber()
	{
		goodsItem.BY_LineNo = 5;
		AssertEquals("5", Provider.GoodsItemNumber);
	}

	public void TestDeclarationGoodsItemNumber()
	{
		goodsItem.BY_DeclarationGoodsItemNumber = 123;
		AssertEquals("123", Provider.DeclarationGoodsItemNumber);
	}

	public void TestCommodity() => AssertNotNull(Provider.Commodity);

	public void TestPackaging()
	{
		goodsItem.Packages.AddNew();
		goodsItem.Packages.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("2 Packaging", 2, Provider.Packaging.Count);
			AssertEquals("Packaging sequence number should start with 1", 1, Provider.Packaging.First().SequenceNumber);
			AssertEquals("2nd Packaging sequence number should start with 2", 2, Provider.Packaging.Last().SequenceNumber);
		});
	}

	public void TestSupportingDocument()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty SupportingDocument", 0, Provider.SupportingDocument.Count);

			goodsItem.SupportingDocuments.AddNew();
			goodsItem.SupportingDocuments.AddNew();
			AssertEquals("Empty SupportingDocument", 2, GetProvider().SupportingDocument.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().SupportingDocument.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().SupportingDocument.Last().SequenceNumber);
		});
	}

	public void TestTransportDocument()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty TRA AdditionalInfos", 0, Provider.TransportDocument.Count);

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);

			AssertEquals("Not Empty TRA AdditionalInfos", 2, GetProvider().TransportDocument.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().TransportDocument.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().TransportDocument.Last().SequenceNumber);

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
				AddAdditionalInfosDocumentOfType(ZString.Empty);

			AssertEquals("Only TRA AdditionalInfos should be included", 2, GetProvider().TransportDocument.Count);
		});
	}

	public void TestAdditionalReference()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty REF AdditionalInfos", 0, Provider.AdditionalReference.Count);

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);

			AssertEquals("Not Empty REF AdditionalInfos", 2, GetProvider().AdditionalReference.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().AdditionalReference.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().AdditionalReference.Last().SequenceNumber);

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
				AddAdditionalInfosDocumentOfType(ZString.Empty);

			AssertEquals("Only REF AdditionalInfos should be included", 2, GetProvider().AdditionalReference.Count);
		});
	}

	void AddAdditionalInfosDocumentOfType(ZString subType)
	{
		var document = goodsItem.AdditionalInfos.AddNew();
		document.CSI_SubType = subType;
	}

	protected override CC044CConsignmentItemProvider GetProvider()
	{
		return new CC044CConsignmentItemProvider(goodsItem);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		bill = header.Bills.AddNew();
		goodsItem = bill.ArrivalGoodsItems.AddNew();
		goodsItem.BY_TransportChargesMethodOfPayment = "B";
	}

	NctsHeader header;
	NctsBill bill;
	NctsArrivalCargoDesc goodsItem;
}
