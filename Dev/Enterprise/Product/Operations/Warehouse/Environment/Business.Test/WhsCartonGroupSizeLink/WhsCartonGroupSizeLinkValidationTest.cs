using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Testing
{
	class WhsCartonGroupSizeLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckWCV_OptimizationCost()
		{
			var link = Factory.New<WhsCartonGroupSizeLink>();
			AssertNoErrors(link);

			link.WCV_OptimizationCost = -1;
			AssertHasError(link.WCV_OptimizationCostInfo, "Please enter an 'Optimization Cost' greater than 0.");

			link.WCV_OptimizationCost = 1;
			AssertNoErrors(link);

			link.WCV_OptimizationCost = 0;
			AssertHasError(link.WCV_OptimizationCostInfo, "Please enter an 'Optimization Cost' greater than 0.");
		}

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var link = Factory.New<WhsCartonGroupSizeLink>();
			var validation = new TestWhsCartonGroupSizeLinkValidation(link);

			var list = new string[]
			{
				WhsCartonGroupSizeLinkSchema.Constants.WCV_WCG,
				WhsCartonGroupSizeLinkSchema.Constants.WCV_WCS
			};

			foreach (var propertyInfo in link.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
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

		#region TestWhsCartonGroupSizeLinkValidation

		class TestWhsCartonGroupSizeLinkValidation : WhsCartonGroupSizeLinkValidation
		{
			public TestWhsCartonGroupSizeLinkValidation(WhsCartonGroupSizeLink parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
