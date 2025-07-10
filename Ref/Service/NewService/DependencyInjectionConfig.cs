using System;
using System.IO;
using System.Runtime.Caching;
using CacheTower.Providers.FileSystem;
using CacheTower.Serializers.SystemTextJson;
using CargoWise.eServices.Authentication.ServiceClient;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.NewService.Cachable;
using CargoWise.RefDbRepo.NewService.UploadDownloadService;
using CargoWise.RefDbRepo.RemoteDbManager;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.NewService;

public static class DependencyInjectionConfig
{
	public static void Register(IServiceCollection services, ILogWrapper logWrapper)
	{
		var connectionString = ApplicationConfig.ZZDbServerEntities;
		var readOnlyConnectionString = ApplicationConfig.ZZReadOnlyDbServerEntities;
		var stagingConnectionString = ApplicationConfig.StagingConnectionString;
		var authWebServiceApi = new AuthWebServiceApi(ApplicationConfig.Config);
		FileCache.DefaultCacheManager = FileCacheManagers.Hashed;

		var cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConfig.CacheFolder);
		services.AddSingleton(provider =>
			new FileCacheLayer(new FileCacheLayerOptions(cacheDir, SystemTextJsonCacheSerializer.Instance)));
#pragma warning disable CA2000 // Dispose objects before losing scope
		services.AddScoped<IDateTimeProvider, DateTimeProvider>()
		.AddScoped<IDataAdaptor, DataAdaptor>()
		.AddSingleton(logWrapper)
		.AddSingleton(new ErrorReportingClientWrapper())
		.AddScoped<IReferenceDataRepository>((p) => new ReferenceDataRepository(connectionString))
		.AddScoped<IReadOnlyReferenceDataRepository>((p) => new ReadOnlyReferenceDataRepository(readOnlyConnectionString))
		.AddSingleton<ISourceDataProvider>(new SourceDataProvider(stagingConnectionString))
		.AddSingleton<ILogHelper, LogHelper>()
		.AddSingleton<ICacheWrapper, CacheWrapper>()
		.AddSingleton<IFileCacheProvider, FileCacheProvider>()
		.AddScoped<IUserService, UserService>()
		.AddSingleton<ICacheTowerProvider, CacheTowerProvider>()
		.AddHostedService<CacheTowerManifestInitializationService>()
		.AddScoped<IFileCacheWrapper, FileCacheWrapper>()
		.AddScoped<IDataBlockCacheHelper, DataBlockCacheHelper>()
		.AddScoped<IClientRecord, ClientRecord>()
		.AddScoped<IUpdateScriptProvider, UpdateScriptProvider>()
		.AddScoped<IUpgradeScriptProvider, UpgradeScriptProvider>()
		.AddScoped<IDbUpgraderHelper, DbUpgraderHelper>()
		.AddScoped<ITokenValidationHelper>(p => new S2STrustTokenValidationHelper(ApplicationConfig.S2SAuthority, ApplicationConfig.S2SAudience, ApplicationConfig.DisableAuthentication))
		.AddScoped<IAuthenticationHelper>(p => new AuthenticationHelper(authWebServiceApi, AuthType.BasicAuth, ApplicationConfig.DisableAuthentication));

#pragma warning restore CA2000 // Dispose objects before losing scope

		RegisterRefDataRepoServices(services);
	}

	static void RegisterRefDataRepoServices(IServiceCollection services)
	{
		services.AddScoped<IReferenceDataService<Common.Contract_0_9.RefAccTaxRate>, RefAccTaxRateService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefAirlineCommodityCode>, RefAirlineCommodityCodeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefAirlineProductCode>, RefAirlineProductCodeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefAirline>, RefAirlineService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCarrierCode>, RefCarrierCodeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefComplianceList>, RefComplianceListService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCountry>, RefCountryService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefClient>, RefClientService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCountryStates>, RefCountryStatesService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCurrency>, RefCurrencyService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusAUNexdocECMCode>, RefCusAUNexdocECMCodeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusCodeListAttributeName>, RefCusCodeListAttributeNameService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusCodeList>, RefCusCodeListService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusCodeType>, RefCusCodeTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusConditionType>, RefCusConditionTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusConditionValueType>, RefCusConditionValueTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusConfiguration>, RefCusConfigurationService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusMap>, RefCusMapService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusMapType>, RefCusMapTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusNomenclatureGroup>, RefCusNomenclatureGroupService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusNomenclatureGroupType>, RefCusNomenclatureGroupTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusPreference>, RefCusPreferenceService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusProcedure>, RefCusProcedureService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusQuota>, RefCusQuotaService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusRateType>, RefCusRateTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusRuling>, RefCusRulingService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusTariffAdditionalCodeCategory>, RefCusTariffAdditionalCodeCategoryService>()
		.AddScoped<RefCusApplicabilityService>()
		.AddScoped<RefCusConditionService>()
		.AddScoped<RefCusTariffBRCharacteristicService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusTariff>, RefCusTariffService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusTariffType>, RefCusTariffTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusTaxOrFeeType>, RefCusTaxOrFeeTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusTaxOrFee>, RefCusTaxOrFeeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusTradeGroup>, RefCusTradeGroupService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefDataGrouping>, RefDataGroupingService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefDocOrgCusCode>, RefDocOrgCusCodeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefExchangeRateZZ>, RefExchangeRateZZService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefFacility>, RefFacilityService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefHarbourRate>, RefHarbourRateService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefLanguageType>, RefLanguageTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefPortPolygon>, RefPortPolygonService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefShippingLineMessagingRequirementType>, RefShippingLineMessagingRequirementTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefShippingLine>, RefShippingLineService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefStlFieldMapping>, RefStlFieldMappingService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefStlScript>, RefStlScriptService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefSysConfigType>, RefSysConfigTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefTimeZoneSet>, RefTimeZoneSetService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefUNLOCOPortMapping>, RefUNLOCOPortMappingService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefUNLOCO>, RefUNLOCOService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefVessel>, RefVesselService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefVesselZZ>, RefVesselZZService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.UNDGCommonData>, UNDGCommonDataService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.UNDGCountryReference>, UNDGCountryReferenceService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.UNDGSubstanceADN>, UNDGSubstanceADNService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.UNDGSubstanceADR>, UNDGSubstanceADRService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.UNDGSubstanceCFR>, UNDGSubstanceCFRService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.UNDGSubstanceJTT>, UNDGSubstanceJTTService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.UNDGSubstanceRID>, UNDGSubstanceRIDService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.UNDGSubstance>, UNDGSubstanceService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefMessagingBussPackageInfo>, RefMessagingBussPackageInfoService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusTariffAttributeName>, RefCusTariffAttributeNameService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusConditionCode>, RefCusConditionCodeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefMaterial>, RefMaterialService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefDamage>, RefDamageService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefMRComponentCode>, RefMRComponentCodeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusProfileType>, RefCusProfileTypeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusProfile>, RefCusProfileService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusProfileQuestion>, RefCusProfileQuestionService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefCusProfileQuestionPathway>, RefCusProfileQuestionPathwayService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefUnitSection>, RefUnitSectionService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefRepairCode>, RefRepairCodeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefAccElectronicProcessingFee>, RefAccElectronicProcessingFeeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefEquipmentGrade>, RefEquipmentGradeService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefGlbReleaseNote>, RefGlbReleaseNoteService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefComplianceCommodityAlert>, RefComplianceCommodityAlertService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.UNDGVersion>, UNDGVersionService>()
		.AddScoped<IReferenceDataService<Common.Contract_0_9.RefAccessorial>, RefAccessorialService>();
	}
}
