using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business.RateSelector
{
	public interface IRateSelectorFilterValueProvider
	{
		BusinessObjectFactory Factory { get; }
		RatingCriteria OriginalCriteria { get; }

		string OriginCode { get; }
		string DestinationCode { get; }
		string CommodityCodeFromFilter { get; }
		IEnumerable<string> UniversalCommodityGroupsFromFilters { get; }
		IEnumerable<string> CommodityCodesFromJobContainers { get; }
		IEnumerable<string> ContractNumbersFromFilters { get; }
		IEnumerable<string> CarrierServiceLevelsFromFilters { get; }
		IEnumerable<string> PaymentTermsFromFilters { get; }
		IEnumerable<ZGuid> CarrierPKs { get; }
		IEnumerable<OrgWithSource> Carriers { get; }
		string OrgSourceText { get; }
		ZDateTime EffectiveDate { get; }

		RatingCriteria CreateCriteria();
		(RatesQuery ratesQuery, bool isValidForRatesService) BuildRatesQuery(ILogger logger);
		ZQuery GetAdditionalCarrierOrgListFilter(Type typeOfBusinessObjectToQuery, SchemaColumn fieldName);

		void SetCargoSphereFilters(IEnumerable<RefCargoSphereContract> contracts, IEnumerable<RefCargoSphereNamedAccount> namedAccounts);
	}
}
