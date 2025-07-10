
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IOrgsAndWarehousesAccessProvider
	{
		ZGuid PK { get; }
		BusinessObjectFactory Factory { get; }
		GlbSecurityAllowedOrgsAndWarehousesView SecurityAllowedOrgsAndWarehousesView { get; }
		void AddSecurityToAccessOrgOrWarehouse(string code);
	}
}
