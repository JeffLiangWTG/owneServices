using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsCycleCountLocationValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var cycleCountLocation = Factory.New<WhsCycleCountLocation>();
			var validation = new TestWhsCycleCountLocationValidation(cycleCountLocation);

			var list = new string[]
			{
				WhsCycleCountLocationSchema.Constants.WCL_WL_Location,
				WhsCycleCountLocationSchema.Constants.WCL_WCL_RejectedCycleCount,
				WhsCycleCountLocationSchema.Constants.WCL_P9_Task,
			};

			foreach (var propertyInfo in cycleCountLocation.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
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

		#region TestWhsCycleCountLocationValidation

		class TestWhsCycleCountLocationValidation : WhsCycleCountLocationValidation
		{
			public TestWhsCycleCountLocationValidation(WhsCycleCountLocation parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
