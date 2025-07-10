using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Web.Http.ExceptionHandling;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ErrorReporting.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost.Tests
{
	class WebApiServiceExceptionReporterTest : TestCaseWithFactory
	{
		public void TestLogAsyncShouldReportExceptions()
		{
			var exception = new Exception("Houston, We Have a Problem");
			Log(exception);
			AssertReported(exception);
		}

		public void TestLogAsyncShouldNotReportIISInternalCommunicationErrorWhenClientConnected()
		{
			CombineAssertions(() =>
			{
				NotReportedException(GetExceptionForHResult(unchecked((int)0x80070032)));
				NotReportedException(GetExceptionForHResult(unchecked((int)0x800704CD)));
			});
		}

		HttpException GetExceptionForHResult(int hresult)
		{
			var hrException = Marshal.GetExceptionForHR(hresult);
			return new HttpException(hrException.Message, hrException);
		}

		public void TestLogAsyncShouldNotReportIOExceptionClientDisconnectedError()
		{
			const int ERROR_OPERATION_ABORTED = unchecked((int)0x800703E3);
			var httpException = new HttpException("The client disconnected.", ERROR_OPERATION_ABORTED);
			var ioException = new IOException(httpException.Message, httpException);

			NotReportedException(ioException);
		}

		public void TestLogAsyncShouldNotReportHttpExceptionClientDisconnectedError()
		{
			var httpException = new HttpException("The client disconnected.");

			NotReportedException(httpException);
		}

		static readonly IEnumerable<Exception> InternalServerErrors = new[]
		{
			new HttpException(500, "An error occurred while communicating with the remote host. The error code is 0x80070057.", unchecked((int)0x80070057)),
			new HttpException(500, "An error occurred while communicating with the remote host. The error code is 0x80072746.", unchecked((int)0x80072746)),
			new HttpException(500, "An error occurred while communicating with the remote host. The error code is 0x800703E3.", unchecked((int)0x800703E3)),
			new HttpException(500, "An error occurred while communicating with the remote host. The error code is 0x800704CD.", unchecked((int)0x800704CD)),
			new HttpException(500, "An error occurred while communicating with the remote host. The error code is 0x800703E3.",
				new COMException("The I/O operation has been aborted because of either a thread exit or an application request. (Exception from HRESULT: 0x800703E3)",
				unchecked((int)0x800703E3))),
			new HttpException(500, "The remote host closed the connection. The error code is 0x800703E3.", unchecked((int)0x800703E3)),
			new HttpException(500, "The remote host closed the connection. The error code is 0x80070016.", unchecked((int)0x80070016)),
			new HttpException(500, "The remote host closed the connection. The error code is 0x80070006.", unchecked((int)0x80070006)),
			new HttpException(500, "The remote host closed the connection. The error code is 0x80070040.", unchecked((int)0x80070040)),
			new HttpException(500, "The database has been upgraded and the application must be restarted to get the new version.",
				new DatabaseUpgradedException()),
		};

		public IEnumerable<Exception> SqlNetworkExceptions => new[]
		{
			SqlExceptionBuilder.CreateSqlException(53, "A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible"),
			SqlExceptionBuilder.CreateSqlException(121, "A connection was successfully established with the server, but then an error occurred during the login process"),
		};

		public void TestInternalServerErrorsAreNotReported()
		{
			CombineAssertions(() =>
			{
				foreach (var internalServerError in InternalServerErrors)
				{
					NotReportedException(internalServerError);
				}
			});
		}

		public void TestAggregateInternalServerErrorsAreNotReported()
		{
			CombineAssertions(() =>
			{
				foreach (var internalServerError in InternalServerErrors)
				{
					NotReportedException(new AggregateException(internalServerError));
				}
			});
		}

		public void TestFlattenedAggregateInternalServerErrorsAreNotReported()
		{
			CombineAssertions(() =>
			{
				foreach (var internalServerError in InternalServerErrors)
				{
					NotReportedException(new AggregateException(new AggregateException(internalServerError)));
				}
			});
		}

		public void TestDatabaseUpgradeExceptionAreNotReported()
		{
			NotReportedException(new DatabaseUpgradedException());
		}

		public void TestSqlNetworkExceptionsAreNotReported()
		{
			CombineAssertions(() =>
			{
				foreach (var semaphore in SqlNetworkExceptions)
				{
					NotReportedException(new AggregateException(new AggregateException(semaphore)));
				}
			});
		}

		public void TestUrlParseExceptionAreNotReported()
		{
			NotReportedException(
				new ArgumentException("The key is invalid JQuery syntax because it is missing a closing bracket Parameter name: key"),
				ExceptionCatchBlocks.HttpControllerDispatcher);
		}

		void NotReportedException(Exception exception, ExceptionContextCatchBlock catchBlock = null)
		{
			Log(exception, catchBlock);
			AssertNotReported(exception);
		}

		void Log(Exception exception, ExceptionContextCatchBlock catchBlock = null)
		{
			var exceptionContextCatchBlock = catchBlock ?? new ExceptionContextCatchBlock("Exception context catch block", true, true);
			var exceptionContext = new ExceptionContext(exception, exceptionContextCatchBlock);
			var exceptionLoggerContext = new ExceptionLoggerContext(exceptionContext);

			var webApiServiceExceptionReporter = new WebApiServiceExceptionReporter();
			webApiServiceExceptionReporter.LogAsync(exceptionLoggerContext, new CancellationToken()).GetAwaiter().GetResult();
		}

		void AssertReported(Exception exception)
		{
			AssertEquals("Report count", 1, ExceptionReporter.Instance.TotalReportCount);

			var query = new ZQuery(StmErrorReportSchema.QER_ReportXml, SQLComparisonOperator.Contains, exception.Message);
			var report = Factory.LoadTop1<StmErrorReport>(query);
			AssertNotNull(report);
		}

		void AssertNotReported(Exception exception)
		{
			AssertEquals("Report count", 0, ExceptionReporter.Instance.TotalReportCount);

			var query = new ZQuery(StmErrorReportSchema.QER_ReportXml, SQLComparisonOperator.Contains, exception.Message);
			var report = Factory.LoadTop1<StmErrorReport>(query);
			AssertNull(exception.GetType().Name, report);
		}

		protected override void SetUp()
		{
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			Globals.IsUserInteractive = false;
			Globals.SetIsUnitTestingProductionFunctionality(true);
			Globals.IsWeb = true;
			base.SetUp();
		}

		protected override void TearDown()
		{
			ExceptionReporterTestListener.Instance.Clear();
			ExceptionReporter.Instance.TotalReportCount = 0;

			Globals.IsUserInteractive = true;
			Globals.SetIsUnitTestingProductionFunctionality(false);
			Globals.IsWeb = false;
			base.TearDown();
		}
	}
}
