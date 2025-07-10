using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.MessageManagers
{
	public class EDITWCStatusCalculator
	{
		public EDITWCStatusCalculator(TWMessage message)
		{
			this.message = message;
		}

		readonly TWMessage message;

		public bool IsAwaitingReply(ZString currentMessageStatus)
		{
			return JobDeclarationMessageStatusList.IsAwaiting(currentMessageStatus);
		}

		public ZString GetMessageAwaitingStatus(ZString action)
		{
			var result = ZString.Empty;
			switch (message.EM_MessageType)
			{
				case MessageTypeList.Codes.ICD:
				case MessageTypeList.Codes.ECD:
					switch (action)
					{
						case ActionCodeList.Codes.Create:
							result = JobDeclarationMessageStatusList.Codes.AWO;
							break;
						case ActionCodeList.Codes.Update:
							result = JobDeclarationMessageStatusList.Codes.AWC;
							break;
					}
					break;
				case MessageTypeList.Codes.ADM:
					result = JobDeclarationMessageStatusList.Codes.AWE;
					break;
				case MessageTypeList.Codes.IEA:
					result = JobDeclarationMessageStatusList.Codes.AWG;
					break;
			}
			return result;
		}

		public ZString GetMessageAcknowledgedStatus()
		{
			var result = ZString.Empty;
			switch (message.CusEntryHeader?.CH_Status ?? ZString.Empty)
			{
				case JobDeclarationMessageStatusList.Codes.AWO:
					result = JobDeclarationMessageStatusList.Codes.ACO;
					break;
				case JobDeclarationMessageStatusList.Codes.AWC:
					result = JobDeclarationMessageStatusList.Codes.ACC;
					break;
				case JobDeclarationMessageStatusList.Codes.AWG:
					result = JobDeclarationMessageStatusList.Codes.ACG;
					break;
				case JobDeclarationMessageStatusList.Codes.AWE:
					result = JobDeclarationMessageStatusList.Codes.ACE;
					break;
			}
			return result;
		}

		public ZString GetMessageRejectedStatus()
		{
			var result = ZString.Empty;
			switch (message.CusEntryHeader?.CH_Status ?? ZString.Empty)
			{
				case JobDeclarationMessageStatusList.Codes.AWO:
					result = JobDeclarationMessageStatusList.Codes.ERO;
					break;
				case JobDeclarationMessageStatusList.Codes.AWC:
					result = JobDeclarationMessageStatusList.Codes.ERC;
					break;
				case JobDeclarationMessageStatusList.Codes.AWG:
					result = JobDeclarationMessageStatusList.Codes.ERG;
					break;
				case JobDeclarationMessageStatusList.Codes.AWE:
					result = JobDeclarationMessageStatusList.Codes.ERE;
					break;
			}
			return result;
		}
	}
}
