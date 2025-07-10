using System;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Models;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.NewService.Logging;
using Common.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test.Controllers
{
	[TestFixture]
	class DataSetControllerCacheOutputFixture
	{
		[Test]
		public async Task GetServerTimestampOutputCache()
		{
			var url = new Uri("http://localhost/DummyDataSet/GetServerTimestamp");
			using (client)
			{
				var result = await client.GetAsync(url);
				service.Verify(x => x.GetServerTimestamp());
				log.Verify(x => x.Info(It.IsAny<ApiLogRequest>()));
				log.Verify(x => x.Info(It.IsAny<ApiLogResponse>()));
				result = await client.GetAsync(url);
				service.Verify(x => x.GetServerTimestamp(), Times.Once);
				log.Verify(x => x.Info(It.IsAny<ApiLogRequest>()), Times.Exactly(2));
				log.Verify(x => x.Info(It.IsAny<ApiLogResponse>()), Times.Exactly(2));
			}
		}

		[TestCase("http://localhost/DummyDataSet/Report?clientTimestamp=\"2016-11-16T06:23:39.3946168\"&checkpoint=&clientId=JM&dataSet=", TestName = "ReportOutputCache - Old Client with DataSet")]
		[TestCase("http://localhost/DummyDataSet/Report?clientTimestamp=\"2016-11-16T06:23:39.3946168\"&checkpoint=&clientId=JM&systemType=PRO&dataSet=", TestName = "ReportOutputCache - New Client with DataSet")]
		[TestCase("http://localhost/DummyDataSet/Report?clientTimestamp=\"2016-11-16T06:23:39.3946168\"&checkpoint=&clientId=JM", TestName = "ReportOutputCache - Old Client")]
		[TestCase("http://localhost/DummyDataSet/Report?clientTimestamp=\"2016-11-16T06:23:39.3946168\"&checkpoint=&clientId=JM&systemType=PRO", TestName = "ReportOutputCache - New Client")]
		public async Task ReportOutputCache(string url)
		{
			var requestUri = new Uri(url);
			using (client)
			{
				var result = await client.GetAsync(requestUri);
				clientRecord.Verify(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<bool>()));
				result = await client.GetAsync(requestUri);
				clientRecord.Verify(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
			}
		}

		Mock<IReferenceDataService<RefDataSet>> service;
		Mock<IClientRecord> clientRecord;
		Mock<ILog> log;

		WebApplicationFactory<Program> serverFactory;
		TestServer server;
		HttpClient client;

		#region Test Setup

		[SetUp]
		public void SetUp()
		{
			service = new Mock<IReferenceDataService<RefDataSet>>();
			clientRecord = new Mock<IClientRecord>();
			log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<ApiLogMiddleWare>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);

			serverFactory = DataSetControllerBaseFixture.GetTestServerFactory(null, logWrapper.Object, null, null, null, null, clientRecord.Object, service.Object);
			server = serverFactory.Server;
			client = server.CreateClient();
		}

		[TearDown]
		public void TearDown()
		{
			client?.Dispose();
			server?.Dispose();
			serverFactory?.Dispose();
		}

		#endregion
	}
}
