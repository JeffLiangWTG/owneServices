using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Output;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation)]
	public class CourtesyNoticeProcessor : ACSABIProcessor
	{
		#region IKeysForBlockingParallelProcessingProvider Members

		protected override ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(CBPEDIMessage message)
		{
			var jobNumber = ZString.Empty;
			if (message.MessageBlock.MessageBlocks.FirstOrDefault() is MessageBlock firstMessageBlock)
			{
				if (firstMessageBlock is CTNN1 n1)
				{
					jobNumber = USIUniversalCustomsMessageProcessor.GetEntryJobNumberKey(n1.BrokerNumberEntryFilerCode, n1.EntryNumber, GlbCompany.CurrentCompany.PK);
				}
				else if (firstMessageBlock is CTNN3 n3)
				{
					jobNumber = USIUniversalCustomsMessageProcessor.GetEntryJobNumberKey(n3.BrokerNumberEntryFilerCode, n3.EntryNumber, GlbCompany.CurrentCompany.PK);
				}
			}
			return LinkedBusinessObjectMetaData.New(ZString.Empty, ZGuid.Empty, ZGuid.Empty, jobNumber);
		}

		#endregion

		public override void Process()
		{
			liquidationProvider = null;
			onlyN3BlocksInMessage = true;

			foreach (var messageBlock in messageBlocks)
			{
				var n1 = messageBlock as CTNN1;
				if (n1 != null)
				{
					onlyN3BlocksInMessage = false;
					CreateNewNoticeData();
					ProcessN1(n1);
					AttachEntryNumberAndDeclarationData();
				}
				else
				{
					var n2 = messageBlock as CTNN2;
					if (n2 != null)
					{
						ProcessN2(n2);
						if (liquidationProvider != null && !IsReprocessingMessage)
						{
							liquidationProvider.SetAnticipatedLiquidationDate(noticeData.B8_LiquidationDate);
						}
					}
					else
					{
						var n3 = messageBlock as CTNN3;
						if (n3 != null)
						{
							CreateNewNoticeData();
							ProcessN3(n3);
							AttachEntryNumberAndDeclarationData();
						}
						else
						{
							var n4 = messageBlock as CTNN4;
							if (n4 != null)
							{
								ProcessN4(n4);
							}
						}
					}
				}
			}

			if (!IsReprocessingMessage)
			{
				EmailNotification();
			}
			else
			{
				Message.EM_MessageOwner = ZString.Empty;
			}
		}

		bool IsReprocessingMessage
		{
			get { return Message.EM_MessageOwner == Reprocessing; }
		}

		void CreateNewNoticeData()
		{
			noticeData = Factory.New<CusLiquidation>();
			noticeData.B8_SystemCreateDate = A != null ? A.CurrentDate : ZDateTime.UtcNow;
			noticeData.B8_GC = GlbCompany.CurrentCompany.PK;
			Message.EM_LinkUniqueID = noticeData.PK;
			Message.EM_LinkedObject = noticeData;
			Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.LiquidationNotice;
			Liquidations.Add(noticeData);
		}

		void AttachEntryNumberAndDeclarationData()
		{
			liquidationProvider = null;

			var loadedDeclaration = CusEntryNumberExtension.GetJobDeclarationByEntryNumberAndFilerCode(Factory, noticeData.B8_EntryNumber, noticeData.B8_EntryFilerCode);
			if (loadedDeclaration != null)
			{
				liquidationProvider = loadedDeclaration;

				var matchedEntries = (CusEntryHeader[])loadedDeclaration.ActiveEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, new ZString[] { CusEntryHeaderMessageTypeList.Codes.EntrySummary, CusEntryHeaderMessageTypeList.Codes.ReconEntry }));
				if (matchedEntries.Length > 0)
				{
					liquidationProvider = matchedEntries[0];
				}
			}

			if (liquidationProvider != null)
			{
				if (!IsReprocessingMessage)
				{
					liquidationProvider.SetAnticipatedLiquidatedDuty(noticeData.B8_LiquidatedDuty);
				}

				var declaration = liquidationProvider.Declaration;
				if (declaration != null)
				{
					noticeData.B8_JE = declaration.PK;
					noticeData.B8_ClusterKey = declaration.JE_ClusterKey;

					if (noticeData.B8_NoOfSuspensions != 0 &&
						!declaration.Liquidations.Where(x => x.B8_NoOfSuspensions != 0).Any(x => x.B8_SystemCreateDate > noticeData.B8_SystemCreateDate) &&
						declaration.FormalEntry is CusEntryHeader entryHeader)
					{
						entryHeader.US_TIBNumOfExtensions = noticeData.B8_NoOfSuspensions;
					}

					// Forward only if external broker exists and communication mode is defined for bird. Should not forward if this message is in fact a converted message from BIRD messages.
					if (Message.EM_ApplicationReference != ApplicationIdentifierCodeList.Codes.BIRDTransaction && declaration.HasBIRDCommunicationMode() && !IsReprocessingMessage)
					{
						new MessageBuilders.BIRDLiquidationMessageBuilder(declaration).PopulateMessage();
					}
				}
			}
		}

		#region Process Message Blocks

		void ProcessN1(CTNN1 n1)
		{
			noticeData.DistrictPortOfEntry = n1.DistrictPortOfEntrySummary;
			noticeData.B8_EntryFilerCode = n1.BrokerNumberEntryFilerCode;
			noticeData.B8_EntryNumber = n1.EntryNumber;
			noticeData.B8_EntryType = n1.EntryType;
			noticeData.B8_ImportOfRecordNo = n1.ImporterOfRecordNumber;
			noticeData.B8_DutyPaid = n1.TotalPaidDuty;
			noticeData.B8_TaxPaid = n1.TotalPaidTax;
			noticeData.B8_LiquidatedDuty = n1.LiquidatedDuty;
			noticeData.B8_LiquidatedTax = n1.LiquidatedTax;
			noticeData.NegativeInterestIndicator = n1.NegativeInterestAmountIndicator;
		}

		void ProcessN2(CTNN2 n2)
		{
			noticeData.B8_LiquidationDate = n2.LiquidationDate;
			noticeData.B8_ChangeLiquidationReasonCode = n2.ChangeLiquidationReasonCode.ToString();
			noticeData.B8_BrokerReferenceNo = n2.BrokerReferenceNumber;
			noticeData.B8_CustomsDocumentFilingLocation = n2.CBPDocumentFilingLocation;
			noticeData.B8_LiquidationType = n2.LiquidationType;
			noticeData.B8_EntryDate = n2.EntryDate;
			noticeData.B8_TotalPaidAntiDumpingDuty = n2.TotalPaidAntidumpingDuty;
			noticeData.B8_InterestAmount = n2.InterestAmount;
			if (noticeData.NegativeInterestIndicator == 1)
			{
				noticeData.B8_InterestAmount = noticeData.B8_InterestAmount * -1;
			}
		}

		void ProcessN3(CTNN3 n3)
		{
			noticeData.B8_EntryFilerCode = n3.BrokerNumberEntryFilerCode;
			noticeData.B8_BrokerReferenceNo = n3.BrokerReferenceNumber;
			noticeData.DistrictPortOfEntry = n3.DistrictPortOfEntrySummary;
			noticeData.B8_EntryDate = n3.EntryDate;
			noticeData.B8_EntryNumber = n3.EntryNumber;
			noticeData.B8_EntryType = n3.EntryTypeCode;
			noticeData.B8_ImportOfRecordNo = n3.ImporterOfRecordNumber;
			noticeData.B8_ExtensionSuspensionCode = n3.ExtensionSuspensionCode.ToString();
			noticeData.B8_NoOfSuspensions = ZByte.ParseSafe(n3.NumberOfTimesExtended.ToString(), 0);
			noticeData.B8_ExtensionSuspensionDate = n3.ExtensionSuspensionNoticeDate;
			noticeData.B8_ExtensionSuspensionCode2 = n3.ExtensionSuspensionCode2.ToString();
			noticeData.B8_ExtensionSuspensionCode3 = n3.ExtensionSuspensionCode3.ToString();
			noticeData.B8_ExtensionSuspensionCode4 = n3.ExtensionSuspensionCode4.ToString();
		}

		void ProcessN4(CTNN4 n4)
		{
			noticeData.B8_TotalPaidCounterVailingDuty = n4.TotalPaidCountervailingDuty;
			noticeData.B8_TotalPaidFees = n4.TotalPaidFees;
			noticeData.B8_TotalLiquidatedAntiDumpingDuty = n4.LiquidatedAntidumpingDuty;
			noticeData.B8_TotalLiquidatedCounterVailingDuty = n4.LiquidatedCountervailingDuty;
			noticeData.B8_TotalLiquidatedFees = n4.LiquidatedFees;
			noticeData.B8_ChangeLiquidationReasonCode2 = n4.ChangeLiquidationReasonCode2;
			noticeData.B8_ChangeLiquidationReasonCode3 = n4.ChangeLiquidationReasonCode3;
			noticeData.B8_ChangeLiquidationReasonCode4 = n4.ChangeLiquidationReasonCode4;
		}

		#endregion

		#region EMail

		void EmailNotification()
		{
			foreach (CusLiquidation lND in Liquidations)
			{
				var suppressNoChangeLiquidation = USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.
					GetFallBackValueAtAllLevels(lND.B8_GC.ToGuid(), Guid.Empty, Guid.Empty).SuppressNoChangeLiquidations;

				if (!suppressNoChangeLiquidation || lND.B8_ChangeLiquidationReasonCode != ChangeLiquidationReasonCodeList.Codes.Code99 && lND.B8_ChangeLiquidationReasonCode != ChangeLiquidationReasonCodeList.Codes.Code00)
				{
					EmailDef email = GenerateEmail(lND);

					var brokerEmail = GetBrokerEmailFromDeclaration(lND.Declaration);
					GlbBranch branch = lND.Declaration != null ? lND.Declaration.Branch : null;
					SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, branch, brokerEmail, false);
				}

				LogMessageStatusChangeEventAgainstTopLevelBusinessObject(lND.Declaration);
			}
		}

		ZString GetBrokerEmailFromDeclaration(JobDeclaration declaration)
		{
			var result = ZString.Empty;

			if (declaration != null)
			{
				var broker = declaration.CusAgent;
				if (broker != null && broker.GS_IsActive)
				{
					result = broker.GS_EmailAddress;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		EmailDef GenerateEmail(CusLiquidation noticeData)
		{
			string uri = "";
			string emailTemplateHtml;
			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.US.Business.MessageProcessors.Template.Liquidation.htm"))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}

			JobDeclaration declaration = noticeData.Declaration;
			if (declaration != null)
			{
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration);
			}

			ZString jobNumber = !noticeData.JobNumber.IsEmpty && declaration != null ? noticeData.JobNumber : (ZString)"Non CargoWise One Job";
			if (!string.IsNullOrEmpty(uri))
			{
				emailTemplateHtml = emailTemplateHtml.Replace("{0}", "<a href=\"" + uri + "\">" + jobNumber + "</a>");
			}
			else
			{
				emailTemplateHtml = emailTemplateHtml.Replace("{0}", jobNumber);
			}

			emailTemplateHtml = emailTemplateHtml.Replace("{1}", noticeData.B8_EntryNumber);
			if (!noticeData.B8_LiquidationType.IsEmpty)
			{
				emailTemplateHtml = emailTemplateHtml.Replace("<!--LiquidationType-->", "Liquidation Type : " + noticeData.B8_LiquidationType + " - " + LiquidationTypeCodeList.GetDescriptionFromCode(noticeData.B8_LiquidationType.ToString()) + "<br />");
			}

			if (!noticeData.B8_LiquidationDate.IsEmpty)
			{
				emailTemplateHtml = emailTemplateHtml.Replace("<!--LiquidationDate-->", "Liquidation Date : " + noticeData.B8_LiquidationDate + "<br />");
			}

			HtmlTableCreator creator = new HtmlTableCreator(new string[] { "Column", "Description" });
			creator.WriteRow("District - Port of Entry", noticeData.DistrictPortOfEntry);
			creator.WriteRow("Broker Number", noticeData.B8_EntryFilerCode);
			creator.WriteRow("Entry Type", noticeData.B8_EntryType);
			creator.WriteRow("Entry Date", noticeData.B8_EntryDate);
			if (!SocialSecurityNumberValidator.IsValidSSN(noticeData.B8_ImportOfRecordNo))
			{
				creator.WriteRow("Importer Of Record", noticeData.B8_ImportOfRecordNo);
			}

			//should provide job number only in case when non enterprise job
			if (declaration == null)
			{
				creator.WriteRow("Broker Reference Number", noticeData.B8_BrokerReferenceNo);
			}

			creator.WriteRow("CBP Document Filing Location", noticeData.B8_CustomsDocumentFilingLocation);

			var reasonCodes = GetChangeLiquidationReasonCodes(noticeData);
			if (!reasonCodes.IsEmpty)
			{
				creator.WriteRow("Change Liquidation Reason Code(s)", reasonCodes);
			}

			var extensionSuspensionCodes = GetExtensionSuspensionCodes(noticeData);
			if (!extensionSuspensionCodes.IsEmpty)
			{
				creator.WriteRow("Extension/Suspension Code(s)", extensionSuspensionCodes);
			}

			if (!noticeData.B8_ExtensionSuspensionDate.IsEmpty)
			{
				creator.WriteRow("Extension/Suspension Notice Date", noticeData.B8_ExtensionSuspensionDate);
			}

			creator.WriteRow("Number Of Times Extended", noticeData.B8_NoOfSuspensions);

			// always empty if standalone N3 blocks coming
			if (!onlyN3BlocksInMessage)
			{
				creator.WriteRow(" ", " ");
				creator.WriteRow(" ", " ");
				creator.WriteRow("Total Paid Duty", noticeData.B8_DutyPaid.ToString("$0.00"));
				creator.WriteRow("Total Paid Antidumping Duty", noticeData.B8_TotalPaidAntiDumpingDuty.ToString("$0.00"));
				creator.WriteRow("Total Paid Countervailing Duty", noticeData.B8_TotalPaidCounterVailingDuty.ToString("$0.00"));
				creator.WriteRow("Total Paid Tax", noticeData.B8_TaxPaid.ToString("$0.00"));
				creator.WriteRow("Total Paid Fees", noticeData.B8_TotalPaidFees.ToString("$0.00"));
				creator.WriteRow(" ", " ");
				creator.WriteRow(" ", " ");
				creator.WriteRow("Liquidated Duty", noticeData.B8_LiquidatedDuty.ToString("$0.00"));
				creator.WriteRow("Liquidated Antidumping Duty", noticeData.B8_TotalLiquidatedAntiDumpingDuty.ToString("$0.00"));
				creator.WriteRow("Liquidated Countervailing Duty", noticeData.B8_TotalLiquidatedCounterVailingDuty.ToString("$0.00"));
				creator.WriteRow("Liquidated Tax", noticeData.B8_LiquidatedTax.ToString("$0.00"));
				creator.WriteRow("Liquidated Fees", noticeData.B8_TotalLiquidatedFees.ToString("$0.00"));
				creator.WriteRow(" ", " ");
				creator.WriteRow(" ", " ");
				creator.WriteRow("Interest Amount", noticeData.B8_InterestAmount.ToString("$0.00"));
			}

			emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", creator.ToHtml());
			return CreateEmail("Courtesy Notice of Liquidation for " + jobNumber + " / " + noticeData.B8_EntryNumber, emailTemplateHtml);
		}

		ZString GetChangeLiquidationReasonCodes(CusLiquidation noticeData)
		{
			var sb = new ZStringBuilder();
			sb.AppendIfNotEmpty(noticeData.B8_ChangeLiquidationReasonCode);
			sb.AppendIfNotEmpty(noticeData.B8_ChangeLiquidationReasonCode2);
			sb.AppendIfNotEmpty(noticeData.B8_ChangeLiquidationReasonCode3);
			sb.AppendIfNotEmpty(noticeData.B8_ChangeLiquidationReasonCode4);
			return sb.ToStringWithDelimiterBetweenAppends(",");
		}

		ZString GetExtensionSuspensionCodes(CusLiquidation noticeData)
		{
			var sb = new ZStringBuilder();
			sb.AppendIfNotEmpty(noticeData.B8_ExtensionSuspensionCode);
			sb.AppendIfNotEmpty(noticeData.B8_ExtensionSuspensionCode2);
			sb.AppendIfNotEmpty(noticeData.B8_ExtensionSuspensionCode3);
			sb.AppendIfNotEmpty(noticeData.B8_ExtensionSuspensionCode4);
			return sb.ToStringWithDelimiterBetweenAppends(",");
		}

		protected override IRegistryItem GetEmailGroupRegistryItem()
		{
			return USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup;
		}

		protected override IRegistryItem GetAlternativeEmailGroupRegistryItemWhenNoRecipientFound()
		{
			return USCustomsDataRegistry.Instance.ABIMessagesGroup;
		}

		#region CodeDescripitonLists

		LiquidationTypeCodeList LiquidationTypeCodeList
		{
			get { return fLiquidationTypeCodeList ?? (fLiquidationTypeCodeList = new LiquidationTypeCodeList()); }
		}
		LiquidationTypeCodeList fLiquidationTypeCodeList;

		#endregion

		#endregion

		ILiquidationProvider liquidationProvider;
		bool onlyN3BlocksInMessage;
		CusLiquidation noticeData;
		List<CusLiquidation> Liquidations
		{
			get { return liquidations ?? (liquidations = new List<CusLiquidation>()); }
		}
		List<CusLiquidation> liquidations;
	}
}
