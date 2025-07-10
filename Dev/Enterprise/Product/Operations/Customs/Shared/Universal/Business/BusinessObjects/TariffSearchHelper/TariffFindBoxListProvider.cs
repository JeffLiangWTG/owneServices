using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class TariffFindBoxListProvider : IFindBoxListProvider
	{
		public TariffFindBoxListProvider(ZString dataGroupingCode, ZString tariffType, Func<ZDateTime> getEffectiveDate,
			ZQuery tariffAdditionalFilter, ITariffFormatter tariffFormatter)
		{
			this.dataGroupingCode = dataGroupingCode;
			this.tariffType = tariffType;
			this.getEffectiveDate = getEffectiveDate;
			this.tariffAdditionalFilter = tariffAdditionalFilter;
			this.tariffFormatter = tariffFormatter;
		}

		public TariffFindBoxListProvider(ZString dataGroupingCode, ZString tariffType, Func<ZDateTime> getEffectiveDate,
			ZQuery tariffAdditionalFilter, ITariffFormatter tariffFormatter, bool showNearestMatchDesciption)
			: this(dataGroupingCode, tariffType, getEffectiveDate, tariffAdditionalFilter, tariffFormatter)
		{
			this.showNearestMatchDesciption = showNearestMatchDesciption;
		}

		readonly ZString dataGroupingCode;
		readonly ZString tariffType;
		readonly Func<ZDateTime> getEffectiveDate;
		readonly ZQuery tariffAdditionalFilter;
		readonly ITariffFormatter tariffFormatter;
		readonly bool showNearestMatchDesciption;

		ZString[] DataGroupingCodes => dataGroupingCodes ?? (dataGroupingCodes = GetDataGroupingCodes().ToArray());
		ZString[] dataGroupingCodes;

		IEnumerable<ZString> GetDataGroupingCodes()
		{
			yield return dataGroupingCode;

			var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(new BusinessObjectFactory(), dataGroupingCode);
			if (!parentDataGroupingCode.IsEmpty)
			{
				yield return parentDataGroupingCode;
			}
		}

		#region IFindBoxListProvider Members

		ZQuery GetTariffQuery(ZString tariffCode, bool exactMatch = false)
		{
			ZQuery result;
			if (!tariffCode.IsEmpty)
			{
				result = GetFilter();
				result.AddToFilter(TariffViewSchema.ZZ1_TariffCode, exactMatch ? SQLComparisonOperator.Equal : SQLComparisonOperator.StartsWith, tariffCode);
			}
			else
			{
				result = ZQuery.NoResultQuery;
			}
			return result;
		}

		ZQuery GetFilter()
		{
			var query = new ZQuery(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, DataGroupingCodes);
			if (getEffectiveDate != null)
			{
				var effectiveDate = getEffectiveDate.Invoke();
				effectiveDate = effectiveDate.IsValid ? effectiveDate : ZDateTime.Today;
				query.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, effectiveDate);
				query.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, effectiveDate);
			}

			if (!tariffType.IsEmpty)
			{
				query.AddToFilter(TariffViewSchema.ZZ1_ZZI_NKTariffType, tariffType);
			}
			if (tariffAdditionalFilter != null)
			{
				query.AddToFilter(tariffAdditionalFilter);
			}
			return query;
		}

		(string, bool) IFindBoxListProvider.NearestMatch(string code, bool explicitAutoComplete, int cursor)
		{
			return ((IFindBoxListProvider)this).NearestMatchCore(code, explicitAutoComplete);
		}

		(string, bool) IFindBoxListProvider.NearestMatchCore(string code, bool explicitAutoComplete)
		{
			var result = code;
			var success = false;
			if (!string.IsNullOrEmpty(code))
			{
				var tariffCode = tariffFormatter?.Format(code) ?? code;
				var filter = GetTariffQuery(tariffCode);
				filter.OrderBy = TariffViewSchema.Constants.ZZ1_TariffCode + OrderByClause.Descending;
				var factory = new BusinessObjectFactory();
				var match = factory.LoadTop1<TariffView>(filter);
				if (match != null)
				{
					result = match.ZZ1_TariffCode;
					success = true;
				}
			}
			return (result, success);
		}

		string IFindBoxListProvider.CodeFromPrimaryKey(ZGuid pk)
		{
			var factory = new BusinessObjectFactory();
			var tariff = factory.Load<TariffView>(pk);
			return tariff?.ZZ1_TariffCode ?? ZString.Empty;
		}

		ZGuid IFindBoxListProvider.PrimaryKeyFromCode(string code)
		{
			throw new NotSupportedException();
		}

		string IFindBoxListProvider.DescriptionFromPrimaryKey(ZGuid pk)
		{
			var factory = new BusinessObjectFactory();
			var tariff = factory.Load<TariffView>(pk);
			return tariff?.ZZ1_Description ?? ZString.Empty;
		}

		BusinessObject IFindBoxListProvider.GetBusinessObjectFromCode(string code)
		{
			return ((IFindBoxListProvider)this).GetBusinessObjectFromCodeWithoutFilter(code);
		}

		BusinessObject IFindBoxListProvider.GetBusinessObjectFromCodeWithoutFilter(string code)
		{
			return ((IFindBoxListProvider)this).GetBusinessObjectsFromCodeWithoutFilter(code).Cast<TariffView>().FirstOrDefault();
		}

		IEnumerable<BusinessObject> IFindBoxListProvider.GetBusinessObjectsFromCode(string code)
		{
			return ((IFindBoxListProvider)this).GetBusinessObjectsFromCodeWithoutFilter(code);
		}

		IEnumerable<BusinessObject> IFindBoxListProvider.GetBusinessObjectsFromCodeWithoutFilter(string code)
		{
			var tariffCode = tariffFormatter?.Format(code) ?? code;
			var filter = GetTariffQuery(tariffCode, true);
			var factory = new BusinessObjectFactory();
			var tariffs = factory.Load<TariffView>(filter).OrderByDescending(x => x.ZZ1_StartDate);
			var exactMatched = tariffs.FirstOrDefault(x => x.ZZ1_TariffCode == tariffCode);
			if (exactMatched != null)
			{
				yield return exactMatched;
			}
			else
			{
				foreach (var tariff in tariffs)
				{
					yield return tariff;
				}
			}
		}

		string IFindBoxListProvider.DescriptionFromCode(string code)
		{
			var result = "";
			var tariffCode = tariffFormatter?.Format(code) ?? code;
			if (!tariffCode.IsEmpty)
			{
				var filter = GetTariffQuery(tariffCode, !showNearestMatchDesciption);
				if (showNearestMatchDesciption)
				{
					filter.OrderBy = TariffView.Schema.ZZ1_TariffCode + ",";
				}
				filter.OrderBy += TariffView.Schema.ZZ1_StartDate + OrderByClause.Descending;
				var factory = new BusinessObjectFactory();
				var match = factory.LoadTop1<TariffView>(filter);
				if (match != null)
				{
					result = match.ZZ1_Description;
				}
			}
			return result;
		}

		public ICodeDescription GetCustomCodeDescription(BusinessObject bizo)
			=> bizo;

		IBusinessObjectCollection IFindBoxListProvider.List => new TariffViewCollection(new BusinessObjectFactory(), GetFilter());

		bool IFindBoxListProvider.AutoCompleteOnCommit => false;

		#endregion
	}
}
