using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public abstract class RefCusCodeListLanguageParserConfig
	{
		public abstract string GetZXA_Description(string[] columns);
		public virtual bool ValidateZXA_Description(RefCusCodeListLanguage refCusCodeListLanguage) => !string.IsNullOrWhiteSpace(refCusCodeListLanguage.ZXA_Description);
		public virtual bool ValidateZXA_ZX6_NKLanguage(RefCusCodeListLanguage refCusCodeListLanguage) => !string.IsNullOrWhiteSpace(refCusCodeListLanguage.ZXA_ZX6_NKLanguage);
		public virtual Func<RefCusCodeList, RefCusCodeListLanguage, bool> Validate => (refCusCodeList, refCusCodeListLanguage)
			=> ValidateZXA_Description(refCusCodeListLanguage) && ValidateZXA_ZX6_NKLanguage(refCusCodeListLanguage);
	}
}
