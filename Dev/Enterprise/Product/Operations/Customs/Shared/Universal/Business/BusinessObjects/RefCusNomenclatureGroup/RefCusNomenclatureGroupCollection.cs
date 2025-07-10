using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusNomenclatureGroupCollection : ActiveBusinessObjectCollection<RefCusNomenclatureGroup>
	{
		public RefCusNomenclatureGroupCollection(TariffView tariffView)
			: base(tariffView.Factory, GetQuery(tariffView, ZDate.Today))
		{
		}

		public static ZQuery GetQuery(TariffView tariffView, ZDateTime date)
		{
			if (!date.IsValid || tariffView.ZZ1_EndDate < date)
			{
				return ZQuery.NoResultQuery;
			}

			var query = new ZQuery(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualTo, date);
			query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, date);
			var tariffType = tariffView.ZZ1_CompositeKeyOnZZ5.IsEmpty ? null : tariffView.CusTariffType;
			var dataGrouping = tariffType == null || tariffType.ZZI_ZZ9_NKNomenclatureGroupType.IsEmpty ? null : tariffType.DataGrouping;
			if (dataGrouping == null)
			{
				query.IsNoResultQuery = true;
			}
			else
			{
				query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZ9_NKNomenclatureGroupType, tariffType.ZZI_ZZ9_NKNomenclatureGroupType);
				var parameters = new ZSqlParameterCollection();
				parameters.Add("@CompositeKeyOnZZ5", tariffView.ZZ1_CompositeKeyOnZZ5, TariffViewSchema.ZZ1_CompositeKeyOnZZ5);
				query.AddFilterAndZSQLParameterCollection($"(@CompositeKeyOnZZ5 LIKE {RefCusNomenclatureGroupSchema.Constants.ZZ5_CompositeKey} +'%')", parameters);
				var dataGroupingCodes = new List<ZString>();
				dataGroupingCodes.Add(dataGrouping.ZZZ_DataGrouping);
				var parent = dataGrouping.Parent;
				if (parent != null)
				{
					dataGroupingCodes.Add(parent.ZZZ_DataGrouping);
				}
				query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, dataGroupingCodes);
			}
			return query;
		}

		protected override bool AllowNew => false;
	}
}
