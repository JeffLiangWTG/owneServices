using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FTZMessageManager
	{
		public FTZMessageManager(IFTZHeader header)
		{
			this.header = header;
		}
		readonly IFTZHeader header;

		public void PopulateMessage(UpdateActionCode actionCode)
		{
			var builder = new FTZMessageBuilder(header, actionCode);
			var message = builder.PopulateMessage();
			header.AddMessage(message);
			CalculateStatus(actionCode, message);
		}

		void CalculateStatus(UpdateActionCode actionCode, MQEDIMessage message)
		{
			var subType = ZString.Empty;
			if (actionCode == UpdateActionCode.Add)
			{
				subType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			}
			else if (actionCode == UpdateActionCode.Replace)
			{
				subType = EM_MessageSubTypeList.Codes.FTZAdmissionReplace;
			}
			else if (actionCode == UpdateActionCode.Delete)
			{
				subType = EM_MessageSubTypeList.Codes.FTZAdmissionDelete;
			}
			message.EM_MessageSubType = subType;
			message.UpdateErrorFlag();
			header.SetMessageStatus(subType, new FTZMessageStatusCalculator().Calculate(message, false, false));
		}

		public bool CanSendThisMessage(UpdateActionCode actionCode, out string notificationText, out bool hasBeenLodged)
		{
			bool canSend;

			notificationText = "";
			hasBeenLodged = header.HasBeenLodgedAtCustoms;
			canSend = true;

			if (actionCode == UpdateActionCode.Add)
			{
				if (hasBeenLodged)
				{
					notificationText = AlreadyAdded;
					canSend = false;
				}
			}
			else
			{
				if (!hasBeenLodged)
				{
					notificationText = NotAdded;
					canSend = false;
				}
			}
			return canSend;
		}
		internal const string AlreadyAdded = "the FTZ has already been added, you should send Replace message.";
		internal const string NotAdded = "the FTZ has not been added yet.";

		public bool IsWaitingForResponse
		{
			get { return header.IsWaitingForResponse; }
		}
	}
}
