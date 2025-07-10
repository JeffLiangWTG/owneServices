using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF
{
	public class CALINFMessageData : ICALINFMessageDataProvider
	{
		public CALINFMessageData(JobVoyage jobVoyage)
		{
			this.jobVoyage = Argument.NotNull(jobVoyage, nameof(jobVoyage));
			DetermineMessageSender();
			CreateCALINFTransport();
		}

		#region ICALINFMessageDataProvider

		ICALINFTransportInformation ICALINFMessageDataProvider.Transport => calinfTransport;

		ZString IBGM_BeginningOfMessage.CALINFMessageType
		{
			get
			{
				var calinfMessageType = ZString.Empty;
				if (jobVoyage.IsSea)
				{
					calinfMessageType = CALINFEDIMessage.CALINFMessageTypes.SCH;
				}
				else if (jobVoyage.IsAir)
				{
					calinfMessageType = CALINFEDIMessage.CALINFMessageTypes.ASC;
				}
				return calinfMessageType;
			}
		}

		ZDateTime IDTM_DocumentDateTime.DocumentIssueDateTime => ZDateTime.UtcNow;

		ZString IRFF_DocumentToBeAmended.DocumentToBeAmended => GetLastAccepted_CALINF_EDIMessage()?.ParentMessageNumber ?? ZString.Empty;

		CALINFEDIMessage GetLastAccepted_CALINF_EDIMessage()
		{
			CALINFEDIMessage result = null;
			var messages = jobVoyage.Messages;
			var factory = jobVoyage.Factory;

			var acceptedCUSRESs = messages.GetMatchingMessages(
					EDIMessage.ApplicationCodes.SouthAfricanCustoms,
					new ZString[] { SARSEDIMessage.MessageTypes.CUSRES },
					EDIMessage.Direction.Receive).OfType<CUSRESEDIMessage>()
				.Where(x => IsAccepted(factory, x))
				.OrderByDescending(x => x.EM_MessageDateTime);

			List<CALINFEDIMessage> outgoingMessages = messages.ToList().Cast<ZAMessage>().ToList()
				.Where(x => x.EM_ApplicationCode == EDIMessage.ApplicationCodes.SouthAfricanCustoms
							&& x.EM_MessageType == SARSEDIMessage.MessageTypes.CALINF
							&& x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit).Cast<CALINFEDIMessage>().ToList();

			foreach (var cusres in acceptedCUSRESs)
			{
				var lrn = cusres.LocalReferenceNumber;
				result = outgoingMessages.FirstOrDefault(x => x.LocalReferenceNumber == lrn);
				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		bool IsAccepted(BusinessObjectFactory factory, CUSRESEDIMessage msg) => AsycudaUniversalReference.CustomsStatusAttributeHelper.IsCustomsCleared(factory, Core.Constants.CountryCodes.SouthAfrica, msg.EntryStatus);

		ZString INAD_MessageSender.MessageSender => messageSender;

		void DetermineMessageSender()
		{
			messageSender = ZString.Empty;
			var orgHeader = jobVoyage.Factory.Load<OrgHeader>(jobVoyage.JV_OH_Line);
			if (orgHeader != null)
			{
				messageSender = orgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.SouthAfrica);
			}
		}

		void CreateCALINFTransport()
		{
			if (jobVoyage.IsSea)
			{
				calinfTransport = new CALINFTransportSea(jobVoyage);
			}
			else if (jobVoyage.IsAir)
			{
				calinfTransport = new CALINFTransportAir(jobVoyage);
			}
		}

		#endregion

		#region IEDIMessageCollectionProvider

		Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages => jobVoyage.Messages;

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory => jobVoyage.Factory;

		#endregion

		#region IEDIFACTMessageAttachee

		void IEDIFACTMessageAttachee.AddMessage(EDIMessage message)
		{
			jobVoyage.Messages.Add(message);
		}

		ZString IEDIFACTMessageAttachee.MessageStatus { get; set; } = ZString.Empty;

		ZString IEDIFACTMessageAttachee.JobStatus { get; set; } = ZString.Empty;

		bool IEDIFACTMessageAttachee.HasChanges => jobVoyage.HasChanges;

		ZString IEDIFACTMessageAttachee.JobIdentification => ZString.Empty;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => jobVoyage;

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		#endregion

		readonly JobVoyage jobVoyage;
		ZString messageSender;
		ICALINFTransportInformation calinfTransport;
	}
}
