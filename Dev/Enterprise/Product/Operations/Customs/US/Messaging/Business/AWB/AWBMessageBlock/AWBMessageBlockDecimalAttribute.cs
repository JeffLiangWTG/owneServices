using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Messaging.Business.AWB;
using static System.FormattableString;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class AWBMessageBlockDecimalAttribute : AWBMessageBlockAttribute
	{
		public AWBMessageBlockDecimalAttribute(byte position, byte minLength, byte maxLength, StatusType status, CharType charType)
			: base(position, minLength, maxLength, status, charType)
		{
			if (CharType != CharType.Numeric && CharType != CharType.NumericWithDecimal)
			{
				throw new ArgumentOutOfRangeException(nameof(charType), Invariant($"charType must be either {CharType.Numeric} or {CharType.NumericWithDecimal}."));
			}
		}

		protected override ZString SerialiseCore(IZType value)
		{
			var valueAsDecimal = (ZDecimal)value;
			ZString result = valueAsDecimal.IsEmpty && Status != StatusType.Mandatory ? string.Empty : valueAsDecimal.ToString();
			if (result.IndexOf('.') > -1)
			{
				result = result.TrimEnd(new char[] { '0' });
				result = result.TrimEnd(new char[] { '.' });
			}
			if (result.Length < MinLength)
			{
				if (Status == StatusType.Mandatory || !result.IsEmpty)
				{
					result = result.PadLeft(MinLength, InvalidPaddingCharacter);
				}
			}
			else if (result.Length > MaxLength || valueAsDecimal < ZDecimal.Zero || (result.IndexOf('.') > -1 && CharType == CharType.Numeric))
			{
				result = InvalidValue;
			}
			return result;
		}

		protected override IZType DeSerialiseCore(ZString value)
		{
			if (value.Length == 0 || value[0] == InvalidPaddingCharacter)
			{
				return ZDecimal.Zero;
			}
			else
			{
				object objResult = ZDecimalTypeConverter.Instance.ConvertFrom(null, CultureInfo.InvariantCulture, value);
				return (ZDecimal)objResult;
			}
		}
	}
}
