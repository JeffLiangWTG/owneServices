using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(ConsignmentProvider))]
sealed class ConsignmentProviderBaseOnlyTest : ConsignmentProviderAbstractTest<ConsignmentProvider>
{
	public void TestCountryOfDispatch()
	{
		nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Belgium;
		AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.CountryOfDispatch);
	}

	public void TestCountryOfDestination()
	{
		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = "NL";
		AssertEquals("NL", Provider.CountryOfDestination);
	}

	public void TestContainerIndicator()
	{
		nctsHeader.DepartureHeaderContainers.AddNew();
		AssertEquals(true, Provider.ContainerIndicator);
	}

	public void TestInlandModeOfTransport()
	{
		nctsHeader.MovementHeader.BM_InlandTransportMode = "1";
		AssertEquals(1, Provider.InlandModeOfTransport);
	}

	public void TestModeOfTransportAtTheBorder()
	{
		nctsHeader.MovementHeader.BM_ExportTransportMode = "2";
		AssertEquals(2, Provider.ModeOfTransportAtTheBorder);
	}

	public void TestGrossMass()
	{
		CombineAssertions(() =>
		{
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_GrossWeight = 10.0000m;
			AssertEquals("BY_GrossWeight = 10.0000", "10", new ConsignmentProvider(nctsHeader).GrossMass.ToString());
			movementHeader.BM_GrossWeight = 10.2000m;
			AssertEquals("BY_GrossWeight = 10.2000", "10.2", new ConsignmentProvider(nctsHeader).GrossMass.ToString());
			movementHeader.BM_GrossWeight = 10.2750m;
			AssertEquals("BY_GrossWeight = 10.2750", "10.275", new ConsignmentProvider(nctsHeader).GrossMass.ToString());
		});
	}

	public void TestReferenceNumberUCR()
	{
		nctsHeader.MovementHeader.BM_UniqueConsignmentReference = "text";
		AssertEquals("text", Provider.ReferenceNumberUCR);
	}

	public void TestTransportEquipments()
	{
		nctsHeader.DepartureHeaderContainers.AddNew();
		AssertEquals(1, Provider.TransportEquipments.Count);
	}

	public void TestLocationOfGoods()
	{
		AssertNotNull(Provider.LocationOfGoods);
	}

	public void TestDepartureTransportMeans()
	{
		AssertNotNull(Provider.DepartureTransportMeans);
	}

	public void TestCountryOfRoutingOfConsignments()
	{
		nctsHeader.CountriesOfRouting.AddNew().CY_Data = Core.Constants.CountryCodes.Germany;
		nctsHeader.CountriesOfRouting.AddNew().CY_Data = Core.Constants.CountryCodes.Australia;
		AssertContainsExactElementsInAnyOrder(new[] { (1, "DE"), (2, "AU") }, Provider.CountryOfRoutingOfConsignments.Select(x => (x.SequenceNumeric, x.Country)));
	}

	public void TestActiveBorderTransportMeans()
	{
		AssertEquals("Mandatory fields are empty", 0, Provider.ActiveBorderTransportMeans.Count);

		nctsHeader.MovementHeader.BM_ActiveBorderIdentificationType = "A";
		nctsHeader.MovementHeader.BM_TOLCarrierID = "ABC123";
		nctsHeader.MovementHeader.BM_RN_NKTOLCarrierNationality = "ES";
		AssertEquals("Mandatory fields populated", 1, new ConsignmentProvider(nctsHeader).ActiveBorderTransportMeans.Count);
	}

	public void TestPlaceOfLoading()
	{
		AssertNotNull(Provider.PlaceOfLoading);
	}

	public void TestPlaceOfUnloading()
	{
		AssertNotNull(Provider.PlaceOfUnloading);
	}

	public void TestCarrier()
	{
		var carrier = nctsHeader.MovementHeader.DocAddresses.AddNew();
		carrier.E2_AddressType = AutoDocAddressTypes.Codes.Carrier;
		AssertNotNull(Provider.Carrier);
	}

	public void TestConsignor()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(Provider.Consignor);
			AssertType<OmitNameAndAddressPartyProvider>(Provider.Consignor);
		});
	}

	public void TestConsignee()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(Provider.Consignee);
			AssertType<OmitNameAndAddressPartyProvider>(Provider.Consignee);
		});
	}

	public void TestAdditionalSupplyChainActors()
	{
		Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "SCA", "AddSupID", nctsHeader.MovementHeader);
		Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "SCA", "AddSupID", nctsHeader.MovementHeader);

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, Provider.AdditionalSupplyChainActors.Count);
			var first = Provider.AdditionalSupplyChainActors.First();
			AssertEquals("1st IdentificationNumber", "AddSupID", first.Id);
			AssertEquals("1st Role", "FR1", first.Role);
			AssertEquals("1st SequenceNumber", 1, first.SequenceNumeric);
			AssertEquals("2nd SequenceNumber", 2, Provider.AdditionalSupplyChainActors.ElementAt(1).SequenceNumeric);
		});
	}

	public void TestPreviousDocuments()
	{
		nctsHeader.PreviousDocuments.AddNew();
		AssertEquals("Count", 1, Provider.PreviousDocuments.Count);
	}

	public void TestSupportingDocuments()
	{
		nctsHeader.MovementHeader.SupportingDocuments.AddNew();
		AssertEquals("Count", 1, Provider.SupportingDocuments.Count);
	}

	public void TestTransportDocuments()
	{
		var additionalDocument = nctsHeader.AdditionalDocuments.AddNew();
		additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		AssertEquals("Count", 1, Provider.TransportDocuments.Count);
	}

	public void TestAdditionalReferences()
	{
		var additionalDocument = nctsHeader.AdditionalDocuments.AddNew();
		additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		AssertEquals("Count", 1, Provider.AdditionalReferences.Count);
	}

	public void TestAdditionalInformations()
	{
		var additionalDocument = nctsHeader.AdditionalDocuments.AddNew();
		additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		AssertEquals("Count", 1, Provider.AdditionalInformations.Count);
	}

	public void TestHouseConsignments()
	{
		var bill1 = Factory.New<NctsBill>();
		bill1.B0_BH = nctsHeader.PK;
		nctsHeader.Bills.AddNew();
		var bill2 = Factory.New<NctsBill>();
		bill2.B0_BH = nctsHeader.PK;
		nctsHeader.Bills.AddNew();
		nctsHeader.Bills.Delete(bill1);

		CombineAssertions(() =>
		{
			AssertEquals("Count", 3, Provider.HouseConsignments.Count);
			AssertContainsExactElementsInExactOrder("Sequence", new List<int> { 1, 2, 3 }, Provider.HouseConsignments.Select(x => x.SequenceNumeric).ToList());
		});
	}

	public void TestTransportChargesMethodOfPayment()
	{
		AssertNullOrEmpty(Provider.TransportChargesMethodOfPayment);
	}

	public void TestIncidents() => CombineAssertions(() =>
	{
		AssertEquals("No incidents on Departure", 0, Provider.Incidents.Count);

		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		var incident = nctsHeader.EnRouteIncidents.AddNew();
		incident.BN_IncidentCode = "2";
		provider = CreateProvider(nctsHeader);
		AssertEquals("1 incident on Arrival", 1, provider.Incidents.Count);
	});

	protected override ConsignmentProvider CreateProvider(NctsHeader nctsHeader) => new ConsignmentProvider(nctsHeader);
}
