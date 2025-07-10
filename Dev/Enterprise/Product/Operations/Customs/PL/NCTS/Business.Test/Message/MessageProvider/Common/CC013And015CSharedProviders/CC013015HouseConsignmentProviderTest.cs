using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

abstract class CC013015HouseConsignmentProviderTest<T> : ConsignmentProviderBaseTest<T> where T : ConsignmentProviderBase, IHouseConsignment
{
	public void TestSequenceNumber() => AssertEquals("99", GetProvider().SequenceNumber);

	public void TestConsignmentItem()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No consignment items", 0, GetProvider().ConsignmentItem.Count);

			nctsBill.GoodsItems.AddNew();
			AssertEquals("1 consignment item", 1, GetProvider().ConsignmentItem.Count);
			nctsBill.GoodsItems.AddNew();
			AssertEquals("2 consignment item", 2, GetProvider().ConsignmentItem.Count);
		});
	}

	public void TestConsignmentItem_SortedBy_BY_DeclarationGoodsItemNumber()
	{
		nctsBill.B0_ReferenceID = ZString.Empty;

		var goodsItem5 = nctsBill.GoodsItems.AddNew();
		goodsItem5.BY_DeclarationGoodsItemNumber = 5;
		goodsItem5.BY_CommercialReferenceNumber = nameof(goodsItem5);

		var goodsItem4 = nctsBill.GoodsItems.AddNew();
		goodsItem4.BY_DeclarationGoodsItemNumber = 4;
		goodsItem4.BY_CommercialReferenceNumber = nameof(goodsItem4);

		var goodsItem3 = nctsBill.GoodsItems.AddNew();
		goodsItem3.BY_DeclarationGoodsItemNumber = 3;
		goodsItem3.BY_CommercialReferenceNumber = nameof(goodsItem3);

		var goodsItem6 = nctsBill.GoodsItems.AddNew();
		goodsItem6.BY_DeclarationGoodsItemNumber = 6;
		goodsItem6.BY_CommercialReferenceNumber = nameof(goodsItem6);

		AssertSequencesEqual(
			"Sorted by BY_DeclarationGoodsItemNumber",
			[nameof(goodsItem3), nameof(goodsItem4), nameof(goodsItem5), nameof(goodsItem6)],
			GetProvider().ConsignmentItem.Select(x => x.ReferenceNumberUCR));
	}

	public void TestConsignee_OutsidePhase5TransitionPeriod()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			AssertNull("No Consignee", GetProvider().Consignee);

			SetUpValidAddress(nctsBill.Consignee);
			AssertNotNull("Valid Consignee", GetProvider().Consignee);
		});
	}

	public void TestConsignee_InPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			AssertNull("No Consignee", GetProvider().Consignee);

			SetUpValidAddress(nctsBill.Consignee);
			AssertNull("Valid Consignee but in phase5 transition period", GetProvider().Consignee);
		});
	}

	public abstract void TestConsignee_RuleC0001_HeaderAdditionalDocument();

		public void TestConsignee_RuleC0001_ConsignmentAdditionalDocument()
		{
			SetUpValidAddress(nctsBill.Consignee);
			CombineAssertions(() =>
			{
				var document1 = nctsBill.AdditionalDocuments.AddNew();
				document1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				document1.CSI_Code = "30600";
				AssertNotNull("Consignment contains additional document with CSI_SubType <> INF", GetProvider().Consignee);

				var document2 = nctsBill.AdditionalDocuments.AddNew();
				document2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				document2.CSI_Code = "10600";
				AssertNotNull("Consignment contains additional document with CSI_Code <> 30600", GetProvider().Consignee);

				var document3 = nctsBill.AdditionalDocuments.AddNew();
				document3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				document3.CSI_Code = "30600";
				AssertNull("Consignment contains additional document with CSI_SubType INF and CSI_Code 30600", GetProvider().Consignee);
			});
		}

	public abstract void TestConsignee_RuleC0001_HeaderConsignee();

	public void TestAdditionalSupplyChainActor()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No AdditionalSupplyChainActor", 0, GetProvider().AdditionalSupplyChainActor.Count);

			nctsBill.CusSupplyChainActorReferences.AddNew();
			nctsBill.CusSupplyChainActorReferences.AddNew();
			AssertEquals("2 AdditionalSupplyChainActor", 2, GetProvider().AdditionalSupplyChainActor.Count);
			AssertEquals("AdditionalSupplyChainActor Sequence number should start with 1", 1, GetProvider().AdditionalSupplyChainActor.First().SequenceNumber);
			AssertEquals("2nd AdditionalSupplyChainActor Sequence number should be 2", 2, GetProvider().AdditionalSupplyChainActor.Last().SequenceNumber);
		});
	}

		public void TestAdditionalReference_InPhase5TransitionPeriod()
		{
			TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);

			AssertNull("Still empty REF AdditionalInfos when NotInPhase5TransitionPeriod is true", GetProvider().AdditionalReference);
		});
	}

	public void TestAdditionalReference_NotInPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionOutsidePhase5TransitionPeriod(() =>
		{
			var provider = GetProvider();
			AssertNotNull("Not null AdditionalReference 1", provider.AdditionalReference);
			Assert("Empty AdditionalReference", !provider.AdditionalReference.Any());

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

		public void TestAdditionalInformation_InPhase5TransitionPeriod()
		{
			TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalInformation);

			AssertNull(GetProvider().AdditionalInformation);
		});
	}

	public void TestAdditionalInformation_NotInPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionOutsidePhase5TransitionPeriod(() =>
		{
			var provider = GetProvider();
			AssertNotNull("Not null AdditionalInformation 1", provider.AdditionalInformation);
			Assert("Empty AdditionalInformation", !provider.AdditionalInformation.Any());

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalInformation);

			provider = GetProvider();
			AssertNotNull("Not null AdditionalInformation 2", provider.AdditionalInformation);
			AssertEquals("AdditionalInformation count == 2", 2, provider.AdditionalInformation.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", provider.AdditionalInformation.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", provider.AdditionalInformation.Last().SequenceNumber);

				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.AdditionalReference);
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);
				AddAdditionalInfosDocumentOfType(ZString.Empty);

			AssertEquals("Only INF AdditionalInfos should be included", 2, GetProvider().AdditionalInformation.Count);
		});
	}

	public void TestPreviousDocument_InPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionInPhase5TransitionPeriod(() =>
		{
			nctsBill.SupportingDocuments.AddNew();

			AssertNull(GetProvider().SupportingDocument);
		});
	}

	public void TestPreviousDocument_NotInPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			var provider = GetProvider();
			AssertNotNull("Not null PreviousDocument 1", provider.PreviousDocument);
			Assert("Empty PreviousDocument", !provider.PreviousDocument.Any());

			nctsBill.PreviousDocuments.AddNew();
			nctsBill.PreviousDocuments.AddNew();

			provider = GetProvider();
			AssertNotNull("Not null PreviousDocuments 2", provider.PreviousDocument);
			AssertEquals("PreviousDocuments count == 2", 2, provider.PreviousDocument.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", provider.PreviousDocument.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", provider.PreviousDocument.Last().SequenceNumber);
		});
	}

	public void TestSupportingDocument_InPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionInPhase5TransitionPeriod(() =>
		{
			nctsBill.PreviousDocuments.AddNew();

			AssertNull(GetProvider().PreviousDocument);
		});
	}

	public void TestSupportingDocument_NotInPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			var provider = GetProvider();
			AssertNotNull("Not null SupportingDocuments 1", provider.SupportingDocument);
			Assert("Empty SupportingDocuments", !provider.SupportingDocument.Any());

			nctsBill.SupportingDocuments.AddNew();
			nctsBill.SupportingDocuments.AddNew();

			provider = GetProvider();
			AssertNotNull("Not null SupportingDocuments 2", provider.SupportingDocument);
			AssertEquals("SupportingDocuments count == 2", 2, provider.SupportingDocument.Count);
			AssertEquals("1st item should start with sequenceNumber 1", "1", provider.SupportingDocument.First().SequenceNumber);
			AssertEquals("2nd item should have sequenceNumber 2", "2", provider.SupportingDocument.Last().SequenceNumber);
		});
	}

		public void TestTransportDocument_InPhase5TransitionPeriod()
		{
			TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AddAdditionalInfosDocumentOfType(AdditionalInfoSubTypeList.Codes.TransportDocument);

			AssertNull(GetProvider().TransportDocument);
		});
	}

	public void TestTransportDocument_NotInPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionOutsidePhase5TransitionPeriod(() =>
		{
			var provider = GetProvider();
			AssertNotNull("Not null TransportDocument 1", provider.TransportDocument);
			Assert("Empty TransportDocument", !provider.TransportDocument.Any());

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

	protected override bool IsTransportTypeAtDepartureRequired => true;

	protected override void SetUp()
	{
		base.SetUp();

		nctsBill.B0_ReferenceID = "ReferenceId";
		nctsBill.B0_Weight = 123040m;
		nctsBill.B0_WeightUQ = Core.Constants.Weight.Grams;
	}

	void AddAdditionalInfosDocumentOfType(ZString subType)
	{
		var document = nctsBill.AdditionalDocuments.AddNew();
		document.CSI_SubType = subType;
	}

	void SetUpValidAddress(JobDocAddress docAddress) => docAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
}
