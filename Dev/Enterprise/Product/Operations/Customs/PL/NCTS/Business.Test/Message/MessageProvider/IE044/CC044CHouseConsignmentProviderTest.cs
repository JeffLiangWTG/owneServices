using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC044CHouseConsignmentProviderTest : DataProviderTestCase<CC044CHouseConsignmentProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsBill", "Value cannot be null.\r\nParameter name: bill", () => new CC044CHouseConsignmentProvider(1, null));
		});
	}

	public void TestSequenceNumber() => AssertEquals("99", GetProvider().SequenceNumber);

	public void TestGrossMass() => CombineAssertions(() =>
	{
		nctsBill.B0_WeightUQ = Core.Constants.Weight.Grams;

		nctsBill.B0_Weight = 123040m;
		nctsBill.B0_GrossWeightUnloaded = 150750m;
		nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals("GrossMass for DIF UnloadedState", 150.75m, GetProvider().GrossMass);
		nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("GrossMass for NEW UnloadedState", 123.04m, GetProvider().GrossMass);

		nctsBill.B0_WeightUQ = Core.Constants.Weight.Tonnes;
		nctsBill.B0_Weight = 0.123040m;
		nctsBill.B0_GrossWeightUnloaded = 0.150750m;
		nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals("GrossMass for DIF UnloadedState", 150.75m, GetProvider().GrossMass);
		nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("GrossMass for NEW UnloadedState", 123.04m, GetProvider().GrossMass);
	});

	public void TestDepartureTransportMeans()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty DepartureTransportMeans", 0, Provider.DepartureTransportMeans.Count);

			nctsBill.ArrivalTransportInfos.AddNew();
			nctsBill.ArrivalTransportInfos.AddNew();
			AssertEquals("Should have 2 DepartureTransportMeans", 2, GetProvider().DepartureTransportMeans.Count);
			AssertEquals("Sequence number should start from 1", "1", GetProvider().DepartureTransportMeans.First().SequenceNumber);
			AssertEquals("Sequence number for 2nd DepartureTransportMeans should be 2", "2", GetProvider().DepartureTransportMeans.Last().SequenceNumber);
		});
	}

	public void TestConsignmentItem()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No consignment items", 0, GetProvider().ConsignmentItem.Count);

			nctsBill.ArrivalGoodsItems.AddNew();
			AssertEquals("1 consignment item", 1, GetProvider().ConsignmentItem.Count);
			nctsBill.ArrivalGoodsItems.AddNew();
			AssertEquals("2 consignment item", 2, GetProvider().ConsignmentItem.Count);
		});
	}

	public void TestSupportingDocument()
	{
		CombineAssertions(() =>
		{
			var provider = GetProvider();
			AssertNotNull("Not null SupportingDocuments 1", provider.SupportingDocument);
			Assert("Empty SupportingDocuments", !(provider.SupportingDocument.Count > 0));

			nctsBill.SupportingDocuments.AddNew();
			nctsBill.SupportingDocuments.AddNew();

			provider = GetProvider();
			AssertNotNull("Not null SupportingDocuments 2", provider.SupportingDocument);
			AssertEquals("SupportingDocuments count == 2", 2, provider.SupportingDocument.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", provider.SupportingDocument.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", provider.SupportingDocument.Last().SequenceNumber);
		});
	}

	public void TestTransportDocument()
	{
		CombineAssertions(() =>
		{
			var provider = GetProvider();
			AssertNotNull("Not null TransportDocument 1", provider.TransportDocument);
			Assert("Empty TransportDocument", !(provider.TransportDocument.Count > 0));

			AssertEquals("Empty TRA AdditionalInfos", 0, GetProvider().TransportDocument.Count);

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);

			provider = GetProvider();
			AssertNotNull("Not null TransportDocument 2", provider.TransportDocument);
			AssertEquals("TransportDocument count == 2", 2, provider.TransportDocument.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", provider.TransportDocument.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", provider.TransportDocument.Last().SequenceNumber);

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
			var provider = GetProvider();
			AssertNotNull("Not null AdditionalReference 1", provider.AdditionalReference);
			Assert("Empty AdditionalReference", !(provider.AdditionalReference.Count > 0));

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);

			provider = GetProvider();
			AssertNotNull("Not null AdditionalReference 2", provider.AdditionalReference);
			AssertEquals("AdditionalReference count == 2", 2, provider.AdditionalReference.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", provider.AdditionalReference.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", provider.AdditionalReference.Last().SequenceNumber);

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
				AddAdditionalInfosDocumentOfType(ZString.Empty);

			AssertEquals("Only REF AdditionalInfos should be included", 2, GetProvider().AdditionalReference.Count);
		});
	}

	void AddAdditionalInfosDocumentOfType(ZString subType)
	{
		var document = nctsBill.AdditionalDocuments.AddNew();
		document.CSI_SubType = subType;
	}

	protected override CC044CHouseConsignmentProvider GetProvider() => new CC044CHouseConsignmentProvider(99, nctsBill);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsBill = nctsHeader.Bills.AddNew();

		nctsBill.B0_ReferenceID = "ReferenceId";
	}

	NctsHeader nctsHeader;
	NctsBill nctsBill;
}
