using System;
using System.Linq;
using System.Runtime.Caching;
using CacheTower.Providers.FileSystem;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.NewService.UploadDownloadService;
using CargoWise.RefDbRepo.RemoteDbManager;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test;

[TestFixture]
class DependencyInjectionConfigFixture
{
	[Test]
	public void DependencyInjectionsHaveCorrectLifetime()
	{
		var serviceCollection = new ServiceCollection();
		DependencyInjectionConfig.Register(serviceCollection, new Mock<ILogWrapper>().Object);

		var singletonServices = serviceCollection.Where(x => x.Lifetime == ServiceLifetime.Singleton);
		Assert.That(singletonServices.Select(x => x.ServiceType), Is.EquivalentTo(expectedSingletonServiceTypes));

		var scopedServices = serviceCollection.Where(x => x.Lifetime == ServiceLifetime.Scoped && !x.ServiceType.Name.StartsWith("IReferenceDataService", StringComparison.OrdinalIgnoreCase));
		Assert.That(scopedServices.Select(x => x.ServiceType), Is.EquivalentTo(expectedScopedServiceTypes));
	}

	[Test]
	public void AllDataSetServicesAreRegistered()
	{
		var serviceCollection = new ServiceCollection();
		DependencyInjectionConfig.Register(serviceCollection, new Mock<ILogWrapper>().Object);
		var dataSets = DataSetStructureProvider.StructuredDataSets.Select(x => x[0]);
		foreach (var dataSet in dataSets)
		{
			var service = serviceCollection.FirstOrDefault(x => x.ServiceType.FullName.StartsWith($"CargoWise.RefDbRepo.NewService.IReferenceDataService`1[[CargoWise.RefDbRepo.Common.Contract_0_9.{dataSet}", StringComparison.OrdinalIgnoreCase));
			Assert.That(service, Is.Not.Null);
			Assert.That(service.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
		}
	}

	[Test]
	public void DefaultCacheManagerIsHashed()
	{
		FileCache.DefaultCacheManager = FileCacheManagers.Basic;
		var serviceCollection = new ServiceCollection();
		DependencyInjectionConfig.Register(serviceCollection, new Mock<ILogWrapper>().Object);
		Assert.That(FileCache.DefaultCacheManager, Is.EqualTo(FileCacheManagers.Hashed));
	}

	readonly Type[] expectedSingletonServiceTypes =
	[
		typeof(ILogWrapper),
		typeof(ErrorReportingClientWrapper),
		typeof(ISourceDataProvider),
		typeof(ILogHelper),
		typeof(ICacheWrapper),
		typeof(IFileCacheProvider),
		typeof(FileCacheLayer),
		typeof(ICacheTowerProvider),
		typeof(IHostedService),
	];

	readonly Type[] expectedScopedServiceTypes =
	[
		typeof(IDateTimeProvider),
		typeof(IDataAdaptor),
		typeof(IReferenceDataRepository),
		typeof(IReadOnlyReferenceDataRepository),
		typeof(IUserService),
		typeof(IFileCacheWrapper),
		typeof(IDataBlockCacheHelper),
		typeof(IClientRecord),
		typeof(IUpdateScriptProvider),
		typeof(IUpgradeScriptProvider),
		typeof(IDbUpgraderHelper),
		typeof(RefCusApplicabilityService),
		typeof(RefCusConditionService),
		typeof(RefCusTariffBRCharacteristicService),
		typeof(ITokenValidationHelper),
		typeof(IAuthenticationHelper)
	];
}
