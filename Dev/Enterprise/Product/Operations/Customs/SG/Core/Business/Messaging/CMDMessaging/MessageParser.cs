using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public abstract class MessageParser
	{
		public MessageParser(ZString messageText)
		{
			MessageText = messageText;
			MessageLines = messageText.Replace("\r\n", "\n").Split('\n');
			var lengthofFirstLine = MessageLines[0].Length;
			StandardMessageIdentifier = (lengthofFirstLine == 3 || lengthofFirstLine == 5)
										? messageText.Left(3)  // e.g. CMD/2 (outbound)
										: messageText.SubstringSafe(3, 3);  // e.g. CMDFNA-->FNA (inbound)
		}

		public ZString MasterBillNumber
		{
			get
			{
				if (IsValid && fMasterBillNumber.IsEmpty)
				{
					fMasterBillNumber = GetMasterBillNumber();
				}
				return fMasterBillNumber;
			}
		}

		public ZString HouseBillNumber
		{
			get
			{
				if (IsValid && fHouseBillNumber.IsEmpty)
				{
					fHouseBillNumber = GetHouseBillNumber();
				}
				return fHouseBillNumber;
			}
		}

		public readonly ZString MessageText;
		public readonly ZString StandardMessageIdentifier;

		#region Implementation

		#region Parsing

		ZString GetMasterBillNumber()
		{
			ZString result = ZString.Empty;
			int lineIndex = GetLineIndexStartsWith(CMD.Constants.MAWB);
			if (lineIndex != -1)
			{
				ZString line = MessageLines[lineIndex];
				result = line.SubstringSafe(4, 12);
			}
			return result;
		}

		ZString GetHouseBillNumber()
		{
			ZString result = ZString.Empty;
			int lineIndex = GetLineIndexStartsWith(CMD.Constants.HAWB);
			if (lineIndex != -1)
			{
				ZString line = MessageLines[lineIndex];
				ZString tempString = line.SubstringSafe(4, 17);
				int index = tempString.IndexOf('/');
				result = (index < 0) ? tempString : tempString.Left(index);
			}
			return result;
		}

		#endregion

		int GetLineIndexStartsWith(ZString identifier)
		{
			for (int i = 0; i < MessageLines.Length; i++)
			{
				if (MessageLines[i].StartsWith(identifier))
				{
					return i;
				}
			}
			return -1;
		}

		protected readonly ZString[] MessageLines;
		public abstract bool IsValid { get; }

		ZString fMasterBillNumber;
		ZString fHouseBillNumber;

		#endregion
	}
}
