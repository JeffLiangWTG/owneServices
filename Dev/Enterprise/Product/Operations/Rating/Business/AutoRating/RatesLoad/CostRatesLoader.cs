using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business;

public class CostRatesLoader(BusinessObjectFactory factory, ILogger logger) : RatesLoader(factory, logger)
{
	/// <summary>
	///		Loads cost rate entries from the CW1 database for the <paramref name="criteria"/> provided.
	/// </summary>
	/// <remarks>
	///		It doesn't have any business logic regarding filtering, matching, overriding rates. This logic is quite complex, has many rules and cannot
	///		be done on the DB level. It is done by the autorating engine after loading in <see cref="RateEntryFilter"/>.
	///
	///		In <see cref="RatesLoader"/>, we apply only basic filters that can be implemented by sql queries, i.e. preliminary and rough filtering
	///		to filter out rates that are not relevant to the criteria at all.
	/// </remarks>
	public IEnumerable<IRateEntry> Load(RatingCriteria criteria, bool excludeCarrierFilter = false, ZQuery additionalOrganisationsFilter = null)
	{
		var headers = GetHeaders(criteria, null, additionalOrganisationsFilter);
		var entries = Load(criteria, headers, excludeCarrierFilter);

		return entries;
	}

	public IEnumerable<IRateEntry> Load(RatingCriteria criteria, IEnumerable<ZGuid> carriers)
	{
		var headers = GetHeaders(criteria, carriers, null);
		var entries = Load(criteria, headers);
		return entries;
	}

	public IEnumerable<IRateEntry> Load(RatingCriteria criteria, IEnumerable<IRatingHeader> headers, bool excludeCarrierFilter = false)
	{
		var entries = GetRateEntries(headers, criteria, excludeCarrierFilter, isCosting: true);
		return entries;
	}

	IEnumerable<IRatingHeader> GetHeaders(RatingCriteria criteria, IEnumerable<ZGuid> carriers, ZQuery additionalOrganisationsFilter)
	{
		if (criteria.GatewayConfiguration.IsGatewayShipment)
		{
			return GetIntercompanyTariffRatingHeaders(criteria);
		}

		var headers = new List<IRatingHeader>();

		// Load headers for carriers
		if (carriers != null && carriers.Any())
		{
			headers.AddRange(GetCarrierHeadersSorted(criteria, carriers, additionalOrganisationsFilter));
		}
		else
		{
			var costOrgs = criteria.GetCostsSearchOrgs().WhereNotNull().Select(s => s.Org.PK).Distinct().ToList();
			headers.AddRange(GetCarrierHeaders(criteria, costOrgs, additionalOrganisationsFilter));
		}

		// Load headers for intercompany tariff if required
		if (criteria.GatewayConfiguration.ContinueAutoratingCostFromIntercompanyTariff)
		{
			var intercompanyTariffHeaders = GetIntercompanyTariffRatingHeaders(criteria);
			headers.AddRange(intercompanyTariffHeaders);
		}

		return headers;
	}

	IEnumerable<IRatingHeader> GetCarrierHeaders(RatingCriteria criteria, IEnumerable<ZGuid> carriers, ZQuery additionalOrganisationsFilter)
	{
		var filter = new ZQuery();
		filter.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Costing);

		var companyFilter = new ZQuery(RatingHeaderSchema.TH_GC, criteria.Company.PK);
		companyFilter.AddToFilter(new ZQuery(RatingHeaderSchema.TH_GC, null), JoinCondition.Or);
		filter.AddToFilter(companyFilter);

		ZQuery carrierFilter;

		if (additionalOrganisationsFilter != null)
		{
			carrierFilter = carriers.Any()
				? new ZQuery(RatingHeaderSchema.TH_OH, carriers).And(additionalOrganisationsFilter).Or(new ZQuery(RatingHeaderSchema.TH_OH, null))
				: additionalOrganisationsFilter;
		}
		else
		{
			carrierFilter = carriers.Any()
				? new ZQuery(RatingHeaderSchema.TH_OH, carriers).Or(new ZQuery(RatingHeaderSchema.TH_OH, null))
				: new ZQuery();
		}

		filter.AddToFilter(carrierFilter);
		return factory.Load<RatingHeader>(filter);
	}

	/// <summary>
	///		When carriers are specified, we need to ensure that we return exact headers for those carriers in the same order as carriers.
	///		It is required by CompanyTariffOrCostLineCloneHelper, which for some reason depends on the order of headers loaded.
	///
	///		It is a HACK, something to investigate. If something (CompanyTariffOrCostLineCloneHelper) prioritizes headers for one carrier over
	///		another, it should be done in a more explicit way than relying on orders and expecting that the loader will
	///		return headers in the same order as carriers passed.
	/// </summary>
	IEnumerable<IRatingHeader> GetCarrierHeadersSorted(RatingCriteria criteria, IEnumerable<ZGuid> carriers, ZQuery additionalOrganisationsFilter)
	{
		var headers = GetCarrierHeaders(criteria, carriers, additionalOrganisationsFilter);
		var sorted = carriers
			.Select(carrier => headers.FirstOrDefault(h => h.TH_OH == carrier))
			.WhereNotNull()
			.ToList();

		return sorted;
	}

	IEnumerable<IRatingHeader> GetIntercompanyTariffRatingHeaders(RatingCriteria criteria)
	{
		var filter = new ZQuery();
		filter.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.IntercompanyTariff);
		filter.AddToFilter(RatingHeaderSchema.TH_OH, criteria.GatewayConfiguration.OrganizationsForAutoratingCostFromIntercompanyTariff);

		return factory.Load<RatingHeader>(filter);
	}

	public IEnumerable<IRateEntry> LoadSpotRates(RatingCriteria criteria)
	{
		return LoadSpotRates(criteria, true);
	}
}