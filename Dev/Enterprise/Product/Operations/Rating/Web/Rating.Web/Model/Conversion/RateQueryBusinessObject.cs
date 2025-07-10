using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Web.Model.Conversion
{
	/// <summary>
	/// To be able to create a RatingAdapter based on RateQuery, we need to present RateQuery as a BusinessObject.
	/// This class is a wrapper around RateQuery that present it as a business object. 
	/// </summary>
	public class RateQueryBusinessObject : NonPersistentBusinessObject
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="rateQuery"></param>
		/// <param name="sourceEndpoint"></param>
		public RateQueryBusinessObject(BusinessObjectFactory factory, RateQuery rateQuery, SourceEndpoint sourceEndpoint) : base(factory)
		{
			RateQuery = Argument.NotNull(rateQuery, nameof(rateQuery));
			SourceEndpoint = Argument.NotNull(sourceEndpoint, nameof(sourceEndpoint));
		}

		internal readonly SourceEndpoint SourceEndpoint;

		internal readonly RateQuery RateQuery;

		/// <summary>
		/// Origin
		/// </summary>
		public ILocation Origin
		{
			get
			{
				var location = LocationHelper.GetCachedLocationFromString(RateQuery.Origin?.Value, Factory);

				if (location == null && !string.IsNullOrEmpty(RateQuery.Origin?.Value))
				{
					var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(Origin)}:{RateQuery.Origin?.Value}";
					var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.Origin)}.{nameof(Location.Value)} ({RateQuery.Origin?.Value}) is not a valid Location in CW1.";
					ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
				}

				return location;
			}
		}

		/// <summary>
		/// Pickup address from provided organisation code and address code, or an overridden address from provided City and/or Postcode
		/// </summary>
		public IDocAddress PickupAddress =>
			GetAddress
			(
				ref isPickupAddressCalculated,
				ref pickupAddress,
				DocAddressType.ConsignorPickupDeliveryAddress,
				RateQuery.PickupCity,
				RateQuery.PickupPostcode,
				RateQuery.PickupOrg,
				RateQuery.PickupAddrCode,
				$"{nameof(PickupAddress)}.{nameof(RateQuery.PickupOrg)}",
				$"{nameof(RateQuery.PickupOrg)}",
				$"{nameof(PickupAddress)}.{nameof(RateQuery.PickupAddrCode)}",
				$"{nameof(RateQuery.PickupAddrCode)}"
			);

		JobDocAddress pickupAddress;
		bool isPickupAddressCalculated;

		/// <summary>
		/// Delivery address from provided organisation code and address code or an overridden address from provided City and/or Postcode
		/// </summary>
		public IDocAddress DeliveryAddress =>
			GetAddress
			(
				ref isDeliveryAddressCalculated,
				ref deliveryAddress,
				DocAddressType.ConsigneePickupDeliveryAddress,
				RateQuery.DeliveryCity,
				RateQuery.DeliveryPostcode,
				RateQuery.DeliveryOrg,
				RateQuery.DeliveryAddrCode,
				$"{nameof(DeliveryAddress)}.{nameof(RateQuery.DeliveryOrg)}",
				$"{nameof(RateQuery.DeliveryOrg)}",
				$"{nameof(DeliveryAddress)}.{nameof(RateQuery.DeliveryAddrCode)}",
				$"{nameof(RateQuery.DeliveryAddrCode)}"
			);

		JobDocAddress deliveryAddress;
		bool isDeliveryAddressCalculated;

		IDocAddress GetAddress
		(
			ref bool isCalculated,
			ref JobDocAddress calculatedAddress,
			DocAddressType docAddressType,
			string city,
			string postcode,
			string organisationCode,
			string addressCode,
			string organisationCodePath,
			string organisationCodeFieldName,
			string addressCodePath,
			string addressCodeFieldName)
		{
			if (isCalculated)
			{
				return calculatedAddress;
			}

			// Overridden address
			if (!string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(postcode))
			{
				calculatedAddress = Factory.New<JobDocAddress>();
				// By default, when address is overridden then the country in the address is the current login country
				// See: constructor of JobDocAddress, GetDefaultCountryCodeIfEmpty
				calculatedAddress.E2_AddressOverride = true;
				calculatedAddress.E2_City = city;
				calculatedAddress.E2_Postcode = postcode;
				calculatedAddress.DocAddressType = docAddressType;

				isCalculated = true;
				return calculatedAddress;
			}

			if (string.IsNullOrWhiteSpace(organisationCode) && string.IsNullOrWhiteSpace(addressCode))
			{
				isCalculated = true;
				return null;
			}

			var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, organisationCode);
			if (org == null)
			{
				ExtraValidationLogger.LogExtraValidationMessageOnce(
					$"{nameof(RateQueryBusinessObject)}.{organisationCodePath}:{organisationCode}",
					$"Provided {nameof(RateQuery)}.{organisationCodeFieldName} ({organisationCode}) is not a valid organisation in CW1.");

				isCalculated = true;
				return null;
			}

			var orgAddress = org.Addresses
				.Find(new ZQuery(OrgAddressSchema.OA_Code, addressCode))
				.FirstOrDefault() as OrgAddress;
			if (orgAddress == null)
			{
				ExtraValidationLogger.LogExtraValidationMessageOnce(
					$"{nameof(RateQueryBusinessObject)}.{addressCodePath}:{addressCode}",
					$"Provided {nameof(RateQuery)}.{addressCodeFieldName} ({addressCode}) is not a recognised address for this organisation.");

				isCalculated = true;
				return null;
			}

			// Org address
			calculatedAddress = Factory.New<JobDocAddress>();
			calculatedAddress.E2_OA_Address = orgAddress.PK;

			isCalculated = true;
			return calculatedAddress;
		}

		/// <summary>
		/// Via
		/// </summary>
		public ILocation Via
		{
			get
			{
				var location = LocationHelper.GetCachedLocationFromString(RateQuery.Via?.Value, Factory);
				if (location == null && !string.IsNullOrEmpty(RateQuery.Via?.Value))
				{
					var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(Origin)}:{RateQuery.Via?.Value}";
					var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.Via)}.{nameof(Location.Value)} ({RateQuery.Via?.Value}) is not a valid Location in CW1.";
					ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
				}
				return location;
			}
		}

		/// <summary>
		/// Destination
		/// </summary>
		public ILocation Destination
		{
			get
			{
				var location = LocationHelper.GetCachedLocationFromString(RateQuery.Destination?.Value, Factory);
				if (location == null && !string.IsNullOrEmpty(RateQuery.Destination?.Value))
				{
					var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(Origin)}:{RateQuery.Destination?.Value}";
					var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.Destination)}.{nameof(Location.Value)} ({RateQuery.Destination?.Value}) is not a valid Location in CW1.";
					ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
				}
				return location;
			}
		}

		/// <summary>
		/// PlannedLoad
		/// </summary>
		public ILocation PlannedLoad
		{
			get
			{
				var location = LocationHelper.GetCachedLocationFromString(RateQuery.PlannedLoad?.Value, Factory);
				if (location == null && !string.IsNullOrEmpty(RateQuery.PlannedLoad?.Value))
				{
					var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(Origin)}:{RateQuery.PlannedLoad?.Value}";
					var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.PlannedLoad)}.{nameof(Location.Value)} ({RateQuery.PlannedLoad?.Value}) is not a valid Location in CW1.";
					ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
				}
				return location;
			}
		}

		/// <summary>
		/// PlannedDischarge
		/// </summary>
		public ILocation PlannedDischarge
		{
			get
			{
				var location = LocationHelper.GetCachedLocationFromString(RateQuery.PlannedDischarge?.Value, Factory);
				if (location == null && !string.IsNullOrEmpty(RateQuery.PlannedDischarge?.Value))
				{
					var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(Origin)}:{RateQuery.PlannedDischarge?.Value}";
					var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.PlannedDischarge)}.{nameof(Location.Value)} ({RateQuery.PlannedDischarge?.Value}) is not a valid Location in CW1.";
					ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
				}
				return location;
			}
		}

		/// <summary>
		/// RateOrigin
		/// </summary>
		public ILocation RateOrigin
		{
			get
			{
				var location = LocationHelper.GetCachedLocationFromString(RateQuery.RateOrigin?.Value, Factory);
				if (location == null && !string.IsNullOrEmpty(RateQuery.RateOrigin?.Value))
				{
					var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(Origin)}:{RateQuery.RateOrigin?.Value}";
					var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.RateOrigin)}.{nameof(Location.Value)} ({RateQuery.RateOrigin?.Value}) is not a valid Location in CW1.";
					ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
				}
				return location;
			}
		}

		/// <summary>
		/// RateDestination
		/// </summary>
		public ILocation RateDestination
		{
			get
			{
				var location = LocationHelper.GetCachedLocationFromString(RateQuery.RateDestination?.Value, Factory);
				if (location == null && !string.IsNullOrEmpty(RateQuery.RateDestination?.Value))
				{
					var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(Origin)}:{RateQuery.RateDestination?.Value}";
					var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.RateDestination)}.{nameof(Location.Value)} ({RateQuery.RateDestination?.Value}) is not a valid Location in CW1.";
					ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
				}
				return location;
			}
		}

		public ILocation GetRelatedLocation(ZString code)
		{
			var queryLocation = RateQuery.Locations?.FirstOrDefault(loc => loc.RelatedField == code);
			if (queryLocation == null)
			{
				return null;
			}

			var location = LocationHelper.GetCachedLocationFromString(queryLocation.Value, Factory);
			if (location == null && !string.IsNullOrEmpty(queryLocation.Value))
			{
				var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(Origin)}:{queryLocation.Value}";
				var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.Locations)}.{nameof(Location.Value)} ({queryLocation.Value}) is not a valid Location in CW1.";
				ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
			}

			return location;
		}

		/// <summary>
		/// RatingAdapter
		/// </summary>
		public IAutoRating RatingAdapter
		{
			get { return ratingAdapter ?? (ratingAdapter = new RateQueryRatingAdapter(this)); }
		}
		IAutoRating ratingAdapter;

		/// <summary>
		/// ServiceProviders
		/// </summary>
		public IReadOnlyCollection<OrgHeader> ServiceProviders =>
			serviceProviders ?? (serviceProviders = RateQuery.ServiceProviders?.ResolveOrganisations(Factory) ?? Array.Empty<OrgHeader>());

		IReadOnlyCollection<OrgHeader> serviceProviders;

		/// <summary>
		/// Carrier
		/// </summary>
		public OrgHeader Carrier
		{
			get
			{
				if (carrier == null)
				{
					if (RateQuery.Carriers?.Any() ?? false)
					{
						carrier = RateQuery.Carriers.FirstOrDefault()?.ResolveOrganisation(Factory);
					}
					else if (this.GetResolvedRateParties().TryGetValue(OrganisationRole.Roles.CAR, out var oldVersionCarrier)) // to support old clients
					{
						carrier = oldVersionCarrier;
					}
				}

				return carrier;
			}
		}

		OrgHeader carrier;

		/// <summary>
		/// PossibleCarriers
		/// </summary>
		public IReadOnlyCollection<OrgHeader> PossibleCarriers =>
			possibleCarriers ?? (possibleCarriers = RateQuery.Carriers?.Skip(1).ResolveOrganisations(Factory) ?? Array.Empty<OrgHeader>());

		IReadOnlyCollection<OrgHeader> possibleCarriers;

		/// <summary>
		/// CarrierContractNumbers
		/// </summary>
		public IEnumerable<ZString> CarrierContractNumbers
		{
			get
			{
				return RateQuery.CarrierContracts?.Where(cc => cc != null).Select(cc => new ZString(cc)) ?? Enumerable.Empty<ZString>();
			}
		}

		/// <summary>
		/// ClientContractNumbers
		/// </summary>
		public IEnumerable<ZString> ClientContractNumbers
		{
			get
			{
				return RateQuery.ClientContracts?.Where(cc => cc != null).Select(cc => new ZString(cc)) ?? Enumerable.Empty<ZString>();
			}
		}

		/// <summary>
		/// NamedAccounts
		/// </summary>
		public IEnumerable<string> NamedAccounts
		{
			get
			{
				return
					RateQuery
					.NamedAccounts?
					.Where(a => NamedAccount.Types.NamedAccount.Equals(a?.Type) && !string.IsNullOrEmpty(a?.Value))?
					.Select(a => a.Value) ?? Enumerable.Empty<string>();
			}
		}

		/// <summary>
		/// FreightMode
		/// </summary>
		public FreightMode FreightMode
		{
			get
			{
				return FreightRatingHelper.CalculateFreightMode(RateQuery.TransportMode, RateQuery.ContainerMode);
			}
		}

		/// <summary>
		/// CompanyTariffLevelOverride
		/// </summary>
		public ZByte CompanyTariffLevelOverride
		{
			get
			{
				if (((RateQuery.JobInfo?.CTLevelOverride ?? 0) != 0) && DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.Value)
				{
					var companyTariffLevelOverrideList = CompanyTariffLevelOverrideList.CompanyTariffLevelOverrideList;
					if (companyTariffLevelOverrideList.ContainsCode(RateQuery.JobInfo.CTLevelOverride))
					{
						return (ZByte)RateQuery.JobInfo.CTLevelOverride;
					}
					var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(CompanyTariffLevelOverride)}:{RateQuery.JobInfo.CTLevelOverride}";
					var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.JobInfo)}.{nameof(RateQuery.JobInfo.CTLevelOverride)}. ({RateQuery.JobInfo.CTLevelOverride}) is not a valid Company Tariff Level in CW1.";
					ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
				}
				return 0;
			}
		}

		public CompanyTariffLevelList CompanyTariffLevelOverrideList
		{
			get
			{
				if (companyTariffLevelList == null)
				{
					companyTariffLevelList = new CompanyTariffLevelList(Factory);
				}
				return companyTariffLevelList;
			}
		}

		CompanyTariffLevelList companyTariffLevelList;

		/// <summary>
		/// TransportMode
		/// </summary>
		public ZString TransportMode => RateQuery.TransportMode;

		/// <summary>
		/// ContainerMode
		/// </summary>
		public ZString ContainerMode => RateQuery.ContainerMode;

		/// <summary>
		/// HBL Delivery Mode
		/// </summary>
		public ZString HBLDeliveryMode => RateQuery.HBLDeliveryMode;

		/// <summary>
		/// FMC Tariff ID
		/// </summary>
		public ZString FMCTariffID => RateQuery.FMCTariffID;

		/// <summary>
		/// CarrierServiceLevels
		/// </summary>
		public IEnumerable<ServiceLevelInfo> CarrierServiceLevels
		{
			get
			{
				return
					RateQuery
					.CarrierServiceLevels?
					.Where(c => !string.IsNullOrEmpty(c.CWCode))
					.Select(c => new ServiceLevelInfo(c.CWCode, ServiceLevelType.Carrier)) ?? Enumerable.Empty<ServiceLevelInfo>();
			}
		}

		/// <summary>
		/// ClientServiceLevels
		/// </summary>
		public IEnumerable<ServiceLevelInfo> ClientServiceLevels
		{
			get
			{
				return
					RateQuery
					.ServiceLevels?
					.Where(sl => !string.IsNullOrEmpty(sl))
					.Select(sl => new ServiceLevelInfo(sl, ServiceLevelType.Client)) ?? Enumerable.Empty<ServiceLevelInfo>();
			}
		}

		/// <summary>
		/// UniversalCarrierServiceLevels
		/// </summary>
		public IEnumerable<string> UniversalCarrierServiceLevels
		{
			get
			{
				return
					RateQuery
					.CarrierServiceLevels?
					.Where(c => !string.IsNullOrEmpty(c?.UniversalCode))
					.Select(c => c.UniversalCode) ?? Enumerable.Empty<string>();
			}
		}

		/// <summary>
		/// GatewayServiceLevels
		/// </summary>
		public IEnumerable<ServiceLevelInfo> GatewayServiceLevels
		{
			get
			{
				return
					RateQuery
					.GatewayServiceLevels?
					.WhereNotNull()
					.Select(x => new ServiceLevelInfo(x, ServiceLevelType.Gateway)) ?? Enumerable.Empty<ServiceLevelInfo>();
			}
		}

		/// <summary>
		/// ShipmentGatewayServiceLevels
		/// </summary>
		public IEnumerable<ServiceLevelInfo> ShipmentGatewayServiceLevels
		{
			get
			{
				return
					RateQuery
					.ShipmentGatewayServiceLevels?
					.WhereNotNull()
					.Select(x => new ServiceLevelInfo(x, ServiceLevelType.Gateway)) ?? Enumerable.Empty<ServiceLevelInfo>();
			}
		}

		/// <summary>
		/// Commodities
		/// </summary>
		public Dictionary<string, RefCommodityCode> Commodities
		{
			get
			{
				if (commodities == null)
				{
					string[] cw1CommodityCodes = null;

					if (SourceEndpoint == SourceEndpoint.JobCharges)
					{
						cw1CommodityCodes = RateQuery.IsContainerized ?
							cw1CommodityCodes = RateQuery.JobInfo?.Containers?.Where(c => !string.IsNullOrEmpty(c?.Commodity))?.Select(c => c.Commodity)?.ToArray() ?? System.Array.Empty<string>() :
							cw1CommodityCodes = RateQuery.JobInfo?.Containers?.SelectMany(c => c?.PackLines?.Where(p => !string.IsNullOrEmpty(p?.Commodity))?.Select(p => p.Commodity) ?? System.Array.Empty<string>())?.ToArray() ?? System.Array.Empty<string>();
					}
					else
					{
						cw1CommodityCodes = RateQuery.Commodities?.Where(c => !string.IsNullOrEmpty(c?.CWCode))?.Select(c => c.CWCode)?.ToArray() ?? System.Array.Empty<string>();
					}

					var query = new ZDBOnlyQuery(typeof(RefCommodityCode));
					query.AddToFilter(RefCommodityCodeSchema.RH_Code, cw1CommodityCodes);
					commodities = Factory.Load<RefCommodityCode>(query).ToDictionary(rc => rc.RH_Code.ToString());
				}

				return commodities;
			}
		}

		Dictionary<string, RefCommodityCode> commodities;

		/// <summary>
		/// UniversalCommodityGroups
		/// </summary>
		public IEnumerable<string> UniversalCommodityGroups
		{
			get
			{
				if (universalCommodityGroups == null)
				{
					var groups = new HashSet<string>();
					foreach (var commodity in Commodities.Values)
					{
						if (!string.IsNullOrWhiteSpace(commodity.RH_UniversalCommodityGroup))
						{
							groups.Add(commodity.RH_UniversalCommodityGroup);
							continue;
						}

						if (commodity.RH_IsHazardous)
						{
							groups.Add(RefCommodityCode.HAZD);
						}
						if (commodity.RH_IsPerishable)
						{
							groups.Add(RefCommodityCode.PERS);
						}
						if (commodity.RH_IsTimber)
						{
							groups.Add(RefCommodityCode.TIMB);
						}
						if (commodity.RH_IsFlammable)
						{
							groups.Add(RefCommodityCode.FLAM);
						}
						if (commodity.RH_ContainerVentRequired)
						{
							groups.Add(RefCommodityCode.CNVT);
						}
					}

					groups.UnionWith(RateQuery.Commodities?.Where(c => !string.IsNullOrEmpty(c.UniversalCommodityGroup))?.Select(c => c.UniversalCommodityGroup) ?? Enumerable.Empty<string>());

					universalCommodityGroups = groups.ToArray();
				}

				return universalCommodityGroups;
			}
		}
		IEnumerable<string> universalCommodityGroups;

		/// <summary>
		/// ContainerTypes
		/// </summary>
		public Dictionary<string, RefContainer> ContainerTypes
		{
			get
			{
				if (containerTypes == null)
				{
					var zQuery = new ZDBOnlyQuery(typeof(RefContainer));
					zQuery.IsNoResultQuery = true;

					string[] cw1Codes = null;

					if (SourceEndpoint != SourceEndpoint.JobCharges)
					{
						cw1Codes = RateQuery.ContainerTypes?.Where(ct => !string.IsNullOrEmpty(ct?.CWCode))?.Select(ct => ct.CWCode).ToArray() ?? System.Array.Empty<string>();
					}
					else
					{
						cw1Codes = RateQuery.JobInfo?.Containers?.Where(c => !string.IsNullOrEmpty(c.ContainerTypeCWCode))?.Select(c => c.ContainerTypeCWCode).ToArray() ?? System.Array.Empty<string>();
					}

					if (cw1Codes.Any())
					{
						zQuery.AddToFilter(RefContainerSchema.RC_Code, cw1Codes);
						zQuery.IsNoResultQuery = false;
					}

					if (SourceEndpoint != SourceEndpoint.JobCharges)
					{
						var isoTypes = RateQuery.ContainerTypes?.Where(ct => !string.IsNullOrEmpty(ct?.ISOType)).Select(ct => ct.ISOType).ToArray() ?? System.Array.Empty<string>();
						if (isoTypes.Any())
						{
							zQuery.AddToFilter(JoinCondition.Or, RefContainerSchema.RC_ISOType, isoTypes);
							zQuery.IsNoResultQuery = false;
						}
					}

					containerTypes = Factory.Load<RefContainer>(zQuery).ToDictionary(c => c.RC_Code.ToString());
				}

				return containerTypes;
			}
		}

		Dictionary<string, RefContainer> containerTypes;

		/// <summary>
		/// IsDomesticFreightForChargeableWeightCalculations
		/// </summary>
		public bool IsDomesticFreightForChargeableWeightCalculations
		{
			get
			{
				return ImportExportHelper.IsDomestic(new ZString(RateQuery.Origin?.Value), new ZString(RateQuery.Destination?.Value)) || AreBothPortsInUnitedStatesOrUSOverseasTerritories();
			}
		}

		bool AreBothPortsInUnitedStatesOrUSOverseasTerritories()
		{
			var originCountry = new ZString(RateQuery.Origin?.Value).SubstringSafe(0, 2);
			var destinationCountry = new ZString(RateQuery.Destination?.Value).SubstringSafe(0, 2);

			return Constants.CountryCodes.IsUsaOrTerritory(originCountry)
				&& Constants.CountryCodes.IsUsaOrTerritory(destinationCountry);
		}

		/// <summary>
		/// IsContainerized
		/// </summary>
		public bool IsContainerized => RateQuery.IsContainerized;

		/// <summary>
		/// JobDirection
		/// </summary>
		public Directions JobDirection
		{
			get { return ImportExportHelper.GetJobDirection(RateQuery.Origin?.Value, RateQuery.Destination?.Value); }
		}

		/// <summary>
		/// ConsolPaymentTerms
		/// </summary>
		public PaymentTermInfos ConsolPaymentTerms
		{
			get
			{
				var infos = new PaymentTermInfos();
				if (!string.IsNullOrWhiteSpace(RateQuery.CarrierPayTerm))
				{
					infos.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Cost, RateQuery.CarrierPayTerm));
					infos.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Revenue, RateQuery.CarrierPayTerm));
				}

				return infos;
			}
		}

		/// <summary>
		/// GoodsValue
		/// </summary>
		public ZDecimal GoodsValue
		{
			get
			{
				if (RateQuery.JobInfo?.GoodsValue > 0)
				{
					return new ZDecimal(RateQuery.JobInfo.GoodsValue);
				}
				return ZDecimal.Zero;
			}
		}

		/// <summary>
		/// GoodsValueCurrency
		/// </summary>
		public ICurrency GoodsValueCurrency
		{
			get
			{
				if (!string.IsNullOrEmpty(RateQuery.JobInfo?.GoodsValueCurrency))
				{
					var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, RateQuery.JobInfo.GoodsValueCurrency);
					if (currency == null)
					{
						var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(GoodsValueCurrency)}:{RateQuery.JobInfo.GoodsValueCurrency}";
						var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.JobInfo)}.{nameof(JobInfo.GoodsValueCurrency)} ({RateQuery.JobInfo.GoodsValueCurrency}) is not a valid Currency Code in CW1.";
						ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
					}
					return currency;
				}
				return null;
			}
		}

		/// <summary>
		/// InsuranceValue
		/// </summary>
		public ZDecimal InsuranceValue
		{
			get
			{
				if (RateQuery.JobInfo?.InsuranceValue > 0)
				{
					return new ZDecimal(RateQuery.JobInfo.InsuranceValue);
				}
				return ZDecimal.Zero;
			}
		}

		/// <summary>
		/// InsuranceValueCurrency
		/// </summary>
		public ICurrency InsuranceValueCurrency
		{
			get
			{
				if (!string.IsNullOrEmpty(RateQuery.JobInfo?.InsuranceValueCurrency))
				{
					var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, RateQuery.JobInfo.InsuranceValueCurrency);

					if (currency == null)
					{
						var logKey = $"{nameof(RateQueryBusinessObject)}.{nameof(InsuranceValueCurrency)}:{RateQuery.JobInfo.InsuranceValueCurrency}";
						var logMessage = $"Provided {nameof(RateQuery)}.{nameof(RateQuery.JobInfo)}.{nameof(JobInfo.InsuranceValueCurrency)} ({RateQuery.JobInfo.InsuranceValueCurrency}) is not a valid Currency Code in CW1.";
						ExtraValidationLogger.LogExtraValidationMessageOnce(logKey, logMessage);
					}

					return currency;
				}
				return null;
			}
		}

		/// <summary>
		/// CustomsValue
		/// </summary>
		public ZDecimal CustomsValue
		{
			get
			{
				if (RateQuery.JobInfo?.CustomsValue > 0)
				{
					return new ZDecimal(RateQuery.JobInfo.CustomsValue);
				}
				return ZDecimal.Zero;
			}
		}

		/// <summary>
		/// Custom Fields
		/// </summary>
		public Dictionary<string, string> CustomFields
		{
			get
			{
				return RateQuery?.JobInfo?.CustomFields?.ToDictionary(cf => cf.Name, cf => cf.Value);
			}
		}
	}
}
