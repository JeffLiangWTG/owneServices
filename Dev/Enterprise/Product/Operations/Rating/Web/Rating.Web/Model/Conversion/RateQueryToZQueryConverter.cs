using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Web.Model.Conversion
{
	/// <summary>
	/// This is a utility class to build a ZQuery object from RateQuery to be able to fetch CW1 rates from DB.
	/// </summary>
	public class RateQueryToZQueryConverter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="logger"></param>
		public RateQueryToZQueryConverter(BusinessObjectFactory factory, ILogger logger)
		{
			Factory = factory;

			Logger = Argument.NotNull(logger, nameof(logger));
		}

		BusinessObjectFactory Factory { get; }
		ILogger Logger { get; }

		/// <summary>
		/// Convert
		/// </summary>
		/// <param name="companyPK"></param>
		/// <param name="rateQuery"></param>
		/// <param name="sourceEndpoint"></param>
		/// <returns></returns>
		public ZQuery Convert(ZGuid companyPK, RateQuery rateQuery, SourceEndpoint sourceEndpoint)
		{
			if
				(
					sourceEndpoint == SourceEndpoint.Costing
					&& !(rateQuery.RateProviders?.Contains(RatesAPIsConstants.RateProviders.CargoWise) ?? false)
				)
			{
				return new ZQuery() { IsNoResultQuery = true };
			}

			var filtersBusinessObject = new RateEntryFilterStripForConvertBusinessObject(sourceEndpoint);

			var queryShouldReturnsNoResults = false;

			AddOriginDestinationFilterStrip(filtersBusinessObject, rateQuery);
			AddViaFilterStrip(filtersBusinessObject, rateQuery);
			AddLocationFilterStrip(filtersBusinessObject, rateQuery);
			AddCarrierContractsFilterStrip(filtersBusinessObject, rateQuery, sourceEndpoint);
			AddClientContractsFilterStrip(filtersBusinessObject, rateQuery, sourceEndpoint);
			AddEffectiveDateFilterStrip(filtersBusinessObject, rateQuery);
			AddCommoditiesFilterStrip(filtersBusinessObject, rateQuery);
			AddCarrierServiceLevelsFilterStrip(filtersBusinessObject, rateQuery);
			AddServiceLevelsFilterStrip(filtersBusinessObject, rateQuery);
			AddGatewayServiceLevelsFilterStrip(filtersBusinessObject, rateQuery);
			AddShipmentGatewayServiceLevelsFilterStrip(filtersBusinessObject, rateQuery);
			AddPlannedLoadDischargeFilterStrip(filtersBusinessObject, rateQuery, sourceEndpoint);
			AddRateOriginDestinationFilterStrip(filtersBusinessObject, rateQuery, sourceEndpoint);

			if (rateQuery.NamedAccounts?.Where(a => NamedAccount.Types.CargoWise.Equals(a?.Type)).Any() ?? false)
			{
				var cw1NamedAccounts = GetResolvedCW1NamedAccounts(rateQuery);
				if (cw1NamedAccounts.Any())
				{
					AddNamedAccountsFilterStrip(filtersBusinessObject, cw1NamedAccounts);
				}
				else
				{
					queryShouldReturnsNoResults = true;
				}
			}

			if (!string.IsNullOrEmpty(rateQuery.CW1RateMode))
			{
				AddCW1RateModeFilterStrip(filtersBusinessObject, rateQuery);
			}
			else
			{
				queryShouldReturnsNoResults = true;
			}

			if (!rateQuery.Carriers.IsNullOrEmpty())
			{
				var carrierPKs =
					rateQuery.Carriers?.ResolveOrganisations(Factory).Select(x => x.PK).ToArray()
					?? Array.Empty<ZGuid>();

				if (carrierPKs.Any())
				{
					foreach (var carrierPK in carrierPKs)
					{
						var filter = filtersBusinessObject.AddFilterStrip<ModuleGuidFilter>(RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider);
						filter.IsActive = true;
						filter.OrCategory = FilterOrCategory.AntiqueWhite;
						filter.Property = carrierPK;
					}
				}
				else
				{
					queryShouldReturnsNoResults = true;
				}
			}

			if (rateQuery.ContainerTypes?.Any() ?? false)
			{
				var containerTypePKs = GetResolvedQueryContainerTypes(rateQuery);

				if (containerTypePKs.Any())
				{
					AddContainerTypesFilterStrip(filtersBusinessObject, containerTypePKs);
				}
				else
				{
					queryShouldReturnsNoResults = true;
				}
			}

			var serviceProviderFilter = GetServiceProviderFilter(companyPK, sourceEndpoint.GetRateType(), rateQuery);

			queryShouldReturnsNoResults = queryShouldReturnsNoResults || serviceProviderFilter.IsNoResultQuery;

			var zQuery = filtersBusinessObject.Filter.DeepClone();

			zQuery.AddToFilter(serviceProviderFilter, JoinCondition.And);
			zQuery.AddToFilter(GetPostcodeFilter(rateQuery), JoinCondition.And);
			zQuery.AddToFilter(GetALLTransportModeFilter(rateQuery), JoinCondition.And);
			zQuery.AddToFilter(GetFilterToRemoveFCLRates(rateQuery), JoinCondition.And);

			if (sourceEndpoint != SourceEndpoint.Costing)
			{
				zQuery.AddToFilter(GetPaymentTermOverrideFilter(rateQuery), JoinCondition.And);
			}

			if (sourceEndpoint != SourceEndpoint.Costing && sourceEndpoint != SourceEndpoint.IntercompanyTariffs)
			{
				zQuery.AddToFilter(GetHBLDeliveryModeFilter(rateQuery), JoinCondition.And);
			}

			if (sourceEndpoint == SourceEndpoint.CompanyTariffs || sourceEndpoint == SourceEndpoint.ClientRates || sourceEndpoint == SourceEndpoint.JobCharges)
			{
				zQuery.AddToFilter(GetFMCTariffIDFilter(rateQuery), JoinCondition.And);
			}

			if (rateQuery.CTLevel >= 0 && sourceEndpoint == SourceEndpoint.CompanyTariffs)
			{
				zQuery.AddToFilter(GetCompanyTariffsLevelFilter(rateQuery), JoinCondition.And);
			}

			zQuery.IsNoResultQuery = zQuery.IsNoResultQuery || queryShouldReturnsNoResults;

			// This method should be enhanced to apply more filters for endpoints rather than costing :
			/*
			 * RateOrigin
			 * RateDestination
			 * 
			 */
			return zQuery;
		}

		#region Implementation

		ZQuery GetALLTransportModeFilter(RateQuery rateQuery)
		{
			var zQuery = new ZDBOnlyQuery(typeof(RateEntry));
			if (rateQuery.CW1RateMode == Core.Constants.RateMode.ALL)
			{
				zQuery.AddToFilter(RateEntrySchema.TI_Mode, Core.Constants.RateMode.ALL);
			}
			return zQuery;
		}

		ZQuery GetCompanyTariffsLevelFilter(RateQuery rateQuery)
		{
			var zQuery = new ZDBOnlyQuery(typeof(RateEntry));
			var ratingHeaderSubQuery = new ZDBOnlySubQuery(typeof(RatingHeader), RatingHeaderSchema.PK);
			ratingHeaderSubQuery.AddToFilter(RatingHeaderSchema.TH_GlobalRateLevel, (byte)(rateQuery.CTLevel ?? 0));
			zQuery.AddSubQuery(RateEntrySchema.TI_TH, ratingHeaderSubQuery, JoinCondition.And);
			return zQuery;
		}

		ZQuery GetFilterToRemoveFCLRates(RateQuery rateQuery)
		{
			var zQuery = new ZDBOnlyQuery(typeof(RateEntry));
			var modesToRemoveFCLRates = new[] {
				RateEntryFilterUtility.TransportModes.AIF,
				RateEntryFilterUtility.TransportModes.SEF,
				RateEntryFilterUtility.TransportModes.RAF,
				RateEntryFilterUtility.TransportModes.ROF
			};

			if (modesToRemoveFCLRates.Contains(rateQuery.CW1RateMode))
			{
				zQuery.AddToFilter(RateEntrySchema.TI_RateCategory, SQLComparisonOperator.NotEqual, RatingConstants.RateCategory.FCL);
			}

			return zQuery;
		}

		ZQuery GetPostcodeFilter(RateQuery rateQuery)
		{
			var zQuery = new ZDBOnlyQuery(typeof(RateEntry));

			if (!string.IsNullOrEmpty(rateQuery.Origin?.Value) && rateQuery.Origin.IsPostcode)
			{
				zQuery.AddToFilter(RateEntrySchema.TI_CartagePickupAddressPostCode, rateQuery.Origin.Value);
			}

			if (!string.IsNullOrEmpty(rateQuery.Destination?.Value) && rateQuery.Destination.IsPostcode)
			{
				zQuery.AddToFilter(RateEntrySchema.TI_CartageDeliveryAddressPostCode, rateQuery.Destination.Value);
			}

			return zQuery;
		}

		ZQuery GetHBLDeliveryModeFilter(RateQuery rateQuery)
		{
			var zQuery = new ZDBOnlyQuery(typeof(RateEntry));

			if (!string.IsNullOrWhiteSpace(rateQuery.HBLDeliveryMode))
			{
				zQuery.AddToFilter(RateEntrySchema.TI_HBLDeliveryMode, rateQuery.HBLDeliveryMode);
			}

			return zQuery;
		}

		ZQuery GetFMCTariffIDFilter(RateQuery rateQuery)
		{
			var zQuery = new ZDBOnlyQuery(typeof(RateEntry));

			if (!string.IsNullOrWhiteSpace(rateQuery.FMCTariffID))
			{
				zQuery.AddToFilter(RateEntrySchema.TI_FMCTariffID, rateQuery.FMCTariffID);
			}

			return zQuery;
		}

		ZQuery GetPaymentTermOverrideFilter(RateQuery rateQuery)
		{
			var zQuery = new ZDBOnlyQuery(typeof(RateEntry));

			if (!string.IsNullOrEmpty(rateQuery.PaymentTermOverride))
			{
				zQuery.AddToFilter(RateEntrySchema.TI_PaymentTerm, string.Empty);
				zQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_PaymentTerm, rateQuery.PaymentTermOverride);
			}

			return zQuery;
		}

		ZQuery GetServiceProviderFilter(ZGuid companyPK, string rateType, RateQuery rateQuery)
		{
			var rateEntryQuery = new ZDBOnlyQuery(typeof(RateEntry));
			var ratingHeaderSubQuery = new ZDBOnlySubQuery(typeof(RatingHeader), RatingHeaderSchema.PK);

			ApplyCompanyFilter(ratingHeaderSubQuery, companyPK);
			ApplyRateTypeFilter(ratingHeaderSubQuery, rateType);
			ApplyClientFilter(ratingHeaderSubQuery, rateType, rateQuery.Client);

			var queryShouldReturnsNoResults = ratingHeaderSubQuery.IsNoResultQuery;

			if (rateQuery.ServiceProviders?.Any() ?? false)
			{
				var resolvedServiceProviders = GetResolvedQueryServiceProviders(rateQuery);

				if (resolvedServiceProviders.Any())
				{
					if (rateType == RatingConstants.RatingHeaderTypes.IntercompanyTariff)
					{
						ratingHeaderSubQuery.AddToFilter(RatingHeaderSchema.TH_OH, resolvedServiceProviders);
					}
					else if (rateType == RatingConstants.RatingHeaderTypes.Tariff)
					{
						ratingHeaderSubQuery.AddToFilter(RateEntrySchema.TI_OH_Supplier, resolvedServiceProviders);
					}
					else if (rateType == RatingConstants.RatingHeaderTypes.ClientRate)
					{
						ratingHeaderSubQuery.AddToFilter(RateEntrySchema.TI_OH_Supplier, resolvedServiceProviders);
					}
					else if (rateType == RatingConstants.RatingHeaderTypes.Costing)
					{
						var serviceProviderOrCarrierFilter = new ZQuery(RatingHeaderSchema.TH_OH, resolvedServiceProviders);
						serviceProviderOrCarrierFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_OH_TransportProvider, resolvedServiceProviders);
						ratingHeaderSubQuery.AddToFilter(serviceProviderOrCarrierFilter);
					}
				}
				else
				{
					queryShouldReturnsNoResults = true;
				}
			}

			rateEntryQuery.AddSubQuery(RateEntrySchema.TI_TH, ratingHeaderSubQuery, JoinCondition.And);
			rateEntryQuery.IsNoResultQuery = rateEntryQuery.IsNoResultQuery || queryShouldReturnsNoResults;

			return rateEntryQuery;
		}

		void ApplyCompanyFilter(ZDBOnlyQuery ratingHeaderQuery, ZGuid companyPK)
		{
			if (companyPK.IsValid)
			{
				var companyFilter = new ZQuery(RatingHeaderSchema.TH_GC, companyPK);
				companyFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_GC, null);
				ratingHeaderQuery.AddToFilter(companyFilter);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging message")]
		void ApplyRateTypeFilter(ZDBOnlyQuery ratingHeaderQuery, string rateType)
		{
			var validTypes = new[] { RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RatingHeaderTypes.IntercompanyTariff, RatingConstants.RatingHeaderTypes.ClientRate };
			if (validTypes.Contains(rateType))
			{
				ratingHeaderQuery.AddToFilter(new ZQuery(RatingHeaderSchema.TH_RateType, rateType));
			}
			else
			{
				ratingHeaderQuery.IsNoResultQuery = true;
				Logger.Error("Only 'Costing', 'Client Rates' 'Company Tariff' and 'Intercompany Tariff' are supported by now.");
			}
		}

		void ApplyClientFilter(ZDBOnlyQuery ratingHeaderQuery, string rateType, string clientCode)
		{
			if (!string.IsNullOrEmpty(clientCode))
			{
				if (RatingConstants.RatingHeaderTypes.ClientRate.Equals(rateType, StringComparison.InvariantCultureIgnoreCase))
				{
					var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
					orgHeaderSubQuery.AddToFilter(new ZQuery(OrgHeaderSchema.OH_Code, clientCode));
					ratingHeaderQuery.AddSubQuery(RatingHeaderSchema.TH_OH, orgHeaderSubQuery, JoinCondition.And);
				}
				else
				{
					ratingHeaderQuery.IsNoResultQuery = true;
				}
			}
		}

		void AddOriginDestinationFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery)
		{
			if (!string.IsNullOrEmpty(rateQuery.Origin?.Value) || !string.IsNullOrEmpty(rateQuery.Destination?.Value))
			{
				var filter = filtersBusinessObject.AddFilterStrip<ModuleLocationFilter>(RateEntryFilterUtility.Constants.Codes.OriginDestination);
				filter.IsActive = true;
				if (!string.IsNullOrEmpty(rateQuery.Origin?.Value) && rateQuery.Origin.IsCW1Location)
				{
					filter.Property1 = rateQuery.Origin.Value;
				}

				if (!string.IsNullOrEmpty(rateQuery.Destination?.Value) && rateQuery.Destination.IsCW1Location)
				{
					filter.Property2 = rateQuery.Destination.Value;
				}
			}
		}

		void AddViaFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery)
		{
			if (!string.IsNullOrEmpty(rateQuery.Via?.Value))
			{
				var filter = filtersBusinessObject.AddFilterStrip<ModuleNkFilter>(RateEntryFilterUtility.Constants.Codes.Via);
				filter.IsActive = true;
				if (!string.IsNullOrEmpty(rateQuery.Via?.Value) && rateQuery.Via.IsCW1Location)
				{
					filter.Property = rateQuery.Via.Value;
				}
			}
		}

		void AddLocationFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery)
		{
			if (rateQuery.Locations != null && rateQuery.Locations.Length > 0)
			{
				foreach (var location in rateQuery.Locations)
				{
					var filterDescription = "";

					switch (location.RelatedField)
					{
						case RateEntryLookups.LocationSourceOption.Code.FirstLoad:
							filterDescription = RateEntryFilterUtility.Constants.Codes.FirstLoad;
							break;
						case RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad:
							filterDescription = RateEntryFilterUtility.Constants.Codes.FirstRouteSetLoad;
							break;
						case RateEntryLookups.LocationSourceOption.Code.LastDischarge:
							filterDescription = RateEntryFilterUtility.Constants.Codes.LastDischarge;
							break;
						case RateEntryLookups.LocationSourceOption.Code.LastRouteSetDischarge:
							filterDescription = RateEntryFilterUtility.Constants.Codes.LastRouteSetDischarge;
							break;
					}

					if (!string.IsNullOrEmpty(filterDescription))
					{
						var filter = filtersBusinessObject.AddFilterStrip<ModuleNkFilter>(filterDescription);
						filter.IsActive = true;
						filter.Property = location.Value;
					}
				}
			}
		}

		void AddNamedAccountsFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, IEnumerable<ZGuid> cw1NamedAccounts)
		{
			foreach (var item in cw1NamedAccounts)
			{
				var controllingCustomerFilter = filtersBusinessObject.AddFilterStrip<ModuleGuidFilter>(RateEntryFilterUtility.Constants.Codes.ControllingCustomer);
				controllingCustomerFilter.IsActive = true;
				controllingCustomerFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				controllingCustomerFilter.OrCategory = FilterOrCategory.CadetBlue;
				controllingCustomerFilter.Property = item;

				var consigneeFilter = filtersBusinessObject.AddFilterStrip<ModuleGuidFilter>(RateEntryFilterUtility.Constants.Codes.Consignee);
				consigneeFilter.IsActive = true;
				consigneeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				consigneeFilter.OrCategory = FilterOrCategory.CadetBlue;
				consigneeFilter.Property = item;

				var consignorFilter = filtersBusinessObject.AddFilterStrip<ModuleGuidFilter>(RateEntryFilterUtility.Constants.Codes.Consignor);
				consignorFilter.IsActive = true;
				consignorFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				consignorFilter.OrCategory = FilterOrCategory.CadetBlue;
				consignorFilter.Property = item;
			}
		}

		void AddCW1RateModeFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery)
		{
			if (rateQuery.CW1RateMode != Core.Constants.RateMode.ALL)
			{
				var filter = filtersBusinessObject.AddFilterStrip<ModuleTextFilter>(RateEntryFilterUtility.Constants.Codes.TransportMode);
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = rateQuery.CW1RateMode;
			}
		}

		void AddCarrierContractsFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery, SourceEndpoint sourceEndpoint)
		{
			if (rateQuery.CarrierContracts != null && sourceEndpoint == SourceEndpoint.Costing)
			{
				foreach (var item in rateQuery.CarrierContracts.Distinct())
				{
					var filter = filtersBusinessObject.AddFilterStrip<ModuleTextFilter>(RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);
					filter.IsActive = true;
					filter.OrCategory = FilterOrCategory.Aqua;

					if (!string.IsNullOrEmpty(item))
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
						filter.Property = item;
					}
					else
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
					}
				}
			}
		}

		void AddClientContractsFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery, SourceEndpoint sourceEndpoint)
		{
			if (rateQuery.ClientContracts != null && sourceEndpoint == SourceEndpoint.ClientRates)
			{
				foreach (var item in rateQuery.ClientContracts.Distinct())
				{
					var filter = filtersBusinessObject.AddFilterStrip<ModuleTextFilter>(RateEntryFilterUtility.Constants.Codes.ClientContractNumber);
					filter.IsActive = true;
					filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
					filter.OrCategory = FilterOrCategory.Aquamarine;

					if (!string.IsNullOrEmpty(item))
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
						filter.Property = item;
					}
					else
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
					}
				}
			}
		}

		void AddEffectiveDateFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery)
		{
			if (rateQuery.EffectiveDate != default)
			{
				var filter = filtersBusinessObject.AddFilterStrip<ModuleSingleDateFilter>(RateEntryFilterUtility.Constants.Codes.EffectiveOn);
				filter.IsActive = true;
				filter.Property1 = rateQuery.EffectiveDate.LocalDateTime;
			}
		}

		void AddCommoditiesFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery)
		{
			if (rateQuery.Commodities != null)
			{
				var commodities =
					rateQuery
					.Commodities
					.Where(c => CommodityInfo.Types.CargoWise.Equals(c.Type, StringComparison.InvariantCultureIgnoreCase))
					.Select(c => c.CWCode)
					.Distinct();

				foreach (var item in commodities)
				{
					var filter = filtersBusinessObject.AddFilterStrip<ModuleNkFilter>(RateEntryFilterUtility.Constants.Codes.CommodityCode);
					filter.IsActive = true;
					filter.OrCategory = FilterOrCategory.AliceBlue;

					if (!string.IsNullOrEmpty(item))
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
						filter.Property = item;
					}
					else
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
					}
				}
			}
		}

		void AddContainerTypesFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, IEnumerable<ZGuid> containerTypePKs)
		{
			foreach (var item in containerTypePKs)
			{
				var filter = filtersBusinessObject.AddFilterStrip<ModuleGuidFilter>(RateEntryFilterUtility.Constants.Codes.ContainerType);
				filter.IsActive = true;
				filter.OrCategory = FilterOrCategory.AntiqueWhite;
				filter.Property = item;
			}
		}

		void AddCarrierServiceLevelsFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery)
		{
			if (rateQuery.CarrierServiceLevels != null)
			{
				var carrierServiceLevels =
					rateQuery
					.CarrierServiceLevels
					.Where(csl => CarrierServiceLevel.Types.CargoWise.Equals(csl.Type, StringComparison.InvariantCultureIgnoreCase))
					.Select(csl => csl.CWCode)
					.Distinct();

				foreach (var item in carrierServiceLevels)
				{
					var filter = filtersBusinessObject.AddFilterStrip<ModuleTextFilter>(RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel);
					filter.IsActive = true;
					filter.OrCategory = FilterOrCategory.AntiqueWhite;

					if (!string.IsNullOrEmpty(item))
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
						filter.Property = item;
					}
					else
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
					}
				}
			}
		}

		void AddServiceLevelsFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery)
		{
			if (rateQuery.ServiceLevels != null)
			{
				foreach (var item in rateQuery.ServiceLevels.Distinct())
				{
					var filter = filtersBusinessObject.AddFilterStrip<ModuleTextFilter>(RateEntryFilterStripForConvertBusinessObject.ServiceLevelFilterAsText);
					filter.IsActive = true;
					filter.OrCategory = FilterOrCategory.Azure;

					if (!string.IsNullOrEmpty(item))
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
						filter.Property = item;
					}
					else
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
					}
				}
			}
		}

		void AddGatewayServiceLevelsFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery)
		{
			if (rateQuery.GatewayServiceLevels != null)
			{
				foreach (var item in rateQuery.GatewayServiceLevels.Distinct())
				{
					var filter = filtersBusinessObject.AddFilterStrip<ModuleTextFilter>(RateEntryFilterStripForConvertBusinessObject.GatewayServiceLevelFilterAsText);
					filter.IsActive = true;
					filter.OrCategory = FilterOrCategory.Beige;

					if (!string.IsNullOrEmpty(item))
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
						filter.Property = item;
					}
					else
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
					}
				}
			}
		}

		void AddShipmentGatewayServiceLevelsFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery)
		{
			if (rateQuery.ShipmentGatewayServiceLevels != null)
			{
				foreach (var item in rateQuery.ShipmentGatewayServiceLevels.Distinct())
				{
					var filter = filtersBusinessObject.AddFilterStrip<ModuleTextFilter>(RateEntryFilterStripForConvertBusinessObject.ShipmentGatewayServiceLevelFilterAsText);
					filter.IsActive = true;
					filter.OrCategory = FilterOrCategory.Beige;

					if (!string.IsNullOrEmpty(item))
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
						filter.Property = item;
					}
					else
					{
						filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
					}
				}
			}
		}

		void AddPlannedLoadDischargeFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery, SourceEndpoint sourceEndpoint)
		{
			if
				(
					(rateQuery.PlannedLoad.IsValidOptionalLocation() || rateQuery.PlannedDischarge.IsValidOptionalLocation())
					&& sourceEndpoint != SourceEndpoint.Costing
				)
			{
				var filter = filtersBusinessObject.AddFilterStrip<ModuleLocationFilter>(RateEntryFilterStripForConvertBusinessObject.PlannedLoadDischarge);
				filter.IsActive = true;
				if (rateQuery.PlannedLoad.IsValidOptionalLocation() && (rateQuery.PlannedLoad.IsCW1Location))
				{
					filter.Property1 = rateQuery.PlannedLoad.Value;
					filter.IsEmptyProperty1ComparisonOperation = rateQuery.PlannedLoad.IsEmptyValue();
				}

				if (rateQuery.PlannedDischarge.IsValidOptionalLocation() && (rateQuery.PlannedDischarge.IsCW1Location))
				{
					filter.Property2 = rateQuery.PlannedDischarge.Value;
					filter.IsEmptyProperty2ComparisonOperation = rateQuery.PlannedDischarge.IsEmptyValue();
				}
			}
		}

		void AddRateOriginDestinationFilterStrip(RateEntryFilterStripForConvertBusinessObject filtersBusinessObject, RateQuery rateQuery, SourceEndpoint sourceEndpoint)
		{
			if
				(
					(rateQuery.RateOrigin.IsValidOptionalLocation() || rateQuery.RateDestination.IsValidOptionalLocation())
					&& sourceEndpoint != SourceEndpoint.Costing
				)
			{
				var filter = filtersBusinessObject.AddFilterStrip<ModuleLocationFilter>(RateEntryFilterStripForConvertBusinessObject.RateOriginDestination);
				filter.IsActive = true;
				if (rateQuery.RateOrigin.IsValidOptionalLocation() && (rateQuery.RateOrigin.IsCW1Location))
				{
					filter.Property1 = rateQuery.RateOrigin.Value;
					filter.IsEmptyProperty1ComparisonOperation = rateQuery.RateOrigin.IsEmptyValue();
				}

				if (rateQuery.RateDestination.IsValidOptionalLocation() && (rateQuery.RateDestination.IsCW1Location))
				{
					filter.Property2 = rateQuery.RateDestination.Value;
					filter.IsEmptyProperty2ComparisonOperation = rateQuery.RateDestination.IsEmptyValue();
				}
			}
		}

		#region Helpers

		IEnumerable<ZGuid> GetResolvedQueryContainerTypes(RateQuery rateQuery)
		{
			var result = new HashSet<ZGuid>();

			var cw1Codes = rateQuery.ContainerTypes.Where(ct => !string.IsNullOrEmpty(ct?.CWCode)).Select(ct => ct.CWCode).ToArray();
			if (cw1Codes.Any())
			{
				var zQuery = new ZDBOnlyQuery(typeof(RefContainer));
				zQuery.AddToFilter(RefContainerSchema.RC_Code, cw1Codes);
				var containers = Factory.Load<RefContainer>(zQuery);

				foreach (var item in cw1Codes.Except(containers.Select(c => c.RC_Code.ToString())))
				{
					Logger.Warning($"Provided Container Type with Code '{item}' does not exists in CargoWise.");
				}

				result.UnionWith(containers.Select(c => c.PK));
			}

			var isoTypes = rateQuery.ContainerTypes.Where(ct => !string.IsNullOrEmpty(ct?.ISOType)).Select(ct => ct.ISOType).ToArray();
			if (isoTypes.Any())
			{
				var zQuery = new ZDBOnlyQuery(typeof(RefContainer));
				zQuery.AddToFilter(RefContainerSchema.RC_ISOType, isoTypes);

				var containers = Factory.Load<RefContainer>(zQuery);

				foreach (var item in isoTypes.Except(containers.Select(c => c.RC_ISOType.ToString())))
				{
					Logger.Warning($"Provided Container Type with ISO Type '{item}' does not exists in CargoWise.");
				}

				result.UnionWith(containers.Select(c => c.PK));
			}

			return result.ToArray();
		}

		IEnumerable<ZGuid> GetResolvedQueryServiceProviders(RateQuery rateQuery)
		{
			var result = new HashSet<ZGuid>();
			var sql = string.Empty;

			if (rateQuery.ServiceProviders?.Any(p => !string.IsNullOrEmpty(p.CWCode)) ?? false)
			{
				var cwCodes = rateQuery.ServiceProviders.Where(p => !string.IsNullOrEmpty(p.CWCode)).Select(sp => sp.CWCode).ToArray();

				var cwCodesCollection = new DynamicBusinessObjectCollection(Factory);
				sql = $@"
SELECT {OrgHeaderSchema.Constants.PK}, {OrgHeaderSchema.Constants.OH_Code}
FROM {OrgHeaderSchema.Constants.SqlSchemaName}.{OrgHeaderSchema.Constants.TableName}
WHERE
{OrgHeaderSchema.Constants.OH_Code} IN ({string.Join(", ", cwCodes.Select(c => c.ToQuotedString()))})";

				cwCodesCollection.Load(sql);
				result.UnionWith(cwCodesCollection.Select(i => (ZGuid)i[OrgHeaderSchema.Constants.PK]));

				foreach (var item in cwCodes.Except(cwCodesCollection.Select(c => ((ZString)c[OrgHeaderSchema.Constants.OH_Code]).ToString())))
				{
					Logger.Warning($"Provided Service Provider with CWCode '{item}' does not exists in CargoWise.");
				}
			}

			if (rateQuery.ServiceProviders?.Any(p => !string.IsNullOrEmpty(p.IATACode)) ?? false)
			{
				var iataCodes = rateQuery.ServiceProviders.Where(p => !string.IsNullOrEmpty(p.IATACode)).Select(p => p.IATACode).ToArray();

				var iataCodesCollection = new DynamicBusinessObjectCollection(Factory);

				sql = $@"
SELECT
	OMS.{OrgMiscServSchema.Constants.OM_OH},
	A.{RefAirlineSchema.Constants.RM_TwoCharacterCode}
FROM {OrgMiscServSchema.Constants.SqlSchemaName}.{OrgMiscServSchema.Constants.TableName} OMS
INNER JOIN {RefAirlineSchema.Constants.SqlSchemaName}.{RefAirlineSchema.Constants.TableName} A ON
	OMS.{OrgMiscServSchema.Constants.OM_RM_Airline} = A.{RefAirlineSchema.Constants.PK}
WHERE
	A.{RefAirlineSchema.Constants.RM_TwoCharacterCode} IN ({string.Join(",", iataCodes.Select(i => i.ToQuotedString()))})";

				iataCodesCollection.Load(sql);
				result.UnionWith(iataCodesCollection.Select(i => (ZGuid)i[OrgMiscServSchema.Constants.OM_OH]));

				foreach (var item in iataCodes.Except(iataCodesCollection.Select(i => ((ZString)i[RefAirlineSchema.Constants.RM_TwoCharacterCode]).ToString())))
				{
					Logger.Warning($"Provided Service Provider with IATA Code '{item}' does not exists in CargoWise.");
				}
			}

			if (rateQuery.ServiceProviders?.Any(p => !string.IsNullOrEmpty(p.SCAC)) ?? false)
			{
				var scacCodes = rateQuery.ServiceProviders.Where(p => !string.IsNullOrEmpty(p.SCAC)).Select(p => p.SCAC).ToArray();
				var scacCodesCollection = new DynamicBusinessObjectCollection(Factory);

				sql = $@"
SELECT
	O.{OrgHeaderSchema.Constants.PK},
	S.{RefShippingLineSchema.Constants.RSL_StandardCarrierAlphaCode}
FROM {OrgHeaderSchema.Constants.SqlSchemaName}.{OrgHeaderSchema.Constants.TableName} O
INNER JOIN {RefShippingLineSchema.Constants.SqlSchemaName}.{RefShippingLineSchema.Constants.TableName} S ON
	O.{OrgHeaderSchema.Constants.OH_RSL_ShippingLine} = S.{RefShippingLineSchema.Constants.PK}
WHERE
	S.{RefShippingLineSchema.Constants.RSL_StandardCarrierAlphaCode} IN ({string.Join(",", scacCodes.Select(s => s.ToQuotedString()))})";

				scacCodesCollection.Load(sql);
				result.UnionWith(scacCodesCollection.Select(i => (ZGuid)i[OrgHeaderSchema.Constants.PK]));

				foreach (var item in scacCodes.Except(scacCodesCollection.Select(s => ((ZString)s[RefShippingLineSchema.Constants.RSL_StandardCarrierAlphaCode]).ToString())))
				{
					Logger.Warning($"Provided Service Provider with SCAC Code '{item}' does not exists in CargoWise.");
				}
			}

			if (rateQuery.ServiceProviders?.Any(p => !string.IsNullOrEmpty(p.C1CCode)) ?? false)
			{
				var c1cCodes = rateQuery.ServiceProviders.Where(p => !string.IsNullOrEmpty(p.C1CCode)).Select(p => p.C1CCode).ToArray();
				var c1cCodesCollection = new DynamicBusinessObjectCollection(Factory);

				sql = $@"
SELECT
	O.{OrgHeaderSchema.Constants.PK},
	S.{RefShippingLineSchema.Constants.RSL_CargoWiseOneCode}
FROM {OrgHeaderSchema.Constants.SqlSchemaName}.{OrgHeaderSchema.Constants.TableName} O
INNER JOIN {RefShippingLineSchema.Constants.SqlSchemaName}.{RefShippingLineSchema.Constants.TableName} S ON
	O.{OrgHeaderSchema.Constants.OH_RSL_ShippingLine} = S.{RefShippingLineSchema.Constants.PK}
WHERE
	S.{RefShippingLineSchema.Constants.RSL_CargoWiseOneCode} IN ({string.Join(",", c1cCodes.Select(c => c.ToQuotedString()))})";

				c1cCodesCollection.Load(sql);
				result.UnionWith(c1cCodesCollection.Select(i => (ZGuid)i[OrgHeaderSchema.Constants.PK]));

				foreach (var item in c1cCodes.Except(c1cCodesCollection.Select(s => ((ZString)s[RefShippingLineSchema.Constants.RSL_CargoWiseOneCode]).ToString())))
				{
					Logger.Warning($"Provided Service Provider with C1C Code '{item}' does not exists in CargoWise.");
				}
			}

			return result.ToArray();
		}

		IEnumerable<ZGuid> GetResolvedCW1NamedAccounts(RateQuery rateQuery)
		{
			var cw1OrgCodes = rateQuery.NamedAccounts?.Where(a => NamedAccount.Types.CargoWise.Equals(a?.Type) && !string.IsNullOrEmpty(a?.Value))?.Select(a => a.Value) ?? Enumerable.Empty<string>();

			if (cw1OrgCodes.Any())
			{
				var cwCodesCollection = new DynamicBusinessObjectCollection(Factory);
				var sql = $@"
SELECT {OrgHeaderSchema.Constants.PK}, {OrgHeaderSchema.Constants.OH_Code}
FROM {OrgHeaderSchema.Constants.SqlSchemaName}.{OrgHeaderSchema.Constants.TableName}
WHERE
{OrgHeaderSchema.Constants.OH_Code} IN ({string.Join(",", cw1OrgCodes.Select(c => c.ToQuotedString()))})";

				cwCodesCollection.Load(sql);

				foreach (var item in cw1OrgCodes.Except(cwCodesCollection.Select(c => ((ZString)c[OrgHeaderSchema.Constants.OH_Code]).ToString())))
				{
					Logger.Warning($"Provided NamedAccount organization with Code '{item}' does not exists in CargoWise.");
				}

				return cwCodesCollection.Select(i => (ZGuid)i[OrgHeaderSchema.Constants.PK]).ToArray();
			}

			return Enumerable.Empty<ZGuid>();
		}

		#endregion

		#endregion
	}

	static class QueryBuildHelper
	{
		internal static string ToQuotedString(this string str)
		{
			return $"'{str}'";
		}
	}
}
