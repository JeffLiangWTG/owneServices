using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public sealed class ExportPreviousDocumentCodeList : CodeDescriptionPairList
{
	public ExportPreviousDocumentCodeList(BusinessObjectFactory factory)
	{
		var previousDocumentOfExportDirection = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Poland,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, ZDateTime.Today, includeParentDataGrouping: false);
		var exportPreviousDocumentSpecialProcedures = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Poland,
			UniversalReferenceConstants.RefCusCodeListType.Codes.ExportPreviousDocumentSpecialProcedures, ZDateTime.Today, includeParentDataGrouping: false);
		AddRange(previousDocumentOfExportDirection);
		AddRange(exportPreviousDocumentSpecialProcedures);
	}
}
