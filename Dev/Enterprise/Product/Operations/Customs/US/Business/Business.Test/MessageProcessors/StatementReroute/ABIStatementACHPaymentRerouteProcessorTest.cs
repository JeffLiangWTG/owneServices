using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ABIStatementACHPaymentRerouteProcessorTest : RerouteProcessorTest<DSTQR, DSTQX, ABIStatementACHPaymentRerouteProcessor>
	{
		protected override string expectedBody
		{
			get
			{
				return @"<strong>Reroute of statement and/or ACH payment response</strong><br />
<br />
A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
Requested Data:<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Transmission Date</th><th>Importer No.</th><th>Client Branch</th><th>Statement No.</th><th>All</th><th>Preliminary</th><th>Final</th><th>ACH Payment</th><th>Periodic Payment</th></tr></thead><tr><td>07-Jul-06</td><td>IMP123456</td><td>CB</td><td>ST12345678</td><td>Y</td><td>Y</td><td>N</td><td>Y</td><td>N</td></tr></table><br />Response Data:<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error/Notification</th><th>Message</th><th>Total No. Of Reroutes</th></tr></thead><tr><td>QO1 - THE TRANSMIT DATE OF THE &#39;QR&#39; RECORD IS MORE THAN 14 DAYS PAST THE CURRENT SYSTEM DATE.</td><td>DATE NO GOOD</td><td>2</td></tr><tr><td>&nbsp;</td><td>GOOD JOB MATE.</td><td>6</td></tr><tr><td>QO4 - &#39;QR&#39; RECORD DID NOT CONTAIN A REROUTE REQUEST. EXPECT ONE OF THREE INDICATORS = Y.</td><td>WHERE IS MY REQUEST?</td><td>3</td></tr></table>
<br />";
			}
		}

		protected override string transmitDateBeyond14Days
		{
			get { return "QO1"; }
		}

		protected override string noReroutesRequested
		{
			get { return "QO4"; }
		}

		protected override string rerouteSubject
		{
			get { return ABIStatementACHPaymentRerouteProcessor.ABIStatementACHPaymentRerouteSubject; }
		}

		protected override GroupNotificationRegistryItem<GroupNotification> StatementRerouteGroup
		{
			get { return USCustomsDataRegistry.Instance.DailyStatementsMessagesGroup; }
		}

		protected override string applicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentRerouteResponse; }
		}

		protected override bool IsPeriodicMonthly
		{
			get { return false; }
		}

		protected override DSTQR CreateQRBlock()
		{
			return new DSTQR()
			{
				TransmissionDateOfStatementOrACHPaymentTransaction = new ZDate(2006, 7, 7),
				ImporterOfRecordNumber = "IMP123456",
				ClientBranch = "CB",
				StatementNumber = "ST12345678",
				ScopeIndicator = "A",
				ACHPaymentRequest = "Y",
				PeriodicStatementPaymentAuthorizationRequest = "N",
				PreliminaryStatementRequest = "Y",
				FinalStatementRequest = "N"
			};
		}
	}
}
