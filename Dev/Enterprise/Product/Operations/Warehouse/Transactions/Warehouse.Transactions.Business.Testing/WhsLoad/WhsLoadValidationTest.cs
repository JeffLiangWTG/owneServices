using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLoadValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var load = Factory.New<WhsLoad>();
			var validation = new TestWhsLoadValidation(load);

			var list = new string[]
			{
				WhsLoadSchema.Constants.WLO_WL_PlannedDockDoor,
				WhsLoadSchema.Constants.WLO_PL_NKCarrierServiceLevel
			};

			foreach (var propertyInfo in load.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
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

		#region TestWhsLoadValidation

		class TestWhsLoadValidation : WhsLoadValidation
		{
			public TestWhsLoadValidation(WhsLoad parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
