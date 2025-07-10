using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class ChildTariffViewCollection : BusinessObjectCollection<TariffView>
	{
		public ChildTariffViewCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, ZDateTime effectiveValuationDate, KeyValuePair<ZString, ZString>[] applicableTariffDetail)
			: base(factory, null)
		{
			DataGroupingCode = dataGroupingCode;
			TariffType = Argument.NotNull(tariffType, "tariffType");

			relationshipFilter = GetRelationshipFilter(factory, dataGroupingCode, tariffType);
			relationshipFilter.AddToFilter(GetAdditionalFilter(factory, dataGroupingCode, effectiveValuationDate, applicableTariffDetail));
		}
		internal ZQuery relationshipFilter;

		public static ChildTariffViewCollection GetNewCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, ZDateTime effectiveValuationDate, KeyValuePair<ZString, ZString>[] relatedTariffTypeAndCode)
		{
			var result = new ChildTariffViewCollection(factory, dataGroupingCode, tariffType, effectiveValuationDate, relatedTariffTypeAndCode);
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.TariffCode, "Property", ZString.Empty, true));
			if (!tariffType.IsEmpty)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.TariffType, "Property1", dataGroupingCode, false));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.TariffType, "Property2", tariffType, false));
			}
			if (effectiveValuationDate.IsValid)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.EffectiveDate, "Property1", effectiveValuationDate));
			}
			return result;
		}

		public static ChildTariffViewCollection GetNewCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, ZDateTime effectiveValuationDate, ZString relatedTariffType, ZString relatedTariffCode)
		{
			return GetNewCollection(factory, dataGroupingCode, tariffType, effectiveValuationDate, new[] { new KeyValuePair<ZString, ZString>(relatedTariffType, relatedTariffCode) });
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return relationshipFilter;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			errors.Add(Res.GetString("b3c7f45f-82e3-4700-81dd-23ab1ddef0c1", "This Tariff cannot be chosen here. Please choose another applicable Tariff."));
		}

		static ZQuery GetRelationshipFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType)
		{
			var result = new ZDBOnlyQuery(typeof(TariffView));
			if (!dataGroupingCode.IsEmpty)
			{
				result.AddToFilter(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, dataGroupingCode));
			}
			if (!tariffType.IsEmpty)
			{
				var tariffTypeSubQuery = new ZDBOnlySubQuery(typeof(RefCusTariffType), TariffViewSchema.ZZ1_ZZI_TariffType);
				tariffTypeSubQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, SQLComparisonOperator.StartsWith, tariffType);
				result.AddSubQuery(tariffTypeSubQuery, JoinCondition.And);
			}

			return result;
		}

		static ZQuery GetAdditionalFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZDateTime effectiveValuationDate, KeyValuePair<ZString, ZString>[] applicableTariffDetail)
		{
			var result = new ZDBOnlyQuery(typeof(TariffView));

			if (effectiveValuationDate.IsValid)
			{
				result.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, effectiveValuationDate);
				result.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, effectiveValuationDate);
			}

			if (applicableTariffDetail != null)
			{
				if (applicableTariffDetail.Any())
				{
					var query = new ZDBOnlyQuery(typeof(TariffView));
					foreach (var relatedTariff in applicableTariffDetail)
					{
						if (!relatedTariff.Value.IsEmpty)
						{
							var subQuery = TariffRelationshipViewCollection.GetChildTariffRelationshipSubQuery(factory, dataGroupingCode, relatedTariff.Value, relatedTariff.Key, TariffRelationshipViewSchema.ZZH_ZZ1_LinkedTariffOrNationalCode);
							query.AddSubQuery(subQuery, JoinCondition.Or);
						}
					}
					result.AddFilterAndZSQLParameterCollection(query.LiteralTextADO, new ZSqlParameterCollection());
				}
				else
				{
					result.AddFilterAndZSQLParameterCollection(ZString.Format("{0} IN (NULL)", TariffViewSchema.Constants.PK), new ZSqlParameterCollection());
				}
			}
			return result;
		}

		RefCusTariffAttributeNameCollection fMandatoryAttributeNames;
		public RefCusTariffAttributeNameCollection MandatoryAttributeNames
		{
			get
			{
				if (fMandatoryAttributeNames == null)
				{
					fMandatoryAttributeNames = new RefCusTariffAttributeNameCollection(this);
					fMandatoryAttributeNames.SetReadOnlyIncludingChildren(true);
				}
				return fMandatoryAttributeNames;
			}
		}

		public ZString TariffType { get; }
		public ZString DataGroupingCode { get; }
	}
}
