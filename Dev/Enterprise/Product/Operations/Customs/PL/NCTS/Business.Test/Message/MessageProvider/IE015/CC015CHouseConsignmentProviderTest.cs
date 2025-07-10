using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC015CHouseConsignmentProviderTest : CC013015HouseConsignmentProviderTest<CC015CHouseConsignmentProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsBill", "Value cannot be null.\r\nParameter name: transportMeansProvider", () => new CC015CHouseConsignmentProvider(1, null, null));
			AssertExceptionThrown<ArgumentNullException>("Null ICC015C", "Value cannot be null.\r\nParameter name: cc015cProvider", () => new CC015CHouseConsignmentProvider(1, nctsBill, null));
		});
	}

	public void TestGrossMass() => AssertEquals(123.04m, GetProvider().GrossMass);

	public void TestConsignor_InPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			AssertNull("No Consignor", GetProvider().Consignor);

			SetUpValidAddress(nctsBill.Consignor);
			SetUpValidC0542Data();

			AssertNull("Valid Consignor but in phase5 transition period", GetProvider().Consignor);
		});
	}

	public void TestConsignor_OutsidePhase5TransitionPeriod()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			AssertNull("No Consignor", GetProvider().Consignor);

			SetUpValidAddress(nctsBill.Consignor);
			SetUpValidC0542Data();

			AssertNotNull("Valid Consignor", GetProvider().Consignor);
		});
	}

	public void TestConsignor_RuleC0542()
	{
		SetUpValidAddress(nctsBill.Consignor);
		CombineAssertions(() =>
		{
			SetUpTransitOperationData(NctsTypeOfSecurityList.Codes.NON, NCTSIndicator.NO);
			AssertNull("Security is NON", GetProvider().Consignor);

			SetUpTransitOperationData(NctsTypeOfSecurityList.Codes.ENT, NCTSIndicator.YES);
			AssertNull("Indicator is Item1", GetProvider().Consignor);

			SetUpTransitOperationData(NctsTypeOfSecurityList.Codes.NON, NCTSIndicator.YES);
			AssertNull("Security is NON and Indicator is Item1", GetProvider().Consignor);

			SetUpTransitOperationData(NctsTypeOfSecurityList.Codes.ENT, NCTSIndicator.NO);
			AssertNotNull("Security and Indicator are valid", GetProvider().Consignor);
		});
	}

	public void TestConsignor_RuleG0123()
	{
		SetUpValidC0542Data();

		var organisation = Factory.New<OrgHeader>();
		var anotherOrganisation = Factory.New<OrgHeader>();
		var anotherNctsBill = nctsHeader.Bills.AddNew();

		CombineAssertions(() =>
		{
			nctsHeader.Principal.E2_OA_Address = organisation.PK;
			nctsBill.Consignor.OrganisationPK = organisation.PK;
			anotherNctsBill.Consignor.OrganisationPK = organisation.PK;
			AssertNull("Principal has the same address as ALL consignors", GetProvider().Consignor);

			nctsHeader.Principal.E2_OA_Address = anotherNctsBill.PK;
			AssertNotNull("Principal has a different address", GetProvider().Consignor);

			nctsHeader.Principal.E2_OA_Address = organisation.PK;
			nctsBill.Consignor.OrganisationPK = anotherOrganisation.PK;
			AssertNotNull("Consignor has a different address", GetProvider().Consignor);

			nctsBill.Consignor.OrganisationPK = organisation.PK;
			anotherNctsBill.Consignor.OrganisationPK = anotherOrganisation.PK;
			AssertNotNull("Another consignor has a different address", GetProvider().Consignor);
		});
	}

	public void TestCountryOfDispatch()
	{
		nctsBill.B0_RN_NKCountryOfExport = "AA";
		AssertEquals("OutsidePhase5TransitionPeriod", "AA", GetProvider().CountryOfDispatch);
	}

	public void TestReferenceNumberUCR()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			AssertEquals("OutsidePhase5TransitionPeriod", "ReferenceId", GetProvider().ReferenceNumberUCR);
		});

		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			AssertEquals("InPhase5TransitionPeriod", ZString.Empty, GetProvider().ReferenceNumberUCR);
		});
	}

	public void TestTransportCharges_InPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			nctsBill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cheque;

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertNull($"BM_TypeOfSecurity is {NctsTypeOfSecurityList.Codes.NON}", GetProvider().TransportCharges);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertNull($"BM_TypeOfSecurity is {NctsTypeOfSecurityList.Codes.BTH}", GetProvider().TransportCharges);
		});
	}

	public void TestTransportCharges_NotInPhase5TransitionPeriod()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			nctsBill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cheque;

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertNull($"BM_TypeOfSecurity is {NctsTypeOfSecurityList.Codes.NON}", GetProvider().TransportCharges);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertEquals($"BM_TypeOfSecurity is {NctsTypeOfSecurityList.Codes.BTH}", TransportChargesModeOfPayment.Codes.Cheque, GetProvider().TransportCharges);
		});
	}

	public override void TestConsignee_RuleC0001_HeaderAdditionalDocument()
	{
		SetUpValidAddress(nctsBill.Consignee);

			CombineAssertions(() =>
			{
				var document1 = nctsHeader.AdditionalDocuments.AddNew();
				document1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				document1.CSI_Code = "30600";
				AssertNotNull("Header contains additional document with CSI_SubType <> INF", GetProvider().Consignee);

				var document2 = nctsHeader.AdditionalDocuments.AddNew();
				document2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				document2.CSI_Code = "10600";
				AssertNotNull("Header contains additional document with CSI_Code <> 30600", GetProvider().Consignee);

				var document3 = nctsHeader.AdditionalDocuments.AddNew();
				document3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				document3.CSI_Code = "30600";
				AssertNull("Header contains additional document with CSI_SubType INF and CSI_Code 30600", GetProvider().Consignee);
			});
		}

	public override void TestConsignee_RuleC0001_HeaderConsignee()
	{
		SetUpValidAddress(nctsBill.Consignee);

		CombineAssertions(() =>
		{
			nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
			AssertNotNull("Header consignee is not defined", GetProvider().Consignee);

			SetUpValidAddress(nctsHeader.Consignee);
			AssertNull("Header consignee is defined", GetProvider().Consignee);
		});
	}

	protected override CC015CHouseConsignmentProvider GetProvider() => new CC015CHouseConsignmentProvider(99, nctsBill, GetCC015CProvider());

	ICC015C GetCC015CProvider() => new CC015CProvider(movementHeader, messageType);

	void SetUpValidAddress(JobDocAddress docAddress) => docAddress.OrganisationPK = Factory.New<OrgHeader>().PK;

	void SetUpValidC0542Data() => SetUpTransitOperationData(NctsTypeOfSecurityList.Codes.ENT, NCTSIndicator.NO);

	void SetUpTransitOperationData(string security, NCTSIndicator indicator)
	{
		movementHeader.BM_TypeOfSecurity = security;
		movementHeader.BM_ReducedDatasetIndicator = indicator == NCTSIndicator.YES;
	}

	protected override void SetUp()
	{
		base.SetUp();

		messageType = "Type";
		nctsBill.B0_ReferenceID = "ReferenceId";
		nctsBill.B0_Weight = 123040m;
		nctsBill.B0_WeightUQ = Core.Constants.Weight.Grams;
	}

	string messageType;
}
