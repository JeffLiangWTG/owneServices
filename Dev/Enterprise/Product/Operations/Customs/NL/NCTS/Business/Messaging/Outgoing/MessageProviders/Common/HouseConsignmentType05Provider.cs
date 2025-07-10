using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class HouseConsignmentType05Provider : INCTSHouseConsignmentType05
{
	readonly NctsBill bill;
	public HouseConsignmentType05Provider(NctsBill bill, ZString sequenceNumber)
	{
		this.bill = Argument.NotNull(bill, nameof(bill));
		SequenceNumeric = int.TryParse(sequenceNumber, out int value) ? value : 0;
	}

	public int SequenceNumeric { get; }

	public decimal? GrossMass
	{
		get
		{
			decimal? result = decimal.Zero;
			var movementDetail = bill.MovementDetail;
			var unloadedState = movementDetail.B9_UnloadedState;
			if (unloadedState != NctsUnloadedStateList.Codes.DIF)
			{
				result = unloadedState == NctsUnloadedStateList.Codes.MIS ? null : bill.B0_Weight;
			}
			else
			{
				var differenceMoveDetail = movementDetail.DifferenceMoveDetail;
				if (differenceMoveDetail != null)
				{
					result = differenceMoveDetail.DifferenceWeight;
				}
			}
			return result;
		}
	}

	public IReadOnlyCollection<INCTSDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ??= bill.ArrivalTransportInfos
		.Where(t => t.TPM_TransportState == NctsUnloadedStateList.Codes.NEW || t.TPM_TransportState == NctsUnloadedStateList.Codes.MIS)
		.Select(t => new CC044CDepartureTransportMeansProvider(t))
		.ToArray<INCTSDepartureTransportMeans>();
	IReadOnlyCollection<INCTSDepartureTransportMeans> departureTransportMeans;

	public IReadOnlyCollection<INCTSSupportingDocument> SupportingDocuments => supportingDocuments ??= bill.SupportingDocuments
		.Where(s => s.CSI_Status != NctsUnloadedStateList.Codes.DEC)
		.Select(s => new SupportingDocumentProvider(s))
		.ToArray();
	IReadOnlyCollection<INCTSSupportingDocument> supportingDocuments;

	public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ??= bill.AdditionalDocuments
		.Where(t => t.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo && t.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && t.CSI_Status != NctsUnloadedStateList.Codes.DEC)
		.Select(t => new DocumentProvider(t))
		.ToArray();
	IReadOnlyCollection<IDocument> additionalReferences;

	public IReadOnlyCollection<INCTSConsignmentItemType05> ConsignmentItems => consignmentItems ??= bill.ArrivalGoodsItems
		.Where(i => i.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF || (i.BY_UnloadedState == NctsUnloadedStateList.Codes.MIS && bill.MovementDetail.B9_UnloadedState != NctsUnloadedStateList.Codes.MIS) || i.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW)
		.Select(i => new ConsignmentItemType05Provider(i))
		.OrderBy(x => x.GoodsItemNumber)
		.ToArray();
	IReadOnlyCollection<INCTSConsignmentItemType05> consignmentItems;

	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??= bill.AdditionalDocuments
		.Where(t => t.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo && t.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument && t.CSI_Status != NctsUnloadedStateList.Codes.DEC)
		.Select(t => new DocumentProvider(t))
		.ToArray();
	IReadOnlyCollection<IDocument> transportDocuments;
}
