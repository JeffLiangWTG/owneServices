using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsStocktakeProductFilterValidationTest : BusinessObjectValidationTestCase
	{
		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var filter = Factory.New<WhsStocktakeProductFilter>();
			var validation = new TestWhsStocktakeProductFilterValidation(filter);

			foreach (var propertyInfo in filter.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (propertyInfo.Name == WhsStocktakeProductFilterSchema.Constants.WSP_WS_Stocktake)
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

		#region TestWhsStocktakeProductFilterValidation

		class TestWhsStocktakeProductFilterValidation : WhsStocktakeProductFilterValidation
		{
			public TestWhsStocktakeProductFilterValidation(WhsStocktakeProductFilter parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
