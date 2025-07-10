using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class NumberFountainUniqueIndexFailureHandlingWithExceptionTest : TestCaseWithFactory
	{
		public void TestConsolInformationForErrorReport()
		{
			try
			{
				var consignRef = "C00001000";
				var consol = Factory.New<CommonConsolWithException>();
				consol.JK_UniqueConsignRef = consignRef;
				Factory.Save();

				var brokenHandler = consol.NewConsolNumberFountainUniqueIndexFailureHandlerWithException();
				brokenHandler.NotifyUserAndAttemptToResolve(new NotificationHandler(), brokenHandler.HandledUniqueIndexNames.Single());

				string reportMask =
					"Number Fountain adjust failed after an unique index violation\r\n" +
					"  Fountain                    : {0}\r\n";

				string expectedDeveloperErrorReportedStart = String.Format(reportMask, typeof(BrokenNumberFountain).FullName);
				AssertStartsWith("Error message should start with number fountain fail message", expectedDeveloperErrorReportedStart, ErrorReporter.LastMessageReported);

				reportMask =
					"Number Fountain Information on Consol\r\n" +
					"  Consol.consignRefHandler is null                                      : {0}\r\n" +
					"  Consol.consignRefHandler.NumberFountain is null                       : {1}\r\n" +
					"  Consol.consignRefHandler.NumberFountain.PeekPreliminaryFormatted      : {2}\r\n" +
					"  Consol.FountainUsedForGeneration.PeekPreliminaryFormatted             : {3}\r\n" +
					"  Consol.NumberFountainForUniqueConsignRef.PeekPreliminaryFormatted     : {4}\r\n";

				string expectedDeveloperErrorReportedContains = String.Format(reportMask, "True", "True", "Null", "Null", consignRef);
				AssertContains("Error message should contain consol information", expectedDeveloperErrorReportedContains, ErrorReporter.LastMessageReported);

				var expectedDeveloperErrorReportedEnd = "ConsignRefHandler Stack Trace:\nConsignRefHandler has not been set and has no recorded stack trace.";
				AssertEndsWith("Error message should end with consignRefHandler stack trace", expectedDeveloperErrorReportedEnd, ErrorReporter.LastMessageReported);
				AssertEquals("Failed to fix fountain conflict.", ErrorReporter.LastExceptionReported.Message);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
	}
}
