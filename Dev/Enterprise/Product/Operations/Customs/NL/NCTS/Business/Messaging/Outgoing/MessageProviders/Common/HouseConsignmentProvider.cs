using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NL.NCTS.Business;

public class HouseConsignmentProvider : IHouseConsignment
{
	public HouseConsignmentProvider(NctsBill bill)
	{
		this.bill = Argument.NotNull(bill, nameof(bill));
	}
	readonly NctsBill bill;

	public int SequenceNumeric => bill.SequenceNumber;

	public string CountryOfDispatch => bill.B0_RN_NKCountryOfExport;

	public decimal GrossMass => bill.B0_Weight.Normalize();

	public string ReferenceNumberUCR => bill.B0_ReferenceID;

	public string TransportChargesMethodOfPayment => bill.B0_TransportPaymentMethod;

	public INCTSParty Consignor => CachedValueHelper.GetValue(ref consignor, () => bill.DocAddresses.Cast<JobDocAddress>().FirstOrDefault(x => x.E2_AddressType == AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress) is JobDocAddress consignorAddress ? new OmitNameAndAddressPartyProvider(consignorAddress) : null);
	CachedValue<INCTSParty> consignor;

	public INCTSParty Consignee => CachedValueHelper.GetValue(ref consignee, () => bill.DocAddresses.Cast<JobDocAddress>().FirstOrDefault(x => x.E2_AddressType == AutoDocAddressTypes.Codes.ConsigneeAddress) is JobDocAddress consigneeAddress ? new OmitNameAndAddressPartyProvider(consigneeAddress) : null);
	CachedValue<INCTSParty> consignee;

	public IReadOnlyCollection<IConsignmentItem> ConsignmentItems => consignmentItems ??= bill.GoodsItems.Select(item => new ConsignmentItemProvider(item)).ToArray<IConsignmentItem>();
	IConsignmentItem[] consignmentItems;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??= NctsDataRetrieveMethods.GetCusReferences(bill.Factory, bill.PK).Select((cr, index) => new AdditionalSupplyChainActorProvider(cr, index + 1)).ToArray<IAdditionalSupplyChainActor>();
	IAdditionalSupplyChainActor[] additionalSupplyChainActors;

	public IReadOnlyCollection<INCTSDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ??= bill.DepartureTransportInfos?.OfType<DepartureCusTransportMeans>()
		.Where(c => !string.IsNullOrEmpty(c.TPM_IdentificationNumber))
		.OrderBy(x => x.TPM_SequenceNumber)
		.Select((ccd, index) => new DepartureTransportMeansBillProvider(ccd, bill.InlandTransportModeAtDeparture, index + 1))
		.ToList();
	IReadOnlyCollection<INCTSDepartureTransportMeans> departureTransportMeans;

	public IReadOnlyCollection<INCTSPreviousDocument> PreviousDocuments => previousDocuments ??= bill.PreviousDocuments.Select(csi => new PreviousDocumentProvider(csi)).ToArray();
	IReadOnlyCollection<INCTSPreviousDocument> previousDocuments;

	public IReadOnlyCollection<INCTSSupportingDocument> SupportingDocuments => supportingDocuments ??= bill.SupportingDocuments.Select(csi => new SupportingDocumentProvider(csi)).ToArray();
	IReadOnlyCollection<INCTSSupportingDocument> supportingDocuments;

	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??= bill.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).Select(csi => new DocumentProvider(csi)).ToArray();
	IReadOnlyCollection<IDocument> transportDocuments;

	public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ??= bill.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).Select(csi => new DocumentProvider(csi)).ToArray();
	IReadOnlyCollection<IDocument> additionalReferences;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformation ??= bill.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).Select(csi => new AdditionalInformationProvider(csi)).ToArray();
	IAdditionalInformation[] additionalInformation;
}
