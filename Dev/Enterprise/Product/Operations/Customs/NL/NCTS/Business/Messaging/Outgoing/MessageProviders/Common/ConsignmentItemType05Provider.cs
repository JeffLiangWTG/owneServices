using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class ConsignmentItemType05Provider : INCTSConsignmentItemType05
{
	readonly NctsCommonCargoDesc item;
	public ConsignmentItemType05Provider(NctsCommonCargoDesc item)
	{
		this.item = Argument.NotNull(item, nameof(item));
	}

	public int GoodsItemNumber => item.BY_LineNo;

	public int DeclarationGoodsItemNumber => item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW || item.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF || item.BY_UnloadedState == NctsUnloadedStateList.Codes.MIS
		? item.BY_DeclarationGoodsItemNumber
		: ZInt.Zero;

	public INCTSCommodity Commodity => CachedValueHelper.GetValue(ref commodity,
		() => item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW || item.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF
		? new CC044CCommodityProvider(UnloadedItemIfExists)
		: null);
	CachedValue<INCTSCommodity> commodity;

	public IReadOnlyCollection<INCTSPackaging> Packagings => packagings ??= item.Packages
		.Where(p => p.B5_TypeOfDifference == NctsUnloadedStateList.Codes.NEW || p.B5_TypeOfDifference == NctsUnloadedStateList.Codes.MIS)
		.Select(p => new CC044CPackagingProvider(p))
		.ToArray();
	IReadOnlyCollection<INCTSPackaging> packagings;

	public IReadOnlyCollection<INCTSSupportingDocument> SupportingDocuments => supportingDocuments ??=
		NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, CusSupportingInfoTypeList.Codes.SupportingDocument)
		.Where(s => s.CSI_Status == NctsUnloadedStateList.Codes.NEW || s.CSI_Status == NctsUnloadedStateList.Codes.MIS)
		.Select(s => new CC044CSupportingDocumentProvider(s))
		.ToArray();
	IReadOnlyCollection<INCTSSupportingDocument> supportingDocuments;

	public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ??=
		NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.AdditionalReference)
		.Where(i => i.CSI_Status == NctsUnloadedStateList.Codes.NEW || i.CSI_Status == NctsUnloadedStateList.Codes.MIS)
		.Select(i => new CC044CDocumentProvider(i))
		.ToArray();
	IReadOnlyCollection<IDocument> additionalReferences;

	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??=
		NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.TransportDocument)
		.Where(t => t.CSI_Status == NctsUnloadedStateList.Codes.NEW || t.CSI_Status == NctsUnloadedStateList.Codes.MIS)
		.Select(t => new CC044CDocumentProvider(t))
		.ToArray();
	IReadOnlyCollection<IDocument> transportDocuments;

	NctsCommonCargoDesc UnloadedItemIfExists => unloadedItemIfExists ??= item.BY_BY_Commodity.IsEmpty ? item : NctsUnloadedCargoDesc.Load((NctsArrivalCargoDesc)item);
	NctsCommonCargoDesc unloadedItemIfExists;
}
