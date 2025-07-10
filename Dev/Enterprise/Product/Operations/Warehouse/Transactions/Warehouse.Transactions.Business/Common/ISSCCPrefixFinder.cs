using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ISSCCPrefixFinder
	{
		ZString GetSSCCPrefix(SSCCGenerationContext context, Func<OrgHeader> getClient, Func<WhsWarehouse> getWarehouse, INotifications notify, bool shouldPrompt);
	}
}
