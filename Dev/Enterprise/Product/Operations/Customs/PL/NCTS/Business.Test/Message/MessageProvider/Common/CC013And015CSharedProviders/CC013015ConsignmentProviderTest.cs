using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing.Message.MessageProvider.Common;

abstract class CC013015ConsignmentProviderTest<T> : ConsignmentProviderBaseTest<T> where T : ConsignmentProviderBase, IConsignment
{
	public void TestHouseConsignment_SortedBy_SequenceNumber()
	{
		nctsBill.SequenceNumber = 5;
		nctsBill.B0_Weight = new(5M);
		var bill3 = nctsHeader.Bills.AddNew();
		bill3.SequenceNumber = 3;
		bill3.B0_Weight = new(3M);
		var bill4 = nctsHeader.Bills.AddNew();
		bill4.SequenceNumber = 4;
		bill4.B0_Weight = new(4M);
		var bill1 = nctsHeader.Bills.AddNew();
		bill1.SequenceNumber = 1;
		bill1.B0_Weight = new(1M);
		var bill2 = nctsHeader.Bills.AddNew();
		bill2.SequenceNumber = 2;
		bill2.B0_Weight = new(2M);

		AssertSequencesEqual("House Consignments are sorted by SequenceNumber", [1M, 2M, 3M, 4M, 5M], GetProvider().HouseConsignment.Select(x => x.GrossMass));
	}

	public void TestCountryOfDestination()
	{
		CombineAssertions(() =>
		{
			AssertEquals(string.Empty, GetProvider().CountryOfDestination);

			movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
			AssertEquals(CountryCodes.Germany, GetProvider().CountryOfDestination);
		});
	}

	public void TestContainerIndicator()
	{
		CombineAssertions(() =>
		{
			AssertEquals(NCTSIndicator.NO, GetProvider().ContainerIndicator);

			var headerContainerIndicator1 = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainerIndicator1.BC_ContainerNum = "1";
			AssertEquals(NCTSIndicator.NO, GetProvider().ContainerIndicator);

			headerContainerIndicator1.BC_Mode = ContainerModes.Containerised;
			AssertEquals(NCTSIndicator.YES, GetProvider().ContainerIndicator);
		});
	}

