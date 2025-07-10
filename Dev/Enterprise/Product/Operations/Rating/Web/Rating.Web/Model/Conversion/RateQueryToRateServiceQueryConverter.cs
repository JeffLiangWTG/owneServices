using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Web.Model.Conversion
{
	/// <summary>
	/// This is a utility class to convert RateQuery to the RatesQuery that is used to call Rates Services.
	/// </summary>
	public class RateQueryToRateServiceQueryConverter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="logger"></param>
		public RateQueryToRateServiceQueryConverter(BusinessObjectFactory factory, ILogger logger)
		{
			this.Factory = factory;
			this.Logger = Argument.NotNull(logger, nameof(logger));
		}

		BusinessObjectFactory Factory { get; }
		ILogger Logger { get; }

		/// <summary>
		/// Convert
		/// </summary>
		/// <param name="query"></param>
		/// <param name="criteria"></param>
		/// <returns></returns>
		public RatesQuery Convert(RateQuery query, out RatingCriteria criteria)
		{
			var rateQueryBusinessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.Costing);
			criteria = new RatingCriteria(rateQueryBusinessObject.RatingAdapter, Factory);

			if (AreQueryBuildingInputsValid(query, rateQueryBusinessObject, criteria))
			{
				return BuildRatesQuery(rateQueryBusinessObject, criteria);
			}
			else
			{
				return null;
			}
		}

		RatesQuery BuildRatesQuery(RateQueryBusinessObject rateQueryBusinessObject, RatingCriteria criteria)
		{
			var queryBuilder = new WiseRatesQueryBuilder(Logger);
			var (ratesQuery, error) = queryBuilder.Build(criteria);

			ReassignCarriers(ratesQuery, rateQueryBusinessObject.RateQuery);

			if (!string.IsNullOrWhiteSpace(error))
			{
				var errors = error.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
				var someErrorLogged = false;

				foreach (var item in errors)
				{
					if (item.Equals(WiseRatesQueryBuilder.LogMessages.ServiceProviderError) && ratesQuery.Carrier.Any())
					{
						continue;
					}

					Logger.Log(LogType.Error, item);
					someErrorLogged = true;
				}

				if (someErrorLogged)
				{
					return null;
				}
			}

			ratesQuery.ServiceLevel = rateQueryBusinessObject.UniversalCarrierServiceLevels;
			ratesQuery.Commodities = rateQueryBusinessObject.UniversalCommodityGroups;

			if (!string.IsNullOrEmpty(rateQueryBusinessObject.RateQuery.CarrierPayTerm))
			{
				ratesQuery.PaymentTerm = new string[] { rateQueryBusinessObject.RateQuery.CarrierPayTerm };
			}

			SetCSAndCGFiltersToRatesQuery(ratesQuery, rateQueryBusinessObject);

			ratesQuery.NamedAccount = rateQueryBusinessObject.NamedAccounts.Select(na => new RatesQueryNamedAccount { Name = na }).ToArray();

			return ratesQuery;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging message")]
		bool AreQueryBuildingInputsValid(RateQuery rateQuery, RateQueryBusinessObject relatedBusinessObject, RatingCriteria relatedCriteria)
		{
			var result = true;

			if ((rateQuery.ContainerTypes?.Any() ?? false) && !relatedBusinessObject.ContainerTypes.Any())
			{
				Logger?.Log(LogType.Error, "Cannot search for rates via Rate Service. Provided Container Types does not exist in CW or cannot be recognized.");
				result = false;
			}

			if ((rateQuery.Commodities?.Where(c => !string.IsNullOrEmpty(c?.CWCode))?.Any() ?? false) && !relatedBusinessObject.Commodities.Any())
			{
				Logger?.Log(LogType.Error, "Cannot search for rates via Rate Service. Provided CW Commodities does not exist in CW or cannot be recognized.");
				result = false;
			}

			return result;
		}

		void SetCSAndCGFiltersToRatesQuery(RatesQuery ratesQuery, RateQueryBusinessObject rateQueryBusinessObject)
		{
			ratesQuery.CargoSphereFilters = new CargoSphereFilters()
			{
				RateTypes = rateQueryBusinessObject.RateQuery.CSFilter?.RateTypes,
				RateTypes2 = rateQueryBusinessObject.RateQuery.CSFilter?.RateTypes2,
				ServiceStringIDs = rateQueryBusinessObject.RateQuery.CSFilter?.ServiceStrings,
			};

			ratesQuery.CargoGuideFilters = new CargoGuideFilters()
			{
				RateClasses = rateQueryBusinessObject.RateQuery.CGFilter?.RateClasses,
				ProductNames = rateQueryBusinessObject.RateQuery.CGFilter?.Products,
				References = rateQueryBusinessObject.RateQuery.CGFilter?.References,
				Vias = rateQueryBusinessObject.RateQuery.CGFilter?.Vias,
			};
		}

		void ReassignCarriers(RatesQuery ratesQuery, RateQuery ratesAPIsRateQuery)
		{
			var carriers = new List<RatesQueryCarrier>(ratesQuery.Carrier);
			foreach (var item in (ratesAPIsRateQuery.ServiceProviders ?? Enumerable.Empty<Organisation>()))
			{
				if (string.IsNullOrEmpty(item?.C1CCode) && string.IsNullOrEmpty(item?.IATACode) && string.IsNullOrEmpty(item?.SCAC))
				{
					continue;
				}

				var existingCarriers =
					ratesQuery.Carrier.Where(c =>
						(!string.IsNullOrEmpty(item?.C1CCode) && item.C1CCode.Equals(c.C1Code))
						|| (!string.IsNullOrEmpty(item?.IATACode) && item.IATACode.Equals(c.IATACode))
						|| (!string.IsNullOrEmpty(item?.SCAC) && item.SCAC.Equals(c.SCACCode)));

				if (!existingCarriers.Any())
				{
					var carrier = new RatesQueryCarrier()
					{
						SCACCode = item?.SCAC,
						IATACode = item?.IATACode,
						C1Code = item?.C1CCode,
						Source = (NoResString)"Rates API --> Rate Query"
					};

					carriers.Add(carrier);
				}
			}

			ratesQuery.Carrier = carriers.ToArray();
		}
	}
}
