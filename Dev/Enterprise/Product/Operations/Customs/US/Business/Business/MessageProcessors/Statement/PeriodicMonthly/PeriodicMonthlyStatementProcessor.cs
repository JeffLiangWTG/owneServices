using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageProcessors.Statement;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public class PeriodicMonthlyStatementProcessor : StatementProcessor<APLA, APLB, APLY>
	{
		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			return USCustomsDataRegistry.Instance.PeriodicMonthlyStatementsMessagesGroup;
		}

		internal const string PeriodicMonthlyStatementReportSubject = "Periodic Monthly Statement Reports";
		protected override string StatementReportSubject
		{
			get { return PeriodicMonthlyStatementReportSubject; }
		}

		internal const string PeriodicMonthlyStatementReportDescription = "The reports attached are generated as part of the U.S. Customs Border and";
		protected override string StatementReportDescription
		{
			get { return PeriodicMonthlyStatementReportDescription; }
		}

		internal const string PeriodicMonthlyStatementReportBodyDetails = "Protection (CBP) end-of-day processing.";
		protected override string StatementReportBodyDetails
		{
			get { return PeriodicMonthlyStatementReportBodyDetails; }
		}

		internal const string PeriodicMonthlyStatementReportFooterDetails = "Please review them and take appropriate action.";
		protected override string StatementReportFooterDetails
		{
			get { return PeriodicMonthlyStatementReportFooterDetails; }
		}

		public override void Process()
		{
			base.Process();

			var loader = new CusStatementHeader.Loader(Factory);
			var branch = Message.Branch ?? Message.Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var cusStatementHeader = Message.EM_LinkedObject as CusStatementHeader;
			if (cusStatementHeader == null)
			{
				cusStatementHeader = loader.LoadWithStatementNumber(B.EntryFilerCode, B.StatementNumber, branch.GB_GC);
				if (cusStatementHeader == null)
				{
					cusStatementHeader = Factory.New<CusStatementHeader>();
				}
				else
				{
					cusStatementHeader.B2_StatementAmount = ZDecimal.Zero;
				}
				Message.EM_LinkedObject = cusStatementHeader;
			}
			else
			{
				cusStatementHeader.B2_StatementAmount = ZDecimal.Zero;
			}
			cusStatementHeader.B2_IsMonthlyStatement = true;
			cusStatementHeader.B2_StatementType = StatementTypeList.Codes.ACE;

			FillCusStatementHeaderPropertiesFromAOrBBlock(cusStatementHeader);

			var jobNumber = cusStatementHeader.B2_StatementNumber;

			var htmlBody = new StringBuilder();
			htmlBody.Append(StatementReportBodyDetails);
			htmlBody.Append("<br />");
			htmlBody.Append("                 BUREAU OF CUSTOMS AND BORDER PROTECTION			");
			htmlBody.Append("<br />");
			htmlBody.Append("					PERIODIC MONTHLY STATEMENT REPORT				");
			htmlBody.Append("<br />");

			foreach (var block in messageBlocks)
			{
				if (block is PMSQ1)
				{
					var pmsq1 = (PMSQ1)block;

					if (branch != null)
					{
						var query = loader.CreateNewCusStatementHeaderFilter(pmsq1.PeriodicDailyStatementFilerCode, pmsq1.PeriodicDailyStatementNumber, branch.GB_GC);
						var dailyStatements = Factory.Load<CusStatementHeader>(query);
						foreach (var daily in dailyStatements)
						{
							daily.B2_B2_PeriodicStatement = cusStatementHeader.PK;
						}
					}
				}
				else if (block is PMSQ2)
				{
					var pmsq2 = (PMSQ2)block;

					cusStatementHeader.B2_StatementAmount += pmsq2.TotalAmountDue;
				}
				else if (block is PMSQ3)
				{
					var pmsq3 = (PMSQ3)block;

					cusStatementHeader.B2_DueDate = pmsq3.PeriodicMonthlyStatementDueDate;
				}
			}

			SetImporterCustomsIDAndImporterOfRecord(cusStatementHeader);

			if (Message.EM_MessageOwner != Reprocessing)
			{
				cusStatementHeader.B2_PaymentParty = cusStatementHeader.DailyStatements.GetUniquePaymentParty();
				cusStatementHeader.B2_AccountNo = cusStatementHeader.DailyStatements.GetUniqueAccountNo();

				htmlBody.Append("STATEMENT NUMBER:			 " + B.StatementNumber);
				htmlBody.Append("<br />");
				htmlBody.Append("STATEMENT PRINT DATE:		 " + B.PreliminaryStatementPrintDate);
				htmlBody.Append("<br />");
				htmlBody.Append("ENTRY FILER CODE:			 " + B.EntryFilerCode);
				htmlBody.Append("<br />");
				htmlBody.Append("DISTRICT PORT CODE:		 " + B.ProcessingDistrictPortCode.ToString());
				htmlBody.Append("<br />");

				var uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(cusStatementHeader);
				var branchForLogo = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
				GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Periodic Monthly Statement", htmlBody.ToString(), false, branchForLogo, cusStatementHeader);
			}
			else
			{
				Message.EM_MessageOwner = ZString.Empty;
			}
		}
	}
}
