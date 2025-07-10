using System;
using System.ComponentModel;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.GUI
{
	public delegate ZQuery GetDecimalQuery(SQLComparisonOperator comparisonOperator, ZInt value);

	public class CampaignContactNumberFilter : ModuleFilter
	{
		public CampaignContactNumberFilter(ZString description, SchemaNumericColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		protected CampaignContactNumberFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public CampaignContactNumberFilter(ZString description, GetDecimalQuery queryDelegate)
			: base(description, queryDelegate)
		{
		}

		#region CopyPersistantValuesFromFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new CampaignContactNumberFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			CampaignContactNumberFilter filter = (CampaignContactNumberFilter)filterToCopyFrom;
			Property = filter.Property;

			if (SupportsComparisonOperatorSet)
			{
				ComparisonOperator = filter.ComparisonOperator;
			}
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		#endregion

		#region class ComparisonConstants

		public static class ComparisonConstants
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
		public virtual ZString ComparisonOperator // for binding
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

		public SQLComparisonOperator SqlComparisonOperator
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

		public ZPropertyInfo ComparisonOperatorInfo
		{
			get { return GetZPropertyInfo(nameof(ComparisonOperator)); }
		}

		public CodeDescriptionPairList ComparisonOperator_List
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

		protected virtual bool SupportsComparisonOperatorSet
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

		#region Clear / IsEmpty / Defaults

		protected override void ClearCore()
		{
			Property = DefaultProperty;

			if (SupportsComparisonOperatorSet)
			{
				ComparisonOperator = DefaultComparisonOperator;
			}
		}

		protected override bool IsEmptyCore => false;

		public ZInt DefaultProperty
		{
			get { return defaultProperty; }
			set
			{
				defaultProperty = value;
				Property = value;
			}
		}

		ZInt defaultProperty;

		public ZString DefaultComparisonOperator
		{
			get { return defaultComparisonOperator; }
			set
			{
				EnsureSupportsComparisonOperatorSet();

				if (defaultComparisonOperator != value)
				{
					if (value.EqualsIgnoringCase(ComparisonConstants.Exact) ||
						value.EqualsIgnoringCase(ComparisonConstants.LessThan) ||
						value.EqualsIgnoringCase(ComparisonConstants.GreaterThan) ||
						value.EqualsIgnoringCase(ComparisonConstants.GreaterThanOrEqualTo))
					{
						defaultComparisonOperator = value;
						ComparisonOperator = value;
					}
					else
					{
						throw new ArgumentException("Only Equal, LessThan, GreaterThan and GreaterThanOrEqualTo are supported.");
					}
				}
			}
		}

		ZString defaultComparisonOperator = ComparisonConstants.Default;

		#endregion

		#region Property

		public virtual ZInt Property
		{
			get { return property; }
			set
			{
				if (property != value)
				{
					property = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty();
					}
					PropertyInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PropertyInfo
		{
			get { return GetZPropertyInfo(nameof(Property)); }
		}

		ZInt property;

		#endregion

		#region PropertyValidation

		public Validation PropertyValidation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return propertyValidation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { propertyValidation = value; }
		}
		Validation propertyValidation;

		#endregion

		#region Validation

		public new CampaignContactNumberFilterValidation Validation
		{
			get { return (CampaignContactNumberFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new CampaignContactNumberFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, Property }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			if (Property > 0)
			{
				return new ZQuery(FilterColumn, SqlComparisonOperator, Property);
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			if (SupportsComparisonOperatorSet)
			{
				writer.WriteElementString("Comparer", ComparisonOperator);
			}
			writer.WriteElementString("Property", Property.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "XML element")]
		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "Comparer")
			{
				string temp = reader.ReadElementString("Comparer");
				if (SupportsComparisonOperatorSet)
				{
					ComparisonOperator = temp;
				}
			}

			if (reader.Name == "Property")
			{
				string propertyAsString = reader.ReadElementString("Property");
				Property = ZInt.ParseSafe(propertyAsString, 0);
			}
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property = 1;
		}

#endif
		#endregion
	}

	#region class CampaignContactNumberFilterValidation

	public class CampaignContactNumberFilterValidation : ModuleFilterValidation
	{
		public CampaignContactNumberFilterValidation(CampaignContactNumberFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		#region ValidateProperty

		public void ValidateProperty()
		{
			ValidateCalculatedProperty(Parent.PropertyInfo);
		}

		protected virtual void CheckProperty()
		{
			if (Parent.PropertyValidation != null)
			{
				Parent.PropertyValidation(Parent.PropertyInfo);
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateProperty();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		protected readonly CampaignContactNumberFilter Parent;
	}

	#endregion
}
