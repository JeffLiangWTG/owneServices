using System;
using System.Web.Services.Protocols;
using CargoWise.Common;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class DummyServiceTest : BaseServiceTestCase<DummyService>
	{
		protected override DummyService GetNewWebService() => new DummyService();

		#region TestHandleExceptionReportError

		public void TestHandleExceptionReportError_NoException()
		{
			TestHandleExceptionReportErrorCore(
				exceptionType: null,
				action: () => { },
				expectedErrorReport: null);
		}

		public void TestHandleExceptionReportError_ArgumentException()
		{
			TestHandleExceptionReportErrorCore(
				exceptionType: typeof(ArgumentException),
				action: () => throw new ArgumentException("Value does not fall within the expected range."),
				expectedErrorReport: "Value does not fall within the expected range.");
		}

		public void TestHandleExceptionReportError_InvalidOperationException()
		{
			TestHandleExceptionReportErrorCore(
				exceptionType: typeof(InvalidOperationException),
				action: () => throw new InvalidOperationException("You messed up."),
				expectedErrorReport: "You messed up.");
		}

		void TestHandleExceptionReportErrorCore(Type exceptionType, Action action, string expectedErrorReport)
		{
			var webService = GetNewWebService();
			var response = new WebServiceResponse();
			var isCatchException = string.IsNullOrEmpty(expectedErrorReport); // no need to catch if we're not expecting an error
			try
			{
				response = webService.DoSomething(action);
			}
			catch (SoapException)
			{
				isCatchException = true;
			}

			Assert(isCatchException);
			if (string.IsNullOrEmpty(expectedErrorReport))
			{
				AssertNull(ErrorReporter.LastExceptionReported);
			}
			else
			{
				AssertEquals(exceptionType, ErrorReporter.LastExceptionReported.GetType());
				AssertEquals(expectedErrorReport, ErrorReporter.LastExceptionReported.Message);
				AssertNotNull(ErrorReporter.LastExceptionReported.StackTrace);
			}

			ErrorReporter.Clear();
		}

		#endregion
	}

	#region DummyService

	public class DummyService : BaseService
	{
		public WebServiceResponse DoSomething(Action action)
		{
			var result = new WebServiceResponse();
			try
			{
				action();
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
			return result;
		}
	}

	#endregion
}
