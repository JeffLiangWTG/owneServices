using System;
using System.Globalization;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public class TransitTimeModuleFilter : ModuleFilter
	{
		public TransitTimeModuleFilter(ZString description, SchemaColumn transitHoursColumn)
			: base(description, transitHoursColumn)
		{
			comparisonOperator = ComparisonOperatorConstants.Equal;
		}

		#region Properties

		[List("ComparisonOperators")]
		public ZString ComparisonOperator
		{
			get { return comparisonOperator; }
			set
			{
				if (comparisonOperator != value)
				{
					comparisonOperator = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateComparisonOperator();
					}

					ComparisonOperatorInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		ZString comparisonOperator;

		public ZPropertyInfo ComparisonOperatorInfo
		{
			get { return GetZPropertyInfo(nameof(ComparisonOperator)); }
		}

		public ZInt TransitDays
		{
			get { return transitDays; }
			set
			{
				if (transitDays != value)
				{
					transitDays = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateTransitDays();
					}

					TransitDaysInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		ZInt transitDays;

		public ZPropertyInfo TransitDaysInfo
		{
			get { return GetZPropertyInfo(nameof(TransitDays)); }
		}

		public ZInt TransitHours
		{
			get { return transitHours; }
			set
			{
				if (transitHours != value)
				{
					transitHours = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateTransitHours();
					}

					TransitHoursInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		ZInt transitHours;

		public ZPropertyInfo TransitHoursInfo
		{
			get { return GetZPropertyInfo(nameof(TransitHours)); }
		}

		#endregion

		#region ComparisonOperators

		public CodeDescriptionPairList ComparisonOperators
		{
			get { return lazyComparisonOperators.Value; }
		}

		readonly Lazy<CodeDescriptionPairList> lazyComparisonOperators = new Lazy<CodeDescriptionPairList>(GetComparisonOperators);

		static CodeDescriptionPairList GetComparisonOperators()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ComparisonOperatorConstants.Equal, Res.GetString("f08209b5-5012-4787-af1d-b90412b48f24", "Matches jobs with this exact Transit Time"));
			result.AddPair(ComparisonOperatorConstants.GreaterThan, Res.GetString("fc31e81e-0ebd-4483-acaf-ec74c9bd322e", "Matches jobs with Transit Time greater than the supplied value"));
			result.AddPair(ComparisonOperatorConstants.GreaterThanOrEqual, Res.GetString("8cd708d5-d8f3-4f46-85b4-92a9be94cd1f", "Matches jobs with Transit Time greater than or equal to the supplied value"));
			result.AddPair(ComparisonOperatorConstants.LessThan, Res.GetString("a7fcdffb-7925-4008-ba1a-6e0400c43a9a", "Matches jobs with Transit Time less than the supplied value"));
			result.AddPair(ComparisonOperatorConstants.LessThanOrEqual, Res.GetString("2da01174-ac4e-44d0-aae3-a6daf327e51b", "Matches jobs with Transit Time less than or equal to the supplied value"));
			return result;
		}

		public SQLComparisonOperator SQLComparisonOperator
		{
			get
			{
				switch (ComparisonOperator)
				{
					case ComparisonOperatorConstants.Equal:
						return SQLComparisonOperator.Equal;
					case ComparisonOperatorConstants.GreaterThan:
						return SQLComparisonOperator.GreaterThan;
					case ComparisonOperatorConstants.GreaterThanOrEqual:
						return SQLComparisonOperator.GreaterThanOrEqualTo;
					case ComparisonOperatorConstants.LessThan:
						return SQLComparisonOperator.LessThan;
					case ComparisonOperatorConstants.LessThanOrEqual:
						return SQLComparisonOperator.LessThanOrEqualTo;
					default:
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Empty or unsupported comparison operator: {0}", ComparisonOperator));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant")]
		class ComparisonOperatorConstants
		{
			public const string Equal = "Equals";
			public const string GreaterThan = "Greater than";
			public const string GreaterThanOrEqual = "Greater than or equal";
			public const string LessThan = "Less than";
			public const string LessThanOrEqual = "Less than or equal";
		}

		#endregion

		#region Implementation

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException();
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var otherFilter = filterToCopyFrom as TransitTimeModuleFilter;

			if (otherFilter != null)
			{
				ComparisonOperator = otherFilter.ComparisonOperator;
				TransitDays = otherFilter.TransitDays;
				TransitHours = otherFilter.TransitHours;
			}
		}

		protected override void ClearCore()
		{
			ComparisonOperator = ComparisonOperatorConstants.Equal;
			TransitDays = 0;
			TransitHours = 0;
		}

		protected override bool IsEmptyCore => TransitDays == 0 && TransitHours == 0;

		protected override FilterCategory DefaultCategory => FilterCategories.Other;

		public override bool IsExpensiveQuery => false;

		ZInt TotalHours => (TransitDays * 24) + TransitHours;

		protected override object[] QueryDelegateParameters => new object[] { SQLComparisonOperator, TotalHours };

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			return new ZQuery(FilterColumn, SQLComparisonOperator, TotalHours);
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("ComparisonOperator", ComparisonOperator);
			writer.WriteElementString("TransitDays", TransitDays.ToString());
			writer.WriteElementString("TransitHours", TransitHours.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			ComparisonOperator = reader.ReadElementString("ComparisonOperator");
			TransitDays = ZInt.ParseSafe(reader.ReadElementString("TransitDays"), 0);
			TransitHours = ZInt.ParseSafe(reader.ReadElementString("TransitHours"), 0);
		}

		#endregion

		#region Validation

		public new TransitTimeModuleFilterValidation Validation
		{
			get { return (TransitTimeModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new TransitTimeModuleFilterValidation(this);
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			TransitDays = 1;
		}

#endif
		#endregion
	}
}
