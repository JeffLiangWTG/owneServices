using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC015CHouseConsignmentProvider : CC013015HouseConsignmentProvider, IHouseConsignment
{
	public CC015CHouseConsignmentProvider(int sequenceNumber, NctsBill bill, ICC015C cc015cProvider)
		: base(sequenceNumber, bill)
	{
		this.cc015cProvider = Argument.NotNull(cc015cProvider, nameof(cc015cProvider));
	}

	readonly ICC015C cc015cProvider;

	protected override IConsignment ParentConsignment => cc015cProvider.Consignment;

	protected override ITransitOperation TransitOperation => cc015cProvider.TransitOperation;
}
