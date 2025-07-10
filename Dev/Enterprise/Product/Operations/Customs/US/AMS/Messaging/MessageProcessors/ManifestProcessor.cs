using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse, AMSEDIMessage.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, AMSEDIMessage.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, AMSEDIMessage.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, AMSEDIMessage.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse, US.Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, US.Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, US.Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, US.Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, US.Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, US.Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	class ManifestProcessor : AMSProcessor
	{
		bool isFailure;

		protected override void ProcessCore()
		{
			var emailBody = new ZStringBuilder();
			HtmlTableCreator tarw01Creator = null;
			HtmlTableCreator tarw02Creator = null;
			var carrierCode = ZString.Empty;
			var issuerCode = ZString.Empty;
			var billOfLading = ZString.Empty;
			var vesselName = ZString.Empty;
			var voyageNumber = ZString.Empty;
			var portOfUnlading = ZString.Empty;
			var estimatedDate = ZDate.Empty;
			var manifestSequenceNumber = ZString.Empty;
			var billWithErrorList = new List<ZString>();
			var messageNumber = ZString.Empty;
			PTTT01 firstPTTT01 = null;

			foreach (var block in messageBlocks)
			{
				var inpm01 = block as IINPM01;
				if (inpm01 != null)
				{
					carrierCode = inpm01.CarrierCode;
					manifestSequenceNumber = inpm01.ManifestSequenceNumber;
					vesselName = inpm01.VesselName;
					voyageNumber = inpm01.VoyageNumber;
				}
				else
				{
					var inpm02 = block as IINPM02;
					if (inpm02 != null)
					{
						billOfLading = CarrierAssignedBatchNumberCreator.GetBillOfLading(inpm02);
						messageNumber = CarrierAssignedBatchNumberCreator.GetMessageNumber(inpm02);
					}
					else
					{
						var inpp01 = block as IINPP01;
						if (inpp01 != null)
						{
							portOfUnlading = inpp01.PortOfUnlading;
							estimatedDate = inpp01.EstimatedDate;
						}
						else
						{
							var inpj01 = block as IINPJ01;
							if (inpj01 != null)
							{
								issuerCode = inpj01.IssuerCode;
							}
							else
							{
								var pttt01 = block as PTTT01;
								if (pttt01 != null)
								{
									if (firstPTTT01 == null)
									{
										firstPTTT01 = pttt01;
									}
									if (billOfLading.IsEmpty)
									{
										billOfLading = pttt01.BillOfLadingSequenceNumber;
									}
								}
								else
								{
									var tarw01 = block as TARW01;
									if (tarw01 != null)
									{
										if (billOfLading.IsEmpty)
										{
											billOfLading = tarw01.EntityNumber;
										}
										if (portOfUnlading.IsEmpty)
										{
											portOfUnlading = tarw01.CBPDistrictPort;
										}
										if (manifestSequenceNumber.IsEmpty)
										{
											manifestSequenceNumber = tarw01.ManifestSequenceNumber;
										}
										isFailure = true;
										if (tarw01Creator == null)
										{
											tarw01Creator = new HtmlTableCreator(GetTARW01ColumnTtles());
										}

										var messageError = tarw01.ErrorMessage;
										var comments = AppendCommentsForSpecificError(messageError);
										tarw01Creator.WriteRow(tarw01.EntityNumber, tarw01.CBPDistrictPort, tarw01.ManifestSequenceNumber, messageError, comments);
										if (!tarw01.EntityNumber.IsEmpty)
										{
											billWithErrorList.Add(tarw01.EntityNumber);
										}
									}
									else if (tarw02Creator == null)
									{
										var tarw02 = block as TARW02;
										if (tarw02 != null)
										{
											if (carrierCode.IsEmpty)
											{
												carrierCode = tarw02.CarrierCode;
											}
											tarw02Creator = new HtmlTableCreator(new[] { new CellWithFormatting("Transmission Summary", new NameValueCollection { { "colspan", "2" } }, true) });
											tarw02Creator.WriteRow("Carrier Code", tarw02.CarrierCode);
											tarw02Creator.WriteRow("Date of Transmission", tarw02.DateOfTransmission);
											tarw02Creator.WriteRow("Time of Transmission", tarw02.TimeOfTransmission);
											tarw02Creator.WriteRow("Total Manifests Read", tarw02.TotalManifestsRead);
											tarw02Creator.WriteRow("Total Ports Read", tarw02.TotalPortsRead);
											tarw02Creator.WriteRow("Total Bills Read", tarw02.TotalBillsRead);
											tarw02Creator.WriteRow("Total Amendments Read", tarw02.TotalAmendmentsRead);
											tarw02Creator.WriteRow("Total G01/H01 Records Input", tarw02.TotalG01H01RecordsInput);
											tarw02Creator.WriteRow("Total Bills Rejected", tarw02.TotalBillsRejected);
											tarw02Creator.WriteRow("Total Bills Accepted", tarw02.TotalBillsAccepted);
											tarw02Creator.WriteRow("Total Records Read", tarw02.TotalRecordsRead);
											tarw02Creator.WriteRow("Total Export Transaction Data Read", tarw02.TotalExportTransactionDataRead);
											tarw02Creator.WriteRow("Total Export Transaction Data Rejected", tarw02.TotalExportTransactionDataRejected);
											tarw02Creator.WriteRow("Total Export Transaction Data Accepted", tarw02.TotalExportTransactionDataAccepted);
										}
									}
								}
							}
						}
					}
				}
			}

			if (tarw01Creator != null)
			{
				emailBody.Append(tarw01Creator.ToHtml());
				emailBody.Append("<br>");
			}

			if (tarw02Creator != null)
			{
				emailBody.Append(tarw02Creator.ToHtml());
			}

			Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch = null;
			switch (Message.EM_MessageType)
			{
				// TODO Handle In-Bond
				case SubApplicationCodeList.Codes.PermitToTransfer:
					movementMatch = GetPTTMovementMatch(firstPTTT01, ZString.Empty);
					break;
				default:
					movementMatch = x => x.Item2.ApplicationCode == CusInBondApplicationCodeList.Codes.AMS && x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS;
					break;
			}

			var url = "";
			var jobNumberBuilder = new ZStringBuilder(billOfLading);
			jobNumberBuilder.AppendIfNotEmpty(messageNumber);
			var moveHeader = LinkToMessageAttacheeAndUpdateMessageSubType(carrierCode, manifestSequenceNumber, vesselName, voyageNumber, portOfUnlading, estimatedDate, issuerCode, billOfLading, ZString.Empty, ZString.Empty, movementMatch);
			if (moveHeader != null)
			{
				var textForPendingMessage = ProcessPendingMessages(moveHeader, isFailure);
				emailBody.Append(textForPendingMessage);

				if (!isFailure && !manifestSequenceNumber.IsEmpty && manifestSequenceNumber != moveHeader.ManifestSequenceNumber)
				{
					moveHeader.ManifestSequenceNumber = manifestSequenceNumber;
				}
				var jobNumber = moveHeader.JobNumber;
				if (!jobNumber.IsEmpty)
				{
					url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(moveHeader);
					jobNumberBuilder.Prepend(moveHeader.JobNumber);
				}
				moveHeader.UpdateIncomingBillStatus(issuerCode, billOfLading, Message.EM_MessageSubType, isFailure, Message);
				var branch = moveHeader.Branch;

				if (!isFailure && Message.OriginalMessage is CBPEDIMessage originalMessage && AMSMessageSubTypeList.IsVesselArrivalEventRelevent(originalMessage.EM_MessageSubType))
				{
					var h01Block = (ICMH01)originalMessage.MessageBlock.MessageBlocks.Find(delegate (MessageBlock block)
					{ return block is ICMH01; });
					if (h01Block != null)
					{
						var timeString = ConvertTimeString(h01Block.Time);
						var newEstimatedDateOfArrival = new ZDateTime(h01Block.Date.Year,
												h01Block.Date.Month,
												h01Block.Date.Day,
												ZInt.ParseSafe(timeString.Left(2), 0), ZInt.ParseSafe(timeString.Right(2), 0), 0);

						if (!newEstimatedDateOfArrival.IsEmpty)
						{
							var messageSubType = originalMessage.EM_MessageSubType;
							if (messageSubType == AMSMessageSubTypeList.Codes.ChangeEstimatedDateOfArrival)
							{
								moveHeader.UpdateEstimatedDateOfArrival(newEstimatedDateOfArrival);
							}

							var vesselArrival = moveHeader as IVesselArrivalMessageAttachee;
							if (vesselArrival != null)
							{
								vesselArrival.UpdateActualArrivalDate(portOfUnlading, newEstimatedDateOfArrival, messageSubType);
							}
						}
					}
				}

				var messageTypeInSubject = "Unknown";
				if (A != null)
				{
					messageTypeInSubject = GetMessageTypeInSubject(A.ApplicationIdentifier);
				}
				GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumberBuilder.ToStringWithDelimiterBetweenAppends(" - "), messageTypeInSubject, emailBody.ToString(), isFailure, branch, moveHeader as BusinessObject);
			}
			else
			{
				throw new MessageProcessDiscardedException($"No matched AMS job found for message {Message.EM_MessageNum}");
			}
		}

		public ZString ProcessPendingMessages(IMessageAttachee moveHeader, bool messageIsFailed)
		{
			const string PendingMessagesCancelledBody = "\r\n\r\nPlease Note: There were pending messages waiting for this message to be acknowledged before being transmitted.\r\nAs this message has been rejected, those pending messages have been canceled.";
			const string PendingMessagesQueuedBody = "\r\n\r\nPlease Note: There were pending messages waiting for this message to be acknowledged. Those messages have just been transmitted.";
			ZString result = "";

			if (moveHeader is IManifestPendingMessagesAttachee pendingMessageAttache)
			{
				if (!messageIsFailed)
				{
					var pendingMessages = pendingMessageAttache.PendingMessages.UpdateStatusOfPendingMessagesTo(EDIMessage.Status.Queued);
					if (pendingMessages.Length > 0)
					{
						result = PendingMessagesQueuedBody;
					}
				}
				else
				{
					var pendingMessages = pendingMessageAttache.PendingMessages.UpdateStatusOfPendingMessagesTo(EDIMessage.Status.Cancelled);
					if (pendingMessages.Length > 0)
					{
						result = PendingMessagesCancelledBody;
					}
				}
			}

			return result;
		}

		string GetMessageTypeInSubject(ZString applicationIdentifier)
		{
			string result = null;
			if (applicationIdentifier == AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse)
			{
				var originalMessage = Message.OriginalMessage;
				if (originalMessage != null)
				{
					var icmh01 = originalMessage.MessageBlock.MessageBlocks.OfType<ICMH01>().FirstOrDefault();
					if (icmh01 != null && !icmh01.MessageCode.IsEmpty)
					{
						result = Factory.GetCachedValue<InBondAndVesselEventMessageCodeList>().GetDescriptionFromCode(icmh01.MessageCode);
					}
				}
			}
			return result ?? ApplicationIdentifierCodeList.GetDescriptionFromCode(applicationIdentifier).Replace("Response", "").Trim();
		}

		ZString ConvertTimeString(ZString timeString)
		{
			var result = timeString;

			if (timeString.Length == 3)
			{
				timeString = "0" + timeString;
			}

			return result;
		}

		Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> GetPTTMovementMatch(PTTT01 firstPTTT01, ZString inBondNumber)
		{
			return x => x.Item2.ApplicationCode == "AMS" && x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer && GetPTTMovementMatch(x, firstPTTT01, x.Item3);
		}

		bool GetPTTMovementMatch(Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString> matchData, PTTT01 firstPTTT01, ZString inBondNumber)
		{
			return false;
		}

		string[] GetTARW01ColumnTtles()
		{
			return new string[] {
				"Entity Number",
				"CBP District/Port",
				"Manifest Sequence Number",
				"Error Message",
				"Comments"
			};
		}

		AMSApplicationIdentifierCodeList ApplicationIdentifierCodeList
		{
			get { return Factory.GetCachedValue<AMSApplicationIdentifierCodeList>(); }
		}

		string AppendCommentsForSpecificError(ZString errorMessage)
		{
			var comment = string.Empty;
			if (!errorMessage.IsEmpty && errorMessage.StartsWith("059"))
			{
				comment = "In the case that a bill is being added after vessel arrival, a Manifest Amendment should be filed.";
			}

			return comment;
		}

		protected override ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(EDIMessage message)
		{
			var jobNumber = ZString.Empty;
			var branchPK = message.EM_GB;
			if (QueryOriginMessage(message) is CBPEDIMessage originalMessage)
			{
				branchPK = originalMessage.EM_GB;
			}
			MultilingualString discardReason = (NoResString)string.Empty;
			var linkedObject = QueryLinkedObject(message);
			if (linkedObject is not IManifestMessageAttachee)
			{
				var carrierCode = ZString.Empty;
				var issuerCode = ZString.Empty;
				var billOfLading = ZString.Empty;
				var vesselName = ZString.Empty;
				var voyageNumber = ZString.Empty;
				var portOfUnlading = ZString.Empty;
				var estimatedDate = ZDate.Empty;
				var manifestSequenceNumber = ZString.Empty;
				PTTT01 firstPTTT01 = null;

				var amsMessage = message as AMSEDIMessage;

				foreach (var block in amsMessage.MessageBlock.MessageBlocks)
				{
					if (block is IINPM01 inpm01)
					{
						carrierCode = inpm01.CarrierCode;
						manifestSequenceNumber = inpm01.ManifestSequenceNumber;
						vesselName = inpm01.VesselName;
						voyageNumber = inpm01.VoyageNumber;
					}
					else if (block is IINPM02 inpm02)
					{
						billOfLading = CarrierAssignedBatchNumberCreator.GetBillOfLading(inpm02);
					}
					else if (block is IINPP01 inpp01)
					{
						portOfUnlading = inpp01.PortOfUnlading;
						estimatedDate = inpp01.EstimatedDate;
					}
					else if (block is IINPJ01 inpj01)
					{
						issuerCode = inpj01.IssuerCode;
					}
					else if (block is PTTT01 pttt01)
					{
						if (firstPTTT01 == null)
						{
							firstPTTT01 = pttt01;
						}
						if (billOfLading.IsEmpty)
						{
							billOfLading = pttt01.BillOfLadingSequenceNumber;
						}
					}
					else if (block is TARW01 tarw01)
					{
						if (billOfLading.IsEmpty)
						{
							billOfLading = tarw01.EntityNumber;
						}
						if (portOfUnlading.IsEmpty)
						{
							portOfUnlading = tarw01.CBPDistrictPort;
						}
						if (manifestSequenceNumber.IsEmpty)
						{
							manifestSequenceNumber = tarw01.ManifestSequenceNumber;
						}
					}
					else if (carrierCode.IsEmpty && block is TARW02 tarw02)
					{
						carrierCode = tarw02.CarrierCode;
					}
				}

				Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch = null;
				switch (message.EM_MessageType)
				{
					case SubApplicationCodeList.Codes.PermitToTransfer:
						movementMatch = GetPTTMovementMatch(firstPTTT01, ZString.Empty);
						break;
					default:
						movementMatch = x => x.Item2.ApplicationCode == CusInBondApplicationCodeList.Codes.AMS && x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS;
						break;
				}
				var linker = (IManifestMessageAttacheeMessageLinker)ObjectFactory.Get("ManifestMessageAttacheeMessageLinker");
				var messageAttachee = linker.MatchManifest((AMSEDIMessage)message, carrierCode, manifestSequenceNumber, vesselName, voyageNumber, portOfUnlading, estimatedDate, issuerCode, billOfLading, ZString.Empty, ZString.Empty, ZString.Empty, movementMatch, new AMSBillMatchingComparer());
				if (messageAttachee != null)
				{
					jobNumber = messageAttachee.JobNumber;
					linkedObject = (BusinessObject)messageAttachee;
					if (messageAttachee.Branch is GlbBranch branch && branch.PK.IsValid)
					{
						branchPK = branch.PK;
					}
				}
			}

			if (linkedObject == null)
			{
				discardReason = UCMPMessageProcessorFactory.GetUnableToFindTheLinkedJobMessage(message);
				return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, discardReason);
			}

			return ProcessingResult.New(new LinkedBusinessObjectMetaData(linkedObject.TableName, linkedObject.PK, branchPK, jobNumber), discardReason);
		}
	}
}
