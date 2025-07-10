using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.RefDbEntUS;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class ACECargoManifestStatusQueryProcessor : ACEABIProcessor
	{
		readonly string noteMessage = "Please inspect the response message for further information. This can be inspected on the job from which the original message was sent or from the ‘Query Messages’ Module.";
		public const string MessageDetailsAttachmentFileName = "Message Details.html";
		public const string EmailBodyWhenUseAttachment = "Please see attachment.";

		public override void Process()
		{
			var originalMessage = Message.OriginalMessage;
			var queryData = originalMessage?.EM_LinkedObject as ICargoManifestStatusQueryData;
			if (queryData != null)
			{
				queryData.LinkToMessage(Message);
			}

			var isError = false;

			var emailBody = "";
			AttachmentDef attachmentDef = null;

			var firstBlock = messageBlocks.Count > 0 ? messageBlocks[0] : null;

			if (firstBlock != null)
			{
				if (firstBlock is IStatusesAndErrors)
				{
					isError = true;
					emailBody = ProcessingForErrorQuery();
				}
				else
				{
					updateEntryWithResults = (queryData != null && Message.OriginalMessage != null && Message.OriginalMessage.EM_ApplicationReference == "UpdateEntryWithResults");
					(emailBody, attachmentDef) = ProcessForSuccessfulQuery(queryData);
				}
			}

			var url = "";
			var jobNumber = "Unknown";
			if (queryData != null)
			{
				jobNumber = queryData.HumanFriendlyReference + (queryData.JobReferenceNumber.IsEmpty
																|| (!queryData.JobReferenceNumber.IsEmpty && queryData.HumanFriendlyReference == queryData.JobReferenceNumber) ? "" : " in job " + queryData.JobReferenceNumber);
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(queryData);
			}

			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			var actionTypeString = GetActionTypeText(originalMessage);
			if (actionTypeString.IsEmpty)
			{
				actionTypeString = queryData?.QueryActionType.ToString() ?? "Unknown";
			}
			GenerateAndSendEmail(url, jobNumber, "ACE Cargo/Manifest/Entry Status Query", actionTypeString, emailBody, isError, branch, attachmentDef);
		}
		ZBool updateEntryWithResults;

		void GenerateAndSendEmail(string uri, string jobNumber, string messageTypeInSubject, string actionType, string body, bool isFailure, GlbBranch branchForEmailLogo, AttachmentDef attachmentDef = null)
		{
			EmailDef email;
			string failureText = isFailure ? "(Failure) " : "";
			string hrefJobNumber = jobNumber;
			if (!string.IsNullOrEmpty(uri))
			{
				hrefJobNumber = "<a href=\"" + uri + "\">" + jobNumber + "</a>";
			}

			string subject;
			if (string.IsNullOrEmpty(jobNumber))
			{
				subject = ZString.Format("{0} Response {1}", messageTypeInSubject, failureText).Trim();
			}
			else
			{
				subject = ZString.Format("{0} Response {1}on {2} {3}", messageTypeInSubject, failureText, actionType, jobNumber);
			}

			string responseHeader;
			if (string.IsNullOrEmpty(hrefJobNumber))
			{
				responseHeader = ZString.Format("{0} Response {1}", messageTypeInSubject, failureText).Trim();
			}
			else
			{
				responseHeader = ZString.Format("{0} Response {1}on {2} {3}", messageTypeInSubject, failureText, actionType, hrefJobNumber);
			}

			if (new HtmlResponseEmailGenerator().TryGenerateEmail(subject, responseHeader, messageTypeInSubject, body, ZString.Empty, out email, branchForEmailLogo))
			{
				if (attachmentDef != null)
				{
					email.Attachments.Add(attachmentDef);
				}
				SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, branchForEmailLogo, isFailure);
			}
		}

		ZString GetActionTypeText(MQEDIMessage originalMessage)
		{
			var result = ZString.Empty;
			if (originalMessage != null)
			{
				var messageSubType = originalMessage.EM_MessageSubType;
				switch (messageSubType)
				{
					case EM_MessageSubTypeList.Codes.CargoManifestAirQuery:
						result = "Air";
						break;
					case EM_MessageSubTypeList.Codes.CargoManifestEntryQuery:
						result = "Entry";
						break;
					case EM_MessageSubTypeList.Codes.CargoManifestBillOfLadingQuery:
						result = "Bill of Lading";
						break;
					case EM_MessageSubTypeList.Codes.CargoManifestInBondQuery:
						result = "InBond";
						break;
					case EM_MessageSubTypeList.Codes.CargoManifestHAWBQuery:
						result = "HAWB";
						break;
					case EM_MessageSubTypeList.Codes.CargoManifestMAWBQuery:
						result = "MAWB";
						break;
				}
			}
			return result;
		}

		string ProcessingForErrorQuery()
		{
			HtmlTableCreator errorTable = null;

			foreach (MessageBlock block in messageBlocks)
			{
				IStatusesAndErrors errorBlock = block as IStatusesAndErrors;
				if (errorBlock != null)
				{
					if (errorTable == null)
					{
						errorTable = new HtmlTableCreator(new string[] { "Error Description for " + errorBlock.ReferenceNumber });
					}
					errorTable.WriteRow(errorBlock.NarrativeMessage);
				}
			}

			return errorTable == null ? string.Empty : errorTable.ToHtml();
		}

		bool IsWOBlock(MessageBlock block)
		{
			return block.MandatoryCharacters.StartsWith("WO");
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		(string, AttachmentDef) ProcessForSuccessfulQuery(ICargoManifestStatusQueryData queryData)
		{
			var entry = queryData as CusEntryHeader;
			var declaration = entry?.Declaration;

			var firstReleaseBlock = entry != null ? Message.GetFirstReleaseDetailBlock() : null;
			var hasEarlierReleaseDisposition = firstReleaseBlock != null && firstReleaseBlock.HasEarlierReleaseDispositionDateTime(entry.GetReleaseDispositionMessages(Message));
			var woMessageBlocks = this.messageBlocks.Where(x => IsWOBlock(x)).ToList();

			if (declaration != null && entry != null)
			{
				SimplifiedEntryStatusNotificationProcessor.UpdateMessageLinkedParentBO(GetWOMessageBlocksWithSequentialOrder(woMessageBlocks), Message, entry, hasEarlierReleaseDisposition, GetLatestPGADispositionDateForEachPGA(declaration));
			}

			var aceCargoReleaseEmailBodyDetails = SimplifiedEntryStatusNotificationProcessor.GetEmailBody(woMessageBlocks, new ZStringBuilder(), entry, Message, hasEarlierReleaseDisposition);

			var result = new ZStringBuilder();
			if (!string.IsNullOrEmpty(aceCargoReleaseEmailBodyDetails))
			{
				result.Append(aceCargoReleaseEmailBodyDetails);
			}

			int scSequence = 1;
			int sdSequence = 1;
			HtmlTableCreator tariffLineTable = null; //WR3
			HtmlTableCreator airbillNumberAndErrorsTable = null;
			ACEQWR5 lastWR5BlockProcessed = null;
			wsdErrorsTable = null; //WSD
			dispositionTable = null; //WR5

			var masterBillNumber = ZString.Empty;
			var houseBillNumber = ZString.Empty;
			var parentDeclaration = FindParentDeclarationOfQueryData(queryData);
			var isAssociatedToJob = CheckBillAssociatedToJob(parentDeclaration);
			ACEQWR1 masterBillR1Block = null;
			ACEQWR1 lastWR1Block = null;
			var isMasterBillR1Block = true;
			var isFirstWR4BlockAfterWR1Block = true;

			if (messageBlocks.Count > 0)
			{
				result.Append("<br>");
			}
			foreach (MessageBlock block in messageBlocks)
			{
				if (IsWOBlock(block))
				{
					continue;
				}

				var r1 = block as ACEQWR1;
				if (r1 != null)
				{
					if (dispositionTable != null || wsdErrorsTable != null)
					{
						AppendHtmlTablesToFinalResult(result, dispositionTable, wsdErrorsTable);
						dispositionTable = null;
						wsdErrorsTable = null;
						result.Append("<hr />");
					}

					var fIRMSCode = string.Empty;
					var tripNumber = string.Empty;
					var nextIndex = messageBlocks.IndexOf(block) + 1;
					if (nextIndex < messageBlocks.Count)
					{
						var r2 = messageBlocks[nextIndex] as ACEQWR2;
						if (r2 != null)
						{
							fIRMSCode = r2.FIRMS;
							tripNumber = r2.TripNumber;
						}
					}

					result.Append(WriteHTML_R1Block(r1, fIRMSCode, tripNumber));

					if (isMasterBillR1Block && masterBillR1Block == null && lastWR1Block != null)
					{
						masterBillR1Block = lastWR1Block;
					}
					lastWR1Block = r1;
					isMasterBillR1Block = true;
					isFirstWR4BlockAfterWR1Block = true;
					continue;
				}

				var r3 = block as ACEQWR3;
				if (r3 != null)
				{
					ProcessWR3Block(ref tariffLineTable, r3);
					continue;
				}

				var r4 = block as IManifestInfoProvider;
				if (r4 != null)
				{
					masterBillNumber = r4.MasterBillNumber;
					houseBillNumber = r4.HouseBillNumber;
					lastWR5BlockProcessed = null;

					isMasterBillR1Block = !isFirstWR4BlockAfterWR1Block && isMasterBillR1Block || houseBillNumber.IsEmpty;
					isFirstWR4BlockAfterWR1Block = false;

					if (!IsMessageOverDisplayLimit || IsBillContainedInDeclaration(parentDeclaration, masterBillNumber, houseBillNumber) || !isAssociatedToJob)
					{
						result.Append(WriteHTML_R4(r4));
						Process_R4(r4, parentDeclaration);
					}
					continue;
				}

				var r5 = block as ACEQWR5;
				if (r5 != null)
				{
					if (!IsMessageOverDisplayLimit || IsBillContainedInDeclaration(parentDeclaration, masterBillNumber, houseBillNumber) || !isAssociatedToJob)
					{
						ProcessWR5Block(parentDeclaration, ref dispositionTable, r5, masterBillNumber, houseBillNumber, lastWR5BlockProcessed);
						lastWR5BlockProcessed = r5;
					}
					continue;
				}

				var inbondStatusProvider = block as IInBondStatusProvider;
				if (inbondStatusProvider != null)
				{
					result.Append(ProcessInBondStatusBlock(inbondStatusProvider));
					continue;
				}

				var n0 = block as ACEQWN0;
				if (n0 != null)
				{
					result.Append(ProcessN0Block(n0));
					continue;
				}

				var n1 = block as ACEQWN1;
				if (n1 != null)
				{
					result.Append(ProcessN1Block(n1));
					continue;
				}

				var sb = block as ACEQWSB;
				if (sb != null)
				{
					ProcessWSBBlock(ref airbillNumberAndErrorsTable, sb);
					continue;
				}

				var sc = block as ACEQWSC;
				if (sc != null)
				{
					masterBillNumber = sc.AirWaybillNumber;
					houseBillNumber = sc.HouseAirWaybillNumber;

					if (IsBillContainedInDeclaration(parentDeclaration, masterBillNumber, houseBillNumber) || !isAssociatedToJob)
					{
						WriteHTML_SCBlock(sc, result);
						ProcessSCBlock(sc, parentDeclaration);
					}
					else
					{
						AddToAdditionalBillList(sc.AirWaybillNumber, sc.HouseAirWaybillNumber, scSequence++);
					}
					continue;
				}

				var sd = block as ACEQWSD;
				if (sd != null)
				{
					ProcessWSDBlock(parentDeclaration, ref wsdErrorsTable, sd, sdSequence++, masterBillNumber, houseBillNumber);
					continue;
				}
			}

			if (updateEntryWithResults)
			{
				ProcessR1Block(masterBillR1Block == null && isMasterBillR1Block ? lastWR1Block : masterBillR1Block, parentDeclaration);

				var totalPacks = ZInt.Zero;
				foreach (Bill bill in parentDeclaration.LowestBills)
				{
					totalPacks += (int)bill.CU_NoOfPacks;
				}
				parentDeclaration.JE_TotalNoOfPacks = totalPacks;
			}

			AppendHtmlTablesToFinalResult(result, tariffLineTable, dispositionTable, airbillNumberAndErrorsTable, wsdErrorsTable);

			if (additionBillList != null)
			{
				result.Append("<b>" + noteMessage + "</b>");
				result.Append(additionBillList.ToHtml());
			}

			var emailBody = result.ToStringWithDelimiterBetweenAppends("<br>");
			if (IsMessageOverDisplayLimit)
			{
				var htmlemail = new HtmlEmailDef();
				htmlemail.LoadHtmlUsingTemplate(emailBody, null, Guid.Empty, Guid.Empty, Guid.Empty, false);
				return (EmailBodyWhenUseAttachment, new AttachmentDef(MessageDetailsAttachmentFileName, Encoding.ASCII.GetBytes(htmlemail.Body)));
			}
			else
			{
				return (emailBody, null);
			}
		}

		HtmlTableCreator dispositionTable;
		HtmlTableCreator wsdErrorsTable;

		void ProcessSCBlock(ACEQWSC sc, JobDeclaration parentDeclaration)
		{
			if (parentDeclaration != null && parentDeclaration.IsAir && updateEntryWithResults)
			{
				var carrier = Factory.LoadTop1<USCCarrier>(new ZQuery(USCCarrierSchema.UI_AirwayBillPrefix, sc.ImportingCarrierCode));
				if (carrier != null)
				{
					parentDeclaration.US_UI_NKCarrierSCAC = carrier.UI_Code;
				}

				if (!sc.HouseAirWaybillNumber.IsEmpty)
				{
					var billHB = parentDeclaration.Bills.FindByBillNumberAndType(sc.HouseAirWaybillNumber, Customs.Business.BillTypeList.Codes.HouseBill);
					if (billHB != null)
					{
						billHB.CU_NoOfPacks = (ZDecimal)sc.ManifestQuantity1;
					}
				}
				else if (!sc.AirWaybillNumber.IsEmpty &&
					messageBlocks.OfType<ACEQWSC>().Count(x => x.AirWaybillNumber == sc.AirWaybillNumber) == 1)
				{
					var billMB = parentDeclaration.Bills.FindByBillNumberAndType(sc.AirWaybillNumber, Customs.Business.BillTypeList.Codes.MasterBill);
					if (billMB != null)
					{
						billMB.CU_NoOfPacks = (ZDecimal)sc.ManifestQuantity;
					}
				}
			}
		}

		void WriteHTML_SCBlock(ACEQWSC sc, ZStringBuilder result)
		{
			AppendHtmlTablesToFinalResult(result, dispositionTable, wsdErrorsTable);
			dispositionTable = null;
			wsdErrorsTable = null;

			result.Append("<b>" + sc.AirWaybillNumber + (!sc.HouseAirWaybillNumber.IsEmpty ? "/" + sc.HouseAirWaybillNumber : "") +
						  " Bill" + (!sc.HouseAirWaybillNumber.IsEmpty ? "s" : "") + " Details</b><br>");
			var transportTable = new HtmlTableCreator();
			transportTable.WriteRow("Importing Carrier Code", sc.ImportingCarrierCode);
			transportTable.WriteRow("Flight Number", sc.FlightNumber);
			transportTable.WriteRow("Scheduled Arrival Date", sc.ScheduledArrivalDate.ToShortDateString());
			transportTable.WriteRow("AWB", sc.AirWaybillNumber);
			transportTable.WriteRow("AWB Split Indicator", sc.PartIndicator);
			transportTable.WriteRow("AWB Manifest Qty", sc.ManifestQuantity);
			transportTable.WriteRow("AWB Boarded Qty", sc.BoardedQuantity);

			if (!sc.HouseAirWaybillNumber.IsEmpty)
			{
				transportTable.WriteRow("HAWB Number", sc.HouseAirWaybillNumber);
				transportTable.WriteRow("HAWB Split Indicator", sc.PartIndicator1);
				transportTable.WriteRow("HAWB Manifest Qty", sc.ManifestQuantity1);
				transportTable.WriteRow("HAWB Boarded Qty", sc.BoardedQuantity1);
			}

			transportTable.WriteRow("In-Bond Number", sc.InbondNumber);
			var inbondStatus = InbondStatusList[sc.InbondStatus] == null ? "" : ((CodeDescriptionPair)InbondStatusList[sc.InbondStatus]).CodeAndDescription;
			transportTable.WriteRow("In-Bond Status", inbondStatus);
			transportTable.WriteRow("In-Bond Entry Type", GetBondEntryTypeDesc(sc.InbondEntryType));
			AppendHtmlTablesToFinalResult(result, transportTable);
			transportTable = null;
		}

		bool IsMessageOverDisplayLimit
		{
			get
			{
				if (!isMessageOverDisplayLimitCached.HasValue)
				{
					isMessageOverDisplayLimitCached = messageBlocks.Count(messageBlock => messageBlock is IManifestInfoProvider || messageBlock is ACEQWSC) > 30;
				}
				return isMessageOverDisplayLimitCached.Value;
			}
		}
		bool? isMessageOverDisplayLimitCached;

		bool CheckBillAssociatedToJob(JobDeclaration declarationLink)
		{
			int billsCount = 0;
			if (declarationLink != null)
			{
				foreach (MessageBlock block in messageBlocks)
				{
					var r4 = block as IManifestInfoProvider;
					if (r4 != null)
					{
						if (IsBillContainedInDeclaration(declarationLink, r4.MasterBillNumber, r4.HouseBillNumber))
						{
							billsCount++;
						}
					}
					else
					{
						var sc = block as ACEQWSC;
						if (sc != null)
						{
							if (IsBillContainedInDeclaration(declarationLink, sc.AirWaybillNumber, sc.HouseAirWaybillNumber))
							{
								billsCount++;
							}
						}
					}
				}
			}
			return billsCount > 0;
		}

		bool IsBillContainedInDeclaration(JobDeclaration declarationLink, ZString masterBillNumber, ZString houseBillNumber)
		{
			return declarationLink != null
				&& ((!houseBillNumber.IsEmpty && FindBillsInDeclaration(declarationLink, houseBillNumber, Customs.Business.BillTypeList.Codes.HouseBill).Length > 0)
				|| (houseBillNumber.IsEmpty && !masterBillNumber.IsEmpty && FindBillsInDeclaration(declarationLink, masterBillNumber, Customs.Business.BillTypeList.Codes.MasterBill).Length > 0));
		}

		JobDeclaration FindParentDeclarationOfQueryData(ICargoManifestStatusQueryData queryData)
		{
			JobDeclaration result = null;
			if (queryData != null)
			{
				switch (queryData.TableCode)
				{
					case JobDeclarationSchema.Constants.Prefix:
						result = Factory.Load<JobDeclaration>(queryData.MessageAttacheePK);
						break;
					case CusEntryHeaderSchema.Constants.Prefix:
						result = Factory.Load<JobDeclaration>(((CusEntryHeader)queryData).CH_JE);
						break;
				}
			}
			return result;
		}

		void AppendHtmlTablesToFinalResult(ZStringBuilder emailBody, params HtmlTableCreator[] tables)
		{
			foreach (HtmlTableCreator table in tables)
			{
				if (table != null)
				{
					emailBody.Append(table.ToHtml());
				}
			}
		}

		string WriteHTML_R1Block(ACEQWR1 r1, string fIRMSCode = "", string tripNumber = "")
		{
			var result = new ZStringBuilder();

			result.Append("<b>ACE Cargo/Manifest/Entry Status Query Results</b>");
			result.Append("<br>");
			result.Append("<b>Trip Details</b>");
			result.Append("<br>");
			var table = new HtmlTableCreator();
			if (!r1.EntryFilerCode.IsEmpty)//entry query
			{
				table.WriteRow("District Port of Entry", r1.DistrictPortOfEntry);
				table.WriteRow("Entry Filer Code", r1.EntryFilerCode);
				table.WriteRow("Entry Number", r1.EntryNumber);
				table.WriteRow("Entry Type", r1.EntryTypeCode);
				if (!SocialSecurityNumberValidator.IsValidSSN(r1.ImporterOfRecordNumber))
				{
					table.WriteRow("Importer of Record Number", r1.ImporterOfRecordNumber);
				}
				table.WriteRow("Broker Reference Number", r1.BrokerReferenceNumber);
			}

			table.WriteRow("Carrier Code", r1.CarrierCode);
			table.WriteRow("Importing Conveyance Name", r1.ImportingVesselCodeOrImportingConveyanceName);
			table.WriteRow("Voyage/Flight/Trip Number", r1.VoyageFlightTripManifestNumber);
			table.WriteRow("Date of Arrival", r1.DateOfArrival.ToShortDateString());
			table.WriteRow("FIRMS Code", fIRMSCode);
			table.WriteRow("Trip Number", tripNumber);
			result.Append(table.ToHtml());

			return result.ToString();
		}

		void ProcessR1Block(ACEQWR1 r1, JobDeclaration parentDeclaration)
		{
			if (parentDeclaration != null && r1 != null)
			{
				if (parentDeclaration.IsSea || parentDeclaration.IsRail || parentDeclaration.IsAir)
				{
					parentDeclaration.JE_VesselName = r1.ImportingVesselCodeOrImportingConveyanceName;
					parentDeclaration.JE_VoyageFlightNo = r1.VoyageFlightTripManifestNumber;
					parentDeclaration.JE_DateOfArrival = r1.DateOfArrival;
				}
			}
		}

		string ProcessInBondStatusBlock(IInBondStatusProvider inbondStatusProvider)
		{
			var table = new HtmlTableCreator(new string[] { "In-Bond Status", "In-Bond Arrival Date", "In-Bond Export Date", "In-Bond Entry Type" });

			ZString desc = InbondStatusList.GetDescriptionFromCode(inbondStatusProvider.InBondStatus);
			desc = desc.IsEmpty ? inbondStatusProvider.InBondStatus : desc;

			table.WriteRow(desc, inbondStatusProvider.InBondArrivalDate, inbondStatusProvider.InBondExportDate, GetBondEntryTypeDesc(inbondStatusProvider.InbondEntryType));

			return table.ToHtml();
		}

		void Process_R4(IManifestInfoProvider r4, JobDeclaration parentDeclaration)
		{
			if (parentDeclaration != null && updateEntryWithResults)
			{
				if (parentDeclaration.IsSea || parentDeclaration.IsRail)
				{
					var billNum = ZString.Empty;
					var billType = ZString.Empty;

					if (!r4.SubHouseBillNumber.IsEmpty)
					{
						billNum = r4.SubHouseBillNumber;
						billType = Customs.Business.BillTypeList.Codes.SubHouseBill;
					}
					else if (!r4.HouseBillNumber.IsEmpty &&
						messageBlocks.OfType<IManifestInfoProvider>().Count(x => x.HouseBillNumber == r4.HouseBillNumber) == 1)
					{
						billNum = r4.HouseBillNumber;
						billType = Customs.Business.BillTypeList.Codes.HouseBill;
					}
					else if (!r4.MasterBillNumber.IsEmpty &&
						messageBlocks.OfType<IManifestInfoProvider>().Count(x => x.MasterBillNumber == r4.MasterBillNumber) == 1)
					{
						billNum = r4.MasterBillNumber;
						billType = Customs.Business.BillTypeList.Codes.MasterBill;
					}

					var bill = parentDeclaration.Bills.FindByBillNumberAndType(billNum, billType);
					if (bill != null)
					{
						bill.CU_PackType = r4.Unit;
						parentDeclaration.JE_TotalNoOfPacksPackType = bill.CU_PackType.Left(3);
						bill.CU_NoOfPacks = r4.ManifestQuantity;
					}
				}
			}
		}

		string WriteHTML_R4(IManifestInfoProvider r4)
		{
			var table = new HtmlTableCreator(new string[] { "In-Bond Number", "Master Bill Number", "House Bill Number", "Sub-House Bill Number", "Manifest Qty", "UQ", "Master Bill Issuer Code", "House Bill Issuer Code", "Bill of Lading Type", "ISF Indicator", "MOT" });

			table.WriteRow(r4.InBondNumber, r4.MasterBillNumber, r4.HouseBillNumber, r4.SubHouseBillNumber, r4.ManifestQuantity, r4.Unit, r4.IssuerCodeOfMasterBillNumber, r4.IssuerCodeOfHouseBillNumber, GetBillOfLadingTypeDescription(r4.BillOfLadingType), GetISFIndicatorDescription(r4.ImporterSecurityFilingIndicator), GetMOTDescription(r4.ModeOfTransportationCode));

			return table.ToHtml();
		}

		void ProcessWR3Block(ref HtmlTableCreator tableCreator, ACEQWR3 wr3)
		{
			if (tableCreator == null)
			{
				tableCreator = new HtmlTableCreator(new string[] { "Line No.", "Country of Origin", "Tariff Number" });
			}
			tableCreator.WriteRow(wr3.RecordControlNumber, wr3.CountryOfOrigin, wr3.TariffNumber);
		}

		void ProcessWR5Block(JobDeclaration declaration, ref HtmlTableCreator tableCreator, ACEQWR5 wr5, ZString masterBillNumber, ZString houseBillNumber, ACEQWR5 lastWR5Processed)
		{
			if (tableCreator == null)
			{
				tableCreator = new HtmlTableCreator(new string[] { "Sequence", "Disposition Code", "Disposition Desc", "Disposition Date/Time", "Quantity" });
			}

			var dispositionDateTime = DateTimeParser.GetDateTimeFromZDateAndStringTime(wr5.DispositionActionDate, wr5.DispositionActionTime);
			tableCreator.WriteRow(wr5.Sequence, wr5.DispositionActionCode, wr5.NarrativeMessage, dispositionDateTime.ToLongTimeString(), wr5.Quantity.ToString());

			if (declaration != null && IsQueryMadeForBill(Message) && (lastWR5Processed == null || !lastWR5Processed.Equals(wr5)))
			{
				if (!houseBillNumber.IsEmpty)
				{
					FindBillAndAttachDispositionData(declaration, houseBillNumber, BillTypeList.Codes.HouseBill, wr5.DispositionActionCode, dispositionDateTime);
				}
				else if (!masterBillNumber.IsEmpty)
				{
					FindBillAndAttachDispositionData(declaration, masterBillNumber, BillTypeList.Codes.MasterBill, wr5.DispositionActionCode, dispositionDateTime);
				}
			}
		}

		void ProcessWSBBlock(ref HtmlTableCreator tableCreator, ACEQWSB wsb)
		{
			if (tableCreator == null)
			{
				tableCreator = new HtmlTableCreator(new string[] { "Air Way Bill Number", "House Air Way Bill Number", "Error" });
			}

			var errorMessage = wsb.ErrorMessageIdentifier + " " + wsb.NarrativeMessage;
			tableCreator.WriteRow(wsb.AirWaybillNumber, wsb.HouseAirWaybillNumber, errorMessage);
		}

		void ProcessWSDBlock(JobDeclaration declaration, ref HtmlTableCreator tableCreator, ACEQWSD wsd, int sequence, ZString masterBillNumber, ZString houseBillNumber)
		{
			if (tableCreator == null)
			{
				tableCreator = new HtmlTableCreator(new string[] { "Bill Number", "Sequence", "Disposition Code", "Disposition Desc", "Disposition Date/Time" });
			}

			var dispositionDateTime = DateTimeParser.GetDateTimeFromZDateAndStringTime(wsd.DispositionActionDate, wsd.DispositionActionTime);
			tableCreator.WriteRow(houseBillNumber.IsEmpty ? masterBillNumber : houseBillNumber, sequence, wsd.DispositionCode, wsd.NarrativeMessage, dispositionDateTime.ToLongTimeString());

			if (declaration != null && IsQueryMadeForBill(Message))
			{
				if (!houseBillNumber.IsEmpty)
				{
					FindBillAndAttachDispositionData(declaration, houseBillNumber, BillTypeList.Codes.HouseBill, wsd.DispositionCode, dispositionDateTime);
				}
				else if (!masterBillNumber.IsEmpty)
				{
					FindBillAndAttachDispositionData(declaration, masterBillNumber, BillTypeList.Codes.MasterBill, wsd.DispositionCode, dispositionDateTime);
				}
			}
		}

		public static bool IsQueryMadeForBill(MQEDIMessage message)
		{
			var result = false;
			var originalMessage = message.OriginalMessage;
			if (originalMessage != null)
			{
				var messageBlock = originalMessage.MessageBlock.MessageBlocks.OfType<ICargoManifestQueryInputR1Block>().FirstOrDefault();
				result = messageBlock != null && (!messageBlock.BillOfLadingNumber.IsEmpty ||
					!messageBlock.AirWaybillNumber.IsEmpty || !messageBlock.HouseAirWaybillNumber.IsEmpty);
			}
			return result;
		}

		void FindBillAndAttachDispositionData(JobDeclaration declaration, ZString billNumber, ZString billType, ZString dispositionCode, ZDateTime dispositionDate)
		{
			var bills = FindBillsInDeclaration(declaration, billNumber, billType);
			if (bills.Length > 0)
			{
				var bill = bills[0];
				var dispositionCodes = bill.DispositionCodes;
				dispositionCodes.AddNewIfNotExist(dispositionCode, dispositionDate, BillDispositionSourceList.Codes.CQ);
				SimplifiedEntryStatusNotificationProcessor.UpdateBillMessageStatus(dispositionCode, bill);
			}
		}
		public static Bill[] FindBillsInDeclaration(JobDeclaration declaration, ZString billNumber, ZString billType)
		{
			var query = new ZQuery(CusDecHouseBillSchema.CU_BillNum, billNumber);
			query.AddToFilter(CusDecHouseBillSchema.CU_BillType, billType);
			return (Bill[])declaration.Bills.Find(query);
		}

		void AddToAdditionalBillList(ZString masterBillNo, ZString houseBillNo, int sequence)
		{
			if (additionBillList == null)
			{
				additionBillList = new HtmlTableCreator(new string[] { "Sequence", "Bill Number" });
			}
			if (!masterBillNo.IsEmpty)
			{
				additionBillList.WriteRow(sequence, masterBillNo + (!houseBillNo.IsEmpty ? "/" + houseBillNo : ""));
			}
		}
		HtmlTableCreator additionBillList;

		string ProcessN1Block(ACEQWN1 n1)
		{
			var result = new ZStringBuilder();

			result.Append("<b>Additional In-Bond Trip Details</b>");
			result.Append("<br>");
			var table = new HtmlTableCreator();
			table.WriteRow("In-Bond Number", n1.InbondEntryNumber);
			table.WriteRow("Manifested Port Of Unlading", n1.ManifestedPortOfUnladingImport);
			table.WriteRow("Actual Port Of Unlading", n1.ActualPortOfUnladingImport);
			table.WriteRow("Actual Port Of Unlading Ocean Vessel Diversion", n1.ActualPortOfUnladingImportOceanVesselDiversion);
			table.WriteRow("In-Bond Originating Port", n1.InbondOriginatingPort);
			table.WriteRow("Manifested In-Bond Destination Port", n1.ManifestedInbondDestinationPort);
			table.WriteRow("Actual In-Bond Destination Manual Diversion", n1.ActualInbondDestinationManualDiversion);
			table.WriteRow("Actual In-Bond Destination Via Edi In-Bond Diversion", n1.ActualInbondDestinationViaEdiInbondDiversion);
			table.WriteRow("Vessel Departure Port", n1.VesselDeparturePort);
			table.WriteRow("Vessel Departure Date", n1.VesselDepartureDate);
			table.WriteRow("Container Load Port", n1.ContainerLoadPort);
			table.WriteRow("Container Load Date", n1.ContainerLoadDate);
			result.Append(table.ToHtml());

			return result.ToString();
		}

		string ProcessN0Block(ACEQWN0 n0)
		{
			var result = new ZStringBuilder();

			result.Append("<b>Bill Of Lading Quantities</b>");
			result.Append("<br>");
			var table = new HtmlTableCreator();
			table.WriteRow("Master Bill Amended Quantity", n0.MasterBillAmendedQuantity);
			table.WriteRow("House Bill Amended Quantity", n0.HouseBillAmendedQuantity);
			result.Append(table.ToHtml());

			return result.ToString();
		}

		string GetBillOfLadingTypeDescription(string code)
		{
			switch (code)
			{
				case "0":
					return "Regular Bill of Lading";
				case "M":
					return "Master Bill of Lading";
				case "H":
					return "House Bill of Lading";
				case "F":
					return "Freight Remaining on Board";
				default:
					return "";
			}
		}

		string GetISFIndicatorDescription(string code)
		{
			switch (code)
			{
				case "Y":
					return "ISF On File";
				case "N":
					return "ISF Not On File";
				default:
					return "";
			}
		}

		string GetMOTDescription(string code)
		{
			switch (code)
			{
				case "1":
					return "Ocean";
				case "2":
					return "Rail";
				case "3":
					return "Truck";
				default:
					return "";
			}
		}

		CodeDescriptionPairList InbondStatusList
		{
			get
			{
				return Factory.GetCachedValue("InbondStatusList",
				delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(new InBondStatusCMQList());
					result.AddRange(new InbondStatusList());
					return result;
				});
			}
		}

		InbondCommonTypeList InBondEntryTypeList
		{
			get { return Factory.GetCachedValue<InbondCommonTypeList>(); }
		}

		ZString GetBondEntryTypeDesc(ZString inBondEntryType)
		{
			var result = InBondEntryTypeList.GetDescriptionFromCode(inBondEntryType);
			if (!string.IsNullOrEmpty(result))
			{
				result = inBondEntryType + " - " + result;
			}
			else
			{
				result = inBondEntryType;
			}
			return result;
		}

		IEnumerable<KeyValuePair<PGALineKey, ZDateTime>> GetLatestPGADispositionDateForEachPGA(JobDeclaration declaration)
		{
			var result = new Dictionary<PGALineKey, ZDateTime>();

			var groupsInMessage = messageBlocks.OfType<IPGADispositionProvider>().GroupBy(x => new PGALineKey(x));
			foreach (var group in groupsInMessage)
			{
				result.Add(group.Key, group.Max(x => x.DispositionDateTime));
			}

			var groupsInDeclaration = declaration.OGADispositionCodes.Cast<OGADispositionData>().GroupBy(x => new PGALineKey(x));

			if (!result.Keys.EqualIgnoringOrder(groupsInDeclaration.Select(x => x.Key)))
			{
				return result;
			}

			foreach (var group in groupsInDeclaration)
			{
				ZDateTime latestDispositionDateFromMessage;
				result.TryGetValue(group.Key, out latestDispositionDateFromMessage);

				if (latestDispositionDateFromMessage.IsEmpty || latestDispositionDateFromMessage > group.Max(x => x.US_DispositionDate))
				{
					return result;
				}
			}

			return new Dictionary<PGALineKey, ZDateTime>();
		}

		List<MessageBlock> GetWOMessageBlocksWithSequentialOrder(List<MessageBlock> messageBlocksInReversedOrder)
		{
			var wo10GroupNumber = ZInt.Zero;
			List<MessageBlock> currentBlocksInWO10Group = null;
			var messageBlocksInGroup = new Dictionary<ZInt, List<MessageBlock>>();
			foreach (var messageBlock in messageBlocksInReversedOrder)
			{
				if (currentBlocksInWO10Group == null || messageBlock is ACEQWO10)
				{
					if (!wo10GroupNumber.IsEmpty && currentBlocksInWO10Group != null && !messageBlocksInGroup.ContainsKey(wo10GroupNumber))
					{
						messageBlocksInGroup.Add(wo10GroupNumber, currentBlocksInWO10Group);
					}

					currentBlocksInWO10Group = new List<MessageBlock>();
					wo10GroupNumber++;
				}

				currentBlocksInWO10Group.Add(messageBlock);
			}

			if (!wo10GroupNumber.IsEmpty && currentBlocksInWO10Group != null && !messageBlocksInGroup.ContainsKey(wo10GroupNumber))
			{
				messageBlocksInGroup.Add(wo10GroupNumber, currentBlocksInWO10Group);
			}

			var messageBlocksInSequentialOrder = new List<MessageBlock>();
			foreach (var keyValuePair in messageBlocksInGroup.OrderByDescending(x => x.Key))
			{
				messageBlocksInSequentialOrder.AddRange(keyValuePair.Value);
			}

			return messageBlocksInSequentialOrder;
		}
	}

	public class PGALineKey
	{
		public PGALineKey(IPGADispositionProvider dispositionProvider)
		{
			this.OtherAgencyQuotaIdentifier = dispositionProvider.OtherAgencyQuotaIdentifier;
			this.BeginningCBPLineNo = dispositionProvider.BeginningCBPLineNo;
			this.BeginningOGALineNo = dispositionProvider.BeginningOGALineNo;
			this.BeginningTariffPosition = dispositionProvider.BeginningTariffPosition;
			this.EndingCBPLineNo = dispositionProvider.EndingCBPLineNo;
			this.EndingOGALineNo = dispositionProvider.EndingOGALineNo;
			this.EndingTariffPosition = dispositionProvider.EndingTariffPosition;
			this.RangeIndicator = dispositionProvider.RangeIndicator;
		}

		public PGALineKey(OGADispositionData dispositionData)
		{
			this.OtherAgencyQuotaIdentifier = dispositionData.US_OGAIdentifier;
			this.BeginningCBPLineNo = dispositionData.US_OGADispositionBeginningCBPLine;
			this.BeginningOGALineNo = dispositionData.US_OGADispositionBeginningOGALine;
			this.BeginningTariffPosition = dispositionData.US_BeginningTariffPosition;
			this.EndingCBPLineNo = dispositionData.US_OGADispositionEndCBPLine;
			this.EndingOGALineNo = dispositionData.US_OGADispositionEndOGALine;
			this.EndingTariffPosition = dispositionData.US_EndingTariffPosition;
			this.RangeIndicator = dispositionData.US_OGADispositionRangeIndicator;
		}

		public readonly ZString OtherAgencyQuotaIdentifier;
		public readonly ZString BeginningCBPLineNo;
		public readonly ZString BeginningOGALineNo;
		public readonly ZString BeginningTariffPosition;
		public readonly ZString EndingCBPLineNo;
		public readonly ZString EndingOGALineNo;
		public readonly ZString EndingTariffPosition;
		public readonly ZString RangeIndicator;

		public override int GetHashCode()
		{
			return OtherAgencyQuotaIdentifier.GetHashCode() ^
				   BeginningCBPLineNo.GetHashCode() ^
				   BeginningOGALineNo.GetHashCode() ^
				   BeginningTariffPosition.GetHashCode() ^
				   EndingCBPLineNo.GetHashCode() ^
				   EndingOGALineNo.GetHashCode() ^
				   EndingTariffPosition.GetHashCode() ^
				   RangeIndicator.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var lineKey = (PGALineKey)obj;
			return this.OtherAgencyQuotaIdentifier == lineKey.OtherAgencyQuotaIdentifier
				&& this.BeginningCBPLineNo == lineKey.BeginningCBPLineNo
				&& this.BeginningOGALineNo == lineKey.BeginningOGALineNo
				&& this.BeginningTariffPosition == lineKey.BeginningTariffPosition
				&& this.EndingCBPLineNo == lineKey.EndingCBPLineNo
				&& this.EndingOGALineNo == lineKey.EndingOGALineNo
				&& this.EndingTariffPosition == lineKey.EndingTariffPosition
				&& this.RangeIndicator == lineKey.RangeIndicator;
		}
	}
}
