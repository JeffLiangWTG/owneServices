using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using static Enterprise.Customs.PL.NCTS.Business.Constants;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class CC013CConsignmentProvider : CC013015ConsignmentProvider, IConsignment
{
	public CC013CConsignmentProvider(NctsDepartureMovementHeader movementHeader, ICC013C cc013cProvider) : base(movementHeader, MessageTypeCodes.IE013)
	{
		nctsHeader = Argument.NotNull(movementHeader.Header, $"{nameof(NctsDepartureMovementHeader)}.{nameof(NctsDepartureMovementHeader.Header)}");
		this.cc013cProvider = Argument.NotNull(cc013cProvider, nameof(cc013cProvider));
	}

	readonly ICC013C cc013cProvider;
	readonly NctsHeader nctsHeader;

	protected override bool HasCustomsOfficeOfTransitDeclared() => cc013cProvider.CustomsOfficeOfTransitDeclared.Count > 0;

	protected override IReadOnlyCollection<IHouseConsignment> GetHouseConsignments() => nctsHeader.Bills
		.OrderBy(x => x.SequenceNumber)
		.Select(x => new CC013CHouseConsignmentProvider(x.SequenceNumber, x, cc013cProvider))
		.ToArray();

	protected override ITransitOperation TransitOperation => cc013cProvider.TransitOperation;
}
