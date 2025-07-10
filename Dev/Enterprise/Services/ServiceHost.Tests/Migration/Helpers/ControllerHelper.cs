#if NETFRAMEWORK
using System.Net.Http;
using System.Web.Http;
#elif NET
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
#endif
using System;

namespace Enterprise.Services.ServiceHost.Tests
{
	public static class ControllerHelper
	{
		static void SetUpControllerContext<TController>(TController controllerObj)
			where TController : class
		{
#if NETFRAMEWORK
			if (controllerObj is ApiController controller)
			{
				controller.Request = new HttpRequestMessage();
				controller.Configuration = new HttpConfiguration();
			}
#elif NET
			if (controllerObj is ControllerBase controller)
			{
				controller.ControllerContext.HttpContext = new DefaultHttpContext();
			}
#endif
		}

		/// <summary>
		/// Creates a controller with it's dependency mocked
		/// </summary>
		/// <typeparam name="TController">The type of the controller</typeparam>
		/// <typeparam name="TService">The type of the dependency required by the controller</typeparam>
		/// <returns>The created controller</returns>
		/// <remarks>
		/// Consider mocking the TService your self and calling the version that accepts the TService to be injected.
		/// </remarks>
		public static TController GetController<TController, TService>()
			where TController : class
			where TService : new()
		{
			var service = new TService();
			var controllerObj = (TController)Activator.CreateInstance(typeof(TController), service);
			SetUpControllerContext(controllerObj);

			return controllerObj;
		}

		/// <summary>
		/// Creates a controller with it's dependency injected
		/// </summary>
		/// <typeparam name="TController">The type of the controller</typeparam>
		/// <typeparam name="TService">The type of the dependency required by the controller</typeparam>
		/// <returns>The created controller</returns>
		/// <remarks>
		public static TController GetController<TController, TService>(TService injected)
			where TController : class
			where TService : class
		{
			var controllerObj = (TController)Activator.CreateInstance(typeof(TController), injected);
			SetUpControllerContext(controllerObj);

			return controllerObj;
		}

		/// <summary>
		/// Creates a controller That has no dependencies dependencies
		/// </summary>
		/// <typeparam name="TController"></typeparam>
		/// <returns>The created controller</returns>
		public static TController GetController<TController>()
			where TController : class, new()
		{
			var controllerObj = new TController();
			SetUpControllerContext(controllerObj);
			return controllerObj;
		}

#if NET
		/// <summary>
		/// Creates a controller with two dependencies mocked
		/// </summary>
		/// <typeparam name="TController">The type of the controller</typeparam>
		/// <typeparam name="TMock1">The type of the first mock </typeparam>
		/// <typeparam name="TMock2">The type of the second mock</typeparam>
		/// <returns>The created controller</returns>
		/// <remarks>
		/// Consider mocking the mocks your self and calling the version that accepts the mocks to be injected.
		///	</remarks>
		public static TController GetController<TController, TMock1, TMock2>()
			where TController : ControllerBase
			where TMock1 : class
			where TMock2 : class
		{
			var mockService1 = new Mock<TMock1>();
			var mockService2 = new Mock<TMock2>();
			var controllerObj = (TController)Activator.CreateInstance(typeof(TController), mockService1.Object, mockService2.Object);
			SetUpControllerContext(controllerObj);
			return controllerObj;
		}

		/// <summary>
		/// Creates a controller with two dependencies injected
		/// </summary>
		/// <typeparam name="TController">The type of the controller</typeparam>
		/// <typeparam name="TMock1">The type of the first mock </typeparam>
		/// <typeparam name="TMock2">The type of the second mock</typeparam>
		/// <returns>The created controller</returns>
		public static TController GetController<TController, TMock1, TMock2>(TMock1 injected1, TMock2 injected2)
			where TController : ControllerBase
			where TMock1 : class
			where TMock2 : class
		{
			var controllerObj = (TController)Activator.CreateInstance(typeof(TController), injected1, injected2);
			SetUpControllerContext(controllerObj);
			return controllerObj;
		}
#endif
	}
}
