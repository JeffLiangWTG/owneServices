using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.Universal
{
	public class TariffPreferredLanguageManager
	{
		public TariffPreferredLanguageManager()
		{
			settingsStorage = ObjectFactory.Get<ISettingsStorage>("StmModuleFilterSettingsStorage", SettingsStorageContextPrefix, Env.CurrentUser.PK.ToString().ToUpperInvariant());
		}
		readonly ISettingsStorage settingsStorage;

		public ZString Language
		{
			get => language ?? (language = LoadLanguageFromStorage());
			set => SaveLanguageToStorage(value);
		}
		string language;

		public ZBool IsDefaultLanguage => Language == DefaultPreferredLanguageCode;

		#region Implementation

		const string DefaultPreferredLanguageCode = "DEF";
		const string SettingsStorageContextPrefix = "TARIFF_SEARCH_HELPER";
		const string SettingsStorageFilterName = "TARIFF_SEARCH_HELPER_PREFERENCE";

		void SaveLanguageToStorage(ZString language)
		{
			settingsStorage.SaveSettings(SettingsStorageFilterName, new TariffSearchHelperSettings() { PreferedTariffLanguage = language }.AsXml());
			ResetCachedLanguage();
		}

		void ResetCachedLanguage()
		{
			language = null;
		}

		ZString LoadLanguageFromStorage()
		{
			var savedLanguage = ZString.Empty;
			var xml = settingsStorage.LoadSettings(SettingsStorageFilterName);
			if (xml != null)
			{
				var helperSettings = (TariffSearchHelperSettings)XmlSerializableSetting.FromXml(xml, typeof(TariffSearchHelperSettings), false);
				if (helperSettings != null)
				{
					savedLanguage = helperSettings.PreferedTariffLanguage;
				}
			}
			if (savedLanguage.IsEmpty)
			{
				savedLanguage = DefaultPreferredLanguageCode;
			}
			return savedLanguage;
		}

		#endregion
	}
}
