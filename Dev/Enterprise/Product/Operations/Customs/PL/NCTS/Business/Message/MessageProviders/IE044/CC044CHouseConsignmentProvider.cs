using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC044CHouseConsignmentProvider : ICC044CHouseConsignment
{
	public CC044CHouseConsignmentProvider(int sequenceNumber, NctsBill bill)
	{
		this.bill = Argument.NotNull(bill, nameof(bill));
		SequenceNumber = sequenceNumber.ToString();
	}

	readonly NctsBill bill;

	public string SequenceNumber { get; }

	public decimal GrossMass => bill.MovementDetail.B9_UnloadedState == NctsUnloadedStateList.Codes.DIF ? bill.GrossWeightUnloadedInKilograms : bill.GrossWeightInKilograms;

	public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeans());
	IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

	public IReadOnlyCollection<ICC044CConsignmentItem> ConsignmentItem => consignmentItem ?? (consignmentItem = GetConsignmentItems());
	IReadOnlyCollection<ICC044CConsignmentItem> consignmentItem;

	public IReadOnlyCollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocument());
	IReadOnlyCollection<ISupportingDocument> supportingDocument;

	public IReadOnlyCollection<IDocument> TransportDocument => transportDocument ?? (transportDocument = GetTransportDocument());
	IReadOnlyCollection<IDocument> transportDocument;

	public IReadOnlyCollection<IDocument> AdditionalReference => additionalReference ?? (additionalReference = GetAdditionalReference());
	IReadOnlyCollection<IDocument> additionalReference;

	IReadOnlyCollection<IDepartureTransportMeans> GetDepartureTransportMeans() => bill.ArrivalTransportInfos.Cast<ArrivalCusTransportMeans>()
		.Select((x, i) => new DepartureTransportMeansProvider(i + 1, x.TPM_TypeOfIdentification, x.TPM_IdentificationNumber, x.TPM_RN_NKTransportNationality))
		.ToArray();

	IReadOnlyCollection<ICC044CConsignmentItem> GetConsignmentItems() => bill.ArrivalGoodsItems
		.Select(x => new CC044CConsignmentItemProvider(x))
		.ToArray();

	IReadOnlyCollection<ISupportingDocument> GetSupportingDocument() => bill.SupportingDocuments
		.Select((x, i) => new SupportingDocumentProvider(i + 1, x, bill.IsInPhase5TransitionPeriod))
		.ToArray();

		IReadOnlyCollection<IDocument> GetTransportDocument() => bill.AdditionalDocuments
				.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument)
				.Select((x, i) => new TransportDocumentProvider(i + 1, x))
				.ToArray();

		IReadOnlyCollection<IDocument> GetAdditionalReference() => bill.AdditionalDocuments
				.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference)
				.Select((x, i) => new CC044CAdditionalReferenceProvider(i + 1, x))
				.ToArray();
}
