using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	class ValueAnalysisQuantityFilter : ModuleNumberRangeFilter
	{
		public enum Context { Any, JobProfit, JobCost, JobRevenue, Revenue, Committed }

		public ValueAnalysisQuantityFilter(ZString description, SchemaNumericColumn column, bool isTradedFilter, Context context = Context.Any)
			: base(description, delegate
			{ return new ZQuery(); })
		{
			Column = column;
			FilterContext = context;
			IsTradedFilter = isTradedFilter;
		}

		readonly SchemaNumericColumn Column;
		readonly Context FilterContext;
		readonly bool IsTradedFilter;

		#region Period

		public ZString Period
		{
			get => fPeriod;
			set
			{
				if (fPeriod != value)
				{
					InvalidateCachedQuery();
					fPeriod = value;
					PeriodInfo.RefreshBinding();
				}
			}
		}
		ZString fPeriod;

		public ZPropertyInfo PeriodInfo => GetZPropertyInfo(nameof(Period));

		[List("PeriodDescriptionList")]
		[ResourceStringData("ValueAnalysisQuantityFilter|PeriodDescription", Caption = "Analysis Period")]
		public ZString PeriodDescription
		{
			get
			{
				return PeriodList.GetDescriptionFromCode(Period);
			}
			set
			{
				Period = PeriodList.GetCodeFromDescription(value);
			}
		}

		public ZPropertyInfo PeriodDescriptionInfo => GetWrappedZPropertyInfo(nameof(PeriodDescription), x => PeriodInfo);

		public SalesAnalysisPeriodList PeriodList => fPeriodList ?? (fPeriodList = new SalesAnalysisPeriodList());
		SalesAnalysisPeriodList fPeriodList;

		public CodeDescriptionPairList PeriodDescriptionList => SalesAnalysisPeriodListUtils.GetPeriodDescriptionList(PeriodList);

		#endregion

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);
			var filter = filterToCopyFrom as ValueAnalysisQuantityFilter;
			if (filter != null)
			{
				Period = filter.Period;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Period = SalesAnalysisPeriodList.Codes.Trailing12Months;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			Period = SalesAnalysisPeriodList.Codes.Trailing12Months;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it is sql statement")]
		protected override ZQuery GetQuery()
		{
			var result = new ZDBOnlyQuery(typeof(ViewValueAnalysis));
			var parameters = new ZSqlParameterCollection();

			var having = FilterContext == Context.JobProfit ? "(SUM(PAV_Revenue)-SUM(PAV_Cost))" : string.Format(CultureInfo.InvariantCulture, "SUM({0})", Column.Name);

			if (IsGreaterThanOrEqualToSearch)
			{
				parameters.Add("@Property1", Property1, Column);
				having += ">=@Property1";
			}
			else if (IsLessThanOrEqualToSearch)
			{
				parameters.Add("@Property2", Property2, Column);
				having += "<=@Property2";
			}
			else if (IsEqualToSearch)
			{
				parameters.Add("@Property1", Property1, Column);
				having += "=@Property1";
			}
			else // IsBetweenSearch
			{
				parameters.Add("@Property1", Property1, Column);
				parameters.Add("@Property2", Property2, Column);
				having += string.Format(CultureInfo.InvariantCulture, ">=@Property1 AND {0}<=@Property2", having);
			}

			var tables = $"{Column.TableSchema.SqlSchemaName}.{Column.TableName}";
			if (tables == $"{OrgTradeValueSchema.Constants.SqlSchemaName}.{OrgTradeValueSchema.Constants.TableName}")
			{
				tables += " JOIN dbo.OrgTradePeriod ON PAS_PK=PAV_PAS";
			}

			tables += string.Format(CultureInfo.InvariantCulture, " JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = {0}", IsTradedFilter ? 1 : 0);

			if (IsTradedFilter)
			{
				if (FilterContext != Context.Any)
				{
					tables += string.Format(CultureInfo.InvariantCulture, " AND PAV_GC = '{0}'", Env.CurrentCompanyPK);
				}

				if (FilterContext == Context.Revenue || FilterContext == Context.Any)
				{
					tables += " AND PAS_IsJobValue = 0";
				}
				else
				{
					tables += " AND PAS_IsJobValue = 1";
				}
			}
			else
			{
				tables += " AND PA_Status = 'SUC'";

				if (FilterContext == Context.Committed)
				{
					tables += " AND PAS_IsForecast = 0";
				}
			}

			if (Period != SalesAnalysisPeriodList.Codes.TotalTradingLifetime)
			{
				var firstDayOfTheMonth = new ZDate(ZDate.Today.Year, ZDate.Today.Month, 1);
				var startDate = SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(firstDayOfTheMonth, Period);
				parameters.Add("@startDate", startDate, OrgTradePeriodSchema.PAS_Period);
				tables += " AND PAS_Period>=@startDate";

				var endDate = SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(firstDayOfTheMonth, Period);
				if (!endDate.IsEmpty)
				{
					parameters.Add("@endDate", endDate, OrgTradePeriodSchema.PAS_Period);
					tables += " AND PAS_Period<@endDate";
				}
			}

			string sql = string.Format(CultureInfo.InvariantCulture, "EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM {0} GROUP BY PA_PK, PAS_OH_Client HAVING {1}) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)", tables, having);
			result.AddFilterAndZSQLParameterCollection(sql, parameters);
			return result;
		}

		#region XML Serialization

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Xml element")]
		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			if (reader.Name == "Period")
			{
				Period = reader.ReadElementContentAsString();
			}

			if (reader.Name == "PropertySearch")
			{
				PropertySearch = reader.ReadElementContentAsString();
			}

			base.DeserializePropertiesFromXml(reader);
		}

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			writer.WriteElementString("Period", Period);
			writer.WriteElementString("PropertySearch", PropertySearch);
			base.SerializePropertiesToXml(writer);
		}
		#endregion
	}
}
