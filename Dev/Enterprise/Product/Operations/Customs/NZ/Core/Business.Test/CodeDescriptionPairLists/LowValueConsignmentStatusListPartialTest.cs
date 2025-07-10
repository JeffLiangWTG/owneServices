
namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	class LowValueConsignmentStatusListTest : TestCaseWithFactory
	{
		public void TestCanDelete()
		{
			Assert(LowValueConsignmentStatusList.CanDelete(LowValueConsignmentStatusList.Codes.NotSentToCustoms));
			Assert(LowValueConsignmentStatusList.CanDelete(LowValueConsignmentStatusList.Codes.ConsignmentCancelled));
		}

		public void TestStatusIsImpediment()
		{
			Assert(LowValueConsignmentStatusList.LastStatusIsImpediment(LowValueConsignmentStatusList.Codes.ConsignmentHeld));
			Assert(LowValueConsignmentStatusList.LastStatusIsImpediment(LowValueConsignmentStatusList.Codes.RescindPreviousStatusNotification));
			Assert(LowValueConsignmentStatusList.LastStatusIsImpediment(LowValueConsignmentStatusList.Codes.ImportDeclarationRequired));
			Assert(LowValueConsignmentStatusList.LastStatusIsImpediment(LowValueConsignmentStatusList.Codes.ConsolidationIcrRequired));
			Assert(LowValueConsignmentStatusList.LastStatusIsImpediment(LowValueConsignmentStatusList.Codes.MpiImportDecRequired));
			AssertEquals(false, LowValueConsignmentStatusList.LastStatusIsImpediment(LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff));
		}
	}
}