	public void TestInlandModeOfTransport()
	{
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, GetProvider().InlandModeOfTransport);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			AssertEquals(ModeOfTransportList.Codes._9_OwnPropulsion, GetProvider().InlandModeOfTransport);
		});
	}

	public void TestModeOfTransportAtTheBorder()
	{
		CombineAssertions(() =>
		{
			AssertEquals(string.Empty, GetProvider().ModeOfTransportAtTheBorder);

			movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			AssertEquals(ModeOfTransportList.Codes._2_RailTransport, GetProvider().ModeOfTransportAtTheBorder);
		});
	}

	public void TestGrossMass_OutsideTransitionPeriod()
	{
		const decimal testGrossMass = 123456789123m;
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			movementHeader.BM_GrossWeight = testGrossMass;
			movementHeader.BM_GrossWeightUQ = Weight.Grams;
			AssertEquals("GrossMass in Kgs is not restricted by rules for transition period", 123456789.123m, GetProvider().GrossMass);
		});
	}

	public void TestGrossMass_InTransitionPeriod()
	{
		const decimal testGrossMass = 123456789123m;
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			movementHeader.BM_GrossWeight = testGrossMass;
			movementHeader.BM_GrossWeightUQ = Weight.Grams;
			AssertEquals("GrossMass in Kgs rounded by rules for transition period", 123456789.12m, GetProvider().GrossMass);
		});
	}

	public void TestCarrier()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(GetProvider().Carrier);
		});
	}

	public void TestConsignor()
	{
		var consignor = OrgHeader.New(Factory);
		var principal = OrgHeader.New(Factory);

		CheckConsignorRuleC0542();
		CheckConsignorRuleG0123();

		void CheckConsignorRuleC0542()
		{
			nctsHeader.Consignor.OrganisationPK = consignor.PK;
			nctsHeader.Principal.E2_OA_Address = principal.PK;

			CombineAssertions(() =>
			{
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				movementHeader.BM_ReducedDatasetIndicator = true;
				AssertNull(GetProvider().Consignor);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				movementHeader.BM_ReducedDatasetIndicator = false;
				AssertNotNull(GetProvider().Consignor);
			});
		}

		void CheckConsignorRuleG0123()
		{
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			movementHeader.BM_ReducedDatasetIndicator = false;

			CombineAssertions(() =>
			{
				nctsHeader.Principal.E2_OA_Address = consignor.PK;
				nctsHeader.Consignor.E2_OA_Address = consignor.PK;
				AssertNull(GetProvider().Consignor);

				nctsHeader.Principal.E2_OA_Address = principal.PK;
				nctsHeader.Consignor.E2_OA_Address = consignor.PK;
				AssertNotNull(GetProvider().Consignor);
			});
		}
	}

	public void TestTransportEquipment()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty List", 0, GetProvider().TransportEquipment.Count);

			nctsHeader.DepartureHeaderContainers.AddNew();
			nctsHeader.DepartureHeaderContainers.AddNew();
			AssertEquals("2 HeaderContainers", 2, GetProvider().TransportEquipment.Count);
			AssertEquals("1st HeaderContainer sequenceNumber", "1", GetProvider().TransportEquipment.First().SequenceNumber);
			AssertEquals("2nd HeaderContainer sequenceNumber", "2", GetProvider().TransportEquipment.Last().SequenceNumber);
		});
	}

	public void TestLocationOfGoods() => AssertNotNull(GetProvider().LocationOfGoods);

	public void TestCountryOfRoutingOfConsignment()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty List", 0, GetProvider().CountryOfRoutingOfConsignment.Count);

			nctsHeader.CountriesOfRouting.AddNew();
			nctsHeader.CountriesOfRouting.AddNew();
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals("2 countries of routing exist", 2, GetProvider().CountryOfRoutingOfConsignment.Count);
			AssertEquals("1st country sequenceNumber", "1", GetProvider().CountryOfRoutingOfConsignment.First().SequenceNumber);
			AssertEquals("2nd country sequenceNumber", "2", GetProvider().CountryOfRoutingOfConsignment.Last().SequenceNumber);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals("NON BM_TypeOfSecurity", 0, GetProvider().CountryOfRoutingOfConsignment.Count);
		});
	}

	public void TestPlaceOfLoading()
	{
		var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco.RL_PortName = "Port Name";

		CombineAssertions(() =>
		{
			TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(combineAssertions: false, () =>
			{
				AssertNull("No PlaceOfLoading Outside Phase 5", GetProvider().PlaceOfLoading);

				movementHeader.BM_PortOfPresentationCode = unloco.Code;
				AssertNotNull("PlaceOfLoading Outside Phase 5", GetProvider().PlaceOfLoading);

				movementHeader.BM_PortOfPresentationCode = ZString.Empty;
				movementHeader.BM_PlaceOfLoading = "asd";
				AssertNotNull("PlaceOfLoading Outside Phase 5", GetProvider().PlaceOfLoading);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertNotNull("NON BM_TypeOfSecurity Outside Phase 5", GetProvider().PlaceOfLoading);
			});

			movementHeader.BM_PlaceOfLoading = ZString.Empty;
			movementHeader.BM_PortOfPresentationCode = ZString.Empty;
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			TestHelper.RunAssertionsInPhase5TransitionPeriod(combineAssertions: false, () =>
			{
				AssertNull("No PlaceOfLoading In Phase 5", GetProvider().PlaceOfLoading);

				movementHeader.BM_PortOfPresentationCode = unloco.Code;
				AssertNotNull("BM_PortOfPresentationCode PlaceOfLoading In Phase 5", GetProvider().PlaceOfLoading);

				movementHeader.BM_PortOfPresentationCode = ZString.Empty;
				movementHeader.BM_PlaceOfLoading = "asd";
				AssertNotNull("PlaceOfLoading In Phase 5", GetProvider().PlaceOfLoading);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertNull("NON BM_TypeOfSecurity In Phase 5", GetProvider().PlaceOfLoading);
			});
		});
	}

	public void TestPlaceOfUnloading()
	{
		var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco.RL_PortName = "Port Name";
		movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;

		CombineAssertions(() =>
		{
			TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(combineAssertions: false, () =>
			{
				AssertNull("No PlaceOfUnloading Outside Phase 5", GetProvider().PlaceOfUnloading);

				movementHeader.BM_ForeignDestPortKCode = unloco.Code;
				AssertNotNull("BM_ForeignDestPortKCode PlaceOfUnloading Outside Phase 5", GetProvider().PlaceOfUnloading);

				movementHeader.BM_ForeignDestPortKCode = ZString.Empty;
				movementHeader.BM_PlaceOfUnloading = "123";
				AssertNotNull("BM_PlaceOfUnloading PlaceOfUnloading Outside Phase 5", GetProvider().PlaceOfUnloading);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertNull("NON BM_TypeOfSecurity Outside Phase 5", GetProvider().PlaceOfUnloading);
			});

			movementHeader.BM_PlaceOfUnloading = ZString.Empty;
			movementHeader.BM_ForeignDestPortKCode = ZString.Empty;
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			TestHelper.RunAssertionsInPhase5TransitionPeriod(combineAssertions: false, () =>
			{
				AssertNull("No PlaceOfUnloading In Phase 5", GetProvider().PlaceOfUnloading);

				movementHeader.BM_ForeignDestPortKCode = unloco.Code;
				AssertNotNull("BM_ForeignDestPortKCode PlaceOfUnloading In Phase 5", GetProvider().PlaceOfUnloading);

				movementHeader.BM_ForeignDestPortKCode = ZString.Empty;
				movementHeader.BM_PlaceOfUnloading = "123";
				AssertNotNull("BM_PlaceOfUnloading PlaceOfUnloading In Phase 5", GetProvider().PlaceOfUnloading);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertNull("NON BM_TypeOfSecurity In Phase 5", GetProvider().PlaceOfUnloading);
			});
		});
	}

	public void TestCountryOfDispatch()
	{
		movementHeader.BM_RN_NKCountryOfDispatch = CountryCodes.Poland;

		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			AssertEquals("When inbond entry type is TIR, Country of dispatch is required", CountryCodes.Poland, GetProvider().CountryOfDispatch);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
			AssertEquals("Country of dispatch is required", CountryCodes.Poland, GetProvider().CountryOfDispatch);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("When application code is NCTS4, Country of dispatch is required", CountryCodes.Poland, GetProvider().CountryOfDispatch);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			AssertEquals("When inbond entry type is TIR, Country of dispatch is required", CountryCodes.Poland, GetProvider().CountryOfDispatch);
		});
	}

	public void TestReferenceNumberUCR()
	{
		const string testReferenceNumberUCR = "dejavu";

		CombineAssertions(() =>
		{
			AssertEquals(string.Empty, GetProvider().ReferenceNumberUCR);

			movementHeader.BM_UniqueConsignmentReference = testReferenceNumberUCR;
			AssertEquals(testReferenceNumberUCR, GetProvider().ReferenceNumberUCR);
		});
	}

	public void TestDepartureTransportMeansNationality_B1897()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		SetTransportAtDeparture("A");
		SetTrailer1IDAtDeparture("B");
		movementHeader.BM_RN_NKTransportAtDepartureCountry = "PL";
		movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = "PL";

		NctsConfigurationTestHelper.RunAssertionsInAndOutPhase5TransitionPeriod(
			insidePhase5: () =>
			{
				var provider = GetProvider();
				AssertEquals("Inside transit period, count", 1, provider.DepartureTransportMeans.Count);
				var transportAtDeparture = provider.DepartureTransportMeans.Cast<DepartureTransportMeansProvider>().First();
				AssertEquals("In transit period, nationality", null, transportAtDeparture.Nationality);
			},
			outsidePhase5: () =>
			{
				var provider = GetProvider();
				AssertEquals("Out of the transit period, count", 1, provider.DepartureTransportMeans.Count);
				var transportAtDeparture = provider.DepartureTransportMeans.Cast<DepartureTransportMeansProvider>().First();
				AssertNotNull("Out of the transit period, nationality", transportAtDeparture.Nationality);
			});
	}

	public void TestConsignee()
	{
		var additionalDocuments = nctsHeader.AdditionalDocuments.AddNew();
		nctsHeader.Consignee.OrganisationPK = Factory.New<OrgHeader>().PK;

			CombineAssertions(() =>
			{
				additionalDocuments.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocuments.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				additionalDocuments.CSI_Code = AdditionalInfoCodes._30600;
				AssertNull(GetProvider().Consignee);

				additionalDocuments.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocuments.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				additionalDocuments.CSI_Code = AdditionalInfoCodes._30600;
				AssertNotNull(GetProvider().Consignee);

				additionalDocuments.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocuments.CSI_Type = CusSupportingInfoTypeList.Codes.RequestHeader;
				additionalDocuments.CSI_Code = AdditionalInfoCodes._30600;
				AssertNotNull(GetProvider().Consignee);

				additionalDocuments.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocuments.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				additionalDocuments.CSI_Code = AdditionalInfoCodes._0PL10;
				AssertNotNull(GetProvider().Consignee);
			});
		}

	public virtual void TestAdditionalReference()
	{
		CombineAssertions(() =>
		{
			TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(combineAssertions: false, () =>
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

			TestHelper.RunAssertionInPhase5TransitionPeriod(() =>
			{
				AssertEquals("Inside Phase 5 Transition Period", 0, GetProvider().AdditionalReference.Count);
			});
		});
	}

	public virtual void TestAdditionalInformation()
	{
		CombineAssertions(() =>
		{
			TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(combineAssertions: false, () =>
			{
				AssertEquals("Empty INF AdditionalInfos", 0, Provider.AdditionalInformation.Count);

				var doc1 = nctsHeader.AdditionalDocuments.AddNew();
				doc1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				doc1.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				doc1.CSI_Code = "INFO01";

				var doc2 = nctsHeader.AdditionalDocuments.AddNew();
				doc2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				doc2.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				doc2.CSI_Code = "INFO02";

				AssertEquals("Should include 2 additional info documents", 2, GetProvider().AdditionalInformation.Count);
				AssertEquals("1st item should have sequenceNumber 1", "1", GetProvider().AdditionalInformation.First().SequenceNumber);
				AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().AdditionalInformation.Last().SequenceNumber);

				var docPOW = nctsHeader.AdditionalDocuments.AddNew();
				docPOW.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				docPOW.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				docPOW.CSI_Code = "POW01";

				var docPCS = nctsHeader.AdditionalDocuments.AddNew();
				docPCS.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				docPCS.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				docPCS.CSI_Code = "PCS01";

				var additionalInfos = GetProvider().AdditionalInformation;
				AssertEquals("Should include only one special code", 3, additionalInfos.Count);
				AssertEquals("Should include either POW01 or PCS01 but not both",
					true,
					additionalInfos.Any(x => x.Code == "POW01") ^ additionalInfos.Any(x => x.Code == "PCS01"));

				var docRef = nctsHeader.AdditionalDocuments.AddNew();
				docRef.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				docRef.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				docRef.CSI_Code = "REF01";

				var docTransport = nctsHeader.AdditionalDocuments.AddNew();
				docTransport.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				docTransport.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				docTransport.CSI_Code = "TRA01";

				var docEmpty = nctsHeader.AdditionalDocuments.AddNew();
				docEmpty.CSI_SubType = ZString.Empty;
				docEmpty.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				docEmpty.CSI_Code = "EMPTY";

				AssertEquals("Should still only include 3 additional info documents", 3, GetProvider().AdditionalInformation.Count);
			});

			TestHelper.RunAssertionInPhase5TransitionPeriod(() =>
			{
				AssertEquals("Inside Phase 5 Transition Period", 0, GetProvider().AdditionalInformation.Count);
			});
		});
	}

	public virtual void TestPreviousDocument()
	{
		CombineAssertions(() =>
		{
			TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(combineAssertions: false, () =>
			{
				AssertEquals("Empty PreviousDocuments", 0, Provider.PreviousDocument.Count);

				nctsHeader.PreviousDocuments.AddNew();
				nctsHeader.PreviousDocuments.AddNew();

				AssertEquals("Not empty PreviousDocuments", 2, GetProvider().PreviousDocument.Count);
				AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().PreviousDocument.First().SequenceNumber);
				AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().PreviousDocument.Last().SequenceNumber);
			});

			TestHelper.RunAssertionInPhase5TransitionPeriod(() =>
			{
				AssertEquals("Inside Phase 5 Transition Period", 0, GetProvider().PreviousDocument.Count);
			});
		});
	}

	public virtual void TestSupportingDocument()
	{
		CombineAssertions(() =>
		{
			TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(combineAssertions: false, () =>
			{
				AssertEquals("Empty SupportingDocuments", 0, Provider.SupportingDocument.Count);

				nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				nctsHeader.MovementHeader.SupportingDocuments.AddNew();

				AssertEquals("Not empty SupportingDocuments", 2, GetProvider().SupportingDocument.Count);
				AssertEquals("1st item should start with sequenceNumber 1", "1", GetProvider().SupportingDocument.First().SequenceNumber);
				AssertEquals("2nd item should have sequenceNumber 2", "2", GetProvider().SupportingDocument.Last().SequenceNumber);
			});

			TestHelper.RunAssertionInPhase5TransitionPeriod(() =>
			{
				AssertEquals("Inside Phase 5 Transition Period", 0, GetProvider().SupportingDocument.Count);
			});
		});
	}

	public virtual void TestTransportDocument()
	{
		CombineAssertions(() =>
		{
			TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(combineAssertions: false, () =>
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

			TestHelper.RunAssertionInPhase5TransitionPeriod(() =>
			{
				AssertEquals("Inside Phase 5 Transition Period", 0, GetProvider().TransportDocument.Count);
			});
		});
	}

	public virtual void TestTransportCharges()
	{
		movementHeader.BM_MethodOfPayment = "A";
		CombineAssertions(() =>
		{
			AssertEquals("Not NON security", "A", GetProvider().TransportCharges);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals("NON Security", string.Empty, GetProvider().TransportCharges);
		});
	}

	protected override void SetTransportTypeAtDeparture(string value) => movementHeader.TransportTypeAtDeparture = value;

	protected override void SetTransportAtDeparture(string transport) => movementHeader.TransportAtDeparture = transport;

	protected override void SetTrailer1IDAtDeparture(string value) => movementHeader.Trailer1IDAtDeparture = value;

	protected override void SetTrailer2IDAtDeparture(string value) => movementHeader.Trailer2IDAtDeparture = value;

	protected override void SetVesselNameAtDeparture(string value) => movementHeader.VesselNameAtDeparture = value;

	protected readonly string[] AllMeansOfTransportExcept_5_PostalConsignment = {
		ModeOfTransportList.Codes._1_SeaTransport,
		ModeOfTransportList.Codes._2_RailTransport,
		ModeOfTransportList.Codes._3_RoadTransport,
		ModeOfTransportList.Codes._4_AirTransport,
		ModeOfTransportList.Codes._7_FixedTransportInstallations,
		ModeOfTransportList.Codes._8_InlandWaterwayTransport
	};

	protected void AddAdditionalInfosDocumentOfType(ZString subType) => nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = subType;
}
