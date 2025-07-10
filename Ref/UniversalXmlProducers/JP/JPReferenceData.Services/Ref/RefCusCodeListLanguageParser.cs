using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public abstract class RefCusCodeListLanguageParser
	{
		public abstract string ZXA_ZX6_NKLanguage { get; }

		RefCusCodeListLanguageParserConfig[] configs;
		public RefCusCodeListLanguageParserConfig[] Configs
		{
			get
			{
				if (configs == null)
				{
					configs = GetConfigsCore();
				}
				return configs;
			}
		}
		protected abstract RefCusCodeListLanguageParserConfig[] GetConfigsCore();

		public bool TryAddRefCusCodeListLanguage(RefCusCodeList refCusCodeList, string[] columns)
		{
			return TryAddRefCusCodeListLanguageCore(refCusCodeList, columns);
		}

		protected virtual bool TryAddRefCusCodeListLanguageCore(RefCusCodeList refCusCodeList, string[] columns)
		{
			for (var i = 0; i < Configs.Length; i++)
			{
				var refCusCodeListLanguage = new RefCusCodeListLanguage()
				{
					ZXA_ZX6_NKLanguage = ZXA_ZX6_NKLanguage,
					ZXA_Description = Configs[i].GetZXA_Description(columns)
				};

				if (Configs[i].Validate(refCusCodeList, refCusCodeListLanguage))
				{
					refCusCodeList.RefCusCodeListLanguages[i] = refCusCodeListLanguage;
				}
				else
				{
					return false;
				}
			}

			return true;
		}
	}
}
