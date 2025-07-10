using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class TariffRelationshipViewCollection : TariffEffectiveDatesRelatedFilteredCollection<TariffRelationshipView>
	{
		public TariffRelationshipViewCollection(TariffView tariffView)
			: base(tariffView, new ZQuery(), TariffRelationshipViewSchema.ZZH_ZZ1_LinkedTariffOrNationalCode, false, false)
		{
		}

		protected TariffRelationshipViewCollection(TariffView tariffView, ZQuery filter, bool enableEffectiveDateFilter)
			: base(tariffView, filter, false, enableEffectiveDateFilter)
		{
		}

		public static TariffRelationshipViewCollection NewChildTariffRelationshipCollection(TariffView tariffView)
		{
			var query = NewChildTariffRelationshipCollectionQuery(tariffView);
			return new TariffRelationshipViewCollection(tariffView, query, false);
		}

		protected static ZQuery NewChildTariffRelationshipCollectionQuery(TariffView tariffView)
		{
			var query = new ZDBOnlyQuery(typeof(TariffRelationshipView));
			query.AddSubQuery(GetChildTariffRelationshipSubQuery(tariffView.Factory, tariffView.ZZ1_ZZZ_NKDataGrouping, tariffView.ZZ1_TariffCode, tariffView.ZZ1_ZZI_TariffTypeCode), JoinCondition.And);
			return query;
		}

		public static ZDBOnlySubQuery GetChildTariffRelationshipSubQuery(BusinessObjectFactory factory, ZString dataGroupingCode, ZString relatedTariffCode, ZString relatedTariffType, SchemaColumn key = null)
		{
			var relatedTariffSubQuery = new ZDBOnlySubQuery(typeof(TariffRelationshipView), key ?? TariffRelationshipViewSchema.PK);
			if (!relatedTariffCode.IsEmpty)
			{
				relatedTariffSubQuery.AddFilterAndZSQLParameterCollection(ZString.Format("@TariffCode LIKE {1} + '%'", relatedTariffCode, TariffRelationshipViewSchema.ZZH_TariffCode.Name),
					new ZSqlParameterCollection(ZSqlParameter.New("@TariffCode", relatedTariffCode, TariffRelationshipViewSchema.ZZH_TariffCode)));
			}
			if (!relatedTariffType.IsEmpty)
			{
				var tariffTypeSubQuery = new ZDBOnlySubQuery(typeof(RefCusTariffType), TariffRelationshipViewSchema.ZZH_ZZI_TariffType);
				tariffTypeSubQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, relatedTariffType);
				tariffTypeSubQuery.AddToFilter(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, dataGroupingCode));
				relatedTariffSubQuery.AddSubQuery(tariffTypeSubQuery, JoinCondition.And);
			}
			return relatedTariffSubQuery;
		}

		static ZQuery GetNonGeneralTariffFilter()
		{
			return new ZQuery(TariffRelationshipViewSchema.ZZH_TariffCode, SQLComparisonOperator.NotEqual, ZString.Empty);
		}

		public bool ExcludeGeneralTariffs
		{
			get { return excludeGeneralTariffs; }
			set
			{
				if (excludeGeneralTariffs != value)
				{
					excludeGeneralTariffs = value;
					if (excludeGeneralTariffs)
					{
						var query = new ZQuery(AdditionalFilter);
						query.AddToFilter(GetNonGeneralTariffFilter());
						AdditionalFilter = query;
					}
					else
					{
						var query = new ZQuery();
						var parts = AdditionalFilter.GetCompositeParts();
						foreach (var part in parts)
						{
							if (!part.Equals(GetNonGeneralTariffFilter()))
							{
								query.AddToFilter(part);
							}
						}
						AdditionalFilter = query;
					}
				}
			}
		}
		bool excludeGeneralTariffs;

		protected override object[] GetCollectionState()
		{
			return new object[] { excludeGeneralTariffs };
		}
	}
}
