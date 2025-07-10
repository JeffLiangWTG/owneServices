using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC013CProvider : MessageHeaderProvider, ICC013C
{
	readonly NctsDepartureMovementHeader depHeader;
	public CC013CProvider(MessageSendingAction sendingAction) : base(Argument.NotNull(sendingAction, nameof(sendingAction)).Header)
	{
		depHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(depHeader));
	}

	public CC013CProvider(NctsHeader nctsHeader) : base(nctsHeader)
	{
		depHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(depHeader));
	}

	public ITransitOperation TransitOperation => transitOperation ??= new CC013CTransitOperationProvider(nctsHeader);
	ITransitOperation transitOperation;

	public IReadOnlyCollection<INCTSAuthorization> Authorisations => authorisations ??= depHeader.CusAuthorizationUsages
			.OrderBy(cau => cau.AGC_Code)
			.ThenBy(cau => cau.AGC_Number)
			.Select((cau, index) => new AuthorizationProvider(cau, index + 1)).ToArray();
	IReadOnlyCollection<INCTSAuthorization> authorisations;

	public string CustomsOfficeOfDeparture => depHeader.DepartureCustomsOfficeCode;

	public string CustomsOfficeOfDestination => depHeader.DestinationCustomsOfficeCodeForDeparture;

	public IReadOnlyCollection<ICustomsOfficeOfTransit> CustomsOfficesOfTransit => customsOfficesOfTransit ??= depHeader.TransitCustomsOfficeCodeList.Select((co, index) => new CustomsOfficesOfTransitProvider(co.OfficeCode, co.ArrivalTime, index + 1)).ToArray<ICustomsOfficeOfTransit>();
	IReadOnlyCollection<ICustomsOfficeOfTransit> customsOfficesOfTransit;

	public IReadOnlyCollection<ICustomsOfficeOfExitForTransit> CustomsOfficesOfExitForTransit => customsOfficesOfExitForTransit ??= depHeader.ExitForTransitCustomsOfficeCodeList.Select((co, index) => new CustomsOfficesOfExitForTransitProvider(co.OfficeCode, index + 1)).ToArray<ICustomsOfficeOfExitForTransit>();
	IReadOnlyCollection<ICustomsOfficeOfExitForTransit> customsOfficesOfExitForTransit;

	public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure => holderOfTheTransitProcedure ??= nctsHeader.Principal != null ? new HolderOfTheTransitProcedureProvider(nctsHeader.Principal) : null;
	IHolderOfTheTransitProcedure holderOfTheTransitProcedure;

	public INCTSRepresentative Representative => representative ??= RepresentativeProvider.New(depHeader.Representative);
	INCTSRepresentative representative;

	public IReadOnlyCollection<IGuarantee> Guarantees => guarantees ??= depHeader.Guarantees.Cast<NctsGuarantee>().Select((pw, index) => new GuaranteeProvider(pw, index + 1)).ToArray<IGuarantee>();
	IReadOnlyCollection<IGuarantee> guarantees;

	public INCTSConsignment Consignment => consignment ??= new CC013CConsignmentProvider(nctsHeader);
	INCTSConsignment consignment;

	public override string MessageType => NLConstants.WCoTypeCodes.Amendment;
}
