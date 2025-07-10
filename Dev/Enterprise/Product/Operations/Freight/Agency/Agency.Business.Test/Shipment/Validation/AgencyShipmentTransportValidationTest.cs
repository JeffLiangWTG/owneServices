using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Agency.Business
{
	internal class AgencyShipmentTransportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateJW_OA_ArrivalLocation()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var packingMode in PackingModes)
				{
					AssertValidate_JW_OA_ArrivalLocation(modeTypePair.Item1, modeTypePair.Item2, packingMode);
				}
			}
		}

		public void TestValidateJW_OA_DepartureLocation()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var packingMode in PackingModes)
				{
					AssertValidate_JW_OA_DepartureLocation(modeTypePair.Item1, modeTypePair.Item2, packingMode);
				}
			}
		}

		#region Implementation
		void AssertValidate_JW_OA_ArrivalLocation(ZString transportMode, ZString transportType, ZString packingMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = packingMode;
			shipment.JS_RL_NKDestination = "AUSYD";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_OA_ArrivalLocation = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertNull(shipment.DeliveryRoadOrRailLeg);
			AssertNoWarnings("No DeliveryRoadOrRailLeg", transport1.JW_OA_ArrivalLocationInfo);
			var address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var consigneePickupDeliveryAddress = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneePickupDeliveryAddress);
			consigneePickupDeliveryAddress.E2_OA_Address = address.PK;
			transport1.Validation.ValidateJW_OA_ArrivalLocation();
			AssertNoWarnings("No DeliveryRoadOrRailLeg", transport1.JW_OA_ArrivalLocationInfo);
			var warningMessage = "Address entered here does not match with delivery address on the Addresses tab. You may want to synchronize the data in order to create a correct Transport Booking.";
			transport1.JW_TransportMode = transportMode;
			transport1.JW_TransportType = transportType;
			AssertEquals(transport1, shipment.DeliveryRoadOrRailLeg);
			transport1.Validation.ValidateJW_OA_ArrivalLocation();
			AssertHasWarning(transport1.JW_OA_ArrivalLocationInfo, warningMessage);
			transport1.JW_OA_ArrivalLocation = ZGuid.Empty;
			AssertNoWarnings("Empty JW_OA_ArrivalLocation", transport1.JW_OA_ArrivalLocationInfo);
			transport1.JW_OA_ArrivalLocation = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertHasWarning(transport1.JW_OA_ArrivalLocationInfo, warningMessage);
			transport1.JW_OA_ArrivalLocation = address.PK;
			AssertNoWarnings("No warnings for JW_OA_ArrivalLocation", transport1.JW_OA_ArrivalLocationInfo);
		}

		public void AssertValidate_JW_OA_DepartureLocation(ZString transportMode, ZString transportType, ZString packingMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = packingMode;
			shipment.JS_RL_NKOrigin = "AUSYD";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_OA_DepartureLocation = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertNull(shipment.PickupRoadOrRailLeg);
			AssertNoWarnings("No PickupRoadOrRailLeg", transport1.JW_OA_DepartureLocationInfo);
			var address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var consignorPickupAddress = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
			consignorPickupAddress.E2_OA_Address = address.PK;
			transport1.Validation.ValidateJW_OA_DepartureLocation();
			AssertNoWarnings("No PickupRoadOrRailLeg", transport1.JW_OA_DepartureLocationInfo);
			var warningMessage = "Address entered here does not match with pickup address on the Addresses tab. You may want to synchronize the data in order to create a correct Transport Booking.";
			transport1.JW_TransportMode = transportMode;
			transport1.JW_TransportType = transportType;
			AssertEquals(transport1, shipment.PickupRoadOrRailLeg);
			transport1.Validation.ValidateJW_OA_DepartureLocation();
			AssertHasWarning(transport1.JW_OA_DepartureLocationInfo, warningMessage);
			transport1.JW_OA_DepartureLocation = ZGuid.Empty;
			AssertNoWarnings("Empty JW_OA_DepartureLocation", transport1.JW_OA_DepartureLocationInfo);
			transport1.JW_OA_DepartureLocation = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertHasWarning(transport1.JW_OA_DepartureLocationInfo, warningMessage);
			transport1.JW_OA_DepartureLocation = address.PK;
			AssertNoWarnings("No warnings for JW_OA_DepartureLocation", transport1.JW_OA_DepartureLocationInfo);
		}

		List<Tuple<ZString, ZString>> TransportModeTypes
		{
			get
			{
				return new List<Tuple<ZString, ZString>>()
				{ { Core.Constants.TransportModes.Road, Core.Constants.TransportPlanningType.PreCarriage }, { Core.Constants.TransportModes.Road, Core.Constants.TransportPlanningType.OnForwarding }, { Core.Constants.TransportModes.Road, Core.Constants.TransportPlanningType.Other }, { Core.Constants.TransportModes.Rail, Core.Constants.TransportPlanningType.PreCarriage }, { Core.Constants.TransportModes.Rail, Core.Constants.TransportPlanningType.OnForwarding }, { Core.Constants.TransportModes.Rail, Core.Constants.TransportPlanningType.Other } };
			}
		}

		List<ZString> PackingModes
		{
			get
			{
				if (packingModes == null)
				{
					packingModes = new List<ZString>();
					var shipment = Factory.New<AgencyShipment>();
					foreach (var packingModePair in shipment.Lookups.JS_PackingMode_List.ToArray())
					{
						packingModes.Add(packingModePair.Code);
					}
				}

				return packingModes;
			}
		}

		List<ZString> packingModes;
		#endregion
	}
}
