using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.NCTS.Business;

public class DepartureHeaderProvider : MessageHeaderProvider, INCTSDepartureHeader
{
	public DepartureHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
	{
		departureHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(departureHeader));
	}
	readonly NctsDepartureMovementHeader departureHeader;

	public virtual ITransitOperation TransitOperation => CachedValueHelper.GetValue(ref transitOperation, () => new TransitOperationProvider(nctsHeader));
	CachedValue<ITransitOperation> transitOperation;

	public IReadOnlyCollection<INCTSAuthorization> Authorisations => authorisations ??= GetAuthorisations(nctsHeader);
	IReadOnlyCollection<INCTSAuthorization> authorisations;

	public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure => CachedValueHelper.GetValue(ref holderOfTheTransitProcedure, () => HolderOfTheTransitProcedureProvider.New(nctsHeader.Principal));
	CachedValue<IHolderOfTheTransitProcedure> holderOfTheTransitProcedure;

	public string CustomsOfficeOfDeparture => nctsHeader.CommonMovementHeader.DepartureCustomsOfficeCode;

	public string CustomsOfficeOfDestination => nctsHeader.CommonMovementHeader.DestinationCustomsOfficeCode;

	public IReadOnlyCollection<ICustomsOfficeOfTransit> CustomsOfficesOfTransit => customsOfficesOfTransit ??= GetCustomsOfficesOfTransit(departureHeader);
	IReadOnlyCollection<ICustomsOfficeOfTransit> customsOfficesOfTransit;

	public IReadOnlyCollection<ICustomsOfficeOfExitForTransit> CustomsOfficesOfExitForTransit => customsOfficesOfExitForTransit ??= GetCustomsOfficesOfExitForTransit(departureHeader);
	IReadOnlyCollection<ICustomsOfficeOfExitForTransit> customsOfficesOfExitForTransit;

	public INCTSRepresentative Representative => CachedValueHelper.GetValue(ref representative, () => RepresentativeProvider.New(nctsHeader.MovementHeader.Representative));
	CachedValue<INCTSRepresentative> representative;

	public IReadOnlyCollection<IGuarantee> Guarantees => guarantees ?? (guarantees = nctsHeader.MovementHeader.Guarantees.Cast<NctsGuarantee>().Select((pw, index) => new GuaranteeProvider(pw, index + 1)).ToArray<IGuarantee>());
	IReadOnlyCollection<IGuarantee> guarantees;

	public INCTSConsignment Consignment => consignment ??= ConsignmentCore;
	INCTSConsignment consignment;
	protected virtual INCTSConsignment ConsignmentCore => new ConsignmentProvider(nctsHeader);

	static IReadOnlyCollection<INCTSAuthorization> GetAuthorisations(NctsHeader nctsHeader)
	{
		return (nctsHeader.IsPhase5Departure
			? (IBusinessObjectCollection<CusAuthorizationUsage>)nctsHeader.MovementHeader.CusAuthorizationUsages
			: nctsHeader.CusAuthorizationUsages)
				.OrderBy(cau => cau.AGC_Code)
				.ThenBy(cau => cau.AGC_Number)
				.Select((cau, index) => new AuthorizationProvider(cau, index + 1))
				.ToArray();
	}

	static IReadOnlyCollection<ICustomsOfficeOfTransit> GetCustomsOfficesOfTransit(NctsDepartureMovementHeader departureHeader)
	{
		return departureHeader.TransitCustomsOfficeCodeList
			.Select((co, index) => new CustomsOfficesOfTransitProvider(co.OfficeCode, co.ArrivalTime, index + 1))
			.ToArray<ICustomsOfficeOfTransit>();
	}

	static IReadOnlyCollection<ICustomsOfficeOfExitForTransit> GetCustomsOfficesOfExitForTransit(NctsDepartureMovementHeader departureHeader)
	{
		return departureHeader.ExitForTransitCustomsOfficeCodeList
			.Select((co, index) => new CustomsOfficesOfExitForTransitProvider(co.OfficeCode, index + 1))
			.ToArray<ICustomsOfficeOfExitForTransit>();
	}
}
