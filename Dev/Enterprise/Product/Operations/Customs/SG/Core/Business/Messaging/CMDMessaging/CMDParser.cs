using System.Text;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDParser : MessageParser
	{
		public CMDParser(ZString messageText)
			: base(messageText)
		{
			if (IsValid)
			{
				IsLate = (MessageLines[1][4] == 'Y');
				fActionCode = DefaultActionCode;
			}
		}

		public CMD.ActionCodes ActionCode
		{
			get { return fActionCode; }
			set
			{
				if (fActionCode != value)
				{
					fActionCode = value;
					ClearModifiedMessageTextCache();
				}
			}
		}

		public ZString ModifiedMessageText
		{
			get
			{
				if (fModifiedMessageText.IsEmpty)
				{
					fModifiedMessageText = GetModifiedMessageText();
				}
				return fModifiedMessageText;
			}
		}

		public readonly bool IsLate;

		void ClearModifiedMessageTextCache()
		{
			fModifiedMessageText = "";
		}

		#region Implementation

		public override bool IsValid
		{
			get
			{
				return
					(StandardMessageIdentifier == EDIMessage.ApplicationCodes.SingaporeCMD &&
					MessageLines.Length > 1 && MessageLines[1].Length >= 5);
			}
		}

		ZString GetModifiedMessageText()
		{
			StringBuilder builder = new StringBuilder(MessageLines[0], MessageLines.Length);
			builder.Append("\r\n");
			builder.Append(char.ToUpper(ActionCode.ToString()[0]));
			builder.Append(MessageLines[1].SubstringSafe(1));
			builder.Append("\r\n");
			builder.Append(ZString.Join("\r\n", MessageLines, 2, MessageLines.Length - 2));

			return builder.ToString();
		}

		CMD.ActionCodes DefaultActionCode
		{
			get
			{
				CMD.ActionCodes result = CMD.ActionCodes.Add;
				switch (MessageLines[1][0])
				{
					case 'M':
						result = CMD.ActionCodes.Modify;
						break;
					case 'D':
						result = CMD.ActionCodes.Delete;
						break;
				}
				return result;
			}
		}

		CMD.ActionCodes fActionCode;
		ZString fModifiedMessageText;

		#endregion
	}
}
