using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.GUI
{
	public class CampaignContactTextRangeFilter : ModuleTextFilter
	{
		public CampaignContactTextRangeFilter(ZString description, GetTextQueryWithOperator queryDelegate, IList listDelegate)
			: base(description, queryDelegate, listDelegate)
		{
		}

		public CampaignContactTextRangeFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		protected override SQLComparisonOperator DefaultSqlComparisonOperator
		{
			get
			{
				return SQLComparisonOperator.Equal;
			}
		}

		#region class ComparisonConstants

		new public static class ComparisonConstants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used in SQL expression")]
			public const string Exact = "equals";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used in SQL expression")]
			public const string LessThan = "less than";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used in SQL expression")]
			public const string GreaterThan = "greater than";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used in SQL expression")]
			public const string LessThanOrEqualTo = "less than or equal to";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used in SQL expression")]
			public const string GreaterThanOrEqualTo = "greater than or equal to";

			public const string Default = Exact;
		}

		#endregion

		#region Comparison Operator

		[BusinessObjectTestExclude] // "?" means invalid value
		[EditorBrowsable(EditorBrowsableState.Never)]
		[List("ComparisonOperator_List")]
		public override ZString ComparisonOperator // for binding
		{
			get { return comparisonOperator; }
			set
			{
				EnsureSupportsComparisonOperatorSet();

				if (comparisonOperator != value)
				{
					if (!value.IsEmpty &&
						!value.EqualsIgnoringCase(ComparisonConstants.Exact) &&
						!value.EqualsIgnoringCase(ComparisonConstants.LessThan) &&
						!value.EqualsIgnoringCase(ComparisonConstants.GreaterThan) &&
						!value.EqualsIgnoringCase(ComparisonConstants.LessThanOrEqualTo) &&
						!value.EqualsIgnoringCase(ComparisonConstants.GreaterThanOrEqualTo))
					{
						comparisonOperator = "?";
					}
					else
					{
						comparisonOperator = value;
					}
					ComparisonOperatorInfo.RefreshBinding();
				}
			}
		}

		public override SQLComparisonOperator SqlComparisonOperator
		{
			get
			{
				switch (ComparisonOperator)
				{
					case ComparisonConstants.Exact:
						return SQLComparisonOperator.Equal;
					case ComparisonConstants.LessThan:
						return SQLComparisonOperator.LessThan;
					case ComparisonConstants.GreaterThan:
						return SQLComparisonOperator.GreaterThan;
					case ComparisonConstants.LessThanOrEqualTo:
						return SQLComparisonOperator.LessThanOrEqualTo;
					case ComparisonConstants.GreaterThanOrEqualTo:
						return SQLComparisonOperator.GreaterThanOrEqualTo;
					default:
						return SQLComparisonOperator.Equal;
				}
			}
			set
			{
				EnsureSupportsComparisonOperatorSet();

				if (value == SQLComparisonOperator.Equal)
				{
					ComparisonOperator = ComparisonConstants.Exact;
				}
				else if (value == SQLComparisonOperator.LessThan)
				{
					ComparisonOperator = ComparisonConstants.LessThan;
				}
				else if (value == SQLComparisonOperator.GreaterThan)
				{
					ComparisonOperator = ComparisonConstants.GreaterThan;
				}
				else if (value == SQLComparisonOperator.LessThanOrEqualTo)
				{
					ComparisonOperator = ComparisonConstants.LessThanOrEqualTo;
				}
				else if (value == SQLComparisonOperator.GreaterThanOrEqualTo)
				{
					ComparisonOperator = ComparisonConstants.GreaterThanOrEqualTo;
				}
				else
				{
					throw new ArgumentException("Only Equal, LessThan, and GreaterThan are supported.");
				}
			}
		}

		new public CodeDescriptionPairList ComparisonOperator_List
		{
			get
			{
				if (comparisonOperator_List == null)
				{
					comparisonOperator_List = new CodeDescriptionPairList();
					comparisonOperator_List.AddPair(ComparisonConstants.Exact, Res.GetString("20ec35c2-5cb2-4c32-a0f4-3e69c6ff2454", "Search for an Exact match"));
					comparisonOperator_List.AddPair(ComparisonConstants.LessThan, Res.GetString("7f4fc9a5-3837-4d49-a86b-8a59f670990d", "Search for fields that are Less Than the supplied value"));
					comparisonOperator_List.AddPair(ComparisonConstants.GreaterThan, Res.GetString("9ec6748f-2a74-4800-b6db-e530acaaf345", "Search for fields that are Greater Than the supplied value"));
					comparisonOperator_List.AddPair(ComparisonConstants.LessThanOrEqualTo, Res.GetString("49275ca7-ed14-4e79-8eeb-8cfc3e645e9a", "Search for fields that are Less Than or Equal to the supplied value"));
					comparisonOperator_List.AddPair(ComparisonConstants.GreaterThanOrEqualTo, Res.GetString("bdfe9b5e-28ec-4183-a50c-86ad6ffecd75", "Search for fields that are Greater Than or Equal to the supplied value"));
				}
				return comparisonOperator_List;
			}
		}

		protected override bool SupportsComparisonOperatorSet
		{
			get { return true; }
		}

		void EnsureSupportsComparisonOperatorSet()
		{
			if (!SupportsComparisonOperatorSet)
			{
				ErrorReporter.ReportOnce("Cannot set SqlComparisonOperator on a module filter",
					"Cannot set SqlComparisonOperator on a module filter of type " + GetType().Name + ".\n" +
					"Filter Description: " + Description);
			}
		}

		ZString comparisonOperator = ComparisonConstants.Default;
		CodeDescriptionPairList comparisonOperator_List;

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, Property }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			if (!Property.IsEmpty)
			{
				return new ZQuery(FilterColumn, SqlComparisonOperator, Property);
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion
	}
}
