using CargoWise.RefDbRepo.PLReferenceData.Services;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

sealed class TaricUpdateParserConfig(ISettingsIndexer settings = null) : ITaricUpdateParserConfig
{
	readonly ISettingsIndexer appSettings = settings;

	public string TariffUpdateFolder => AppConfigHelper.GetAppSettingsValue(Constants.AppSettingsKeys.TariffUpdateFolder, appSettings);
}
