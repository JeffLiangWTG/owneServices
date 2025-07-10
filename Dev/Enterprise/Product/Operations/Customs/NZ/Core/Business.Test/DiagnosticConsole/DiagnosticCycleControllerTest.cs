namespace Enterprise.Customs.NZ.Business.Testing
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.MailManager;
	using Enterprise.MailManager.Business;
	using Enterprise.Messaging.Business;

	class DiagnosticCycleControllerTest : TestCaseWithFactory
	{
		public void TestLoadReplyEdiInterchangeByIndex()
		{
			using (TestConnection.TrackExecutedCommands())
			{
				var message = Factory.New<NZCMessage>();

				testController = new DiagnosticCycleControllerForTesting(message.PK, TestNzBrokerageId, Factory);
				testController.CheckNextDiagnosticCycleStep(DiagnosticStatusList.Codes.WaitingForTestMessageResponse);

				var executedCommand = TestConnection.ExecutedCommands.First(c => c.Contains("SELECT  TOP 1") && c.Contains("FROM dbo.EDIInterchange"));

				CombineAssertions(@"EDIInterchange often has a large data count, we should make sure the query should be supported by appropriate indexes. See more details in CS00879576 And WI00346569.", () =>
				{
					AssertContains("For fetch NR_UX__EI_InterchangeNum_EI_From_EI_To", @"and EI_To = @", executedCommand, ignoreCase: true);
					AssertContains("For fetch NR_UX__EI_InterchangeNum_EI_From_EI_To", @"and EI_From = @", executedCommand, ignoreCase: true);
					AssertContains("For fetch NR_RX__EI_SystemCreateTimeUtc", @"and EI_SystemCreateTimeUtc > @", executedCommand, ignoreCase: true);
				});
			}
		}

		public void TestCheckNextDiagnosticCycleStep()
		{
			var testMessage = Factory.New<NZCMessage>();
			testController = new DiagnosticCycleControllerForTesting(testMessage.PK, TestNzBrokerageId, Factory);

			AssertEquals("Nonexistant step ignored", "!Ex", testController.CheckNextDiagnosticCycleStep("!Ex"));

			//
			// TestMessageAtQUEStatus => LookingForOutMailItem
			AssertStepTransition(
				DiagnosticStatusListNZ.Codes.TestMessageAtQUEStatus,
				DiagnosticStatusListNZ.Codes.TestMessageStatusInvalid,
				() => testMessage.EM_Status = "XXX",
				DiagnosticStatusListNZ.Codes.LookingForOutgoingItem,
				() =>
				{
					testMessage.EM_Status = EDIMessage.Status.Sent;
					var intx = Factory.New<EDIInterchange>();
					intx.EI_Status = EDIInterchange.Status.Queued;
					testMessage.EM_EI = intx.PK;
				}
			);

			//
			// LookingForOutMailItem => OutMailAtQUEStatus
			var outEmail = Factory.New<MailItem>();
			AssertStepTransition(
				DiagnosticStatusListNZ.Codes.LookingForOutgoingItem,
				DiagnosticStatusListNZ.Codes.OutInterchangeStatusInvalid,
				() => testMessage.Interchange.EI_Status = "XXX",
				DiagnosticStatusListNZ.Codes.OutgoingItemAtQUEStatus,
				() =>
				{
					testMessage.Interchange.EI_Status = EDIInterchange.Status.Sent;
					outEmail.MI_Subject = string.Format(
						"{0}={1},{2},{3}",
						EDIMessage.ApplicationCodes.NewZealandCustoms, EDIInterchange.InterchangePartyIDs.NZCustomsTestMailbox,
						testMessage.Interchange.EI_InterchangeNum, TestNzBrokerageId);
					outEmail.MI_Direction = MailDirection.Transmit;
				}
			);

			//
			// OutMailAtQUEStatus => WaitingForTestMessageResponse
			AssertEquals("OEQ -> WTR has no conditions, should pass straight on", DiagnosticStatusListNZ.Codes.WaitingForTestMessageResponse, testController.CheckNextDiagnosticCycleStep(DiagnosticStatusListNZ.Codes.OutgoingItemAtQUEStatus));

			//
			// WaitingForTestMessageResponse = > InResponseMailReceived
			var inIntx = Factory.New<EDIInterchange>();

			AssertStepTransition(
				DiagnosticStatusListNZ.Codes.WaitingForTestMessageResponse,
				null, null,
				DiagnosticStatusListNZ.Codes.InComingResponseReceived,
				() =>
				{
					testMessage.Interchange.EI_Status = EDIInterchange.Status.Sent;
					testMessage.EM_ApplicationReference = NZDiagnosticConsol.NzDiagnosticMark;

					inIntx.EI_ApplicationCode = "NZC";
					inIntx.EI_InterchangeType = "NZC";
					inIntx.EI_ReceiveTransmit = "RCV";
					inIntx.EI_InterchangeNum = testMessage.Interchange.EI_InterchangeNum;
					inIntx.EI_From = EDIInterchange.InterchangePartyIDs.NZCustomsTestMailbox;
					inIntx.EI_To = TestNzBrokerageId;
					inIntx.EI_SystemCreateTimeUtc = DateTime.Now;
					inIntx.EI_BodyText = NZDiagnosticConsol.NzDiagnosticMark;
				}
			);

			//
			// InResponseMailReceived => InInterchangeAtQUEStatus
			var inEdiMsg = Factory.New<EDIMessage>();
			AssertStepTransition(
				DiagnosticStatusListNZ.Codes.InComingResponseReceived,
				null, null,
				DiagnosticStatusListNZ.Codes.InInterchangeProcessed,
				() =>
				{
					inEdiMsg.EM_EI = inIntx.PK;
					inIntx.ContainedMessages.Add(inEdiMsg);
				}
			);

			//
			// InInterchangeAtQUEStatus => OKSuccess
			AssertStepTransition(
				DiagnosticStatusListNZ.Codes.InInterchangeProcessed,
				null, null,
				DiagnosticStatusListNZ.Codes.OKSuccess,
				() => inEdiMsg.EM_Status = EDIMessage.Status.Recognised
			);
		}

		void AssertStepTransition(string currentStatus,
			string expectedFailStatus, Action setFailScenario,
			string expectedNextStatus, Action nextStepAction)
		{
			AssertEquals("Before performing step", currentStatus, testController.CheckNextDiagnosticCycleStep(currentStatus));

			if (expectedFailStatus != null)
			{
				AssertWhenDiagnosticProcessFails(currentStatus, expectedFailStatus, setFailScenario);
			}

			nextStepAction();
			AssertEquals("After performing step", expectedNextStatus, testController.CheckNextDiagnosticCycleStep(currentStatus));
		}

		void AssertWhenDiagnosticProcessFails(string currentStatus, string expectedFailStatus, Action setFailScenario)
		{
			try
			{
				setFailScenario();
				testController.CheckNextDiagnosticCycleStep(currentStatus);
				Fail("Should throw exception");
			}
			catch (DiagnosticMessageException ex)
			{
				AssertEquals("Failed Status", expectedFailStatus, ex.FailStatus);
			}
		}

		const string TestNzBrokerageId = "123456789";
		DiagnosticCycleController testController;

		class DiagnosticCycleControllerForTesting : DiagnosticCycleController
		{
			public DiagnosticCycleControllerForTesting(ZGuid testEdiMessagePk, string companyNzBrokerageId, BusinessObjectFactory testFactory)
				: base(testEdiMessagePk, DateTime.Now.AddDays(-3), companyNzBrokerageId)
			{
				this.testFactory = testFactory;
			}

			protected override BusinessObjectFactory GetNewBusinessObjectFactory()
			{
				return testFactory;
			}

			readonly BusinessObjectFactory testFactory;
		}
	}
}
