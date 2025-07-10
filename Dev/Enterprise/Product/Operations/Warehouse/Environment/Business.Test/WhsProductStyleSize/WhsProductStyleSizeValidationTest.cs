using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsProductStyleSizeValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWSZ_Size

		public void TestCheckWSZ_Size()
		{
			var style = Helper.CreateProductStyle();
			var styleSize1 = style.Sizes.AddNew();
			styleSize1.Validation.ValidateWSZ_Size();
			AssertHasError(styleSize1.WSZ_SizeInfo, "Please enter a Product Style Size.");

			styleSize1.WSZ_Size = "SMALL";
			AssertNoErrors(styleSize1.WSZ_SizeInfo);

			var styleSize2 = style.Sizes.AddNew();
			styleSize2.WSZ_Size = "SMALL";
			AssertHasError(styleSize2.WSZ_SizeInfo, "Product Style Size 'SMALL' is already on the list.");

			styleSize2.WSZ_Size = "LARGE";
			AssertNoErrors(styleSize2.WSZ_SizeInfo);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var style = Helper.CreateProductStyle();
			var styleSize1 = style.Sizes.AddNew();
			var validation = new TestWhsProductStyleSizeValidation(styleSize1);

			foreach (var propertyInfo in styleSize1.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (propertyInfo.Name == WhsProductStyleSizeSchema.Constants.WSZ_WST_ProductStyle)
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

		#region TestWhsProductStyleSizeValidation

		class TestWhsProductStyleSizeValidation : WhsProductStyleSizeValidation
		{
			public TestWhsProductStyleSizeValidation(WhsProductStyleSize parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
