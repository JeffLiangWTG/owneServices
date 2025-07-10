using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsProductStyleClassificationValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestWSS_Code

		public void TestWSS_Code_Enter()
		{
			var style = Helper.CreateProductStyle();
			var styleClassification = style.Classifications.AddNew();
			styleClassification.Validation.ValidateWSS_Code();
			AssertHasError(styleClassification.WSS_CodeInfo, "Please enter a Product Style Classification Code.");

			styleClassification.WSS_Code = "06";
			AssertNoErrors(styleClassification.WSS_CodeInfo);
		}

		public void TestCode_Duplicate()
		{
			var style = Helper.CreateProductStyle();
			var styleClassification = style.Classifications.AddNew();

			styleClassification.WSS_Code = "06";
			styleClassification.WSS_Description = "oo six";

			var styleClassification2 = style.Classifications.AddNew();
			styleClassification2.WSS_Code = "06";
			AssertHasError(styleClassification2.WSS_CodeInfo, "Product Style Classification 'oo six' is already using the Code '06'.");

			styleClassification2.WSS_Code = "14";
			AssertNoErrors(styleClassification2.WSS_CodeInfo);
		}

		#endregion

		#region TestWSS_Description

		public void TestWSS_Description()
		{
			var style = Helper.CreateProductStyle();
			var styleClassification = style.Classifications.AddNew();
			styleClassification.Validation.ValidateWSS_Description();
			AssertHasError(styleClassification.WSS_DescriptionInfo, "Please enter a Product Style Classification Description.");

			styleClassification.WSS_Description = "O Six";
			AssertNoErrors(styleClassification.WSS_DescriptionInfo);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var style = Helper.CreateProductStyle();
			var styleClassification = style.Classifications.AddNew();
			var validation = new TestWhsProductStyleClassificationValidation(styleClassification);

			foreach (var propertyInfo in styleClassification.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (propertyInfo.Name == WhsProductStyleClassificationSchema.Constants.WSS_WST_ProductStyle)
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

		#region TestWhsProductStyleClassificationValidation

		class TestWhsProductStyleClassificationValidation : WhsProductStyleClassificationValidation
		{
			public TestWhsProductStyleClassificationValidation(WhsProductStyleClassification parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
