using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Messages.MEDPID;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors
{
	class CrewOrEquipmentRegistrationMessageProcessor : ResponseMessageProcessor
	{
		public CrewOrEquipmentRegistrationMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			var resultStatus = ZString.Empty;
			var message = (EDIMessage)ediMessage;
			var medpid = message.GetAutoEdifactMessageUsingNamedFactory(new eManifestMessageFactory(), new UNOACharacterSet()) as MEDPIDMessage;
			if (medpid != null)
			{
				SetMessageNumberAndTypeAndDefineStatusCalculator(message, medpid);
				SetLinkedObjectAndItsReference(message);
				if (responseWrapper.IsValidResponse)
				{
					if (linkedObject != null)
					{
						using (DisposableEnvironment.ForBranch(SetMessageBranchReturningCode(message)))
						{
							message.EM_MessageSubType = statusCalculator.GetMessageSubType(linkedObject.MessageStatus);

							if (responseWrapper.IsAcceptedResponse)
							{
								var email = GetAcceptedResponseEmailAndSetOnMessage(message);
								linkedObject.MessageStatus = statusCalculator.GetMessageClearedStatus(message);
								message.EM_MessageSubType = EntryStatusList.Codes.Clear;
								SendAcknowledgementReport(EmailResponseLinkedObject, email);
								SetACEIdsOnRelatedObjects();
							}
							else if (responseWrapper.IsErrorResponse)
							{
								var email = GetErrorResponseEmailAndSetOnMessage(message);
								if (statusCalculator.IsAwaitingReply(linkedObject.MessageStatus))
								{
									linkedObject.MessageStatus = statusCalculator.GetMessageRejectedStatus(message);
								}
								message.EM_MessageSubType = EntryStatusList.Codes.Error;
								SendErrorReport(EmailResponseLinkedObject, email);
							}
							resultStatus = Enterprise.Messaging.Business.EDIMessage.Status.Received;
						}
					}
					else
					{
						throw new CouldNotFindAssociatedTransmitMessageException(message, this);
					}
				}
			}

			if (resultStatus.IsEmpty)
			{
				throw new UnableToInterpretMessageException(message, this);
			}
			return resultStatus;
		}

		#region Set Message Number/Type/Linked Object/Branch/ACEId

		void SetMessageNumberAndTypeAndDefineStatusCalculator(EDIMessage message, MEDPIDMessage medpid)
		{
			responseWrapper = new CrewOrEquipmentRegistrationMessageWrapper(message.Factory, medpid);
			message.EM_MessageType = MessageTypes.Codes.CrewOrEquipmentRegistration;

			originalMessage = GetOriginalMessage(message);
			if (originalMessage != null)
			{
				message.EM_MessageNum = originalMessage.EM_MessageNum;
			}

			statusCalculator = new eManifestStatusCalculator(message.EM_MessageType);
		}

		EDIMessage GetOriginalMessage(EDIMessage responseMessage)
		{
			var query = GetMessagesWithin7DaysQuery();
			var subQuery = new ZDBOnlySubQuery(typeof(Trip), CusInBondHeaderSchema.PK);
			subQuery.AddToFilter(CusInBondHeaderSchema.BH_GB, Env.CurrentBranch.PK);
			subQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, EDIMessage.ApplicationCodes.USeManifest);
			subQuery.AddToFilter(CusInBondHeaderSchema.BH_ReleaseStatus, SQLComparisonOperator.StartsWith, "A");
			query.AddSubQuery(EDIMessageSchema.EM_LinkUniqueID, subQuery, JoinCondition.And);

			var factory = responseMessage.Factory;
			return factory.Load<EDIMessage>(query).FirstOrDefault(m => DataMatches(factory, m))
				   ?? factory.Load<EDIMessage>(GetMessagesWithin7DaysQuery()).FirstOrDefault(m => DataMatches(factory, m));
		}

		static ZDBOnlyQuery GetMessagesWithin7DaysQuery()
		{
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USeManifest);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypes.Codes.CrewOrEquipmentRegistration);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddDays(-7));
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";
			return query;
		}

		bool DataMatches(BusinessObjectFactory factory, EDIMessage message)
		{
			var medpid = message.GetAutoEdifactMessageUsingNamedFactory(new eManifestMessageFactory(), new UNOACharacterSet()) as MEDPIDMessage;
			if (medpid != null)
			{
				var originalWrapper = new CrewOrEquipmentRegistrationMessageWrapper(factory, medpid);
				return originalWrapper.TransmissionReferenceNumber == responseWrapper.TransmissionReferenceNumber;
			}
			return false;
		}

		void SetLinkedObjectAndItsReference(EDIMessage message)
		{
			if (originalMessage != null)
			{
				var trip = (Trip)originalMessage.EM_LinkedObject;
				linkedObject = trip == null ? null : new eManifestMessageWrapper(trip, message.EM_MessageType);
			}
			if (linkedObject != null)
			{
				linkedObjectReference = linkedObject.JobIdentification;
				linkedObject.Messages.Add(message);
			}
		}

		Guid SetMessageBranchReturningCode(EDIMessage message)
		{
			GlbBranch branch = null;
			if (originalMessage != null)
			{
				branch = originalMessage.Branch;
			}

			if (branch == null)
			{
				var lastSendMessage = linkedObject.Messages.GetLastMessage(message.EM_ApplicationCode, message.EM_MessageType, EDIMessage.Direction.Transmit);
				if (lastSendMessage != null)
				{
					branch = lastSendMessage.Branch;
				}
			}
			branch = branch ?? message.Branch ?? GlbBranch.CurrentBranch;
			message.EM_GB = branch.PK;
			return branch.PK.ToGuid();
		}

		void SetACEIdsOnRelatedObjects()
		{
			if (linkedObject is eManifestMessageWrapper attachee)
			{
				foreach (var crewMember in responseWrapper.CrewMembers)
				{
					attachee.SetACEId(crewMember.InfoType, crewMember.UniqueKey, crewMember.ACEId);
				}
			}
		}

		#endregion

		#region Emails

		#region GetAcceptedResponseEmailAndSetOnMessage

		protected override StringBuilder GetReportedData()
		{
			const string dataDescription = " Reported Data";

			var length = 0;
			var result = new StringBuilder();
			Action appendLineSeparatorFunc = () =>
			{
				if ((result.Length - length) > 0)
				{
					result.Append("<hr />");
					length = result.Length;
				}
			};

			foreach (var equipment in responseWrapper.Equipment)
			{
				result.Append(FieldValueTableInterpretation.GetTableInterpretation(equipment, equipment.InfoDescription + dataDescription));
				appendLineSeparatorFunc();
			}

			foreach (var crewMember in responseWrapper.CrewMembers)
			{
				result.Append(FieldValueTableInterpretation.GetTableInterpretation(crewMember, "Crew Member" + dataDescription));

				foreach (var travelDocument in crewMember.TravelDocuments)
				{
					result.Append(FieldValueTableInterpretation.GetTableInterpretation(travelDocument, "Crew Member Travel Document" + dataDescription));
				}

				appendLineSeparatorFunc();
			}

			return result;
		}

		#endregion GetAcceptedResponseEmailAndSetOnMessage

		#region GetErrorResponseEmailAndSetOnMessage

		protected override string GetNotificationsText()
		{
			var result = new StringBuilder();
			foreach (var equipment in responseWrapper.Equipment)
			{
				result.Append(TableInterpretation.GetTableInterpretation(equipment.Notifications, null, TableInterpretation.Attributes.AlignLeft));
			}
			foreach (var crewMember in responseWrapper.CrewMembers)
			{
				result.Append(TableInterpretation.GetTableInterpretation(crewMember.Notifications, null, TableInterpretation.Attributes.AlignLeft));
			}
			return result.ToString();
		}

		#endregion GetErrorResponseEmailAndSetOnMessage

		#endregion Emails

		CrewOrEquipmentRegistrationMessageWrapper responseWrapper;
	}
}
