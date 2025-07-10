using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business.AWB;
using static System.FormattableString;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class AWBMessageBlockDateAttribute : AWBMessageBlockAttribute
	{
		public AWBMessageBlockDateAttribute(byte position, StatusType status, CharType charType)
			: this(position, status, charType, "MMddyy")
		{
		}

		public AWBMessageBlockDateAttribute(byte position, StatusType status, CharType charType, string dateFormat)
			: base(position, (byte)dateFormat.Length, (byte)dateFormat.Length, status, charType)
		{
			DateFormat = dateFormat;
		}

		public string DateFormat { get; private set; }

		#region Implementation

		protected override ZString SerialiseCore(IZType value)
		{
			var date = (ZDate)value;
			return date.IsEmpty ?
				(Status == StatusType.Mandatory ? InvalidValue : ZString.Empty) :
				(date.IsValid ? (ZString)date.ToString(DateFormat, CultureInfo.InvariantCulture).ToUpperInvariant() : InvalidValue);
		}

		protected override IZType DeSerialiseCore(ZString value)
		{
			ZDate result;
			if (value == new string('9', DateFormat.Length))
			{
				result = new ZDate(2099, 12, 31);
			}
			else if (IsEmpty(value))
			{
				result = ZDate.Empty;
			}
			else if (IsValid(value))
			{
				ZDateTime dateTimeResult;
				if (ZDateTime.TryParseExact(value, out dateTimeResult, DateFormat))
				{
					result = dateTimeResult.Date;
				}
				else
				{
					throw new ArgumentException(Invariant($"'{value}' is not a valid {DateFormat} format"), nameof(value));
				}
			}
			else
			{
				result = ZDate.Invalid;
			}

			return result;
		}

		bool IsEmpty(string serializedDate)
		{
			return serializedDate.All(c => c == ZeroPaddingCharacter || c == SpacePaddingCharacter);
		}

		#endregion
	}
}
