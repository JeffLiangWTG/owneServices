using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPackageAuditLineFailureCollection : ActiveBusinessObjectCollection<WhsPackageAuditLineFailure>
	{
		public WhsPackageAuditLineFailureCollection(WhsPackageAudit master)
			: base(master.Factory, master, null, WhsPackageAuditLineFailureSchema.WPF_WPA_WhsPackageAudit)
		{
		}
	}
}
