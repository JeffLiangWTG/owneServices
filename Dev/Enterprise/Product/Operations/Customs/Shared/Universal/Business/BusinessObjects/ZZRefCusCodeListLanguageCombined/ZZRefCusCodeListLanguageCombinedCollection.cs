using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusCodeListLanguageCombinedCollection : DependentBusinessObjectCollection<ZZRefCusCodeListLanguageCombined, ZZRefCusCodeListCombined>
	{
		public ZZRefCusCodeListLanguageCombinedCollection(ZZRefCusCodeListCombined cusCodeList)
			: base(cusCodeList)
		{
		}

		public ZZRefCusCodeListLanguageCombined AddNew(ZString language, ZString description)
		{
			var result = AddNew();
			result.ZXA_ZX6_NKLanguage = language;
			result.ZXA_Description = description;
			return result;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => ZZRefCusCodeListLanguageCombinedSchema.ZXA_ZZD_CodeList;

		protected override bool AllowNewCore => !Master.ZZD_IsSystem;
	}
}
