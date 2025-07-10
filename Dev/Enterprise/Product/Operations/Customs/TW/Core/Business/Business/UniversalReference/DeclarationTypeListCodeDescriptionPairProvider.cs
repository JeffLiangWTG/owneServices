using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class DeclarationTypeListCodeDescriptionPairProvider : Integration.Customs.TW.ITWDeclarationTypeListCodeDescriptionPairProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var factory = new BusinessObjectFactory();
			return factory.GetCachedValue("TWDeclarationTypeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(TWRefCusCodeListLoader.GetImportDeclarationType(factory, ZDateTime.Today));
				result.AddRange(TWRefCusCodeListLoader.GetExportDeclarationType(factory, ZDateTime.Today));
				result.Sort();
				return result;
			});
		}
	}
}
