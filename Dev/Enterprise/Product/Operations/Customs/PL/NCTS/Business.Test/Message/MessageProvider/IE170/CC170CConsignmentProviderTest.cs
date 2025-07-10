using System;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC170CConsignmentProviderTest : ConsignmentProviderBaseTest<CC170CConsignmentProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: transportMeansProvider", () => new CC170CConsignmentProvider(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: NctsDepartureMovementHeader.Header", () => new CC170CConsignmentProvider(Factory.New<NctsDepartureMovementHeader>(), null));
			AssertExceptionThrown<ArgumentNullException>("Null ICC170C", "Value cannot be null.\r\nParameter name: rootProvider", () => new CC170CConsignmentProvider(movementHeader, null));
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

			headerContainerIndicator1.BC_Mode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(NCTSIndicator.YES, GetProvider().ContainerIndicator);
		});
	}

	public void TestInlandModeOfTransport()
	{
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, GetProvider().InlandModeOfTransport);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals(ModeOfTransportList.Codes._4_AirTransport, GetProvider().InlandModeOfTransport);
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

	public void TestLocationOfGoods() => AssertNotNull(Provider.LocationOfGoods);

	public void TestActiveBorderTransportMeans() => CombineAssertions(() =>
	{
		movementHeader.BM_CustomsOfficeAtBorder = "ABC";
		movementHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
		movementHeader.BM_TOLCarrierID = "DFF";
		movementHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Poland;
		movementHeader.BM_ConveyanceNumber = "S7";

		var additionalTransport1 = movementHeader.AdditionalTransportAtBorderList.AddNew();
		{
			additionalTransport1.TPM_CustomsOffice = "DEF";
			additionalTransport1.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._11;
			additionalTransport1.TPM_IdentificationNumber = "CCD";
			additionalTransport1.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Albania;
			additionalTransport1.TPM_ReferenceNumber = "E5";
		}

		var additionalTransport2 = movementHeader.AdditionalTransportAtBorderList.AddNew();
		{
			additionalTransport2.TPM_CustomsOffice = "TTR";
			additionalTransport2.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._31;
			additionalTransport2.TPM_IdentificationNumber = "ETY";
			additionalTransport2.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.CzechRepublic;
			additionalTransport2.TPM_ReferenceNumber = "D5";
		}

		var activeBorderTransportMeans = GetProvider().ActiveBorderTransportMeans;
		AssertEquals(3, activeBorderTransportMeans.Count);

		AssertActiveBorderTransportMeans("Main",
			activeBorderTransportMeans.First(),
			expectedSequenceNumber: "1",
			expectedCustomsOfficeAtBorderReferenceNumber: movementHeader.BM_CustomsOfficeAtBorder,
			expectedTypeOfIdentification: movementHeader.BM_ActiveBorderIdentificationType,
			expectedIdentificationNumber: movementHeader.BM_TOLCarrierID,
			expectedNationality: movementHeader.BM_RN_NKTOLCarrierNationality,
			expectedConveyanceReferenceNumber: movementHeader.BM_ConveyanceNumber);

		AssertActiveBorderTransportMeans("additionalTransport1",
			activeBorderTransportMeans.Skip(1).First(),
			expectedSequenceNumber: "2",
			expectedCustomsOfficeAtBorderReferenceNumber: additionalTransport1.TPM_CustomsOffice,
			expectedTypeOfIdentification: additionalTransport1.TPM_TypeOfIdentification,
			expectedIdentificationNumber: additionalTransport1.TPM_IdentificationNumber,
			expectedNationality: additionalTransport1.TPM_RN_NKTransportNationality,
			expectedConveyanceReferenceNumber: additionalTransport1.TPM_ReferenceNumber);

		AssertActiveBorderTransportMeans("additionalTransport2",
			activeBorderTransportMeans.Skip(2).First(),
			expectedSequenceNumber: "3",
			expectedCustomsOfficeAtBorderReferenceNumber: additionalTransport2.TPM_CustomsOffice,
			expectedTypeOfIdentification: additionalTransport2.TPM_TypeOfIdentification,
			expectedIdentificationNumber: additionalTransport2.TPM_IdentificationNumber,
			expectedNationality: additionalTransport2.TPM_RN_NKTransportNationality,
			expectedConveyanceReferenceNumber: additionalTransport2.TPM_ReferenceNumber);

		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		AssertNull("Null", GetProvider().ActiveBorderTransportMeans);

		void AssertActiveBorderTransportMeans(
			string description,
			IActiveBorderTransportMeansType provider,
			ZString expectedSequenceNumber,
			ZString expectedCustomsOfficeAtBorderReferenceNumber,
			ZString expectedTypeOfIdentification,
			ZString expectedIdentificationNumber,
			ZString expectedNationality,
			ZString expectedConveyanceReferenceNumber)
		{
			AssertEquals(description + " SequenceNumber", expectedSequenceNumber, provider.SequenceNumber);
			AssertEquals(description + " CustomsOfficeAtBorderReferenceNumber", expectedCustomsOfficeAtBorderReferenceNumber, provider.CustomsOfficeAtBorderReferenceNumber);
			AssertEquals(description + " TypeOfIdentification", expectedTypeOfIdentification, provider.TypeOfIdentification);
			AssertEquals(description + " IdentificationNumber", expectedIdentificationNumber, provider.IdentificationNumber);
			AssertEquals(description + " Nationality", expectedNationality, provider.Nationality);
			AssertEquals(description + " ConveyanceReferenceNumber", expectedConveyanceReferenceNumber, provider.ConveyanceReferenceNumber);
		}
	});

	public void TestPlaceOfLoading()
	{
		var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco.RL_PortName = "Port Name";
		var movementHeader = nctsHeader.MovementHeader;

		CombineAssertions(() =>
		{
			AssertNull("PlaceOfLoading is null when BM_PortOfPresentationCode is empty", GetProvider().PlaceOfLoading);

			movementHeader.BM_PortOfPresentationCode = unloco.Code;
			AssertNotNull("PlaceOfLoading is not null when BM_PortOfPresentationCode is defined", GetProvider().PlaceOfLoading);

			movementHeader.BM_PortOfPresentationCode = ZString.Empty;
			movementHeader.BM_PlaceOfLoading = "as";
			AssertNotNull("PlaceOfLoading is not null when BM_PlaceOfLoading is defined", GetProvider().PlaceOfLoading);
		});
	}

	public void TestHouseConsignment()
	{
		nctsHeader.Bills.DeleteAll();
		nctsHeader.Bills.AddNew();
		nctsHeader.Bills.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Should have 2 HouseConsignments", 2, GetProvider().HouseConsignment.Count);
			AssertEquals("Sequence number should start from 1", "1", GetProvider().HouseConsignment.First().SequenceNumber);
			AssertEquals("Sequence number for 2nd HouseConsignments should be 2", "2", GetProvider().HouseConsignment.Last().SequenceNumber);
		});
	}

	ICC170C GetCC170CProvider() => new CC170CProvider(movementHeader, "CC170C");

	protected override CC170CConsignmentProvider GetProvider() => new CC170CConsignmentProvider(movementHeader, GetCC170CProvider());

	protected override void SetTransportTypeAtDeparture(string value) => movementHeader.TransportTypeAtDeparture = value;

	protected override void SetTransportAtDeparture(string transport) => movementHeader.TransportAtDeparture = transport;

	protected override void SetTrailer1IDAtDeparture(string value) => movementHeader.Trailer1IDAtDeparture = value;

	protected override void SetTrailer2IDAtDeparture(string value) => movementHeader.Trailer2IDAtDeparture = value;

	protected override void SetVesselNameAtDeparture(string value) => movementHeader.VesselNameAtDeparture = value;
}
