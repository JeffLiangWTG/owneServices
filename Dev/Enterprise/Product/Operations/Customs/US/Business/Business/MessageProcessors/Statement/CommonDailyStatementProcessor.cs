using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageProcessors.Statement;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class CommonDailyStatementProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : StatementProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IABIControlMessageBlockA, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IABIControlMessageBlockB, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			return USCustomsDataRegistry.Instance.DailyStatementsMessagesGroup;
		}

		protected override string StatementReportDescription
		{
			get { return dailyStatementReportDescription; }
		}
		internal const string dailyStatementReportDescription = "The reports attached are generated as part of the U.S. Customs Border and";

		protected override string StatementReportBodyDetails
		{
			get { return dailyStatementReportBodyDetails; }
		}
		internal const string dailyStatementReportBodyDetails = "Protection (CBP) end-of-day processing.";

		protected override string StatementReportFooterDetails
		{
			get { return dailyStatementReportFooterDetails; }
		}
		internal const string dailyStatementReportFooterDetails = "Please review them and take appropriate action.";

		protected bool IsFinal
		{
			get { return B.StatementStatus == "F"; }
		}
		public override void Process()
		{
			base.Process();

			StringBuilder htmlBody = new StringBuilder();

			var branch = Message.Branch ?? Message.Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var cusStatementHeader = Message.EM_LinkedObject as CusStatementHeader;
			if (cusStatementHeader == null)
			{
				cusStatementHeader = new CusStatementHeader.Loader(Factory).LoadWithStatementNumber(B.EntryFilerCode, B.StatementNumber, branch.GB_GC);
				if (cusStatementHeader == null)
				{
					cusStatementHeader = Factory.New<CusStatementHeader>();
				}
				Message.EM_LinkedObject = cusStatementHeader;
			}

			if (Message.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.DailyStatement)
			{
				cusStatementHeader.B2_StatementType = StatementTypeList.Codes.ACE;
			}
			else
			{
				cusStatementHeader.B2_StatementType = StatementTypeList.Codes.QR;
			}

			FillCusStatementHeaderPropertiesFromAOrBBlock(cusStatementHeader);

			cusStatementHeader.B2_DueDate = B.PreliminaryStatementPrintDate;
			CusStatementLine cusStatementLine = null;

			var jobNumber = cusStatementHeader.B2_StatementNumber;

			htmlBody.Append(StatementReportBodyDetails);
			htmlBody.Append("<br />");
			htmlBody.Append("BUREAU OF CUSTOMS AND BORDER PROTECTION");
			htmlBody.Append("<br />");

			var listOfDeclarationJobsRequiredAII = new Dictionary<ZString, HtmlTableCreator>();

			foreach (MessageBlock block in messageBlocks)
			{
				cusStatementLine = ProcessMessageBlockCore(cusStatementHeader, cusStatementLine, block, listOfDeclarationJobsRequiredAII);
			}

			SetImporterCustomsIDAndImporterOfRecord(cusStatementHeader);

			CalculateTotalAmountForStatementLine(cusStatementLine);

			if (eIRequiredEntriesTableCreator != null)
			{
				htmlBody.Append("<br />");
				htmlBody.Append(@"<b><font color=""red"">" + Constants.RequestFullAII + "</font></b><br>");
			}

			htmlBody.Append("<br />");
			var tableCreator = new HtmlTableCreator(new string[] { "Heading", "Details" });
			tableCreator.WriteRow("STATEMENT PRINT DATE", B.PreliminaryStatementPrintDate);
			tableCreator.WriteRow("STATEMENT NUMBER", B.StatementNumber);
			tableCreator.WriteRow("ENTRY FILER CODE", B.EntryFilerCode);
			tableCreator.WriteRow("DISTRICT PORT CODE", B.ProcessingDistrictPortCode);
			htmlBody.Append(tableCreator.ToHtml());
			if (eIRequiredEntriesTableCreator != null)
			{
				htmlBody.Append("<br />");
				htmlBody.Append("Entries, which required Electronic Invoice, listed below:");
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				htmlBody.Append(eIRequiredEntriesTableCreator.ToHtml());
			}

			string uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(cusStatementHeader);
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Daily Statement", htmlBody.ToString(), false, branch, cusStatementHeader);

			GenerateAIIRequestedEmailForBrokers(cusStatementHeader, listOfDeclarationJobsRequiredAII);
		}

		protected abstract CusStatementLine ProcessMessageBlockCore(CusStatementHeader statementHeader, CusStatementLine statementLine, MessageBlock block, Dictionary<ZString, HtmlTableCreator> listOfDeclarationJobsRequiredAII);

		protected void SetCensusWarningIndicator(string cWIndicatorForMessage, CusStatementLine line)
		{
			if (cWIndicatorForMessage == StatementCensusWarningList.Codes.Census)
			{
				line.B3_EntryStatus = StatementEntryStatus.Codes.Census;
			}
			else if (cWIndicatorForMessage == StatementCensusWarningList.Codes.CensusRemoved &&
				line.B3_EntryStatus == StatementEntryStatus.Codes.Census)
			{
				line.B3_EntryStatus = ZString.Empty;
			}
		}

		protected void ProcessDeletedEntry(CusStatementHeader statementHeader, ZString entryNumber, ZString deletedSource)
		{
			CusStatementLine cusStatementLine = statementHeader.StatementLines.GetStatementLineFor(statementHeader.B2_EntryFilerCode, entryNumber);
			if (cusStatementLine != null)
			{
				cusStatementLine.B3_DeletedByParty = deletedSource;
				cusStatementLine.B3_Status = StatementLineStatusList.Codes.Deleted;
			}
		}

		protected void CalculateTotalAmountForStatementLine(CusStatementLine cusStatementLine)
		{
			if (cusStatementLine != null)
			{
				cusStatementLine.B3_CustomsFeesTotal = cusStatementLine.Charges.GetTotalPayableAmount();
			}
		}

		void GenerateAIIRequestedEmailForBrokers(CusStatementHeader cusStatementHeader, Dictionary<ZString, HtmlTableCreator> noticeBrokerSendEIemailBodyCollection)
		{
			foreach (var htmlTableCreator in noticeBrokerSendEIemailBodyCollection)
			{
				var emailAddress = htmlTableCreator.Key;
				if (!emailAddress.IsEmpty)
				{
					var htmlBody = new StringBuilder();
					htmlBody.Append(ZString.Format(EmailHeader, ObjectFactory.Get<IShowEditFormUrlCreator>().Create(cusStatementHeader), cusStatementHeader.B2_StatementNumber));
					htmlBody.Append("<br />");
					htmlBody.Append("<br />");
					htmlBody.Append(htmlTableCreator.Value.ToHtml());
					htmlBody.Append("<br />");
					var email = new HtmlEmailDef { Subject = ZString.Format(EmailSubject, cusStatementHeader.B2_StatementNumber) };
					email.LoadHtmlUsingTemplate(htmlBody.ToString());
					email.AddRecipientForSystemCommunication(emailAddress);
					Env.OutgoingCustomsMailManager.Create(Factory, email);
				}
			}
		}

		protected HtmlTableCreator eIRequiredEntriesTableCreator;

		const string EmailHeader = "A statement [<a href=\"{0}\">{1}</a>] has been received and Customs has requested electronic invoice for the following declarations you are in charge of and these declarations have at least one commercial invoice for which electronic invoice has not been sent.";
		const string EmailSubject = "Electronic Invoice notification received in Daily Statement '{0}'";
	}
}
