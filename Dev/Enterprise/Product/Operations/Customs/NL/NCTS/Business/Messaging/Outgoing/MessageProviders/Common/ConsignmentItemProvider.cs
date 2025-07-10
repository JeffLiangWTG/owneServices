using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NL.NCTS.Business;

public class ConsignmentItemProvider : IConsignmentItem
{
	readonly NctsCommonCargoDesc item;
	public ConsignmentItemProvider(NctsCommonCargoDesc item)
	{
		this.item = Argument.NotNull(item, nameof(item));
	}

	public int GoodsItemNumber => item.BY_LineNo;

	public int DeclarationGoodsItemNumber => item.BY_DeclarationGoodsItemNumber;

	public string DeclarationType => item.BY_Type;

	public string CountryOfDispatch => item.BY_RN_NKCountryOfDispatch;

	public string CountryOfDestination => item.BY_RN_NKCountryOfDestination;

	public string ReferenceNumberUCR => item.BY_CommercialReferenceNumber;

	public string TransportChargesMethodOfPayment => item.BY_TransportChargesMethodOfPayment;

	public INCTSParty Consignee => CachedValueHelper.GetValue(ref consignee, () => item is IDocAddresses docAddresses && docAddresses.DocAddresses.Cast<JobDocAddress>().FirstOrDefault(x => x.E2_AddressType == AutoDocAddressTypes.Codes.ConsigneeAddress) is JobDocAddress jobDocAddress ? new PartyProvider(jobDocAddress) : null);
	CachedValue<INCTSParty> consignee;

	public INCTSCommodity Commodity => commodity ??= new CommodityProvider(item);
	INCTSCommodity commodity;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??= NctsDataRetrieveMethods.GetCusReferences(item.Factory, item.PK, CusReferenceTypeList.Codes.SupplyChainActor).Select((reference, index) => new AdditionalSupplyChainActorProvider(reference, index + 1)).ToArray<IAdditionalSupplyChainActor>();
	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

	public IReadOnlyCollection<IPreviousDocumentExtended> PreviousDocuments => previousDocuments ??= GetPreviousDocuments().Select(csi => new PreviousDocumentExtendedProvider(csi)).ToArray();
	IReadOnlyCollection<IPreviousDocumentExtended> previousDocuments;

	public IReadOnlyCollection<INCTSSupportingDocument> SupportingDocuments => supportingDocuments ??= item.SupportingDocuments.Select(csi => new SupportingDocumentProvider(csi)).ToArray();
	IReadOnlyCollection<INCTSSupportingDocument> supportingDocuments;

	public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ??= item.AdditionalInfos.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).Select(csi => new DocumentProvider(csi)).ToArray();
	IReadOnlyCollection<IDocument> additionalReferences;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => additionalInformation ??= item.AdditionalInfos.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).Select(csi => new AdditionalInformationProvider(csi)).ToArray();
	IReadOnlyCollection<IAdditionalInformation> additionalInformation;

	public IReadOnlyCollection<INCTSPackaging> Packagings => packagings ??= item.Packages.Select((package, index) => new PackagingProvider(package, index + 1)).ToArray();
	IReadOnlyCollection<INCTSPackaging> packagings;

	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??= item.AdditionalInfos.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).Select(csi => new DocumentProvider(csi)).ToArray();
	IReadOnlyCollection<IDocument> transportDocuments;

	IEnumerable<NctsPreviousDocument> GetPreviousDocuments()
	{
		IEnumerable<NctsPreviousDocument> result = Array.Empty<NctsPreviousDocument>();
		if (item is NctsArrivalCargoDesc arrivalCargoDesc)
		{
			result = arrivalCargoDesc.PreviousDocuments;
		}
		else if (item is NctsDepartureCargoDesc departureCargoDesc)
		{
			result = departureCargoDesc.PreviousDocuments;
		}

		return result;
	}
}
