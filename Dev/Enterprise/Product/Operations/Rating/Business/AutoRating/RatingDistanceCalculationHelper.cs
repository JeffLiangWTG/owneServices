namespace Enterprise.Rating.Business
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.Environment;
	using Enterprise.Freight.DistanceCalculation.Business;
	using Enterprise.Freight.DistanceCalculation.Integration;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.Rating.Integration;
	using Enterprise.ZArchitecture.Core;

	public class RatingDistanceCalculationHelper
	{
		public RatingDistanceCalculationHelper(RatingCriteria criteria, IRateLine line)
		{
			this.criteria = criteria;
			this.line = line;
			factory = line.Factory;
		}

		readonly RatingCriteria criteria;
		readonly IRateLine line;
		readonly BusinessObjectFactory factory;

		#region SuppressResourceStringsCheckRegion

		/// <summary>
		/// Fallbacks
		/// 1. Manually entered/calculated
		/// 2. Distance Calculation Service
		/// 3. Postcode To Postcode Distance (Australia Only)
		/// </summary>
		public decimal GetDistance(ZStringBuilder description)
		{
			if (line == null || criteria == null)
			{
				return 0m;
			}

			var result = GetDistanceFromMeasures(description);
			if (result == 0)
			{
				var cartageLocation = GetCartageLocation();
				if (Env.Registry.Rating.UseDistanceCalculationService)
				{
					result = GetDistanceFromService(description, cartageLocation);
				}
				else
				{
					description.AppendLine(Log("Distance Calculation Service", 0m, " (registry disabled)"));
				}

				if (result == 0)
				{
					result = GetDistancePostcodeToPostcode(description, cartageLocation);
				}
			}

			return result;
		}

		#region Get Cartage Location

		ILocation GetCartageLocationFromCriteria()
		{
			switch (criteria.RateTypeToUse)
			{
				case RateType.TransportBookings:
				case RateType.LocalTransport:
					return criteria.Origin;
				case RateType.Warehouse:
					if (line.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSInwards)
					{
						if (criteria.PickupAddress != null)
						{
							return LocationHelper.GetLocationFromIDocAddress(criteria.PickupAddress, factory);
						}
					}
					else if (criteria.DeliveryAddress != null)
					{
						return LocationHelper.GetLocationFromIDocAddress(criteria.DeliveryAddress, factory);
					}
					return null;

				default:
					return null;
			}
		}

		ILocation GetCartageLocation()
		{
			var location = GetCartageLocationFromCriteria();
			if (location != null)
			{
				return location;
			}

			// Otherwise if we have a parent...
			if (line.ParentRateEntry != null)
			{
				switch (line.ChargeCode.AC_ChargeGroup)
				{
					case ChargeCodeGroupList.Codes.Origin:
					case ChargeCodeGroupList.Codes.OriginBrokerage:
					case ChargeCodeGroupList.Codes.OriginBrokerageOnly:
						return line.ParentRateEntry.Origin();

					case ChargeCodeGroupList.Codes.Destination:
					case ChargeCodeGroupList.Codes.Brokerage:
					case ChargeCodeGroupList.Codes.BrokerageOnly:
						return line.ParentRateEntry.Destination();
				}
			}

			return null;
		}

		#endregion

		decimal GetDistanceFromMeasures(ZStringBuilder description)
		{
			var result = 0m;
			var measureDescription = "Job Distance";

			if (criteria.RateTypeToUse == RateType.TransportBookings || criteria.RateTypeToUse == RateType.LocalTransport || (line.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Origin))
			{
				result = criteria.JobMeasures.GetActualLength(MeasureType.PickupDistance, Constants.Length.Kilometres);
				measureDescription = "Pickup Distance";
			}
			else if (line.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Destination)
			{
				result = criteria.JobMeasures.GetActualLength(MeasureType.DeliveryDistance, Constants.Length.Kilometres);
				measureDescription = "Delivery Distance";
			}

			description.AppendLine(Log(measureDescription, result));

			return result;
		}

		decimal GetDistancePostcodeToPostcode(ZStringBuilder description, ILocation cartageLocation)
		{
			var result = 0m;
			var cartageCountry = cartageLocation?.Country;

			// RefLatLongPostcode system data only exists for AU currently. This check should be removed if RefLatLongPostcode is implemented for other countries
			if (cartageCountry != null && cartageCountry.Code == Constants.CountryCodes.Australia)
			{
				RefLatLongPostcode postcode1 = null;
				RefLatLongPostcode postcode2 = null;
				var distanceDescription = "";

				if (criteria.RateTypeToUse == RateType.TransportBookings || criteria.RateTypeToUse == RateType.LocalTransport)
				{
					distanceDescription = ZString.Format(" (Consignor Pickup Address to Consignee Delivery Address)");
					postcode1 = GetLatLongPostcode(criteria.PickupAddress, criteria.ConsignorDocumentaryAddress, cartageCountry);
					postcode2 = GetLatLongPostcode(criteria.DeliveryAddress, criteria.ConsigneeDocumentaryAddress, cartageCountry);
				}
				else
				{
					bool? shouldUseConsignorAddress = ShouldUseConsignorAddress;
					if (shouldUseConsignorAddress.HasValue)
					{
						if (shouldUseConsignorAddress.Value)
						{
							distanceDescription = ZString.Format(" (Consignor Pickup Address to CTO/Wharf)");
							postcode1 = GetLatLongPostcode(criteria.PickupAddress, criteria.ConsignorDocumentaryAddress, cartageCountry);
							postcode2 = GetWharfCTOLatLongPostcode(cartageCountry);
						}
						else
						{
							distanceDescription = ZString.Format(" (Consignee Delivery Address to CTO/Wharf)");
							postcode1 = GetLatLongPostcode(criteria.DeliveryAddress, criteria.ConsigneeDocumentaryAddress, cartageCountry);
							postcode2 = GetWharfCTOLatLongPostcode(cartageCountry);
						}
					}
					else
					{
						distanceDescription = ZString.Format(" (Charge group ({0}) is incompatible with this service)", line.ChargeCode.AC_ChargeGroup);
					}
				}

				if (postcode1 != null && postcode2 != null)
				{
					result = (decimal)RefLatLongPostcode.CalculateDistance(postcode1, postcode2);
				}

				description.AppendLine(Log("Lat/Long Postcode Distance", result, distanceDescription));
			}

			return result;
		}

		#region Distance Calculation Implementation

		decimal GetDistanceFromService(ZStringBuilder description, ILocation cartageLocation)
		{
			if (criteria.RateTypeToUse == RateType.TransportBookings || criteria.RateTypeToUse == RateType.LocalTransport)
			{
				var originAddress = DistanceCalculationHelper.GetAddressFromAddress(criteria.PickupAddress);
				var destinationAddress = DistanceCalculationHelper.GetAddressFromAddress(criteria.DeliveryAddress);

				return GetDistanceFromService(null, originAddress, destinationAddress, description, " (Consignor Pickup Address to Consignee Delivery Address)");
			}

			if (cartageLocation != null)
			{
				IDocAddress originIDocAddress = null;
				OrgHeader clientWithConfiguration = null;
				ZString distanceDescription;

				bool? shouldUseConsignorAddress = ShouldUseConsignorAddress;
				if (shouldUseConsignorAddress.HasValue)
				{
					if (shouldUseConsignorAddress.Value)
					{
						originIDocAddress = criteria.PickupAddress;
						GetClientWithConfiguration(originIDocAddress, criteria.Consignor);
						distanceDescription = " (Consignor Pickup Address to CTO/Wharf)";
					}
					else
					{
						originIDocAddress = criteria.DeliveryAddress;
						clientWithConfiguration = GetClientWithConfiguration(originIDocAddress, criteria.Consignee);
						distanceDescription = " (Consignee Delivery Address to CTO/Wharf)";
					}
				}
				else
				{
					distanceDescription = ZString.Format(" (Charge group ({0}) is incompatible with this service)", line.ChargeCode.AC_ChargeGroup);
				}

				var originAddress = DistanceCalculationHelper.GetAddressFromAddress(originIDocAddress);
				var destinationAddress = GetWharfCTOAddress(cartageLocation.Country);

				return GetDistanceFromService(clientWithConfiguration, originAddress, destinationAddress, description, distanceDescription);
			}
			else
			{
				return GetDistanceFromUNLOCO(description);
			}
		}

		decimal GetDistanceFromUNLOCO(ZStringBuilder description)
		{
			var result = 0m;

			var originUnloco = line.ParentRateEntry?.Origin()?.UNLOCO;
			var destinationUnloco = line.ParentRateEntry?.Destination()?.UNLOCO;

			if (originUnloco != null && destinationUnloco != null)
			{
				var originAddress = DistanceCalculationHelper.GetAddressFromUNLOCO(originUnloco);
				var destinationAddress = DistanceCalculationHelper.GetAddressFromUNLOCO(destinationUnloco);

				result = GetDistanceFromService(null, originAddress, destinationAddress, description, " (Rate Origin and Destination)");
			}
			else
			{
				description.AppendLine(Log("Distance Calculation Service", result, " (no compatible locations)"));
			}

			return result;
		}

		decimal GetDistanceFromService(OrgHeader clientWithConfiguration, DistanceCalculationAddress originAddress, DistanceCalculationAddress destinationAddress, ZStringBuilder description, ZString source)
		{
			var result = 0m;
			if (!originAddress.IsEmpty && !destinationAddress.IsEmpty)
			{
				var config = DistanceCalculationHelper.GetConfigurationFromOrg(clientWithConfiguration);
				config.UnitsForCalculation = DistanceCalculationConstants.UnitsForCalculation.Kilometres;

				var calculationResult = GetDistanceCalculationResult(config, originAddress, destinationAddress);

				result = (decimal)calculationResult.Distance;

				if (!string.IsNullOrEmpty(calculationResult.StatusMessage))
				{
					source += ZString.Format("{0}Error: {1}", Environment.NewLine, calculationResult.StatusMessage);
				}
			}

			description.AppendLine(Log("Distance Calculation Service", result, source));

			return result;
		}

		DistanceCalculationResult GetDistanceCalculationResult(DistanceCalculationConfiguration config, DistanceCalculationAddress originAddress, DistanceCalculationAddress destinationAddress)
		{
			DistanceCalculationResult result;
			var distancesCache = GetDistanceCachedValues();
			var key = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", originAddress, destinationAddress);
			if (!distancesCache.TryGetValue(key, out result))
			{
				result = new DistanceCalculationManager().Calculate(criteria.ConsumerType.DistanceCalculationCheckpoint, Guid.NewGuid(), config, originAddress, destinationAddress);
				distancesCache.Add(key, result);
			}
			return result;
		}

		Dictionary<string, DistanceCalculationResult> GetDistanceCachedValues()
		{
			return factory.GetCachedValue("DistanceCalculationResult", delegate
			{ return new Dictionary<string, DistanceCalculationResult>(); });
		}

		DistanceCalculationAddress GetWharfCTOAddress(RefCountry country)
		{
			var result = new DistanceCalculationAddress();
			result.PostCode = WharfCTOPostCode;
			result.Country = (country != null) ? country.RN_DescMultilingual.ToString() : "";

			return result;
		}

		OrgHeader GetClientWithConfiguration(IDocAddress iDocAddress, OrgHeader fallbackOrg)
		{
			OrgHeader clientWithConfiguration = null;
			if (iDocAddress != null && !iDocAddress.E2_AddressOverride)
			{
				var address = iDocAddress as JobDocAddress;
				if (address != null)
				{
					clientWithConfiguration = address.Organisation;
				}
			}
			else
			{
				clientWithConfiguration = fallbackOrg;
			}

			return clientWithConfiguration;
		}

		#endregion

		#region Postcode from Latitude/Longitude

		RefLatLongPostcode GetLatLongPostcode(ZString postcode, RefCountry country)
		{
			return RefLatLongPostcode.Load(factory, postcode, country);
		}

		RefLatLongPostcode GetLatLongPostcode(IDocAddress address, IDocAddress fallbackAddress, RefCountry country)
		{
			var postcode = address?.E2_Postcode ?? ZString.Empty;
			if (postcode.IsEmpty)
			{
				postcode = fallbackAddress?.E2_Postcode ?? ZString.Empty;
			}
			return !postcode.IsEmpty
				? GetLatLongPostcode(postcode, country)
				: null;
		}

		RefLatLongPostcode GetWharfCTOLatLongPostcode(RefCountry country)
		{
			RefLatLongPostcode result = null;

			if (criteria.WharfCTOAddress != null)
			{
				result = GetLatLongPostcode(criteria.WharfCTOAddress, null, country);
			}

			var postCode = WharfCTOPostCode;
			if (result == null && !postCode.IsEmpty)
			{
				result = GetLatLongPostcode(postCode, country);
			}

			return result;
		}

		#endregion

		ZString WharfCTOPostCode
		{
			get
			{
				var result = ZString.Empty;
				if (criteria.WharfCTOAddress != null)
				{
					result = criteria.WharfCTOAddress.OA_PostCode;
				}
				else
				{
					if (criteria.IsRoadFreight)
					{
						result = Env.Registry.Rating.DefaultCFSAddressRoad;
					}
					else if (criteria.IsAirFreight)
					{
						result = Env.Registry.Rating.DefaultCTOAddressAir;
					}
					else if (criteria.IsRailFreight)
					{
						result = Env.Registry.Rating.DefaultCTOAddressRail;
					}
					else
					{
						result = Env.Registry.Rating.DefaultCTOAddressSea;
					}
				}

				return result;
			}
		}

		bool? ShouldUseConsignorAddress
		{
			get
			{
				switch (line.ChargeCode.AC_ChargeGroup)
				{
					case ChargeCodeGroupList.Codes.Origin:
					case ChargeCodeGroupList.Codes.OriginBrokerage:
					case ChargeCodeGroupList.Codes.OriginBrokerageOnly:
					case ChargeCodeGroupList.Codes.WHSInwards:
						return true;

					case ChargeCodeGroupList.Codes.Destination:
					case ChargeCodeGroupList.Codes.Brokerage:
					case ChargeCodeGroupList.Codes.BrokerageOnly:
					case ChargeCodeGroupList.Codes.WHSOutwards:
						return false;

					default:
						return null;
				}
			}
		}

		static ZString Log(string methodDescription, decimal distance, string distanceSource = "")
		{
			var distanceDescription = distance == 0m ? "empty" : distance.ToString(Culture.CurrentCompanyCountryCulture);

			return ZString.Format("- {0}: {1}{2}", methodDescription, distanceDescription, distanceSource);
		}

		#endregion
	}
}

