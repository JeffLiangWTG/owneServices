using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC013015CConsignmentItemProviderTest : Customs.Business.Testing.DataProviderTestCase<CC013015CConsignmentItemProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureCargoDesc", "Value cannot be null.\r\nParameter name: goodsItem", () => new CC013015CConsignmentItemProvider(null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: NctsDepartureCargoDesc.Header", () => new CC013015CConsignmentItemProvider(Factory.New<NctsDepartureCargoDesc>()));
			var header1 = Factory.New<NctsHeader>();
			header1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header1.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: NctsDepartureCargoDesc.MoveHeader", () => new CC013015CConsignmentItemProvider(header1.Bills.AddNew().GoodsItems.AddNew()));
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

	public void TestDeclarationType()
	{
		goodsItem.BY_Type = "TST";
		movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;

		CombineAssertions(() =>
		{
			AssertEquals("Movement declaration type is T", "TST", Provider.DeclarationType);

			movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			AssertNullOrEmpty("Movement declaration type is not T", GetProvider().DeclarationType);
		});
	}

	public void TestCountryOfDispatch()
	{
		movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
		movementHeader.BM_RN_NKCountryOfDispatch = ZString.Empty;
		bill.B0_RN_NKCountryOfExport = ZString.Empty;
		goodsItem.BY_RN_NKCountryOfDispatch = "AU";

		CombineAssertions(() =>
		{
			AssertEquals("Country of Dispatch required", "AU", Provider.CountryOfDispatch);

			movementHeader.BM_RN_NKCountryOfDispatch = "PL";
			AssertNullOrEmpty("Country of Dispatch present at Consignment", GetProvider().CountryOfDispatch);

			movementHeader.BM_RN_NKCountryOfDispatch = ZString.Empty;
			bill.B0_RN_NKCountryOfExport = "PL";
			AssertNullOrEmpty("Country of Dispatch present at HouseConsignment", GetProvider().CountryOfDispatch);

			movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
			movementHeader.BM_RN_NKCountryOfDispatch = ZString.Empty;
			bill.B0_RN_NKCountryOfExport = ZString.Empty;
			goodsItem.BY_RN_NKCountryOfDispatch = "AU";
			AssertEquals("Country of Dispatch present at ConsignmentItem level even if consignment declaration type is not TIR", "AU", GetProvider().CountryOfDispatch);
		});
	}

	public void TestCountryOfDestination()
	{
		movementHeader.BM_RL_NKDestinationPort = ZString.Empty;
		goodsItem.BY_RN_NKCountryOfDestination = "PL";

		CombineAssertions(() =>
		{
			AssertEquals("Country of Destination is required", "PL", Provider.CountryOfDestination);

			movementHeader.BM_RL_NKDestinationPort = "PL";
			AssertNullOrEmpty("Country of Destination is present at Consignment", GetProvider().CountryOfDestination);
		});
	}

	public void TestReferenceNumberUCR()
	{
		movementHeader.BM_UniqueConsignmentReference = ZString.Empty;
		bill.B0_ReferenceID = ZString.Empty;
		goodsItem.BY_CommercialReferenceNumber = "TstRefNum";

		CombineAssertions(() =>
		{
			AssertEquals("Item Reference Number is required", "TstRefNum", Provider.ReferenceNumberUCR);

			movementHeader.BM_UniqueConsignmentReference = "12345";
			AssertNullOrEmpty("Reference Number UCR is present at Consignment", GetProvider().ReferenceNumberUCR);

			movementHeader.BM_UniqueConsignmentReference = ZString.Empty;
			bill.B0_ReferenceID = "12345";
			AssertNullOrEmpty("Reference Number UCR is present at HouseConsignment", GetProvider().ReferenceNumberUCR);
		});
	}

	public void TestConsignee()
	{
		var organisation = Factory.New<OrgHeader>();

		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			AssertNull("No Consignee", Provider.Consignee);

			goodsItem.Consignee.OrganisationPK = organisation.PK;
			AssertNotNull("Valid Consignee", GetProvider().Consignee);

			header.Consignee.OrganisationPK = organisation.PK;
			AssertNull("Consignee present at Consignment", GetProvider().Consignee);

				header.Consignee.OrganisationPK = ZGuid.Empty;
				var additionalDocument1 = header.AdditionalDocuments.AddNew();
				additionalDocument1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocument1.CSI_Code = "30600";
				AssertNull("Consignment has Additional Document INF 30600", GetProvider().Consignee);

				header.AdditionalDocuments.RemoveAll();
				var additionalDocument2 = bill.AdditionalDocuments.AddNew();
				additionalDocument2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocument2.CSI_Code = "30600";
				AssertNull("House Consignment has Additional Document INF 30600", GetProvider().Consignee);

				bill.AdditionalDocuments.RemoveAll();
				var additionalDocument3 = goodsItem.AdditionalInfos.AddNew();
				additionalDocument3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocument3.CSI_Code = "30600";
				AssertNull("Consignment item has Additional Document INF 30600", GetProvider().Consignee);
			});

		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			goodsItem.Consignee.OrganisationPK = organisation.PK;
			AssertNull("Consignee not needed outside of the transition period", GetProvider().Consignee);
		});
	}

	public void TestAdditionalSupplyChainActor()
	{
		goodsItem.Bill.CusSupplyChainActorReferences.AddNew();
		goodsItem.Bill.CusSupplyChainActorReferences.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("2 AdditionalSupplyChainActor", 2, Provider.AdditionalSupplyChainActor.Count);
			AssertEquals("AdditionalSupplyChainActor Sequence number should start with 1", 1, Provider.AdditionalSupplyChainActor.First().SequenceNumber);
			AssertEquals("2nd AdditionalSupplyChainActor Sequence number should be 2", 2, Provider.AdditionalSupplyChainActor.Last().SequenceNumber);
		});
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

	public void TestAdditionalInformation()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty INF AdditionalInfos", 0, Provider.AdditionalInformation.Count);

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalInformation);

			AssertEquals("Not Empty INF AdditionalInfos", 2, GetProvider().AdditionalInformation.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().AdditionalInformation.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().AdditionalInformation.Last().SequenceNumber);

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);
				AddAdditionalInfosDocumentOfType(ZString.Empty);

			AssertEquals("Only INF AdditionalInfos should be included", 2, GetProvider().AdditionalInformation.Count);
		});
	}

	public void TestPreviousDocument()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty PreviousDocuments", 0, GetProvider().PreviousDocument.Count);

			goodsItem.PreviousDocuments.AddNew();
			goodsItem.PreviousDocuments.AddNew();
			AssertEquals("Empty PreviousDocuments", 2, GetProvider().PreviousDocument.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().PreviousDocument.First().SequenceNumber);
			AssertEquals("Empty PreviousDocuments", "2", GetProvider().PreviousDocument.Last().SequenceNumber);
		});
	}

	public void TestSupportingDocument()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty SupportingDocument", 0, GetProvider().SupportingDocument.Count);

			goodsItem.SupportingDocuments.AddNew();
			goodsItem.SupportingDocuments.AddNew();
			AssertEquals("Empty SupportingDocument", 2, GetProvider().SupportingDocument.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().SupportingDocument.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().SupportingDocument.Last().SequenceNumber);
		});
	}

	public void TestTransportDocument()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			AssertEquals("If not in the phase5 transition period", true, Provider.TransportDocument.IsNullOrEmpty());
		});

		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
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

	public void TestTransportCharges()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			AssertNull("If not in the phase5 transition period", Provider.TransportCharges);
		});

		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			goodsItem.MoveHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			goodsItem.MoveHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
			AssertNull("When BM_TypeOfSecurity equals NON and BM_MethodOfPayment is present", GetProvider().TransportCharges);

			goodsItem.MoveHeader.BM_MethodOfPayment = ZString.Empty;
			goodsItem.MoveHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals("When BM_MethodOfPayment is not present", TransportChargesModeOfPayment.Codes.CreditCard, GetProvider().TransportCharges);

			goodsItem.MoveHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
			goodsItem.MoveHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals("When BM_TypeOfSecurity is not NON", TransportChargesModeOfPayment.Codes.CreditCard, GetProvider().TransportCharges);
		});
	}

	void AddAdditionalInfosDocumentOfType(ZString subType)
	{
		var document = goodsItem.AdditionalInfos.AddNew();
		document.CSI_SubType = subType;
	}

	protected override CC013015CConsignmentItemProvider GetProvider()
	{
		return new CC013015CConsignmentItemProvider(goodsItem);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		bill = header.Bills.AddNew();
		goodsItem = bill.GoodsItems.AddNew();
		movementHeader = (NctsDepartureMovementHeader)goodsItem.MoveHeader;
		goodsItem.BY_TransportChargesMethodOfPayment = "B";
	}

	NctsHeader header;
	NctsDepartureMovementHeader movementHeader;
	NctsBill bill;
	NctsDepartureCargoDesc goodsItem;
}
