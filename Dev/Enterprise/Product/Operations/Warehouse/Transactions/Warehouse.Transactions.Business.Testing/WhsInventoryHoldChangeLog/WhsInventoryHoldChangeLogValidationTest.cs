using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInventoryHoldChangeLogValidationTest : BusinessObjectValidationTestCase
	{
		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var log = Factory.New<WhsInventoryHoldChangeLog>();
			var validation = new TestWhsInventoryHoldChangeLogValidation(log);

			var list = new string[]
			{
				WhsInventoryHoldChangeLogSchema.Constants.WHL_WE_ParentDocketLine,
				WhsInventoryHoldChangeLogSchema.Constants.WHL_WHC_NKCode
			};

			foreach (var propertyInfo in log.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
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

		#region TestWhsInventoryHoldChangeLogValidation

		class TestWhsInventoryHoldChangeLogValidation : WhsInventoryHoldChangeLogValidation
		{
			public TestWhsInventoryHoldChangeLogValidation(WhsInventoryHoldChangeLog parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
