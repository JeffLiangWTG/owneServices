using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.ExceptionHandling;
using CargoWise.Data;
using Enterprise.Services.Scim.Api.Controllers;
using Enterprise.Services.Scim.Api.Helpers;
using Enterprise.Services.Scim.Business;
using Enterprise.Services.Scim.Contracts;
using Enterprise.Services.Scim.Helpers;
using Enterprise.Services.Scim.Models;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Scim;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.Helpers;
using SimpleIdServer.Scim.Persistence;
using SimpleIdServer.Scim.Persistence.InMemory;

namespace Enterprise.Services.Scim.Api.Config
{
	static class DependencyInjectionConfig
	{
		public static IServiceProvider CreateServiceProvider()
		{
			var services = new ServiceCollection();
			RegisterTypes(services);
			return services.BuildServiceProvider();
		}

		public static void RegisterTypes(IServiceCollection services)
		{
			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
			services.AddSingleton<IAppSettings, AppSettings>();
			services.AddSingleton<IExceptionHandler, ScimServiceAPIGlobalExceptionHandler>();
			services.AddSingleton<IIPSafelistCacheStore, IPSafelistCacheStore>();
			services.AddSingleton<IIPSafelistDBStore, IPSafelistDBStore>();
			services.AddSingleton<IIPSafelistHelper, IPSafelistHelper>();
			services.AddSingleton<ISafelistValidator, SafelistValidator>();
			services.AddTransient<GroupsController>();
			services.AddTransient<UsersController>();
			services.AddTransient<WtgController>();

			var representations = new List<SCIMRepresentation>();

			var serviceProvider = services.BuildServiceProvider();
			var configurations = serviceProvider.GetRequiredService<IAppSettings>();
			var basePath = CW1ApplicationInitialiser.IsHostedInIIS() ? HostingEnvironment.MapPath($"~/{configurations.SchemaPath}") : Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), configurations.SchemaPath);
			var userSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "UserSchema.json"), SCIMResourceTypes.User, true);
			var groupSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "GroupSchema.json"), SCIMResourceTypes.Group, true);
			var cwUserSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "CargoWiseUserSchema.json"), SCIMResourceTypes.User, true);
			var cwGroupSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "CargoWiseGroupSchema.json"), SCIMResourceTypes.Group, true);

			userSchema.SchemaExtensions.Add(new SCIMSchemaExtension
			{
				Id = Guid.NewGuid().ToString(),
				Schema = "urn:ietf:params:scim:schemas:extension:cw:2.0:User"
			});

			groupSchema.SchemaExtensions.Add(new SCIMSchemaExtension
			{
				Id = Guid.NewGuid().ToString(),
				Schema = "urn:ietf:params:scim:schemas:extension:cw:2.0:Group"
			});

			var schemas = new List<SCIMSchema>();
			schemas.AddRange(new List<SCIMSchema>
			{
				userSchema,
				groupSchema,
				cwUserSchema,
				cwGroupSchema
			});

			var provisioningConfigurations = new List<ProvisioningConfiguration>();
			services.AddSingleton<ISCIMRepresentationCommandRepository>(new DefaultSCIMRepresentationCommandRepository(representations));
			services.AddSingleton<ISCIMRepresentationQueryRepository>(new DefaultSCIMRepresentationQueryRepository(representations));
			services.AddSingleton<ISCIMSchemaCommandRepository>(new DefaultSchemaCommandRepository(schemas));
			services.AddSingleton<ISCIMSchemaQueryRepository>(new DefaultSchemaQueryRepository(schemas));
			services.AddSingleton<ISCIMAttributeMappingQueryRepository>(new DefaultAttributeMappingQueryRepository(SCIMConstants.StandardAttributeMapping));
			services.AddSingleton<IProvisioningConfigurationRepository>(new DefaultProvisioningConfigurationRepository(provisioningConfigurations));
			services.AddSingleton(new SCIMHostOptions());

			services.AddSingleton<IPersistanceRepository<ScimUser>, UserRepository>();
			services.AddSingleton<IPersistanceRepository<ScimGroup>, GroupRepository>();
			services.AddTransient<IAttributeReferenceEnricher, AttributeReferenceEnricher>();
			services.AddTransient<IResourceTypeResolver, ResourceTypeResolver>();
			services.AddTransient<Helpers.IUriProvider, UriProvider>();

			services.AddSingleton<IScimRepresentationConverter<ScimUser>, ScimRepresentationUserConverter>();
			services.AddSingleton<IScimRepresentationConverter<ScimGroup>, ScimGroupRepresentationConverter>();
			services.AddTransient<ISCIMRepresentationHelper, SCIMRepresentationHelper>();
			services.AddTransient<IScimToScimRepresentation, ScimToScimRepresentation>();
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
