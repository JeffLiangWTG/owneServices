using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Business
{
	public partial class CommonAviationSecuritySupport
	{
		public CommonAviationSecuritySupport(CommonShipment shipmentBO, string countryCode = null)
		{
			ShipmentBO = Argument.NotNull(shipmentBO, nameof(shipmentBO));
			var country = countryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CountryCode = Core.Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(country)
				? Core.Constants.CountryCodes.EuropeanUnion
				: country;
		}

		protected CommonShipment ShipmentBO { get; }
		protected ZString CountryCode { get; }

		#region Is Applicable

		public bool IsAviationSecurityApplicableForTransportMode => ShipmentBO.IsAir || ShipmentBO.IsCourier || ShipmentBO.JS_TransportMode == Core.Constants.TransportModes.SeaAir;

		#endregion

		#region Inspection Status

		public ZBool IsApprovedForAviationSecurity => !HasUnknownInspectionTypeCode;

		public bool HasUnknownInspectionTypeCode
		{
			get
			{
				return ShipmentBO.JS_InspectionTypeCode.IsEmpty
					|| ShipmentBO.JS_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code
					|| (IsHighRiskShipment && ShipmentBO.JS_AdditionalInspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code);
			}
		}

		#endregion

		#region Is High Risk

		public bool IsHighRiskShipment
		{
			get
			{
				return IsAviationSecurityApplicableForTransportMode
					&& SupplyChainSecurityConfiguration.IsHighRiskApplicable
					&& ShipmentBO.JS_IsHighRisk;
			}
		}

		#endregion

		#region Passenger Flights

		public bool IsAllowedOnPassengerFlights()
		{
			if (!IsAllowedOnPassengerFlights(ShipmentBO.JS_InspectionTypeCode))
			{
				return false;
			}

			if (IsHighRiskShipment && !IsAllowedOnPassengerFlights(ShipmentBO.JS_AdditionalInspectionTypeCode))
			{
				return false;
			}

			return true;
		}

		public bool IsAllowedOnPassengerFlights(ZString inspectionTypeCode)
		{
			return SupplyChainSecurityConfiguration.IsInspectionTypeAllowedOnPassengerFlights(inspectionTypeCode);
		}

		public virtual bool RelevantOrganisationsAreApprovedForShippingOnPassengerFlights => true;

		#endregion

		#region SupplyChainSecurityConfiguration

		internal ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = GetNewSupplyChainSecurityConfiguration()); }
		}
		ISupplyChainSecurityConfiguration supplyChainSecurityConfiguration;
		
		protected virtual ISupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfiguration()
		{
			return ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfigurationForCountry(CountryCode);
		}
	
		public ZString ReasonForAviationSecurityNotBeingAvailable
		{
			get
			{
				if (SupplyChainSecurityConfiguration.IsLicensedModule && !SupplyChainSecurityConfiguration.IsEnabled && !SupplyChainSecurityConfiguration.IsOnlyForDevelopers)
				{
					return Res.GetString("e2735667-d1cf-4e00-9c24-ee10ffa5df91", "Supply Chain Security must be enabled via the registry under '{0}'.", SupplyChainSecurityConfiguration.RegistryItemKey);
				}

				return ZString.Empty;
			}
		}

		#endregion
	}
}
