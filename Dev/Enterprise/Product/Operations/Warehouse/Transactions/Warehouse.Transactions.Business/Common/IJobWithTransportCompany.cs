using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IJobWithTransportCompany
	{
		JobDocAddress TransportCoDocAddress { get; }
		bool GetTransportCoDocAddressReadOnly();

		ZString TransportCoName { get; }

		ZGuid TransportCoPK { get; set; }

		ZString TransportCoFieldType { get; }
		ZString TransportCoNameOrPK { get; set; }
		ZPropertyInfo TransportCoNameOrPKInfo { get; }
	}
}
