using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business.Testing
{
	public class CommonAviationSecuritySupportTest : TestCaseWithFactory
	{
		#region IsAviationSecurityApplicableForTransportMode

		public void TestIsAviationSecurityApplicableForTransportMode()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";
			Assert("AIR", shipment.AviationSecurity.IsAviationSecurityApplicableForTransportMode);

			shipment.JS_TransportMode = "COU";
			Assert("COU", shipment.AviationSecurity.IsAviationSecurityApplicableForTransportMode);

			shipment.JS_TransportMode = "SEA";
			Assert("SEA", !shipment.AviationSecurity.IsAviationSecurityApplicableForTransportMode);

			shipment.JS_TransportMode = "ROA";
			Assert("ROA", !shipment.AviationSecurity.IsAviationSecurityApplicableForTransportMode);

			shipment.JS_TransportMode = "FSA";
			Assert("FSA", shipment.AviationSecurity.IsAviationSecurityApplicableForTransportMode);
		}

		#endregion

		#region Inspection Status

		public void TestHasUnknownInspectionStatus()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRSAO";
			shipment.JS_InspectionTypeCode = "UNK";

			Assert("UNK", shipment.AviationSecurity.HasUnknownInspectionTypeCode);

			shipment.JS_InspectionTypeCode = "PHS";
			Assert("PHS", !shipment.AviationSecurity.HasUnknownInspectionTypeCode);

			shipment.JS_InspectionTypeCode = "APP";
			Assert("APP", !shipment.AviationSecurity.HasUnknownInspectionTypeCode);
		}

		#endregion

		#region Passenger Flights

		public void TestIsAllowedOnPassengerFlights()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "DEBER";

			shipment.JS_InspectionTypeCode = "UNK";
			Assert("UNK shipment is allowed. (It means 'not inspected yet')", shipment.AviationSecurity.IsAllowedOnPassengerFlights());
			Assert("UNK code is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights("UNK"));

			shipment.JS_InspectionTypeCode = "APP";
			Assert("APP shipment is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights());
			Assert("APP code is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights("APP"));

			shipment.JS_InspectionTypeCode = "PHS";
			Assert("PHS shipment is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights());
			Assert("PHS code is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights("PHS"));

			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypes_Australia.Value;
			var phs = inspectionTypes.Types.Cast<ShipmentInspectionType>().FirstOrDefault(t => t.Code == "PHS");
			phs.AllowedOnPassengerFlights = false;
			using (FreightDataRegistry.Instance.ShipmentInspectionTypes_Australia.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				shipment.JS_InspectionTypeCode = "UNK";
				Assert("UNK shipment is allowed. (It means 'not inspected yet')", shipment.AviationSecurity.IsAllowedOnPassengerFlights());
				Assert("UNK code is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights("UNK"));

				shipment.JS_InspectionTypeCode = "APP";
				Assert("APP shipment is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights());
				Assert("APP code is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights("APP"));

				shipment.JS_InspectionTypeCode = "PHS";
				Assert("PHS shipment is not allowed", !shipment.AviationSecurity.IsAllowedOnPassengerFlights());
				Assert("PHS code is not allowed", !shipment.AviationSecurity.IsAllowedOnPassengerFlights("PHS"));
			}
		}

		public void TestIsAllowedOnPassengerFlights_CustomInspectionType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
				var bob = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value.Types.Cast<ShipmentInspectionType>().FirstOrDefault(t => t.Code == "BOB");
				AssertNull("Precondition: BOB should not exist", bob);

				var shipment = Factory.New<CommonShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "AUMEL";

				shipment.JS_InspectionTypeCode = "BOB";
				Assert("BOB shipment is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights());
				Assert("BOB code is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights("BOB"));

				inspectionTypes.Types.Add("BOB", (NoResString)"BOB THE MAN", false, false);

				using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
				{
					Assert("BOB shipment is not allowed", !shipment.AviationSecurity.IsAllowedOnPassengerFlights());
					Assert("BOB code is not allowed", !shipment.AviationSecurity.IsAllowedOnPassengerFlights("BOB"));

					bob = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value.Types.Cast<ShipmentInspectionType>().FirstOrDefault(t => t.Code == "BOB");
					bob.AllowedOnPassengerFlights = true;
					bob.ShowInList = true;
					Assert("BOB shipment is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights());
					Assert("BOB code is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights("BOB"));
				}
			}
		}

		public void TestIsAllowedOnPassengerFlights_HighRisk()
		{
			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
			var phs = inspectionTypes.Types.Cast<ShipmentInspectionType>().FirstOrDefault(t => t.Code == "PHS");
			phs.AllowedOnPassengerFlights = false;

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.New<CommonShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "AUMEL";

				shipment.JS_IsHighRisk = true;
				shipment.JS_InspectionTypeCode = "UNK";
				shipment.JS_AdditionalInspectionTypeCode = "UNK";
				Assert("UNK shipment is allowed. (It means 'not inspected yet')", shipment.AviationSecurity.IsAllowedOnPassengerFlights());

				shipment.JS_AdditionalInspectionTypeCode = "APP";
				Assert("APP is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights());

				shipment.JS_AdditionalInspectionTypeCode = "XRY";
				Assert("XRY is allowed", shipment.AviationSecurity.IsAllowedOnPassengerFlights());

				shipment.JS_AdditionalInspectionTypeCode = "PHS";
				Assert("PHS is not allowed", !shipment.AviationSecurity.IsAllowedOnPassengerFlights());
			}
		}

		#endregion
	}
}
