using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.NZ.Business.MessageBuilders;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class MessageBuilder : XmlMessageBuilder, IManifestMessageBuilder
	{
		public enum MessageTypes { None = 0, Cancellation = 1, Replacement = 5, Original = 9 }

		public MessageBuilder(ForwardingConsol consol, IAdditionalInformation additionalInformation, MessageTypes messageType, ICRManifestStatus manifestStatus, string submitterCode)
			: base(consol)
		{
			this.consol = consol;
			this.messageType = messageType;
			this.manifestStatus = manifestStatus;

			msgBuilder = new ICRMessageBuilder(new ICRConsolWrapper(consol, additionalInformation), TransactionType, submitterCode);
			consol.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
		}

		readonly ForwardingConsol consol;
		readonly MessageTypes messageType;
		readonly ICRManifestStatus manifestStatus;
		readonly ICRMessageBuilder msgBuilder;

		TSWTransactionTypes TransactionType
		{
			get
			{
				if (messageType == MessageBuilder.MessageTypes.Cancellation)
				{
					return TSWTransactionTypes.Cancel;
				}
				else if (messageType == MessageBuilder.MessageTypes.Replacement)
				{
					return TSWTransactionTypes.Replace;
				}
				else
				{
					return TSWTransactionTypes.Original;
				}
			}
		}

		protected override EDIMessage GetNewMessage()
		{
			CheckErrorsBeforeGeneratingMessage();
			var message = (ICRMessage)consol.Messages.AddNew(typeof(ICRMessage));
			message.EM_LinkedObject = consol;
			return message;
		}

		public override ZString DeclarantPinEncrypted
		{
			get { return msgBuilder.DeclarantPinEncrypted; }
		}

		public override ZBool DeclarantPinRequired
		{
			get { return msgBuilder.DeclarantPinRequired; }
		}

		void CheckErrorsBeforeGeneratingMessage()
		{
			if (!errorChecked)
			{
				errorList = ICRValidation.CheckErrorsBeforeGeneratingMessage();
				errorList = new List<string>();
				errorChecked = true;
			}
		}

		ICRValidation ICRValidation
		{
			get
			{
				if (fICRValidation == null)
				{
					fICRValidation = new ICRValidation(consol, TransactionType, manifestStatus);
				}
				return fICRValidation;
			}
		}
		ICRValidation fICRValidation;

		protected override void SetMessageSubType()
		{
			message.SetMessageSubType(messageType);
		}

		new ICRMessage message
		{
			get { return (ICRMessage)base.message; }
		}

		protected override void SetParentMessagingStatusAfterMessagePosting()
		{
			manifestStatus.E2_MessageStatus = LowValueManifestStatusList.Codes.SentToCustoms;
		}

		#region IManifestMessageBuilder
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
						errorReport.Append(error);
					}
				}
				return errorReport.ToString();
			}
		}

		public int ErrorCount
		{
			get { return 0; }
		}

		public override string GetMessageText()
		{
			return msgBuilder.GetXMLMessage();
		}

		public ZString ManifestMessageTypeCode
		{
			get
			{
				ZString result = ZString.Empty;
				switch (messageType)
				{
					case MessageTypes.Cancellation:
						result = "Cancel";
						break;
					case MessageTypes.None:
						result = "Unknown";
						break;
					case MessageTypes.Original:
						result = "Original";
						break;
					case MessageTypes.Replacement:
						result = "Replacement";
						break;
				}
				return result;
			}
		}

		#endregion
		StringBuilder errorReport;
		List<string> errorList;
		bool errorChecked;

#if DEBUG
		internal
#endif
 void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				errorChecked = false;
			}
		}

		#region IManifestMessageBuilder

		int IManifestMessageBuilder.ErrorCount
		{
			get { return ICRValidation.ErrorCount; }
		}

		string IManifestMessageBuilder.Errors
		{
			get
			{
				CheckErrorsBeforeGeneratingMessage();
				if (errorReport == null)
				{
					errorReport = new StringBuilder(ErrorCount);
					foreach (string error in errorList)
					{
						errorReport.Append(error);
					}
				}
				return errorReport.ToString();
			}
		}

		ZString IManifestMessageBuilder.ManifestMessageTypeCode
		{
			get
			{
				var result = ZString.Empty;
				switch (messageType)
				{
					case MessageTypes.Cancellation:
						result = "Cancel";
						break;
					case MessageTypes.None:
						result = "Unknown";
						break;
					case MessageTypes.Original:
						result = "Original";
						break;
					case MessageTypes.Replacement:
						result = "Replacement";
						break;
				}
				return result;
			}
		}

		IMessageBuilderResult Messaging.MessageBuilders.IMessageBuilder.PopulateMessages()
		{
			GenerateMessage();
			try
			{
				message.Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}
			return null;
		}

		#endregion
	}
}
