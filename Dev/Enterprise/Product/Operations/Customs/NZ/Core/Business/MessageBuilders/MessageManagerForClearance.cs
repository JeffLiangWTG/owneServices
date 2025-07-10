using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.BatchProcessor;

namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	public abstract class MessageManagerForClearance : MessageManager
	{
		protected MessageManagerForClearance(OperationType operationType)
			: base(operationType)
		{
		}

		public override bool IsOkToExecute
		{
			get
			{
				bool result = false;
				if (!ValidateMessagingPreconditions())
				{
					result = false;
				}
				else if (fOperationType == OperationType.ResetToOriginal)
				{
					result = ValidateResetToOriginal();
				}
				else
				{
					result = ValidateSendMessage();
				}

				if (result)
				{
					ValidateServiceTasksAreRunning();
				}

				return result;
			}
		}

		protected virtual void ValidateServiceTasksAreRunning()
		{
			fLastHumanReadableWarning = ServiceTasksHelper.CheckRequiredServiceTasksAreActive();
		}

		protected virtual bool ValidateMessagingPreconditions()
		{
			fLastHumanReadableStatus = string.Empty;
			var errors = new BatchProcessorEnvironmentChecker().CheckEverythingRequiredToRunIsInPlace();
			if (errors?.Length > 0)
			{
				var details = System.Environment.NewLine + string.Join(System.Environment.NewLine, errors);
				fLastHumanReadableStatus = GetBatchProcessorEnvironmentErrorMessage(details);
			}

			return string.IsNullOrEmpty(fLastHumanReadableStatus);
		}

		public string GetBatchProcessorEnvironmentErrorMessage(string details) => ResString.GetMultilingualString("ed031610-bc18-4177-bb19-e64321158b83", "You must setup the following registry items before sending to Customs.{0}", details);

		protected bool ValidateSendMessage()
		{
			ZString impediment = GetSendMessageImpediment();
			bool result = impediment.IsEmpty;
			fLastHumanReadableStatus = result ? MessageReadyToSendMessage : "Cannot Send Message - " + impediment;
			return result;
		}

		public const string MessageReadyToSendMessage = "Ready to Send Message";
		public const string MessageReportingImmediateSend = "Generated and Ready to be sent by Service Tasks.";

		protected abstract ZString GetSendMessageImpediment();
		protected abstract ZString GetResetToOriginalImpediment();

		protected bool ValidateResetToOriginal()
		{
			ZString impediment = GetResetToOriginalImpediment();
			bool result = impediment.IsEmpty;
			fLastHumanReadableStatus = result ? "Ready to Reset to Original" : "Cannot Reset to Original - " + impediment;
			return result;
		}

		public override string MessageTypeAndStatus
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
							case MessageType.ReplaceHeaderAndLines:
								result += "Replacement Header and Lines Messages";
								break;
							case MessageType.Replacement:
								result += "Replace Rejected Message";
								break;
						}
					}
				}
				return result;
			}
		}

		[CargoWise.ComponentModel.MaxLength(250)]
		public ZString EnteredRemarks
		{
			get { return GetEnteredRemarks(); }
			set
			{
				CheckMaximumLength(EnteredRemarksInfo, value);
				SetEnteredRemarks(value);
				EnteredRemarksInfo.RefreshBinding();
			}
		}

		protected abstract ZString GetEnteredRemarks();
		protected abstract void SetEnteredRemarks(ZString value);

		public virtual ZPropertyInfo EnteredRemarksInfo
		{
			get { return GetZPropertyInfo(nameof(EnteredRemarks)); }
		}

		public abstract bool IsOKToSendWithMessagingErrors();

		public bool SaveHandlingSaveExceptions(BusinessObjectFactory factory, Action onFailure = null)
		{
			bool result = false;
			try
			{
				factory.Save();
				result = true;
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				onFailure?.Invoke();
				// This is insufficient exception handling for a call to factory save, given that there is no handling for concurrency exceptions.
				ZExceptionReporting.HandleSaveException(exception);
			}
			return result;
		}

		public ZString GetErrorsForEnteredValues()
		{
			ZString result = GetErrorsForEnteredValuesCore();
			if (MessageTypeToBeSent != MessageType.Original && EnteredRemarks.IsEmpty)
			{
				result += "Invalid Remarks: Must have Remarks sending any amendment messages.";
			}
			return result;
		}

		protected virtual ZString GetErrorsForEnteredValuesCore()
		{
			return ZString.Empty;
		}
	}
}
