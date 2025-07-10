using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	public abstract class MessageManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public enum MessageType { None, CancelEntry, Replacement, Original, ReplaceHeaderAndLines, ResetToOriginal }
		public enum OperationType { SubmitMessage, CancelMessage, ResetToOriginal }

		public MessageManager(OperationType operationType)
		{
			fOperationType = operationType;
		}
		protected readonly OperationType fOperationType;

		public string HumanReadableOperationType
		{
			get
			{
				string result = "";
				switch (fOperationType)
				{
					case OperationType.CancelMessage:
						result = "Submit Cancellation";
						break;
					case OperationType.SubmitMessage:
						result = "Submit Message";
						break;
					case OperationType.ResetToOriginal:
						result = "Reset to Original";
						break;
				}
				return result;
			}
		}

		protected string fLastHumanReadableStatus = "";

		protected string fLastHumanReadableWarning = "";

		public string LastHumanReadableStatus
		{
			get { return fLastHumanReadableStatus; }
		}

		public string LastHumanReadableWarning
		{
			get { return fLastHumanReadableWarning; }
		}

		public abstract bool IsOkToExecute { get; }

		public MessageType MessageTypeToBeSent
		{
			get
			{
				MessageType result = MessageType.None;
				if (!IsOkToExecute)
				{
					result = MessageType.None;
				}
				else if (fOperationType == OperationType.CancelMessage)
				{
					result = MessageType.CancelEntry;
				}
				else if (fOperationType == OperationType.ResetToOriginal)
				{
					result = MessageType.ResetToOriginal;
				}
				else
				{
					result = GetMessageTypeForSubmit();
				}

				return result;
			}
		}

		public virtual string MessageTypeAndStatus
		{
			get
			{
				string result = "";
				if (!IsOkToExecute)
				{
					result = LastHumanReadableStatus;
				}
				else
				{
					result = "Ready to Send ";
					if (fOperationType == OperationType.CancelMessage)
					{
						result += "Cancellation Message";
					}
					else
					{
						switch (GetMessageTypeForSubmit())
						{
							case MessageType.Original:
								result += "Original Message";
								break;
							case MessageType.Replacement:
								result += "Replacement Message";
								break;
						}
					}
				}
				return result;
			}
		}

		public abstract bool Execute();

		protected abstract MessageType GetMessageTypeForSubmit();
	}
}
