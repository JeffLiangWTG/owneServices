using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Manifest.Business;

class PreviousDocumentRefCusCodeListCombinedCollection : ZZRefCusCodeListCombinedCollection
{
	protected PreviousDocumentRefCusCodeListCombinedCollection(BusinessObjectFactory factory, ZString codeType, ZDateTime date) : base(factory)
	{
		this.codeType = codeType;
		this.date = date;
	}

	protected override ZQuery CreateRelationshipFilter()
	{
		return ZZRefCusCodeListCombined.Loader.GetFilter(Factory,
			new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN, Core.Constants.CountryCodes.Norway },
			new[] { codeType },
			date,
			null,
			false);
	}

	public static PreviousDocumentRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString codeType, ZDateTime effectiveDate)
	{
		var cacheKey = FormattableString.Invariant($"PreviousDocumentRefCusCodeListCombinedCollection_NO_EUN_{codeType}_{effectiveDate}");
		return factory.GetCachedValue(cacheKey, () =>
		{
			var collection = new PreviousDocumentRefCusCodeListCombinedCollection(factory, codeType, effectiveDate);
			collection.FilterBusinessObjectDefaults.Add(new(Constants.ZZRefCusCodeListFilters.ListType, (NoResString)"Property", codeType));
			collection.FilterBusinessObjectDefaults.Add(new(Constants.ZZRefCusCodeListFilters.EffectiveDate, (NoResString)"Property1", effectiveDate));
			return collection;
		});
	}

	readonly ZString codeType;
	readonly ZDateTime date;
}
