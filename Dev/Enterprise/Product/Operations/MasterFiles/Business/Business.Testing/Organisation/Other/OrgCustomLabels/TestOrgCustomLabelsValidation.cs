using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestOrgCustomLabelsValidation : BusinessObjectValidationTestCase
	{
		public void TestMultiuseSameFieldName()
		{
			var emptyFilter = new ZQuery();
			var testFactory = new BusinessObjectFactory();
			var testOrg = testFactory.LoadTop1<OrgHeader>(emptyFilter);

			//refresh to update CustomLabels list
			testOrg.CustomLabels.RefreshBinding();

			OrgCustomLabels newCustomLabel = testOrg.CustomFormLabels.AddNew();
			newCustomLabel.OT_FieldName = "CusContainer.CustomAttribute1";
			newCustomLabel.OT_Caption = "Attribute1";

			OrgCustomLabels newCustomLabelCopy = testOrg.CustomFormLabels.AddNew();
			newCustomLabelCopy.OT_FieldName = "CusContainer.CustomAttribute1";
			newCustomLabelCopy.OT_Caption = "Attribute2";

			Assert("Error message for multiuse of same field name", newCustomLabelCopy.OT_FieldNameInfo.HasError("This field has already been entered."));
		}

		public void TestOT_Caption_NonWesternEuropeanCharacters()
		{
			var expectedError = "Caption only accepts Western European languages characters.";

			var listOfSupportedCustomFields = new ZString[]
			{
				Constants.CustomLabels.OrderLine.CustomAttribute1,
				Constants.CustomLabels.OrderLine.CustomAttribute2,
				Constants.CustomLabels.OrderLine.CustomAttribute3,
				Constants.CustomLabels.OrderLine.CustomAttribute4,
				Constants.CustomLabels.OrderLine.CustomAttribute5,
				Constants.CustomLabels.OrderLine.CustomAttribute6,
				Constants.CustomLabels.OrderLine.CustomTextBlob1,
			};

			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var customLabel = organisation.CustomLabels.AddNew();
			customLabel.OT_Caption = "你好";

			foreach (var supportedField in listOfSupportedCustomFields)
			{
				customLabel.OT_FieldName = supportedField;
				customLabel.Validation.ValidateOT_Caption();

				AssertNoError(
					"JobOrderLine custom fields should allow non Western-European characters",
					customLabel.OT_CaptionInfo,
					expectedError
				);
			}

			customLabel.OT_FieldName = Constants.CustomLabels.Order.CustomAttribute1;
			customLabel.Validation.ValidateOT_Caption();
			AssertHasError(
				"Not a JobOrderLine custom field, should not accept non Western-European characters",
				customLabel.OT_CaptionInfo,
				expectedError
			);

			customLabel.OT_FieldName = Constants.CustomLabels.OrderLine.CustomDate1;
			customLabel.Validation.ValidateOT_Caption();

			AssertHasError(
				"Is a JobOrderLine column but isn't a custom attribute, should not accept non Western-European characters",
				customLabel.OT_CaptionInfo,
				expectedError
			);
		}

		public void TestOT_Caption_Length()
		{
			var expectedError = "The Caption must be more than two characters.";
			var twoCharacterString = "你好";

			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var customLabel1 = organisation.CustomLabels.AddNew();
			customLabel1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute1;
			customLabel1.OT_Caption = twoCharacterString;

			var customLabel2 = organisation.CustomLabels.AddNew();
			customLabel2.OT_FieldName = Constants.CustomLabels.Order.CustomAttribute1;
			customLabel2.OT_Caption = twoCharacterString;

			customLabel1.Validation.ValidateOT_Caption();
			customLabel2.Validation.ValidateOT_Caption();

			AssertNoError(
				"Supports Non-Western European characters, should allow 1-2 characters as it can be meaningful in these languages",
				customLabel1.OT_CaptionInfo,
				expectedError
			);

			AssertHasError(
				"Doesn't support non Western-European characters so it should error if less than 3 characters",
				customLabel2.OT_CaptionInfo,
				expectedError
			);
		}
	}
}
