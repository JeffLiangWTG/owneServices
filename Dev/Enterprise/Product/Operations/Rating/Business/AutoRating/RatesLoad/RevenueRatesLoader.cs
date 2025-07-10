using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Tools;

namespace Enterprise.Rating.Business;

[Flags]
public enum RevenueLoadOptions
{
	Default = ClientRates | Tariffs | Quotes, // 7, UnacceptedQuotes and IntercopanyTariffs are special cases, they are not normally loaded by default, and therefore not included
	ClientRates = 1 << 0,		// 1
	Tariffs = 1 << 1,			// 2
	Quotes = 1 << 2,			// 4
	UnacceptedQuotes = 1 << 3,  // 8
	IntercopanyTariffs = 1 << 4 // 16
}

/// <summary>
///		Loads revenue rates (costing, tariffs, client rates, quotation, etc.) from the CW1 database for the <see cref="RatingCriteria"/> provided.
///		Basically, it is just a wrapper around rates in the database so that all revenue related rates loading logic is in one place.
/// </summary>
/// <remarks>
///		It doesn't have any business logic regarding filtering, matching, overriding rates. This logic is quite complex, has many rules and cannot
///		be done on the DB level. It is done by the autorating engine after loading in <see cref="RateEntryFilter"/>.
///
///		In <see cref="RevenueRatesLoader"/>, we apply only basic filters that can be implemented by sql queries, i.e. preliminary and rough filtering
///		to filter out rates that are not relevant to the criteria at all.
/// </remarks>
public class RevenueRatesLoader(BusinessObjectFactory factory, ILogger logger) : RatesLoader(factory, logger)
{
	/// <summary>
	///		Loads revenue rates matching <paramref name="criteria"/> of type <paramref name="ratesToLoad"/>.
	/// </summary>
	public IEnumerable<IRateEntry> Load(RatingCriteria criteria, RevenueLoadOptions ratesToLoad = RevenueLoadOptions.Default)
	{
		var headers = LoadHeaders(criteria, ratesToLoad).ToList();
		var entries = GetRateEntries(headers, criteria, true, isCosting: false);

		return entries;
	}

	/// <summary>
	///		Loads revenue rates matching <paramref name="criteria"/> from <paramref name="headersToLoad"/> .
	/// </summary>
	public IEnumerable<IRateEntry> Load(RatingCriteria criteria, IEnumerable<IRatingHeader> headersToLoad)
	{
		var entries = GetRateEntries(headersToLoad, criteria, true, isCosting: false);
		return entries;
	}

	public IEnumerable<IRatingHeader> LoadHeaders(RatingCriteria criteria, RevenueLoadOptions ratesToLoad = RevenueLoadOptions.Default)
	{
		var ratingHeaders = new List<IRatingHeader>();

		if (ratesToLoad.HasFlag(RevenueLoadOptions.ClientRates))
		{
			var clientRates = GetClientRates(criteria);
			ratingHeaders.AddRange(clientRates);
		}

		if (ratesToLoad.HasFlag(RevenueLoadOptions.Tariffs))
		{
			var tariffs = GetCompanyTariffs(criteria);
			ratingHeaders.AddRange(tariffs);
		}

		if (ratesToLoad.HasFlag(RevenueLoadOptions.IntercopanyTariffs))
		{
			var tariffs = GetIntercompanyTariffs(criteria);
			ratingHeaders.AddRange(tariffs);
		}

		if (ratesToLoad.HasFlag(RevenueLoadOptions.Quotes))
		{
			var quote = GetOneOffQuote(criteria);
			if (quote != null)
			{
				ratingHeaders.Add(quote);
			}
		}

		if (ratesToLoad.HasFlag(RevenueLoadOptions.UnacceptedQuotes))
		{
			var quotes = GetUnacceptedQuotes(criteria);
			ratingHeaders.AddRange(quotes);
		}

		return ratingHeaders;
	}

	#region Client Rates

	IEnumerable<IRatingHeader> GetClientRates(RatingCriteria criteria)
	{
		var filter = new ZQuery(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.ClientRate);

		var companyFilter = new ZQuery(RatingHeaderSchema.TH_GC, criteria.Company?.PK);
		companyFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_GC, null);

