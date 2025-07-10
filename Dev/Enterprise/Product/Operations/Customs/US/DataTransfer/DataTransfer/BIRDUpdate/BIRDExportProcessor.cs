using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer
{
	/// <summary>
	/// This class is responsible for scanning the EDIMessage table and transmitting any pending BIRD EDIMessages to its job's external broker.
	/// </summary>
	public class BIRDExportProcessor
	{
		public void Execute(INotifications notifications)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			MQEDIMessage[] messages = GetAllMessagesToProcess(factory);

			foreach (MQEDIMessage message in messages)
			{
				if (ProcessAndTransmitBIRDMessage(message, notifications))
				{
					message.EM_Status = EDIMessage.Status.Sent;
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Failed;
				}
			}

			if (messages.Length > 0)
			{
				try
				{
					factory.Save();
				}
				catch (ZSaveException exception)
				{
					ZExceptionReporting.HandleSaveException(exception);
				}
			}
		}

		MQEDIMessage[] GetAllMessagesToProcess(BusinessObjectFactory factory)
		{
			ZQuery query = new ZQuery(EDIMessageSchema.EM_Status, EDIMessage.Status.Pending);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.GetBIRDApplicationIdentifierCodes());
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + ", " + EDIMessageSchema.EM_MessageNum.Name;
			query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum));

			return factory.Load<MQEDIMessage>(query);
		}

		bool ProcessAndTransmitBIRDMessage(MQEDIMessage message, INotifications notifications)
		{
			bool processedSuccessfully = false;

			IMessageAttacheeInDeclaration msgAttachee = message.EM_LinkedObject as IMessageAttacheeInDeclaration;

			if (msgAttachee == null)
			{
				notifications.AddWarning("This message is linked to a record that is not related to Customs Declaration. To get the recipient details, a message should be linked to a record that is related to Customs Declarations. This is linked to " + message.EM_LinkTable);
			}
			else
			{
				JobDeclaration declaration = message.Factory.Load<JobDeclaration>(msgAttachee.DeclarationPK);

				if (declaration == null)
				{
					notifications.AddWarning("This message is linked to a Customs Declaration related record, however no declaration exists with this PK, " + msgAttachee.DeclarationPK);
				}
				else
				{
					byte[] messageToSend = UTF8Encoding.ASCII.GetBytes(message.EM_FormattedMessageText);

					string abiApplicationID = GetBIRDApplicationID(message.EM_MessageSubType);

					EDIMessageDelivery delievery = new EDIMessageDelivery(declaration.JE_DeclarationReference);
					delievery.ExtraDataSubstitution = (CommunicationModeSubstitutorProperty _, ZString fileName) => fileName.Replace(EDIMessageDelivery.ReplacementConstants.Type, abiApplicationID);

					OrgHeader receipient = declaration.ExternalBroker;

					if (receipient != null)
					{
						var deliveryContext = new DeliveryContext(declaration.Factory)
						{
							ParentInfo = EntityInfo.New(declaration)
						};
						var modes = receipient.EDICommunicationsModes.FindByModule(EDICommunicationsMode.Modules.US_BIRD);

						var deliveryResults = new List<IDeliveryResult>();
						foreach (var mode in modes)
						{
							using (var stream = (SubStreamableStream)new MemoryStream(messageToSend))
							{
								deliveryResults.Add(delievery.Deliver(deliveryContext, mode, new DeliveryStreamWrapperUXML(stream, deliveryContext.ParentInfo)));
							}
						}

						processedSuccessfully = deliveryResults.Any(d => d.CanSave);

						if (modes.Length == 0)
						{
							notifications.AddWarning(string.Format("No EDI Communication Mode is specified against the external broker, {0} on a declaration, {1}.", declaration.ExternalBroker.OH_Code, declaration.JE_DeclarationReference));
						}
					}
					else
					{
						notifications.AddWarning(string.Format("No external broker is entered on this declaration, {0}.", declaration.JE_DeclarationReference));
					}
				}
			}

			return processedSuccessfully;
		}

		string GetBIRDApplicationID(string messageSubType)
		{
			switch (messageSubType)
			{
				case EM_MessageSubTypeList.Codes.BIRDCargoRelease:
					return BIRDApplicationCodeList.Codes.CargoRelease;

				case EM_MessageSubTypeList.Codes.BIRDEntrySummary:
					return BIRDApplicationCodeList.Codes.EntrySummary;

				case EM_MessageSubTypeList.Codes.BIRDEntrySummaryQueryInput:
					return BIRDApplicationCodeList.Codes.EntrySummaryQueryInput;

				case EM_MessageSubTypeList.Codes.BIRDEntrySummaryQueryOutput:
					return BIRDApplicationCodeList.Codes.EntrySummaryQueryOutput;

				case EM_MessageSubTypeList.Codes.BIRDLiquidationNotice:
					return BIRDApplicationCodeList.Codes.CourtesyNoticeOfLiquidation;

				case EM_MessageSubTypeList.Codes.BIRDStatusRecords:
					return BIRDApplicationCodeList.Codes.Status;

				default:
					return "";
			}
		}
	}
}
