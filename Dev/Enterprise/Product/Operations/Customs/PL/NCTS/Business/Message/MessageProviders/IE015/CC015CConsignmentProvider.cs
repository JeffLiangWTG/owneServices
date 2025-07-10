using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC015CConsignmentProvider : CC013015ConsignmentProvider, IConsignment
{
	public CC015CConsignmentProvider(NctsDepartureMovementHeader movementHeader, ICC015C cc015cProvider)
		: base(movementHeader, Constants.MessageTypeCodes.IE015)
	{
		nctsHeader = Argument.NotNull(movementHeader.Header, $"{nameof(movementHeader)}.{nameof(NctsDepartureMovementHeader.Header)}");
		this.cc015cProvider = Argument.NotNull(cc015cProvider, nameof(cc015cProvider));
	}

	readonly ICC015C cc015cProvider;
	readonly NctsHeader nctsHeader;

	protected override bool HasCustomsOfficeOfTransitDeclared() => cc015cProvider.CustomsOfficeOfTransitDeclared.Count > 0;

	protected override IReadOnlyCollection<IHouseConsignment> GetHouseConsignments() => nctsHeader.Bills
		.OrderBy(x => x.SequenceNumber)
		.Select(x => new CC015CHouseConsignmentProvider(x.SequenceNumber, x, cc015cProvider))
		.ToArray();

	protected override ITransitOperation TransitOperation => cc015cProvider.TransitOperation;
}
