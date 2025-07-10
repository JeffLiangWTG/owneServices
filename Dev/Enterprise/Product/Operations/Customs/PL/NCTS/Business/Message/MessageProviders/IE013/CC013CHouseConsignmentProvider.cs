using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC013CHouseConsignmentProvider : CC013015HouseConsignmentProvider, IHouseConsignment
{
	public CC013CHouseConsignmentProvider(int sequenceNumber, NctsBill bill, ICC013C cc013cProvider)
		: base(sequenceNumber, bill)
	{
		this.cc013cProvider = Argument.NotNull(cc013cProvider, nameof(cc013cProvider));
	}

	readonly ICC013C cc013cProvider;

	protected override IConsignment ParentConsignment => cc013cProvider.Consignment;

	protected override ITransitOperation TransitOperation => cc013cProvider.TransitOperation;
}
