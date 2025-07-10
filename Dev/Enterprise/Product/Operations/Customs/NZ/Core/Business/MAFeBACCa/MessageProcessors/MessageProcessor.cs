using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors
{
	abstract class MessageProcessor : Messaging.MessageProcessors.CustomsMessageProcessor
	{
		protected MessageProcessor(LoggingInformation logger, string messageFriendlyName)
			: base(logger, NZMMessage.ApplicationCodes.NewZealandMAFeBACCa, messageFriendlyName)
		{
		}

		protected NZMMessage Message
		{
			get;
			private set;
		}

		protected NZMMessage OriginalMessage
		{
			get;
			set;
		}

		protected BusinessObjectFactory Factory
		{
			get;
			private set;
		}

		GlbBranch CurrentBranch
		{
			get { return Message != null ? (Message.Branch ?? GlbBranch.CurrentBranch) : GlbBranch.CurrentBranch; }
		}

		protected ZStringBuilder EmailBody
		{
			get;
			private set;
		}

		protected ZString responseTypeDescription;
		protected ZString jobNumber;

		protected sealed override string DoProcessingReturningStatus(Messaging.Business.EDIMessage message)
		{
			responseTypeDescription = "unknown";
			jobNumber = "unknown";
			EmailBody = new ZStringBuilder();

			string result = NZMMessage.Status.Failed;
			try
			{
				Message = (NZMMessage)message;
				Message.EM_MessageText = FormatXml(Message.EM_MessageText);
				Factory = Message.Factory;
				result = DoProcessingReturningStatus();
			}
			catch (MessageProcessingException e)
			{
				responseTypeDescription = "FATAL ERROR PROCESSING";
				EmailBody.Append("Exception Thrown processing response message:-");
				EmailBody.Append(message.EM_MessageText);
				EmailBody.Append("");
				EmailBody.Append(e.ToString());

				if (e.ShouldSendDeveloperInformation)
				{
					ErrorReporter.ReportOnce("Exception occurred processing eBACCa response. Message marked as failed.", e);
				}

				Logger.LogError("Exception Thrown processing response message:-");
				Logger.LogError(e.ToString());
			}

			ZString emailSubject = "[" + responseTypeDescription + "] Response for eBACCa sent from: " + jobNumber;
			ZString emailBody = EmailBody.ToStringWithNewLineBetweenAppends();

			message.EM_MessageInterpretation = emailSubject + "\r\n" + "-".PadRight(emailSubject.Length, '-') + "\r\n" + emailBody;

			EmailDef email = new EmailDef();
			email.Subject = emailSubject;
			email.Body = emailBody;
			foreach (IeDoc eDoc in message.DocManagerInfo.AllEDocs)
			{
				email.Attachments.Add(new AttachmentDef(eDoc.FileName, eDoc.ImageData));
				//TODO: Test to make sure the PDF's are then readable out of eDocs and the sent emails.
				//Sent emails will probably be fine, it's eDocs that will likely be a problem.
			}

			switch (result)
			{
				case NZMMessage.Status.Failed:
					SendErrorReport(message.EM_LinkedObject, email);
					break;

				case NZMMessage.Status.Received:
					switch (message.EM_MessageSubType)
					{
						case NZMMessage.MessageTypes.Receive.MessageSubTypes.Acknowledgement:
						case NZMMessage.MessageTypes.Receive.MessageSubTypes.NotifyCRN:
							SendAcknowledgementReport(message.EM_LinkedObject, email);
							break;

						case NZMMessage.MessageTypes.Receive.MessageSubTypes.Cancellation:
						case NZMMessage.MessageTypes.Receive.MessageSubTypes.RequestMoreInfo:
							SendImpedimentReport(message.EM_LinkedObject, email);
							break;

						default:
							SendErrorReport(message.EM_LinkedObject, email);
							break;
					}
					break;
			}

			return result;
		}

		/// <summary>
		/// Formats the supplied XML string with proper indentation using single tabs and carriage returns where appropriate.
		/// </summary>
		string FormatXml(string unformattedXml)
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(unformattedXml);

				using (StringWriter stringWriter = new StringWriter(stringBuilder))
				using (XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter))
				{
					xmlWriter.Formatting = Formatting.Indented;
					xmlWriter.IndentChar = '\t';
					xmlWriter.Indentation = 1;
					xmlDocument.WriteTo(xmlWriter);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return unformattedXml;
			}
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Should be implemented to completely process the message linking it to the appropriate parent BO, 
		/// update the status on the parent BO, and returning the Status Code to be applied against the message.
		/// Should not save to the factory at all. That's done in the base class so that if the save fails, 
		/// it can be handled and logged/retried appropriately.
		/// </summary>
		protected abstract string DoProcessingReturningStatus();

		public abstract bool CanProcess(string xmlMessageType);

		#region Response Email Registry Getters
		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.MAFeBACCaSendAcknowledgementsToGroup.GetFallBackValueAtAllLevels(CurrentBranch.Company.PK.ToGuid(), CurrentBranch.PK.ToGuid(), Guid.Empty); }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.MAFeBACCaSendAcknowledgements.GetFallBackValueAtAllLevels(CurrentBranch.Company.PK.ToGuid(), CurrentBranch.PK.ToGuid(), Guid.Empty); }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.MAFeBACCaSendErrorsToGroup.GetFallBackValueAtAllLevels(CurrentBranch.Company.PK.ToGuid(), CurrentBranch.PK.ToGuid(), Guid.Empty); }
		}

		protected override ZString ErrorEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.MAFeBACCaSendErrors.GetFallBackValueAtAllLevels(CurrentBranch.Company.PK.ToGuid(), CurrentBranch.PK.ToGuid(), Guid.Empty); }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.MAFeBACCaSendImpedimentsToGroup.GetFallBackValueAtAllLevels(CurrentBranch.Company.PK.ToGuid(), CurrentBranch.PK.ToGuid(), Guid.Empty); }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.MAFeBACCaSendImpediments.GetFallBackValueAtAllLevels(CurrentBranch.Company.PK.ToGuid(), CurrentBranch.PK.ToGuid(), Guid.Empty); }
		}
		#endregion

		/// <summary>
		/// Finds the Original Message being responded to so that we can get the Parent BO from that message.
		/// </summary>
		protected NZMMessage GetOriginalMessage(ZString messageNo, ZString brokerageID)
		{
			if (messageNo.IsEmpty)
			{
				throw new MessageProcessingException("Could not find Message No in XML Message. (CallerRefID)", Message, false, true);
			}
			if (brokerageID.IsEmpty)
			{
				throw new MessageProcessingException("Could not find Brokerage ID in XML Message. (CallerRefID)", Message, false, true);
			}
			CompanyFinder companyFinder = new CompanyFinder();
			GlbCompany company = companyFinder.FindFromBrokerageID(brokerageID);
			if (company != null)
			{
				List<ZGuid> branchPKs = new List<ZGuid>();
				foreach (GlbBranch branch in company.Branches)
				{
					branchPKs.Add(branch.PK);
				}
				ZQuery messageFilter = new ZQuery();
				messageFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, NZMMessage.Direction.Transmit);
				messageFilter.AddToFilter(EDIMessageSchema.EM_GB, branchPKs);
				messageFilter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, NZMMessage.ApplicationCodes.NewZealandMAFeBACCa);
				messageFilter.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNo);
				messageFilter.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";

				return Factory.LoadTop1<NZMMessage>(messageFilter)
					?? throw new MessageProcessingException("Could not find original message sent with Brokerage ID [" + brokerageID + "] and Message No [" + messageNo + "].", Message, true, false);
			}
			throw new MessageProcessingException("Could not find Company with Brokerage ID [" + brokerageID + "].", Message, true, false);
		}

		protected override GlbStaff GetUserToNotify(IBusiness parent)
		{
			return OriginalMessage?.UserWhoQueuedThisRecord ?? base.GetUserToNotify(parent);
		}
	}
}
