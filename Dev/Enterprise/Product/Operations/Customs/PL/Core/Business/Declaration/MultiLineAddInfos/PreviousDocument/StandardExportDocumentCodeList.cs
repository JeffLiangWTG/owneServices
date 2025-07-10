using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;
sealed class StandardExportDocumentCodeList : CodeDescriptionPairList
{
	public StandardExportDocumentCodeList(BusinessObjectFactory factory)
	{
		var previousDocumentOfExportDirection = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Poland,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, ZDateTime.Today, includeParentDataGrouping: false);
		AddRange(previousDocumentOfExportDirection);
	}
}
