using System;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class StatementRerouteProcessorTest : RerouteProcessorTest<PMSQR, QXCommon, StatementRerouteProcessor>
	{
		public void TestACEDaily()
		{
			USCustomsDataRegistry.Instance.DailyStatementsMessagesGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new GroupNotification(GroupNotification.StaffMemberOrNominatedGroup, groupZZ1.PK));

			var generator = new ABIOutputBlockControlGenerator();
			generator.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.StatementRequestRerouteResponse;

			var qrBlock = new PMSQR() { TransmissionDateOfStatement = new ZDate(2015, 08, 04), ImporterOfRecordNumber = "IMP123456", ClientBranch = "A2", ScopeIndicator = "A", PreliminaryStatementRequest = "Y", PreliminaryPeriodicMonthlyStatementRequest = "N", FinalStatementRequest = "N", FinalPeriodicMonthlyStatementRequest = "N" };
			var qxBlock = new APMSQX() { SeverityCode = "I", ConditionCode = "MS9", NarrativeText = "ASK SOMETHING ELSE", TotalNumberOfReroutes = 0 };
			generator.AddMessageBlock(qrBlock);
			generator.AddMessageBlock(qxBlock);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ProcessMessage(generator);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == StatementRerouteProcessor.DailyStatementRerouteSubject; }));

			Assert(email.Body.Contains(@"<strong>Reroute of Daily Statement response</strong><br />
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
Requested Data:<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Transmission Date</th><th>Importer No.</th><th>Client Branch</th><th>Statement No.</th><th>All</th><th>Preliminary</th><th>Final</th></tr></thead><tr><td>04-Aug-15</td><td>IMP123456</td><td>A2</td><td>&nbsp;</td><td>Y</td><td>Y</td><td>N</td></tr></table><br />Response Data:<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error/Notification</th><th>Message</th><th>Total No. Of Reroutes</th></tr></thead><tr><td>MS9 - NO STATEMENT FOUND TO REROUTE</td><td>ASK SOMETHING ELSE</td><td>0</td></tr></table>"));
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));
		}

		public void TestQXTotalNumberOfReroutes()
		{
			var qxOldRaw = "QX IMSA   AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA 000100                       ";
			var qxBlock = new APMSQX();
			qxBlock.Deserialise(qxOldRaw);
			var qxBlockStatementRerouteResponse = qxBlock as IStatementRerouteResponse;
			AssertEquals(100, qxBlockStatementRerouteResponse.TotalNumberOfReroutes);

			qxOldRaw = "QX IMSA   AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA 900100                       ";
			qxBlock.Deserialise(qxOldRaw);
			qxBlockStatementRerouteResponse = qxBlock;
			AssertEquals(100, qxBlockStatementRerouteResponse.TotalNumberOfReroutes); //number at pos 52 is ignored so 900100 becomes 00100

			var qxNewRaw = "QX IMSA   AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA  000100                      ";
			qxBlock.Deserialise(qxNewRaw);
			qxBlockStatementRerouteResponse = qxBlock;
			AssertEquals(100, qxBlockStatementRerouteResponse.TotalNumberOfReroutes);

			qxNewRaw = "QX IMSA   AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA  900100                      ";
			qxBlock.Deserialise(qxNewRaw);
			qxBlockStatementRerouteResponse = qxBlock;
			AssertEquals(900100, qxBlockStatementRerouteResponse.TotalNumberOfReroutes);
		}

		public void TestACEPeriodicMonthly()
		{
			USCustomsDataRegistry.Instance.PeriodicMonthlyStatementsMessagesGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new GroupNotification(GroupNotification.StaffMemberOrNominatedGroup, groupZZ1.PK));

			var generator = new ABIOutputBlockControlGenerator();
			generator.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.StatementRequestRerouteResponse;

			var qrBlock = new PMSQR() { TransmissionDateOfStatement = new ZDate(2015, 08, 04), ImporterOfRecordNumber = "IMP123456", ClientBranch = "A2", ScopeIndicator = "A", PreliminaryStatementRequest = "N", PreliminaryPeriodicMonthlyStatementRequest = "Y", FinalStatementRequest = "N", FinalPeriodicMonthlyStatementRequest = "N" };
			var qxBlock = new APMSQX() { SeverityCode = "I", ConditionCode = "MS9", NarrativeText = "ASK SOMETHING ELSE", TotalNumberOfReroutes = 0 };
			generator.AddMessageBlock(qrBlock);
			generator.AddMessageBlock(qxBlock);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ProcessMessage(generator);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == StatementRerouteProcessor.PeriodicMonthlyStatementRerouteSubject; }));

			Assert(email.Body.Contains(@"<strong>Reroute of Periodic Monthly Statement response</strong><br />
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
Requested Data:<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Transmission Date</th><th>Importer No.</th><th>Client Branch</th><th>Statement No.</th><th>All</th><th>Preliminary</th><th>Final</th></tr></thead><tr><td>04-Aug-15</td><td>IMP123456</td><td>A2</td><td>&nbsp;</td><td>Y</td><td>N</td><td>N</td></tr></table><br />Response Data:<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error/Notification</th><th>Message</th><th>Total No. Of Reroutes</th></tr></thead><tr><td>MS9 - NO STATEMENT FOUND TO REROUTE</td><td>ASK SOMETHING ELSE</td><td>0</td></tr></table>"));
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));
		}

		protected override string expectedBody
		{
			get
			{
				return @"<strong>Reroute of Periodic Monthly Statement response</strong><br />
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
Requested Data:<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Transmission Date</th><th>Importer No.</th><th>Client Branch</th><th>Statement No.</th><th>All</th><th>Preliminary</th><th>Final</th></tr></thead><tr><td>07-Jul-06</td><td>IMP123456</td><td>CB</td><td>ST12345678</td><td>Y</td><td>Y</td><td>N</td></tr></table><br />Response Data:<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error/Notification</th><th>Message</th><th>Total No. Of Reroutes</th></tr></thead><tr><td>MS1 - INPUT REROUTE REQUEST MISSING</td><td>DATE NO GOOD</td><td>2</td></tr><tr><td>&nbsp;</td><td>GOOD JOB MATE.</td><td>6</td></tr><tr><td>MS4 - IMPORTER OF RECORD UNKNOWN</td><td>WHERE IS MY REQUEST?</td><td>3</td></tr></table>";
			}
		}

		protected override string transmitDateBeyond14Days
		{
			get { return "MS1"; }
		}

		protected override string noReroutesRequested
		{
			get { return "MS4"; }
		}

		protected override string rerouteSubject
		{
			get { return StatementRerouteProcessor.PeriodicMonthlyStatementRerouteSubject; }
		}

		protected override GroupNotificationRegistryItem<GroupNotification> StatementRerouteGroup
		{
			get { return USCustomsDataRegistry.Instance.PeriodicMonthlyStatementsMessagesGroup; }
		}

		protected override string applicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatementRerouteResponse; }
		}

		protected override bool IsPeriodicMonthly
		{
			get { return true; }
		}

		protected override PMSQR CreateQRBlock()
		{
			return new PMSQR()
			{
				TransmissionDateOfStatement = new ZDate(2006, 7, 7),
				ImporterOfRecordNumber = "IMP123456",
				ClientBranch = "CB",
				StatementNumber = "ST12345678",
				ScopeIndicator = "A",
				PreliminaryPeriodicMonthlyStatementRequest = "Y",
				FinalPeriodicMonthlyStatementRequest = "N"
			};
		}
	}
}
