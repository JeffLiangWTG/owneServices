using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Messages.CUSRES;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors
{
	class eManifestResponseMessageProcessor : ResponseMessageProcessor
	{
		public eManifestResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			var resultStatus = ZString.Empty;
			var message = (EDIMessage)ediMessage;
			var messageText = message.EM_MessageText;
			var unoaCharSet = new UNOACharacterSet();

			message.EM_MessageText = InsertDocSegmentIfMissing(messageText, unoaCharSet);

			var cusresMessage = message.GetAutoEdifactMessageUsingNamedFactory(new eManifestMessageFactory(), unoaCharSet) as CUSRESMessage;
			if (cusresMessage != null)
			{
				SetMessageNumberAndTypeAndDefineStatusCalculator(message, cusresMessage);
				SetLinkedObjectAndItsReference(message);
				if (linkedObject != null)
				{
					if (responseWrapper.IsValidResponse)
					{
						using (DisposableEnvironment.ForBranch(SetMessageBranchReturningCode(message)))
						{
							message.EM_MessageSubType = statusCalculator.GetMessageSubType(linkedObject.MessageStatus);
							if (responseWrapper.IsStatusUpdate)
							{
								var email = GetStatusUpdateEmailAndSetOnMessage(message);
								message.EM_MessageSubType = statusCalculator.GetStatusUpdateMessageSubType(responseWrapper);
								statusCalculator.UpdateReleaseStatus(linkedObject, responseWrapper, originalWrapper);
								SendAcknowledgementReport(EmailResponseLinkedObject, email);
							}
							else if (originalMessage != null)
							{
								var originalMessagePendings = GetOriginalMessage(originalMessage, linkedObject);
								if (responseWrapper.IsAcceptedResponse)
								{
									statusCalculator.UpdateReleaseStatus(linkedObject, responseWrapper, originalWrapper);
									var resetQueSucessed = false;
									var originalMessagePending = originalMessagePendings.FirstOrDefault();
									if (originalMessagePending != null)
									{
										originalMessagePending.EM_Status = EDIMessage.Status.Queued;
										if (originalMessage.EM_LinkedObject is Trip trip)
										{
											if (originalMessagePending.EM_MessageText == eManifestMessageManagerHelper.MessagePlaceHolder)
											{
												var newWrapper = new eManifestMessageWrapper(trip, originalMessagePending.EM_MessageType);
												trip.ResetShipmentsActionsIfMessageTypeChanged(originalMessagePending.EM_MessageType);
												var actionCode = originalMessagePending.EM_MessageSubType == MessageActionCodes.Codes.Confirmation ? MessageSubTypes.Confirmation : MessageSubTypes.Create;
												var builder = eManifestMessageManagerHelper.GetMessageBuilder(newWrapper, originalMessagePending.EM_MessageType, actionCode, false);
												if (builder != null)
												{
													builder.GenerateMessageContent(originalMessagePending);
													resetQueSucessed = true;
												}
											}
											else
											{
												resetQueSucessed = true;
											}
										}

										if (!resetQueSucessed)
										{
											originalMessagePendings.ForEach(x =>
											{
												x.IsCancelled = true;
												x.EM_Status = EDIMessage.Status.Discarded;
											});
										}
									}
									var email = GetAcceptedResponseEmailAndSetOnMessage(message);
									var messageStatus = ZString.Empty;
									if (originalMessagePending != null && originalMessagePendings.Any())
									{
										messageStatus = statusCalculator.GetMessageAwaitingStatus(originalMessagePending);
									}
									else
									{
										messageStatus = statusCalculator.GetMessageClearedStatus(message);
									}
									linkedObject.MessageStatus = messageStatus;
									message.EM_MessageSubType = statusCalculator.GetAcceptedMessageSubType(originalWrapper);
									SendAcknowledgementReport(EmailResponseLinkedObject, email);
								}
								else if (responseWrapper.IsErrorResponse)
								{
									originalMessagePendings.ForEach(x =>
									{
										x.IsCancelled = true;
										x.EM_Status = EDIMessage.Status.Discarded;
									});
									var email = GetErrorResponseEmailAndSetOnMessage(message);
									if (statusCalculator.IsAwaitingReply(linkedObject.MessageStatus) || IsOneOfNoACEIDMessages(linkedObject, originalMessage))
									{
										linkedObject.MessageStatus = statusCalculator.GetMessageRejectedStatus(message);
									}
									message.EM_MessageSubType = statusCalculator.GetErrorMessageSubType(responseWrapper);
									statusCalculator.UpdateReleaseStatus(linkedObject, responseWrapper, originalWrapper);
									SendErrorReport(EmailResponseLinkedObject, email);
								}
							}
							else
							{
								throw new CouldNotFindAssociatedTransmitMessageException(message, this);
							}
							statusCalculator.SetJobStatus(linkedObject, originalWrapper);
							resultStatus = Enterprise.Messaging.Business.EDIMessage.Status.Received;
						}
					}
				}
				else
				{
					throw new CouldNotFindLinkedObjectException(linkedObjectReference, message, this);
				}
			}

			if (resultStatus.IsEmpty)
			{
				throw new UnableToInterpretMessageException(message, this);
			}
			return resultStatus;
		}

		ZBool IsOneOfNoACEIDMessages(IEDIFACTMessageAttachee trip, EDIMessage origMessage)
		{
			var messageTypes = new[] { MessageTypes.Codes.UnassociatedShipments, MessageTypes.Codes.PreliminaryTrip, MessageTypes.Codes.CrewAndPassenger };

			return !origMessage.EM_ApplicationReference.IsEmpty && trip.Messages.OfType<EDIMessage>().Any((x) =>
			{
				return x.EM_ApplicationCode.EqualsIgnoringCase(EDIMessage.ApplicationCodes.USeManifest)
					&& x.EM_ReceiveTransmit.EqualsIgnoringCase(EDIMessage.Direction.Transmit)
					&& x.EM_ApplicationReference.EqualsIgnoringCase(origMessage.EM_ApplicationReference)
					&& messageTypes.Any(t => x.EM_MessageType.EqualsIgnoringCase(t))
					&& x.PK != origMessage.PK;
			});
		}

		IEnumerable<Enterprise.Messaging.Business.EDIMessage> GetOriginalMessage(EDIMessage origMessage, IEDIFACTMessageAttachee trip)
		{
			IEnumerable<Enterprise.Messaging.Business.EDIMessage> result = null;
			var applicationReference = origMessage.EM_ApplicationReference;
			if (!applicationReference.IsEmpty)
			{
				Tuple<ZString, ZString>[] messageTypesActions = null;
				switch (origMessage.EM_MessageType)
				{
					case MessageTypes.Codes.UnassociatedShipments:
						messageTypesActions = new Tuple<ZString, ZString>[]
						{
							new Tuple<ZString, ZString>(MessageTypes.Codes.PreliminaryTrip, MessageActionCodes.Codes.Original),
							new Tuple<ZString, ZString>(MessageTypes.Codes.CrewAndPassenger, MessageActionCodes.Codes.Original),
							new Tuple<ZString, ZString>(MessageTypes.Codes.PreliminaryTrip, MessageActionCodes.Codes.Confirmation),
						};
						break;
					case MessageTypes.Codes.PreliminaryTrip:
						messageTypesActions = new Tuple<ZString, ZString>[]
						{
							new Tuple<ZString, ZString>(MessageTypes.Codes.CrewAndPassenger, MessageActionCodes.Codes.Original),
							new Tuple<ZString, ZString>(MessageTypes.Codes.PreliminaryTrip, MessageActionCodes.Codes.Confirmation),
						};
						break;
					case MessageTypes.Codes.CrewAndPassenger:
						messageTypesActions = new Tuple<ZString, ZString>[]
						{
							new Tuple<ZString, ZString>(MessageTypes.Codes.PreliminaryTrip, MessageActionCodes.Codes.Confirmation),
						};
						break;
					case MessageTypes.Codes.eManifest:
						messageTypesActions = new Tuple<ZString, ZString>[]
						{
							new Tuple<ZString, ZString>(MessageTypes.Codes.eManifest, MessageActionCodes.Codes.Change),
						};
						break;
					default:
						messageTypesActions = Array.Empty<Tuple<ZString, ZString>>();
						break;
				}

				if (messageTypesActions.Length > 0)
				{
					var createTimeTimeUtc = origMessage.EM_SystemCreateTimeUtc;
					result = trip.Messages.Find((x) =>
					{
						return x.EM_ApplicationCode.EqualsIgnoringCase(EDIMessage.ApplicationCodes.USeManifest)
							&& x.EM_ReceiveTransmit.EqualsIgnoringCase(EDIMessage.Direction.Transmit)
							&& x.EM_ApplicationReference.EqualsIgnoringCase(applicationReference)
							&& x.EM_SystemCreateTimeUtc >= createTimeTimeUtc
							&& x.IsPending
							&& messageTypesActions.Any(t => x.EM_MessageType.EqualsIgnoringCase(t.Item1) && x.EM_MessageSubType.EqualsIgnoringCase(t.Item2));
					}).OrderBy(x => x.EM_SystemCreateTimeUtc).ThenBy(x => x.EM_MessageNum);
				}
			}
			return result ?? Enumerable.Empty<Enterprise.Messaging.Business.EDIMessage>();
		}

		ZString InsertDocSegmentIfMissing(ZString messageText, UNOACharacterSet unoaCharSet)
		{
			//This is because US Customs have a bug where they are missing the DOC segment which they have said they will fix.
			if (!messageText.IsEmpty)
			{
				var pacSegIndex = messageText.IndexOf(string.Concat(unoaCharSet.SegmentDelimiterChar, "PAC", unoaCharSet.ElementDelimiterChar), StringComparison.Ordinal);
				var docSegIndex = messageText.IndexOf(string.Concat(unoaCharSet.SegmentDelimiterChar, "DOC", unoaCharSet.ElementDelimiterChar), StringComparison.Ordinal);

				if (pacSegIndex >= 0 && docSegIndex == -1)
				{
					messageText = messageText.Insert(pacSegIndex, string.Concat(unoaCharSet.SegmentDelimiterChar + "DOC" + unoaCharSet.ElementDelimiterChar + "ZZZ"));
				}
			}
			return messageText;
		}

		#region Set Message Number/Type/Linked Object/Branch

		void SetMessageNumberAndTypeAndDefineStatusCalculator(EDIMessage message, CUSRESMessage cusresMessage)
		{
			responseWrapper = new eManifestResponseMessageWrapper(message.Factory, cusresMessage);
			var reference = responseWrapper.TransmissionReferenceNumber;
			if (reference.Length >= 3)
			{
				message.EM_MessageNum = reference.SubstringSafe(3);
				message.EM_MessageType = reference.Left(3);
			}
			message.EM_MessageOwner = responseWrapper.TripReference.SubstringSafe(4, 10);
			statusCalculator = new eManifestStatusCalculator(responseWrapper.IsStatusUpdate ? (ZString)MessageTypes.Codes.eManifest : message.EM_MessageType);
		}

		void SetLinkedObjectAndItsReference(EDIMessage message)
		{
			Trip trip = null;
			originalMessage = message.OriginalMessage;
			originalWrapper = new eManifestOriginalMessageWrapper(originalMessage);
			if (originalMessage != null)
			{
				trip = originalMessage.EM_LinkedObject as Trip;
			}

			linkedObjectReference = responseWrapper.TripReference;
			if (trip == null && !linkedObjectReference.IsEmpty)
			{
				var query = new ZQuery(CusInBondHeaderSchema.BH_VoyageNumber, linkedObjectReference.SubstringSafe(4, 10));
				query.AddToFilter(CusInBondHeaderSchema.BH_CarrierSCAC, linkedObjectReference.Left(4));
				query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.eManifest);

				var trips = message.Factory.Load<Trip>(query);
				if (trips.Length == 1)
				{
					trip = trips[0];
				}
				else if (trips.Length > 1)
				{
					trip = trips.Where(t => t.BH_MessageStatus == MessageStatusList.Codes.AwaitingOriginal || t.BH_MessageStatus == MessageStatusList.Codes.AwaitingChange).OrderByDescending(t => t.LatestSentMessageDateTime).FirstOrDefault();
				}
			}

			linkedObject = trip == null ? null : new eManifestMessageWrapper(trip, message.EM_MessageType);
			if (linkedObject != null)
			{
				if (linkedObjectReference.IsEmpty)
				{
					linkedObjectReference = linkedObject.JobIdentification;
				}

				linkedObject.Messages.Add(message);
			}
		}

		Guid SetMessageBranchReturningCode(EDIMessage message)
		{
			GlbBranch branch = null;
			if (originalMessage == null)
			{
				originalMessage = message.OriginalMessage;
				originalWrapper = new eManifestOriginalMessageWrapper(originalMessage);
			}
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

		#endregion

		#region Emails

		#region GetAcceptedResponseEmailAndSetOnMessage

		protected override StringBuilder GetReportedData()
		{
			var dataDescription = responseWrapper.DataDescription;

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

			#region Trip

			result.Append(FieldValueTableInterpretation.GetTableInterpretation(responseWrapper, "Trip" + dataDescription));
			appendLineSeparatorFunc();

			#endregion

			#region Conveyance

			var conveyance = responseWrapper.Conveyance;
			result.Append(FieldValueTableInterpretation.GetTableInterpretation(conveyance, "Conveyance" + dataDescription));
			foreach (var licensePlate in conveyance.LicensePlates)
			{
				result.Append(FieldValueTableInterpretation.GetTableInterpretation(licensePlate, "Conveyance License Plate" + dataDescription));
			}
			appendLineSeparatorFunc();

			#endregion

			#region Equipment

			foreach (var equipment in responseWrapper.Equipment)
			{
				result.Append(FieldValueTableInterpretation.GetTableInterpretation(equipment, "Equipment" + dataDescription));
				foreach (var licensePlate in equipment.LicensePlates)
				{
					result.Append(FieldValueTableInterpretation.GetTableInterpretation(licensePlate, "Equipment License Plate" + dataDescription));
				}
				appendLineSeparatorFunc();
			}

			#endregion

			#region Crew Members

			foreach (var crewMember in responseWrapper.CrewMembers)
			{
				result.Append(FieldValueTableInterpretation.GetTableInterpretation(crewMember, "Crew Member/Passenger" + dataDescription));
				result.Append(FieldValueTableInterpretation.GetTableInterpretation(crewMember.TravelDocument, "Crew Member/Passenger Travel Document" + dataDescription));
				result.Append(FieldValueTableInterpretation.GetTableInterpretation(crewMember.USAddress, "Crew Member/Passenger US Address" + dataDescription));
				appendLineSeparatorFunc();
			}

			#endregion

			#region Shipments

			foreach (var shipment in responseWrapper.Shipments)
			{
				result.Append(FieldValueTableInterpretation.GetTableInterpretation(shipment, "Shipment" + dataDescription));

				foreach (var party in shipment.Parties)
				{
					result.Append(FieldValueTableInterpretation.GetTableInterpretation(party, "Party" + dataDescription));
				}

				result.Append(FieldValueTableInterpretation.GetTableInterpretation(shipment.Commodity, "Commodity" + dataDescription));
				result.Append(FieldValueTableInterpretation.GetTableInterpretation(shipment.InBond, "In-Bond" + dataDescription));
				appendLineSeparatorFunc();
			}

			#endregion

			return result;
		}

		#endregion GetAcceptedResponseEmailAndSetOnMessage

		#region GetStatusUpdateEmailAndSetOnMessage

		EmailDef GetStatusUpdateEmailAndSetOnMessage(EDIMessage message, string additionalComments = "")
		{
			var subject = string.Format("{0} Status Update Message for {1}", statusCalculator.MessageTypeDescription, linkedObjectReference);
			var emailBuilder = new EmailDefBuilder(subject, message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.StatusUpdate);
			emailBuilder.AddArgReplacementRange(GetJobLink(), linkedObjectReference, statusCalculator.MessageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetStatusUpdateMessageText(), true);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.AdditionalComments, additionalComments);
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		string GetStatusUpdateMessageText()
		{
			var result = GetReportedData();
			if (result.Length > 0)
			{
				result.Insert(0, "<br/><br/><p >The following data has been reported for the trip.</p><br/>");
			}
			result.Insert(0, GetNotificationsText());
			return result.ToString();
		}

		#endregion GetStatusUpdateEmailAndSetOnMessage

		#region GetErrorResponseEmailAndSetOnMessage

		protected override string GetNotificationsText()
		{
			var result = new StringBuilder(TableInterpretation.GetTableInterpretation(responseWrapper.Notifications, null, TableInterpretation.Attributes.AlignLeft));
			foreach (var shipment in responseWrapper.Shipments)
			{
				result.Append(TableInterpretation.GetTableInterpretation(shipment.Notifications, null, TableInterpretation.Attributes.AlignLeft));
			}
			return result.ToString();
		}

		#endregion GetErrorResponseEmailAndSetOnMessage

		#endregion Emails

		eManifestResponseMessageWrapper responseWrapper;
		eManifestOriginalMessageWrapper originalWrapper;
	}
}
