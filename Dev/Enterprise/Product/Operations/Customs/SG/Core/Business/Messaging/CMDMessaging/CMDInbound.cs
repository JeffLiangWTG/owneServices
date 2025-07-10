using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDInbound : MessageParser
	{
		public static class Constants
		{
			public const string CMA = "CMA";
			public const string FNA = "FNA";
			public const string FMA = "FMA";
			public const string UNK = "UNK";
		}

		public CMDInbound(ZString messageText)
			: base(messageText)
		{
			if (IsValid)
			{
				Sender = MessageLines[1].Trim();
			}
		}

		public readonly ZString Sender;

		public ZString MessageTextWithoutRoutingInfo
		{
			get
			{
				if (fMessageTextWithoutRoutingInfo.IsEmpty)
				{
					fMessageTextWithoutRoutingInfo = ZString.Join("\r\n", MessageLines, 2, MessageLines.Length - 2).TrimEnd((char)4);
				}
				return fMessageTextWithoutRoutingInfo;
			}
		}
		ZString fMessageTextWithoutRoutingInfo;

		public bool IsErrorMessage => StandardMessageIdentifier == Constants.FNA;

		public override bool IsValid => MessageLines.Length > 2 && validMessageIdentifiers.Contains(StandardMessageIdentifier.ToString());

		readonly string[] validMessageIdentifiers = new string[] { Constants.CMA, Constants.FNA, Constants.FMA };
	}
}
