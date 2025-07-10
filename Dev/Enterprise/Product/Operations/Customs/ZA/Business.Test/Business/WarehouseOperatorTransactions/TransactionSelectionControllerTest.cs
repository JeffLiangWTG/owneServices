using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.OperationalActions.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	abstract class TransactionSelectionControllerTest : NonPersistentBusinessObjectTestCase
	{
		public abstract void TestProcessSelectedRecords();

		protected ZGuid WarehousePK { get; private set; }

		protected ZGuid ProductOwnerPK { get; private set; }

		public void TestSelectAllRecords()
		{
			var controller = (TransactionSelectionController)GetNewBusinessObject();
			var recordCount = controller.Records.Count;

			AssertGreaterThan(recordCount, 0);

			controller.SelectAll = true;
			AssertEquals(recordCount, controller.SelectedRecords.Count);

			controller.SelectAll = false;
			AssertEquals(0, controller.SelectedRecords.Count);
		}

		protected override void SetUp()
		{
			(_, ProductOwnerPK, WarehousePK, _) = ExportApplicatorTestHelper.SetupTestDataForRecordFilterTesting(Factory);
		}
	}
}
