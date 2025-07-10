using System;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer.Universal.CO2e;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using CO2eTestHelper = Enterprise.Freight.DataTransfer.Universal.Testing.CO2eTestHelper;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

public class RelatedConsolCO2eRequestDataObjectWriterTest : BaseFreightTest
{
	public void TestPopulateLegs_PopulatesFirstAndLastLegs()
	{
		// Arrange
		var shipmentBO = Factory.New<ForwardingShipment>();
		var consolBO = shipmentBO.Consols.AddNew();
		consolBO.JK_RL_NKLoadPort = "AUSYD";
		consolBO.JK_RL_NKDischargePort = "VNVNH";
		consolBO.Transports[0].JW_RL_NKLoadPort = "AUMEL";
		consolBO.Transports[0].JW_RL_NKDiscPort = "VNVPC";
		consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;

		using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			// Act
			var writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, null, ZString.Empty, ZString.Empty, false, false, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertNull("PreCarriageShipmentCollection", consolData.PreCarriageShipmentCollection);
			AssertNull("PostCarriageShipmentCollection", consolData.PostCarriageShipmentCollection);
			AssertEquals("TransportLegCollection", 3, consolData.TransportLegCollection.Count);

			var firstLegDO = consolData.TransportLegCollection[0];
			AssertEquals("First leg: starts from C: 1st Load", "AUSYD", firstLegDO.PortOfLoading?.Code);
			AssertEquals("First leg: ends at C: Load", "AUMEL", firstLegDO.PortOfDischarge?.Code);

			var lastLegDO = consolData.TransportLegCollection[2];
			AssertEquals("Last leg: starts from C: Disc.", "VNVPC", lastLegDO.PortOfLoading?.Code);
			AssertEquals("Last leg: ends at C: Last Disc.", "VNVNH", lastLegDO.PortOfDischarge?.Code);
		}
	}

	public void TestPopulateLegs_PopulatesFirstAndLastLegs_IfHasTransportBooking()
	{
		// Arrange
		var shipmentBO = Factory.New<ForwardingShipment>();
		var consolBO = shipmentBO.Consols.AddNew();
		consolBO.JK_RL_NKLoadPort = "AUSYD";
		consolBO.JK_RL_NKDischargePort = "VNVNH";
		consolBO.Transports[0].JW_RL_NKLoadPort = "AUMEL";
		consolBO.Transports[0].JW_RL_NKDiscPort = "VNVPC";
		consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
		CO2eTestHelper.CreateTransportBooking(consolBO, nameof(DtbBookingDirection.PIC), Factory);
		CO2eTestHelper.CreateTransportBooking(consolBO, nameof(DtbBookingDirection.DLV), Factory);

		using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			// Act
			var writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, null, ZString.Empty, ZString.Empty, false, false, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertNull("PreCarriageShipmentCollection", consolData.PreCarriageShipmentCollection);
			AssertNull("PostCarriageShipmentCollection", consolData.PostCarriageShipmentCollection);
			AssertEquals("TransportLegCollection", 3, consolData.TransportLegCollection.Count);

			var firstLegDO = consolData.TransportLegCollection[0];
			AssertEquals("First leg: starts from C: 1st Load", "AUSYD", firstLegDO.PortOfLoading?.Code);
			AssertEquals("First leg: ends at C: Load", "AUMEL", firstLegDO.PortOfDischarge?.Code);

			var lastLegDO = consolData.TransportLegCollection[2];
			AssertEquals("Last leg: starts from C: Disc.", "VNVPC", lastLegDO.PortOfLoading?.Code);
			AssertEquals("Last leg: ends at C: Last Disc.", "VNVNH", lastLegDO.PortOfDischarge?.Code);
		}
	}

	public void TestPopulatePrePostCarriageLegs()
	{
		// Arrange
		var shipmentBO = Factory.New<ForwardingShipment>();
		var consolBO = shipmentBO.Consols.AddNew();
		var departureCFS = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
		var arrivalCFS = CreateAddress(ZGeography.CreatePoint(20, 20), "1111", "Vinh", "VN", "VNVNH");
		consolBO.JK_RL_NKLoadPort = "AUSYD";
		consolBO.JK_RL_NKDischargePort = "VNVNH";
		consolBO.JK_OA_PackDepotAddress = departureCFS.PK;
		consolBO.CFSDepartureByTransportMode = Core.Constants.TransportModes.Rail;
		consolBO.JK_OA_UnpackDepotAddress = arrivalCFS.PK;
		consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;

		using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			// Act
			var writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, null, ZString.Empty, ZString.Empty, false, false, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			// Assert
			var transportModeConverter = new TransportModeConverter();

			AssertEquals(1, consolData.PreCarriageShipmentCollection.Count);
			AssertEquals(1, consolData.PreCarriageShipmentCollection[0].TransportLegCollection.Count);
			var preCarriageLegDO = consolData.PreCarriageShipmentCollection[0].TransportLegCollection[0];
			AssertEquals("TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Rail), preCarriageLegDO.TransportMode);
			AssertOrganizationAddress("DepartureFrom", preCarriageLegDO.DepartureFrom, ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
			AssertOrganizationAddress("ArrivalAt", preCarriageLegDO.ArrivalAt, ZGeography.Empty, port: "AUSYD");

			AssertEquals(1, consolData.PostCarriageShipmentCollection.Count);
			AssertEquals(1, consolData.PostCarriageShipmentCollection[0].TransportLegCollection.Count);
			var postCarriageLegDO = consolData.PostCarriageShipmentCollection[0].TransportLegCollection[0];
			AssertEquals("TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Road), postCarriageLegDO.TransportMode);
			AssertOrganizationAddress("DepartureFrom", postCarriageLegDO.DepartureFrom, ZGeography.Empty, port: "VNVNH");
			AssertOrganizationAddress("ArrivalAt", postCarriageLegDO.ArrivalAt, ZGeography.CreatePoint(20, 20), "1111", "Vinh", "VN", "VNVNH");
		}
	}

	public void TestPopulatePrePostCarriageLegs_WithLastLocation()
	{
		// Arrange
		var shipmentBO = Factory.New<ForwardingShipment>();
		var consolBO = shipmentBO.Consols.AddNew();
		var departureCFS = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
		var arrivalCFS = CreateAddress(ZGeography.CreatePoint(20, 20), "1111", "Vinh", "VN", "VNVNH");
		consolBO.JK_RL_NKLoadPort = "AUSYD";
		consolBO.JK_RL_NKDischargePort = "VNVNH";
		consolBO.JK_OA_PackDepotAddress = departureCFS.PK;
		consolBO.JK_OA_UnpackDepotAddress = arrivalCFS.PK;
		consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
		consolBO.Transports[0].JW_RL_NKDiscPort = "SGSIN";
		var transport2 = consolBO.Transports.AddNew();
		transport2.JW_RL_NKLoadPort = "SGSIN";
		transport2.JW_RL_NKDiscPort = "VNVNH";
		transport2.JW_TransportMode = "SEA";

		var lastLocation = new PrePostCarriageLocationWrapper("VNSGN");

		using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			// Act
			var writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, lastLocation, ZString.Empty, ZString.Empty, false, false, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			// Assert
			var transportModeConverter = new TransportModeConverter();

			AssertEquals(1, consolData.PreCarriageShipmentCollection.Count);
			AssertEquals(1, consolData.PreCarriageShipmentCollection[0].TransportLegCollection.Count);

			var preCarriageLegDO1 = consolData.PreCarriageShipmentCollection[0].TransportLegCollection[0];
			AssertEquals("Pre Leg 1: TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Road), preCarriageLegDO1.TransportMode);
			AssertOrganizationAddress("Pre Leg 2: DepartureFrom", preCarriageLegDO1.DepartureFrom, ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
			AssertOrganizationAddress("Pre Leg 2: ArrivalAt", preCarriageLegDO1.ArrivalAt, ZGeography.Empty, port: "AUSYD");

			AssertEquals(1, consolData.PostCarriageShipmentCollection.Count);
			AssertEquals(2, consolData.PostCarriageShipmentCollection[0].TransportLegCollection.Count);

			var postCarriageLegDO1 = consolData.PostCarriageShipmentCollection[0].TransportLegCollection[0];
			AssertEquals("Post Leg 1: TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Road), postCarriageLegDO1.TransportMode);
			AssertOrganizationAddress("Post Leg 1: DepartureFrom", postCarriageLegDO1.DepartureFrom, ZGeography.Empty, port: "VNVNH");
			AssertOrganizationAddress("Post Leg 1: ArrivalAt", postCarriageLegDO1.ArrivalAt, ZGeography.CreatePoint(20, 20), "1111", "Vinh", "VN", "VNVNH");

			var postCarriageLegDO2 = consolData.PostCarriageShipmentCollection[0].TransportLegCollection[1];
			AssertEquals("Post Leg 2: TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Road), postCarriageLegDO2.TransportMode);
			AssertOrganizationAddress("Post Leg 2: DepartureFrom", postCarriageLegDO2.DepartureFrom, ZGeography.CreatePoint(20, 20), "1111", "Vinh", "VN", "VNVNH");
			AssertOrganizationAddress("Post Leg 2: ArrivalAt", postCarriageLegDO2.ArrivalAt, ZGeography.Empty, port: "VNSGN");

			// Arrange fallback when no CFSs
			consolBO.JK_OA_PackDepotAddress = ZGuid.Empty;
			consolBO.JK_OA_UnpackDepotAddress = ZGuid.Empty;

			// Act
			consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertNull(consolData.PreCarriageShipmentCollection);

			AssertEquals(1, consolData.PostCarriageShipmentCollection.Count);
			AssertEquals(1, consolData.PostCarriageShipmentCollection[0].TransportLegCollection.Count);

			postCarriageLegDO1 = consolData.PostCarriageShipmentCollection[0].TransportLegCollection[0];
			AssertEquals("Post Leg 1: TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Road), postCarriageLegDO1.TransportMode);
			AssertOrganizationAddress("Post Leg 1: DepartureFrom", postCarriageLegDO1.DepartureFrom, ZGeography.Empty, port: "VNVNH");
			AssertOrganizationAddress("Post Leg 1: ArrivalAt", postCarriageLegDO1.ArrivalAt, ZGeography.Empty, port: "VNSGN");
		}
	}

	public void TestPopulatePrePostCarriageLegs_ExcludePreCarriageLegsIfHasTransportBooking()
	{
		// Arrange
		var shipmentBO = Factory.New<ForwardingShipment>();
		var consolBO = shipmentBO.Consols.AddNew();
		var departureCFS = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
		var arrivalCFS = CreateAddress(ZGeography.CreatePoint(20, 20), "1111", "Vinh", "VN", "VNVNH");
		consolBO.JK_RL_NKLoadPort = "AUSYD";
		consolBO.JK_RL_NKDischargePort = "VNVNH";
		consolBO.JK_OA_PackDepotAddress = departureCFS.PK;
		consolBO.JK_OA_UnpackDepotAddress = arrivalCFS.PK;
		consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
		consolBO.Transports[0].JW_RL_NKDiscPort = "SGSIN";
		var transport2 = consolBO.Transports.AddNew();
		transport2.JW_RL_NKLoadPort = "SGSIN";
		transport2.JW_RL_NKDiscPort = "VNVNH";
		transport2.JW_TransportMode = "SEA";

		using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			// Act
			CO2eTestHelper.CreateTransportBooking(consolBO, nameof(DtbBookingDirection.DLV), Factory);
			var writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, null, ZString.Empty, ZString.Empty, true, false, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertEquals(1, consolData.PreCarriageShipmentCollection.Count);
			AssertEquals(1, consolData.PreCarriageShipmentCollection[0].TransportLegCollection.Count);
			AssertEquals(1, consolData.PostCarriageShipmentCollection.Count);
			AssertEquals(1, consolData.PostCarriageShipmentCollection[0].TransportLegCollection.Count);

			// Arrange consol PIC TB
			CO2eTestHelper.CreateTransportBooking(consolBO, nameof(DtbBookingDirection.PIC), Factory);

			// Act
			writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, null, ZString.Empty, ZString.Empty, true, false, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertNull(consolData.PreCarriageShipmentCollection);
			AssertEquals(1, consolData.PostCarriageShipmentCollection.Count);
			AssertEquals(1, consolData.PostCarriageShipmentCollection[0].TransportLegCollection.Count);
		}
	}

	public void TestPopulatePrePostCarriageLegs_ExcludePostCarriageLegsIfHasTransportBooking()
	{
		// Arrange
		var shipmentBO = Factory.New<ForwardingShipment>();
		var consolBO = shipmentBO.Consols.AddNew();
		var departureCFS = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
		var arrivalCFS = CreateAddress(ZGeography.CreatePoint(20, 20), "1111", "Vinh", "VN", "VNVNH");
		consolBO.JK_RL_NKLoadPort = "AUSYD";
		consolBO.JK_RL_NKDischargePort = "VNVNH";
		consolBO.JK_OA_PackDepotAddress = departureCFS.PK;
		consolBO.JK_OA_UnpackDepotAddress = arrivalCFS.PK;
		consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
		consolBO.Transports[0].JW_RL_NKDiscPort = "SGSIN";
		var transport2 = consolBO.Transports.AddNew();
		transport2.JW_RL_NKLoadPort = "SGSIN";
		transport2.JW_RL_NKDiscPort = "VNVNH";
		transport2.JW_TransportMode = "SEA";

		using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			// Act
			CO2eTestHelper.CreateTransportBooking(consolBO, nameof(DtbBookingDirection.PIC), Factory);
			var writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, null, ZString.Empty, ZString.Empty, false, true, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertEquals(1, consolData.PreCarriageShipmentCollection.Count);
			AssertEquals(1, consolData.PreCarriageShipmentCollection[0].TransportLegCollection.Count);
			AssertEquals(1, consolData.PostCarriageShipmentCollection.Count);
			AssertEquals(1, consolData.PostCarriageShipmentCollection[0].TransportLegCollection.Count);

			// Arrange consol DLV TB
			CO2eTestHelper.CreateTransportBooking(consolBO, nameof(DtbBookingDirection.DLV), Factory);

			// Act
			writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, null, ZString.Empty, ZString.Empty, false, true, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertNull(consolData.PostCarriageShipmentCollection);
			AssertEquals(1, consolData.PreCarriageShipmentCollection.Count);
			AssertEquals(1, consolData.PreCarriageShipmentCollection[0].TransportLegCollection.Count);
		}
	}

	public void TestPopulateWeightAndTEU()
	{
		// Arrange
		var shipmentBO = Factory.New<ForwardingShipment>();
		shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;
		var consolBO = shipmentBO.Consols.AddNew();
		consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
		var container1 = CreateContainer(consolBO, "20GP");
		CreateContainer(consolBO, "40GP");
		var container3 = CreateContainer(consolBO, "45HC");

		using (shipmentBO.OuterPackLines.TemporarilyDisableAutomaticPackingIntoContainer())
		{
			CreatePackLine(shipmentBO, 100m, "KG", container1);
			CreatePackLine(shipmentBO, 0.5m, "T");
			CreatePackLine(shipmentBO, 1.5m, "T", container3);
		}

		using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			// Act
			var writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, new PrePostCarriageLocationWrapper(""), ZString.Empty, ZString.Empty, false, false, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertEquals("TotalWeight", 1600m, consolData.TotalWeight);
			AssertEquals("TotalWeightUnit", "KG", consolData.TotalWeightUnit.Code);

			AssertNotNull(consolData.TEU);
			AssertEquals("TEU.NumberOfTEU", 3.5m, consolData.TEU.NumberOfTEU);
			AssertEquals("TEU.TonnesPerTEU", 0.457m, consolData.TEU.TonnesPerTEU.Value.Round(3));
			AssertEquals("TEU.ContainerEmptyWeightPerTEU", 2022.857m, consolData.TEU.ContainerEmptyWeightPerTEU.Value.Round(3));
			AssertEquals("TEU.ContainerEmptyWeightPerTEUUnit", "KG", consolData.TEU.ContainerEmptyWeightPerTEUUnit.Code);
		}
	}

	public void TestPopulateWeightAndTEU_NoPackLinesPackedIntoConsol()
	{
		// Arrange
		var shipmentBO = Factory.New<ForwardingShipment>();
		shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;
		var consolBO = shipmentBO.Consols.AddNew();
		consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
		CreateContainer(consolBO, "20GP");
		CreateContainer(consolBO, "40GP");
		CreateContainer(consolBO, "45HC");

		using (shipmentBO.OuterPackLines.TemporarilyDisableAutomaticPackingIntoContainer())
		{
			CreatePackLine(shipmentBO, 100m, "KG");
			CreatePackLine(shipmentBO, 0.5m, "T");
			CreatePackLine(shipmentBO, 1.5m, "T");
		}
		shipmentBO.JS_ActualWeight = 2.1m;
		shipmentBO.JS_UnitOfWeight = "T";

		using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			// Act
			var writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, new PrePostCarriageLocationWrapper(""), ZString.Empty, ZString.Empty, false, false, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertEquals("TotalWeight", 2100m, consolData.TotalWeight);
			AssertEquals("TotalWeightUnit", "KG", consolData.TotalWeightUnit.Code);

			AssertNull(consolData.TEU);
		}
	}

	public void TestPopulateWeightAndTEU_ContainersAreUsedByAnotherShipment()
	{
		// Arrange
		var shipmentBO = Factory.New<ForwardingShipment>();
		shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;
		var consolBO = shipmentBO.Consols.AddNew();
		consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
		var container1 = CreateContainer(consolBO, "20GP");
		CreateContainer(consolBO, "40GP");
		var container3 = CreateContainer(consolBO, "45HC");

		using (shipmentBO.OuterPackLines.TemporarilyDisableAutomaticPackingIntoContainer())
		{
			CreatePackLine(shipmentBO, 100m, "KG", container1);
			CreatePackLine(shipmentBO, 0.5m, "T");
			CreatePackLine(shipmentBO, 1.5m, "T", container3);
		}

		var anotherShipmentBO = consolBO.Shipments.AddNew();
		anotherShipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;
		using (anotherShipmentBO.OuterPackLines.TemporarilyDisableAutomaticPackingIntoContainer())
		{
			CreatePackLine(anotherShipmentBO, 100m, "KG", container1);
			CreatePackLine(anotherShipmentBO, 1.5m, "T", container3);
		}

		using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			// Act
			var writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, new PrePostCarriageLocationWrapper(""), ZString.Empty, ZString.Empty, false, false, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertEquals("TotalWeight", 1600m, consolData.TotalWeight);
			AssertEquals("TotalWeightUnit", "KG", consolData.TotalWeightUnit.Code);

			AssertNotNull(consolData.TEU);
			AssertEquals("TEU.NumberOfTEU", 1.75m, consolData.TEU.NumberOfTEU);
			AssertEquals("TEU.TonnesPerTEU", 0.914m, consolData.TEU.TonnesPerTEU.Value.Round(3));
			AssertEquals("TEU.ContainerEmptyWeightPerTEU", 2022.857m, consolData.TEU.ContainerEmptyWeightPerTEU.Value.Round(3));
			AssertEquals("TEU.ContainerEmptyWeightPerTEUUnit", "KG", consolData.TEU.ContainerEmptyWeightPerTEUUnit.Code);
		}
	}

	public void TestRelatedConsolCO2eRequestDataObjectWriter_ConsolContainerModeTagIsFCL_WhenConsolContainerModeIsGRP()
	{
		// Arrange
		var shipmentBO = Factory.New<ForwardingShipment>();
		shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;

		var consolBO = shipmentBO.Consols.AddNew();
		consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
		consolBO.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;

		// Act
		var writer = new RelatedConsolCO2eRequestDataObjectWriter(shipmentBO, new PrePostCarriageLocationWrapper(""), ZString.Empty, ZString.Empty, false, false, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
		var consolData = writer.GetDataObject(consolBO);

		// Assert
		AssertNotNull("consolData", consolData);
		AssertEquals("consolData.ContainerMode", "FCL", consolData.ContainerMode.Code);
	}

	void AssertOrganizationAddress(string message, OrganizationAddress addressDO, ZGeography geoLocation, string postcode = null, string city = null, string countryCode = null, string port = null)
	{
		CombineAssertions(message, () =>
		{
			if (geoLocation.IsEmpty)
			{
				AssertNull("No location", addressDO.GeoLocation);
			}
			else
			{
				AssertEquals("Location latitude", geoLocation.Latitude, (double)addressDO.GeoLocation.Latitude);
				AssertEquals("Location longitude", geoLocation.Longitude, (double)addressDO.GeoLocation.Longitude);
			}
			AssertEquals("Postcode", postcode, addressDO.Postcode);
			AssertEquals("City", city, addressDO.City);
			AssertEquals("Country", countryCode, addressDO.Country?.Code);
			AssertEquals("Port", port, addressDO.Port?.Code);
		});
	}

	OrgAddress CreateAddress(ZGeography geoLocation, string postcode, string city, string countryCode, string port)
	{
		var address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
		address.OA_GeoLocation = geoLocation;
		address.OA_City = city;
		address.OA_PostCode = postcode;
		address.OA_RN_NKCountryCode = countryCode;
		address.OA_RL_NKRelatedPortCode = port;
		address.OA_ValidationStatus = AddressValidationStatus.Verified;
		return address;
	}

	ForwardingContainer CreateContainer(ForwardingConsol consol, ZString containerType)
	{
		var container = consol.Containers.AddNew();
		container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;
		return container;
	}

	ForwardingPackLine CreatePackLine(ForwardingShipment shipment, ZDecimal weight, ZString uow, ForwardingContainer container = null)
	{
		var packLine = shipment.OuterPackLines.AddNew();
		packLine.JL_ActualWeight = weight;
		packLine.JL_ActualWeightUQ = uow;
		if (container != null)
		{
			packLine.JL_JC = container.PK;
		}

		return packLine;
	}
}
