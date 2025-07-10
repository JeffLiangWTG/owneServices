using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class ConsignmentType06Provider : INCTSConsignmentType06
{
	public ConsignmentType06Provider(NctsHeader nctsHeader)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
	}

	public decimal GrossMass => nctsHeader.ArrivalMovementHeader.TotalUnloadedGrossMassInKilograms;

	public IReadOnlyCollection<CargoWise.Customs.NL.MessageContracts.Interfaces.INCTSTransportEquipment> TransportEquipments => transportEquipments ??= nctsHeader.ArrivalHeaderContainers
				.Where(c => c.BC_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS })
					|| (c.BC_UnloadedState == NctsUnloadedStateList.Codes.DEC && c.Seals.Cast<CusSeal>().Any(s => s.BK_UnloadingState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS }))))
				.Select((x, index) => new CC044CTransportEquipmentProvider(x))
				.OrderBy(x => x.SequenceNumeric)
				.ToArray();
	IReadOnlyCollection<CargoWise.Customs.NL.MessageContracts.Interfaces.INCTSTransportEquipment> transportEquipments;

	public IReadOnlyCollection<INCTSDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ??=
			nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos
				.Where(t => t.TPM_TransportState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS }))
				.Select((t, index) => new CC044CDepartureTransportMeansProvider(t))
				.OrderBy(x => x.SequenceNumeric)
				.ToArray();
	IReadOnlyCollection<INCTSDepartureTransportMeans> departureTransportMeans;

	public IReadOnlyCollection<INCTSHouseConsignmentType05> HouseConsignments => houseConsignments ??= nctsHeader.Bills.Cast<NctsBill>().Where(b => b.UnloadedStatus == NctsUnloadedStateList.Codes.DIF || b.UnloadedStatus == NctsUnloadedStateList.Codes.MIS || b.UnloadedStatus == NctsUnloadedStateList.Codes.NEW).OrderBy(b => b.MovementDetail.B9_SeqNo).Select((bill) => new HouseConsignmentType05Provider(bill, bill.MovementDetail.B9_SeqNo)).ToArray();
	IReadOnlyCollection<INCTSHouseConsignmentType05> houseConsignments;

	public IReadOnlyCollection<INCTSSupportingDocument> SupportingDocuments => Array.Empty<INCTSSupportingDocument>();

	public IReadOnlyCollection<IDocument> TransportDocuments => Array.Empty<IDocument>();

	public IReadOnlyCollection<IDocument> AdditionalReferences => Array.Empty<IDocument>();

	readonly NctsHeader nctsHeader;
}
