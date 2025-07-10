using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GermanValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidateEBS()
		{
			dummyBO.Z0_Description = "ZB01";
			GermanValidationHelper.ValidateEBS(dummyBO.Z0_DescriptionInfo);
			AssertHasMessageError(dummyBO.Z0_DescriptionInfo, "EORI Branch Suffix should be 4 digits and different to the EORI code, e.g. 0001");
			dummyBO.Z0_Description = "00001";
			GermanValidationHelper.ValidateEBS(dummyBO.Z0_DescriptionInfo);
			AssertHasMessageError(dummyBO.Z0_DescriptionInfo, "EORI Branch Suffix should be 4 digits and different to the EORI code, e.g. 0001");
			dummyBO.Z0_Description = "0002";
			GermanValidationHelper.ValidateEBS(dummyBO.Z0_DescriptionInfo);
			AssertNoMessageError(dummyBO.Z0_DescriptionInfo, "EORI Branch Suffix should be 4 digits and different to the EORI code, e.g. 0001");
			Assert(ErrorReporter.LastMessageReported.StartsWith("Attempt to change validation on a property info outside of its Check method"));
			ErrorReporter.Clear();
		}

		public void TestValidateDBI()
		{
			dummyBO.Z0_Description = "123456789012345";
			GermanValidationHelper.ValidateDBI(dummyBO.Z0_DescriptionInfo);
			AssertHasError(dummyBO.Z0_DescriptionInfo, "The maximum length is 10 characters.");
			dummyBO.Z0_Description = "1234";
			GermanValidationHelper.ValidateDBI(dummyBO.Z0_DescriptionInfo);
			AssertNoError(dummyBO.Z0_DescriptionInfo, "The maximum length is 10 characters.");
			Assert(ErrorReporter.LastMessageReported.StartsWith("Attempt to change validation on a property info outside of its Check method"));
			ErrorReporter.Clear();
		}

		public void TestValidateUST()
		{
			var message = "VAT Business Registration Number structures for Germany are either 'DE999999999' or '999999999'.";
			AssertValidationForCode("123456789", "123456789", message, () => GermanValidationHelper.ValidateUST(dummyBO.Z0_DescriptionInfo), "A");
			AssertValidationForCode("DE123456789", "DE123456789", message, () => GermanValidationHelper.ValidateUST(dummyBO.Z0_DescriptionInfo), "A");
		}

		public void TestValidateCWA()
		{
			AssertValidationForCode("1234567890", "123456789012345678901234567890123", "Customs Warehouse Procedure Authorization code should be 10-33 characters long.", () => GermanValidationHelper.ValidateCWA(dummyBO.Z0_DescriptionInfo));
		}

		public void TestValidationLCO()
		{
			AssertValidationForCode("123456789012", "12345678901234567890123456789012345", "Local Clearance Outward Processing code should be 12-35 characters long.", () => GermanValidationHelper.ValidateLCO(dummyBO.Z0_DescriptionInfo));
		}

		public void TestValidationOPR()
		{
			AssertValidationForCode("123456789012", "12345678901234567890123456789012345", "Outward Processing code should be 12-35 characters long.", () => GermanValidationHelper.ValidateOPR(dummyBO.Z0_DescriptionInfo));
		}

		public void TestValidationAEX()
		{
			AssertValidationForCode("123456789012", "12345678901234567890123456789012345", "Accredited Exporter code should be 12-35 characters long.", () => GermanValidationHelper.ValidateAEX(dummyBO.Z0_DescriptionInfo));
		}

		public void TestValidateTAO()
		{
			AssertValidationForCode("1000", "9999", "TAO value must be a number between 1000 and 9999.", () => GermanValidationHelper.ValidateTAO(dummyBO.Z0_DescriptionInfo), "A");
		}

		public void TestValidateIMA()
		{
			CombineAssertions(() =>
			{
				var message = "IMA Code must contain 5 characters.";
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "DDE";

				var code = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.IMA, "123456789012345", CountryCodes.Germany);
				AssertHasMessageError("RegNo contains more than 5 characters", code.OK_CustomsRegNoInfo, message);

				code.OK_CustomsRegNo = "1234";
				AssertHasMessageError("RegNo contains more than 5 characters", code.OK_CustomsRegNoInfo, message);

				code.OK_CustomsRegNo = "12345";
				AssertNoMessageError("RegNo contains more than 5 characters", code.OK_CustomsRegNoInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			dummyBO = Factory.New<DummyBusinessObject>();
		}
		DummyBusinessObject dummyBO;

		void AssertValidationForCode(ZString minimumValue, ZString maximumValue, ZString warningMessage, Action validator) => AssertValidationForCode(minimumValue, maximumValue, warningMessage, validator, ZString.Empty);

		void AssertValidationForCode(ZString minimumValue, ZString maximumValue, ZString warningMessage, Action validator, ZString invalidStringToAdd)
		{
			dummyBO.Z0_Description = minimumValue.Left(minimumValue.Length - 1);
			validator.Invoke();
			AssertHasWarning(dummyBO.Z0_DescriptionInfo, warningMessage);
			dummyBO.Z0_Description = minimumValue;
			validator.Invoke();
			AssertNoWarning(dummyBO.Z0_DescriptionInfo, warningMessage);
			dummyBO.Z0_Description = maximumValue + "1";
			validator.Invoke();
			AssertHasWarning(dummyBO.Z0_DescriptionInfo, warningMessage);
			dummyBO.Z0_Description = maximumValue;
			validator.Invoke();
			AssertNoWarning(dummyBO.Z0_DescriptionInfo, warningMessage);
			if (!invalidStringToAdd.IsEmpty)
			{
				dummyBO.Z0_Description = minimumValue + invalidStringToAdd;
				validator.Invoke();
				AssertHasWarning(dummyBO.Z0_DescriptionInfo, warningMessage);
			}
			Assert(ErrorReporter.LastMessageReported.StartsWith("Attempt to change validation on a property info outside of its Check method"));
			ErrorReporter.Clear();
		}
	}
}
