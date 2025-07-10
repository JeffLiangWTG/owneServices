using Enterprise.Environment;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ProductActionSupporter : OrgSupplierPartActionSupporter
	{
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsConfigProduct;
	}
}