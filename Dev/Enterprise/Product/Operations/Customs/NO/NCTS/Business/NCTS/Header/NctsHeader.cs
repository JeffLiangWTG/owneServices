using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.NCTS.Business;

public class NctsHeader : EU.NCTS.Business.NctsHeader,
	Integration.Customs.NO.ICusInBondHeader
{
	public NctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;
}
