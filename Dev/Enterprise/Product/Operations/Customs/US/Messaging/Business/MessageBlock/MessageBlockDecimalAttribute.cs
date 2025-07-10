using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	public sealed class MessageBlockDecimalAttribute : MessageBlockAttribute
	{
		public MessageBlockDecimalAttribute(byte length, byte position, string status, byte impliedDecimalPlaces, FillType fillType, bool supportNegative = false)
			: base(length, position, status, fillType)
		{
			ImpliedDecimalPlaces = impliedDecimalPlaces;
			this.supportNegative = supportNegative;
		}

		public MessageBlockDecimalAttribute(byte length, byte position, string status, byte impliedDecimalPlaces, string fillTypePropertyName, bool supportNegative = false)
			: base(length, position, status, fillTypePropertyName)
		{
			ImpliedDecimalPlaces = impliedDecimalPlaces;
			this.supportNegative = supportNegative;
		}

		public MessageBlockDecimalAttribute(byte length, byte position, string status, byte impliedDecimalPlaces, bool supportNegative = false)
			: this(length, position, status, impliedDecimalPlaces, status == FieldStatus.Mandatory ? FillType.AlwaysZeroFill : FillType.ZeroFillUnlessEmpty, supportNegative)
		{
		}

		const string MinusSymbol = "-";
		readonly bool supportNegative;
		public readonly int ImpliedDecimalPlaces;
		readonly static ImmutableArray<int> DecimalPowers = ImmutableArray.Create(1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000);

		protected override string SerialiseCore(MessageBlock block, IZType value, bool humanFriendly)
		{
			ZDecimal valueAsDecimal = (ZDecimal)value;
			return humanFriendly ? SerialiseHumanFriendly(block, valueAsDecimal) : SerialiseNonHumanFriendly(block, valueAsDecimal);
		}

		string SerialiseNonHumanFriendly(MessageBlock block, ZDecimal value)
		{
			string result;

			var fillType = GetFillTypeCore(block);

			if (value.IsEmpty && fillType == FillType.ZeroFillUnlessEmpty)
			{
				result = new string(SpacePaddingCharacter, Length);
			}
			else
			{
				ZDecimal messageValue = value * DecimalPowers[ImpliedDecimalPlaces];
				result = messageValue.ToString(0);
				if (messageValue != messageValue.Truncate() ||
					!messageValue.IsWithinSqlPrecisionAndScale(Length, 0) ||
					result.Length > Length ||
					(!supportNegative && messageValue < ZInt.Zero))
				{
					result = new string(InvalidPaddingCharacter, Length);
				}
				else if (result.Length < Length)
				{
					var paddingChars = GetPaddingCharacter(fillType);

					if (messageValue < ZInt.Zero)
					{
						result = result.Replace(MinusSymbol, MinusSymbol.PadRight(Length - result.Length + 1, paddingChars));
					}
					else
					{
						result = result.PadLeft(Length, paddingChars);
					}
				}
			}
			return result;
		}

		string SerialiseHumanFriendly(MessageBlock block, ZDecimal value)
		{
			string result;
			var fillType = GetFillTypeCore(block);

			if (value.IsEmpty && fillType == FillType.ZeroFillUnlessEmpty)
			{
				result = "";
			}
			else
			{
				result = value.ToString(ImpliedDecimalPlaces);
				ZDecimal checkValue = value * DecimalPowers[ImpliedDecimalPlaces];
				if (checkValue != checkValue.Truncate() ||
					!checkValue.IsWithinSqlPrecisionAndScale(Length, 0) ||
					checkValue.ToString(0).Length > Length ||
					(!supportNegative && checkValue < ZDecimal.Zero))
				{
					result = new string(InvalidPaddingCharacter, Length);
				}
			}
			return result;
		}

		protected override IZType DeSerialiseCore(string value)
		{
			if (value.Length == 0 || value[0] == InvalidPaddingCharacter)
			{
				return ZDecimal.Zero;
			}
			else
			{
				value = value.Trim();
				if (supportNegative && value.Contains(MinusSymbol))
				{
					value = MinusSymbol + value.Replace(MinusSymbol, string.Empty);
					if (!Regex.IsMatch(value, @"^-\d*$"))
					{
						return ZDecimal.Zero;
					}
				}

				var objResult = ZDecimalTypeConverter.Instance.ConvertFromString(value);
				var result = (ZDecimal)objResult;
				if (ImpliedDecimalPlaces > 0)
				{
					result = new ZDecimal(result / DecimalPowers[ImpliedDecimalPlaces]);
				}
				return result;
			}
		}
	}
}
