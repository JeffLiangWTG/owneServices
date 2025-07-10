using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(HouseConsignmentProvider))]
sealed class HouseConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<HouseConsignmentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new HouseConsignmentProvider(null));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(1, Provider.SequenceNumeric);
	}

	public void TestCountryOfDispatch()
	{
		bill.B0_RN_NKCountryOfExport = "BE";
		AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.CountryOfDispatch);
	}

	public void TestGrossMass() => CombineAssertions(() =>
	{
		AssertEquals("empty", decimal.Zero, Provider.GrossMass);
		bill.B0_Weight = 10.2700m;
		AssertEquals("filled", 10.27m, Provider.GrossMass);
		bill.B0_Weight = 10.2700m;
		AssertEquals("normalized", "10.27", Provider.GrossMass.ToString());
	});

	public void TestReferenceNumberUCR()
	{
		bill.B0_ReferenceID = "ReferenceNumberUCR";
		AssertEquals("ReferenceNumberUCR", Provider.ReferenceNumberUCR);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		bill.B0_TransportPaymentMethod = "D";
		AssertEquals("D", Provider.TransportChargesMethodOfPayment);
	}

	public void TestAdditionalSupplyChainActors()
	{
		Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "SCA", parent: bill);
		Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "SCA", parent: bill);

		CombineAssertions(() =>
		{
			AssertEquals(2, Provider.AdditionalSupplyChainActors.Count);
			AssertEquals(1, Provider.AdditionalSupplyChainActors.First().SequenceNumeric);
			AssertEquals(2, Provider.AdditionalSupplyChainActors.Skip(1).First().SequenceNumeric);
		});
	}

	public void TestDepartureTransportMeans()
	{
		var departureTransportMeans1 = bill.DepartureTransportInfos.AddNew();
		var departureTransportMeans2 = bill.DepartureTransportInfos.AddNew();

		departureTransportMeans1.TPM_SequenceNumber = 1;
		departureTransportMeans1.TPM_IdentificationNumber = "1";
		departureTransportMeans2.TPM_SequenceNumber = 2;
		departureTransportMeans2.TPM_IdentificationNumber = "2";

		CombineAssertions(() =>
		{
			AssertEquals(2, Provider.DepartureTransportMeans.Count);
			AssertEquals(1, Provider.DepartureTransportMeans.First().SequenceNumeric);
			AssertEquals(2, Provider.DepartureTransportMeans.Skip(1).First().SequenceNumeric);
		});
	}

	public void TestPreviousDocuments()
	{
		bill.PreviousDocuments.AddNew();
		AssertEquals("Count", 1, Provider.PreviousDocuments.Count);
	}

	public void TestTransportDocuments()
	{
		var additionalDocument = bill.AdditionalDocuments.AddNew();
		additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		AssertEquals("Count", 1, Provider.TransportDocuments.Count);
	}

	public void TestAdditionalReferences()
	{
		var additionalDocument = bill.AdditionalDocuments.AddNew();
		additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		AssertEquals("Count", 1, Provider.AdditionalReferences.Count);
	}

	public void TestConsignor()
	{
		var consignor = bill.DocAddresses.AddNew();
		consignor.E2_AddressType = AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress;
		CombineAssertions(() =>
		{
			AssertNotNull(Provider.Consignor);
			AssertType<OmitNameAndAddressPartyProvider>(Provider.Consignor);
		});
	}

	public void TestConsignee()
	{
		var consignee = bill.DocAddresses.AddNew();
		consignee.E2_AddressType = AutoDocAddressTypes.Codes.ConsigneeAddress;
		CombineAssertions(() =>
		{
			AssertNotNull(Provider.Consignee);
			AssertType<OmitNameAndAddressPartyProvider>(Provider.Consignee);
		});
	}

	public void TestConsignmentItems()
	{
		bill.GoodsItems.AddNew();
		AssertEquals(1, Provider.ConsignmentItems.Count);
	}

	public void TestSupportingDocuments()
	{
		bill.SupportingDocuments.AddNew();
		AssertEquals("Count", 1, Provider.SupportingDocuments.Count);
	}

	public void TestAdditionalInformations()
	{
		var additionalDocument = bill.AdditionalDocuments.AddNew();
		additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		AssertEquals("Count", 1, Provider.AdditionalInformations.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		bill = nctsHeader.Bills.AddNew();

		provider = new HouseConsignmentProvider(bill);
	}

	NctsBill bill;
	HouseConsignmentProvider provider;

	protected override HouseConsignmentProvider GetProvider() => provider;
}
