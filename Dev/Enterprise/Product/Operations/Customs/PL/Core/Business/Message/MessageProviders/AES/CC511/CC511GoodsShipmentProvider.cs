using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class CC511GoodsShipmentProvider : ICC511GoodsShipment
{
	public CC511GoodsShipmentProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	readonly CusEntryHeader entryHeader;

	public IConsignment Consignment => consignment ??= new ConsignmentProvider(entryHeader);
	IConsignment consignment;
}
