using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.EDIInterchanges;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.TR.Business
{
	public class TRInterchangeProvider : InterchangeProviderBase
	{
		public TRInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages)
			: base(messages)
		{
			this.logger = logger;
		}

		readonly LoggingInformation logger;

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected override Type InterchangeType => typeof(TREDIInterchange);

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count > 0)
			{
				var message = messages[0];
				var to = TRInterchangeHelper.GetInterchangeTo(message.EM_MessageType, message.EM_IsTestMessage);
				interchange.EI_TransportType = EDIInterchange.TransportType.xT;
				SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, to, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
				interchange.EI_SessionGUID = ZGuid.NewZGuid();

				var trMessage = message as TRBaseMessage;
				if (trMessage != null)
				{
					if (trMessage.NeedToSignMessage)
					{
						interchange.EI_BodyData = trMessage.EM_MessageData;
					}

					if (trMessage.EM_MessageType == TRMessageTypes.Codes.EUT)
					{
						interchange.EI_HeaderText =  TRInterchangeHelper.CreateHeaderTextForEUTInterchange(interchange.EI_SessionGUID);
					}
				}

				if (interchange.EI_BodyText.IsEmpty || interchange.EI_BodyData.IsEmpty)
				{
					var messageToLog = Res.GetString("3B00CE8C-0449-45BB-98FF-5AA34242BF2B",
					"The Interchange Body is empty even though there are {0} messages.\r\nMessage Text : \r\n{1}",
					messages.Count.ToString(CultureInfo.InvariantCulture),
					GetMessageText(messages));

					ErrorReporter.ReportOnce(messageToLog);
					logger.LogWarning(messageToLog);

					interchange.ContainedMessages.RemoveAll();
					interchange.Delete();
				}
				else
				{
					interchange.EI_InterchangeNum = message.EM_MessageNum;
				}
			}
		}

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			return ZString.Empty;
		}

		protected override string GetCollationKey(EDIMessage message)
		{
			return DoNotCollateType;
		}

		ZString GetMessageText(NonDependentEDIMessageCollection messages)
		{
			var builder = new ZStringBuilder();
			foreach (EDIMessage message in messages)
			{
				builder.Append(message.EM_MessageText);
				if (builder.Length > 100)
				{
					break;
				}
			}
			return builder.ToString();
		}
	}
}
