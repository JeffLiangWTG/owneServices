using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common.Models;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.NewService.Logging;
using CargoWise.RefDbRepo.NewService.Test.Controllers;
using Common.Logging;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Diagnostics;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class ApiExceptionHandlerExtensionsFixture
	{
		[Test]
		public async Task HandleException()
		{
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			var errorReportWrapper = new Mock<ErrorReportingClientWrapper>(null);
			logWrapper.Setup(x => x.GetLog<ApiLogMiddleWare>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);

			var uri = new Uri("http://localhost/DummyDataSet/GetString");
			var service = new Mock<IReferenceDataService<RefDataSet>>();
			using (var factory = DataSetControllerBaseFixture.GetTestServerFactory(null, logWrapper.Object, null, null, null, null, null, service.Object, errorReportWrapper.Object))
			using (var server = factory.Server)
			using (var client = server.CreateClient())
			using (var response = await client.GetAsync(uri))
			{
				log.Verify(x => x.Error("Exception in /DummyDataSet/GetString", It.Is<Exception>(e => e.Message == "Operation is not valid.")));
				errorReportWrapper.Verify(x => x.PostCrashReport(It.IsAny<InvalidOperationException>(), "Delivery Service", null));
				Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
				var contentString = await response.Content.ReadAsStringAsync();
				Assert.AreEqual("An unexpected server error occurred.", contentString);
			}
		}

		[Test]
		public async Task UseAuthenticationBeforeExceptionHandler()
		{
			var url = new Uri("http://localhost/DummyDataSet/GetAvailableDataSetTimestamps");
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<ApiLogMiddleWare>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
			var service = new Mock<IReferenceDataService<RefDataSet>>();
			service.Setup(x => x.GetAllDataSetTimestamps()).Throws(new NotSupportedException("Not Supported"));
			using (var factory = DataSetControllerBaseFixture.GetTestServerFactory(null, logWrapper.Object, null, null, null, null, null, service.Object))
			using (var server = factory.Server)
			using (var client = server.CreateClient())
			{
				var result = await client.GetAsync(url);
				log.Verify(x => x.Error("Exception in /DummyDataSet/GetAvailableDataSetTimestamps - User XX", It.IsAny<Exception>()));
			}
		}

		[TestCase(-2147023667)]
		[TestCase(-2147023901)]
		[TestCase(-2147024874)]
		[TestCase(-2146233087)]
		[TestCase(-2147467259)]
		[TestCase(-2147024809)]
		[TestCase(-2147024890)]
		[TestCase(-2146232060)]
		[TestCase(-2147024832)]
		[TestCase(-2146233029)]
		public async Task Log_Ignore(int hResult)
		{
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<ApiLogMiddleWare>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
			var service = new Mock<IReferenceDataService<RefDataSet>>();
			service.Setup(x => x.GetAllDataSetTimestamps()).Throws(new HttpException("Not Supported.") { HResult = hResult });
			service.Setup(x => x.GetServerTimestamp()).Throws(new Exception("Outer Exception", new ConnectionResetException("Inner Exception") { HResult = hResult }));

			var errorReportWrapper = new Mock<ErrorReportingClientWrapper>(null);
			using (var factory = DataSetControllerBaseFixture.GetTestServerFactory(null, logWrapper.Object, null, null, null, null, null, service.Object, errorReportWrapper.Object))
			using (var server = factory.Server)
			using (var client = server.CreateClient())
			{
				var url = new Uri("http://localhost/DummyDataSet/GetAvailableDataSetTimestamps");
				var result = await client.GetAsync(url);
				log.Verify(x => x.Error("Exception in /DummyDataSet/GetAvailableDataSetTimestamps - User XX", It.Is<Exception>(e => e.Message == "Not Supported." && e.HResult == hResult)));
				errorReportWrapper.Verify(x => x.PostCrashReport(It.IsAny<Exception>(), "Delivery Service", null), Times.Never);

				url = new Uri("http://localhost/DummyDataSet/GetServerTimestamp");
				result = await client.GetAsync(url);
				log.Verify(x => x.Error("Exception in /DummyDataSet/GetServerTimestamp - User XX", It.Is<Exception>(e => e.InnerException != null && e.InnerException.HResult == hResult)));
				errorReportWrapper.Verify(x => x.PostCrashReport(It.IsAny<Exception>(), "Delivery Service", null), Times.Never);
			}
		}
	}
}
