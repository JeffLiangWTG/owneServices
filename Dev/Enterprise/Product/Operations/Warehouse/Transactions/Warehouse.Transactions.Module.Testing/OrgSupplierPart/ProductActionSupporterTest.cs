using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ProductActionSupporter))]
	public class ProductActionSupporterTest : OrgSupplierPartActionSupporterTest<ProductActionSupporter>
	{
		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigProduct;
	}
}
