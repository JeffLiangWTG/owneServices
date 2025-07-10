using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	public class SimplifiedEntryStatusNotificationProcessor : ACEABIProcessor
	{
		#region IKeysForBlockingParallelProcessingProvider Members

		protected override ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(CBPEDIMessage message)
		{
			var branchPK = ZGuid.Empty;
			var linkUniqueID = ZGuid.Empty;
			var linkTableName = ZString.Empty;
			var jobNumber = ZString.Empty;
			var matchingData = GetMatchingData(message.MessageBlock.MessageBlocks, message.Factory, message.Branch.GB_GC);
			var linkedObject = matchingData.linkedObject;
			if (linkedObject != null)
			{
				var branch = linkedObject.Branch;
				branchPK = branch.PK;
				if (linkedObject.LinkedObject is BusinessObject bizObj)
				{
					linkUniqueID = bizObj.PK;
					linkTableName = bizObj.TableName;
				}
				jobNumber = linkedObject.TopLevelBusinessObject is IJobNumber topLevelBusinessObject ? topLevelBusinessObject.JobNumber : linkedObject.TopLevelBizObjReferenceNumber;
			}
			return new LinkedBusinessObjectMetaData(linkTableName, linkUniqueID, branchPK, jobNumber);
		}

		protected override HashSet<string> TryFindSerializationKeys(CBPEDIMessage message, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var result = new HashSet<string>();
			var matchingData = GetMatchingData(message.MessageBlock.MessageBlocks, message.Factory, message.Branch.GB_GC);
			var linkedObject = matchingData.linkedObject;
			if (linkedObject != null)
			{
				var branch = linkedObject.Branch;
				result.Add(USIUniversalCustomsMessageProcessor.GetEntryJobNumberKey(matchingData.entryFilerCode, matchingData.entryNumber, branch.GB_GC));
				if (linkedObject is IMessageAttacheeInDeclaration messageAttacheeInDeclaration)
				{
					var jobReferenceNumber = messageAttacheeInDeclaration.JobReferenceNumber;
					var jobNumber = linkedBusinessObjectMetaData.JobNumber;
					if (!jobNumber.IsEmpty && !jobReferenceNumber.IsEmpty && jobNumber != jobReferenceNumber)
					{
						result.Add(jobReferenceNumber);
					}
				}
			}
			return result;
		}

		#endregion

		public override void Process()
		{
			if (!Message.EM_MessageNum.IsEmpty)
			{
				Message.EM_MessageNum = ZString.Empty; //unsolisited status message
			}

			var matchingData = GetMatchingData(messageBlocks, Factory, GlbCompany.CurrentCompany.PK);
			var messageLinkedObject = matchingData.linkedObject;
			if (messageLinkedObject != null)
			{
				Message.EM_LinkedObject = messageLinkedObject as BusinessObject;
				Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseStatusNotification;
				messageLinkedObject.Messages.Load();
			}

			var html = new ZStringBuilder();
			var messageLinkedParentBO = messageLinkedObject == null ? null : messageLinkedObject.ParentBusinessObject;
			if (messageLinkedParentBO == null)
			{
				html.Append("No entry found. No ACE Cargo Release Entry has been found for the entry number: " + matchingData.entryFilerCode + "-" + matchingData.entryNumber);
			}
			else
			{
				var importer = messageLinkedParentBO.Importer;
				var importerName = importer != null ? importer.OH_FullName : ZString.Empty;
				if (!importerName.IsEmpty)
				{
					html.Append("<b>Importer: " + importerName + "</b>");
					html.Append("<br>");
					html.Append("<br>");
				}
			}

			UpdateApplicationReference(messageBlocks.Where(x => x is ASESSO20Base));

			var firstReleaseBlock = messageLinkedObject != null ? Message.GetFirstReleaseDetailBlock() : null;
			var hasEarlierReleaseDisposition = firstReleaseBlock != null && firstReleaseBlock.HasEarlierReleaseDispositionDateTime(messageLinkedObject.GetReleaseDispositionMessages(Message));
			UpdateMessageLinkedParentBO(messageBlocks, Message, messageLinkedObject, hasEarlierReleaseDisposition);
			UpdateEntryStatus(messageLinkedObject);
			var emailBodyDetails = GetEmailBody(messageBlocks, html, messageLinkedObject, Message, hasEarlierReleaseDisposition);

			var jobNumber = "Unknown";

			if (messageLinkedParentBO != null && Message.EM_MessageOwner != Reprocessing)
			{
				jobNumber = messageLinkedParentBO.ReferenceNumber;
				var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(messageLinkedObject);
				EmailDef email;
				var branch = messageLinkedParentBO.Branch;
				if (new HtmlResponseEmailGenerator().TryGenerateEmail(url, jobNumber, "ACE Cargo Release Status", emailBodyDetails, "", false, out email, branch))
				{
					var registryItem = GetEmailGroupRegistryItem() as ManifestGroupNotificationRegistryItem;
					var groupNotification = registryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);

					var shouldSendErrorNotificationsOnly = groupNotification.SendErrorOnly;
					if (!shouldSendErrorNotificationsOnly)
					{
						var messages = messageLinkedObject.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoRelease);
						if (messages != null && messageLinkedParentBO.EntrySummaryEntry != null)
						{
							messages = messages.Concat(messageLinkedParentBO.EntrySummaryEntry.Messages.OfType<EDIMessage>().Where(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummary));
						}
						var emailAddresses = messageLinkedParentBO.GetEmailRecipients(messages);

						var recipientCalculator = new EmailRecipientCalculator(groupNotification.SendMode, groupNotification.SendGroupPK, emailAddresses, ZGuid.Empty);
						recipientCalculator.SendNotifications(Factory, email, registryItem);
					}
				}
			}
			Message.EM_MessageOwner = ZString.Empty;
		}

		static (ZString entryFilerCode, ZString entryNumber, ISimplifiedMessageLinkedObject linkedObject) GetMatchingData(List<MessageBlock> messageBlocks, BusinessObjectFactory factory, ZGuid companyPK)
		{
			var entryFilerCode = ZString.Empty;
			var entryNumber = ZString.Empty;
			var so10 = messageBlocks.OfType<ASESSO10>().FirstOrDefault();
			if (so10 != null)
			{
				entryFilerCode = so10.EntryFilerCode;
				entryNumber = so10.EntryNumber;
			}

			var jobReference = ZString.Empty;
			var so20BlockWithJobReference = messageBlocks.OfType<ASESSO20>().FirstOrDefault(x => x.ReferenceIdentifierQualifier == ReferenceIdentifierCodeList.Codes.UserDefinedReferenceNumber);
			if (so20BlockWithJobReference != null)
			{
				jobReference = so20BlockWithJobReference.ReferenceIdentifier;
			}

			return (entryFilerCode, entryNumber, FindByEntryNumberAndFilerCode(factory, entryNumber, entryFilerCode, jobReference, companyPK));
		}

		static ISimplifiedMessageLinkedObject FindByEntryNumberAndFilerCode(BusinessObjectFactory factory, ZString entryNumber, ZString filerCode, ZString jobReference, ZGuid companyPK)
		{
			var linkedObjectsByEntryNumber = new List<ISimplifiedMessageLinkedObject>();
			if (!entryNumber.IsEmpty && !filerCode.IsEmpty)
			{
				var parentFilter = new ZQuery(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
				parentFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_ParentTable, JobDeclarationSchema.Constants.TableName);
				parentFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_ParentTable, CusUSLVConsignmentSchema.Constants.TableName);

				var entryNumberFilter = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNumber);
				entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
				entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
				entryNumberFilter.AddToFilter(parentFilter);

				var cusEntryHeaderFilter = new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease);
				foreach (var bizO in factory.Load<CusEntryNumber>(entryNumberFilter))
				{
					ISimplifiedMessageLinkedObject result = null;
					JobDeclaration declaration = null;
					switch (bizO.CE_ParentTable)
					{
						case CusEntryHeaderSchema.Constants.TableName:
							CusEntryHeader entryHeader = factory.Load(typeof(Customs.Business.CusEntryHeader), bizO.CE_ParentID) as CusEntryHeader;
							if (entryHeader != null && entryHeader.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ACECargoRelease)
							{
								declaration = entryHeader.Declaration;
							}
							if (declaration != null &&
								declaration.JE_GC == companyPK &&
								entryHeader.EntryFilerCode == filerCode)
							{
								result = entryHeader;
							}
							break;
						case JobDeclarationSchema.Constants.TableName:
							declaration = factory.Load(typeof(BaseJobDeclaration), bizO.CE_ParentID) as JobDeclaration;
							if (declaration != null &&
								declaration.JE_GC == companyPK)
							{
								var entries = (CusEntryHeader[])declaration.ActiveEntryHeaders.Find(cusEntryHeaderFilter);
								if (entries.Length > 0 && entries[0].EntryFilerCode == filerCode)
								{
									result = entries[0];
								}
							}
							break;
						case CusUSLVConsignmentSchema.Constants.TableName:
							var consignment = factory.Load(CusUSLVConsignmentSchema.Constants.Prefix, bizO.CE_ParentID) as ISimplifiedMessageLinkedObject;
							if (consignment != null && consignment.EntryFilerCode == filerCode)
							{
								result = consignment;
							}
							break;
					}

					if (result != null)
					{
						linkedObjectsByEntryNumber.Add(result);
					}
				}
			}
			if (linkedObjectsByEntryNumber.Count == 1)
			{
				return linkedObjectsByEntryNumber[0];
			}
			else
			{
				return linkedObjectsByEntryNumber.FirstOrDefault(x => x.TopLevelBizObjReferenceNumber.Contains(jobReference));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		internal static string GetEmailBody(List<MessageBlock> messageBlocks, ZStringBuilder html, ISimplifiedMessageLinkedObject messageLinkedObject, MQEDIMessage message, bool hasEarlierReleaseDisposition)
		{
			HtmlTableCreator billTable = null;
			var billDetails = new Dictionary<List<string>, HtmlTableCreator>();
			var billsAdded = new List<string>();
			ASESSO40Base blockSO40 = null;

			HtmlTableCreator tariffLineTable = null;
			HtmlTableCreator referenceDetailsTable = null;
			HtmlTableCreator so60Table = null;

			var so20CMTTableRow = new ZStringBuilder();

			HtmlTableCreator so70Table = null;
			List<string> so70TableRow = null;
			var dispositionComments = new ZStringBuilder();
			var dispositionReasons = new ZStringBuilder();
			var rejectionreasonCodeList = message.Factory.GetCachedValue<RejectionReasonCodeList>();
			var subReasonList = message.Factory.GetCachedValue<OGADispositionSubReasonList>();
			var messageLinkedParentBO = messageLinkedObject == null ? null : messageLinkedObject.ParentBusinessObject;

			var releaseDetailsRemainSame = hasEarlierReleaseDisposition;
			var hasPNCNumberInSO71 = ZBool.False;

			foreach (MessageBlock block in messageBlocks)
			{
				if (block is ASESSO10Base)
				{
					var so10 = block as ASESSO10Base;
					var entryDetailsTable = new HtmlTableCreator();
					entryDetailsTable.WriteRow("District Port of Entry", so10.DistrictPortOfEntry);
					entryDetailsTable.WriteRow("Entry Filer Code", so10.EntryFilerCode);
					entryDetailsTable.WriteRow("Entry Number", so10.EntryNumber);
					entryDetailsTable.WriteRow("Entry Type", so10.EntryTypeCode);
					if (!SocialSecurityNumberValidator.IsValidSSN(so10.ImporterOfRecordNumber))
					{
						entryDetailsTable.WriteRow("Importer of Record Number", so10.ImporterOfRecordNumber);
					}
					entryDetailsTable.WriteRow("Carrier Code", so10.CarrierCode);
					entryDetailsTable.WriteRow("Estimated Date of Arrival", so10.EstimatedDateOfArrival.ToShortDateString());
					entryDetailsTable.WriteRow("Split Shipment Release Code", so10.SplitShipmentReleaseCode);
					entryDetailsTable.WriteRow("Importing Conveyance Name", so10.VesselName);
					entryDetailsTable.WriteRow("Voyage/Flight/Trip Number", so10.VoyageFlightTripManifestNumber);
					entryDetailsTable.WriteRow("PGA Correction Response", so10.CorrectionResponseIndicator);
					html.Append(entryDetailsTable.ToHtml());
					html.Append("<br>");
				}
				else if (block is ASESSO20Base)
				{
					var so20 = block as ASESSO20Base;
					if (referenceDetailsTable == null)
					{
						referenceDetailsTable = new HtmlTableCreator();
					}

					if (so20.ReferenceIdentifierQualifier == ReferenceIdentifierQualifierCodeList.Codes.CMT)
					{
						so20CMTTableRow.Append(so20.ReferenceIdentifier);
					}
					else if (so20.ReferenceIdentifierQualifier == ReferenceIdentifierQualifierCodeList.Codes.RSN)
					{
						referenceDetailsTable.WriteRow(ReferenceIdentifierQualifierCodeList.Descriptions.RSN, so20.ReferenceIdentifier + " " + rejectionreasonCodeList.GetDescriptionFromCode(so20.ReferenceIdentifier));
					}
					else if (so20.ReferenceIdentifierQualifier == ReferenceIdentifierQualifierCodeList.Codes.RRN)
					{
						referenceDetailsTable.WriteRow(ReferenceIdentifierQualifierCodeList.Descriptions.RRN, so20.ReferenceIdentifier);
					}
				}
				else if (block is ASESSO30Base)
				{
					var so30 = block as ASESSO30Base;
					if (tariffLineTable == null)
					{
						tariffLineTable = new HtmlTableCreator(new string[] { "Line No.", "Country of Origin", "Tariff Number" });
					}
					tariffLineTable.WriteRow(so30.LineItemIdentifier, so30.CountryOfOrigin, so30.HTSNumber);
				}
				else if (block is ASESSO40Base)
				{
					var previousBlock = messageBlocks[messageBlocks.FindIndex(x => x == block) - 1];
					if (previousBlock == null || !(previousBlock is ASESSO40Base))
					{
						if (billTable != null)
						{
							billDetails.Add(billsAdded, billTable);
						}
						billsAdded = new List<string>();
						billTable = new HtmlTableCreator();
					}

					blockSO40 = block as ASESSO40Base;
					if (!billsAdded.Contains(blockSO40.IssuerCodeOfBillOfLadingNumber + blockSO40.BillOfLadingNumber))
					{
						billsAdded.Add(blockSO40.IssuerCodeOfBillOfLadingNumber + blockSO40.BillOfLadingNumber);
					}

					var manifestQuantity = ZInt.ParseSafe(blockSO40.ManifestedQuantity, 0);
					if (manifestQuantity > 0)
					{
						billTable.WriteRow("Manifest Quantity", manifestQuantity);
					}
					if (!blockSO40.Quantity.IsEmpty)
					{
						billTable.WriteRow("Reported Quantity", blockSO40.Quantity + " " + blockSO40.UnitOfMeasure);
					}
				}
				else if (block is ASESSO42Base)
				{
					var block42 = block as ASESSO42Base;
					var data = (!block42.InbondNumber.IsEmpty ? "Inbond #/" : "") + (!block42.InbondEntryType.IsEmpty ? " Type/" : "") +
						(!block42.InBondQuantity.IsEmpty ? " Qty/" : "") + (!block42.USPortOfInbondArrival.IsEmpty ? " Arrival Port/" : "") +
						(!block42.USPortOfInbondDeparture.IsEmpty ? " Departure Port/" : "") + (!block42.DateOfInbondArrival.IsEmpty ? " ATD/" : "") +
						(!block42.InbondCreateDate.IsEmpty ? " DTD" : "");

					var values = (!block42.InbondNumber.IsEmpty ? block42.InbondNumber.ToString() + "/" : "") + (!block42.InbondEntryType.IsEmpty ? block42.InbondEntryType.ToString() + "/" : "") +
						(!block42.InBondQuantity.IsEmpty ? block42.InBondQuantity.ToString() + "/" : "") + (!block42.USPortOfInbondArrival.IsEmpty ? block42.USPortOfInbondArrival.ToString() + "/" : "") +
						(!block42.USPortOfInbondDeparture.IsEmpty ? block42.USPortOfInbondDeparture.ToString() + "/" : "") + (!block42.DateOfInbondArrival.IsEmpty ? block42.DateOfInbondArrival.ToString() + "/" : "") +
						(!block42.InbondCreateDate.IsEmpty ? block42.InbondCreateDate.ToString() + "/" : "");

					billTable.WriteRow(data.TrimEnd('/'), values.TrimEnd('/'));
				}
				else if (block is ASESSO50Base)
				{
					var billNum = blockSO40 != null ? blockSO40.BillOfLadingNumber : ZString.Empty;

					var so50 = block as ASESSO50Base;
					if (billTable == null)
					{
						billTable = new HtmlTableCreator();
					}

					so50.ProcessBillDispositionDetails(billTable);

					if (!so50.CarrierCode.IsEmpty)
					{
						billTable.WriteRow("Carrier Code", so50.CarrierCode);
					}
					if (!so50.DateOfArrival.IsEmpty)
					{
						billTable.WriteRow("Date Of Arrival", so50.DateOfArrival);
					}
					if (!so50.DistrictPortOfArrival.IsEmpty)
					{
						billTable.WriteRow("Port Of Arrival", so50.DistrictPortOfArrival);
					}
					if (!so50.SplitIndicator.IsEmpty)
					{
						var splitIndicatorDiscrepancy = string.Empty;

						Bill bill = messageLinkedParentBO?.GetFirstBillHasSameNumber(billNum);

						if (bill != null && bill.US_SESplitShip && so50.SplitIndicator == YesNoDefaultList.Codes.No)
						{
							splitIndicatorDiscrepancy = "\r\nThis bill is marked as Split, but Customs indicates that it is not. The system did not update the flag due to this discrepancy. Please check.";
						}

						billTable.WriteRow("Split Indicator", so50.SplitIndicator + splitIndicatorDiscrepancy);
					}
					if (!so50.VoyageFlightTripManifestNumber.IsEmpty)
					{
						billTable.WriteRow("Voyage/Flight/Trip Number", so50.VoyageFlightTripManifestNumber);
					}
				}
				else if (block is ASESSO60Base)
				{
					var so60 = block as ASESSO60Base;

					if (so60Table == null)
					{
						so60Table = new HtmlTableCreator(new string[] { "Data", "Value and Description" });
					}
					so60Table.WriteRow("Disposition Action/Date", so60.DispositionActionCode + " " + so60.NarrativeMessage + "/" + ((IDispositionDetailProvider)so60).DispositionDateTime.ToLongTimeString());

					if (!so60.ReleaseDate.IsEmpty)
					{
						var releaseOriginDesc = message.Factory.GetCachedValue<ReleaseOriginCodeList>().GetDescriptionFromCode(so60.ReleaseOrigin);

						so60Table.WriteRow("Release Origin/Date", (releaseOriginDesc ?? so60.ReleaseOrigin.ToString()) + "/" + so60.ReleaseDate.ToShortDateString());
					}

					var documentType = so60.DocumentType;
					if (!documentType.IsEmpty)
					{
						var documentTypeDesc = DocumentTypeCodeList.GetDescriptionFromDocumentType(message.Factory, documentType);
						so60Table.WriteRow("Document Type", documentType + " - " + documentTypeDesc);
					}
				}
				else if (block is IPGADispositionProvider)
				{
					var so70 = (IPGADispositionProvider)block;

					if (so70Table == null)
					{
						so70Table = new HtmlTableCreator(new string[] { "Agency/Quota Indicator", "Disposition Date", "PGA Entry Status Desc", "PGA Line Status Desc", "Review Reason Status Desc", "CBP Line Status", "Beg. CBP Line", "Beg. Tariff", "Beg. PGA Line", "End PGA Line", "End Tariff", "End CBP Line", "Document Type", "PGA Entry Hold Type", "Comment", "Reasons" });
					}
					else
					{
						ProcessSO70Block(so70Table, so70TableRow, dispositionComments, dispositionReasons);
					}
					so70TableRow = new List<string>();
					so70TableRow.Add(so70.OtherAgencyQuotaIdentifier);
					so70TableRow.Add(so70.DispositionDateTime.ToLongTimeString());
					var dispositionCodeList = PGADispositionCodeList.GetPGADispositionCodeList(message.Factory);
					so70TableRow.Add(GetDispositionDesc(dispositionCodeList, so70.EntryDispositionCode));
					so70TableRow.Add(GetDispositionDesc(dispositionCodeList, so70.PGALineDispositionCode));
					var reviewReasonDesc = PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(message.Factory, so70.ReviewReasonCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason);
					so70TableRow.Add(reviewReasonDesc);
					so70TableRow.Add(GetDispositionDesc(dispositionCodeList, so70.EntryLineDispositionCode));
					so70TableRow.Add(so70.BeginningCBPLineNo);
					so70TableRow.Add(so70.BeginningTariffPosition);
					so70TableRow.Add(so70.BeginningOGALineNo);
					so70TableRow.Add(so70.EndingOGALineNo);
					so70TableRow.Add(so70.EndingTariffPosition);
					so70TableRow.Add(so70.EndingCBPLineNo);

					var documentTypeFromso70 = so70.DocumentTypeCode;
					var documentTypeDesc = DocumentTypeCodeList.GetDescriptionFromDocumentType(message.Factory, documentTypeFromso70);
					var documentTypeAndDesc = documentTypeFromso70.IsEmpty ? string.Empty : documentTypeFromso70 + " - " + documentTypeDesc;
					so70TableRow.Add(documentTypeAndDesc);

					var entryHoldType = so70.PGAEntryHoldType;
					var entryHoldTypeAndDesc = entryHoldType.IsEmpty ? string.Empty : entryHoldType + " - " + message.Factory.GetCachedValue<PGAEntryHoldTypeCodeList>().GetDescriptionFromCode(entryHoldType);
					so70TableRow.Add(entryHoldTypeAndDesc);

					dispositionComments = new ZStringBuilder();
					dispositionReasons = new ZStringBuilder();
				}
				else if (block is IPGADispositionDetailProvider)
				{
					var so71 = block as IPGADispositionDetailProvider;
					foreach (string subReasonCode in so71.SubReasonCodes)
					{
						if (!string.IsNullOrEmpty(subReasonCode))
						{
							var subReasonCodeDescription = subReasonList.GetDescriptionFromCode(subReasonCode);
							dispositionReasons.Append(subReasonCodeDescription ?? subReasonCode);
						}
					}

					var identifierID = so71.ReferenceIDQualifier.ToString();
					hasPNCNumberInSO71 |= OGADispositionReferenceQualifierList.Codes.PriorNoticeConfirmationNum.Equals(identifierID) && !so71.ReferenceID.IsEmpty;
				}
				else if (block is IPGADispositionComments)
				{
					var so72 = block as IPGADispositionComments;
					dispositionComments.Append(so72.CommentsToTradeFromPGA);
				}
			}

			ProcessSO70Block(so70Table, so70TableRow, dispositionComments, dispositionReasons);

			if (billTable != null)
			{
				billDetails.Add(billsAdded, billTable);
			}

			foreach (KeyValuePair<List<string>, HtmlTableCreator> keyValuePair in billDetails)
			{
				var billNumbers = new ZStringBuilder();
				keyValuePair.Key.ForEach(x => billNumbers.Append(x));
				html.Append("<b> Bill " + billNumbers.ToStringWithDelimiterBetweenAppends("/ ") + "</b>");

				html.Append(keyValuePair.Value.ToHtml());
				html.Append("<br>");
			}

			if (tariffLineTable != null)
			{
				html.Append(tariffLineTable.ToHtml());
				html.Append("<br>");
			}

			if (referenceDetailsTable != null)
			{
				if (!so20CMTTableRow.IsEmpty)
				{
					var cell1 = new CellWithFormatting("Comments", "style", "color:red");
					var cell2 = new CellWithFormatting(so20CMTTableRow.ToString(), "style", "color:red");
					referenceDetailsTable.WriteRowWithFormatting(new NameValueCollection { { "style", "color:red" } }, new[] { cell1, cell2 });
				}
				html.Append(referenceDetailsTable.ToHtml());
				html.Append("<br>");
			}

			if (so60Table != null)
			{
				html.Append(so60Table.ToHtml());
				html.Append("<br>");
			}

			if (so70Table != null)
			{
				html.Append(so70Table.ToHtml());
				html.Append("<br>");
			}

			if (hasPNCNumberInSO71)
			{
				html.Append("<b>" + PNCNumbersReceived + "</b>");
				html.Append("<br>");
			}

			var remarks = messageLinkedParentBO?.GetUnableToDeactivateStatementLineRemarkIfNecessary() ?? ZString.Empty;
			if (!remarks.IsEmpty)
			{
				html.Append("<b>" + remarks + "</b>");
				html.Append("<br>");
				html.Append("<br>");
			}

			(ZString suppressMesageLog, bool redAndBold) = messageLinkedParentBO?.GetSuppresseMessageLogs() ?? (ZString.Empty, false);
			if (!suppressMesageLog.IsEmpty)
			{
				if (redAndBold)
				{
					html.Append("<b style=\"color:red\">" + suppressMesageLog + "</b>");
				}
				else
				{
					html.Append(suppressMesageLog);
				}
				html.Append("<br>");
				html.Append("<br>");
			}

			if (releaseDetailsRemainSame)
			{
				html.Append(BelatedDispositionReleaseNotificationWarning);
			}
			return html.ToString();
		}

		void UpdateApplicationReference(IEnumerable<MessageBlock> messageBlocks)
		{
			foreach (ASESSO20Base so20 in messageBlocks)
			{
				if (so20.ReferenceIdentifierQualifier == ReferenceIdentifierQualifierCodeList.Codes.CMT)
				{
					Message.EM_ApplicationReference = ReferenceIdentifierQualifierCodeList.Codes.CMT;
					if (Message.EM_LinkedObject is CusEntryHeader header)
					{
						header.Declaration?.ReCalculateCRLAction();
					}
					break;
				}
			}
		}

		void UpdateEntryStatus(ISimplifiedMessageLinkedObject messageLinkedObject)
		{
			if (messageLinkedObject != null)
			{
				var originalStatus = messageLinkedObject.MessageStatus;
				var newStatus = ZString.Empty;
				if (originalStatus == ImportMessageStatusList.Codes.ReplaceRequestPending || originalStatus == ImportMessageStatusList.Codes.ClearACECargoReleaseReplace)
				{
					if (messageLinkedObject.HasSpecifiedDispositionCodes(new[] { CargoReleaseProcessingResultList.Codes.CorrectionRequestRejected }) && !messageLinkedObject.HasSpecifiedDispositionCodes(new[] { CargoReleaseProcessingResultList.Codes.Released }))
					{
						newStatus = ImportMessageStatusList.Codes.ReplaceRequestRejected;
					}
					else
					{
						newStatus = ImportMessageStatusList.Codes.ClearACECargoReleaseReplace;
					}
				}

				if (!newStatus.IsEmpty)
				{
					messageLinkedObject.MessageStatus = newStatus;
				}
			}
		}

		internal static void UpdateMessageLinkedParentBO(List<MessageBlock> messageBlocks, MQEDIMessage message, ISimplifiedMessageLinkedObject messageLinkedObject, bool hasEarlierReleaseDisposition)
		{
			if (messageLinkedObject != null)
			{
				UpdateMessageLinkedParentBO(messageBlocks, message, messageLinkedObject, hasEarlierReleaseDisposition, null);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal static void UpdateMessageLinkedParentBO(List<MessageBlock> messageBlocks, MQEDIMessage message, ISimplifiedMessageLinkedObject messageLinkedObject, bool hasEarlierReleaseDisposition, IEnumerable<KeyValuePair<PGALineKey, ZDateTime>> latestPGADispositionDateForEachPGA)
		{
			var dispositionComments = new ZStringBuilder();
			OGADispositionData processingOgaDispositionData = null;
			bool process71And72Blocks = false;
			var messageLinkedParentBO = messageLinkedObject != null ? messageLinkedObject.ParentBusinessObject : null;
			var pgaEntryStatusMapping = new Dictionary<ZString, IPGADispositionProvider>();
			var dispositionProviders = new List<IPGADispositionProvider>();
			var previousPGAEntryStatus = messageLinkedParentBO?.GetPGAEntryStatus() ?? new Dictionary<ZString, ZString>();
			ASESSO10Base blockSO10 = null;
			ASESSO40Base blockSO40 = null;
			var blockSO40List = new List<ASESSO40Base>();
			var blockSO50List = new List<ASESSO50Base>();
			var pgaLineStatusList = new List<IPGADispositionProvider>();

			foreach (var block in messageBlocks)
			{
				if (block is ASESSO10Base)
				{
					blockSO10 = block as ASESSO10Base;
				}
				else if (block is ASESSO40Base)
				{
					blockSO40 = block as ASESSO40Base;
					blockSO40List.Add(blockSO40);
				}
				else if (block is ASESSO50Base)
				{
					if (!hasEarlierReleaseDisposition)
					{
						var billNum = blockSO40 != null ? blockSO40.BillOfLadingNumber : ZString.Empty;
						if (messageLinkedParentBO != null)
						{
							var bill = messageLinkedParentBO.GetFirstBillHasSameNumber(billNum);
							var so50 = (ASESSO50Base)block;
							blockSO50List.Add(so50);
							if (bill != null)
							{
								UpdateBill(bill, so50);

								if (so50.SplitIndicator == YesNoDefaultList.Codes.Yes)
								{
									bill.US_SESplitShip = true;
								}
							}
						}
					}
				}
				else if (block is ASESSO60Base)
				{
					var so60 = block as ASESSO60Base;
					if (messageLinkedObject != null && !hasEarlierReleaseDisposition)
					{
						so60.UpdateMessageLinkedParentBO(messageLinkedObject, message.EM_MessageType);
					}
				}
				else if (block is IPGADispositionProvider)
				{
					var so70 = (IPGADispositionProvider)block;

					if (process71And72Blocks)
					{
						UpdateComment(dispositionComments, processingOgaDispositionData);
					}

					process71And72Blocks = false;
					dispositionComments = new ZStringBuilder();

					if (messageLinkedParentBO != null && (latestPGADispositionDateForEachPGA == null || latestPGADispositionDateForEachPGA.Any(x => x.Key.Equals(new PGALineKey(so70)) && x.Value == so70.DispositionDateTime)))
					{
						processingOgaDispositionData = messageLinkedParentBO.AddOGADispositionData(so70, OGADispositionSourceList.Codes.PGA);
						if (!so70.OtherAgencyQuotaIdentifier.IsEmpty && so70.DispositionDateTime.IsValid)
						{
							pgaEntryStatusMapping[so70.OtherAgencyQuotaIdentifier] = so70;
						}
						dispositionProviders.Add(so70);
						process71And72Blocks = true;
					}
					pgaLineStatusList.Add(so70);
				}
				else if (block is IPGADispositionDetailProvider)
				{
					if (process71And72Blocks && messageLinkedObject != null)
					{
						var so71 = block as IPGADispositionDetailProvider;
						if (processingOgaDispositionData != null)
						{
							processingOgaDispositionData.OGADispositionDetails.AddNewDispositionDetail(so71);
						}

						if (message.EM_MessageOwner != Reprocessing)
						{
							messageLinkedObject.UpdateACEFDALineInfoIfRequired(processingOgaDispositionData);
							messageLinkedObject.UpdateFWSLineInfoIfRequired(processingOgaDispositionData);
						}
					}
				}
				else if (block is IPGADispositionComments)
				{
					if (process71And72Blocks)
					{
						var so72 = block as IPGADispositionComments;
						dispositionComments.Append(so72.CommentsToTradeFromPGA);
					}
				}
			}

			messageLinkedParentBO.AddOrUpdateCusDisposition(pgaEntryStatusMapping, pgaLineStatusList);

			if (process71And72Blocks)
			{
				UpdateComment(dispositionComments, processingOgaDispositionData);
			}

			if (messageLinkedParentBO != null && message.EM_MessageOwner != Reprocessing)
			{
				if (!hasEarlierReleaseDisposition)
				{
					messageLinkedObject.UpdateAndLogStatusDetails(message.EM_MessageType, false);
					UpdateMessageLinkedParentBOAfterReleased(messageLinkedParentBO, blockSO10, blockSO40List, blockSO50List);
				}
				messageLinkedParentBO.LogPGAEntryAndLineStatus(previousPGAEntryStatus, dispositionProviders, message.EM_MessageType);
			}
		}

		static void UpdateMessageLinkedParentBOAfterReleased(IEntryHeaderParentBusinessObject messageLinkedParentBO, ASESSO10Base blockSO10, IEnumerable<ASESSO40Base> blockSO40List, IEnumerable<ASESSO50Base> blockSO50List)
		{
			var formalEntry = messageLinkedParentBO.EntrySummaryEntry;
			var shouldUpdateManifestDetails = messageLinkedParentBO.ShouldUpdateDeclarationWithCargoReleaseResults && messageLinkedParentBO.ReleaseStatus == CRLReleaseStatusList.Codes.REL && (formalEntry == null || !formalEntry.HasBeenLodgedAtCustoms);

			if (shouldUpdateManifestDetails)
			{
				messageLinkedParentBO.UpdateMessageLinkedParentBOAfterReleased(blockSO10, blockSO40List, blockSO50List);
			}
		}

		public static void UpdateBill(Bill bill, ASESSO50Base so50)
		{
			if (bill != null)
			{
				var dispositionCode = so50.DispositionCode;
				bill.DispositionCodes.AddNewIfNotExist(dispositionCode, so50.GetDispositionDateTime(), BillDispositionSourceList.Codes.SO);
				UpdateBillMessageStatus(dispositionCode, bill);
			}
		}

		public static void UpdateBillMessageStatus(ZString dispositionCode, Bill bill)
		{
			if (!dispositionCode.IsEmpty && CargoReleaseProcessingResultList.IsHoldExamOrHoldExamRemove(bill.Declaration.TransportMode, bill.Factory, dispositionCode))
			{
				if (!bill.CU_MessageStatus.Contains(dispositionCode, StringComparison.CurrentCulture))
				{
					if (CargoReleaseProcessingResultList.IsHold(dispositionCode) || DispositionCodeListLoader.IsExam(bill.Declaration.TransportMode, bill.Factory, dispositionCode) || DispositionCodeListLoader.IsHold(bill.Declaration.TransportMode, bill.Factory, dispositionCode))
					{
						bill.CU_MessageStatus = bill.CU_MessageStatus.IsEmpty ? dispositionCode : ZString.Format("{0}/{1}", bill.CU_MessageStatus, dispositionCode);
					}
					else if (!bill.CU_MessageStatus.IsEmpty)
					{
						var holdStatuses = bill.CU_MessageStatus.Split('/');
						foreach (var holdStatus in holdStatuses)
						{
							if (CargoReleaseProcessingResultList.IsHoldRemoved(holdStatus, dispositionCode) || DispositionCodeListLoader.IsHoldRemovedOrExamCompleted(bill.Declaration.TransportMode, bill.Factory, holdStatus, dispositionCode))
							{
								bill.CU_MessageStatus = bill.CU_MessageStatus.Replace(holdStatus, "").Replace("//", "/").Trim('/');
							}
						}
					}
				}
			}
		}

		protected override IRegistryItem GetEmailGroupRegistryItem()
		{
			if (Message.EM_LinkTable == CusUSLVConsignmentSchema.Constants.TableName)
			{
				return USCustomsDataRegistry.Instance.LowValueEntriesReleaseMessages;
			}

			return base.GetEmailGroupRegistryItem();
		}

		internal static void UpdateComment(ZStringBuilder dispositionComments, OGADispositionData processingOgaDispositionData)
		{
			if (processingOgaDispositionData != null)
			{
				var dispositionComment = ((ZString)dispositionComments.ToStringWithDelimiterBetweenAppends(" ")).Left(USOGADispositionDataAddInfo.Schema.US_CommentMaxLength);
				processingOgaDispositionData.US_Comment = dispositionComment;
			}
		}

		internal static string GetDispositionDesc(ICodeDescriptionPairList list, string dispositionCode)
		{
			var dispositionCodeDesc = list.GetDescriptionFromCode(dispositionCode);
			if (string.IsNullOrEmpty(dispositionCodeDesc))
			{
				dispositionCodeDesc = dispositionCode;
			}
			else
			{
				dispositionCodeDesc = dispositionCode + " " + dispositionCodeDesc;
			}
			return dispositionCodeDesc;
		}

		internal static void ProcessSO70Block(HtmlTableCreator so70Table, List<string> so70TableRow, ZStringBuilder dispositionComments, ZStringBuilder dispositionReasons)
		{
			if (so70TableRow != null)
			{
				var dispositionComment = dispositionComments == null ? ZString.Empty : ((ZString)dispositionComments.ToStringWithDelimiterBetweenAppends(" ")).Left(USOGADispositionDataAddInfo.Schema.US_CommentMaxLength);
				so70TableRow.Add(dispositionComment);
				if (dispositionReasons == null)
				{
					so70TableRow.Add(ZString.Empty);
				}
				else
				{
					so70TableRow.Add(dispositionReasons.ToStringWithDelimiterBetweenAppends(", "));
				}
				so70Table.WriteRow(so70TableRow.ToArray());
			}
		}
	}
}
