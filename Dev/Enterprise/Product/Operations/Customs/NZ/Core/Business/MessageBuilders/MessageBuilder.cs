using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	public abstract class MessageBuilder
	{
		public MessageBuilder()
		{
			generated = false;
		}

		public virtual EDIMessage GenerateMessage()
		{
			message = GetNewMessage();
			message.EM_MessageText = GetMessageText();
			message.EM_MessageOwner = GetEncryptedPassword();
			SetMessageType();
			SetMessageSubType();
			SetMessageHeldUntilDate();
			SetParentMessagingStatusAfterMessagePosting();

			return message;
		}

		public string PostedMessageNumber
		{
			get
			{
				string result = "0";
				if (message != null)
				{
					result = message.EM_MessageNum;
				}
				return result;
			}
		}

		public EDIMessage MessageBusinessObject
		{
			get { return message; }
		}

		protected abstract EDIMessage GetNewMessage();
		protected abstract void GenerateIfNotAlreadyGenerated();
		public abstract string GetMessageText();
		protected abstract void SetMessageSubType();
		protected abstract void SetMessageType();
		protected abstract void SetParentMessagingStatusAfterMessagePosting();
		protected EDIMessage message;
		protected bool generated;
		protected const int AlowanceForMinutesClientsClockMayBeDifferentFromCustomsClock = 15;

		public virtual string GetEncryptedPassword()
		{
			return ZString.Empty;
		}

		protected void SetMessageHeldUntilDate()
		{
			ZDateTime heldUntilDate = GetHeldUntilDate();
			if (!heldUntilDate.IsEmpty && heldUntilDate.IsValid)
			{
				message.EM_HeldUntilDate = heldUntilDate;
			}
		}

		protected virtual ZDateTime GetHeldUntilDate()
		{
			return ZDateTime.Empty;
		}

		protected ZDateTime CalculateMessageHeldDate(JobDeclaration declaration)
		{
			var result = ZDateTime.Empty;
			var transmitDate = declaration?.JE_EDITransmitDate.Date ?? ZDate.Empty;
			if (transmitDate.IsValid && transmitDate > ZDate.Today)
			{
				result = transmitDate.AddMinutes(AlowanceForMinutesClientsClockMayBeDifferentFromCustomsClock).ToUniversalBranchTime(declaration.Factory);
			}

			return result;
		}
	}
}
