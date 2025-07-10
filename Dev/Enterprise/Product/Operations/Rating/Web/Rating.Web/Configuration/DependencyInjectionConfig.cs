using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.ExceptionHandling;
using CargoWise.Data;
using Enterprise.Rating.Business;
using Enterprise.Rating.Web.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Rating.Web.Configuration
{
	static class DependencyInjectionConfig
	{
		public static IServiceProvider CreateServiceProvider()
		{
			var services = new ServiceCollection();
			RegisterTypes(services);
			return services.BuildServiceProvider();
		}

		static void RegisterTypes(IServiceCollection services)
		{
			services.AddScoped<ICWServiceProvider, CWServiceProvider>();
			services.AddScoped<IExceptionHandler, APIControllersPipelineExceptionHandler>();
			services.AddScoped<ILoggerExtended, ElementaryLogger>();
			services.AddTransient<RatesAPIController>();
		}
	}

	class DefaultDependencyScope : IDependencyScope
	{
		protected IServiceScope ServiceScope;
		protected IServiceProvider ServiceProvider { get; private set; }

		public DefaultDependencyScope(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		public object GetService(Type serviceType)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (typeof(IHttpController).IsAssignableFrom(serviceType))
				{
					return ServiceProvider.GetService(serviceType);
				}

				return ServiceProvider.GetServiceSafe(serviceType) ?? CargoWise.Application.ObjectFactory.TryGet(serviceType.Name);
			}
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return ServiceProvider.GetServices(serviceType);
		}

		public void Dispose()
		{
			ServiceScope?.Dispose();
		}
	}

	class DefaultDependencyResolver : DefaultDependencyScope, IDependencyResolver
	{
		public DefaultDependencyResolver(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}

		public IDependencyScope BeginScope()
		{
			ServiceScope = ServiceProvider.CreateScope();
			return new DefaultDependencyScope(ServiceScope.ServiceProvider);
		}
	}

	static class ServiceProviderExtensions
	{
		public static object GetServiceSafe(this IServiceProvider serviceProvider, Type serviceType)
		{
			try
			{
				return serviceProvider.GetService(serviceType);
			}
			catch
			{
				return null;
			}
		}
	}
}
