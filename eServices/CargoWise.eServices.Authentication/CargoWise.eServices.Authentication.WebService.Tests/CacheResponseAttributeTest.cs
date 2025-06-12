using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using log4net.Appender;
using log4net.Config;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.WebService.Tests
{
	[TestFixture]
	class CacheResponseAttributeTest
	{
		[Test]
		public void CheckSystemExistence_CacheResponse_ReturnSimilarResponses()
		{
			var appender = new MemoryAppender();
			BasicConfigurator.Configure(appender);

			MockDatabaseHelper("CheckSystemIDExistence", true, 1);
			var cacheAttributeMock = GenerateCacheAttribute();

			GenerateActionContext("CheckSystemIDExistence", "ABC");
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Found, actionContext.Response.StatusCode);

			GenerateActionContext("CheckSystemIDExistence", "ABC");
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Found, actionContext.Response.StatusCode);
			databaseMock.Verify(_ => _.CheckSystemIDExistence(It.IsAny<string>()), Times.Once);

			var messagesList = appender.GetEvents().ToList();
			Assert.That(messagesList.Count, Is.EqualTo(7));
			foreach(var message in messagesList)
			{
				Assert.That(message.Level == log4net.Core.Level.Debug);
			}
			Assert.That(messagesList[0].RenderedMessage.Contains("New Request"));
			Assert.That(messagesList[1].RenderedMessage.Contains("Using Database"));
			Assert.That(messagesList[2].RenderedMessage.Contains("IP Address/es"));
			Assert.That(messagesList[3].RenderedMessage.Contains("Saving Cache"));
			Assert.That(messagesList[4].RenderedMessage.Contains("Cache Saved"));
			Assert.That(messagesList[5].RenderedMessage.Contains("New Request"));
			Assert.That(messagesList[6].RenderedMessage.Contains("Using Cache"));
		}

		[Test]
		public void CheckCodeExistence_CacheResponse_ReturnSimilarResponses()
		{
			MockDatabaseHelper("CheckCodeExistence", false, 1);
			var cacheAttributeMock = GenerateCacheAttribute();

			GenerateActionContext("CheckCodeExistence", new[] { "ABC", "123" });
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Unauthorized, actionContext.Response.StatusCode);

			GenerateActionContext("CheckCodeExistence", new[] { "ABC", "123" });
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Unauthorized, actionContext.Response.StatusCode);
			databaseMock.Verify(_ => _.CheckCodeExistence(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void CheckSystem_ExpiredCache()
		{
			MockDatabaseHelper("CheckSystemIDExistence", true, 1);
			var timeMock = new Mock<IDateTime>();
			var cacheAttributeMock = GenerateCacheAttribute();
			cacheAttributeMock.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);

			GenerateActionContext("CheckSystemIDExistence", "ABC");
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now);
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Found, actionContext.Response.StatusCode);

			GenerateActionContext("CheckSystemIDExistence", "ABC");
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now.AddSeconds(301));
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Found, actionContext.Response.StatusCode);
			databaseMock.Verify(_ => _.CheckSystemIDExistence(It.IsAny<string>()), Times.Exactly(2));
		}

		[Test]
		public void ValidateSystemIDAndPassword_CacheResponse_ReturnSimilarResponses()
		{
			MockDatabaseHelper("ValidateSystemIDAndPassword", true, 1);
			var cacheAttributeMock = GenerateCacheAttribute();

			GenerateActionContext("ValidateSystemIDAndPassword", new[] { "SYSID", "correct" });
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.OK, actionContext.Response.StatusCode);

			GenerateActionContext("ValidateSystemIDAndPassword", new[] { "SYSID", "correct" });
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.OK, actionContext.Response.StatusCode);
			databaseMock.Verify(_ => _.ValidateSystemIDAndPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void ValidateSystemIDAndPassword_CacheResponse_DifferentPassword()
		{
			MockDatabaseHelper("ValidateSystemIDAndPassword", true, 2);
			var cacheAttributeMock = GenerateCacheAttribute();

			GenerateActionContext("ValidateSystemIDAndPassword", new[] { "SYSID", "correct" });
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.OK, actionContext.Response.StatusCode);

			GenerateActionContext("ValidateSystemIDAndPassword", new[] { "SYSID", "wrong" });
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.OK, actionContext.Response.StatusCode);
			databaseMock.Verify(_ => _.ValidateSystemIDAndPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
		}

		[Test]
		public void ValidateCodeAndPassword_CacheResponse_ReturnSimilarResponses()
		{
			MockDatabaseHelper("ValidateCodeAndPassword", false, 1);
			var cacheAttributeMock = GenerateCacheAttribute();

			GenerateActionContext("ValidateCodeAndPassword", new[] { "ABC", "DEF", "correct" });
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Unauthorized, actionContext.Response.StatusCode);

			GenerateActionContext("ValidateCodeAndPassword", new[] { "ABC", "DEF", "correct" });
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Unauthorized, actionContext.Response.StatusCode);

			databaseMock.Verify(_ => _.ValidateCodeAndPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void ValidateCodeAndPassword_CacheResponse_DifferentPassword()
		{
			MockDatabaseHelper("ValidateCodeAndPassword", false, 1);
			var cacheAttributeMock = GenerateCacheAttribute();

			GenerateActionContext("ValidateCodeAndPassword", new[] { "ABC", "DEF", "correct" });
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Unauthorized, actionContext.Response.StatusCode);

			GenerateActionContext("ValidateCodeAndPassword", new[] { "ABC", "DEF", "wrong" });
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Unauthorized, actionContext.Response.StatusCode);

			databaseMock.Verify(_ => _.ValidateCodeAndPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
		}

		[Test]
		public void ValidateSystem_ExpiredCache()
		{
			MockDatabaseHelper("ValidateSystemIDAndPassword", true, 2);
			var timeMock = new Mock<IDateTime>();
			var cacheAttributeMock = GenerateCacheAttribute();
			cacheAttributeMock.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);

			GenerateActionContext("ValidateSystemIDAndPassword", new[] { "SYSID", "correct" });
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now);
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.OK, actionContext.Response.StatusCode);

			GenerateActionContext("ValidateSystemIDAndPassword", new[] { "SYSID", "correct" });
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now.AddSeconds(301));
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.OK, actionContext.Response.StatusCode);
			databaseMock.Verify(_ => _.ValidateSystemIDAndPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
		}

		#region All cache 24 hrs when Database IsNot Alive

		[TestCase(secondsOf5Minutes - 1, true, 1)]
		[TestCase(secondsOf5Minutes + 1, true, 2)]
		[TestCase(secondsOf5Minutes - 1, false, 1)]
		[TestCase(secondsOf5Minutes + 1, false, 1)]
		[TestCase(secondsOf24Hours - 1, false, 1)]
		[TestCase(secondsOf24Hours + 1, false, 2)]
		public void CheckSystemIDExistence_CacheResponse­_24hrs(int secondsPast, bool isDatabaseAlive, int expectedCallTimes)
		{
			MockDatabaseHelper("CheckSystemIDExistence", true, 1, isDatabaseAlive);

			var timeMock = new Mock<IDateTime>();
			var cacheAttributeMock = GenerateCacheAttribute();
			cacheAttributeMock.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);

			GenerateActionContext("CheckSystemIDExistence", "ABC");
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now);
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Found, actionContext.Response.StatusCode);

			GenerateActionContext("CheckSystemIDExistence", "ABC");
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now.AddSeconds(secondsPast));
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Found, actionContext.Response.StatusCode);
			databaseMock.Verify(_ => _.CheckSystemIDExistence(It.IsAny<string>()), Times.Exactly(expectedCallTimes));
		}

		[TestCase(secondsOf5Minutes - 1, true, 1)]
		[TestCase(secondsOf5Minutes + 1, true, 2)]
		[TestCase(secondsOf5Minutes - 1, false, 1)]
		[TestCase(secondsOf5Minutes + 1, false, 1)]
		[TestCase(secondsOf24Hours - 1, false, 1)]
		[TestCase(secondsOf24Hours + 1, false, 2)]
		public void CheckCodeExistence_CacheResponse­_24hrs(int secondsPast, bool isDatabaseAlive, int expectedCallTimes)
		{
			MockDatabaseHelper("CheckCodeExistence", true, 1, isDatabaseAlive);
			var timeMock = new Mock<IDateTime>();
			var cacheAttributeMock = GenerateCacheAttribute();
			cacheAttributeMock.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);

			GenerateActionContext("CheckCodeExistence", new[] { "ABC", "123" });
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now);
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Found, actionContext.Response.StatusCode);

			GenerateActionContext("CheckCodeExistence", new[] { "ABC", "123" });
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now.AddSeconds(secondsPast));
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.Found, actionContext.Response.StatusCode);
			databaseMock.Verify(_ => _.CheckCodeExistence(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(expectedCallTimes));
		}

		[TestCase(secondsOf5Minutes - 1, true, 1)]
		[TestCase(secondsOf5Minutes + 1, true, 2)]
		[TestCase(secondsOf5Minutes - 1, false, 1)]
		[TestCase(secondsOf5Minutes + 1, false, 1)]
		[TestCase(secondsOf24Hours - 1, false, 1)]
		[TestCase(secondsOf24Hours + 1, false, 2)]
		public void ValidateSystemIDAndPassword_CacheResponse­_24hrs(int secondsPast, bool isDatabaseAlive, int expectedCallTimes)
		{
			MockDatabaseHelper("ValidateSystemIDAndPassword", true, 1, isDatabaseAlive);
			var timeMock = new Mock<IDateTime>();
			var cacheAttributeMock = GenerateCacheAttribute();
            cacheAttributeMock.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);

			GenerateActionContext("ValidateSystemIDAndPassword", new[] { "SYSID", "correct" });
            timeMock.Setup(_ => _.Now()).Returns(DateTime.Now);
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.OK, actionContext.Response.StatusCode);

			GenerateActionContext("ValidateSystemIDAndPassword", new[] { "SYSID", "correct" });
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now.AddSeconds(secondsPast));
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.OK, actionContext.Response.StatusCode);
			databaseMock.Verify(_ => _.ValidateSystemIDAndPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(expectedCallTimes));
		}

		[TestCase(secondsOf5Minutes - 1, true, 1)]
		[TestCase(secondsOf5Minutes + 1, true, 2)]
		[TestCase(secondsOf5Minutes - 1, false, 1)]
		[TestCase(secondsOf5Minutes + 1, false, 1)]
		[TestCase(secondsOf24Hours - 1, false, 1)]
		[TestCase(secondsOf24Hours + 1, false, 2)]
		public void ValidateCodeAndPassword_CacheResponse­_24hrs(int secondsPast, bool isDatabaseAlive, int expectedCallTimes)
		{
			MockDatabaseHelper("ValidateCodeAndPassword", true, 1, isDatabaseAlive);
			var timeMock = new Mock<IDateTime>();
			var cacheAttributeMock = GenerateCacheAttribute();
            cacheAttributeMock.Setup(_ => _.DateTimeWrapper).Returns(timeMock.Object);

			GenerateActionContext("ValidateCodeAndPassword", new[] { "ABC", "DEF", "correct" });
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now);
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.OK, actionContext.Response.StatusCode);

			GenerateActionContext("ValidateCodeAndPassword", new[] { "ABC", "DEF", "correct" });
			timeMock.Setup(_ => _.Now()).Returns(DateTime.Now.AddSeconds(secondsPast));
			cacheAttributeMock.Object.OnActionExecuting(actionContext);
			Assert.AreEqual(HttpStatusCode.OK, actionContext.Response.StatusCode);
			databaseMock.Verify(_ => _.ValidateCodeAndPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(expectedCallTimes));
		}

		#endregion

		void MockDatabaseHelper(string action, bool result, int expectedCallTimes, bool isDatabaseAlive = true)
		{
			databaseMock = new Mock<IDatabaseHelper>();
			databaseMock.Setup(_ => _.IsDatabaseAlive()).Returns(isDatabaseAlive);
			switch (action)
			{
				case "CheckSystemIDExistence":
					databaseMock.Setup(_ => _.CheckSystemIDExistence(It.IsAny<string>())).Returns(result);
					break;
				case "CheckCodeExistence":
					databaseMock.Setup(_ => _.CheckCodeExistence(It.IsAny<string>(), It.IsAny<string>())).Returns(result);
					break;
				case "ValidateSystemIDAndPassword":
					databaseMock.Setup(_ => _.ValidateSystemIDAndPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(result);
					break;
				case "ValidateCodeAndPassword":
					databaseMock.Setup(_ => _.ValidateCodeAndPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(result);
					break;
			}
		}

		Mock<CacheResponseAttribute> GenerateCacheAttribute()
		{
			serviceMock = new Mock<AuthenticationController>();
			serviceMock.Setup(_ => _.DatabaseHelper).Returns(databaseMock.Object);

			var cacheAttributeMock = new Mock<CacheResponseAttribute> { CallBase = true };

			cacheAttributeMock.Setup(_ => _.InvokeAuthenticationAction(It.IsAny<HttpActionContext>())).Callback<HttpActionContext>(
				ctx =>
				{
					var unWrappedController = serviceMock.Object;
					var result = unWrappedController.GetType().GetMethod(ActionName).Invoke(unWrappedController, new[] { Param });
					cacheAttributeMock.Object.OnActionExecuted(GenerateExecutedContext(actionContext, result));
				});
			cacheAttributeMock.Setup(_ => _.DatabaseHelper).Returns(databaseMock.Object);
			cacheAttributeMock.Object.ClearCaches();
			return cacheAttributeMock;
		}

		void GenerateActionContext(string actionName, object param)
		{
			var httpRouteDataMock = new Mock<IHttpRouteData>();
			var routeAction = new Dictionary<string, object>() { { "Action", actionName } };
			httpRouteDataMock.Setup(_ => _.Values).Returns(routeAction);
			httpRouteDataMock.Setup(_ => _.Route).Returns(new HttpRoute());

			var httpRequestContexMock = new Mock<HttpRequestContext>();
			httpRequestContexMock.Setup(_ => _.RouteData).Returns(httpRouteDataMock.Object);

			var controllerContext = new HttpControllerContext()
			{
				RequestContext = httpRequestContexMock.Object,
				RouteData = httpRouteDataMock.Object
			};

			actionContext = new HttpActionContext()
			{
				ControllerContext = controllerContext,
				ActionArguments = { { "param", param } },
			};
		}

		HttpActionExecutedContext GenerateExecutedContext(HttpActionContext actionContext, object result)
		{
			actionContext.Response = (HttpResponseMessage)result;
			var httpExecutedContext = new HttpActionExecutedContext()
			{
				ActionContext = actionContext
			};

			return httpExecutedContext;
		}

		Mock<AuthenticationController> serviceMock;
		Mock<IDatabaseHelper> databaseMock;
		HttpActionContext actionContext;

		const int secondsOf24Hours = 60 * 60 * 24;
		const int secondsOf5Minutes = 300;

		string ActionName
		{
			get
			{
				return actionContext != null
					? actionContext.RequestContext.RouteData.Values["Action"].ToString()
					: string.Empty;
			}
		}

		private object Param
		{
			get { return actionContext.ActionArguments["param"]; }
		}
	}
}
