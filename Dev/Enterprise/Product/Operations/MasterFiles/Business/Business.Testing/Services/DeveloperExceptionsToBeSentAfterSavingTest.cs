using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DeveloperExceptionsToBeSentAfterSavingTest : TestCaseWithFactory
	{
		public void TestQueueReport()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_AH = transaction.PK;
			var transactionLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_AH = transaction.PK;
			Factory.Save();

			transactionLine.AL_Desc += "line needs to have changes to save and report";
			DeveloperExceptionsToBeSentAfterSavingService.QueueReport(transactionLine, "key", "message", new ExceptionForTesting("message"));
			AssertEquals("should not report before saving", 0, ErrorReporter.TotalErrorCount);
			Factory.Save();
			AssertEquals("should be reported when saving", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("message", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			transactionLine.AL_Desc += "line needs to have changes to save and report";
			Factory.Save();
			AssertEquals("should not report again once the queued exception is reported", 0, ErrorReporter.TotalErrorCount);

			DeveloperExceptionsToBeSentAfterSavingService.QueueReport(transactionLine, "key", "message", new ExceptionForTesting("message"));
			Factory.Save();
			AssertEquals("should not report if object is not going to be saved", 0, ErrorReporter.TotalErrorCount);
			transactionLine.AL_Desc += "line needs to have changes to save and report";
			Factory.Save();
			AssertEquals("should be reported once object is saved", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			transactionLine.AL_Desc += "line needs to have changes to save and report";
			transactionLine2.AL_Desc += "line needs to have changes to save and report";
			DeveloperExceptionsToBeSentAfterSavingService.QueueReport(transactionLine, "keyForLine", "message", new ExceptionForTesting("message"));
			DeveloperExceptionsToBeSentAfterSavingService.QueueReport(transactionLine2, "keyForLine2", "message", new ExceptionForTesting("message"));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			Factory.Save();
			AssertEquals("Should report all that are queued", 2, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			transactionLine.AL_Desc += "line needs to have changes to save and report";
			DeveloperExceptionsToBeSentAfterSavingService.QueueReport(transactionLine, "key", "message", new ExceptionForTesting("message"));
			DeveloperExceptionsToBeSentAfterSavingService.QueueReport(transactionLine, "key", "message", new ExceptionForTesting("message"));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			Factory.Save();
			AssertEquals("Should only report once for each object", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			Factory.RefreshEnabled = false;
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var lineInNewFactory = newFactory.Load<AccTransactionLines>(transactionLine.PK);
			transactionLine.AL_Desc += "line needs to have changes to save and report";
			lineInNewFactory.AL_Desc += "line needs to have changes to save and report and needs concurrency error";
			newFactory.Save();

			DeveloperExceptionsToBeSentAfterSavingService.QueueReport(transactionLine, "key", "message", new ExceptionForTesting("message"));
			try
			{
				Factory.Save();
				Fail("Should fail concurrency check");
			}
			catch (ZSaveConcurrencyException ex)
			{
				AssertContains("Save Aborted Due to Concurrency Check", ex.Message);
			}
			AssertEquals("Should not report if saving failed", 0, ErrorReporter.TotalErrorCount);
		}

		[Serializable]
		public class ExceptionForTesting : Exception
		{
			public ExceptionForTesting(string message) : base(message)
			{
			}

#if NETFRAMEWORK
			protected ExceptionForTesting(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif
		}
	}
}
