#if NET
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
#elif NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using CargoWise.Common;
#endif
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost.Tests
{
	class ControllerBuilder<TController> where TController : ControllerBase
	{
#if NET
		readonly IServiceCollection services;
		IIdentity identity;

		public ControllerBuilder()
		{
			services = new ServiceCollection();
			services.AddTransient<TController>();
		}

		public ControllerBuilder<TController> AddDependency<T>(T dependency) where T : class
		{
			services.AddSingleton(dependency);
			return this;
		}

		public ControllerBuilder<TController> AddIdentity(IIdentity identity)
		{
			this.identity = identity;
			return this;
		}

		public TController BuildController()
		{
			var controller = services.BuildServiceProvider()
				.GetRequiredService<TController>();

			SetUpControllerContext(controller);
			SetupUser(controller);
			return controller;
		}

		static void SetUpControllerContext(TController controllerObj)
		{
			if (controllerObj is ControllerBase controller)
			{
				controller.ControllerContext.HttpContext = new DefaultHttpContext();
			}
		}

		void SetupUser(TController controller)
		{
			if (identity is null)
			{
				return;
			}
			controller.HttpContext.User = new GenericPrincipal(identity, null);
		}
#elif NETFRAMEWORK

		readonly List<object> services;
		IIdentity identity;

		public ControllerBuilder()
		{
			services = [];
		}

		public ControllerBuilder<TController> AddDependency<T>(T dependency) where T : class
		{
			services.Add(dependency);
			return this;
		}

		public ControllerBuilder<TController> AddIdentity(IIdentity identity)
		{
			this.identity = identity;
			return this;
		}

		public TController BuildController()
		{
			TController controller;
			if (services.IsNullOrEmpty())
			{
				controller = (TController)Activator.CreateInstance(typeof(TController));
			}
			else
			{
				controller = (TController)Activator.CreateInstance(typeof(TController), services.ToArray());
			}

			SetUpControllerContext(controller);
			SetupUser(controller);
			return controller;
		}

		static void SetUpControllerContext(TController controllerObj)
		{
			if (controllerObj is ApiController controller)
			{
				controller.Request = new HttpRequestMessage();
				controller.Configuration = new HttpConfiguration();
			}
		}

		void SetupUser(TController controller)
		{
			if (identity is null)
			{
				return;
			}
			controller.User = new GenericPrincipal(identity, null);
		}
#endif
	}
}
