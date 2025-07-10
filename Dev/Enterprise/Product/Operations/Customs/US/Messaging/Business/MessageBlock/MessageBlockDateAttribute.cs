using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public sealed class MessageBlockDateAttribute : MessageBlockAttribute
	{
		public MessageBlockDateAttribute(byte position, string status)
			: this(position, status, "MMddyy")
		{
		}

		public MessageBlockDateAttribute(byte position, string status, string dateFormat)
			: base((byte)dateFormat.Length, position, status)
		{
			this.dateFormat = dateFormat;
		}

		readonly string dateFormat;

		public const string JulianDateFormat = "yyddd";

		#region Implementation

		bool IsEmpty(string serializedDate)
		{
			return serializedDate.All(c => c == ZeroPaddingCharacter || c == SpacePaddingCharacter);
		}

		bool IsInvalid(string serializedDate)
		{
			return serializedDate.Length > 0 &&
				   serializedDate.All(c => c == InvalidPaddingCharacter);
		}

		protected override string SerialiseCore(MessageBlock block, IZType value, bool humanFriendly)
		{
			ZDate date = (ZDate)value;
			if (!date.IsEmpty && !date.IsValid)
			{
				return new string(InvalidPaddingCharacter, Length);
			}
			else if (humanFriendly)
			{
				return date.ToShortDateString();
			}
			else if (dateFormat == JulianDateFormat)
			{
				return date.ToJulianDateString().PadRight(Length);
			}
			else
			{
				return date.ToString(dateFormat).PadRight(Length);
			}
		}

		protected override IZType DeSerialiseCore(string value)
		{
			ZDate result;
			if (value == new string('9', Length))
			{
				result = new ZDate(2099, 12, 31);
			}
			else if (IsEmpty(value))
			{
				result = ZDate.Empty;
			}
			else if (IsInvalid(value))
			{
				result = ZDate.Invalid;
			}
			else if (dateFormat == JulianDateFormat)
			{
				if (!ZDate.TryParseJulianDate(value, out result))
				{
					throw new ArgumentException("'" + value + "' is not a valid Julian Date format", nameof(value));
				}
			}
			else
			{
				ZDateTime dateTimeResult;
				if (ZDateTime.TryParseExact(value, out dateTimeResult, dateFormat))
				{
					result = dateTimeResult.Date;
				}
				else
				{
					throw new ArgumentException("'" + value + "' is not a valid " + dateFormat + " format", nameof(value));
				}
			}

			return result;
		}

		#endregion
	}
}
