using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.MessageBuilders;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.BatchProcessor
{
	public class NZTSWInterchangeProvider : NZInterchangeProvider
	{
		public NZTSWInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages)
			: base(logger, messages)
		{
			this.logger = logger;
		}

		readonly LoggingInformation logger;
		ZString xmlWithMetaData;
		ZString pin;

		#region Overrides

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var message = messages[0];
			SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, GetNZCRecipientID(message), NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant());

			xmlWithMetaData = message.EM_MessageText;
			if (message.EM_MessageOwner.IsEmpty)
			{
				pin = ZString.Empty;
			}
			else
			{
				var broker = message.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, message.EM_SystemCreateUser));
				if (broker == null)
				{
					pin = ZString.Empty;
				}
				else
				{
					pin = broker.GetNZWrapper().NZBPassword.CurrentDecryptedPassword.ToUpper();
				}
			}

			if (interchange.EI_BodyText.IsEmpty)
			{
				string messageToLog = string.Format(
					"The Interchange Body is empty even though there are {0} messages.\nMessage Text : \n{1}",
					messages.Count.ToString(),
					GetMessageText(messages));

				ErrorReporter.ReportOnce(messageToLog);
				logger.LogWarning(messageToLog);

				interchange.ContainedMessages.RemoveAll();
				interchange.Delete();
				message.EM_Status = EDIMessage.Status.Failed;
			}
		}

		protected override ZString GetInterchangeFooter(int messageCount)
		{
			// for eHub team, EI_Footer will contain the MAC, (message authentication code), needed for the Authentication element in the Manifest
			// that is sent in the SOAP message, when required by message type and function. 
			// This will be indicated by EM_MessageOwner having a value, when required, which enables the declarant pin to be determined in this class.
			return pin.IsEmpty ? ZString.Empty : (ZString)HMAC256.GenerateNewHMAC(pin, xmlWithMetaData);
		}

		#endregion
	}
}
