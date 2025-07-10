using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public interface ITransportationUnitForApportioning
	{
		ZGuid CreditorPK { get; }

		IEnumerable<IJobInvoicingPlugIn> Consignments { get; }

		IEnumerable<ZString> DefaultChargeGroups { get; }

		ZString MasterBillNum { get; }

		ZDateTime ETA { get; }

		ZDateTime ETD { get; }
	}
}
