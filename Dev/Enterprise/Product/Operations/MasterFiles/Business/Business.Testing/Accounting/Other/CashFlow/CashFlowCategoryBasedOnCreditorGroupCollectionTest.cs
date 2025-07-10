using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnCreditorGroupCollection))]
	sealed class CashFlowCategoryBasedOnCreditorGroupCollectionTest : CashFlowCategoryBasedOnOrgGroupCollectionTest<CashFlowCategoryBasedOnCreditorGroup, CashFlowCategoryBasedOnCreditorGroupCollection>
	{
	}
}
