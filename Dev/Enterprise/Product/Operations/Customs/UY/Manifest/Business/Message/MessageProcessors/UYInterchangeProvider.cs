using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using System.Collections.Generic;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class UYInterchangeProvider : InterchangeProviderBase
	{
		public UYInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages)
			: base(messages)
		{
			this.logger = logger;
		}
		readonly LoggingInformation logger;

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count == 1)
			{
				var message = messages.Cast<EDIMessage>().Single();
				var sender = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
				var receiver = message.EM_IsTestMessage ? UYCInterchange.UYCustomsForTest : UYCInterchange.UYCustomsForProd;
				var messageToLog = "";

				if (!message.EM_MessageText.IsEmpty)
				{
					interchange.EI_TransportType = EDIInterchange.TransportType.xT;
					SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, receiver, sender);
					if (!interchange.EI_BodyText.IsEmpty)
					{
						interchange.EI_InterchangeNum = message.EM_MessageNum;
						interchange.EI_SessionGUID = ZGuid.NewZGuid();
						interchange.EI_GP = message.EM_GP;
					}
					else
					{
						messageToLog = Res.GetString("093E483B-59AA-4E36-B02E-D9C3C67C0C94", "The Certificate is not valid.");
					}
				}
				else
				{
					messageToLog = Res.GetString("B535AAB0-FE85-42AB-A901-3CF0829F198B",
						"The Interchange Body is empty even though there are {0} messages.",
						messages.Count.ToString(CultureInfo.InvariantCulture));
				}

				if (!string.IsNullOrEmpty(messageToLog))
				{
					message.EM_Status = EDIMessage.Status.Discarded;

					var headerMessage = Res.GetString("4D9B3E0E-937E-4996-862B-5B29903AF1FE",
						"Message {0} will be discarded for the following reason:",
						message.EM_MessageNum);

					var stringBuilder = new ZStringBuilder();
					stringBuilder.Append(headerMessage);
					stringBuilder.Append(messageToLog);

					message.Notes.AddNew(true, ProcessingLogDescription, stringBuilder.ToStringWithNewLineBetweenAppends());
					logger.LogWarning(messageToLog);

					interchange.ContainedMessages.RemoveAll();
					interchange.Delete();
				}
			}
		}

		protected override void AppendMessageTextToMessageBody(StringBuilder stringBuilder, IEnumerable<ZString> messageTextList, EDIInterchange interchange)
		{
			foreach (var messageText in messageTextList)
			{
				var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GetGlbExternalPassword<GlbCompanyCredential>(PasswordTypesList.Codes.UTB);

				if (credential != null
					&& credential.GP_PasswordStatus == PasswordStatusList.Codes.Valid
					&& !credential.GP_Certificate.IsEmpty
					&& !credential.CurrentDecryptedCertificatePassphrase.IsEmpty)
				{
					X509Certificate2 certificate = new X509Certificate2(credential.GP_Certificate, credential.CurrentDecryptedCertificatePassphrase);
					stringBuilder.Append(UYMessageHelper.EnvelopeSignedMessage(UYSignature.Sign(messageText, certificate)));
				}
			}
		}

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => EDIInterchange.Status.Queued;

		protected override Type InterchangeType => typeof(UYCInterchange);
		protected override string GetCollationKey(EDIMessage message) => DoNotCollateType;
	}
}
