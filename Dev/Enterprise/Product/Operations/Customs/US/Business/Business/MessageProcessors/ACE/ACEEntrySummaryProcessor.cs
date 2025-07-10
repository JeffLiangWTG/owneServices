using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	class ACEEntrySummaryProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			CusEntryHeader entryHeader = CusEntryHeaderLinker.Link(Message);

			bool hasFailure, hasCensusWarning, hasPGAWarning;

			string email = ProcessBlocksAndGenerateEmailBody(entryHeader, out hasFailure, out hasCensusWarning, out hasPGAWarning);
			string uri = entryHeader == null ? "" : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entryHeader);
			string jobNumber = entryHeader == null ? "Unknown" : entryHeader.Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;

			var declaration = entryHeader?.Declaration;
			var branch = entryHeader?.Branch ?? GlbBranch.CurrentBranch;

			if (entryHeader != null)
			{
				CalculateStatus(entryHeader, hasFailure, hasCensusWarning, hasPGAWarning);
				LogMessageStatusChangeEventAgainstTopLevelBusinessObject(entryHeader);

				if (hasFailure && entryHeader.IsCargoReleaseBeingCertified)
				{
					entryHeader.US_CRLCertStatus = "";
				}

				if (!hasFailure)
				{
					if (entryHeader.IsTemporaryImportationBond)
					{
						entryHeader.SetTIBExpiryDate();
					}
					new AutoEntrySummaryQuerySender().SendIfEligible(entryHeader);
				}

				if (declaration != null && entryHeader.CH_Status == ImportMessageStatusList.Codes.ClearEntrySummaryDelete && declaration.US_Paid == YesNoList.Codes.Yes)
				{
					declaration.US_Paid = ZString.Empty;
				}
			}

			if (declaration != null)
			{
				var pgaLinesDataCorrectionManager = new PGALinesDataCorrectionManager(Message, declaration);
				pgaLinesDataCorrectionManager.UpdatePGALines(hasFailure);
			}

			if (declaration != null && declaration.SupportsBondedWarehousing && ShouldUpdateBondedWhs(declaration))
			{
				Message.Factory.Saved -= Factory_Saved;
				Message.Factory.Saved += Factory_Saved;
				new HtmlResponseEmailGenerator().TryGenerateEmail(uri, jobNumber, "Entry Summary", email + EmailDefBuilder.HtmlTemplates.DynamicHtml1 + EmailDefBuilder.HtmlTemplates.DynamicHtml2 + EmailDefBuilder.HtmlTemplates.DynamicHtml3, hasFailure, out emailReportThatHasBeenDelayed, branch, hasCensusWarning, CensusWarning);
			}
			else
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Entry Summary", email, hasFailure, branch, entryHeader, hasCensusWarning, CensusWarning);
			}
		}

		internal const string CensusWarning = "(Census Warning) ";

		bool ShouldUpdateBondedWhs(JobDeclaration declaration)
		{
			var whsStatus = declaration.WarehouseTransactionStatus;
			return WarehouseTransactionStatusList.IsPendingInward(whsStatus) || WarehouseTransactionStatusList.IsPendingOutward(whsStatus);
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= Factory_Saved;
			}
			new BondedWarehouseMessageProcessor(Message.PK, emailReportThatHasBeenDelayed, SendEmailToOriginalSenderOrGroupIfSenderInvalid).ProcessAfterSaved(savedSuccessfully);
		}

		void SendEmailToOriginalSenderOrGroupIfSenderInvalid(CusEntryHeader entryHeader, EmailDef email, bool isFailure)
		{
			SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, entryHeader == null ? null : entryHeader.Branch, isFailure);
			if (email == emailReportThatHasBeenDelayed)
			{
				emailReportThatHasBeenDelayed = null;
			}
		}
		EmailDef emailReportThatHasBeenDelayed;

		void CalculateStatus(IMessageAttachee header, bool isFailure, bool isCensusWarning, bool isPGAWarning)
		{
			if (header != null)
			{
				ABIResponseStatus responseStatus = ABIResponseStatus.Cleared;

				if (isFailure)
				{
					responseStatus = ABIResponseStatus.Rejected;
				}
				else if (isCensusWarning)
				{
					responseStatus = ABIResponseStatus.CensusWarning;
				}
				else if (isPGAWarning)
				{
					responseStatus = ABIResponseStatus.Warnings;
				}
				new EntrySummaryMessageStatusCalculator(header).CalculateStatus(Message, responseStatus);
			}
		}

		string ProcessBlocksAndGenerateEmailBody(CusEntryHeader entryHeader, out bool hasFailure, out bool hasCensusWarning, out bool hasPGAWarning)
		{
			hasFailure = false;
			hasCensusWarning = false;
			hasPGAWarning = false;

			var table = new HtmlTableCreator(new string[] { "Data Reference", "Severity Code", "Condition Code", "Message" });
			var dispositionTable = new HtmlTableCreator(new string[] { "DISPOSITION", "Condition Code", "Description" });

			Dictionary<CusEntryLine, List<string>> customsCWOs = new Dictionary<CusEntryLine, List<string>>();
			var tariffNumber = ZString.Empty;
			var entryLineNumber = 0;
			var secondaryLineNumber = 0;
			foreach (MessageBlock block in Message.MessageBlock.MessageBlocks)
			{
				var abiX1 = block as AABIOutputX1;

				if (abiX1 != null)
				{
					table.WriteRow("", GetSeverityCode(abiX1.SeverityCode), abiX1.ConditionCode, GetDetailedNarrativeText(abiX1.ConditionCode, abiX1.NarrativeText));
				}
				else
				{
					AENSE0 e0 = block as AENSE0;

					if (e0 != null)
					{
						if (e0.ReferenceDataTypeCode == EntrySummaryReferenceDataList.Codes.SUMMRY)
						{
							ZString teamNumber = e0.GetTeamNumber();
							if (!teamNumber.IsEmpty)
							{
								var declaration = entryHeader?.Declaration;
								if (declaration != null)
								{
									declaration.US_TeamNo = teamNumber;
								}
							}
						}
						else if (e0.ReferenceDataTypeCode == EntrySummaryReferenceDataList.Codes.LINITM)
						{
							entryLineNumber = e0.OccurrencePosition;
						}
						else if (e0.ReferenceDataTypeCode == EntrySummaryReferenceDataList.Codes.TARIFF)
						{
							secondaryLineNumber = e0.OccurrencePosition - 1;
							tariffNumber = e0.ReferenceDataText.Trim();
						}
						else if (e0.ReferenceDataTypeCode.IsEmpty || e0.ReferenceDataTypeCode == EntrySummaryReferenceDataList.Codes.PSTLIN)
						{
							continue;
						}
						table.WriteRow(e0.ReferenceDataTypeCode, "", "", GetDetailedDataReference(e0));
					}
					else
					{
						AENSE1 e1 = block as AENSE1;

						if (e1 != null)
						{
							string message = GetDetailedNarrativeText(e1.ConditionCode, e1.NarrativeText);

							bool isSummaryE1 = !e1.DispositionTypeCode.IsEmpty;

							if (!hasFailure)
							{
								hasFailure = (e1.DispositionTypeCode == ACESeverityList.Codes.Rejected);
							}

							if (!hasCensusWarning)
							{
								hasCensusWarning = (e1.SeverityCode == ACESeverityList.Codes.CensusWarning);
							}

							if (!hasPGAWarning)
							{
								hasPGAWarning = (e1.SeverityCode == ACESeverityList.Codes.PGAWarning || e1.SeverityCode == ACESeverityList.Codes.Information);
							}

							if (e1.SeverityCode == ACESeverityList.Codes.CensusWarning && !isSummaryE1)
							{
								var entryLine = entryHeader == null ? null : GetEntryLine(entryHeader, entryLineNumber, secondaryLineNumber);

								if (entryLine != null)
								{
									List<string> cwos;
									if (!customsCWOs.TryGetValue(entryLine, out cwos))
									{
										cwos = new List<string>();
										customsCWOs[entryLine] = cwos;
									}

									cwos.Add(e1.ConditionCode);
								}
								else
								{
									message += " No entry line exists matching the specified 'Data Reference'.";
								}
							}

							if (isSummaryE1)
							{
								dispositionTable.WriteRow(Factory.GetCachedValue<ACESeverityList>().GetDescriptionFromCode(e1.DispositionTypeCode), e1.ConditionCode, message);
							}
							else
							{
								table.WriteRow("", GetSeverityCode(e1.SeverityCode), e1.ConditionCode, message);
							}
						}
						else
						{
							var abiX0 = messageBlocks.OfType<AABIX0>().FirstOrDefault();
							hasFailure = abiX0 != null;
						}
					}
				}
			}

			if (entryHeader != null)
			{
				UpdateCWOsAndAcceptedCWOs(entryHeader, customsCWOs, hasFailure);
			}

			return table.ToHtml() + "<br/><br/>" + dispositionTable.ToHtml();
		}

		void UpdateCWOsAndAcceptedCWOs(CusEntryHeader entryHeader, Dictionary<CusEntryLine, List<string>> customsCWOs, bool isFailure)
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				var outstandingCWs = ZString.Empty;

				List<string> cwos;
				if (customsCWOs.TryGetValue(entryLine, out cwos))
				{
					outstandingCWs = new ZStringBuilder(cwos.ToArray()).ToString();
				}

				entryLine.US_CWOs = outstandingCWs;
			}
		}

		CusEntryLine GetEntryLine(CusEntryHeader entry, int lineNum, int secondaryLineNum)
		{
			CusEntryLine result = null;

			foreach (var entryLine in entry.EntryLines)
			{
				if (entryLine.CL_LineNumber == lineNum)
				{
					result = entryLine;
					break;
				}
			}

			if (secondaryLineNum > 0 && result != null)
			{
				foreach (var childLine in result.ChildLines)
				{
					if (childLine.US_ChildLineNum == secondaryLineNum)
					{
						result = childLine;
						break;
					}
				}
			}

			return result;
		}

		string GetDetailedDataReference(AENSE0 e0)
		{
			var result = "";
			switch (e0.ReferenceDataTypeCode)
			{
				case EntrySummaryReferenceDataList.Codes.ADDCVD:
					result = "Case Number: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.ARPART:
					result = string.Format("Article Party (Type: {0}, ID: {1})", e0.ReferenceDataText.Left(1).Trim(), e0.ReferenceDataText.SubstringSafe(2).Trim());
					break;
				case EntrySummaryReferenceDataList.Codes.BNDDTL:
					result = string.Format("Bonde Type: {0}, Bond Designation Type: {1}, Surety Company: {2}", e0.ReferenceDataText.Left(1).Trim(), e0.ReferenceDataText.SubstringSafe(2, 1), e0.ReferenceDataText.SubstringSafe(4).Trim());
					break;
				case EntrySummaryReferenceDataList.Codes.BOLINB:
					result = string.Format("Manifest Component (Type: {0}, ID: {1})", e0.ReferenceDataText.Left(1).Trim(), e0.ReferenceDataText.SubstringSafe(2).Trim());
					break;
				case EntrySummaryReferenceDataList.Codes.CARMAN:
					result = string.Format("Manifest (Qty: {0}, UQ: {1})", ZInt.ParseSafe(e0.ReferenceDataText.Left(8).Trim(), ZInt.Zero), e0.ReferenceDataText.SubstringSafe(10).Trim());
					break;
				case EntrySummaryReferenceDataList.Codes.CENWRN:
					result = string.Format("Census Warning Condition (Code: {0}, Override: {1})", e0.ReferenceDataText.Left(3).Trim(), e0.ReferenceDataText.SubstringSafe(5).Trim());
					break;
				case EntrySummaryReferenceDataList.Codes.COMDES:
					result = "Commercial Description: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.CONREL:
					result = string.Format("Release Entry (Filer Code: {0}, Number: {1})", e0.ReferenceDataText.Left(3).Trim(), e0.ReferenceDataText.SubstringSafe(5).Trim());
					break;
				case EntrySummaryReferenceDataList.Codes.DOTLIN:
					result = "DOT Line Number: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.DOTVEH:
					result = "Vehicle Identification Number: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.EIPINV:
					result = string.Format("Supplier ID: {0}, Invoice Number: {1}", e0.ReferenceDataText.Left(15).Trim(), e0.ReferenceDataText.SubstringSafe(17).Trim());
					break;
				case EntrySummaryReferenceDataList.Codes.FCCLIN:
					result = "FCC Line Number: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.FDAACT:
					result = string.Format("Affirmation of Compliance (Code: {0}, Qualifier: {1})", e0.ReferenceDataText.Left(3).Trim(), e0.ReferenceDataText.SubstringSafe(5).Trim());
					break;
				case EntrySummaryReferenceDataList.Codes.FDALIN:
					result = "FDA Line Number: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.FEETOT:
					result = string.Format("Accounting Class: {0}, Total Fee Amount: {1}", e0.ReferenceDataText.Left(3).Trim(), ConvertToDecimal(e0.ReferenceDataText.SubstringSafe(5, 11).Trim(), 2));
					break;
				case EntrySummaryReferenceDataList.Codes.HDRFEE:
					result = string.Format("Accounting Class: {0}, Header Fee Amount: {1}", e0.ReferenceDataText.Left(3).Trim(), ConvertToDecimal(e0.ReferenceDataText.SubstringSafe(5, 8).Trim(), 2));
					break;
				case EntrySummaryReferenceDataList.Codes.INVLIN:
					result = string.Format("Invoice Line Range (Begin: {0}, End: {1})", ZInt.ParseSafe(e0.ReferenceDataText.Left(4).Trim(), ZInt.Zero), ZInt.ParseSafe(e0.ReferenceDataText.SubstringSafe(6, 4).Trim(), ZInt.Zero));
					break;
				case EntrySummaryReferenceDataList.Codes.LICNSE:
					result = string.Format("License / Certificate / Permit (Type: {0}, Number: {1})", e0.ReferenceDataText.Left(2).Trim(), e0.ReferenceDataText.SubstringSafe(3).Trim());
					break;
				case EntrySummaryReferenceDataList.Codes.LINFEE:
					result = string.Format("Accounting Class: {0}, User Fee Amount: {1}", e0.ReferenceDataText.Left(3).Trim(), ConvertToDecimal(e0.ReferenceDataText.SubstringSafe(5, 8).Trim(), 2));
					break;
				case EntrySummaryReferenceDataList.Codes.LINITM:
					var lineNumber = e0.ReferenceDataText.Trim();
					result = "Line Item Identifier: " + (lineNumber.IsEmpty ? e0.OccurrencePosition.ToString() : lineNumber.ToString());
					break;
				case EntrySummaryReferenceDataList.Codes.MISDOC:
					result = "Missing Document Code: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.PGADIS:
					result = "PGA Form Disclaimer Code: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.PSCEXP:
					result = "PSC Filing Explanation Text: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.PSCHRE:
					result = "Post Summary Correction Header Reason Code: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.PSCLRE:
					result = "Post Summary Correction Line Reason Code: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.SUMMRY:
					result = "CBP Team Number: " + e0.GetTeamNumber().Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.TARIFF:
					result = "HTS Number: " + e0.ReferenceDataText.Trim();
					break;
				case EntrySummaryReferenceDataList.Codes.TARQTY:
					result = string.Format("HTS (Qty: {0}, UQ: {1})", ConvertToDecimal(e0.ReferenceDataText.Left(12).Trim(), 2), e0.ReferenceDataText.SubstringSafe(13, 3).Trim());
					break;
				case EntrySummaryReferenceDataList.Codes.TOTALS:
					break;
				default:
					result = "PGA Data: " + e0.ReferenceDataText;
					break;
			}
			return result;
		}

		ZDecimal ConvertToDecimal(ZString value, int decimals)
		{
			var result = ZDecimal.Zero;
			if (value.Contains('.'))
			{
				result = ZDecimal.ParseSafe(value, ZDecimal.Zero);
			}
			else
			{
				result = ZDecimal.ParseSafe(value, ZDecimal.Zero) / (ZDecimal)Math.Pow(10, decimals);
			}
			return result.Round(decimals);
		}

		string GetDetailedNarrativeText(string code, string shortDesc)
		{
			string result = shortDesc;
			MessageError aceError = ACEError.Instance.GetErrorInfoByCode(code);
			if (aceError != null)
			{
				result += " " + aceError.Narrative;
			}

			return result;
		}

		string GetSeverityCode(string code)
		{
			switch (code)
			{
				case ACESeverityList.Codes.Error:
					return ACESeverityList.Descriptions.Error;
				case ACESeverityList.Codes.CensusWarning:
					return ACESeverityList.Descriptions.CensusWarning;
				case ACESeverityList.Codes.Information:
					return ACESeverityList.Descriptions.Information;
				case ACESeverityList.Codes.PGAWarning:
					return ACESeverityList.Descriptions.PGAWarning;
				default:
					return "";
			}
		}
	}
}
