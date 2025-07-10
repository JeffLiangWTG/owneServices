using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using static Enterprise.Freight.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class FreightUtilities
	{
		/// <summary>
		/// Returns the corresponding CommonShipment Container mode for the given Consol Container mode
		/// </summary>
		/// <param name="consolContainerMode">Container Mode of the consol</param>
		/// <param name="consolTransportMode">Transport Mode of the consol</param>
		/// <returns></returns>
		public static ZString ShipmentContainerMode(ZString consolContainerMode, ZString consolTransportMode)
		{
			ZString result = consolContainerMode;
			switch (consolContainerMode)
			{
				case Constants.ContainerModes.Groupage:
					result = Constants.ContainerModes.LCL;
					break;

				case Constants.ContainerModes.Other:
					result = "";
					break;
			}

			return result;
		}

		public static HBLDeliveryModes ShipmentHBLDeliveryMode(ZString shipmentPackingMode)
		{
			switch (shipmentPackingMode)
			{
				case Constants.ContainerModes.FCL:
					return FreightDataRegistry.Instance.HBLDeliveryMode_FCL.Value;

				case Constants.ContainerModes.LCL:
					return FreightDataRegistry.Instance.HBLDeliveryMode_LCL.Value;

				case Constants.ContainerModes.BuyersConsol:
					return FreightDataRegistry.Instance.HBLDeliveryMode_BCN.Value;

				case Constants.ContainerModes.ShippersConsol:
					return FreightDataRegistry.Instance.HBLDeliveryMode_SCN.Value;

				case Constants.ContainerModes.Bulk:
					return FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD.Value;

				case Constants.ContainerModes.BreakBulk:
					return FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD.Value;

				case Constants.ContainerModes.RollOnRollOff:
					return FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD.Value;

				case Constants.ContainerModes.Liquid:
					return FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD.Value;

				case Constants.ContainerModes.Loose:
					return FreightDataRegistry.Instance.HBLDeliveryMode_LSE.Value;

				case Constants.ContainerModes.ULD:
					return FreightDataRegistry.Instance.HBLDeliveryMode_ULD.Value;

				default:
					return new HBLDeliveryModes();
			}
		}

		public static bool AllowCreateScheduleFromJob(string transportMode)
		{
			SecurityCheckpoint checkupoint = GetCreateScheduleFromJobSecurityCheckpoint(transportMode);
			return checkupoint != null && checkupoint.IsAllowed;
		}

		public static SecurityCheckpoint GetCreateScheduleFromJobSecurityCheckpoint(string transportMode)
		{
			switch (transportMode)
			{
				case Constants.TransportModes.Sea:
					return Env.Security.SailingScheduleCreateFromJob;
				case Constants.TransportModes.Rail:
					return Env.Security.RailScheduleCreateFromJob;
				case Constants.TransportModes.Air:
					return Env.Security.FlightScheduleCreateFromJob;
				case Constants.TransportModes.Road:
					return Env.Security.TruckScheduleCreateFromJob;
				default:
					return null;
			}
		}

		public static SecurityCheckpoint GetEditScheduleSecurityCheckpoint(string transportMode)
		{
			switch (transportMode)
			{
				case Constants.TransportModes.Sea:
					return Env.Security.SailingScheduleEdit;
				case Constants.TransportModes.Rail:
					return Env.Security.RailScheduleEdit;
				case Constants.TransportModes.Air:
					return Env.Security.FlightScheduleEdit;
				case Constants.TransportModes.Road:
					return Env.Security.TruckScheduleEdit;
				default:
					return null;
			}
		}

		/// <summary>
		/// Checks that Weight Unit is valid. Use before calling Constants.Weight.Convert.
		/// </summary>
		/// <param name="weightUnit"></param>
		/// <returns>Weight Unit Code</returns>
		public static bool IsValidWeightUnit(ZString weightUnit)
		{
			return Constants.Weight.ContainsCode(weightUnit);
		}

		/// <summary>
		/// Checks that Volume Unit is valid. Use before calling Constants.Volume.Convert.
		/// </summary>
		/// <param name="volumeUnit"></param>
		/// <returns>Volume Unit Code</returns>
		public static bool IsValidVolumeUnit(ZString volumeUnit)
		{
			return Constants.Volume.ContainsCode(volumeUnit);
		}

		public static bool IsValidDimensionUnit(ZString dimensionUnit)
		{
			return Constants.Length.ContainsCode(dimensionUnit);
		}

		/// <summary>
		/// This util method is used for calculate volume from factors passed in as parameters.
		/// Example of its usage can be found in <see cref="VolumeCalculatorWebServiceMethod"/>, <see cref="HVLVItemValidation"/>,
		/// <see cref="HVLVItemDimensionsHelper"/>, <see cref="AgencyShipmentContainer"/>, <see cref="OrderLine"/>,
		/// <see cref="CommonBookedCtgMove"/>, and <see cref="PackLine"/>.
		/// </summary>
		/// <param name="overridenRoundingFunc">Override the default bankers rounding by passing and executing the optional function</param>
		public static decimal CalculateVolume(decimal defaultVolume, int multiplier, decimal length, decimal width, decimal height,
		string unitOfMeasure, string unitOfVolume, byte scale, Func<decimal, decimal> overridenRoundingFunc = null)
		{
			decimal result;

			if (IsValidDimensionUnit(unitOfMeasure) && IsValidVolumeUnit(unitOfVolume))
			{
				//Convert dimensions to Metres
				if (unitOfMeasure != Constants.Length.Metres)
				{
					length = Constants.Length.Convert(length, unitOfMeasure, Constants.Length.Metres);
					width = Constants.Length.Convert(width, unitOfMeasure, Constants.Length.Metres);
					height = Constants.Length.Convert(height, unitOfMeasure, Constants.Length.Metres);
				}

				//Calculate Volume in M3
				decimal unroundedVolume = length * width * height * multiplier;

				//Convert to different Volume unit if required
				if (unitOfVolume != Constants.Volume.CubicMetres)
				{
					unroundedVolume = Constants.Volume.Convert(unroundedVolume, Constants.Volume.CubicMetres, unitOfVolume);
				}
				result = overridenRoundingFunc?.Invoke(unroundedVolume) ?? Enterprise.ZArchitecture.Core.Utilities.Round(unroundedVolume, scale);
			}
			else
			{
				result = defaultVolume;
			}

			return result;
		}

		#region Aviation Security

		public static ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfiguration(); }
		}

		public static bool AviationSecurityIsAvailable
		{
			get { return !IsCountryWithAviationSecurityValidation() || SupplyChainSecurityConfiguration.IsEnabled; }
		}

		public static bool IsCountryWithAviationSecurityValidation(string countryCode)
		{
			return CountriesWithAviationSecurityValidation.Any(code => code == countryCode);
		}

		public static bool IsCountryWithAviationSecurityValidation()
		{
			return IsCountryWithAviationSecurityValidation(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		#region Aviation Security Lists

		static IEnumerable<ZString> CountriesWithAviationSecurityValidation
		{
			get
			{
				yield return Core.Constants.CountryCodes.UnitedStates;
				yield return Core.Constants.CountryCodes.Japan;
				yield return Core.Constants.CountryCodes.HongKong;
				yield return Core.Constants.CountryCodes.Singapore;

				foreach (var euCountry in Constants.CountryCodes.EuropeanUnionAviationSecurityMembersList)
				{
					yield return euCountry;
				}
			}
		}

		#endregion

		#region Country Specific Values

		public static ZString InspectionType_Approved_Description
		{
			get
			{
				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Constants.CountryCodes.HongKong:
						return Res.GetString("aeeab14d-693a-48cb-a0fa-debc742928c1", "Approved (Known or Account Consignor/Regulated Agent)");

					default:
						return Res.GetString("c051c6c2-4af5-41b0-b350-ace120ba1d24", "Approved/Known Shipper");
				}
			}
		}

		#endregion

		#endregion

		public static KeyValuePair<string, string> As(this string key, string value)
		{
			return new KeyValuePair<string, string>(key, value);
		}

		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			TValue value;

			if (!dictionary.TryGetValue(key, out value))
			{
				value = default(TValue);
			}

			return value;
		}

		public static VoyageOrigin In(this VoyageOrigin origin, string unloco)
		{
			origin.JA_RL_NKPortOfLoading = unloco;

			return origin;
		}

		public static VoyageDestination In(this VoyageDestination destination, string unloco)
		{
			destination.JB_RL_NKPortOfDischarge = unloco;

			return destination;
		}
	}
}
