using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC044CConsignmentItemProvider : ICC044CConsignmentItem
{
	public CC044CConsignmentItemProvider(NctsArrivalCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
	}

	readonly NctsArrivalCargoDesc goodsItem;

	public string GoodsItemNumber => goodsItem.BY_LineNo.ToString();

	public string DeclarationGoodsItemNumber => goodsItem.BY_DeclarationGoodsItemNumber.ToString();

	public ICommodity Commodity => commodity ??= new CommodityProvider(goodsItem);
	ICommodity commodity;

	public IReadOnlyCollection<IPackaging> Packaging => packaging ?? (packaging = GetPackaging());
	IReadOnlyCollection<IPackaging> packaging;

	public IReadOnlyCollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocuments());
	IReadOnlyCollection<ISupportingDocument> supportingDocument;

	public IReadOnlyCollection<IDocument> TransportDocument => transportDocument ?? (transportDocument = GetTransportDocument());
	IReadOnlyCollection<IDocument> transportDocument;

	public IReadOnlyCollection<IDocument> AdditionalReference => additionalReference ?? (additionalReference = GetAdditionalReference());
	IReadOnlyCollection<IDocument> additionalReference;
	IReadOnlyCollection<IPackaging> GetPackaging() => goodsItem.Packages.Cast<NctsPackage>()
		.Select((x, i) => new PackagingProvider(i + 1, x, IsInPhase5TransitionPeriod))
		.ToArray();

	IReadOnlyCollection<ISupportingDocument> GetSupportingDocuments() => goodsItem.SupportingDocuments.Lines.Cast<NctsSupportingDocument>()
		.Select((x, i) => new SupportingDocumentProvider(i + 1, x, IsInPhase5TransitionPeriod))
		.ToArray();

	IReadOnlyCollection<IDocument> GetTransportDocument() => goodsItem.AdditionalInfos
		.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument)
		.Select((x, i) => new TransportDocumentProvider(i + 1, x))
		.ToArray();

	IReadOnlyCollection<IDocument> GetAdditionalReference() => goodsItem.AdditionalInfos
		.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference)
		.Select((x, i) => new CC044CAdditionalReferenceProvider(i + 1, x))
		.ToArray();

	bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => goodsItem.IsInPhase5TransitionPeriod);
	CachedValue<bool> isInPhase5TransitionPeriod;
}
