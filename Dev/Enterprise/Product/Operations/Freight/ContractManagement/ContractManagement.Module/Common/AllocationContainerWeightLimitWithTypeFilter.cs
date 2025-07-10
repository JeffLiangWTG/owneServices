using System;
using System.Windows.Forms;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Module.Common;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Module
{
	public delegate ZQuery GetUnitRangeWithTypeQuery(INumericZType value1, INumericZType value2, ZString unit);

	public class AllocationContainerWeightLimitWithTypeFilter : ModuleTextFilter
	{
		#region Construction

		public CodeDescriptionPairList Units { get; set; }

		public AllocationContainerWeightLimitWithTypeFilter(ZString description, GetUnitRangeWithTypeQuery withTypeQueryDelegate, CodeDescriptionPairList unitList)
			: base(description, withTypeQueryDelegate, unitList)
		{
			Units = unitList;
			DefaultLimitType = LimitTypeList[ContainerWeightLimitType.AbsolutePerTEU].Code;
			DefaultProperty = "KG";
		}

		#endregion

		#region Type

		CodeDescriptionPairList limitTypeList;
		public CodeDescriptionPairList LimitTypeList
		{
			get
			{
				if (limitTypeList == null)
				{
					limitTypeList = new CodeDescriptionPairList();
					limitTypeList.AddPair(ContainerWeightLimitType.AveragePerTEU, Res.GetString("6d73a014-e8a6-431f-a6f2-ff4aff7fa243", "Average Weight per TEU"));
					limitTypeList.AddPair(ContainerWeightLimitType.AbsolutePerTEU, Res.GetString("8cd83b89-f143-49e4-9843-1596462fe26d", "Absolute Weight per TEU"));
				}

				return limitTypeList;
			}
		}

		ZString defaultLimitType;

		public ZString DefaultLimitType
		{
			get
			{
				return defaultLimitType;
			}
			set
			{
				defaultLimitType = value;
				LimitType = value;
			}
		}

		[List("LimitTypeList")]
		public ZString LimitType
		{
			get
			{
				return limitType;
			}
			set
			{
				if (limitType != value)
				{
					InvalidateCachedQuery();
				}

				if (SetNonPersistentPropertyValue(LimitTypeInfo, ref limitType, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateLimitType();
					}

					LimitTypeInfo.RefreshBinding();
				}
			}
		}

		ZString limitType;

		public ZPropertyInfo LimitTypeInfo => GetZPropertyInfo(nameof(LimitType));

		#region Limit Type Query

		protected override ZQuery GetQuery()
		{
			var query = base.GetQuery();
			if (!IsEmpty)
			{
				query.AddToFilter(GetLimitTypeQuery());
			}

			return query;
		}

		ZQuery GetLimitTypeQuery()
		{
			var query = new ZQuery();

			if (LimitType.IsEmpty)
			{
				return query;
			}

			if (!LimitTypeList.ContainsCode(LimitType))
			{
				query.IsNoResultQuery = true;
			}
			else
			{
				query.AddToFilter(RatingContractAllocationLineSchema.RCA_ContainerWeightLimitType, SQLComparisonOperator.Equal, LimitType);
			}

			return query;
		}

		#endregion

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get => false;
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			base.ClearCore();
			Property1 = 0;
			Property2 = 0;
			LimitType = DefaultLimitType;
		}

		protected override bool IsEmptyCore => false; // numbers cannot be empty

		#endregion

		#region Properties

		#region Property1

		public ZDecimal Property1
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property1; }
			set
			{
				if (Property1 != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(Property1Info, ref property1, value);

				if (Property2 == 0)
				{
					Property2 = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateProperty();
				}
			}
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		ZDecimal property1;

		#endregion

		#region Property1Validation

		public Validation Property1Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property1Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { property1Validation = value; }
		}
		Validation property1Validation;

		#endregion

		#region Property2

		public ZDecimal Property2
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property2; }
			set
			{
				if (Property2 != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(Property2Info, ref property2, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateProperty();
				}
			}
		}

		public ZPropertyInfo Property2Info
		{
			get { return GetZPropertyInfo(nameof(Property2)); }
		}

		ZDecimal property2;

		#endregion

		#region Property2Validation

		public Validation Property2Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property2Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { property2Validation = value; }
		}
		Validation property2Validation;

		#endregion

		#region Validation

		public new AllocationContainerWeightLimitWithTypeFilterValidation Validation
		{
			get { return (AllocationContainerWeightLimitWithTypeFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new AllocationContainerWeightLimitWithTypeFilterValidation(this);
		}

		#endregion

		#endregion

		#region Query
		protected override object[] QueryDelegateParameters
		{
			get { return [Property1, Property2, Property]; }
		}

		#endregion

		#region Implementation

		public Control[] GetFilterControl(FilterStrip currentDataItem, ZBindingSource bindingSource)
		{
			using var allocationContainerWeightLimitWithTypeFilterControl = new AllocationContainerWeightLimitWithTypeFilterControl();
			return allocationContainerWeightLimitWithTypeFilterControl.GetFilterControl(bindingSource);
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("Property1", Property1.ToString());
			writer.WriteElementString("Property2", Property2.ToString());
			writer.WriteElementString(nameof(LimitType), LimitType);
			writer.WriteElementString("UnitQuantity", Property);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "Property1")
			{
				string property1AsString = reader.ReadElementString("Property1");
				Property1 = ZDecimal.ParseSafe(property1AsString, 0);
			}

			if (reader.Name == "Property2")
			{
				string property2AsString = reader.ReadElementString("Property2");
				Property2 = ZDecimal.ParseSafe(property2AsString, 0);
			}

			if (reader.Name == nameof(LimitType))
			{
				LimitType = reader.ReadElementString(nameof(LimitType));
			}

			if (reader.Name == "UnitQuantity")
			{
				Property = reader.ReadElementString("UnitQuantity");
			}
		}

		#endregion
	}

	#region class UnitRangeTextFilterValidation

	public class AllocationContainerWeightLimitWithTypeFilterValidation : ModuleTextFilterValidation
	{
		public AllocationContainerWeightLimitWithTypeFilterValidation(AllocationContainerWeightLimitWithTypeFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		#region ValidateProperty1

		public void ValidateProperty1()
		{
			ValidateCalculatedProperty(Parent.Property1Info);
			ValidateCalculatedProperty(Parent.Property2Info);
		}

		protected virtual void CheckProperty1()
		{
			if (Parent.Property1 > Parent.Property2)
			{
				Parent.Property1Info.AddError(Res.GetString("308b691c-1129-4d2a-9eeb-db2b669d6731", "The 'From' value is greater than the 'To' value."));
			}

			if (Parent.Property1Validation != null)
			{
				Parent.Property1Validation(Parent.Property1Info);
			}
		}

		#endregion

		#region ValidateLimitType

		public void ValidateLimitType()
		{
			ValidateCalculatedProperty(Parent.LimitTypeInfo);
		}

		protected virtual void CheckLimitType()
		{
			if (!Parent.LimitTypeList.ContainsCode(Parent.LimitType) && !Parent.LimitType.IsEmpty)
			{
				Parent.LimitTypeInfo.AddError(Res.GetString("59cc4331-ef84-4055-88c1-9493ae15dfff", "The weight limit type is invalid."));
			}
		}

		#endregion

		#region ValidateProperty

		protected override void CheckProperty()
		{
			if (Parent.Units.ContainsCode(Parent.Property))
			{
				return;
			}

			Parent.PropertyInfo.AddError(Res.GetString("9f8b068c-5882-42c4-a51b-f39da14ebf25", "The unit is invalid."));
		}

		#endregion

		#region ValidateProperty2

		public void ValidateProperty2()
		{
			ValidateCalculatedProperty(Parent.Property1Info);
			ValidateCalculatedProperty(Parent.Property2Info);
		}

		protected virtual void CheckProperty2()
		{
			if (Parent.Property2 < Parent.Property1)
			{
				Parent.Property2Info.AddError(Res.GetString("ae67d963-62ef-4d37-b037-4e1fae362cf5", "The 'To' value is less than the 'From' value."));
			}

			if (Parent.Property2Validation != null)
			{
				Parent.Property2Validation(Parent.Property2Info);
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateProperty1();
			ValidateProperty2();
			ValidateProperty();
			ValidateLimitType();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		protected new readonly AllocationContainerWeightLimitWithTypeFilter Parent;
	}

	#endregion
}
