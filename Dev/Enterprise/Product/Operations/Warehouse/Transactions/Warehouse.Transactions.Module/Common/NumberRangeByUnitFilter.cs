using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public delegate ZQuery GetUnitRangeQuery(INumericZType value1, INumericZType value2, ZString unit);

	public class NumberRangeByUnitFilter : ModuleTextFilter, INumberRangeByUnitFilter
	{
		#region Construction

		public NumberRangeByUnitFilter(ZString description, GetUnitRangeQuery queryDelegate, IList unitList)
			: base(description, queryDelegate, unitList)
		{
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			base.ClearCore();
			this.Property1 = 0;
			this.Property2 = 0;
		}

		protected override bool IsEmptyCore => false; // numbers cannot be empty

		#endregion

		#region Properties

		#region Property1

		public ZDecimal Property1
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fProperty1; }
			set
			{
				if (Property1 != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(Property1Info, ref fProperty1, value);

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

		ZDecimal fProperty1;

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
			get { return fProperty2; }
			set
			{
				if (Property2 != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(Property2Info, ref fProperty2, value);

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

		ZDecimal fProperty2;

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

		public new NumberRangeByUnitFilterValidation Validation
		{
			get { return (NumberRangeByUnitFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new NumberRangeByUnitFilterValidation(this);
		}

		#endregion

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Property1, Property2, Property }; }
		}

		#endregion

		#region Implementation

		public Control[] GetFilterControl(FilterStrip currentDataItem, Control control, ZBindingSource bindingSource)
		{
			using (var numberRangeByUnitFilterControl = new NumberRangeByUnitFilterControl())
			{
				return numberRangeByUnitFilterControl.GetFilterControl(control, bindingSource);
			}
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("Property1", Property1.ToString());
			writer.WriteElementString("Property2", Property2.ToString());
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
		}

		#endregion				
	}

	#region class UnitRangeTextFilterValidation

	public class NumberRangeByUnitFilterValidation : ModuleTextFilterValidation
	{
		public NumberRangeByUnitFilterValidation(NumberRangeByUnitFilter parent)
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
				Parent.Property1Info.AddError(Res.GetString("0dc4e2c9-86e4-4b5f-9ef8-3d62af4dcecf", "The 'From' value is greater than the 'To' value."));
			}

			if (Parent.Property1Validation != null)
			{
				Parent.Property1Validation(Parent.Property1Info);
			}
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
				Parent.Property2Info.AddError(Res.GetString("a75a725f-0eaf-4e22-89de-d60006b81757", "The 'To' value is less than the 'From' value."));
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
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		protected new readonly NumberRangeByUnitFilter Parent;
	}

	#endregion
}
