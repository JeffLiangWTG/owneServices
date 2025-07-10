using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public interface IReceiveLinesUpdater
	{
		ZDecimal UpdateReceiveLinesAndReturnExcessUnloadQty(WhsReceive receive, List<WhsReceiveLine> receiveLines, decimal unloadQty, string packUQ, ZGuid locationPK, string palletId, bool isDirectPutawayLocation);
	}
}
