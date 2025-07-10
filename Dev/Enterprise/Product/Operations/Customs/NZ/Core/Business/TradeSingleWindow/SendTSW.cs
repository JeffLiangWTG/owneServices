using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	/// <summary>
	/// Base class for sending of CRE, ICR & OCR messages 
	/// </summary>
	public abstract class SendTSW
	{
		public SendTSW(BusinessObject hostEntity, IAdditionalInformation additionalMessageInformation, TSWTransactionTypes transactionType)
		{
			HostEntity = Argument.NotNull(hostEntity, "hostEntity");
			AdditionalMessageInformation = additionalMessageInformation;
			TransactionType = transactionType;
			hostEntity.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
		}

		public abstract ZString GetMessageText();
		public abstract ZString DeclarantPinEncrypted { get; }
		public abstract ZBool DeclarantPinRequired { get; }
		public abstract ZString ApplicationReference { get; }
		protected abstract ZString MessageType { get; }
		protected abstract ZString MessageSubType { get; }
		protected abstract ZBool GetIsMessageInTestMode { get; }
		protected abstract void SetStatusOnSuccess(StatusTransactionScope scope);
		protected abstract void AddMessageToMessages(TSWMessage message);

		protected readonly BusinessObject HostEntity;

		protected readonly IAdditionalInformation AdditionalMessageInformation;

		protected readonly TSWTransactionTypes TransactionType;
		public ZString SubmitterCode => NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant().PadLeft(9, '0');

		// for eHub team, EI_Footer will contain the MAC, (message authentication code), needed for the Authentication element in the 
		// Manifest that is sent in the SOAP message, when required by message type and function. 
		// To be able to generate that MAC at the time of creating the interchange, we need to indicate if the MAC is required.
		// We do that by populating EM_MessageOwner with the a portion of the Declarant's encrypted pin.

		public bool SendMessage(BaseMessageSendingObject action = null)
		{
			var message = HostEntity.Factory.New<TSWMessage>();

			message.EM_MessageType = MessageType.ToString();
			message.EM_MessageText = action != null
				? action.MessageCreated(GetMessageText())
				: GetMessageText();
			message.EM_MessageOwner = DeclarantPinRequired ? DeclarantPinEncrypted : ZString.Empty;
			message.EM_ApplicationReference = ApplicationReference;
			message.EM_MessageSubType = MessageSubType;
			message.EM_LinkedObject = HostEntity;
			message.EM_IsTestMessage = GetIsMessageInTestMode;

			if (AdditionalMessageInformation != null && AdditionalMessageInformation.SupportingDocuments != null)
			{
				foreach (var cusAttachment in AdditionalMessageInformation.SupportingDocuments)
				{
					AddAttachment(message, cusAttachment);
				}
			}

			foreach (var cusAttachment in GetAdditionalSupportingDocuments())
			{
				AddAttachment(message, cusAttachment);
			}

			var transactionScope = new StatusTransactionScope();
			try
			{
				AddMessageToMessages(message);
				SetStatusOnSuccess(transactionScope);
				message.EM_FormattedMessageTextInfo.RefreshBinding();
				HostEntity.Factory.Save();
				return true;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				message.Delete();
				transactionScope.Rollback();
				ZExceptionReporting.HandleSaveException(e);
				return false;
			}
		}

		void AddAttachment(TSWMessage message, ITSWAttachment cusAttachment)
		{
			var ediMessageAttach = message.MessageAttachments.AddNew();
			ediMessageAttach.EG_FileName = TSWMessageFormatter.FormatAcceptableFileNameForNZC(cusAttachment.FileName);
			ediMessageAttach.EG_StorageDocsGuid = cusAttachment.UniqueIdentifier;
			ediMessageAttach.EG_EdiMsgDocType = cusAttachment.DocType;
		}

		protected virtual IEnumerable<ITSWAttachment> GetAdditionalSupportingDocuments()
		{
			return Enumerable.Empty<ITSWAttachment>();
		}

		#region Error Handling

		protected List<string> errorList;
		StringBuilder errorReport;
		bool errorChecked;

		public string Errors
		{
			get
			{
				CheckErrorsBeforeGeneratingMessage();

				if (errorReport == null)
				{
					errorReport = new StringBuilder(ErrorCount);
					foreach (string error in errorList)
					{
						errorReport.Append(error + "\r\n");
					}
				}
				return errorReport.ToString();
			}
		}

		public StringCollection ErrorAndWarningMessages
		{
			get
			{
				var result = new StringCollection();
				if (errorList != null)
				{
					foreach (string error in errorList)
					{
						result.Add(error);
					}
				}

				if (warningList != null)
				{
					foreach (string waring in warningList)
					{
						result.Add(waring);
					}
				}

				return result;
			}
		}

		public int ErrorCount
		{
			get
			{
				CheckErrorsBeforeGeneratingMessage();
				return errorList.Count;
			}
		}

		public int ErrorAndWarningCount
		{
			get
			{
				CheckErrorsBeforeGeneratingMessage();
				CheckWarningsBeforeGeneratingMessage();
				return errorList.Count + warningList.Count;
			}
		}

		void CheckErrorsBeforeGeneratingMessage()
		{
			if (!errorChecked)
			{
				errorList = new List<string>();
				errorList.AddRange(new BatchProcessorEnvironmentChecker().CheckEverythingRequiredToRunIsInPlace());

				if (errorList.IsNullOrEmpty())
				{
					CheckErrorsBeforeGeneratingMessageCore();
				}
				errorChecked = true;
			}
		}

		protected abstract void CheckErrorsBeforeGeneratingMessageCore();

		#endregion

		#region Waring Handling
		protected List<string> warningList;

		public StringCollection MessageWarnings
		{
			get
			{
				var result = new StringCollection();
				if (warningList != null)
				{
					foreach (string waring in warningList)
					{
						result.Add(waring);
					}
				}

				return result;
			}
		}

		public bool CheckWarningsBeforeGeneratingMessage()
		{
			warningList = new List<string>();
			var serviceWarning = ServiceTasksHelper.CheckRequiredServiceTasksAreActive();
			if (!string.IsNullOrEmpty(serviceWarning))
			{
				warningList.Add(serviceWarning);
			}

			return warningList.Count > 0;
		}
		#endregion

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				errorChecked = false;
			}
		}

		#region Transport Validation

		public virtual ZString GetBOValidationMessageErrors()
		{
			var transportMessageErrors = ZString.Empty;
			var consol = HostEntity as Freight.Forwarding.Business.ForwardingConsol;
			if (consol != null)
			{
				var transport = consol.Transports.ExportTransport ?? consol.Transports.MostInterestingTransport;
				if (transport != null)
				{
					try
					{
						transport.IsValidationSuspendedForCargoReport = true;
						var transportValidation = new CargoReportTransportValidation(transport);
						transportValidation.ValidateAll();
					}
					finally
					{
						transport.IsValidationSuspendedForCargoReport = false;
					}

					var messageErrorNotifications = new ZNotificationCollector(consol, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
					transportMessageErrors = messageErrorNotifications.ToUniqueMessageListString();

					if (!transportMessageErrors.IsEmpty)
					{
						transportMessageErrors = MessageSendingValidation.MessageErrorsExistHeaderText + "\r\n\r\n" + transportMessageErrors + "\r\n\r\n" + MessageSendingValidation.MessageErrorConfirmationQuestionText;
					}
				}
			}

			return transportMessageErrors;
		}

		#endregion

		protected const string LogReferenceCancel = "CAN";
		protected const string LogReferenceSent = "SND";
	}
}
