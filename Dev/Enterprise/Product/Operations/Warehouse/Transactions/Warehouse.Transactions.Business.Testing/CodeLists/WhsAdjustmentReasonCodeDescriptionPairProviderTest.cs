using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsAdjustmentReasonCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		#region TestIsReturningCorrectCollection

		public override void TestIsReturningCorrectCollection()
		{
			var generatedList = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			var expectedList = WarehouseDataRegistry.Instance.AdjustmentReasonCodes.Value.GetCodeDescriptionPairList();
			AssertEquals(expectedList.Count, generatedList.Count);
			AssertEquals(expectedList.CodesAsString, generatedList.CodesAsString);
		}

		#endregion

		#region Implementation

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new WhsAdjustmentReasonCodeDescriptionPairProvider();
		}

		#endregion
	}
}
