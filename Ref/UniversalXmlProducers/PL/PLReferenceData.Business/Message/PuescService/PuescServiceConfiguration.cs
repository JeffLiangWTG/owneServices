using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.PLReferenceData.Services;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService;

sealed class PuescServiceConfiguration(ISettingsIndexer settingsIndexer = null) : IPuescServiceConfiguration
{
	readonly ISettingsIndexer settings = settingsIndexer;

	public string Login => AppConfigHelper.GetAppSettingsValue(Constants.AppSettingsKeys.PUESCLogin, settings);

	public string Password
	{
		get
		{
			var appSettingsPassword = AppConfigHelper.GetAppSettingsValue(Constants.AppSettingsKeys.PUESCPassword, settings);

			var decodedArray = Convert.FromBase64String(appSettingsPassword);
			var decodedString = Encoding.UTF8.GetString(decodedArray);
			return string.Join("", decodedString.Where((ch, index) => index % 2 == 0));
		}
	}

	public string Url => AppConfigHelper.GetAppSettingsValue(Constants.AppSettingsKeys.PUESCUrl, settings);
}
