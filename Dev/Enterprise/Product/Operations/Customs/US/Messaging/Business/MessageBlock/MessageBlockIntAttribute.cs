using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public sealed class MessageBlockIntAttribute : MessageBlockAttribute
	{
		public MessageBlockIntAttribute(byte length, byte position, string status)
			: this(length, position, status, status == FieldStatus.Mandatory ? FillType.AlwaysZeroFill : FillType.ZeroFillUnlessEmpty)
		{
		}

		public MessageBlockIntAttribute(byte length, byte position, string status, FillType fillType)
			: base(length, position, status, fillType)
		{
		}

		protected override string SerialiseCore(MessageBlock block, IZType value, bool humanFriendly)
		{
			ZInt valueAsInt = (ZInt)value;
			return humanFriendly ? SerialiseHumanFriendly(block, valueAsInt) : SerialiseNonHumanFriendly(block, valueAsInt);
		}

		string SerialiseNonHumanFriendly(MessageBlock block, ZInt value)
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
				if (result.Length > Length || value < ZInt.Zero)
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

		string SerialiseHumanFriendly(MessageBlock block, ZInt value)
		{
			var fillType = GetFillTypeCore(block);
			string result = (value.IsEmpty && fillType == FillType.ZeroFillUnlessEmpty) ? "" : value.ToString();
			if (result.Length > Length || value < ZInt.Zero)
			{
				result = new string(InvalidPaddingCharacter, Length);
			}
			return result;
		}

		protected override IZType DeSerialiseCore(string value)
		{
			return (value.Length == 0 || value[0] == InvalidPaddingCharacter) ? ZInt.Zero : ZInt.Parse(value);
		}
	}
}
