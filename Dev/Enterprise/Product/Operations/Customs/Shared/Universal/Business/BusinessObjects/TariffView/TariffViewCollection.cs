using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[ModuleID(ModuleId.RefCusTariff)]
	public class TariffViewCollection : BusinessObjectCollection<TariffView>
	{
		#region ctor

		public TariffViewCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public TariffViewCollection(BusinessObjectFactory factory, string dataGroupingCode, ZDateTime effectiveValuationDate) : this(factory, GetLoadingQuery(factory, dataGroupingCode, ZString.Empty, effectiveValuationDate))
		{
		}

		public TariffViewCollection(BusinessObjectFactory factory, string dataGroupingCode, ZDateTime effectiveValuationDate, ZString[] tariffTypes) : this(factory, GetLoadingQuery(factory, dataGroupingCode, ZString.Empty, effectiveValuationDate))
		{
			this.tariffTypes = tariffTypes;
		}

		public TariffViewCollection(BusinessObjectFactory factory, string dataGroupingCode) : this(factory, GetLoadingQuery(factory, dataGroupingCode, ZString.Empty))
		{
		}

		public TariffViewCollection(BusinessObjectFactory factory, string dataGroupingCode, string tariffType) : this(factory, GetLoadingQuery(factory, dataGroupingCode, tariffType))
		{
		}

		public TariffViewCollection(BusinessObjectFactory factory, string dataGroupingCode, string tariffType, ZDateTime effectiveValuationDate) : this(factory, GetLoadingQuery(factory, dataGroupingCode, tariffType, effectiveValuationDate))
		{
		}

		public TariffViewCollection(BusinessObjectFactory factory, string dataGroupingCode, string tariffType, ZDateTime effectiveValuationDate, IFindBoxListProviderFactory findBoxListProviderFactory) : this(factory, GetLoadingQuery(factory, dataGroupingCode, tariffType, effectiveValuationDate))
		{
			if (findBoxListProviderFactory == null)
			{
				throw new ArgumentNullException(nameof(findBoxListProviderFactory));
			}

			this.findBoxListProviderFactory = findBoxListProviderFactory;
		}

		public TariffViewCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}

		#endregion

		public static TariffViewCollection GetCachedCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, ZDateTime effectiveValuationDate)
		{
			var key = string.Format(CultureInfo.InvariantCulture, "RefCusTariffCollection_{0}_{1}_{2}", dataGroupingCode, tariffType, effectiveValuationDate);
			return factory.GetCachedValue(key, () =>
			{
				var result = new TariffViewCollection(factory, dataGroupingCode, tariffType);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.RefCusTariffFilters.TariffCode, "Property", ZString.Empty, true));
				if (!tariffType.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.RefCusTariffFilters.TariffType, "Property1", dataGroupingCode, false));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.RefCusTariffFilters.TariffType, "Property2", tariffType, false));
				}
				if (effectiveValuationDate.IsValid)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.RefCusTariffFilters.EffectiveDate, "Property1", effectiveValuationDate));
				}
				return result;
			});
		}

		public static TariffViewCollection GetNewCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, ZDateTime effectiveValuationDate, ZString tariffRestrictionFilter, bool addBlankTariffCodeFilter = true)
		{
			var result = new TariffViewCollection(factory, dataGroupingCode, tariffType);
			if (addBlankTariffCodeFilter)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.RefCusTariffFilters.TariffCode, "Property", ZString.Empty, true));
			}
			if (!tariffType.IsEmpty)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.RefCusTariffFilters.TariffType, "Property1", dataGroupingCode, false));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.RefCusTariffFilters.TariffType, "Property2", tariffType, false));
			}
			if (effectiveValuationDate.IsValid)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.RefCusTariffFilters.EffectiveDate, "Property1", effectiveValuationDate));
			}
			if (!tariffRestrictionFilter.IsEmpty)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.RefCusTariffFilters.TariffRestriction, "Property", tariffRestrictionFilter, false));
			}
			return result;
		}

		public static ZDBOnlyQuery GetLoadingQuery(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, ZDateTime? effectiveValuationDate = null)
		{
			var result = new ZDBOnlyQuery(typeof(TariffView));
			if (!dataGroupingCode.IsEmpty)
			{
				result.AddToFilter(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, dataGroupingCode));
			}
			if (!tariffType.IsEmpty)
			{
				result.AddToFilter(TariffViewSchema.ZZ1_ZZI_NKTariffType, SQLComparisonOperator.StartsWith, tariffType);
			}

			if (effectiveValuationDate.HasValue && effectiveValuationDate.Value.IsValid)
			{
				result.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, effectiveValuationDate.Value);
				result.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, effectiveValuationDate.Value);
			}
			return result;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			if (tariffTypes != null && tariffTypes.Any(x => !x.IsEmpty))
			{
				var result = new ZDBOnlyQuery(typeof(TariffView));
				result.AddToFilter(TariffViewSchema.ZZ1_ZZI_NKTariffType, SQLComparisonOperator.StartsWith, tariffTypes.Where(x => !x.IsEmpty));
				return result;
			}

			return base.CreateRelationshipFilter();
		}

		readonly ZString[] tariffTypes;

		protected override bool AllowNewCore => false;

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get
			{
				if (findBoxListProviderFactory != null)
				{
					return findBoxListProviderFactory.Create(this);
				}
				else
				{
					return base.FindBoxListProvider;
				}
			}
		}

		readonly IFindBoxListProviderFactory findBoxListProviderFactory;
	}
}
