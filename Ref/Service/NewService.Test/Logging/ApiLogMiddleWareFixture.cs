using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Models;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.NewService.Logging;
using CargoWise.RefDbRepo.NewService.Test.Controllers;
using Common.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class ApiLogMiddleWareFixture
	{
		[Test]
		public async Task Invoke()
		{
			var context = new DefaultHttpContext();
			context.Request.Path = "/test";
			context.Request.QueryString = new QueryString("?version=1");

			context.User = new ClaimsPrincipal(new GenericIdentity("XX", AuthType.BasicAuth));
			await logMiddleWare.Invoke(context);
			log.Verify(x => x.Info(It.Is<ApiLogRequest>(o => o.Id == 1 && o.AuthType == AuthType.BasicAuth && o.UserId == "XX" && o.Uri == "/test?version=1")));
			log.Verify(x => x.Info(It.Is<ApiLogResponse>(o => o.Id == 1 && o.AuthType == AuthType.BasicAuth && o.UserId == "XX" && o.Status == "200")));

			context.User = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { new Claim("azp", "123") }, AuthType.TokenAuth) });
			await logMiddleWare.Invoke(context);
			log.Verify(x => x.Info(It.Is<ApiLogRequest>(o => o.Id == 2 && o.AuthType == AuthType.TokenAuth && o.UserId == "123" && o.Uri == "/test?version=1")));
			log.Verify(x => x.Info(It.Is<ApiLogResponse>(o => o.Id == 2 && o.AuthType == AuthType.TokenAuth && o.UserId == "123" && o.Status == "200")));
		}

		[Test]
		public async Task UseAuthenticationBeforeLog()
		{
			var url = new Uri("http://localhost/DummyDataSet/GetAvailableDataSetTimestamps");
			logWrapper.Setup(x => x.GetLog<ExceptionHandlerMiddleware>()).Returns(log.Object);
			logWrapper.Setup(x => x.GetLog<TrackDurationAttribute>()).Returns(log.Object);
			var service = new Mock<IReferenceDataService<RefDataSet>>();
			using (var factory = DataSetControllerBaseFixture.GetTestServerFactory(null, logWrapper.Object, null, null, null, null, null, service.Object))
			using (var server = factory.Server)
			using (var client = server.CreateClient())
			{
				var result = await client.GetAsync(url);
				log.Verify(x => x.Info(It.Is<ApiLogRequest>(a => a.UserId == "XX")));
				log.Verify(x => x.Info(It.Is<ApiLogResponse>(a => a.UserId == "XX")));
			}
		}

		[Test]
		public async Task GetBodyFromRequest()
		{
			var context = new DefaultHttpContext();
			context.Request.Path = "/test";
			context.Request.QueryString = new QueryString("?version=1");
			context.Request.ContentType = "form-data";
			var fields = new Dictionary<string, StringValues>() { { "AA", "AA" } };
			var files = new FormFileCollection() { new FormFile(new MemoryStream(), 0, 10, "file", "file.txt") };
			context.Request.Form = new FormCollection(fields, files);
			await logMiddleWare.Invoke(context);
			log.Verify(x => x.Info(It.Is<ApiLogRequest>(a => a.Body == "new file uploaded")));
			log.Verify(x => x.Info(It.IsAny<UpdaterDurationLog>()), Times.Never);

			context.Request.Form = null;
			context.Request.ContentType = "application/json";
			var bodyJson = JsonConvert.SerializeObject(Tuple.Create(false, new Exception("updater error")));
			context.Request.SetBody(bodyJson);
			await logMiddleWare.Invoke(context);
			log.Verify(x => x.Info(It.Is<ApiLogRequest>(a => a.Body.Contains("\"Item1\":false"))));
			log.Verify(x => x.Info(It.IsAny<UpdaterDurationLog>()), Times.Never);

			context.Request.Path = "/Report";
			await logMiddleWare.Invoke(context);
			log.Verify(x => x.Info(It.Is<ApiLogRequest>(a => a.Body == "update failed")));
			log.Verify(x => x.Info(It.IsAny<UpdaterDurationLog>()), Times.Never);

			bodyJson = JsonConvert.SerializeObject(Tuple.Create(true, new UpdaterDurationLog { DataSetName = "AA", ClientId = "BB", TotalDuration = 1.5 }));
			context.Request.SetBody(bodyJson);
			await logMiddleWare.Invoke(context);
			log.Verify(x => x.Info(It.Is<ApiLogRequest>(a => a.Body == "update succeeded")));
			log.Verify(x => x.Info(It.Is<UpdaterDurationLog>(x => x.DataSetName == "AA" && x.ClientId == "BB" && x.TotalDuration == 1.5)), Times.Once);
		}

		[Test]
		public void GetBodyFromRequest_DoesNotThrowException()
		{
			var context = new DefaultHttpContext();
			context.Request.Path = "/Report";
			context.Request.ContentType = "application/json";
			var bodyJson = JsonConvert.SerializeObject(Tuple.Create("1", "2"));
			context.Request.SetBody(bodyJson);
			Assert.DoesNotThrowAsync(async () => await logMiddleWare.Invoke(context));
			log.Verify(x => x.Info(It.Is<ApiLogRequest>(a => a.Body == bodyJson)));
			log.Verify(x => x.Info(It.IsAny<UpdaterDurationLog>()), Times.Never);
		}

		[SetUp]
		public void Setup()
		{
			log = new Mock<ILog>();
			logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<ApiLogMiddleWare>()).Returns(log.Object);
			next = new Mock<RequestDelegate>().Object;
			logMiddleWare = new ApiLogMiddleWare(next, logWrapper.Object);
		}

		Mock<ILog> log;
		Mock<ILogWrapper> logWrapper;
		RequestDelegate next;
		ApiLogMiddleWare logMiddleWare;
	}
}
