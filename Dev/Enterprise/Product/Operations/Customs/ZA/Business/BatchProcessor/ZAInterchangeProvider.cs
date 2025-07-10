using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Edifact;
using Enterprise.Edifact.Generic;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.ZA.Business.BatchProcessor
{
	public class ZAInterchangeProvider : InterchangeProviderBase
	{
		public ZAInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages)
			: base(messages)
		{
			this.logger = logger;
		}
		readonly LoggingInformation logger;

		const string LineBreakString = "\n";
		const string EdifactReleaseNumber_16A = "16A";

		#region Overrides

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var pivotMessage = messages[0];
			SetInterchangeValuesForTransmit(interchange, messages, pivotMessage.EM_MessageType, ZACInterchange.SARSeHubID, pivotMessage.Company.GC_Code);
			if (interchange.EI_BodyText.IsEmpty)
			{
				string messageToLog = string.Format(CultureInfo.InvariantCulture,
					"The Interchange Body is empty even though there are {0} messages.\nMessage Text : \n{1}",
					messages.Count.ToString(CultureInfo.InvariantCulture),
					GetMessageText(messages));

				ErrorReporter.ReportOnce(messageToLog);
				logger.LogWarning(messageToLog);

				interchange.ContainedMessages.RemoveAll();
				interchange.Delete();
			}
			else
			{
				var interchangeSenderIdProvider = pivotMessage.EM_LinkedObject as IInterchangeSenderIdProvider ?? pivotMessage as IInterchangeSenderIdProvider;
				if (interchangeSenderIdProvider != null)
				{
					var uNB = GetZAVersion4UNB(interchangeSenderIdProvider.SenderID, pivotMessage.EM_MessageType, pivotMessage.EM_IsTestMessage, PreparedTime.ToZDateTime(), GetMessageReleaseNumber(interchange.EI_BodyText) != EdifactReleaseNumber_16A);
					interchange.EI_HeaderText = uNB.ToString(CharacterSet) + LineBreakString;
				}
			}
		}

		UNCharacterSet CharacterSet => unCharacterSet ?? (unCharacterSet = new ZACharacterSet());
		UNCharacterSet unCharacterSet;

		string GetMessageReleaseNumber(ZString messageText)
		{
			var uNHString = messageText.SubstringSafe(0, messageText.IndexOf(CharacterSet.SegmentDelimiterChar));
			try
			{
				var segment = new UNHSegment();
				segment.Parse(CharacterSet, uNHString);
				return segment.MessageIdentifier.MessageReleaseNumber;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return "";
			}
		}

		#region HeaderPartGeneration

		static UNBSegment GetZAVersion4UNB(ZString profileID, ZString messageType, bool isTest, ZDateTime preparedTime, ZBool isCommunicationsAgreementIdNeeded)
		{
			UNBSegment uNB = new UNBSegment();
			uNB.SyntaxIdentifier.SyntaxIdentifier = "UNOB";
			uNB.SyntaxIdentifier.SyntaxVersionNumber = "4";

			uNB.InterchangeSender.SenderIdentification = profileID;
			uNB.InterchangeSender.InterchangeSenderInternalIdentification = "SENDERID";  // These placeholders get flipped by eHub (yuk)
			uNB.InterchangeSender.InterchangeSenderInternalSubIdentification = "SENDERSUBID";

			uNB.InterchangeRecipient.RecipientIdentification = TranslateMessageTypeToRecipientIdentification(messageType, isTest);

			uNB.DateTimeOfPreparation.Date = preparedTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			uNB.DateTimeOfPreparation.Time = preparedTime.ToString("HHmm", CultureInfo.InvariantCulture);

			uNB.InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder;

			uNB.ApplicationReference = TranslateMessageTypeToApplicationReference(messageType);

			uNB.AcknowlegementRequest = "1";

			if (isCommunicationsAgreementIdNeeded)
			{
				uNB.CommunicationsAgreementId = "TRADINGPARTNER";
			}

			if (isTest)
			{
				uNB.TestIndicator = "1";
			}

			return uNB;
		}

		internal static string TranslateMessageTypeToRecipientIdentification(ZString messageType, bool isTest)
		{
			ZString result;
			switch (messageType)
			{
				case SARSEDIMessage.MessageTypes.CUSCAR:
				case SARSEDIMessage.MessageTypes.COSTCO:
				case SARSEDIMessage.MessageTypes.CALINF:
				case SARSEDIMessage.MessageTypes.GOVGIO:
					result = EDIInterchange.InterchangePartyIDs.ZACCustomsMailbox + SARSEDIMessage.MessageTypes.CUSCAR;
					break;
				default:
					result = EDIInterchange.InterchangePartyIDs.ZACCustomsMailbox + messageType;
					break;
			}
			return result + (isTest ? "T" : string.Empty);
		}

		internal static string TranslateMessageTypeToApplicationReference(ZString messageType)
		{
			var result = string.Empty;
			switch (messageType)
			{
				case SARSEDIMessage.MessageTypes.COSTCO:
					result = SARSEDIMessage.MessageTypeNames.COSTCO;
					break;
				case SARSEDIMessage.MessageTypes.CALINF:
					result = SARSEDIMessage.MessageTypeNames.CALINF;
					break;
				case SARSEDIMessage.MessageTypes.CUSDEC:
					result = SARSEDIMessage.MessageTypeNames.CUSDEC;
					break;
				case SARSEDIMessage.MessageTypes.EXPORT:
					result = SARSEDIMessage.MessageTypeNames.EXPORT;
					break;
				case SARSEDIMessage.MessageTypes.REQDOC:
					result = SARSEDIMessage.MessageTypeNames.REQDOC;
					break;
				case SARSEDIMessage.MessageTypes.CUSCAR:
					result = SARSEDIMessage.MessageTypeNames.CUSCAR;
					break;
				case SARSEDIMessage.MessageTypes.GOVGIO:
					result = SARSEDIMessage.MessageTypeNames.GOVGIO;
					break;
			}
			return result;
		}

		#endregion

		protected ZString GetMessageText(NonDependentEDIMessageCollection messages)
		{
			ZStringBuilder builder = new ZStringBuilder();
			foreach (EDIMessage message in messages)
			{
				builder.Append(message.EM_MessageText);
				if (builder.Length > 100) // MessageProcessingException only use the first 100 characters
				{
					break;
				}
			}
			return builder.ToString();
		}

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected override Type InterchangeType => typeof(ZACInterchange);

		protected override string GetCollationKey(EDIMessage message)
		{
			return message.PK.ToString();
		}

		protected override void AppendMessageTextToMessageBody(StringBuilder stringBuilder, IEnumerable<ZString> messageTextList, EDIInterchange interchange)
		{
			foreach (var messageText in messageTextList)
			{
				stringBuilder.Append(Regex.Replace(messageText, @"(?<!\?)'", "'" + LineBreakString));
			}
		}

		#endregion
	}
}
