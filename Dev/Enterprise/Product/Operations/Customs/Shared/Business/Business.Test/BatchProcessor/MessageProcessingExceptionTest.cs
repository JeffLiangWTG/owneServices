using System.Collections;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	sealed class MessageProcessingExceptionTest : TestCaseWithFactory
	{
		public void TestHandleExceptionForExecuteForEmailOption()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_EmailAddress = "test@edi.com.au";

			GlbGroup group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			GlbGroupLink link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			Factory.Save();

			TestRetriever retriever = new TestRetriever("hehe", true, false);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Current count", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			retriever.Execute();
			AssertEquals("One email should have been sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestHandleExceptionForExecuteForDeveloperInfo()
		{
			TestRetriever retriever = new TestRetriever("hehe", false, true);
			ErrorReporter.Clear();
			try
			{
				retriever.Execute();
				AssertEquals("Key", "hehe", ErrorReporter.LastKeyReported.Trim());
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		class TestRetriever : BaseInterchangeRetriever
		{
			public TestRetriever(string exceptionMessage, bool shouldSendEmail, bool shouldSendDeveloperInfo, CancellationToken token = new CancellationToken())
			{
				this.exceptionMessage = exceptionMessage;
				this.shouldSendEmail = shouldSendEmail;
				this.shouldSendDeveloperInfo = shouldSendDeveloperInfo;
				retrievedInterchanges = new ArrayList();
			}

			readonly string exceptionMessage;
			readonly bool shouldSendEmail;
			readonly bool shouldSendDeveloperInfo;

			protected override void ClearProcessedInterchanges()
			{
			}

			protected override void RetrieveInterchanges(int numberToRetrieve, CancellationToken token = new CancellationToken())
			{
				throw new MessageProcessingException(exceptionMessage, "", shouldSendEmail, shouldSendDeveloperInfo);
			}

			new internal void Execute() => base.Execute();
		}
	}
}
