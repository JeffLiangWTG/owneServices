using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLoadPkgPackagePivotValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var pivot = Factory.New<WhsLoadPkgPackagePivot>();
			var validation = new TestWhsLoadPkgPackagePivotValidation(pivot);

			var list = new string[]
			{
				WhsLoadPkgPackagePivotSchema.Constants.WLP_KP_Package,
				WhsLoadPkgPackagePivotSchema.Constants.WLP_WLO_Load
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

		#region TestWhsLoadPkgPackagePivotValidation

		class TestWhsLoadPkgPackagePivotValidation : WhsLoadPkgPackagePivotValidation
		{
			public TestWhsLoadPkgPackagePivotValidation(WhsLoadPkgPackagePivot parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
