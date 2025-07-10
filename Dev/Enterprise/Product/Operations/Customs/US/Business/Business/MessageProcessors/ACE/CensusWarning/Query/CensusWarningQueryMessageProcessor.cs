using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse)]
	class CensusWarningQueryMessageProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			ZString uri = ZString.Empty;
			bool isFailure;
			string jobNumber = "multiple entries";
			var branch = GlbBranch.CurrentBranch;

			CusEntryHeader entryHeader = GetEntryHeader();
			if (entryHeader != null)
			{
				jobNumber = entryHeader.Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entryHeader);
				branch = entryHeader.Branch;
			}

			string email = GenerateEmailBody(out isFailure);
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Census Warning Query", email, isFailure, branch, entryHeader);
		}

		CusEntryHeader GetEntryHeader()
		{
			return CusEntryHeaderLinker.Link(Message);
		}

		string GenerateEmailBody(out bool isFailure)
		{
			isFailure = false;

			var cj2Blocks = Message.MessageBlock.MessageBlocks.OfType<ACWQCJ2>();
			if (cj2Blocks.IsCountEqualTo(1) && IsNoDataFoundConditionCode(cj2Blocks.ElementAt(0).ConditionCode))
			{
				return GenerateNoDataFoundEmail(cj2Blocks.ElementAt(0));
			}

			var html = new StringBuilder();
			HtmlTableCreator table = null;
			MessageData messageData = null;
			foreach (MessageBlock block in Message.MessageBlock.MessageBlocks)
			{
				ACWQCJ2 cj2 = block as ACWQCJ2;

				if (cj2 != null)
				{
					if (messageData == null || messageData.EntryNumber != cj2.EntryNumber)
					{
						if (messageData != null && table != null)
						{
							AddEntryDataToEmail(cj2, messageData, html, table);
						}

						messageData = new MessageData() { EntryNumber = cj2.EntryNumber, EntryFiler = cj2.EntryFilerCode, PortCode = cj2.DistrictPortOfEntry, AcceptanceDate = cj2.ACEAcceptanceDate.ToString() };
						table = new HtmlTableCreator(new string[] { "Entry Line Number", "Tariff", "Census Warning Code", "Census Warning Description", "Condition Code", "Narrative Message" });
					}

					table.WriteRow(cj2.EntrySummaryLineItemIdentifier, cj2.HTSNumber, cj2.CensusWarningCode, GetCensusWarningDescription(cj2.CensusWarningCode), cj2.ConditionCode, cj2.NarrativeText);
					isFailure |= (!cj2.ConditionCode.IsEmpty || !cj2.NarrativeText.IsEmpty) && !IsNoDataFoundConditionCode(cj2.ConditionCode);
				}
			}

			if (messageData != null && table != null)
			{
				AddEntryDataToEmail(null, messageData, html, table);
			}

			return html.ToString();
		}

		void AddEntryDataToEmail(ACWQCJ2 cj2, MessageData messageData, StringBuilder html, HtmlTableCreator table)
		{
			var declarationReference = ZString.Empty;
			var declarationURL = ZString.Empty;

			var entry = new CusEntryHeader.Loader(Message.Factory).FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, messageData.EntryNumber, messageData.EntryFiler, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			if (entry != null)
			{
				declarationURL = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entry);
				declarationReference = entry.Declaration.JE_DeclarationReference;
			}

			var entryNumberWithFilerCode = CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(messageData.EntryFiler, messageData.EntryNumber);

			var linkToDeclaration = !declarationReference.IsEmpty ?
				"<a href=\"" + declarationURL + "\">" + declarationReference + "/" + entryNumberWithFilerCode + "</a>" :
				(string)entryNumberWithFilerCode;

			if (!messageData.PortCode.IsEmpty && !messageData.AcceptanceDate.IsEmpty)
			{
				html.Append("<b>" + linkToDeclaration + " - Entered into " + messageData.PortCode + " at " + messageData.AcceptanceDate + "</b>");
				html.Append("<br />");
				html.Append("<br />");
			}
			html.Append(table.ToHtml());
			html.Append("<br />");
		}

		string GenerateNoDataFoundEmail(ACWQCJ2 cj2)
		{
			var messageBody = "";
			if (!cj2.EntryNumber.IsEmpty)
			{
				messageBody = "<b>No unresolved census warnings found for Entry " + cj2.EntryFilerCode + "-" + cj2.EntryNumber + ".</b>";
			}
			else
			{
				var originalMessage = Message.OriginalMessage;
				if (originalMessage != null)
				{
					var cj1Block = originalMessage.MessageBlock.MessageBlocks.OfType<ACWQCJ1>().FirstOrDefault();
					if (cj1Block != null)
					{
						var dateRangeMessagePart = !cj1Block.RequestedFromDate.IsEmpty ? "Date From " + cj1Block.RequestedFromDate.ToString() + " to " + cj1Block.RequestedToDate.ToString() : "";

						var message = !cj1Block.DistrictPortOfEntry.IsEmpty ? "Entry Port " + cj1Block.DistrictPortOfEntry +
								(!string.IsNullOrEmpty(dateRangeMessagePart) ? " and " + dateRangeMessagePart : "") : dateRangeMessagePart;

						messageBody = "<b>No entries with census warning exist for the query. You have queried for " + message + ".</b>";
					}
				}
			}
			return messageBody;
		}

		bool IsNoDataFoundConditionCode(ZString conditionCode)
		{
			return conditionCode == "007" || conditionCode == "016";
		}

		ZString GetCensusWarningDescription(string censusWarningCode)
		{
			return CensusWarningCodeList.GetDescriptionFromCode(censusWarningCode);
		}

		CensusWarningCodeList CensusWarningCodeList
		{
			get { return censusWarningCodeList ?? (censusWarningCodeList = new CensusWarningCodeList()); }
		}
		CensusWarningCodeList censusWarningCodeList;

		class MessageData
		{
			public ZString EntryNumber;
			public ZString EntryFiler;
			public ZString PortCode;
			public ZString AcceptanceDate;
		}
	}
}
