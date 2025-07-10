using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsCycleCountLocationVarianceValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var variance = Factory.New<WhsCycleCountLocationVariance>();
			var validation = new TestWhsCycleCountLocationVarianceValidation(variance);

			var list = new string[]
			{
				WhsCycleCountLocationVarianceSchema.Constants.WCC_WCL_CycleCountLocation,
				WhsCycleCountLocationVarianceSchema.Constants.WCC_WL_ExpectedStockLocation
			};

			foreach (var propertyInfo in variance.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
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

		#region TestWhsCycleCountLocationVarianceValidation

		class TestWhsCycleCountLocationVarianceValidation : WhsCycleCountLocationVarianceValidation
		{
			public TestWhsCycleCountLocationVarianceValidation(WhsCycleCountLocationVariance parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
