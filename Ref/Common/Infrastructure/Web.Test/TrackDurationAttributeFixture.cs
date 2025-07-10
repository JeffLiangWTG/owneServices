using System.Collections.Generic;
using System.Threading;
using CargoWise.RefDbRepo.Common.Utils;
using Common.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	public class TrackDurationAttributeFixture
	{
		private TrackDurationAttribute _attribute;
		private Mock<ILogWrapper> _mockLogWrapper;
		private Mock<ILog> _mockLogger;
		private ServiceProvider _serviceProvider;
		private DefaultHttpContext _httpContext;
		private ControllerActionDescriptor _actionDescriptor;
		private RouteData _routeData;

		[SetUp]
		public void SetUp()
		{
			_mockLogWrapper = new Mock<ILogWrapper>();
			_mockLogger = new Mock<ILog>();
			_mockLogWrapper.Setup(lw => lw.GetLog<TrackDurationAttribute>()).Returns(_mockLogger.Object);
			_attribute = new TrackDurationAttribute(_mockLogWrapper.Object);

			var services = new ServiceCollection();
			services.AddSingleton(_mockLogWrapper.Object);
			_serviceProvider = services.BuildServiceProvider();

			_httpContext = new DefaultHttpContext
			{
				RequestServices = _serviceProvider
			};

			_actionDescriptor = new ControllerActionDescriptor
			{
				DisplayName = "SomeClass.FakeMethodName (SomeClass)",
				RouteValues = new Dictionary<string, string>
					{
						{ "controller", "SomeClass" },
						{ "action", "FakeMethodName" }
					}
			};

			_routeData = new RouteData();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProvider.Dispose();
		}

		[Test]
		public void OnActionExecuted_LogsDuration()
		{
			var actionContext = new ActionContext
			{
				HttpContext = _httpContext,
				ActionDescriptor = _actionDescriptor,
				RouteData = _routeData
			};

			var filters = new List<IFilterMetadata>();
			var arguments = new Dictionary<string, object>();
			var controller = new Mock<Controller>().Object;

			_attribute.OnActionExecuting(new ActionExecutingContext(actionContext, filters, arguments, controller));

			Thread.Sleep(1001);

			_attribute.OnActionExecuted(new ActionExecutedContext(actionContext, filters, controller));

			_mockLogger.Verify(l => l.Info(It.Is<ApiDurationLog>(log => log.Duration >= 1000 && log.Id == "SomeClass.FakeMethodName")), Times.Once);
		}
	}
}
