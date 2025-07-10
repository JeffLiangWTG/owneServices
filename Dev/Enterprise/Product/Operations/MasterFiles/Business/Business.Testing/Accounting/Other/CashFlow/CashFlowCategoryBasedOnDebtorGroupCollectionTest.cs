using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnDebtorGroupCollection))]
	sealed class CashFlowCategoryBasedOnDebtorGroupCollectionTest : CashFlowCategoryBasedOnOrgGroupCollectionTest<CashFlowCategoryBasedOnDebtorGroup, CashFlowCategoryBasedOnDebtorGroupCollection>
	{
	}
}
