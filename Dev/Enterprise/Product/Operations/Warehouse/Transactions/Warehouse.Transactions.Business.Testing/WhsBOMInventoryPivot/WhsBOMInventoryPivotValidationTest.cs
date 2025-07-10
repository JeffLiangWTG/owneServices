using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsBOMInventoryPivotValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var pivot = Factory.New<WhsBOMInventoryPivot>();
			var validation = new TestWhsBOMInventoryPivotValidation(pivot);

			var list = new string[]
			{
				WhsBOMInventoryPivotSchema.Constants.WIP_WE_ComponentLine,
				WhsBOMInventoryPivotSchema.Constants.WIP_WE_InventoryLine
			};

			foreach (var propertyInfo in pivot.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsBOMInventoryPivotValidation

		class TestWhsBOMInventoryPivotValidation : WhsBOMInventoryPivotValidation
		{
			public TestWhsBOMInventoryPivotValidation(WhsBOMInventoryPivot parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
