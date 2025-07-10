using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class EntrySummaryStatusNotificationProcessor : ACEABIProcessor
	{
		#region  IKeysForBlockingParallelProcessingProvider Members

		protected override ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(CBPEDIMessage message)
		{
			var branchPK = ZGuid.Empty;
			var linkUniqueID = ZGuid.Empty;
			var linkTableName = ZString.Empty;
			var jobNumber = ZString.Empty;
			var entryData = GetEntryDataFromMessage(message);
			if (entryData.HasValue)
			{
				var entryFilerCode = entryData.Value.EntryFilerCode;
				var entryNumber = entryData.Value.EntryNumber;
				var companyPK = message.Branch.GB_GC;
				var messageAttachee = (IMessageAttachee)new CusEntryHeader.Loader(message.Factory).FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, entryNumber, entryFilerCode, new ZString[] { CusEntryHeaderMessageTypeList.Codes.ReconEntry, CusEntryHeaderMessageTypeList.Codes.EntrySummary })
					?? GetMatchingDeclaration(message.Factory, entryFilerCode, entryNumber, companyPK);
				if (messageAttachee == null)
				{
					jobNumber = USIUniversalCustomsMessageProcessor.GetEntryJobNumberKey(entryFilerCode, entryNumber, companyPK);
				}
				else
				{
					jobNumber = messageAttachee.TopLevelBusinessObject is IJobNumber topLevelBusinessObject ? topLevelBusinessObject.JobNumber : messageAttachee.TopLevelBizObjReferenceNumber;
					branchPK = message.Branch.PK;
					var bizObj = (BusinessObject)messageAttachee;
					linkUniqueID = bizObj.PK;
					linkTableName = bizObj.TableName;
				}
			}
			return LinkedBusinessObjectMetaData.New(linkTableName, linkUniqueID, branchPK, jobNumber);
		}

		protected override HashSet<string> TryFindSerializationKeys(CBPEDIMessage message, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var result = new HashSet<string>();
			var entryData = GetEntryDataFromMessage(message);
			if (entryData.HasValue)
			{
				result.Add(USIUniversalCustomsMessageProcessor.GetEntryJobNumberKey(entryData.Value.EntryFilerCode, entryData.Value.EntryNumber, message.Branch.GB_GC));
			}
			return result;
		}

		(ZString EntryFilerCode, ZString EntryNumber)? GetEntryDataFromMessage(CBPEDIMessage message)
		{
			var e1 = message.MessageBlock.MessageBlocks.OfType<AESSE1>().FirstOrDefault();
			return e1 == null ? null : (e1.EntryFilerCode, e1.EntryNumber);
		}

		#endregion

		public override void Process()
		{
			var messageAttache = Message.EM_LinkedObject as IMessageAttachee ?? LinkToMessageAttache();
			var declaration = messageAttache?.TopLevelBusinessObject as JobDeclaration;
			var emailBody = GenerateEmailBody(messageAttache, declaration);
			var uri = "";
			var jobNumber = "Unknown";

			if (messageAttache != null)
			{
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(messageAttache);
				jobNumber = declaration?.DeclarationReferenceAppendedByFormattedEntryNumber ?? jobNumber;
				UpdateStatus(messageAttache, emailBody, declaration);
			}

			Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryNotification;
			if (Message.EM_MessageOwner != Reprocessing)
			{
				EmailDef email;
				var branch = declaration?.Branch ?? GlbBranch.CurrentBranch;
				if (new HtmlResponseEmailGenerator().TryGenerateEmail(uri, jobNumber, "Entry Summary Status Notification", emailBody.ToString(), "", false, out email, branch))
				{
					var registryItem = GetEmailGroupRegistryItem() as Registry.Business.Customs.ManifestGroupNotificationRegistryItem;
					var groupNotification = registryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);

					var shouldSendErrorNotificationsOnly = groupNotification.SendErrorOnly;
					if (!shouldSendErrorNotificationsOnly)
					{
						var messages = messageAttache?.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummary || x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummary || x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQuery);
						var emailAddressesFromMessage = declaration?.GetEmailRecipients(messages) ?? Array.Empty<ZString>();

						var recipientCalculator = new EmailRecipientCalculator(groupNotification.SendMode, groupNotification.SendGroupPK, emailAddressesFromMessage, ZGuid.Empty);
						recipientCalculator.SendNotifications(Factory, email, registryItem);
					}
				}
			}
			else
			{
				Message.EM_MessageOwner = ZString.Empty;
			}
		}

		IMessageAttachee LinkToMessageAttache()
		{
			IMessageAttachee result = null;
			if (E1MessageBlock.EntryNumber is ZString entryNumber && !entryNumber.IsEmpty && E1MessageBlock.EntryFilerCode is ZString entryFilerCode && !entryFilerCode.IsEmpty)
			{
				result = CusEntryHeaderLinker.Link(entryNumber, entryFilerCode, Message, new ZString[] { CusEntryHeaderMessageTypeList.Codes.ReconEntry, CusEntryHeaderMessageTypeList.Codes.EntrySummary });

				if (result == null)
				{
					result = OriginalMessageLinker.Link<JobDeclaration>(Message);
				}

				if (result == null)
				{
					var declaration = GetMatchingDeclaration(Factory, entryFilerCode, entryNumber, GlbCompany.CurrentCompany.PK);
					if (declaration != null)
					{
						Message.EM_LinkedObject = declaration;
						result = declaration;
					}
				}
			}

			return result;
		}

		static JobDeclaration GetMatchingDeclaration(BusinessObjectFactory factory, ZString entryFilerCode, ZString entryNumber, ZGuid companyPK)
		{
			var declarationFilter = new ZDBOnlyQuery(typeof(JobDeclaration));
			declarationFilter.AddToFilter(JobDeclarationSchema.JE_GC, companyPK);

			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobDeclaration.Schema.TableName);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			declarationFilter.AddSubQuery(entryNumberQuery, JoinCondition.And);
			return factory.Load<JobDeclaration>(declarationFilter).OrderBy(x => x.JE_SystemCreateTimeUtc).FirstOrDefault(x => x.EntryFilerCode == entryFilerCode);
		}

		AESSE1 E1MessageBlock
		{
			get { return fE1MessageBlock ?? (fE1MessageBlock = GetFirstMessageBlock<AESSE1>()); }
		}
		AESSE1 fE1MessageBlock;

		ZString rejectionNotice = ZString.Empty;

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		ZStringBuilder GenerateEmailBody(IMessageAttachee messageAttache, JobDeclaration declaration)
		{
			var result = new ZStringBuilder();
			var remarks = new ZStringBuilder();

			var table = new HtmlTableCreator(new string[] { "Column", "Value" });
			table.EnableHTMLEncoding = false;

			HtmlTableCreator quotaLineStatusTable = null;

			HtmlTableCreator so70Table = null;
			List<string> so70TableRow = null;
			OGADispositionData processingOGADispositionData = null;
			var dispositionComments = new ZStringBuilder();
			var dispositionReasons = new ZStringBuilder();
			var subReasonList = new OGADispositionSubReasonList();
			var previousPGAEntryStatus = declaration?.GetPGAEntryStatus() ?? new Dictionary<ZString, ZString>();
			var pgaEntryStatusMapping = new Dictionary<ZString, IPGADispositionProvider>();
			var dispositionProviders = new List<IPGADispositionProvider>();

			GenerateEntryNumber(messageAttache, table);

			foreach (var block in messageBlocks)
			{
				var e1 = block as AESSE1;

				if (e1 != null)
				{
					var dispositionDescription = ENSStatusDispositionCodeListLoader.GetENSStatusDispositionCodeList(Factory).GetDescriptionFromCode(e1.DispositionTypeCode);
					if (e1.DispositionTypeCode == ENSStatusDispositionCodeList._4)
					{
						rejectionNotice = e1.NotificationReasonCode == ENSStatusRejectReasonList.Codes._022 ? QuotaRejectionNotice : dispositionDescription;
					}
					else if (e1.DispositionTypeCode == ENSStatusDispositionCodeList._R)
					{
						rejectionNotice = dispositionDescription;
					}

					GenerateForE1(e1, table);
					if (messageAttache != null)
					{
						ZPropertyInfo anticipatedLiquidationDateInfo = null;
						if (messageAttache is CusEntryHeader entryHeader)
						{
							anticipatedLiquidationDateInfo = entryHeader.US_ALDateInfo;
						}
						else if (declaration != null)
						{
							anticipatedLiquidationDateInfo = declaration.US_ALDateInfo;
						}

						if (e1.EntrySummaryRedlinedIndicator == YesNoDefaultList.Codes.Yes)
						{
							if (anticipatedLiquidationDateInfo != null)
							{
								anticipatedLiquidationDateInfo.Value = ZDateTime.Empty;
							}
						}
					}
				}
				else
				{
					var e2 = block as AESSE2;
					if (e2 != null)
					{
						GenerateForE2(e2, table);
					}
					else
					{
						var e3 = block as AESSE3;
						if (e3 != null)
						{
							remarks.Append(e3.Remarks);
						}
						else
						{
							var e4 = block as AESSE4;
							if (e4 != null)
							{
								if (quotaLineStatusTable == null)
								{
									quotaLineStatusTable = new HtmlTableCreator(new string[] { "Line #", "Quota Status Code & Description", "Requested Quota Qty & UQ", "Reserved Quota Qty & UQ" });
									quotaLineStatusTable.EnableHTMLEncoding = false;
								}
								GenerateForE4(e4, quotaLineStatusTable, messageAttache);
							}
							else
							{
								var so70 = block as IPGADispositionProvider;
								if (so70 != null)
								{
									if (so70Table == null)
									{
										so70Table = new HtmlTableCreator(new string[] { "Agency/Quota Indicator", "Disposition Date", "PGA Entry Status Desc", "PGA Line Status Desc", "CBP Line Status", "Beg. CBP Line", "Beg. Tariff", "Beg. PGA Line", "End PGA Line", "End Tariff", "End CBP Line", "Document Type", "PGA Entry Hold Type", "Comment", "Reasons" });
									}
									else
									{
										SimplifiedEntryStatusNotificationProcessor.ProcessSO70Block(so70Table, so70TableRow, dispositionComments, dispositionReasons);
										SimplifiedEntryStatusNotificationProcessor.UpdateComment(dispositionComments, processingOGADispositionData);
									}
									so70TableRow = new List<string>();
									so70TableRow.Add(so70.OtherAgencyQuotaIdentifier);
									so70TableRow.Add(so70.DispositionDateTime.ToLongTimeString());
									var dispositionCodeList = PGADispositionCodeList.GetPGADispositionCodeList(Factory);
									so70TableRow.Add(SimplifiedEntryStatusNotificationProcessor.GetDispositionDesc(dispositionCodeList, so70.EntryDispositionCode));
									so70TableRow.Add(SimplifiedEntryStatusNotificationProcessor.GetDispositionDesc(dispositionCodeList, so70.PGALineDispositionCode));
									so70TableRow.Add(SimplifiedEntryStatusNotificationProcessor.GetDispositionDesc(dispositionCodeList, so70.EntryLineDispositionCode));
									so70TableRow.Add(so70.BeginningCBPLineNo);
									so70TableRow.Add(so70.BeginningTariffPosition);
									so70TableRow.Add(so70.BeginningOGALineNo);
									so70TableRow.Add(so70.EndingOGALineNo);
									so70TableRow.Add(so70.EndingTariffPosition);
									so70TableRow.Add(so70.EndingCBPLineNo);

									var documentType = so70.DocumentTypeCode;
									var documentTypeDesc = DocumentTypeCodeList.GetDescriptionFromDocumentType(Factory, documentType);
									var documentTypeAndDesc = documentType.IsEmpty ? string.Empty : documentType + " - " + documentTypeDesc;
									so70TableRow.Add(documentTypeAndDesc);

									var entryHoldType = so70.PGAEntryHoldType;
									var entryHoldTypeAndDesc = entryHoldType.IsEmpty ? string.Empty : entryHoldType + " - " + Message.Factory.GetCachedValue<PGAEntryHoldTypeCodeList>().GetDescriptionFromCode(entryHoldType);
									so70TableRow.Add(entryHoldTypeAndDesc);

									dispositionComments = new ZStringBuilder();
									dispositionReasons = new ZStringBuilder();

									if (declaration != null)
									{
										processingOGADispositionData = declaration.EntryStatusesAndErrors.AddOGADispositionDataAndCalculateDeclarationFDAStatus(so70, OGADispositionSourceList.Codes.PGA);
									}

									if (!so70.OtherAgencyQuotaIdentifier.IsEmpty && so70.DispositionDateTime.IsValid)
									{
										pgaEntryStatusMapping[so70.OtherAgencyQuotaIdentifier] = so70;
									}
									dispositionProviders.Add(so70);
								}
								else
								{
									var so71 = block as IPGADispositionDetailProvider;
									if (so71 != null)
									{
										//When storing AMS Confirmation number/FV6 number/AMS LPS number and FWS eDECS number, please remember to suspend PGA Tracking Status like PNC does.
										foreach (string subReasonCode in so71.SubReasonCodes)
										{
											if (!string.IsNullOrEmpty(subReasonCode))
											{
												var subReasonCodeDescription = subReasonList.GetDescriptionFromCode(subReasonCode);

												dispositionReasons.Append(subReasonCodeDescription ?? subReasonCode);
											}
										}

										if (messageAttache != null && processingOGADispositionData != null)
										{
											processingOGADispositionData.OGADispositionDetails.AddNewDispositionDetail(so71);
										}

										if (messageAttache is ISimplifiedMessageLinkedObject linkedObject)
										{
											linkedObject.UpdateACEFDALineInfoIfRequired(processingOGADispositionData);
											linkedObject.UpdateFWSLineInfoIfRequired(processingOGADispositionData);
										}
									}
									else
									{
										var so72 = block as IPGADispositionComments;
										if (so72 != null)
										{
											dispositionComments.Append(so72.CommentsToTradeFromPGA);
										}
									}
								}
							}
						}
					}
				}
			}

			if (declaration != null)
			{
				declaration.EntryPGACusDispositions.AddOrUpdateCusDisposition(pgaEntryStatusMapping);
			}

			SimplifiedEntryStatusNotificationProcessor.ProcessSO70Block(so70Table, so70TableRow, dispositionComments, dispositionReasons);
			SimplifiedEntryStatusNotificationProcessor.UpdateComment(dispositionComments, processingOGADispositionData);

			table.WriteRow("Remarks", remarks.ToString());
			result.Append(table.ToHtml());

			if (declaration != null)
			{
				declaration.LogPGAEntryStatus(previousPGAEntryStatus, Message.EM_MessageType);
				declaration.LogPGALineStatus(dispositionProviders, Message.EM_MessageType);
			}

			if (quotaLineStatusTable != null)
			{
				result.Append("<br/>");
				result.Append(quotaLineStatusTable.ToHtml());
			}

			if (!rejectionNotice.IsEmpty)
			{
				result.Append("<br/>");
				result.Append(string.Format("<b style='color:red'>{0}</b>", rejectionNotice));
			}

			if (so70Table != null)
			{
				result.Append("<br/>");
				result.Append(so70Table.ToHtml());
			}

			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		void UpdateStatus(IMessageAttachee messageAttache, ZStringBuilder emailBody, JobDeclaration declaration)
		{
			var entryHeader = messageAttache as CusEntryHeader;
			var e1 = Message.MessageBlock.MessageBlocks.OfType<AESSE1>().FirstOrDefault();
			if (e1 != null && declaration != null)
			{
				var e2 = Message.MessageBlock.MessageBlocks.OfType<AESSE2>().FirstOrDefault();
				switch (e1.DispositionTypeCode)
				{
					case ENSStatusDispositionCodeList._1:
						declaration.US_IsAIIRequested = true;
						break;

					case ENSStatusDispositionCodeList._2:
					case ENSStatusDispositionCodeList._3:
						if (!declaration.IsReconMessageType)
						{
							declaration.US_PaperlessEntry = YesNoDefaultList.Codes.No;
						}
						break;

					case ENSStatusDispositionCodeList._4:
						if (entryHeader != null)
						{
							if (entryHeader.IsReconEntry)
							{
								if (entryHeader.LogManager.HasAClearLog(new string[] { ReconMessageStatusList.Codes.ClearReconReplace }))
								{
									entryHeader.CH_Status = ReconMessageStatusList.Codes.ErrorReconReplace;
								}
								else
								{
									messageAttache.MessageStatus = ReconMessageStatusList.Codes.ErrorReconOriginal;
								}
							}
							else
							{
								if (entryHeader.LogManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearEntrySummaryReplace }))
								{
									entryHeader.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryReplace;
								}
								else
								{
									entryHeader.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
								}
							}
						}
						else if (declaration.IsDrawback)
						{
							if (declaration.LogManager.HasAClearLog(new string[] { DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryReplacement }))
							{
								declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryReplacement;
							}
							else
							{
								declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryOriginal;
							}
						}
						break;

					case ENSStatusDispositionCodeList._5:
						messageAttache.MessageStatus = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
						break;

					case ENSStatusDispositionCodeList._6:
						messageAttache.MessageStatus = ImportMessageStatusList.Codes.EntrySummaryCanceled;
						declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
						declaration.ReleaseStatus = CRLReleaseStatusList.Codes.CAN;
						declaration.Logs.AddNew(Events.AuthorisationWithdrawn, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification + " - " + CRLReleaseStatusList.Codes.CAN);

						var statement = declaration.RelatedStatement;
						if (statement != null)
						{
							if (statement.CanDeactivateStatementLine)
							{
								statement.DeactivateStatementLine(declaration.EntryFilerCode, declaration.ImportEntryNumber);
							}
							else
							{
								emailBody.Append("<br>");
								emailBody.Append("<b>This entry has just been canceled, but it is on a statement, '" + statement.B2_StatementNumber + "' which is paid or its ACH Authorization is in progress. System has not adjusted any records. Please follow it up with CBP.</b>");
							}
						}
						break;
				}

				var actionIdentificationNumber = ZString.Empty;

				if ((e1.SourceOfActionRequest == "1" || ENSStatusDispositionCodeListLoader.IsFurtherActionRequiredDespiteAbsenceOfActionID(Factory, e1.DispositionTypeCode)) && (e2 == null || e2.ActionIdentificationNumber.IsEmpty))
				{
					actionIdentificationNumber = "ACTION REQUIRED";
				}
				else if (e1.DispositionTypeCode != ENSStatusDispositionCodeList._7 && e2 != null)
				{
					actionIdentificationNumber = e2.ActionIdentificationNumber;
				}

				Message.EM_ApplicationReference = string.Format(CultureInfo.CurrentCulture, "{0}:{1}", e1.DispositionTypeCode, actionIdentificationNumber);
				declaration.ReCalculateENSAction();
			}
		}

		void GenerateEntryNumber(IMessageAttachee messageAttache, HtmlTableCreator table)
		{
			if (messageAttache == null && E1MessageBlock != null)
			{
				table.WriteRow("Entry Number", CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(E1MessageBlock.EntryFilerCode, E1MessageBlock.EntryNumber));
			}
		}

		void GenerateForE1(AESSE1 e1, HtmlTableCreator table)
		{
			var dispositionDescription = rejectionNotice.IsEmpty ? (ENSStatusDispositionCodeListLoader.GetENSStatusDispositionCodeList(Factory).GetDescriptionFromCode(e1.DispositionTypeCode) ?? e1.DispositionTypeCode) : "Entry summary rejected - " + rejectionNotice;

			table.WriteRow("Disposition Type", dispositionDescription);

			if (!e1.NotificationReasonCode.IsEmpty)
			{
				string desc = new ENSStatusRejectReasonList().GetDescriptionFromCode(e1.NotificationReasonCode) ?? e1.NotificationReasonCode;
				table.WriteRow("Reject Reason", desc);
			}

			table.WriteRow("Source of Action/Request", e1.SourceOfActionRequest == "1" ? "Manual Request" : (e1.SourceOfActionRequest == "2" ? "Automatic Request" : ""));

			table.WriteRow("Import Specialist Team", e1.ImportSpecialistTeam);

			table.WriteRow("Date of Action", e1.DateOfAction.ToShortDateString());

			table.WriteRow("Entry Summary Line #", e1.EntrySummaryLineItemIdentifier);

			if (e1.LiquidationDate.IsValid)
			{
				table.WriteRow("Liquidation Date", e1.LiquidationDate.ToShortDateString());
			}
		}

		void GenerateForE2(AESSE2 e2, HtmlTableCreator table)
		{
			table.WriteRow("CBP Staff Name", e2.CBPUser);

			table.WriteRow("Telephone", e2.TelephoneNumber);

			table.WriteRow("Telephone Extension", e2.TelephoneExtensionNumber);

			table.WriteRow("Action Identification Number", e2.ActionIdentificationNumber);
		}

		void GenerateForE4(AESSE4 e4, HtmlTableCreator table, IMessageAttachee messageAttache)
		{
			var quotaLineStatusCode = e4.QuotaLineStatusCode;
			var quotaLineStatus = ZString.Format("{0} - {1}", quotaLineStatusCode, QuotaLineStatusCodes.GetDescriptionFromCode(quotaLineStatusCode));
			var quotaRequestedQtyAndUQ = ZString.Format("{0:N} {1}", e4.RequestedQuotaQuantity, e4.QuotaRequestedUnitOfMeasureCode);
			var reservedQuotaQtyAndUQ = e4.ReservedQuotaQuantity.IsEmpty ? ZString.Empty : ZString.Format("{0:N} {1}", e4.ReservedQuotaQuantity, e4.ReservedQuotaUnitOfMeasureCode);
			table.WriteRow(e4.LineItemIdentifier, quotaLineStatus, quotaRequestedQtyAndUQ, reservedQuotaQtyAndUQ);

			if (!e4.LineItemIdentifier.IsEmpty && messageAttache is CusEntryHeader entryHeader)
			{
				var entryLine = entryHeader.MergedLines.FindByFormattedLineNumber(e4.LineItemIdentifier);
				if (entryLine != null)
				{
					entryLine.QuotaDispositions.AddQuotaDisposition(e4.QuotaLineStatusCode, Message.EM_MessageType, Message.EM_MessageDateTime, $"Requested Quota Qty: {quotaRequestedQtyAndUQ}  Reserved Quota Qty: {reservedQuotaQtyAndUQ}");
				}
			}
		}

		QuotaLineStatusCodeList QuotaLineStatusCodes
		{
			get { return Factory.GetCachedValue<QuotaLineStatusCodeList>(); }
		}

		internal string QuotaRejectionNotice = "An Entry summary is rejected through Quota processing, please see Quota Details for status and proration/apportionment quantities. The filer has 2 working days to respond to a Quota rejection.";
	}
}
