using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC170CConsignmentProvider : ConsignmentProviderBase, ICC170CConsignment
{
	public CC170CConsignmentProvider(NctsDepartureMovementHeader movementHeader, ICC170C rootProvider) : base(movementHeader)
	{
		nctsHeader = Argument.NotNull(movementHeader.Header, $"{nameof(NctsDepartureMovementHeader)}.{nameof(NctsDepartureMovementHeader.Header)}");
		departureRootProvider = Argument.NotNull(rootProvider, nameof(rootProvider));
		this.movementHeader = movementHeader;
	}

	readonly ICC170C departureRootProvider;
	readonly NctsDepartureMovementHeader movementHeader;
	readonly NctsHeader nctsHeader;

	public NCTSIndicator? ContainerIndicator => nctsHeader.DepartureHeaderContainers.Any(x => x.BC_ContainerNum != ZString.Empty && x.IsContainerised) ? NCTSIndicator.YES : NCTSIndicator.NO;

	public string InlandModeOfTransport => movementHeader.BM_InlandTransportMode;

	public string ModeOfTransportAtTheBorder => movementHeader.BM_ExportTransportMode;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipment => transportEquipment ??= GetTransportEquipment();
	IReadOnlyCollection<ITransportEquipment> transportEquipment;

	public ILocationOfGoods LocationOfGoods => locationOfGoods ??= new LocationOfGoodsProvider(movementHeader.GoodsLocation);
	ILocationOfGoods locationOfGoods;

	public IReadOnlyCollection<IActiveBorderTransportMeansType> ActiveBorderTransportMeans => activeBorderTransportMeans ??=
		movementHeader.BM_ExportTransportMode != ModeOfTransportList.Codes._5_PostalConsignment ? GetActiveBorderTransportMeans() : null;
	IReadOnlyCollection<IActiveBorderTransportMeansType> activeBorderTransportMeans;

	public IPlaceOfLoadingOrUnloading PlaceOfLoading => CachedValueHelper.GetValue(ref placeOfLoading, () => PlaceOfLoadingOrUnloadingProvider.NewOrNull(movementHeader.BM_PortOfPresentationCode, movementHeader.BM_PlaceOfLoading, movementHeader.Factory, InPhase5TransitionPeriod));
	CachedValue<IPlaceOfLoadingOrUnloading> placeOfLoading;

	public IReadOnlyCollection<ICC170CHouseConsignment> HouseConsignment => houseConsignment ??= GetHouseConsignments();
	IReadOnlyCollection<ICC170CHouseConsignment> houseConsignment;

	IActiveBorderTransportMeansType[] GetActiveBorderTransportMeans()
		=> new IActiveBorderTransportMeansType[] { new ActiveBorderTransportMeansTypeProvider(1, movementHeader) }
			.Concat(movementHeader.AdditionalTransportAtBorderList.Select((x, i) => new AdditionalBorderTransportMeansTypeProvider(i + 2, x, movementHeader)))
			.ToArray();

	IReadOnlyCollection<ITransportEquipment> GetTransportEquipment() => nctsHeader.DepartureHeaderContainers
		.Select((x, i) => new TransportEquipmentForDepartureProvider(i + 1, x, MessageTypeCodes.IE170))
		.ToArray();

	IReadOnlyCollection<ICC170CHouseConsignment> GetHouseConsignments() => nctsHeader.Bills
		.Select((x, i) => new CC170CHouseConsignmentProvider(i + 1, x, departureRootProvider))
		.ToArray();

	protected override string GetDepartureTransportMeansTransportMode() => movementHeader.BM_InlandTransportMode;
}
