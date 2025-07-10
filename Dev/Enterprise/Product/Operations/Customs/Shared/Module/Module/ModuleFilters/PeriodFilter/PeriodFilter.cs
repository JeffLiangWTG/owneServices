using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public class PeriodFilter : ModuleTextFilter
	{
		public delegate ZQuery GetPeriodQueryDelegate(SQLComparisonOperator comparisonOperator, SchemaDateTimeColumn filterColumn, ZInt year, ZInt month);

		public PeriodFilter(ZString description, GetPeriodQueryDelegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		public PeriodFilter(ZString description, SchemaDateTimeColumn filterColumn)
			: this(description, GetPeriodQuery)
		{
			dateTimeFilterColumn = filterColumn;
		}

		protected PeriodFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		#region Properities

		[MaxLength(4)]
		public virtual ZInt PeriodYear
		{
			[DebuggerStepThrough]
			get { return year; }
			set
			{
				SetNonPersistentPropertyValue(PeriodYearInfo, ref year, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidatePeriodYear();
				}
			}
		}

		public virtual ZPropertyInfo PeriodYearInfo
		{
			get { return this.GetZPropertyInfo(nameof(PeriodYear)); }
		}

		[MaxLength(2)]
		public virtual ZInt PeriodMonth
		{
			[DebuggerStepThrough]
			get { return month; }
			set
			{
				SetNonPersistentPropertyValue(PeriodMonthInfo, ref month, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidatePeriodMonth();
				}
			}
		}

		public virtual ZPropertyInfo PeriodMonthInfo
		{
			get { return this.GetZPropertyInfo(nameof(PeriodMonth)); }
		}

		#endregion

		#region Implementation

		public new PeriodFilterValidation Validation
		{
			get { return ((PeriodFilterValidation)(base.Validation)); }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new PeriodFilterValidation(this);
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				return new string[] {
					string.Empty,
					ComparisonConstants.Exact,
					ComparisonConstants.NotEqual,
					ComparisonConstants.IsBlank,
					ComparisonConstants.IsNotBlank };
			}
		}

		public override bool HasComparisonOperator
		{
			get
			{
				return true;
			}
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);
			PeriodFilter source = ((PeriodFilter)(filterToCopyFrom));
			source.year = year;
			source.month = month;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			PeriodYear = ZInt.Zero;
			PeriodMonth = ZInt.Zero;
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new PeriodFilter(category, parentCollection);
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new NotSupportedException();
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("Year", PeriodYear.ToString());
			writer.WriteElementString("Month", PeriodMonth.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			PeriodYear = ZInt.ParseSafe(reader.ReadElementString("Year"), ZInt.Zero);
			PeriodMonth = ZInt.ParseSafe(reader.ReadElementString("Month"), ZInt.Zero);
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && (PeriodYear.IsEmpty || PeriodMonth.IsEmpty);

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, dateTimeFilterColumn, PeriodYear, PeriodMonth }; }
		}

		ZInt year;
		ZInt month;
		readonly SchemaDateTimeColumn dateTimeFilterColumn;

		#endregion

		#region GetPeriodQuery

		static ZQuery GetPeriodQuery(SQLComparisonOperator comparisonOperator, SchemaDateTimeColumn filterColumn, ZInt year, ZInt month)
		{
			ZQuery result = new ZQuery();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				result.AddToFilter(filterColumn, null);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(filterColumn, SQLComparisonOperator.NotEqual, null);
			}
			else if (year != ZInt.Zero && month != ZInt.Zero)
			{
				ZDateTime startDate = new ZDateTime(year, month, 1);

				if (startDate.IsValid)
				{
					if (comparisonOperator == SQLComparisonOperator.Equal)
					{
						result.AddToFilter(filterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);
						result.AddToFilter(filterColumn, SQLComparisonOperator.LessThan, startDate.AddMonths(1));
					}
					else if (comparisonOperator == SQLComparisonOperator.NotEqual)
					{
						result.AddToFilter(filterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, startDate.AddMonths(1));
						result.AddToFilter(JoinCondition.Or, filterColumn, SQLComparisonOperator.LessThan, startDate);
						result.AddToFilter(JoinCondition.Or, filterColumn, null);
					}
				}
			}

			return result;
		}

		#endregion
	}
}
