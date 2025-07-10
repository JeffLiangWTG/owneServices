using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// A CalculatorPropertyAttribute provides a mechanism whereby a Calculator
	/// can store properties of itself in different RateLineItem rows in the
	/// database.
	///
	/// A RateLine refers to a Calculator that is used to calculate the price
	/// of the charge that the RateLine will have. However, the database has no
	/// Calculator table. Instead, it has RateLineItems.
	///
	/// A Calculator can store its information in individual RateLineItems or in
	/// a series of RateLineItems, or a mix of both. The CalculatorPropertyAttribute
	/// provides the functionality for a Calculator to store information in
	/// individual RateLineItems.
	///
	/// For example. A Calculator wishing to store a boolean value may choose
	/// to store that in a RateLineItem whose TM_Type is XYZ and the value itself
	/// is stored as a number in TM_Value.
	///
	/// That will have a CalculatorPropertyAttribute with
	/// - ItemType = XYZ,
	/// - FieldName = TM_Value
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class CalculatorPropertyAttribute : Attribute
	{
		public CalculatorPropertyAttribute(string fieldName)
			: this("", fieldName)
		{
		}

		public CalculatorPropertyAttribute(string itemType, string fieldName)
		{
			this.ItemType = itemType;
			this.FieldName = fieldName;
		}

		#region Attribute Properties

		public readonly string ItemType;
		public readonly string FieldName;

		/// <summary>
		/// If true, it will access the {FieldName} value via a property on the
		/// calculator instead of accessing it directly via the RateLineItem
		/// column indexing.
		/// 
		/// It will expect a property with the name "{FieldName}" and "{FieldName}"Info
		/// to exist on the calculator object.
		/// </summary>
		public bool IsCalculatedField;
		public bool IsBool;
		public bool IsMandatory;
		public bool ShowWarningIfEmpty = true;

		/// <summary>
		/// MapTo is the property in the Calculator object that will represent
		/// the value of this CalculatorProperty for binding purposes.
		/// </summary>
		public string MapTo = "";
		public object InitialValue;
		public bool DefaultBooleanValue;
		public string RelatedTo = string.Empty;

		#endregion

		#region Get Value

		internal IZType GetValue(Calculator calculator, decimal @break)
		{
			if (IsCalculatedField)
			{
				return (IZType)calculator.GetType().GetProperty(FieldName).GetValue(calculator, null);
			}
			else
			{
				var bizObj = GetBusinessObject(calculator, @break, false);
				if (bizObj != null)
				{
					if (PropertyType == typeof(ZBool) || MapTo.StartsWith((NoResString)"Bool") || IsBool) // Hard-coded header constant
					{
						var valueString = (ZString)bizObj[FieldName];
						return valueString.IsEmpty ? (ZBool)DefaultBooleanValue : (ZBool)(valueString == "Y");
					}
					else if (PropertyType == typeof(ZInt) || MapTo.StartsWith((NoResString)"Int")) // Hard-coded header constant
					{
						var result = (bizObj[FieldName] as ZDecimal?);
						if (result.HasValue)
						{
							return (ZInt)result.Value;
						}
						else
						{
							var rateLine = bizObj as RateLineItem;

							if (rateLine != null)
							{
								rateLine.Validation.ValidateTM_Value();
							}

							return (ZInt)int.MinValue;
						}
					}
					else
					{
						return (IZType)bizObj[FieldName];
					}
				}
				else
				{
					if (PropertyType == typeof(ZInt) || MapTo.StartsWith((NoResString)"Int")) // Hard-coded constant
					{
						return (ZInt)0;
					}
					else if (PropertyType == typeof(ZDecimal))
					{
						return (ZDecimal)0m;
					}
					else if (PropertyType == typeof(ZBool) || MapTo.StartsWith((NoResString)"Bool") || IsBool) // Hard-coded constant
					{
						return (ZBool)DefaultBooleanValue;
					}
					else if (PropertyType == typeof(ZString))
					{
						return ZString.Empty;
					}
					else if (PropertyType == typeof(ZGuid))
					{
						return ZGuid.Empty;
					}
					else
					{
						return null;
					}
				}
			}
		}

		#endregion

		#region Set Value

		internal void SetValue(Calculator calculator, decimal @break, IZType value)
		{
			if (IsCalculatedField)
			{
				calculator.GetType().GetProperty(FieldName).SetValue(calculator, value, null);
			}
			else
			{
				var bizObj = GetBusinessObject(calculator, @break, true);
				if (bizObj != null)
				{
					if (value is ZBool)
					{
						bizObj[FieldName] = (ZString)((ZBool)value ? "Y" : "N");
					}
					else
					{
						bizObj[FieldName] = value;
					}
				}
			}
		}

		#endregion

		#region Get Property Info

		internal ZPropertyInfo GetMapToInfo(Calculator calculator)
		{
			if (IsCalculatedField)
			{
				return (ZPropertyInfo)calculator.GetType().GetProperty(FieldName + "Info").GetValue(calculator, null);
			}
			else
			{
				var bizObj = GetBusinessObject(calculator, 0m, false);
				if (bizObj != null)
				{
					var result = ((IGetZPropertyInfo)bizObj).GetZPropertyInfo(FieldName);
					return result;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region Related Business Object

		IIndexer GetBusinessObject(Calculator calculator, decimal @break, bool createIfNotExist)
		{
			if (FieldName.StartsWith(RateLinesSchema.Constants.TL_TI.Substring(0, 3)))
			{
				return calculator.Line;
			}
			else if (createIfNotExist)
			{
				return calculator.FindOrAddRateLineItem(this, @break);
			}
			else
			{
				return calculator.FindRateLineItem(ItemType, @break);
			}
		}

		Type BusinessObjectType
		{
			get
			{
				if (IsCalculatedField)
				{
					return GetType();
				}
				else if (FieldName.StartsWith(RateLinesSchema.Constants.TL_TI.Substring(0, 3)))
				{
					return typeof(RateLine);
				}
				else
				{
					return typeof(RateLineItem);
				}
			}
		}

		Type PropertyType
		{
			get { return BusinessObjectType.GetProperty(FieldName).PropertyType; }
		}

		#endregion
	}

	#region IGetZPropertyInfo Interface

	public interface IGetZPropertyInfo
	{
		ZPropertyInfo GetZPropertyInfo(string propertyName);
	}

	#endregion
}

