using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.ZA.Business
{
	public partial class MessageSubTypeCodes
	{
		public static MessageSubTypes TranslateToMessageSubType(string code)
		{
			switch (code)
			{
				case Codes.Original:
					return MessageSubTypes.Create;
				case Codes.Change:
					return MessageSubTypes.Change;
				case Codes.Cancellation:
					return MessageSubTypes.Withdraw;
				case Codes.Replace:
					return MessageSubTypes.Replace;
				default:
					return MessageSubTypes.Undefined;
			}
		}
	}
}
