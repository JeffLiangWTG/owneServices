using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ErrorReporting.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class WebApiServiceConfigTest : TestCaseWithFactory
	{
		public void TestRegisterTest()
		{
			var config = new HttpConfiguration();
			WebApiServiceConfig.Register(config);

			var exceptionLoggerServices = config.Services.GetServices(typeof(IExceptionLogger));
			AssertEquals(1, exceptionLoggerServices.Count());
			AssertType<WebApiServiceExceptionReporter>(exceptionLoggerServices.First());

			AssertEquals(config.MessageHandlers.OfType<InstanceIdHandler>().Count(), 1);
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Admin privilege is required to run the web server.")]
		public void TestExceptionLoggerServices_IsCalled()
		{
			try
			{
				var exception = new Exception("Houston, We Have a Problem");

				var service = new WebApiServiceForTest();
				using (service.Run())
				{
					var result = new HttpClient().GetAsync(service.Uri + "WebApiServiceConfigTest/TriggerError").GetAwaiter().GetResult();
					if (result.StatusCode != HttpStatusCode.InternalServerError)
					{
						Assert(result.Content.ReadAsStringAsync().GetAwaiter().GetResult(), false);
					}
				}

				AssertEquals("Report count", 1, ExceptionReporter.Instance.TotalReportCount);

				var query = new ZQuery(StmErrorReportSchema.QER_ReportXml, SQLComparisonOperator.Contains, exception.Message);
				var report = Factory.LoadTop1<StmErrorReport>(query);

				Assert(report.QER_ReportXml.ToString().Contains(exception.Message));
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
				ExceptionReporter.Instance.TotalReportCount = 0;
			}
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
			Globals.IsUserInteractive = true;
			Globals.SetIsUnitTestingProductionFunctionality(false);
			Globals.IsWeb = false;
			base.TearDown();
		}
	}

	[RoutePrefix("WebApiServiceConfigTest")]
	public class WebApiServiceConfigTestController : ApiController
	{
		[Route("TriggerError")]
		[HttpGet]
		public IHttpActionResult GetWithError()
		{
			throw new Exception("Houston, We Have a Problem");
		}
	}
}
