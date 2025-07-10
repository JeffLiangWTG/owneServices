using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public sealed class MessageBlockShortAttribute : MessageBlockAttribute
	{
		public MessageBlockShortAttribute(byte length, byte position, string status)
			: this(length, position, status, status == FieldStatus.Mandatory ? FillType.AlwaysZeroFill : FillType.ZeroFillUnlessEmpty)
		{
		}

		public MessageBlockShortAttribute(byte length, byte position, string status, FillType fillType)
			: base(length, position, status, fillType)
		{
		}

		protected override string SerialiseCore(MessageBlock block, IZType value, bool humanFriendly)
		{
			ZShort valueAsShort = (ZShort)value;
			return humanFriendly ? SerialiseHumanFriendly(block, valueAsShort) : SerialiseNonHumanFriendly(block, valueAsShort);
		}

		string SerialiseNonHumanFriendly(MessageBlock block, ZShort value)
		{
			string result;

			var fillType = GetFillTypeCore(block);

			if (value.IsEmpty && fillType == FillType.ZeroFillUnlessEmpty)
			{
				result = new string(SpacePaddingCharacter, Length);
			}
			else
			{
				result = value.ToString();
				if (result.Length > Length || value < ZShort.Zero)
				{
					result = new string(InvalidPaddingCharacter, Length);
				}
				else if (result.Length < Length)
				{
					var paddingChars = GetPaddingCharacter(fillType);
					result = result.PadLeft(Length, paddingChars);
				}
			}
			return result;
		}

		string SerialiseHumanFriendly(MessageBlock block, ZShort value)
		{
			var fillType = GetFillTypeCore(block);
			string result = (value.IsEmpty && fillType == FillType.ZeroFillUnlessEmpty) ? "" : value.ToString();
			if (result.Length > Length || value < ZShort.Zero)
			{
				result = new string(InvalidPaddingCharacter, Length);
			}
			return result;
		}

		protected override IZType DeSerialiseCore(string value)
		{
			return (value.Length == 0 || value[0] == InvalidPaddingCharacter) ? ZShort.Zero : new ZShort(short.Parse(value));
		}
	}
}
