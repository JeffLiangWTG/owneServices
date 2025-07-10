using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class DeclarationEntriesStatusesAndErrorCollections
	{
		public DeclarationEntriesStatusesAndErrorCollections(JobDeclaration declaration)
		{
			this.declaration = declaration;
			factory = new BusinessObjectFactory();
			factory.NameForDebugging = "DeclarationEntriesStatusesAndErrorCollections Factory";
		}

		readonly JobDeclaration declaration;
		readonly BusinessObjectFactory factory;

		internal void RefreshWhenANewMessageIsSaved()
		{
			ensStatusDate = null;
			ensTransmitCount = null;
			latestCRMessagesCalculated = false;
			crlTransmitCount = null;
			bluMessageStatusDate = null;
			fITStatusDate = null;
		}

		#region CRL Processing Results Statuses

		public void UpdateOGADispositionCodesFromMessage(MQEDIMessage cRLProcessingResultsMessage)
		{
			OGADispositionData processingDispositionRecord = null;
			foreach (MessageBlock block in cRLProcessingResultsMessage.MessageBlock.MessageBlocks)
			{
				var pgaDisposition = block as IPGADispositionProvider;
				if (pgaDisposition != null)
				{
					processingDispositionRecord = AddOGADispositionData(pgaDisposition);
				}
				else
				{
					var pgaDispositionDetail = block as IPGADispositionDetailProvider;
					if (pgaDispositionDetail != null)
					{
						if (processingDispositionRecord != null)
						{
							processingDispositionRecord.OGADispositionDetails.AddNewDispositionDetail(pgaDispositionDetail);
						}
					}
					else
					{
						var pgaDispositionComments = block as IPGADispositionComments;
						if (pgaDispositionComments != null)
						{
							if (processingDispositionRecord != null)
							{
								var combineComment = (processingDispositionRecord.US_Comment + " " + pgaDispositionComments.CommentsToTradeFromPGA).Trim();
								if (combineComment.Length > USOGADispositionDataAddInfo.Schema.US_CommentMaxLength)
								{
									processingDispositionRecord.US_Comment = combineComment.Substring(0, USOGADispositionDataAddInfo.Schema.US_CommentMaxLength);
								}
								else
								{
									processingDispositionRecord.US_Comment = combineComment;
								}
							}
						}
					}
				}
			}
		}

		public OGADispositionData AddOGADispositionData(IPGADispositionProvider dispProvider, string source = "", string envelopeNumber = "")
		{
			var oGADisposition = declaration.OGADispositionCodes.AddNewIfNotExist(dispProvider, envelopeNumber);
			oGADisposition.US_Source = source;
			return oGADisposition;
		}

		internal OGADispositionData AddOGADispositionDataAndCalculateDeclarationFDAStatus(IPGADispositionProvider dispProvider, string source = "", string envelopeNumber = "")
		{
			var result = AddOGADispositionData(dispProvider, source, envelopeNumber);

			if (!declaration.CanHavePGAFDA)
			{
				if (!dispProvider.EntryDispositionCode.IsEmpty && dispProvider.OtherAgencyQuotaIdentifier == FDAAgencyQuotaIdentifier)
				{
					declaration.FDAStatus = dispProvider.EntryDispositionCode;
				}

				var fDAMsgStatus = GetDeclarationFDAMsgStatus();
				if (!string.IsNullOrEmpty(fDAMsgStatus))
				{
					declaration.FDAMsgStatus = fDAMsgStatus;
				}
			}

			return result;
		}
		const string FDAAgencyQuotaIdentifier = "FDA";
		public string GetDeclarationFDAMsgStatus()
		{
			var result = string.Empty;
			if (declaration.RequiresPriorNoticeReporting)
			{
				if (declaration.HaveAllFDAPNCsBeenReceived)
				{
					result = FDAStatusList.Codes.ACP;
				}
				else
				{
					result = FDAStatusList.Codes.ACR;
				}
			}
			else
			{
				result = FDAStatusList.Codes.ACC;
			}
			return result;
		}

		internal void UpdateDispositionCodesFromMessage(Enterprise.Messaging.Business.EDIMessage[] cRLProcessingResultsMessages, DispositionDataCollection dispositionCodes)
		{
			foreach (MQEDIMessage message in cRLProcessingResultsMessages)
			{
				var statusBlocks = message.MessageBlock.MessageBlocks.OfType<IDispositionDetailProvider>();
				foreach (var dispositionDetail in statusBlocks)
				{
					dispositionDetail.AddDispositionData(dispositionCodes);
				}
			}
		}

		#endregion

		#region 7501 Status

		public ZDateTime ENSStatusDate
		{
			get
			{
				if (!ensStatusDate.HasValue)
				{
					ensStatusDate = ZDateTime.Empty;

					CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

					if (entry != null)
					{
						EDIMessage latestMessage = null;
						if (entry.HasBeenCancelled)
						{
							latestMessage = GetLatestMessageMatching(entry, new string[] { ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification });
							if (latestMessage == null)
							{
								latestMessage = GetLatestMessageMatching(entry.Declaration, new string[] { ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults });
							}
						}
						else
						{
							if (declaration.IsACSCargoCertificationMode)
							{
								latestMessage = GetLatestMessageMatching(entry, new string[] { ApplicationIdentifierCodeList.Codes.EntrySummary, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse });
							}
							else
							{
								latestMessage = GetLatestMessageMatching(entry, new string[] { ACEApplicationIdentifierCodeList.Codes.EntrySummary, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse });
							}
						}
						ensStatusDate = latestMessage != null ? latestMessage.EM_SystemCreateTimeUtc : ZDateTime.Empty;
					}
				}
				return ensStatusDate.Value;
			}
		}
		ZDateTime? ensStatusDate;

		public ZString ENSErrorsExist
		{
			get { return ENSERecords.Count > 0 ? ErrorsExistFor7501Msg : ZString.Empty; }
		}

		internal ZString ErrorsExistFor7501Msg
		{
			get { return declaration.IsACE ? "ENS Errors Exist (See ENS Errors Tab)" : "7501 Errors Exist (See 7501 Errors Tab)"; }
		}

		#region ENSERecords

		public ErrorsRecordCollection ENSERecords
		{
			get
			{
				if (enseRecords == null)
				{
					enseRecords = new ErrorsRecordCollection(factory);

					CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

					if (entry != null)
					{
						MQEDIMessage latestENSMsg = null;

						if (declaration.IsACSCargoCertificationMode)
						{
							latestENSMsg = (MQEDIMessage)GetLatestResponseMessageMatching(entry, new string[] { ApplicationIdentifierCodeList.Codes.EntrySummary, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse });
						}
						else
						{
							latestENSMsg = (MQEDIMessage)GetLatestResponseMessageMatching(entry, new string[] { ACEApplicationIdentifierCodeList.Codes.EntrySummary, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse });
						}

						if (latestENSMsg != null)
						{
							foreach (var block in latestENSMsg.MessageBlock.MessageBlocks)
							{
								var ensMessageError = block as I7501Errors;
								if (ensMessageError != null && ensMessageError.IsError)
								{
									var tariffNumberStatus = block as ITariffNumberStatusAndErrors;
									var tariffNo = tariffNumberStatus != null ? tariffNumberStatus.TariffNumber : ZString.Empty;
									var agencyCode = tariffNumberStatus != null ? tariffNumberStatus.PGAAgencyCode : ZString.Empty;
									var pgaLine = tariffNumberStatus != null ? tariffNumberStatus.PGALineNo : ZString.Empty;

									enseRecords.Add(new ErrorsRecord()
									{
										NarrativeMessage = ensMessageError.NarrativeMessage,
										ErrorMessageIdentifier = ensMessageError.Code,
										LineNumber = ensMessageError.LineNumber,
										TariffNumber = tariffNo,
										PGAAgencyCode = agencyCode,
										PGALine = pgaLine
									});
								}
							}
						}
					}
				}

				return enseRecords;
			}
		}
		ErrorsRecordCollection enseRecords;

		#endregion

		#region ENSStatusNotifications

		public ErrorsRecordCollection ENSStatusNotifications
		{
			get
			{
				if (ensStatusNotifications == null)
				{
					ensStatusNotifications = new ErrorsRecordCollection(factory);

					var declaration = this.declaration;
					IMessageAttachee messageAttachee = null;

					if (declaration != null)
					{
						if (declaration.IsDrawback)
						{
							messageAttachee = declaration;
						}
						else if (declaration.IsACERecon)
						{
							messageAttachee = declaration.ActiveEntryHeaders.ReconciliationEntry;
						}
						else
						{
							messageAttachee = declaration.ActiveEntryHeaders.EntrySummaryEntry;
						}
					}

					if (messageAttachee != null)
					{
						foreach (MQEDIMessage message in messageAttachee.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification }, EDIMessage.Direction.Receive))
						{
							ErrorsRecord record = new ErrorsRecord(message);
							record.UpdateEntrySummaryStatusNotificationDetails();

							ensStatusNotifications.Add(record);
						}
					}
				}
				return ensStatusNotifications;
			}
		}
		ErrorsRecordCollection ensStatusNotifications;

		public bool DoesENSStatusNotificationExistRequiringActions
		{
			get
			{
				foreach (ErrorsRecord error in ENSStatusNotifications)
				{
					if (error.IsFurtherActionRequiredButNotActionedYet)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal void MarkENSStatusElectronicInvoicingActionCompleted()
		{
			foreach (ErrorsRecord error in ENSStatusNotifications)
			{
				if (error.IsFurtherActionRequiredButNotActionedYet && error.DispositionCode == ENSStatusDispositionCodeList._1)
				{
					error.message.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.Authorised, "AII sent and accepted by Customs");
					break;
				}
			}
		}

		#endregion

		#region ENSE0Records

		public ErrorsRecordCollection ENSE0Records
		{
			get
			{
				if (ense0Records == null)
				{
					ense0Records = new ErrorsRecordCollection(factory);

					var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
					if (entry != null)
					{
						var orderedMessages = new List<EDIMessage>();
						if (entry.HasBeenCancelled)
						{
							if (declaration.IsACSCargoCertificationMode)
							{
								orderedMessages.AddRange(new TypedEnumerable<EDIMessage>(entry.Declaration.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults }, EDIMessage.Direction.Receive)));
							}
							else
							{
								orderedMessages.AddRange(new TypedEnumerable<EDIMessage>(entry.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification }, EDIMessage.Direction.Receive)));
							}
						}
						else
						{
							if (declaration.IsACSCargoCertificationMode)
							{
								orderedMessages.AddRange(new TypedEnumerable<EDIMessage>(entry.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ApplicationIdentifierCodeList.Codes.EntrySummaryResponse, ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse }, EDIMessage.Direction.Receive)));
							}
							else
							{
								orderedMessages.AddRange(new TypedEnumerable<EDIMessage>(entry.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse, ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse }, EDIMessage.Direction.Receive)));
							}
						}
						orderedMessages.Sort((x, y) => x.EM_SystemCreateTimeUtc.CompareTo(y.EM_SystemCreateTimeUtc));

						foreach (MQEDIMessage message in orderedMessages)
						{
							var errorBlocks = message.MessageBlock.MessageBlocks.OfType<I7501Status>();

							foreach (var block in errorBlocks)
							{
								if (block.IsMessageStatus)
								{
									var record = new ErrorsRecord();
									record.NarrativeMessage = block.NarrativeMessage;
									record.ErrorMessageIdentifier = block.Code;
									record.StatusDate = block.StatusDate.IsEmpty ? message.EM_SystemCreateTimeUtc : block.StatusDate;

									if (ImportEntryStatusList.IsCRLCertified(record.ErrorMessageIdentifier))
									{
										declaration.DispositionCodesView.Add(record);
									}
									else
									{
										ense0Records.Add(record);
									}
								}
							}

							ProcessCWOMessageAndAddSummary(message);
						}
					}
				}

				return ense0Records;
			}
		}
		ErrorsRecordCollection ense0Records;

		void ProcessCWOMessageAndAddSummary(MQEDIMessage message)
		{
			if (message.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse)
			{
				var record = new ErrorsRecord();

				int acceptedLines = 0;
				int rejectedLines = 0;

				foreach (MessageBlock block in message.MessageBlock.MessageBlocks)
				{
					var cwo3 = block as ACWOCW03;
					if (cwo3 != null)
					{
						if (cwo3.Accepted)
						{
							acceptedLines++;
						}
						else
						{
							rejectedLines++;
						}
					}
				}

				record.ErrorMessageIdentifier = "(CWO)";
				record.NarrativeMessage = string.Format(CWOSummaryTemplate, acceptedLines, rejectedLines);
				record.StatusDate = message.EM_SystemCreateTimeUtc;
				ense0Records.Add(record);
			}
		}

		const string CWOSummaryTemplate = "{0} Line(s) Accepted; {1} Line(s) Rejected";
		#endregion

		#region ENS Messages Count

		public ZString ENSTransmitCount
		{
			get
			{
				if (!ensTransmitCount.HasValue)
				{
					int result = 0;
					CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
					if (entry != null)
					{
						var transmitMessages = entry.Messages.GetMatchingMessages(
							EDIMessage.ApplicationCodes.USCustomsImport,
							new ZString[] { ApplicationIdentifierCodeList.Codes.EntrySummary, ACEApplicationIdentifierCodeList.Codes.EntrySummary },
							EDIMessage.Direction.Transmit);
						result = transmitMessages.Length;
					}
					ensTransmitCount = result.ToString();
				}
				return ensTransmitCount.Value;
			}
		}
		ZString? ensTransmitCount;

		public ZString ENSRejectCount
		{
			get
			{
				if (!ensRejectCount.HasValue)
				{
					int result = 0;
					CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
					if (entry != null)
					{
						var errorBlocks = new List<MessageBlock>();
						if (declaration.IsACSCargoCertificationMode)
						{
							errorBlocks.AddRange(entry.Messages.GetMatchedErrorBlocks(CBPEDIInterchange.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse));
							errorBlocks.AddRange(entry.Messages.GetMatchedErrorBlocks(CBPEDIInterchange.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.EntrySummary));
						}
						else
						{
							errorBlocks.AddRange(entry.Messages.GetMatchedErrorBlocks(CBPEDIInterchange.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse));
						}

						foreach (IStatusesAndErrors block in errorBlocks)
						{
							if (block.NarrativeMessage == USConstants.NarrativeRejectedMessage ||
								block.NarrativeMessage == USConstants.BatchRejected ||
								block.Code == ACEABIProcessor.Constants.TransactionDataRejected)
							{
								result++;
							}
						}
					}
					ensRejectCount = result.ToString();
				}
				return ensRejectCount.Value;
			}
		}
		ZString? ensRejectCount;

		#endregion

		public PSCEntrySummaryData LatestNonPSCEntryData
		{
			get
			{
				if (pscEntryData == null)
				{
					MQEDIMessage latest7501Accepted = null;
					MQEDIMessage latestSTUAccepted = null;

					var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
					if (entry != null)
					{
						var query = new ZQuery(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
						query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
						query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " desc";

						foreach (MQEDIMessage message in entry.Messages.Find(query))
						{
							var response = message.ResponseMessage;
							if (response != null && response.IsENSCleared)
							{
								var ens10 = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
								if (ens10 != null && ens10.PostSummaryCorrectionIndicator.IsEmpty)
								{
									latest7501Accepted = message;
									break;
								}
							}
						}

						if (latest7501Accepted != null)
						{
							var transmittedSTUMessages = declaration.TransmittedStatementMessagesInDescOrder;

							foreach (MQEDIMessage statementMessage in transmittedSTUMessages)
							{
								if (latest7501Accepted.EM_SystemCreateTimeUtc < statementMessage.EM_SystemCreateTimeUtc && statementMessage.IsSTUMessageAndAccepted())
								{
									latestSTUAccepted = statementMessage;
									break;
								}
							}
						}
					}
					pscEntryData = new PSCEntrySummaryData(latest7501Accepted, latestSTUAccepted);
				}
				return pscEntryData;
			}
		}
		PSCEntrySummaryData pscEntryData;

#if DEBUG
		public void RefreshPSCEntryDataForTesting()
		{
			pscEntryData = null;
		}
#endif

		#endregion

		#region CRL Status

		#region CRLH6orBCRE0102Records

		public ZDateTime CRLMessageStatusDate
		{
			get { return LatestCRMessage != null ? LatestCRMessage.EM_SystemCreateTimeUtc : ZDateTime.Empty; }
		}

		public ErrorsRecordCollection CargoReleaseRecords
		{
			get
			{
				if (cargoReleaseRecords == null)
				{
					cargoReleaseRecords = new ErrorsRecordCollection(factory);
					if (LatestCRResponseMessage != null)
					{
						var errorBlocks = LatestCRResponseMessage.MessageBlock.MessageBlocks;

						foreach (var block in errorBlocks)
						{
							var errorBlock = block as ICargoReleaseStatus;
							if (errorBlock != null)
							{
								errors3461Exist |= !SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted(errorBlock.Status);

								var record = new ErrorsRecord();
								record.NarrativeMessage = errorBlock.NarrativeMessage;
								record.DispositionCode = errorBlock.Status;

								var tariffNumberStatusAndErrors = block as ITariffNumberStatusAndErrors;
								if (tariffNumberStatusAndErrors != null)
								{
									record.LineNumber = tariffNumberStatusAndErrors.LineNumber;
									record.TariffNumber = tariffNumberStatusAndErrors.TariffNumber;
									record.PGAAgencyCode = tariffNumberStatusAndErrors.PGAAgencyCode;
									record.PGALine = tariffNumberStatusAndErrors.PGALineNo;
								}

								record.ErrorMessageIdentifier = errorBlock.ErrorIdentifierCode;

								if (ImportEntryStatusList.IsCRLCertified(record.DispositionCode))
								{
									record.StatusDate = LatestCRResponseMessage.EM_SystemCreateTimeUtc;
									declaration.DispositionCodesView.Add(record);
								}
								else
								{
									cargoReleaseRecords.Add(record);
								}
							}
						}
					}
				}
				return cargoReleaseRecords;
			}
		}
		ErrorsRecordCollection cargoReleaseRecords;

		MQEDIMessage LatestCRResponseMessage
		{
			get
			{
				CalculateLatestCRMessagesIfNeeded();
				return latestCRResponseMessage;
			}
		}
		MQEDIMessage latestCRResponseMessage;

		MQEDIMessage LatestCRMessage
		{
			get
			{
				CalculateLatestCRMessagesIfNeeded();
				return latestCRMessage;
			}
		}
		MQEDIMessage latestCRMessage;

		void CalculateLatestCRMessagesIfNeeded()
		{
			if (!latestCRMessagesCalculated)
			{
				latestCRMessagesCalculated = true;
				var messages = new List<MQEDIMessage>();
				var messageTypeQuery = new ZQuery();

				var simplifiedEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
				if (simplifiedEntry != null)
				{
					messageTypeQuery.AddToFilter(EDIMessageSchema.EM_MessageType, new string[] { ACEApplicationIdentifierCodeList.Codes.CargoRelease, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse });

					messages.AddRange(new TypedEnumerable<MQEDIMessage>(simplifiedEntry.Messages.Find(messageTypeQuery)));
				}
				else
				{
					var cargoReleaseEntry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
					var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

					messageTypeQuery.AddToFilter(EDIMessageSchema.EM_MessageType, new string[]
						{
							ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions,
							ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse,
							ApplicationIdentifierCodeList.Codes.BorderCargoRelease,
							ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse
						});

					if (cargoReleaseEntry != null)
					{
						messages.AddRange(new TypedEnumerable<MQEDIMessage>(cargoReleaseEntry.Messages.Find(messageTypeQuery)));
					}

					if (entrySummaryEntry != null)
					{
						if (entrySummaryEntry.HasBeenCancelled)
						{
							messageTypeQuery = new ZQuery();
							messageTypeQuery.AddToFilter(EDIMessageSchema.EM_MessageType, new string[]
								{
									ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification,
									ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults
								});
							messages.AddRange(new TypedEnumerable<MQEDIMessage>(entrySummaryEntry.Messages.Find(messageTypeQuery)));
							messages.AddRange(new TypedEnumerable<MQEDIMessage>(entrySummaryEntry.Declaration.Messages.Find(messageTypeQuery)));
						}
						else
						{
							messageTypeQuery.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_MessageType, new string[]
								{
									ACEApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse,
								});
							messages.AddRange(new TypedEnumerable<MQEDIMessage>(entrySummaryEntry.Messages.Find(messageTypeQuery)));
						}
					}
				}

				if (messages.Count > 0)
				{
					messages.Sort((x, y) => y.EM_SystemCreateTimeUtc.CompareTo(x.EM_SystemCreateTimeUtc));

					if (messages.Count > 0)
					{
						latestCRMessage = messages[0];
						if (messages[0].EM_ReceiveTransmit == EDIMessage.Direction.Receive)
						{
							latestCRResponseMessage = messages[0];
						}
					}
				}
			}
		}

		bool latestCRMessagesCalculated;

		bool Errors3461Exist
		{
			get { return CargoReleaseRecords.Count > 0 && errors3461Exist; }
		}
		bool errors3461Exist;

		#endregion

		#region Country Of Origin/Tariff Details (R3, WO30)

		public ZString COTariffExist
		{
			get { return COTariffRecords.Count > 0 ? "C/O, Tariff(s) Exist" : ""; }
		}

		public ErrorsRecordCollection COTariffRecords
		{
			get
			{
				if (coAndTariffRecords == null)
				{
					coAndTariffRecords = new ErrorsRecordCollection(factory);

					var messages = GetCountryOfOriginTariffDetailsBlock(declaration.Messages);

					var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
					if (entry != null)
					{
						messages = messages.Concat(GetCountryOfOriginTariffDetailsBlock(entry.Messages));
					}

					var simplifiedEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
					if (simplifiedEntry != null)
					{
						messages = messages.Concat(GetCountryOfOriginTariffDetailsBlock(simplifiedEntry.Messages));
					}

					var inMessage = messages.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
					if (inMessage != null)
					{
						PopulateCOAndTariffsFromR3Blocks(coAndTariffRecords, inMessage);
					}
				}
				return coAndTariffRecords;
			}
		}
		ErrorsRecordCollection coAndTariffRecords;

		IEnumerable<MQEDIMessage> GetCountryOfOriginTariffDetailsBlock(EDIMessageCollection messages)
		{
			return messages.OfType<MQEDIMessage>().Where(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Receive &&
					IDispositionDetailProviderExtensionMethods.MayContainReleaseDetails(x.EM_MessageType) &&
					x.MessageBlock.MessageBlocks.Find(block => block is ICountryOfOriginTariffDetailsBlock) != null);
		}

		void PopulateCOAndTariffsFromR3Blocks(ErrorsRecordCollection coAndTariffRecords, MQEDIMessage processingResultMessage)
		{
			coAndTariffRecords.AddRange(from block in processingResultMessage.MessageBlock.MessageBlocks.OfType<ICountryOfOriginTariffDetailsBlock>()
										select new ErrorsRecord()
										{
											LineNumber = block.RecordControlNumber.ToString(),
											CountryOfOrigin = block.CountryOfOrigin,
											TariffNumber = block.TariffNumber,
										});
		}

		#endregion

		#region Reference Data from WO20 (CQ/C1 ACE Query)

		public ErrorsRecordCollection ACECargoRelReferenceData
		{
			get
			{
				if (aceCargoRelReferenceData == null)
				{
					aceCargoRelReferenceData = new ErrorsRecordCollection(factory);

					var messages = declaration.Messages.Cast<MQEDIMessage>();
					var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
					if (entry != null)
					{
						messages = messages.Concat(entry.Messages.Cast<MQEDIMessage>());
					}

					messages = messages.Where(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Receive && x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse);

					var aceCargoReleaseEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
					if (aceCargoReleaseEntry != null)
					{
						messages = messages.Concat(
							aceCargoReleaseEntry.Messages.Cast<MQEDIMessage>()
							.Where(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Receive && x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)
							);
					}

					var latestMessages = messages.OrderByDescending(x => x.EM_SystemCreateTimeUtc);
					bool isCRMessageAlreadyAdded = false;
					var rejectionreasonCodeList = factory.GetCachedValue<RejectionReasonCodeList>();

					foreach (var latestMessage in latestMessages)
					{
						var blocks = latestMessage.MessageBlock.MessageBlocks.FindAll(x => x is ASESSO20Base).Cast<ASESSO20Base>();

						var so20Comments = new ZStringBuilder();

						foreach (ASESSO20Base block in blocks)
						{
							ZString errorMessageIdentifierFromBlock = block.ReferenceIdentifierQualifier;
							ZString narrativeMessageFromBlock = block.ReferenceIdentifier;

							if (latestMessage.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus && errorMessageIdentifierFromBlock == ReferenceIdentifierQualifierCodeList.Codes.CMT)
							{
								so20Comments.Append(narrativeMessageFromBlock);
							}
							else if (!(errorMessageIdentifierFromBlock == ReferenceIdentifierQualifierCodeList.Codes.CR && isCRMessageAlreadyAdded))
							{
								aceCargoRelReferenceData.Add(new ErrorsRecord(factory)
								{
									ErrorMessageIdentifier = errorMessageIdentifierFromBlock,
									NarrativeMessage = narrativeMessageFromBlock + " " +
										(errorMessageIdentifierFromBlock == ReferenceIdentifierQualifierCodeList.Codes.RSN ?
										rejectionreasonCodeList.GetDescriptionFromCode(narrativeMessageFromBlock) : string.Empty),
									StatusDate = latestMessage.EM_SystemCreateTimeUtc,
								});

								if (errorMessageIdentifierFromBlock == ReferenceIdentifierQualifierCodeList.Codes.CR)
								{
									isCRMessageAlreadyAdded = true;
								}
							}
						}

						if (!so20Comments.IsEmpty)
						{
							aceCargoRelReferenceData.Add(new ErrorsRecord(latestMessage)
							{
								ErrorMessageIdentifier = ReferenceIdentifierQualifierCodeList.Codes.CMT,
								NarrativeMessage = so20Comments.ToString(),
								StatusDate = latestMessage.EM_SystemCreateTimeUtc,
							});
						}
					}
				}

				return aceCargoRelReferenceData;
			}
		}
		ErrorsRecordCollection aceCargoRelReferenceData;

		#endregion

		public ZString CRLErrorsExist
		{
			get { return Errors3461Exist ? ErrorsExistFor3461Msg : ZString.Empty; }
		}

		internal ZString ErrorsExistFor3461Msg
		{
			get { return declaration.IsACE ? "CRL Errors Exist" : "3461 Errors Exist"; }
		}

		#region CRL Messages Count

		public ZString CRLTransmitCount
		{
			get
			{
				if (!crlTransmitCount.HasValue)
				{
					int result = 0;
					foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
					{
						if (entry.IsCargoRelease)
						{
							result += GetTransmittedMessagesLength(entry.Messages, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions);
						}
						else if (entry.IsBorderCargoRelease)
						{
							result += GetTransmittedMessagesLength(entry.Messages, ApplicationIdentifierCodeList.Codes.BorderCargoRelease);
						}
						else if (entry.IsACECargoRelease)
						{
							result += GetTransmittedMessagesLength(entry.Messages, ACEApplicationIdentifierCodeList.Codes.CargoRelease);
						}
					}
					crlTransmitCount = result.ToString();
				}
				return crlTransmitCount.Value;
			}
		}
		ZString? crlTransmitCount;

		int GetTransmittedMessagesLength(EDIMessageCollection messages, string messageType)
		{
			var transmitMessages = messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { messageType }, EDIMessage.Direction.Transmit);
			return transmitMessages.Length;
		}

		public ZString CRLRejectCount
		{
			get
			{
				if (!crlRejectCount.HasValue)
				{
					var result = 0;
					var errorBlocks = new List<MessageBlock>();
					foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
					{
						if (entry.IsCargoRelease)
						{
							errorBlocks = entry.Messages.GetMatchedErrorBlocks(CBPEDIInterchange.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse);
						}
						else if (entry.IsBorderCargoRelease)
						{
							errorBlocks.AddRange(entry.Messages.GetMatchedErrorBlocks(CBPEDIInterchange.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse));
						}
						else if (entry.IsACECargoRelease)
						{
							errorBlocks.AddRange(entry.Messages.GetMatchedErrorBlocks(CBPEDIInterchange.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse));
						}
					}

					foreach (IStatusesAndErrors block in errorBlocks)
					{
						if (block.Code == ACSABIProcessor.Constants.TransactionDataRejected ||
							block.Code == SimplifiedEntryMessageTypeCodesList.Codes.MessageRejected)
						{
							result++;
						}
					}

					crlRejectCount = result.ToString();
				}
				return crlRejectCount.Value;
			}
		}
		ZString? crlRejectCount;

		#endregion

		#endregion

		#region AII Status

		public ZString AIIRequested
		{
			get { return AIIURecords != null ? aiiRequested : ZString.Empty; }
		}
		ZString aiiRequested;

		public ZString AIIRejectedReason
		{
			get { return AIIURecords != null ? aiiRejectedReason : ZString.Empty; }
		}
		ZString aiiRejectedReason;

		public ZDateTime AIIUDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

				if (entry != null)
				{
					MQEDIMessage message = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ElectronicRejectRequestNotification, EDIMessage.Direction.Receive);
					if (message != null)
					{
						result = message.EM_SystemCreateTimeUtc;
					}
				}

				return result;
			}
		}

		#region AIIURecords

		public AIIURecordCollection AIIURecords
		{
			get
			{
				if (aiiuRecords == null)
				{
					aiiuRecords = new AIIURecordCollection(factory);

					CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

					if (entry != null)
					{
						MQEDIMessage message = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ElectronicRejectRequestNotification, EDIMessage.Direction.Receive);

						if (message != null)
						{
							AIIURecord record = null;

							foreach (MessageBlock block in message.MessageBlock.MessageBlocks)
							{
								ENSU1 ensu1 = block as ENSU1;
								if (ensu1 != null)
								{
									aiiRequested = ensu1.InvoiceRequestedNotification == 1 ? "Invoice Requested" : "";
									aiiRejectedReason = ensu1.Narrative;
								}
								else
								{
									ENSU3 ensu3 = block as ENSU3;
									if (ensu3 != null)
									{
										record = new AIIURecord();
										record.LineNumber = ensu3.LineNumber.ToString();
										record.RejectComments = ensu3.RejectComments;
										aiiuRecords.Add(record);
									}
									else if (record != null)
									{
										ENSU4 ensu4 = block as ENSU4;
										if (ensu4 != null)
										{
											record.RejectComments += " " + ensu4.RejectComments;
										}
									}
								}
							}
						}
					}
				}

				return aiiuRecords;
			}
		}
		AIIURecordCollection aiiuRecords;

		public ZString AIIErrorsExist
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in declaration.Invoices)
				{
					if (invoice.AIIERecords.Count > 0)
					{
						return "AII Errors Exist";
					}
				}

				return "";
			}
		}

		#endregion

		#endregion

		#region BOL Update Status and ACE Cargo Release BLU

		public IEnumerable<ErrorsRecord> LoadBillErrorsRecord(Bill bill)
		{
			var result =
				from disposition in bill.DispositionCodes.OfType<DispositionData>().Where(x => !x.US_DispositionDate.IsEmpty)
				select new ErrorsRecord()
				{
					ErrorMessageIdentifier = disposition.US_Code,
					NarrativeMessage = ((ZString)((IDispositionCodeDateParent)bill).GetDispositionDescriptionBasedOnSource(disposition.US_Source, disposition.US_Code)).Left(AutoErrorsRecord.Schema.NarrativeMessageMaxLength),
					StatusDate = disposition.US_DispositionDate,
					ActionIDNumber = bill.CU_BillNum.Left(ErrorsRecord.Schema.ActionIDNumberMaxLength)
				};
			return result;
		}

		public ErrorsRecordCollection BLUL7Records
		{
			get
			{
				if (bluL7Records == null)
				{
					bluL7Records = new ErrorsRecordCollection(factory);

					if (declaration.IsACECargoCertificationMode)
					{
						foreach (Bill bill in declaration.Bills)
						{
							if (bill.DispositionCodes.Count > 0)
							{
								bluL7Records.AddRange(LoadBillErrorsRecord(bill));
							}
						}
					}
					else
					{
						MQEDIMessage latestBOLmessage = (MQEDIMessage)declaration.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse, EDIMessage.Direction.Receive);

						if (latestBOLmessage == null)
						{
							foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
							{
								if (entry.IsFormalEntry || entry.IsCargoRelease || entry.IsBorderCargoRelease)
								{
									MQEDIMessage message = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse, EDIMessage.Direction.Receive);
									if (latestBOLmessage == null || (message != null && latestBOLmessage.EM_SystemCreateTimeUtc > message.EM_SystemCreateTimeUtc))
									{
										latestBOLmessage = message;
									}
								}
							}
						}

						if (latestBOLmessage != null)
						{
							bluL7Records.AddRange(from block in latestBOLmessage.MessageBlock.MessageBlocks.OfType<BOLL7>()
												  select new ErrorsRecord()
												  {
													  ErrorMessageIdentifier = block.ErrorMessageIdentifier,
													  NarrativeMessage = block.NarrativeMessage,
												  });
						}
					}
				}

				return bluL7Records;
			}
		}
		ErrorsRecordCollection bluL7Records;

		public ZDateTime BLUMessageStatusDate
		{
			get
			{
				if (!bluMessageStatusDate.HasValue)
				{
					var latestMsg = (MQEDIMessage)GetLatestMessageMatching(declaration, new string[]
					{
						ApplicationIdentifierCodeList.Codes.BillofLadingUpdate,
						ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse,
						ApplicationIdentifierCodeList.Codes.BillofLadingProcessingResults,
						ACEApplicationIdentifierCodeList.Codes.CargoRelease,
						ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse
					});

					bluMessageStatusDate = latestMsg != null && (latestMsg.IsBLU || latestMsg.IsACEBLU) ? latestMsg.EM_SystemCreateTimeUtc : ZDateTime.Empty;
				}
				return bluMessageStatusDate.Value;
			}
		}
		ZDateTime? bluMessageStatusDate;

		public ZString BLUErrorsExist
		{
			get { return declaration.BLUStatus == ImportMessageStatusList.Codes.ErrorBillOfLadingUpdate ? "BLU Errors Exist" : ""; }
		}

		#endregion

		#region IT Status

		#region IT Departure Status

		public ZString ITDepartureStatus
		{
			get
			{
				ZString result = ZString.Empty;

				CusEntryHeader entry = declaration.ActiveEntryHeaders.InBondEntry;

				if (entry != null)
				{
					ZString[] itMessagesSubTypes = new ZString[] { EM_MessageSubTypeList.Codes.InBondDepartureOriginal, EM_MessageSubTypeList.Codes.InBondDepartureDelete, EM_MessageSubTypeList.Codes.InBondDepartureReplacement };
					MQEDIMessage outwardMessage = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.InbondTransaction, EDIMessage.Direction.Transmit, "", itMessagesSubTypes);

					if (outwardMessage != null)
					{
						MQEDIMessage candidateInwardMessage = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, EDIMessage.Direction.Receive, "", itMessagesSubTypes);
						MQEDIMessage inwardMessage = null;

						if (candidateInwardMessage != null)
						{
							if (candidateInwardMessage.EM_SystemCreateTimeUtc > outwardMessage.EM_SystemCreateTimeUtc)
							{
								inwardMessage = candidateInwardMessage;
							}
						}

						if (inwardMessage != null)
						{
							var infoBlocks = inwardMessage.MessageBlock.MessageBlocks.OfType<IINBQT95>();
							var status = infoBlocks.Any(qt95 => qt95.NarrativeMessageTypeCode == InBondAcceptanceRejectionList.Codes.Rejected)
									   ? ABIResponseStatus.Rejected
									   : ABIResponseStatus.Cleared;
							result = CalculateStatus(inwardMessage, status);
						}
						else
						{
							result = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
						}
					}
				}

				return result;
			}
		}

		#endregion

		public ZString ITArrivalStatus
		{
			get { return GetWTInBondStatus(EM_MessageSubTypeList.Codes.InBondArrival); }
		}

		public ZString ITExportStatus
		{
			get { return GetWTInBondStatus(EM_MessageSubTypeList.Codes.InBondExportation); }
		}

		public ZString ITTOLStatus
		{
			get { return GetWTInBondStatus(EM_MessageSubTypeList.Codes.InBondTransferOfLiability); }
		}

		#region IT Status Dates

		public ZDateTime ITStatusDate
		{
			get
			{
				if (!fITStatusDate.HasValue)
				{
					fITStatusDate = ZDateTime.Empty;

					foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
					{
						if (entry.IsInBond)
						{
							var inbondMessage = GetLatestMessageMatching(entry, new string[] { ACEApplicationIdentifierCodeList.Codes.InbondTransaction, ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse });
							fITStatusDate = inbondMessage != null ? inbondMessage.EM_SystemCreateTimeUtc : ZDateTime.Empty;

							var inbondUpdateMsgs = GetMessagesInDescendingChronologicalOrder(entry, new string[] { ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse }).Where(x => x.EM_MessageSubType != EM_MessageSubTypeList.Codes.InBondDiversionRequest);
							var inbondUpdateMsg = inbondUpdateMsgs.Any() ? inbondUpdateMsgs.ElementAt(0) : null;
							var lastITUpdateDate = inbondUpdateMsg != null ? inbondUpdateMsg.EM_SystemCreateTimeUtc : ZDateTime.Empty;

							if (fITStatusDate.Value.IsEmpty || lastITUpdateDate > fITStatusDate)
							{
								fITStatusDate = lastITUpdateDate;
							}

							break;
						}
					}
				}

				return fITStatusDate.Value;
			}
		}
		ZDateTime? fITStatusDate;

		#endregion

		#endregion

		#region ITErrorRecords

		#region Departure

		public ErrorsRecordCollection ITQT95Records
		{
			get
			{
				if (itQT95Records == null)
				{
					itQT95Records = new ErrorsRecordCollection(factory);
					foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
					{
						if (entry.IsInBond)
						{
							var inMessage = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, EDIMessage.Direction.Receive, "", EM_MessageSubTypeList.Codes.InBondDepartureOriginal);

							if (inMessage != null)
							{
								if (inMessage.EM_MessageSubType == EM_MessageSubTypeList.Codes.InBondDepartureOriginal)
								{
									var infoBlocks = inMessage.MessageBlock.MessageBlocks.OfType<IINBQT95>();

									foreach (var block in infoBlocks)
									{
										var record = new ErrorsRecord()
										{
											NarrativeMessage = block.NarrativeMessage,
											ErrorMessageIdentifier = block.Code,
											StatusDate = inMessage.EM_SystemCreateTimeUtc,
										};
										ITQT95Records.Add(record);
										if (block.NarrativeMessageTypeCode == InBondAcceptanceRejectionList.Codes.Rejected)
										{
											ITErrorResponsesExist = true;
										}
									}
								}
							}

							break;
						}
					}
				}

				return itQT95Records;
			}
		}
		ErrorsRecordCollection itQT95Records;

		#endregion

		#region Export

		public ErrorsRecordCollection ITWT95ExportRecords
		{
			get
			{
				if (itWT95ExportRecords == null)
				{
					SplitITErrors();
				}

				return itWT95ExportRecords;
			}
		}

		#endregion

		#region Arrival

		public ErrorsRecordCollection ITWT95ArrivalRecords
		{
			get
			{
				if (itWT95ArrivalRecords == null)
				{
					SplitITErrors();
				}

				return itWT95ArrivalRecords;
			}
		}

		#endregion

		#region TOL

		public ErrorsRecordCollection ITWT95TOLRecords
		{
			get
			{
				if (itWT95TOLRecords == null)
				{
					SplitITErrors();
				}

				return itWT95TOLRecords;
			}
		}

		#endregion

		#region ITErrors

		public void SplitITErrors()
		{
			itWT95ArrivalRecords = new ErrorsRecordCollection(factory);
			itWT95ExportRecords = new ErrorsRecordCollection(factory);
			itWT95TOLRecords = new ErrorsRecordCollection(factory);

			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (entry.IsInBond)
				{
					MQEDIMessage lastArrivalMessage = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, EDIMessage.Direction.Receive, "", EM_MessageSubTypeList.Codes.InBondArrival);
					if (lastArrivalMessage != null)
					{
						OutputErrors(lastArrivalMessage, itWT95ArrivalRecords);
					}

					MQEDIMessage lastExportMessage = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, EDIMessage.Direction.Receive, "", EM_MessageSubTypeList.Codes.InBondExportation);
					if (lastExportMessage != null)
					{
						OutputErrors(lastExportMessage, itWT95ExportRecords);
					}

					MQEDIMessage lastTOLMessage = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, EDIMessage.Direction.Receive, "", EM_MessageSubTypeList.Codes.InBondTransferOfLiability);
					if (lastTOLMessage != null)
					{
						OutputErrors(lastTOLMessage, itWT95TOLRecords);
					}

					break;
				}
			}
		}

		void OutputErrors(MQEDIMessage message, ErrorsRecordCollection collection)
		{
			var infoBlocks = message.MessageBlock.MessageBlocks.OfType<IINBWT95>();

			foreach (var block in infoBlocks)
			{
				var record = new ErrorsRecord()
				{
					NarrativeMessage = block.NarrativeMessage,
					ErrorMessageIdentifier = block.Code,
					StatusDate = message.EM_SystemCreateTimeUtc,
				};
				collection.Add(record);
				if (block.NarrativeMessageTypeCode == InBondAcceptanceRejectionList.Codes.Rejected)
				{
					ITErrorResponsesExist = true;
				}
			}
		}

		public bool ITErrorResponsesExist
		{
			get { return fITErrorResponsesExist; }
			set { fITErrorResponsesExist = value; }
		}
		bool fITErrorResponsesExist;

		ErrorsRecordCollection itWT95ExportRecords;
		ErrorsRecordCollection itWT95ArrivalRecords;
		ErrorsRecordCollection itWT95TOLRecords;

		#endregion

		#endregion

		#region FTZ Dispositions

		public void UpdateFTZDispositionCodes(Enterprise.Messaging.Business.EDIMessage[] fTZResponseMessages, DispositionDataCollection dispositionCodes)
		{
			foreach (MQEDIMessage message in fTZResponseMessages)
			{
				var statusBlocks = message.MessageBlock.MessageBlocks.OfType<FTZNF91>();
				foreach (var block in statusBlocks)
				{
					if (DispositionList.IsNotableFTZDispositionCode(block.DispositionCode))
					{
						var actionTime = ZDateTime.Empty;
						var actionTimeString = block.ActionTime;
						var actionDate = block.ActionDate;
						if (actionDate.IsValid)
						{
							actionTime = new ZDateTime(actionDate.Year, actionDate.Month, actionDate.Day, new ZInt(actionTimeString.SubstringSafe(0, 2)), new ZInt(actionTimeString.SubstringSafe(2, 2)), 0);
						}
						var code = dispositionCodes.AddNewIfNotExist(block.DispositionCode, actionTime, GetQualifierName(block.ReferenceQualifier), block.ReferenceID);
					}
				}
			}
		}
		ZString GetQualifierName(ZString qualifier)
		{
			switch (qualifier)
			{
				case "1":
					return "FTZ";
				case "2":
					return "BOL";
				case "3":
					return "In-Bond";
				default:
					return "";
			}
		}

		#endregion

		#region Implementation

		EDIMessage GetLatestMessageMatching(IMessageAttachee entity, string[] messageTypes)
		{
			return GetMessagesInDescendingChronologicalOrder(entity, messageTypes).FirstOrDefault();
		}

		EDIMessage GetLatestResponseMessageMatching(IMessageAttachee entity, string[] messageTypes)
		{
			var message = GetMessagesInDescendingChronologicalOrder(entity, messageTypes).FirstOrDefault();
			return message != null && message.EM_ReceiveTransmit == EDIMessage.Direction.Receive ? message : null;
		}

		IEnumerable<EDIMessage> GetMessagesInDescendingChronologicalOrder(IMessageAttachee entity, string[] messageTypes)
		{
			var messages = new List<EDIMessage>(new TypedEnumerable<EDIMessage>(entity.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageType, messageTypes))));

			if (messages.Count > 0)
			{
				messages.Sort((x, y) => y.EM_SystemCreateTimeUtc.CompareTo(x.EM_SystemCreateTimeUtc));
			}

			return messages;
		}

		ZString GetWTInBondStatus(string messageSubType)
		{
			ZString result = ZString.Empty;

			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (entry.IsInBond)
				{
					MQEDIMessage outMessage = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability, EDIMessage.Direction.Transmit, "", messageSubType);

					if (outMessage != null)
					{
						result = GetStatus(entry, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, messageSubType);
					}
					else
					{
						outMessage = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.InbondTransaction, EDIMessage.Direction.Transmit, "", messageSubType);
						if (outMessage != null)
						{
							result = GetStatus(entry, ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, messageSubType);
						}
					}

					break;
				}
			}

			return result;
		}

		ZString GetStatus(CusEntryHeader entry, string appId, string messageSubType)
		{
			var result = ZString.Empty;

			var inMessage = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, appId, EDIMessage.Direction.Receive, "", messageSubType);
			if (inMessage != null)
			{
				var hasLinesCleared = false;
				var hasLinesRejected = false;

				var infoBlocks = inMessage.MessageBlock.MessageBlocks.OfType<IINBWT95>();
				foreach (var wt95 in infoBlocks)
				{
					var status = wt95.NarrativeMessageTypeCode == InBondAcceptanceRejectionList.Codes.Rejected ? ABIResponseStatus.Rejected : ABIResponseStatus.Cleared;

					hasLinesCleared |= status == ABIResponseStatus.Cleared;
					hasLinesRejected |= status == ABIResponseStatus.Rejected;

					result = CalculateStatus(inMessage, status);
				}
			}
			else
			{
				result = ImportMessageStatusList.Codes.AwaitingArrival;
				switch (messageSubType)
				{
					case EM_MessageSubTypeList.Codes.InBondArrival:
						result = ImportMessageStatusList.Codes.AwaitingArrival;
						break;
					case EM_MessageSubTypeList.Codes.InBondExportation:
						result = ImportMessageStatusList.Codes.AwaitingExportation;
						break;
					case EM_MessageSubTypeList.Codes.InBondTransferOfLiability:
						result = ImportMessageStatusList.Codes.AwaitingTransferOfLiability;
						break;
				}
			}

			return result;
		}

		#region IT Status Determination

		ZString CalculateStatus(MQEDIMessage lastMessage, ABIResponseStatus status)
		{
			ZString statusCalculated = "";

			switch (status)
			{
				case ABIResponseStatus.Rejected:
					statusCalculated = GetRejectedStatus(lastMessage);
					break;
				case ABIResponseStatus.PartialCleared:
					statusCalculated = GetPartialClearedStatus(lastMessage);
					break;
				case ABIResponseStatus.Warnings:
					statusCalculated = GetWarningStatus(lastMessage);
					break;
				case ABIResponseStatus.Cleared:
					statusCalculated = GetClearedStatus(lastMessage);
					break;
				default:
					statusCalculated = GetUndefinedStatus(lastMessage);
					break;
			}

			return statusCalculated;
		}

		protected ZString GetPartialClearedStatus(MQEDIMessage lastMessage)
		{
			ZString status = ZString.Empty;
			switch (lastMessage.EM_MessageSubType)
			{
				case EM_MessageSubTypeList.Codes.InBondDepartureOriginal:
					status = ImportMessageStatusList.Codes.ClearDeparturePartialOriginal;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureReplacement:
					status = ImportMessageStatusList.Codes.ClearDeparturePartialAmendment;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureDelete:
					status = ImportMessageStatusList.Codes.ClearDeparturePartialWithdraw;
					break;
				case EM_MessageSubTypeList.Codes.InBondArrival:
					status = ImportMessageStatusList.Codes.ClearArrival;
					break;
				case EM_MessageSubTypeList.Codes.InBondExportation:
					status = ImportMessageStatusList.Codes.ClearExportation;
					break;
				case EM_MessageSubTypeList.Codes.InBondFDATransmission:
					status = ImportMessageStatusList.Codes.ClearFDATransmission;
					break;
				case EM_MessageSubTypeList.Codes.InBondTransferOfLiability:
					status = ImportMessageStatusList.Codes.ClearTransferOfLiability;
					break;
				case EM_MessageSubTypeList.Codes.FDACorrection:
					status = ImportMessageStatusList.Codes.ClearFDACorrection;
					break;
			}
			return status;
		}

		protected ZString GetRejectedStatus(MQEDIMessage lastMessage)
		{
			ZString status = ZString.Empty;
			switch (lastMessage.EM_MessageSubType)
			{
				case EM_MessageSubTypeList.Codes.InBondDepartureOriginal:
					status = ImportMessageStatusList.Codes.ErrorDepartureOriginal;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureReplacement:
					status = ImportMessageStatusList.Codes.ErrorDepartureAmendment;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureDelete:
					status = ImportMessageStatusList.Codes.ErrorDepartureWithdraw;
					break;
				case EM_MessageSubTypeList.Codes.InBondArrival:
					status = ImportMessageStatusList.Codes.ErrorArrival;
					break;
				case EM_MessageSubTypeList.Codes.InBondExportation:
					status = ImportMessageStatusList.Codes.ErrorExportation;
					break;
				case EM_MessageSubTypeList.Codes.InBondFDATransmission:
					status = ImportMessageStatusList.Codes.ErrorFDATransmission;
					break;
				case EM_MessageSubTypeList.Codes.InBondTransferOfLiability:
					status = ImportMessageStatusList.Codes.ErrorTransferOfLiability;
					break;
				case EM_MessageSubTypeList.Codes.FDACorrection:
					status = ImportMessageStatusList.Codes.ErrorFDACorrection;
					break;
			}
			return status;
		}

		protected ZString GetAwaitingStatus(MQEDIMessage lastMessage)
		{
			ZString status = ZString.Empty;
			switch (lastMessage.EM_MessageSubType)
			{
				case EM_MessageSubTypeList.Codes.InBondDepartureOriginal:
					status = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureReplacement:
					status = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureDelete:
					status = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
					break;
				case EM_MessageSubTypeList.Codes.InBondArrival:
					status = ImportMessageStatusList.Codes.AwaitingArrival;
					break;
				case EM_MessageSubTypeList.Codes.InBondExportation:
					status = ImportMessageStatusList.Codes.AwaitingExportation;
					break;
				case EM_MessageSubTypeList.Codes.InBondFDATransmission:
					status = ImportMessageStatusList.Codes.AwaitingFDATransmission;
					break;
				case EM_MessageSubTypeList.Codes.InBondTransferOfLiability:
					status = ImportMessageStatusList.Codes.AwaitingTransferOfLiability;
					break;
				case EM_MessageSubTypeList.Codes.FDACorrection:
					status = ImportMessageStatusList.Codes.AwaitingFDACorrection;
					break;
			}
			return status;
		}

		protected ZString GetClearedStatus(MQEDIMessage lastMessage)
		{
			ZString status = ZString.Empty;
			switch (lastMessage.EM_MessageSubType)
			{
				case EM_MessageSubTypeList.Codes.InBondDepartureOriginal:
					status = ImportMessageStatusList.Codes.ClearDepartureOriginal;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureReplacement:
					status = ImportMessageStatusList.Codes.ClearDepartureAmendment;
					break;
				case EM_MessageSubTypeList.Codes.InBondDepartureDelete:
					status = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
					break;
				case EM_MessageSubTypeList.Codes.InBondArrival:
					status = ImportMessageStatusList.Codes.ClearArrival;
					break;
				case EM_MessageSubTypeList.Codes.InBondExportation:
					status = ImportMessageStatusList.Codes.ClearExportation;
					break;
				case EM_MessageSubTypeList.Codes.InBondFDATransmission:
					status = ImportMessageStatusList.Codes.ClearFDATransmission;
					break;
				case EM_MessageSubTypeList.Codes.InBondTransferOfLiability:
					status = ImportMessageStatusList.Codes.ClearTransferOfLiability;
					break;
				case EM_MessageSubTypeList.Codes.FDACorrection:
					status = ImportMessageStatusList.Codes.ClearFDACorrection;
					break;
			}
			return status;
		}

		protected ZString GetWarningStatus(MQEDIMessage lastMessage)
		{
			return ZString.Empty;
		}

		protected ZString GetUndefinedStatus(MQEDIMessage lastMessage)
		{
			return ZString.Empty;
		}

		#endregion

		#endregion
	}
}
