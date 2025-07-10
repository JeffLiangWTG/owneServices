using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification)]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class ACEPriorNoticeStatusNotificationProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.FDAPriorNoticeStatus;

			var emailBody = new StringBuilder();
			emailBody.Append("<b>ACE Prior Notice Status Notification response</b>");
			emailBody.Append("<br />");
			emailBody.Append("<br />");

			var referenceQualifier = ZString.Empty;
			var filerOrIssuerCode = ZString.Empty;
			var entryOrBillNumber = ZString.Empty;
			var transportMode = ZString.Empty;
			var envelopeNumber = ZString.Empty;

			var po10 = GetFirstMessageBlock<SESNPO10>();
			if (po10 != null)
			{
				referenceQualifier = po10.ReferenceQualifierCode;
				filerOrIssuerCode = po10.FilerOrIssuerCodeForReferenceIdentifier;
				entryOrBillNumber = po10.ReferenceIdentifierNumber;
				transportMode = po10.ModeOfTransportationMOTCode;
				envelopeNumber = po10.EnvelopeNumber;
			}

			var jobDeclarationLinked = LinkToDeclaration(referenceQualifier, filerOrIssuerCode, entryOrBillNumber, transportMode);

			if (jobDeclarationLinked != null)
			{
				ProcessCore(jobDeclarationLinked, emailBody, referenceQualifier, entryOrBillNumber, envelopeNumber);
			}
			else
			{
				emailBody.Append("<b>No Declaration Job found for Bill Issuer Code '" + filerOrIssuerCode + "' and Master Bill Number '" + entryOrBillNumber + "'</b>");
			}

			var jobNumber = "Unknown";
			var uri = "";
			var branch = GlbBranch.CurrentBranch;

			if (jobDeclarationLinked != null)
			{
				jobNumber = jobDeclarationLinked.JobNumber;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(jobDeclarationLinked);
				branch = jobDeclarationLinked.Branch;
			}

			emailBody.Append("<br />");

			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "ACE Prior Notice Status", emailBody.ToString(), false, branch, jobDeclarationLinked);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		JobDeclaration LinkToDeclaration(ZString referenceQualifier, ZString filerOrIssuerCode, ZString entryOrBillNumber, ZString transportMode)
		{
			JobDeclaration result = null;
			if (!Message.EM_MessageNum.IsEmpty)
			{
				result = OriginalMessageLinker.Link<JobDeclaration>(Message);
			}

			if (result == null)
			{
				var declarationFilter = new ZDBOnlyQuery(typeof(JobDeclaration));
				declarationFilter.AddToFilter(JobDeclarationSchema.JE_IsCancelled, ZBool.False);
				declarationFilter.AddToFilter(JobDeclarationSchema.JE_MessageType, new string[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.FTZ });
				declarationFilter.AddToFilter(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());

				if (referenceQualifier == PriorNoticeReferenceQualifierCodeList.Codes.ENT || referenceQualifier == PriorNoticeReferenceQualifierCodeList.Codes.FTZ)
				{
					var entryNumberForQuery = referenceQualifier == PriorNoticeReferenceQualifierCodeList.Codes.FTZ ? FTZAdmissionNumberRetriever.GetFTZNumberFromString(entryOrBillNumber) : entryOrBillNumber;
					var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
					entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobDeclaration.Schema.TableName);
					entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumberForQuery);
					declarationFilter.AddSubQuery(entryNumberQuery, JoinCondition.And);
				}
				else
				{
					var masterbillSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
					masterbillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill);
					masterbillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, entryOrBillNumber);
					declarationFilter.AddSubQuery(masterbillSubQuery, JoinCondition.And);
				}

				var transportModeComparisonOperator = TransportModeCodes.IsAirTransport(transportMode) ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
				declarationFilter.AddToFilter(JobDeclarationSchema.JE_TransportMode, transportModeComparisonOperator, Core.Constants.TransportModes.Air);
				var declarations = Message.Factory.Load<JobDeclaration>(declarationFilter);

				int mawbRecyclePeriod = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
				if (mawbRecyclePeriod > 0)
				{
					var recycleTime = ZDateTime.Now.AddMonths(-mawbRecyclePeriod);
					declarations = declarations.Where(x => x.JE_SystemCreateTimeUtc >= recycleTime).ToArray();
				}

				if (referenceQualifier != PriorNoticeReferenceQualifierCodeList.Codes.ENT && referenceQualifier != PriorNoticeReferenceQualifierCodeList.Codes.FTZ)
				{
					if (!filerOrIssuerCode.IsEmpty)
					{
						declarations = declarations.Where(x => x.Bills.Cast<Bill>().Any(bill => bill.CU_BillNum.KeepAlphanumericCharacters().EqualsIgnoringCase(entryOrBillNumber) && bill.US_UI_NKBillIssuerSCAC.EqualsIgnoringCase(filerOrIssuerCode))).ToArray();
					}
					else
					{
						declarations = declarations.Where(x => x.Bills.Cast<Bill>().Any(bill => bill.CU_BillNum.KeepAlphanumericCharacters().EqualsIgnoringCase(entryOrBillNumber))).ToArray();
					}

					if (declarations.Take(2).Count() > 1)
					{
						declarations = declarations.OrderByDescending(x => x.JE_SystemCreateTimeUtc).Where(x => x.Messages.OfType<MQEDIMessage>().Any(msg => msg.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.PriorNotice)).ToArray();
					}
				}

				result = declarations.FirstOrDefault();

				if (result != null)
				{
					Message.EM_LinkedObject = result;
					result.Messages.Load();
				}
			}
			return result;
		}

		internal void SetPNCNumbers(JobDeclaration declaration, ZInt lineIndex, ZString identifier, ZString pncNumber, ZString referenceQualifier, ZString entryOrBillNumber)
		{
			if (identifier == OGADispositionReferenceQualifierList.Codes.PriorNoticeConfirmationNum && !pncNumber.IsEmpty && declaration != null)
			{
				if (fdaLinesMayRequirePNCNumberUpdateCached == null)
				{
					fdaLinesMayRequirePNCNumberUpdateCached = new Dictionary<ZInt, ACEFDA>();
					IEnumerable<JobComInvoiceLine> invoiceLinesToIncluded = null;

					if (referenceQualifier == PriorNoticeReferenceQualifierCodeList.Codes.ENT || referenceQualifier == PriorNoticeReferenceQualifierCodeList.Codes.FTZ || declaration.Bills.NumberOfMasterBill == 1)
					{
						invoiceLinesToIncluded = declaration.InvoiceLines.Cast<JobComInvoiceLine>();
					}
					else
					{
						invoiceLinesToIncluded = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.InvoiceHeader != null && InvoiceHeaderBelongsToThisMasterBill(x.InvoiceHeader, entryOrBillNumber));
					}

					foreach (var invoiceLine in invoiceLinesToIncluded)
					{
						foreach (ACEFDA fda in invoiceLine.ACE_FDALines)
						{
							if (!fdaLinesMayRequirePNCNumberUpdateCached.ContainsKey(fda.US_LineNo))
							{
								fdaLinesMayRequirePNCNumberUpdateCached.Add(fda.US_LineNo, fda);
							}
						}
					}
				}

				ACEFDA matchedFDALine = null;
				fdaLinesMayRequirePNCNumberUpdateCached.TryGetValue(lineIndex, out matchedFDALine);

				if (matchedFDALine != null && matchedFDALine.US_PNC.IsEmpty)
				{
					matchedFDALine.US_PNC = pncNumber;
					matchedFDALine.US_IsPNCFromMsg = true;
				}
			}
		}
		Dictionary<ZInt, ACEFDA> fdaLinesMayRequirePNCNumberUpdateCached;

		ZBool InvoiceHeaderBelongsToThisMasterBill(JobComInvoiceHeader invoice, ZString masterBill)
		{
			Bill relatedMasterBill = null;
			var relatedBill = invoice.Bill;

			if (relatedBill != null)
			{
				if (relatedBill.IsMasterBill)
				{
					relatedMasterBill = relatedBill;
				}
				else if (relatedBill.ParentBill is Bill parentBill && parentBill.IsMasterBill)
				{
					relatedMasterBill = parentBill;
				}
			}

			return relatedMasterBill != null && relatedMasterBill.CU_BillNum.KeepAlphanumericCharacters() == masterBill;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected void ProcessCore(JobDeclaration declaration, StringBuilder emailBody, ZString referenceQualifier, ZString entryOrBillNumber, ZString envelopeNumber)
		{
			var commentsToTrade = new ZStringBuilder();
			var dispositionReasons = new ZStringBuilder();
			var subReasonList = new OGADispositionSubReasonList();
			var dispositionPGALineNumber = ZInt.Zero;

			var remarks = string.Empty;
			HtmlTableCreator po60Table = null;
			HtmlTableCreator po70Table = null;
			List<string> po70TableRow = null;
			OGADispositionData processingOGADispositionData = null;
			var previousPGAEntryStatus = declaration != null ? declaration.GetPGAEntryStatus() : new Dictionary<ZString, ZString>();
			var pgaEntryStatusMapping = new Dictionary<ZString, IPGADispositionProvider>();
			var dispositionProviders = new List<IPGADispositionProvider>();

			foreach (MessageBlock block in messageBlocks)
			{
				if (block.MandatoryCharacters == "PO10")
				{
					var po10 = (SESNPO10)block;
					var headerTable = new HtmlTableCreator(new string[] { "Reference Qualifier", "Filer/Issuer code", "Number", "Carrier SCAC/IATA", "Entry Type", "Mode of Transportation", "Envelope Number" });
					headerTable.WriteRow(GetReferenceQualifierDescription(po10.ReferenceQualifierCode), po10.FilerOrIssuerCodeForReferenceIdentifier, po10.ReferenceIdentifierNumber, po10.Carrier, po10.EntryType, GetModeOfTransportationDescr(po10.ModeOfTransportationMOTCode), po10.EnvelopeNumber);

					emailBody.Append(headerTable.ToHtml());
				}
				else if (block.MandatoryCharacters == "PO60")
				{
					var po60 = block as SESNPO60;

					if (po60.DispositionActionCode == "01") // PN Confirmation Number(s) are cancelled
					{
						CancelAllPNConfirmationNumbers(declaration);
					}

					if (po60Table == null)
					{
						po60Table = new HtmlTableCreator(new string[] { "Data", "Value and Description" });
					}
					po60Table.WriteRow("Disposition Action/Date", po60.DispositionActionCode + " " + po60.NarrativeMessage + "/" + ((IDispositionDetailProvider)po60).DispositionDateTime.ToLongTimeString());

					if (!po60.ReleaseDate.IsEmpty)
					{
						var releaseOriginDesc = Message.Factory.GetCachedValue<ReleaseOriginCodeList>().GetDescriptionFromCode(po60.ReleaseOrigin);

						po60Table.WriteRow("Release Origin/Date", (releaseOriginDesc ?? po60.ReleaseOrigin.ToString()) + "/" + po60.ReleaseDate.ToShortDateString());
					}

					var documentType = po60.DocumentType;
					if (!documentType.IsEmpty)
					{
						var documentTypeDesc = DocumentTypeCodeList.GetDescriptionFromDocumentType(Factory, documentType);
						po60Table.WriteRow("Document Type", documentType + " - " + documentTypeDesc);
					}
				}
				else if (block.MandatoryCharacters == "PO70")
				{
					IPGADispositionProvider po70 = (IPGADispositionProvider)block;
					var pO70Block = po70 as SESNPO70;

					if (po70Table == null)
					{
						po70Table = new HtmlTableCreator(new string[] { "Agency", "Agency Program Code", "Status Desc", "PGA Line" });
					}
					else if (po70TableRow != null)
					{
						po70Table.WriteRow(po70TableRow.ToArray());
					}

					po70TableRow = new List<string>();
					po70TableRow.Add(po70.OtherAgencyQuotaIdentifier); // Agency
					po70TableRow.Add(po70.GovernmentAgencyProgramCode); // Agency Program Code
					var reviewReasonDesc = PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, po70.ReviewReasonCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineReviewReason);
					po70TableRow.Add(reviewReasonDesc); // Status Desc
					po70TableRow.Add(pO70Block.PGALine); // PGALine

					dispositionPGALineNumber = ZInt.ParseSafe(pO70Block.PGALine, 0);

					SimplifiedEntryStatusNotificationProcessor.UpdateComment(commentsToTrade, processingOGADispositionData);

					if (declaration != null)
					{
						processingOGADispositionData = declaration.EntryStatusesAndErrors.AddOGADispositionDataAndCalculateDeclarationFDAStatus(po70, OGADispositionSourceList.Codes.PGA, envelopeNumber);
					}

					if (!po70.OtherAgencyQuotaIdentifier.IsEmpty && po70.DispositionDateTime.IsValid)
					{
						pgaEntryStatusMapping[po70.OtherAgencyQuotaIdentifier] = po70;
					}

					dispositionProviders.Add(po70);
				}
				else if (block.MandatoryCharacters == "PO71")
				{
					var po71 = block as SESNPO71;
					SetPNCNumbers(declaration, dispositionPGALineNumber, po71.PGAReferenceIdentificationNumberQualifier, po71.PGAReferenceIdentificationNumber, referenceQualifier, entryOrBillNumber);
					//When storing AMS Confirmation number/FV6 number/AMS LPS number and FWS eDECS number, please remember to suspend PGA Tracking Status like PNC does.
					foreach (string subReasonCode in ((IPGADispositionDetailProvider)po71).SubReasonCodes)
					{
						if (!string.IsNullOrEmpty(subReasonCode))
						{
							var subReasonCodeDescription = subReasonList.GetDescriptionFromCode(subReasonCode);

							dispositionReasons.Append(subReasonCodeDescription ?? subReasonCode);
						}
					}

					if (processingOGADispositionData != null)
					{
						processingOGADispositionData.OGADispositionDetails.AddNewDispositionDetail(po71);
					}
				}
				else if (block.MandatoryCharacters == "PO72")
				{
					var po72 = (SESNPO72)block;

					if (!po72.CommentsToTradeFromPGA.IsEmpty)
					{
						commentsToTrade.Append(po72.CommentsToTradeFromPGA);
					}
				}
			}
			emailBody.Append("<br />");

			if (po70TableRow != null)
			{
				po70Table.WriteRow(po70TableRow.ToArray());
			}

			if (declaration != null)
			{
				declaration.EntryPGACusDispositions.AddOrUpdateCusDisposition(pgaEntryStatusMapping);
				declaration.LogPGAEntryStatus(previousPGAEntryStatus, Message.EM_MessageType);
				declaration.LogPGALineStatus(dispositionProviders, Message.EM_MessageType);
			}

			SimplifiedEntryStatusNotificationProcessor.UpdateComment(commentsToTrade, processingOGADispositionData);

			if (po60Table != null)
			{
				emailBody.Append(po60Table.ToHtml());
				emailBody.Append("<br>");
			}

			if (po70Table != null)
			{
				emailBody.Append(po70Table.ToHtml());
				emailBody.Append("<br>");
			}

			if (!string.IsNullOrEmpty(remarks))
			{
				emailBody.Append("<b>" + remarks + "</b>");
				emailBody.Append("<br>");
				emailBody.Append("<br>");
			}

			if (!commentsToTrade.IsEmpty)
			{
				emailBody.Append("<b>Comments to Trade from PGA:</b>");
				emailBody.Append("<br />");
				emailBody.Append(commentsToTrade);
				emailBody.Append("<br />");
			}
		}

		void CancelAllPNConfirmationNumbers(JobDeclaration declaration)
		{
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ToList().ForEach(line => line.ACE_FDALines.Cast<ACEFDA>().ToList().ForEach(fda => fda.US_PNC = ZString.Empty));
		}

		static ZString GetReferenceQualifierDescription(ZString status)
		{
			switch (status)
			{
				case "ENT":
					return "Entry Number";
				case "BOL":
					return "Bill of Lading Number (Ocean, Rail or Truck)";
				case "AWB":
					return "Air waybill";
				case "FTZ":
					return "FTZ admission number";
				case "INB":
					return "In-bond number";
				default:
					return "";
			}
		}

		ZString GetModeOfTransportationDescr(ZString code)
		{
			var result = code;
			if (!code.IsEmpty)
			{
				ZString description = Factory.GetCachedValue<TransportModeCodes>().GetDescriptionFromCode(code);
				result = !description.IsEmpty ? description + " (" + code + ")" : code.ToString();
			}
			return result;
		}
	}
}
