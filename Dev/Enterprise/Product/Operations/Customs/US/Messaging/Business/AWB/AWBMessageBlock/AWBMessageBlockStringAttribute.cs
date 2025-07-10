using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Messaging.Business.AWB;
using static System.FormattableString;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB
{
	public enum LengthViolationAction
	{
		SetInvalidValue,
		SetInvalidValueAndThrowException,
		Substring,
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class AWBMessageBlockStringAttribute : AWBMessageBlockAttribute
	{
		public AWBMessageBlockStringAttribute(byte position, byte minLength, byte maxLength, StatusType status, CharType charType)
			: base(position, minLength, maxLength, status, charType)
		{
			OnLengthViolation = LengthViolationAction.SetInvalidValueAndThrowException;
		}

		public LengthViolationAction OnLengthViolation
		{
			get;
			set;
		}

		protected override ZString SerialiseCore(IZType value)
		{
			ZString result = value.ToString().ToUpper(CultureInfo.CurrentCulture);
			result = TrimInvalidCharacters(result.RemoveDiacritics());

			if (result.Length < MinLength)
			{
				if (Status == StatusType.Mandatory || !result.IsEmpty)
				{
					result = result.PadLeft(MinLength, InvalidPaddingCharacter);
				}
			}
			else if (result.Length > MaxLength)
			{
				switch (OnLengthViolation)
				{
					case LengthViolationAction.SetInvalidValue:
						result = InvalidValue;
						break;
					case LengthViolationAction.Substring:
						result = new ZString(result).Left(MaxLength);
						break;
					case LengthViolationAction.SetInvalidValueAndThrowException:
					default:
						throw new MessageBlockSerialisationException(Invariant($"Data provided exceeded allowable maximum length:{System.Environment.NewLine}Maximum Length:{MaxLength}{System.Environment.NewLine}Actual Length:{result.Length}"), InvalidValue);
				}
			}

			return result;
		}

		ZString TrimInvalidCharacters(ZString value)
		{
			var validCharacterSets = "";
			switch (CharType)
			{
				case CharType.Alpha:
					validCharacterSets = ValueElement.CharTypes.Alpha;
					break;
				case CharType.Numeric:
					validCharacterSets = ValueElement.CharTypes.Numeric;
					break;
				case CharType.AlphaNumeric:
					validCharacterSets = ValueElement.CharTypes.AlphaNumeric;
					break;
				case CharType.NumericWithDecimal:
					validCharacterSets = ValueElement.CharTypes.NumericWithDecimal;
					break;
				case CharType.Text:
					validCharacterSets = ValueElement.CharTypes.Text;
					break;
				case CharType.Special:
					return value.Replace(SpecialChars.Slant, " ");
			}
			return value.KeepChars(validCharacterSets);
		}

		protected override IZType DeSerialiseCore(ZString value)
		{
			return TrimInvalidCharacters(value).Left(MaxLength);
		}
	}
}
