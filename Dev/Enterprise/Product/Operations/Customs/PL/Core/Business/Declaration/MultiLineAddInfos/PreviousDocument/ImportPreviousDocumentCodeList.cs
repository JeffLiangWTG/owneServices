using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public sealed class ImportPreviousDocumentCodeList : CodeDescriptionPairList
{
	public ImportPreviousDocumentCodeList(BusinessObjectFactory factory)
	{
		var list = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Poland,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, ZDateTime.Today,
			includeParentDataGrouping: false);
		AddRange(list);
	}
}
