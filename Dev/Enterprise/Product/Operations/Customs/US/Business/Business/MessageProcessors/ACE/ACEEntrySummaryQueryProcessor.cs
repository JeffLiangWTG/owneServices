using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse, Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	class ACEEntrySummaryQueryProcessor : ACEABIProcessor
	{
		#region IKeysForBlockingParallelProcessingProvider Members

		protected override ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(CBPEDIMessage message)
		{
			var jobData = GetJobDataFromMessage(message);
			var declaration = jobData.Declaration;
			var jobNumber = ZString.Empty;
			if (jobData.Declaration != null)
			{
				jobNumber = declaration.JE_DeclarationReference;
			}
			return new LinkedBusinessObjectMetaData(jobData.LinkTableName, jobData.LinkUniqueID, jobData.BranchPK, jobNumber);
		}

		protected override HashSet<string> TryFindSerializationKeys(CBPEDIMessage message, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var result = new HashSet<string>();
			var jobData = GetJobDataFromMessage(message);
			var declaration = jobData.Declaration;
			if (declaration != null)
			{
				result.Add(USIUniversalCustomsMessageProcessor.GetEntryJobNumberKey(declaration.EntryFilerCode, declaration.ImportEntryNumber, declaration.JE_GB));
			}
			return result;
		}

		(ZGuid BranchPK, ZGuid LinkUniqueID, ZString LinkTableName, JobDeclaration Declaration) GetJobDataFromMessage(CBPEDIMessage message)
		{
			var branchPK = ZGuid.Empty;
			var linkUniqueID = ZGuid.Empty;
			var linkTableName = ZString.Empty;
			var jobNumber = ZString.Empty;
			var originalMessage = message.OriginalMessage;
			JobDeclaration declaration = null;
			if (originalMessage != null)
			{
				if (originalMessage.EM_LinkTable.EqualsIgnoringCase(CusEntryHeader.Schema.TableName))
				{
					branchPK = originalMessage.EM_GB;
					linkUniqueID = originalMessage.EM_LinkUniqueID;
					linkTableName = CusEntryHeader.Schema.TableName;
					declaration = (originalMessage.EM_LinkedObject as CusEntryHeader)?.Declaration;
				}
				if (declaration == null)
				{
					var j1Block = originalMessage.MessageBlock.MessageBlocks.OfType<AENQJ1>().FirstOrDefault();
					if (j1Block != null && j1Block.EntryFilerCode2.IsEmpty)
					{
						var entryNumber = j1Block.EntryNumber1;
						var filterCode = j1Block.EntryFilerCode1;
						if (!entryNumber.IsEmpty && !filterCode.IsEmpty)
						{
							var entryHeader = new CusEntryHeader.Loader(message.Factory).FindByEntryNumberAndFilerCode(message.Branch.GB_GC, entryNumber, filterCode, new ZString[] { CusEntryHeaderMessageTypeList.Codes.ReconEntry, CusEntryHeaderMessageTypeList.Codes.EntrySummary });
							declaration = entryHeader?.Declaration;
							if (declaration != null)
							{
								branchPK = declaration.JE_GB;
								linkUniqueID = entryHeader.PK;
								linkTableName = CusEntryHeader.Schema.TableName;
							}
						}
					}
				}
			}
			return (branchPK, linkUniqueID, linkTableName, declaration);
		}

		#endregion

		public override void Process()
		{
			var uri = ZString.Empty;
			var jobNumber = "multiple entries";

			var entryHeader = GetEntryHeader();
			if (entryHeader != null)
			{
				jobNumber = entryHeader.Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entryHeader);
			}

			bool isFailure;
			string email = GenerateEmailBody(out isFailure);

			var originalMessage = Message.OriginalMessage;
			if (originalMessage != null && originalMessage.EM_SystemCreateUser != User.ServiceUserCode)
			{
				var branch = entryHeader != null ? entryHeader.Branch : MasterFiles.Business.GlbBranch.CurrentBranch;
				GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Entry Summary Query", email, isFailure, branch, entryHeader);
			}
		}

		CusEntryHeader GetEntryHeader()
		{
			var entryHeader = CusEntryHeaderLinker.Link(Message);
			if (entryHeader == null)
			{
				var originalMessage = Message.OriginalMessage;
				if (originalMessage != null)
				{
					var j1Block = originalMessage.MessageBlock.MessageBlocks.OfType<AENQJ1>().FirstOrDefault();
					if (j1Block != null && j1Block.EntryFilerCode2.IsEmpty)
					{
						var entryNumber = j1Block.EntryNumber1;
						var filterCode = j1Block.EntryFilerCode1;
						if (!entryNumber.IsEmpty && !filterCode.IsEmpty)
						{
							entryHeader = CusEntryHeaderLinker.Link(entryNumber, filterCode, Message, new ZString[] { CusEntryHeaderMessageTypeList.Codes.ReconEntry, CusEntryHeaderMessageTypeList.Codes.EntrySummary });
							if (originalMessage.EM_LinkedObject == null)
							{
								originalMessage.EM_LinkedObject = entryHeader;
							}
						}
					}
				}
			}
			return entryHeader;
		}

		string GenerateEmailBody(out bool isFailure)
		{
			isFailure = false;
			var jzBlocks = Message.MessageBlock.MessageBlocks.OfType<AENQJZ>();

			if (jzBlocks.IsCountEqualTo(1))
			{
				var jzBlock = jzBlocks.ElementAt(0);
				if (IsNoDataFoundConditionCode(jzBlock.ConditionCode))
				{
					var jaBlock = Message.MessageBlock.MessageBlocks.OfType<AENQJA>().FirstOrDefault();
					return GenerateNoDataFoundEmail(jzBlock, jaBlock);
				}
			}

			var result = new ZStringBuilder();
			result.Append("<a name=\"top\"></a>");
			HtmlTableCreator headerTable = null;
			if (Message.MessageBlock.MessageBlocks.OfType<AENQJB>().Any())
			{
				headerTable = new HtmlTableCreator(new string[] { "Declaration Reference Number", "Entry Number" });
				headerTable.EnableHTMLEncoding = false;
			}

			HtmlTableCreator table = null;
			var isDataBlockExists = Message.MessageBlock.MessageBlocks.Any(x => x is AENQJB || x is IAENQJC);
			if (isDataBlockExists)
			{
				table = new HtmlTableCreator(new string[] { "Column", "Value" });
			}
			HtmlTableCreator errorsTable = null;

			var details = new ZStringBuilder();
			var emailHeaderDetails = new ZStringBuilder();
			var entryNumberWithFilerCode = ZString.Empty;
			CusEntryHeader entryFromJBBlock = null;
			foreach (MessageBlock block in messageBlocks)
			{
				if (block is AENQJA ja)
				{
					GenerateForJA(ja, emailHeaderDetails);
				}
				else if (block is AENQJB jb)
				{
					if (!entryNumberWithFilerCode.IsEmpty)
					{
						details.Append("<br>");
						details.Append("<b>Entry: <a name=\"" + entryNumberWithFilerCode + "\">" + entryNumberWithFilerCode + "</a></b>          <a href=\"#top\"> Go to Top </a>");
						details.Append("<br>");
						details.Append("<br>");
						details.Append(table.ToHtml());
						table = new HtmlTableCreator(new string[] { "Column", "Value" });
					}

					var declarationReference = ZString.Empty;
					var declarationURL = ZString.Empty;
					entryFromJBBlock = new CusEntryHeader.Loader(Message.Factory).FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, jb.EntryNumber, jb.EntryFilerCode, new ZString[] { CusEntryHeaderMessageTypeList.Codes.ReconEntry, CusEntryHeaderMessageTypeList.Codes.EntrySummary });
					if (entryFromJBBlock != null)
					{
						declarationURL = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entryFromJBBlock);
						declarationReference = entryFromJBBlock.Declaration.JE_DeclarationReference;

						ProcessLiquidation(jb, entryFromJBBlock);
					}

					entryNumberWithFilerCode = CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(jb.EntryFilerCode, jb.EntryNumber);

					headerTable.WriteRow((!declarationReference.IsEmpty ? "<a href=\"" + declarationURL + "\">" + declarationReference + "</a>" : "No declaration found"),
										"<a href=\"#" + entryNumberWithFilerCode + "\">" + entryNumberWithFilerCode + "</a>");

					GenerateForJB(jb, table);
				}
				else if (block is IAENQJC jc)
				{
					SetSuspensionDetails(jc);
					GenerateForJC(jc, table);
					if (entryFromJBBlock != null)
					{
						entryFromJBBlock.US_CollectionDate = jc.CollectionDate;
					}
				}
				else if (block is AENQJD jd)
				{
					SetLiquidationAmounts(jd);
					GenerateForJD(jd, table);
					if (entryFromJBBlock != null)
					{
						entryFromJBBlock.US_ALDuty = jd.LiquidatedDuty;
					}
				}
				else if (block is AENQJE je)
				{
					SetLiquidationFeesAndDuty(je);
					GenerateForJE(je, table);
				}
				else if (block is AENQJZ jz)
				{
					isFailure = true;
					if (errorsTable == null)
					{
						errorsTable = new HtmlTableCreator(new string[] { "Entry Number", "Code", "Error" });
					}
					GenerateForJZ(jz, errorsTable);
				}
				else if (block is AENQJF jf)
				{
					GenerateForJF(jf, table);
				}
				else if (block is AENQJG jg)
				{
					GenerateForJG(jg, table);
				}
				else if (block is AENQJH jh)
				{
					GenerateForJH(jh, table);
				}
				else if (block is AENQJI ji)
				{
					GenerateForJI(ji, table);
				}
				else if (block is AENQJI_01 ji_01)
				{
					GenerateForJI_01(ji_01, table);
				}
				else if (block is AENQJJ jj)
				{
					GenerateForJJ(jj, table);
				}
				else if (block is AENQJK jk)
				{
					GenerateForJK(jk, table);
				}
				else if (block is AENQJK_01 jk_01)
				{
					GenerateForJK_01(jk_01, table);
				}
				else if (block is AENQJL jl)
				{
					GenerateForJL(jl, table);
				}
				else if (block is AENQJL_01 jl_01)
				{
					GenerateForJL_01(jl_01, table);
				}
				else if (block is AENQJM jm)
				{
					GenerateForJM(jm, table);
				}
				else if (block is AENQJN jn)
				{
					GenerateForJN(jn, table);
				}
				else if (block is AENQJN_01 jn_01)
				{
					GenerateForJN_01(jn_01, table);
				}
			}

			result.Append(emailHeaderDetails);

			if (headerTable != null)
			{
				result.Append(headerTable.ToHtml());
			}

			if (!entryNumberWithFilerCode.IsEmpty)
			{
				details.Append("<br>");
				details.Append("<b>Entry: <a name=\"" + entryNumberWithFilerCode + "\">" + entryNumberWithFilerCode + "</a></b>          <a href=\"#top\"> Go to Top </a>");
				details.Append("<br>");
				details.Append("<br>");
			}

			if (table != null)
			{
				details.Append(table.ToHtml());
			}

			result.Append(details);

			if (errorsTable != null)
			{
				result.Append(errorsTable.ToHtml());
			}

			return result.ToString();
		}

		void SetLiquidationAmounts(AENQJD jd)
		{
			if (newLiquidation != null)
			{
				newLiquidation.B8_EntryDate = jd.EntryDate;
				newLiquidation.B8_LiquidatedDuty = jd.LiquidatedDuty;
				newLiquidation.B8_LiquidatedTax = jd.LiquidatedTax;
				newLiquidation.B8_TotalLiquidatedFees = jd.LiquidatedFees;
				newLiquidation.B8_InterestAmount = jd.LiquidatedInterest;
			}
		}

		void SetSuspensionDetails(IAENQJC jc)
		{
			if (newLiquidation != null)
			{
				newLiquidation.B8_ExtensionSuspensionDate = jc.ExtensionSuspensionDate;
				newLiquidation.B8_ExtensionSuspensionCode = jc.ExtensionSuspensionStatusCode1;
				newLiquidation.B8_ExtensionSuspensionCode2 = jc.ExtensionSuspensionStatusCode2;
				newLiquidation.B8_ExtensionSuspensionCode3 = jc.ExtensionSuspensionStatusCode3;
				newLiquidation.B8_ExtensionSuspensionCode4 = jc.ExtensionSuspensionStatusCode4;
			}
		}

		void SetLiquidationFeesAndDuty(AENQJE je)
		{
			if (newLiquidation != null)
			{
				newLiquidation.B8_TotalLiquidatedFees = je.EstimatedFees;
				newLiquidation.B8_LiquidatedDuty = je.EstimatedDuty;
			}
		}

		void ProcessLiquidation(AENQJB jb, CusEntryHeader entryFromJBBlock)
		{
			if (jb.LiquidationStatusCode == LiquidationStatusCodeList.Codes.LiquidatedClosed || jb.LiquidationStatusCode == LiquidationStatusCodeList.Codes.Reliquidated)
			{
				var liquidation = GetLiquidation(jb);
				if (liquidation == null && entryFromJBBlock.Declaration != null)
				{
					newLiquidation = GenerateLiquidation(entryFromJBBlock.Declaration.PK);
					newLiquidation.B8_LiquidationType = GetLiquidationType(jb.LiquidationStatusCode);
					newLiquidation.B8_LiquidationDate = jb.LiquidationDate;
					newLiquidation.B8_EntryFilerCode = jb.EntryFilerCode;
					newLiquidation.B8_EntryNumber = jb.EntryNumber;
				}
			}
			else if (jb.LiquidationStatusCode == LiquidationStatusCodeList.Codes.NotLiquidated)
			{
				entryFromJBBlock.US_ALDate = jb.LiquidationDate;
			}
		}

		ZString GetLiquidationType(ZString liquidationStatusCode)
		{
			var result = ZString.Empty;
			switch (liquidationStatusCode)
			{
				case LiquidationStatusCodeList.Codes.LiquidatedClosed:
					result = LiquidationTypeCodeList.Codes.Liquidated;
					break;
				case LiquidationStatusCodeList.Codes.Reliquidated:
					result = LiquidationTypeCodeList.Codes.ReLiquidated;
					break;
			}
			return result;
		}

		CusLiquidation GenerateLiquidation(ZGuid declarationPK)
		{
			var result = Factory.New<CusLiquidation>();
			result.B8_JE = declarationPK;
			return result;
		}

		CusLiquidation GetLiquidation(AENQJB jb)
		{
			if (!jb.EntryFilerCode.IsEmpty && !jb.EntryNumber.IsEmpty)
			{
				return new CusLiquidation.Loader(Factory).LoadTop1WithActualLiquidationDate(jb.EntryFilerCode, jb.EntryNumber);
			}
			return null;
		}

		CusLiquidation newLiquidation;

		string GenerateNoDataFoundEmail(AENQJZ jzBlock, AENQJA jaBlock)
		{
			var messageBody = "";
			ZString entryNumber = !jzBlock.EntryNumber.IsEmpty ? (jzBlock.EntryFilerCode + "-" + jzBlock.EntryNumber) : "";
			if (!entryNumber.IsEmpty)
			{
				messageBody = "<b>No summaries found with Entry Number " + entryNumber + ".</b>";
			}
			else
			{
				if (jaBlock != null)
				{
					ZDateTime fromDate = GetDateAdjusted(jaBlock.RequestedFromDateTime);
					if (fromDate.IsValid)
					{
						messageBody = "<b>No entries found for the query, " + jaBlock.CriteriaQueryTypeCode +
								" (" + GetCriteriaCodeDescription(jaBlock.CriteriaQueryTypeCode).TrimEnd('.') + ")" +
								" from " + fromDate.ToString();

						ZDateTime toDate = GetDateAdjusted(jaBlock.RequestedToDateTime);
						if (toDate.IsValid)
						{
							messageBody += " to " + toDate.ToString();
						}
						messageBody += ".</b><br><br>";
					}
				}
			}
			return messageBody;
		}

		bool IsNoDataFoundConditionCode(string code)
		{
			return code == "013" || code == "015";
		}

		#region Generate Message Blocks

		void GenerateForJA(AENQJA ja, ZStringBuilder emailHeaderDetails)
		{
			if (!ja.CriteriaQueryTypeCode.IsEmpty)
			{
				var criteriaDescription = GetCriteriaCodeDescription(ja.CriteriaQueryTypeCode).TrimEnd('.');
				if (!emailHeaderDetails.ToString().Contains(criteriaDescription))
				{
					emailHeaderDetails.Append("<b>Query for " + ja.CriteriaQueryTypeCode + " (" + criteriaDescription + ")</b><br>");
				}
			}

			ZDateTime fromDate = GetDateAdjusted(ja.RequestedFromDateTime);
			if (fromDate.IsValid && !emailHeaderDetails.ToString().Contains(fromDate.ToString()))
			{
				emailHeaderDetails.Append("<b>Requested From Date: " + fromDate.ToString() + "</b><br>");
			}

			ZDateTime toDate = GetDateAdjusted(ja.RequestedToDateTime);
			if (toDate.IsValid && !emailHeaderDetails.ToString().Contains(toDate.ToString()))
			{
				emailHeaderDetails.Append("<b>Requested To Date: " + toDate.ToString() + "</b><br>");
			}
			emailHeaderDetails.Append("<br>");
		}

		void GenerateForJB(AENQJB jb, HtmlTableCreator table)
		{
			table.WriteRow("Version Number", jb.VersionNumber);
			table.WriteRow("Liquidation Status", new LiquidationStatusCodeList().GetDescriptionFromCode(jb.LiquidationStatusCode));
			table.WriteRow("Liquidation Date", jb.LiquidationDate.ToShortDateString());
			table.WriteRow("PSC Indicator", jb.PSCIndicator);
			table.WriteRow("PSC Accept Date", jb.PSCAcceptDate);
		}

		void GenerateForJC(IAENQJC jc, HtmlTableCreator table)
		{
			table.WriteRow("Control Status", GetControlStatusDesc(jc.EntrySummaryControlStatus));
			table.WriteRow("Status", GetEntrySummaryStatusDesc(jc.EntrySummaryStatusCode));
			table.WriteRow("Status Date", jc.EntrySummaryStatusDate.ToShortDateString());
			table.WriteRow("Release Status", GetReleaseStatusDesc(jc.ReleaseStatusCode));
			table.WriteRow("Release Date", jc.ReleaseDate.ToShortDateString());
			table.WriteRow("Collection Status", GetCollectionStatusDesc(jc.CollectionStatusCode));
			table.WriteRow("Collection Date", jc.CollectionDate.ToShortDateString());
			var extensionSuspensionCodeList = new ExtensionSuspensionCodeList(Factory);
			table.WriteRow("Extension/Suspension Status Code 1", extensionSuspensionCodeList.GetDescriptionFromCode(jc.ExtensionSuspensionStatusCode1));
			table.WriteRow("Extension/Suspension Status Code 2", extensionSuspensionCodeList.GetDescriptionFromCode(jc.ExtensionSuspensionStatusCode2));
			table.WriteRow("Extension/Suspension Status Code 3", extensionSuspensionCodeList.GetDescriptionFromCode(jc.ExtensionSuspensionStatusCode3));
			table.WriteRow("Extension/Suspension Status Code 4", extensionSuspensionCodeList.GetDescriptionFromCode(jc.ExtensionSuspensionStatusCode4));
			table.WriteRow("Extension/Suspension Date", jc.ExtensionSuspensionDate.ToShortDateString());
			table.WriteRow("Extension/Suspension Notice Date", jc.ExtensionSuspensionNoticeDate.ToShortDateString());
			table.WriteRow("Census Header Status", GetCensusHeaderStatusDesc(jc.CensusHeaderStatusCode));
			table.WriteRow("Invoice Status", GetInvoiceStatusDesc(jc.InvoiceStatusCode));
			table.WriteRow("Protest Status", GetProtestStatusDesc(jc.ProtestStatusCode));
			table.WriteRow("Quota Status", GetQuotaStatusDesc(jc.QuotaStatusCode));
			table.WriteRow("Late Filing Status", GetLateFilingStatusDesc(jc.LateFilingStatusCode));
			table.WriteRow("Trade Agreement Recon Entry Number", CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(jc.TradeAgreementReconciliationFilerCode, jc.TradeAgreementReconciliationEntryNumber));
			table.WriteRow("Other Recon Entry Number", CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(jc.OtherReconciliationFilerCode, jc.OtherReconciliationEntryNumber));
		}

		void GenerateForJD(AENQJD jd, HtmlTableCreator table)
		{
			table.WriteRow("CBP Review Indicator", GetCBPReviewIndicatorDesc(jd.CBPReviewIndicator));
			table.WriteRow("Entry Date", jd.EntryDate.ToShortDateString());
			table.WriteRow("Liquidated Duty", jd.LiquidatedDuty);
			table.WriteRow("Liquidated Tax", jd.LiquidatedTax);
			table.WriteRow("Liquidated Fees", jd.LiquidatedFees);
			table.WriteRow("Liquidated Interest", jd.LiquidatedInterest);
			table.WriteRow("Liquidated ADD/CVD", jd.LiquidatedADDCVD);
			if (!jd.LiquidationReasonCode1.IsEmpty)
			{
				table.WriteRow("Liquidation Reason Code (1)", GetLiquidationReasonDesc(jd.LiquidationReasonCode1));
			}
			if (!jd.LiquidationReasonCode2.IsEmpty)
			{
				table.WriteRow("Liquidation Reason Code (2)", GetLiquidationReasonDesc(jd.LiquidationReasonCode2));
			}
			if (!jd.LiquidationReasonCode3.IsEmpty)
			{
				table.WriteRow("Liquidation Reason Code (3)", GetLiquidationReasonDesc(jd.LiquidationReasonCode3));
			}
			table.WriteRow("Immediate Delivery Indicator", GetImmediateDeliveryIndicatorDesc(jd.ImmediateDeliveryIndicator));
		}

		void GenerateForJE(AENQJE je, HtmlTableCreator table)
		{
			table.WriteRow("Estimated Duty", je.EstimatedDuty);
			table.WriteRow("Estimated Tax", je.EstimatedTax);
			table.WriteRow("Estimated Fees", je.EstimatedFees);
			table.WriteRow("Estimated Interest", je.EstimatedInterest);
			table.WriteRow("Estimated ADD/CVD", je.EstimatedADDCVD);
		}

		void GenerateForJF(AENQJF jf, HtmlTableCreator table)
		{
			if (!SocialSecurityNumberValidator.IsValidSSN(jf.ImporterOfRecordNumber))
			{
				table.WriteRow("Importer of Record Number", jf.ImporterOfRecordNumber);
			}
			table.WriteRow("Entry Type", jf.EntryType);
			table.WriteRow("Reject Date", jf.RejectDate);
			table.WriteRow("Accelerated Drawback Indicator", GetAcceleratedDrawbackIndicatorDesc(jf.AcceleratedDrawbackIndicator));
			table.WriteRow("Electronic Invoice Indicator", GetElectronicInvoiceIndicatorDesc(jf.ElectronicInvoiceIndicator));
			table.WriteRow("District/Port of Entry", jf.DistrictPortOfEntry);
			table.WriteRow("Entry Summary Filing Date", jf.EntrySummaryFilingDate);
		}

		void GenerateForJG(AENQJG jg, HtmlTableCreator table)
		{
			table.WriteRow("Number of Withdrawals", jg.NumberOfWithdrawals);
			table.WriteRow("Warehouse Final Withdrawal Indicator", jg.WarehouseFinalWithdrawalIndicator);
			table.WriteRow("Import Specialist Team", jg.ImportSpecialistTeam);
			table.WriteRow("Center ID", jg.CenterID);
			table.WriteRow("Number of Line Items", jg.NumberOfLineItems);
		}

		void GenerateForJH(AENQJH jh, HtmlTableCreator table)
		{
			table.WriteRow("CF-4811 Reference Number", jh.CF4811ReferenceNumber);
			table.WriteRow("Preliminary Statement Print Date", jh.PreliminaryStatementPrintDate);
			table.WriteRow("Broker Reference Number", jh.BrokerReferenceNumber);
		}

		void GenerateForJI(AENQJI ji, HtmlTableCreator table)
		{
			table.WriteRow("Surety Code", ji.SuretyCode);
			table.WriteRow("Primary Surety Indicator", GetPrimarySuretyIndicatorDesc(ji.PrimarySuretyIndicator));
			table.WriteRow("Bond Type Code", GetBondTypeCodeDesc(ji.BondTypeCode));
			table.WriteRow("Bond Designation Type Code", GetBondDesignationTypeCodeDesc(ji.BondDesignationTypeCode));
			table.WriteRow("Multiple Bonds Indicator", GetMultipleBondsIndicatorDesc(ji.MultipleBondsIndicator));
			table.WriteRow("Bond Number", ji.BondNumber);
			table.WriteRow("Single Entry Bond Amount", ji.SingleEntryBondAmount);
			table.WriteRow("Surety Liability Amount", ji.SuretyLiabilityAmount);
		}

		void GenerateForJI_01(AENQJI_01 ji_01, HtmlTableCreator table)
		{
			table.WriteRow("Surety Code", ji_01.SuretyCode);
			table.WriteRow("Narrative Text", ji_01.NarrativeText);
		}

		void GenerateForJJ(AENQJJ jj, HtmlTableCreator table)
		{
			table.WriteRow("Protest Number", jj.ProtestNumber);
			table.WriteRow("Protest Type", GetProtestTypeDesc(jj.ProtestType));
			table.WriteRow("Protest Status", GetProtestStatusDes(jj.ProtestStatus));
			table.WriteRow("Protest Decision Date", jj.ProtestDecisionDate);
			table.WriteRow("Summons Indicator", GetSummonsIndicatorDesc(jj.SummonsIndicator));
		}

		void GenerateForJK(AENQJK jk, HtmlTableCreator table)
		{
			table.WriteRow("Bill Number", jk.BillNumber);
			table.WriteRow("Bill Date", jk.BillDate);
			table.WriteRow("Bill Type", GetBillTypeDesc(jk.BillType));
			table.WriteRow("Bill Collection Status", GetBillStatusDesc(jk.BillCollectionStatus));
			table.WriteRow("Total Bill Amount", jk.TotalBillAmount);
			table.WriteRow("Paid Amount", jk.PaidAmount);
			table.WriteRow("Principal Amount", jk.PrincipalAmount);
			table.WriteRow("Interest Amount", jk.InterestAmount);
		}

		void GenerateForJK_01(AENQJK_01 jk_01, HtmlTableCreator table)
		{
			table.WriteRow("Narrative Text", jk_01.NarrativeText);
		}

		void GenerateForJL(AENQJL jl, HtmlTableCreator table)
		{
			table.WriteRow("Collection Date", jl.CollectionDate);
			table.WriteRow("Total Amount", jl.TotalAmount);
		}

		void GenerateForJL_01(AENQJL_01 jl_01, HtmlTableCreator table)
		{
			table.WriteRow("Narrative Text", jl_01.NarrativeText);
		}

		void GenerateForJM(AENQJM jm, HtmlTableCreator table)
		{
			table.WriteRow("Class Code", jm.ClassCode);
			table.WriteRow("Class Code Amount", jm.ClassCodeAmount);
		}

		void GenerateForJN(AENQJN jn, HtmlTableCreator table)
		{
			table.WriteRow("Surety Code", jn.SuretyCode);
			table.WriteRow("Primary Surety Indicator", GetPrimarySuretyIndicatorDesc(jn.PrimarySuretyIndicator));
			table.WriteRow("612 Report Date", jn.ReportDate612);
			table.WriteRow("Bill Number", jn.BillNumber);
			table.WriteRow("Bill Date", jn.BillDate);
			table.WriteRow("Bill Type", GetBillTypeDesc(jn.BillType));
			table.WriteRow("Bill Collection Status", GetBillStatusDesc(jn.BillCollectionStatus));
			table.WriteRow("Total Bill Amount", jn.TotalBillAmount);
			table.WriteRow("Paid Amount", jn.PaidAmount);
			table.WriteRow("Principal Amount", jn.PrincipalAmount);
			table.WriteRow("Interest Amount", jn.InterestAmount);
		}

		void GenerateForJN_01(AENQJN_01 jn_01, HtmlTableCreator table)
		{
			table.WriteRow("Surety Code", jn_01.SuretyCode);
			table.WriteRow("Primary Surety Indicator", GetPrimarySuretyIndicatorDesc(jn_01.PrimarySuretyIndicator));
			table.WriteRow("Narrative Text", jn_01.NarrativeText);
		}

		void GenerateForJZ(AENQJZ jz, HtmlTableCreator errorTable)
		{
			errorTable.WriteRow(jz.EntryFilerCode + "-" + jz.EntryNumber, jz.ConditionCode, jz.NarrativeText);
			if (!jz.DistrictPortOfEntry.IsEmpty)
			{
				errorTable.WriteRow("District/Port of Entry", jz.DistrictPortOfEntry);
			}
		}

		#endregion

		#region Description

		string GetLiquidationReasonDesc(ZString code)
		{
			switch (code)
			{
				case "01":
					return "Valuation";
				case "02":
					return "Classification";
				case "03":
					return "Quantity";
				case "04":
					return "Antidumping Duties";
				case "05":
					return "CVC Duties";
				case "06":
					return "Special Trade Programs";
				case "07":
					return "Interest Only";
				case "08":
					return "Non-Rev Change Liq";
				case "09":
					return "Other";
				case "35":
					return "Gsp Retroactive Renewal";
				case "37":
					return "Filer Request For Refund < $20";
				case "61":
					return "Vessel Repairs";
				default:
					return code;
			}
		}

		string GetCBPReviewIndicatorDesc(ZString indicator)
		{
			switch (indicator)
			{
				case "1":
					return "Entry summary under CBP review";
				case "2":
					return "Entry summary not under CBP review";
				default:
					return indicator;
			}
		}

		string GetControlStatusDesc(ZString statusCode)
		{
			switch (statusCode)
			{
				case "1":
					return "Entry summary under trade control";
				case "2":
					return "Entry summary under CBP control";
				case "3":
					return "Entry summary inactive in ACE";
				default:
					return statusCode;
			}
		}

		string GetReleaseStatusDesc(ZString statusCode)
		{
			switch (statusCode)
			{
				case "0":
					return "Not Released";
				case "1":
					return "Released";
				case "2":
					return "Information not permitted(when the query date is less than five days from release/exam date for a border port and the selected transport mode)";
				default:
					return statusCode;
			}
		}

		string GetEntrySummaryStatusDesc(ZString statusCode)
		{
			switch (statusCode)
			{
				case "1":
					return "Entry Summary Accepted";
				case "2":
					return "Entry Summary Rejected";
				case "3":
					return "Entry Summary Canceled";
				case "4":
					return "Entry Summary Inactive in ACE";
				default:
					return statusCode;
			}
		}

		string GetCollectionStatusDesc(ZString statusCode)
		{
			switch (statusCode)
			{
				case "0":
					return "Not Paid";
				case "1":
					return "Partially Paid";
				case "2":
					return "Fully Paid";
				case "3":
					return "Duty Free";
				case "5":
					return "Drawback";
				case "6":
					return "Authorized";
				default:
					return statusCode;
			}
		}

		string GetCensusHeaderStatusDesc(ZString statusCode)
		{
			switch (statusCode)
			{
				case "0":
					return "No census warnings exist";
				case "1":
					return "An unresolved census warning exists for at least one line item";
				case "6":
					return "All census warnings have been resolved";
				default:
					return statusCode;
			}
		}

		string GetInvoiceStatusDesc(ZString statusCode)
		{
			switch (statusCode)
			{
				case "":
					return "non-EIP, electronic invoice not available";
				case "1":
					return "Open - default status when the entry summary is received and stored in ACE";
				case "2":
					return "Received - invoices have been received and stored in ACE";
				case "3":
					return "Deleted - invoices stored in ACS have been disassociated from this entry summary";
				case "4":
					return "Invoice requested - this status is set when an automated request for invoices is sent to the filer";
				default:
					return statusCode;
			}
		}

		string GetProtestStatusDesc(ZString statusCode)
		{
			var statusDescriptions = new Dictionary<string, string>()
			{
				{ "", "No protest" },
				{ "OP", "Case open" },
				{ "AP", "Approved" },
				{ "DN", "Denied" },
				{ "SP", "Suspended" },
				{ "PD", "Partially denied" },
				{ "WD", "Withdrawn denial of protest" },
				{ "UT", "Untimely denial" },
				{ "YS", "Yes" },
				{ "NO", "No" }
			};

			var result = string.Empty;
			return statusDescriptions.TryGetValue(statusCode, out result) ? result : statusCode.ToString();
		}

		string GetQuotaStatusDesc(ZString statusCode)
		{
			switch (statusCode)
			{
				case "":
					return "Quota not processed";
				case "1":
					return "Quota processed";
				case "2":
					return "Quota no lines";
				case "3":
					return "Quota processed, no lines are quota";
				case "4":
					return "Quota line deleted by CBP";
				default:
					return statusCode;
			}
		}

		string GetLateFilingStatusDesc(ZString statusCode)
		{
			switch (statusCode)
			{
				case "0":
					return "Not late";
				case "1":
					return "Over 10 days late";
				case "2":
					return "Over 30 days late";
				case "3":
					return "Over 60 days late";
				default:
					return statusCode;
			}
		}

		string GetAcceleratedDrawbackIndicatorDesc(ZInt indicator)
		{
			switch (indicator)
			{
				case 1:
					return "Accelerated drawback approved and paid";
				case 2:
					return "Accelerated drawback approved but not yet paid";
				case 3:
					return "Accelerated drawback not claimed or not approved";
				default:
					return indicator.ToString();
			}
		}

		string GetElectronicInvoiceIndicatorDesc(ZString indicator)
		{
			switch (indicator)
			{
				case "":
					return "No electronic invoice capability for summary data";
				case "E":
					return "Filer has declared ability to transmit electronically complete summary data";
				default:
					return indicator;
			}
		}

		string GetPrimarySuretyIndicatorDesc(ZString indicator)
		{
			switch (indicator)
			{
				case "Y":
					return "Primary Surety";
				case "N":
					return "Non-Primary Surety";
				default:
					return indicator;
			}
		}

		string GetBondTypeCodeDesc(ZString code)
		{
			switch (code)
			{
				case "0":
					return "No Bond Required";
				case "8":
					return "Continuous Bond";
				case "9":
					return "Single Entry Bond";
				default:
					return code;
			}
		}

		string GetBondDesignationTypeCodeDesc(ZString code)
		{
			switch (code)
			{
				case "N":
					return "CRM New Bond Added";
				case "A":
					return "Additional Bond";
				case "V":
					return "Bond Voided";
				case "R":
					return "Bond Rider";
				case "B":
					return "New Bond";
				case "U":
					return "Substitution Bond";
				case "T":
					return "Bond Terminated";
				case "C":
					return "Bond Amount Adjusted";
				case "E":
					return "Superseding Bond";
				default:
					return code;
			}
		}

		string GetMultipleBondsIndicatorDesc(ZString indicator)
		{
			switch (indicator)
			{
				case "":
					return "Not applicable.";
				case "Y":
					return "There is at least one other bond obligated.";
				case "N":
					return "This is the only bond obligated.";
				default:
					return indicator;
			}
		}

		string GetProtestTypeDesc(ZString code)
		{
			switch (code)
			{
				case "1":
					return "514 Protest Section";
				case "2":
					return "520(c) Petition";
				case "3":
					return "520(d) Petition";
				case "4":
					return "181-115 Intervention";
				default:
					return code;
			}
		}

		string GetProtestStatusDes(ZString code)
		{
			switch (code)
			{
				case "OP":
					return "Open";
				case "AP":
					return "Approved";
				case "DN":
					return "Denied";
				case "NP":
					return "Not Protestable";
				case "SP":
					return "Suspended";
				case "PD":
					return "Partly Denied";
				case "WD":
					return "Withdrawn";
				case "UT":
					return "Untimely";
				default:
					return code;
			}
		}

		string GetSummonsIndicatorDesc(ZString indicator)
		{
			switch (indicator)
			{
				case "0":
					return "No Summons";
				case "1":
					return "Summons";
				default:
					return indicator;
			}
		}

		string GetBillTypeDesc(ZString code)
		{
			switch (code)
			{
				case "1":
					return "Deferred Tax";
				case "2":
					return "Supplemental Duty";
				case "3":
					return "Regional Supplemental Duty";
				case "4":
					return "Miscellaneous";
				case "5":
					return "Region Reimbursable";
				case "6":
					return "System Reimbursable";
				default:
					return code;
			}
		}

		string GetBillStatusDesc(ZString code)
		{
			var trimedCode = code.TrimStart('0');

			switch (trimedCode)
			{
				case "1":
					return "Not Paid";
				case "2":
					return "Authorized";
				case "3":
					return "Transmitted";
				case "4":
					return "Paid";
				case "5":
					return "Partial Payment";
				case "6":
					return "Canceled";
				case "7":
					return "Void";
				case "8":
					return "Admin Pay";
				case "9":
					return "Write Off";
				case "10":
					return "Canceled with Partial Payment";
				case "11":
					return "Write-off with Partial Payment";
				default:
					return trimedCode;
			}
		}

		string GetImmediateDeliveryIndicatorDesc(ZString indicator)
		{
			switch (indicator)
			{
				case "Y":
					return "Immediate Delivery Requested";
				case "N":
					return "Immediate Delivery Not Requested";
				default:
					return indicator;
			}
		}

		#endregion

		ZDateTime GetDateAdjusted(ZString dateAsString)
		{
			var result = ZDateTime.Empty;

			var dateMeridian = dateAsString.SubstringSafe(12, 2);
			var dateReceived = dateAsString.SubstringSafe(0, 12);
			if (ZDateTime.TryParseExact(dateReceived, out result, DateFormat))
			{
				result = dateMeridian == "PM" ? result.AddHours(12) : result;
			}
			return result;
		}
		const string DateFormat = "MMddyyHHmmss";

		ZString GetCriteriaCodeDescription(ZString code)
		{
			return new CriteriaCodeList().GetDescriptionFromCode(code) ?? code;
		}
	}
}
