using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SmsSenderTest : SmsSenderTestCase<DummySmsSender>
	{
		#region TestNew

		public void TestNew()
		{
			DummySmsSender.RegisterThisSubTypeOverride();

			try
			{
				AssertEquals(true, SmsSender.New() is DummySmsSender);
			}
			finally
			{
				DummySmsSender.UnregisterThisSubTypeOverride();
			}
		}

		#endregion

		#region TestIsSmsSupported

		public void TestIsSmsSupported()
		{
			DummySmsSender.RegisterThisSubTypeOverride();

			try
			{
				AssertEquals(true, SmsSender.IsSmsSupported);
			}
			finally
			{
				DummySmsSender.UnregisterThisSubTypeOverride();
			}

			ResetIsSmsSupported();
			AssertEquals(false, SmsSender.IsSmsSupported);
		}

		#endregion

		#region TestFailedSendErrorMsg

		public void TestFailedSendErrorMsg()
		{
			ZString errorMsg =
				"Unable to send SMS, the following errors were found:\r\n" +
				"\r\n" +
				"Error - PhoneNumberCount: SMS requires at least one phone number.\r\n" +
				"Error - Message: Please enter a value.\r\n" +
				"Error - No User Name is set in the SMS Configuration registry.\r\n" +
				"Error - No Password is set in the SMS Configuration registry.";

			SmsSendResult sendResult1 = Sender.SendImmediately(SMS);
			AssertEquals(errorMsg, sendResult1.Message);
			AssertEquals(false, sendResult1.Success);

			SmsSendResult sendResult2 = Sender.Send(SMS);
			AssertEquals(errorMsg, sendResult2.Message);
			AssertEquals(false, sendResult2.Success);
		}

		#endregion

		#region TestSend

		public void TestSend()
		{
			SMS.FillWithValidTestData();

			AssertEquals("Precondition - ensure we have a phone number.", 1, SMS.PhoneNumbers.Length);
			AssertEquals("Precondition - ensure we have a message.", false, SMS.Message.IsEmpty);

			Sender.Send(SMS);
			ZQuery query = new ZQuery(StmPrintJobSchema.SP_FaxDestination, SMS.PhoneNumbers[0]);
			BusinessObject job = (BusinessObject)Sender.LastSavedFactoryForTesting.LoadTop1<Enterprise.Integration.DocumentEngine.IStmPrintJob>(query);

			AssertEquals("Send() should save the StmPrintJob in the DB.", true, job.IsInDatabase);
			AssertEquals(SMS.PhoneNumbers[0], job[StmPrintJobSchema.SP_FaxDestination]);
			AssertEquals(SMS.Message, new ZBlob(job[StmPrintJobSchema.SP_CustomProperties]).ToAscii());
		}

		#endregion

		#region TestSendOnFactorySave

		public void TestSendOnFactorySave()
		{
			SMS.FillWithValidTestData();

			AssertEquals("Precondition - ensure we have a phone number.", 1, SMS.PhoneNumbers.Length);
			AssertEquals("Precondition - ensure we have a message.", false, SMS.Message.IsEmpty);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			Sender.SendOnFactorySave(SMS, factory);
			ZQuery query = new ZQuery(StmPrintJobSchema.SP_FaxDestination, SMS.PhoneNumbers[0]);
			BusinessObject job = (BusinessObject)factory.LoadTop1<Enterprise.Integration.DocumentEngine.IStmPrintJob>(query);

			AssertEquals("SendOnFactorySave() should not save the StmPrintJob in the DB. ", false, job.IsInDatabase);
			AssertEquals(SMS.PhoneNumbers[0], job[StmPrintJobSchema.SP_FaxDestination]);
			AssertEquals(SMS.Message, new ZBlob(job[StmPrintJobSchema.SP_CustomProperties]).ToAscii());
		}

		#endregion

		#region TestSendImmediately

		public void TestSendImmediately()
		{
			var senderMock = new Mock<DummySmsSender>();
			senderMock.CallBase = true;
			AssertEquals("Precondition - validation not yet run on the SMS object.", false, SMS.HasErrors);
			senderMock.Object.SendImmediately(SMS);
			AssertEquals("Send should have validated the Sms object and found errors.", true, SMS.HasErrors);
			senderMock.Protected().Verify("SendCore", Times.Never(), ItExpr.IsAny<Sms>());
			senderMock.VerifyAll(); // sms has errors, sms.SendCore() should not be called
			senderMock.Reset();

			SMS.FillWithValidTestData();
			senderMock.Protected().Setup<SmsSendResult>("SendCore", ItExpr.IsAny<Sms>())
				.Returns(new SmsSendResult());
			senderMock.Object.SendImmediately(SMS);
			AssertEquals(false, SMS.HasErrors);
			senderMock.VerifyAll(); // sms has no errors, sms.SendCore() should be called
		}

		#endregion
	}
}
