//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusCodeListAttributeCombinedValidation
//
//    This class should be used for overriding validation in AutoZZRefCusCodeListAttributeCombinedValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusCodeListAttributeCombinedValidation : AutoZZRefCusCodeListAttributeCombinedValidation
	{
		public ZZRefCusCodeListAttributeCombinedValidation(AutoZZRefCusCodeListAttributeCombined parent)
			: base(parent)
		{
		}

		protected new ZZRefCusCodeListAttributeCombined Parent
		{
			get { return (ZZRefCusCodeListAttributeCombined)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCodeListAttributeNamePK();
			CheckBothOrNonDatesArePopulated();
		}

		public void ValidateCodeListAttributeNamePK()
		{
			ValidateCalculatedProperty(Parent.CodeListAttributeNamePKInfo);
		}

		protected void CheckCodeListAttributeNamePK()
		{
			if (!Parent.IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.CodeListAttributeNamePKInfo);
			}
			ValidateDuplicatedAttributes();
			ValidateZZE_Value();
			ValidateTransportModes();
		}

		protected override void CheckZZE_StartDate()
		{
			base.CheckZZE_StartDate();
			var parent = Parent;
			if (parent.ZZE_StartDate > parent.ZZE_EndDate)
			{
				parent.ZZE_StartDateInfo.AddError(ZZRefCusCodeListCombinedValidation.StartDateCannotBeAfterEndDate);
			}
		}

		protected override void CheckZZE_EndDate()
		{
			base.CheckZZE_EndDate();
			ValidateZZE_StartDate();
		}

		protected override void CheckZZE_ZXE_NKName()
		{
			base.CheckZZE_ZXE_NKName();
			ValidateCodeListAttributeNamePK();
		}

		protected override void CheckZZE_Value()
		{
			base.CheckZZE_Value();
			ValidateMandatoryValue();
			if (!Parent.IsSystem)
			{
				ValidateValueRange();
			}
		}

		void ValidateValueRange()
		{
			var value = Parent.ZZE_Value;
			var codeListAttributeName = Parent.CodeListAttributeName;
			if (codeListAttributeName != null)
			{
				var valueIsEmpty = value.IsEmpty;
				var valueDataType = codeListAttributeName.ZXE_ValueDataType.ToUpperInvariant();
				var minLengthOrValue = codeListAttributeName.ZXE_MinLengthOrValue;
				var maxLengthOrValue = codeListAttributeName.ZXE_MaxLengthOrValue;
				var maxLengthOrValueToInt = codeListAttributeName.ZXE_MaxLengthOrValue.ToZInt();
				if (valueDataType.IsEmpty || valueDataType == Constants.RefCusCodeListAttributeName.ValueDataTypes.String || valueDataType == Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean)
				{
					if (!valueIsEmpty)
					{
						var valueList = Parent.Lookups.ValueList;
						if (valueList.Count > 0)
						{
							ListValidation.ErrorIfInvalidCode(Parent.ZZE_ValueInfo, valueList);
						}

						if (value.Length < minLengthOrValue)
						{
							if (maxLengthOrValueToInt > 0)
							{
								Parent.ZZE_ValueInfo.AddError(ValueLengthRangeIsInvalid(minLengthOrValue, maxLengthOrValueToInt));
							}
							else
							{
								Parent.ZZE_ValueInfo.AddError(ValueLengthIsLessThanMinLength(minLengthOrValue));
							}
						}
						else if (maxLengthOrValueToInt > 0 && value.Length > maxLengthOrValueToInt)
						{
							Parent.ZZE_ValueInfo.AddError(ValueLengthRangeIsInvalid(minLengthOrValue, maxLengthOrValueToInt));
						}
					}
				}
				else
				{
					var valueDataTypeIsDecimal = valueDataType == Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal;
					if (valueDataTypeIsDecimal || valueDataType == Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer)
					{
						var valueCanParse = ZDecimal.CanParse(value);
						if (valueIsEmpty || valueCanParse)
						{
							var valueToDecimal = valueCanParse ? Convert.ToDecimal(value, Culture.CurrentCompanyCountryCulture) : decimal.Zero;
							if (valueToDecimal < minLengthOrValue)
							{
								if (maxLengthOrValue > 0)
								{
									var maxValueToINumeric = valueDataTypeIsDecimal ? (INumericZType)maxLengthOrValue : maxLengthOrValueToInt;
									Parent.ZZE_ValueInfo.AddError(ValueRangeIsInvalid(minLengthOrValue, maxValueToINumeric, codeListAttributeName.ZXE_DecimalPlaces));
								}
								else
								{
									Parent.ZZE_ValueInfo.AddError(ValueIsLessThanMinValue(minLengthOrValue, codeListAttributeName.ZXE_DecimalPlaces));
								}
							}
							else if (maxLengthOrValue > 0 && valueToDecimal > maxLengthOrValue)
							{
								var maxValueToINumeric = valueDataTypeIsDecimal ? (INumericZType)maxLengthOrValue : maxLengthOrValueToInt;
								Parent.ZZE_ValueInfo.AddError(ValueRangeIsInvalid(minLengthOrValue, maxValueToINumeric, codeListAttributeName.ZXE_DecimalPlaces));
							}
						}
					}
				}
			}
		}

		internal static string ValueLengthIsLessThanMinLength(ZShort minLength)
		{
			return Res.GetString("8D63B689-D059-4AC5-B6DB-183FAABFEBE7", "Value length must be equal or greater than {0}.", minLength);
		}

		internal static string ValueLengthRangeIsInvalid(ZShort minLength, ZInt maxLength)
		{
			return Res.GetString("2551F964-33CA-4E86-905D-8CE96F48483A", "Value length must be between {0} and {1}.", minLength, maxLength);
		}

		internal static string ValueIsLessThanMinValue(ZShort minValue, byte decimalPlaces)
		{
			return Res.GetString("44CE191C-8BF1-4AE7-867D-C534528691EB", "Number must be equal or greater than {0}.", NumericUtil.ToStringWithDecimalPlaces(minValue, decimalPlaces));
		}

		internal static string ValueRangeIsInvalid(ZShort minValue, INumericZType maxValue, byte decimalPlaces)
		{
			return Res.GetString("1FE54525-6C48-49EE-AD5A-7676B6D0206C", "Number must be between {0} and {1}.",
				NumericUtil.ToStringWithDecimalPlaces(minValue, decimalPlaces), NumericUtil.ToStringWithDecimalPlaces(maxValue, decimalPlaces));
		}

		void ValidateDuplicatedAttributes()
		{
			var codeListAttributeName = Parent.CodeListAttributeName;
			if (codeListAttributeName != null && !codeListAttributeName.ZXE_AllowDuplicates)
			{
				if (Parent.CodeList.Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Any(y => y.ZZE_ZXE_NKName.EqualsIgnoringCase(Parent.ZZE_ZXE_NKName) && y.PK != Parent.PK))
				{
					Parent.CodeListAttributeNamePKInfo.AddError(DuplicateAttributeNameNotAllowed(codeListAttributeName.TranslatedName));
				}
			}
		}

		internal static string DuplicateAttributeNameNotAllowed(string attributeName)
		{
			return Res.GetString("{F97A5065-AC1A-45AE-9C85-E5771AD46330}", "Duplicate attribute name '{0}' is not allowed.", attributeName);
		}

		void ValidateMandatoryValue()
		{
			if (Parent.ZZE_Value.IsEmpty)
			{
				var codeListAttributeName = Parent.CodeListAttributeName;
				if (codeListAttributeName != null && codeListAttributeName.ZXE_IsValueMandatory)
				{
					var valueDataType = codeListAttributeName.ZXE_ValueDataType.ToUpperInvariant();
					var needMandatoryCheck = !(valueDataType == Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean
						|| valueDataType == Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer
						|| valueDataType == Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal);
					if (needMandatoryCheck)
					{
						Parent.ZZE_ValueInfo.AddError(MandatoryValueRequired(codeListAttributeName.TranslatedName));
					}
				}
			}
		}

		internal static string MandatoryValueRequired(string attributeName)
		{
			return Res.GetString("{1ECA62AB-B4DF-4ECD-B951-6D761F4E9EC7}", "Value is mandatory for attribute {0}.", attributeName);
		}

		#region Validate Transport Modes

		void ValidateTransportModes()
		{
			ValidateZZE_IsAir();
			ValidateZZE_IsFix();
			ValidateZZE_IsInw();
			ValidateZZE_IsMai();
			ValidateZZE_IsRai();
			ValidateZZE_IsRoa();
			ValidateZZE_IsSea();
		}

		protected override void CheckZZE_IsAir()
		{
			base.CheckZZE_IsAir();
			EnsureSupportTransportModes(Parent.ZZE_IsAirInfo);
		}

		protected override void CheckZZE_IsFix()
		{
			base.CheckZZE_IsFix();
			EnsureSupportTransportModes(Parent.ZZE_IsFixInfo);
		}

		protected override void CheckZZE_IsInw()
		{
			base.CheckZZE_IsInw();
			EnsureSupportTransportModes(Parent.ZZE_IsInwInfo);
		}

		protected override void CheckZZE_IsMai()
		{
			base.CheckZZE_IsMai();
			EnsureSupportTransportModes(Parent.ZZE_IsMaiInfo);
		}

		protected override void CheckZZE_IsRai()
		{
			base.CheckZZE_IsRai();
			EnsureSupportTransportModes(Parent.ZZE_IsRaiInfo);
		}

		protected override void CheckZZE_IsRoa()
		{
			base.CheckZZE_IsRoa();
			EnsureSupportTransportModes(Parent.ZZE_IsRoaInfo);
		}

		protected override void CheckZZE_IsSea()
		{
			base.CheckZZE_IsSea();
			EnsureSupportTransportModes(Parent.ZZE_IsSeaInfo);
		}

		void EnsureSupportTransportModes(ZPropertyInfo propertyInfo)
		{
			var codeList = Parent.CodeList;
			if (codeList != null)
			{
				var codeType = codeList.ZZD_CodeType;
				var country = codeList.ZZD_CountryOrGrouping;
				var attributeName = Parent.ZZE_ZXE_NKName;

				if (!Parent.IsSystem && (ZBool)propertyInfo.Value
					&& !RefTransportModesHelper.ExistsTransportModesForThatAttributeNameInZZDatabase(Parent.Factory, codeType, country, attributeName))
				{
					propertyInfo.AddError(DoesNotSupportTransportModes(codeType, country, Parent.CodeListAttributeName?.TranslatedName ?? attributeName));
				}
			}
		}

		internal static string DoesNotSupportTransportModes(ZString codeType, ZString country, ZString attributeName)
		{
			return Res.GetString("73bdd7db-0db9-4d99-b01c-1818d0dd6fdd", "Attribute Name {0}/{1}/{2} does not support Transport Modes.", attributeName, codeType, country);
		}

		#endregion

		internal string BothOrNonDatesErrorMessage => Parent.Factory.GetCachedValue("4D0AB883-491B-4A1E-A966-D733E6E9028C", () => Res.GetString("74E43955-EEF2-4DB5-B566-FBABD7D0C0CA", "Both or none of the dates (Start Date, End Date) must be populated."));

		void CheckBothOrNonDatesArePopulated()
		{
			var parent = Parent;
			var errorMessage = BothOrNonDatesErrorMessage;
			parent.RemoveRowError(errorMessage);
			if (!parent.RowErrors.Contains(new RefCusCodeListMultipleIdenticalAttributeValueRecordsValidator(parent.CodeList).MultipleIdenticalAttributeValueRecordsHaveStartAndEndDateRowErrorMessage) && (parent.ZZE_StartDate.IsEmpty ^ parent.ZZE_EndDate.IsEmpty))
			{
				parent.AddRowError(errorMessage);
			}
		}
	}
}