		filter.AddToFilter(companyFilter);

		var orgPKs = new HashSet<ZGuid>();
		var queryNull = false;

		var clientRateOrgs = criteria.GetDebtors().SelectMany(x => x.OrgHeader.RelatedManagementParentsAndSelf());
		foreach (var org in clientRateOrgs)
		{
			if (org != null)
			{
				orgPKs.Add(org.PK);
			}
			else
			{
				queryNull = true;
			}
		}

		var orgFilter = new ZQuery(RatingHeaderSchema.TH_OH, orgPKs);
		if (queryNull)
		{
			orgFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_OH, null);
		}

		filter.AddToFilter(orgFilter);

		var keyMaker = new StringBuilder();
		keyMaker.AppendRange(orgPKs.OrderBy(c => c).Select(c => c.ToStringKey()).Append(criteria.Company?.PK.ToStringKey() ?? string.Empty));
		var cacheKey = keyMaker.ToString();

		var clientRates = factory.GetCachedValue(cacheKey, () => factory.Load<RatingHeader>(filter), CacheStalenessPolicy.StaleWhenDataTableChanges(RatingHeaderSchema.Constants.TableName, factory));
		return clientRates;
	}

	#endregion

	#region Company Tariffs

	IEnumerable<IRatingHeader> GetCompanyTariffs(RatingCriteria criteria)
	{
		const int feeChargeCompanyTariffLevel = 1;
		var rates = new List<IRatingHeader>();
		var companyTariffLevelOverride = criteria.TariffLevel;
		var companyTariffLevels = new List<int> { companyTariffLevelOverride };

		if (companyTariffLevelOverride <= 0) //When CompanyTariffLevelOverride is higher than zero, we will only load Company Tariff with level of CompanyTariffLevelOverride
		{
			companyTariffLevels.Add(feeChargeCompanyTariffLevel);
			var orgsInCharge = criteria.GetDebtors();

			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				var rateMode = RatingHelper.GetRateMode(criteria, category);

				foreach (var chargedOrg in orgsInCharge)
				{
					var tariffLevels = RatingCache.GetMultipleCompanyTariffLevels(chargedOrg.OrgHeader, criteria.Company, category, rateMode, criteria.Direction);
					foreach (var tariffLevel in tariffLevels)
					{
						companyTariffLevels.Add(tariffLevel);
					}
				}
			}
		}

		foreach (var companyTariffLevel in companyTariffLevels.Distinct().Where(level => level > 0))
		{
			var tariffs = GetCompanyTariff(companyTariffLevel, criteria.Company);
			if (tariffs.Any())
			{
				rates.AddRange(tariffs);
			}
		}

		return rates;
	}

	public IEnumerable<IRatingHeader> GetCompanyTariff(int companyTariffLevel, GlbCompany company)
	{
		var filter = new ZQuery(RatingHeaderSchema.TH_GlobalRateLevel, (byte)companyTariffLevel);

		var companyFilter = new ZQuery(RatingHeaderSchema.TH_GC, company.PK);
		companyFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_GC, null);

		filter.AddToFilter(companyFilter);
		filter.AddToFilter(RatingHeaderSchema.TH_RateType,  RatingConstants.RatingHeaderTypes.Tariff);

		var tariffs = GetCompanyTariffFactory(companyTariffLevel).Load<RatingHeader>(filter);
		return tariffs;
	}

	internal BusinessObjectFactory GetCompanyTariffFactory(int level)
	{
		if (companyTariffFactories == null)
		{
			companyTariffFactories = new Dictionary<int, BusinessObjectFactory>();
			companyTariffFactories.Add(level, factory);

			return factory;
		}
		else
		{
			BusinessObjectFactory result;
			if (!companyTariffFactories.TryGetValue(level, out result))
			{
				result = new BusinessObjectFactory();
				companyTariffFactories.Add(level, result);
			}

			return result;
		}
	}

	#endregion

	#region Intercompany Tariffs

	List<IRatingHeader> GetIntercompanyTariffs(RatingCriteria criteria)
	{
		var headers = new List<IRatingHeader>();

		var agentPKs = criteria.GatewayConfiguration.OrganizationsForAutoratingRevenueFromIntercompanyTariff;
		if (agentPKs.Any())
		{
			var filter = new ZQuery(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.IntercompanyTariff);
			filter.AddToFilter(RatingHeaderSchema.TH_OH, agentPKs);

			headers.AddRange(factory.Load<RatingHeader>(filter));
		}

		return headers;
	}

	#endregion

	#region One-Off Quotes

	public IRatingHeader GetOneOffQuote(RatingCriteria criteria)
	{
		if (!criteria.QuoteNumber.IsEmpty)
		{
			return GetSpecificOneOffQuote(criteria.QuoteNumber);
		}

		if (criteria.ConsumerType != JobInvoicingConsumerTypes.OneOffQuotation)
		{
			var quotes = GetPossibleOneOffQuoteMatches(criteria);
			if (quotes.Any() && !selectQuoteCalled)
			{
				selectQuoteCalled = true;

				var chosenQuote = _Rating.Interactor.SelectQuote(quotes);
				if (chosenQuote != null)
				{
					if (OneOffQuoteSelected != null)
					{
						OneOffQuoteSelected(this, new QuoteEventArgs(chosenQuote));
					}

					return chosenQuote;
				}
			}
		}

		return null;
	}

	IEnumerable<IRatingHeader> GetUnacceptedQuotes(RatingCriteria criteria)
	{
		var rates = new List<IRatingHeader>();

		var isForwardingOrShipping = criteria.RateTypeToUse == RateType.Forwarding || criteria.RateTypeToUse == RateType.Shipping;
		if (isForwardingOrShipping)
		{
			var quotes = factory.Load<Quote>(UnacceptedQuotesFilter(criteria));
			rates.AddRange(quotes);
		}

		return rates;
	}

	ZDBOnlyQuery UnacceptedQuotesFilter(RatingCriteria criteria)
	{
		var clientPK = criteria.LocalClient != null ? criteria.LocalClient.PK : Guid.Empty;
		var addressCode = DocAddressTypes.GetCode(factory, DocAddressType.QuotationClientAddress);

		var docAddressesFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
		var orgAddressesFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

		orgAddressesFilter.AddToFilter(OrgAddressSchema.OA_OH, clientPK);
		docAddressesFilter.AddSubQuery(orgAddressesFilter, JoinCondition.And);
		docAddressesFilter.AddToFilter(JobDocAddress.GetFilter(addressCode, RatingHeaderSchema.Constants.Prefix, 0), JoinCondition.And);

		var filter = new ZDBOnlyQuery(typeof(Quote));
		filter.AddSubQuery(RatingHeaderSchema.PK, docAddressesFilter, JoinCondition.And);

		filter.AddToFilter(RatingHeaderSchema.TH_GC, criteria.Company.PK);
		filter.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Quote);

		var endDateFilter = new ZQuery(RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
		endDateFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.Equal, null);
		filter.AddToFilter(endDateFilter, JoinCondition.And);

		filter.AddToFilter(RatingHeaderSchema.TH_Accepted, SQLComparisonOperator.Equal, null);
		filter.AddToFilter(RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
		filter.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, SQLComparisonOperator.Equal, ZBool.False);

		return filter;
	}

	bool selectQuoteCalled;

	public event EventHandler<QuoteEventArgs> OneOffQuoteSelected;

	public class QuoteEventArgs : EventArgs
	{
		public Quote Quote { get; }

		public QuoteEventArgs(Quote quote)
		{
			Quote = quote;
		}
	}

	IRatingHeader GetSpecificOneOffQuote(ZString quoteNumber)
	{
		var filter = new ZQuery();
		filter.AddToFilter(RatingHeaderSchema.TH_QuoteNumber, SQLComparisonOperator.Equal, quoteNumber);
		filter.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, SQLComparisonOperator.Equal, ZBool.True);
		filter.AddToFilter(RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);

		var endDateFilter = new ZQuery(RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
		endDateFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.Equal, null);
		filter.AddToFilter(endDateFilter, JoinCondition.And);

		var rate = factory.LoadTop1<Quote>(filter);
		return rate;
	}

	QuoteCollection GetPossibleOneOffQuoteMatches(RatingCriteria criteria)
	{
		if (criteria.LocalClient == null)
		{
			return new QuoteCollection(factory, criteria.Company);
		}

		var cacheKeyBuilder = new StringBuilder();
		cacheKeyBuilder.Append(criteria.LocalClient.PK.ToStringKey());
		cacheKeyBuilder.Append(criteria.FreightMode.ToString());
		cacheKeyBuilder.Append(criteria.OriginCode);
		cacheKeyBuilder.Append(criteria.DestinationCode);

		var cacheKey = cacheKeyBuilder.ToString();

		return factory.GetCachedValue(cacheKey, () =>
		{
			var results = new QuoteCollection(factory, criteria.Company);
			// DocAddress
			var addressCode = DocAddressTypes.GetCode(factory, DocAddressType.QuotationClientAddress);

			var docAddressesFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressesFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddressesFilter.AddToFilter(OrgAddressSchema.OA_OH, criteria.LocalClient.PK);
			docAddressesFilter.AddSubQuery(orgAddressesFilter, JoinCondition.And);
			docAddressesFilter.AddToFilter(JobDocAddress.GetFilter(addressCode, RatingHeaderSchema.Constants.Prefix, 0), JoinCondition.And);

			var filter = new ZDBOnlyQuery(typeof(RatingHeader));
			filter.AddSubQuery(RatingHeaderSchema.PK, docAddressesFilter, JoinCondition.Or);
			filter.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, SQLComparisonOperator.Equal, ZBool.True);
			filter.AddToFilter(RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);

			var endDateFilter = new ZQuery(RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			endDateFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.Equal, null);
			filter.AddToFilter(endDateFilter, JoinCondition.And);

			filter.AddToFilter(RatingHeaderSchema.TH_IsOneOffQuoteConsumed, SQLComparisonOperator.Equal, ZBool.False);

			var oneOffQuoteQuery = new ZDBOnlySubQuery(typeof(RateOneOffShipment), RateOneOffShipmentSchema.TT_TH);
			if (DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.Value)
			{
				oneOffQuoteQuery.AddToFilter(RateOneOffShipmentSchema.TT_QuoteApprovedByManager, ZBool.True);
			}

			oneOffQuoteQuery.AddToFilter(RateOneOffShipmentSchema.TT_TransportMode, GetTransportModesFromFreightMode(criteria.FreightMode));
			oneOffQuoteQuery.AddToFilter(RateOneOffShipmentSchema.TT_ContainerMode, GetContainerModesFromFreightMode(criteria.FreightMode));
			oneOffQuoteQuery.AddToFilter(RateOneOffShipmentSchema.TT_RL_NKReceivalLocation, criteria.OriginCode);
			oneOffQuoteQuery.AddToFilter(RateOneOffShipmentSchema.TT_RL_NKDeliveryLocation, criteria.DestinationCode);
			filter.AddSubQuery(oneOffQuoteQuery, JoinCondition.And);

			results.Load(filter);

			return results;
		}, CacheStalenessPolicy.StaleWhenDataTableChanges(RateOneOffShipmentSchema.Constants.TableName, factory));
	}

	ZString[] GetTransportModesFromFreightMode(FreightMode freightMode)
	{
		var result = new List<ZString>();
		if ((freightMode & FreightMode.AIR) != 0)
		{
			result.Add(Core.Constants.TransportModes.Air);
			result.Add(Core.Constants.TransportModes.AirSea);
		}
		else if ((freightMode & FreightMode.SEA) != 0)
		{
			result.Add(Core.Constants.TransportModes.Sea);
			result.Add(Core.Constants.TransportModes.SeaAir);
		}
		else if ((freightMode & FreightMode.ROA) != 0)
		{
			result.Add(Core.Constants.TransportModes.Road);
		}
		else if ((freightMode & FreightMode.RAI) != 0)
		{
			result.Add(Core.Constants.TransportModes.Rail);
		}
		else if ((freightMode & FreightMode.COU) != 0)
		{
			result.Add(Core.Constants.TransportModes.Courier);
		}

		return result.ToArray();
	}

	ZString[] GetContainerModesFromFreightMode(FreightMode freightMode)
	{
		var result = new List<ZString>();

		if ((freightMode & FreightMode.BCN) != 0)
		{
			//Mode could be FCL|BCN / LCL|BCN,...
			result.Add(Core.Constants.ContainerModes.BuyersConsol);
		}

		if ((freightMode & FreightMode.SCN) != 0)
		{
			result.Add(Core.Constants.ContainerModes.ShippersConsol);
		}

		if ((freightMode & FreightMode.AIR) != 0)
		{
			if ((freightMode & FreightMode.Containerised) != 0)
			{
				result.Add(Core.Constants.ContainerModes.ULD); //AIR|ULD, FAS|ULD
			}
			else if ((freightMode & FreightMode.NonContainerised) != 0)
			{
				result.Add(Core.Constants.ContainerModes.Loose); // AIR|LSE, FAS|LSE
				result.Add(Core.Constants.ContainerModes.LCL); // FAS|LCL
			}
		}
		else if ((freightMode & FreightMode.SEA) != 0)
		{
			result.Add(Core.Constants.RateMode.SEA);// SEA|SEA
			if ((freightMode & FreightMode.Containerised) != 0)
			{
				result.Add(Core.Constants.ContainerModes.FCL); //SEA|FCL
				result.Add(Core.Constants.ContainerModes.ULD); //FSA|ULD
			}
			else if ((freightMode & FreightMode.NonContainerised) != 0)
			{
				result.Add(Core.Constants.ContainerModes.Loose); // FSA|LSE
				result.Add(Core.Constants.ContainerModes.LCL); // FSA|LCL, SEA|LCL
				result.Add(Core.Constants.ContainerModes.Bulk); // SEA|BLK
				result.Add(Core.Constants.ContainerModes.Liquid); // SEA|LQD
				result.Add(Core.Constants.ContainerModes.BreakBulk); // SEA|BBK
			}
		}
		else if ((freightMode & FreightMode.ROA) != 0)
		{
			result.Add(Core.Constants.RateMode.ROA);// ROA|ROA
			result.Add(Core.Constants.RateMode.LRO);// ROA|LRO
			if ((freightMode & FreightMode.Containerised) != 0)
			{
				result.Add(Core.Constants.ContainerModes.FCL); //ROA|FCL
			}
			else if ((freightMode & FreightMode.NonContainerised) != 0)
			{
				result.Add(Core.Constants.ContainerModes.LTL); // ROA|LTL
				result.Add(Core.Constants.ContainerModes.LCL); // ROA|LCL
				if ((freightMode & FreightMode.FullLoad) != 0)
				{
					result.Add(Core.Constants.ContainerModes.FTL); // ROA|LCL
				}
			}
		}
		else if ((freightMode & FreightMode.RAI) != 0)
		{
			result.Add(Core.Constants.RateMode.RAI);// RAI|RAI
			result.Add(Core.Constants.RateMode.FWL);// RAI|FWL
			if ((freightMode & FreightMode.Containerised) != 0)
			{
				result.Add(Core.Constants.ContainerModes.FCL); //RAI|FCL
			}
			else if ((freightMode & FreightMode.NonContainerised) != 0)
			{
				result.Add(Core.Constants.ContainerModes.LCL); // RAI|LCL
				result.Add(Core.Constants.ContainerModes.Bulk); // RAI|BLK
				result.Add(Core.Constants.ContainerModes.Liquid); // RAI|LQD
			}
		}
		else if ((freightMode & FreightMode.COU) != 0)
		{
			result.Add(Core.Constants.RateMode.COU);
			if (freightMode != FreightMode.COU)
			{
				result.Add(freightMode.ToString());
			}
		}

		return result.ToArray();
	}

	#endregion

	#region Spot Rates

	public IEnumerable<IRateEntry> LoadSpotRates(RatingCriteria criteria)
	{
		return LoadSpotRates(criteria, false);
	}

	#endregion

	Dictionary<int, BusinessObjectFactory> companyTariffFactories;
}