using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsProductStyleColourValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestWSC_Code

		public void TestWSC_Code()
		{
			var style = Helper.CreateProductStyle();
			var styleColour = style.Colours.AddNew();
			styleColour.Validation.ValidateWSC_Code();
			AssertHasError(styleColour.WSC_CodeInfo, "Please enter a Product Style Color Code.");

			styleColour.WSC_Code = "06";
			AssertNoErrors(styleColour.WSC_CodeInfo);

			styleColour.WSC_Description = "oo six";
			var styleColour2 = style.Colours.AddNew();
			styleColour2.WSC_Code = "06";
			AssertHasError(styleColour2.WSC_CodeInfo, "Product Style Color 'oo six' is already using the Code '06'.");

			styleColour2.WSC_Code = "14";
			AssertNoErrors(styleColour2.WSC_CodeInfo);
		}

		#endregion

		#region TestWSC_Description

		public void TestWSC_Description()
		{
			var style = Helper.CreateProductStyle();
			var styleColour = style.Colours.AddNew();
			styleColour.Validation.ValidateWSC_Description();
			AssertHasError(styleColour.WSC_DescriptionInfo, "Please enter a Product Style Color Description.");

			styleColour.WSC_Description = "O Six";
			AssertNoErrors(styleColour.WSC_DescriptionInfo);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var style = Helper.CreateProductStyle();
			var styleColour = style.Colours.AddNew();
			var validation = new TestWhsProductStyleColourValidation(styleColour);

			foreach (var propertyInfo in styleColour.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (propertyInfo.Name == WhsProductStyleColourSchema.Constants.WSC_WST_ProductStyle)
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

		#region TestWhsProductStyleColourValidation

		class TestWhsProductStyleColourValidation : WhsProductStyleColourValidation
		{
			public TestWhsProductStyleColourValidation(WhsProductStyleColour parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
