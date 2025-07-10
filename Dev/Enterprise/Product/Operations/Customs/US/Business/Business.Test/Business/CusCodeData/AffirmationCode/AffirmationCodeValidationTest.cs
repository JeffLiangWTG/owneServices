using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AffirmationCodeValidationTest : TestCaseWithFactory
	{
		public void TestExpiredAffirmationCode()
		{
			var affirmationOfCompliance = Factory.New<USCAffirmationOfCompliance>();
			affirmationOfCompliance.UL_IsExpired = true;
			affirmationOfCompliance.UL_Code = "KNZ";
			var affirmationCode = Factory.New<AffirmationCode>();
			affirmationCode.CY_Code = "KNZ";
			AssertHasMessageError(affirmationCode.CY_CodeInfo, AffirmationCodeValidation.ExpiredAffirmationCode);
		}

		public void TestCheckCY_Code()
		{
			var affirmationCode = Factory.New<AffirmationCode>();
			affirmationCode.CY_Code = "";
			AssertHasMessageError(affirmationCode.CY_CodeInfo, AffirmationCodeValidation.InvalidAffirmationCode);
			affirmationCode.CY_Code = "AAA";
			AssertHasMessageError(affirmationCode.CY_CodeInfo, AffirmationCodeValidation.InvalidAffirmationCode);
			affirmationCode.CY_Code = "REG";
			AssertNoMessageError(affirmationCode.CY_CodeInfo, AffirmationCodeValidation.InvalidAffirmationCode);
		}

		public void TestNoDuplicateAffirmationCodes()
		{
			var fda = Factory.New<FDA>();
			var affirmationCode1 = Factory.New<AffirmationCode>();
			affirmationCode1.CY_ParentID = fda.PK;
			affirmationCode1.CY_ParentTableCode = fda.TablePrefix;
			affirmationCode1.CY_Code = "REG";
			AssertNoWarning(affirmationCode1.CY_CodeInfo, string.Format(AffirmationCodeValidation.DuplicateAffirmationCode, affirmationCode1.CY_Code));
			var affirmationCode2 = Factory.New<AffirmationCode>();
			affirmationCode2.CY_ParentID = fda.PK;
			affirmationCode2.CY_ParentTableCode = fda.TablePrefix;
			affirmationCode2.CY_Code = "REG";
			AssertHasWarning(affirmationCode2.CY_CodeInfo, string.Format(AffirmationCodeValidation.DuplicateAffirmationCode, affirmationCode2.CY_Code));
		}

		public void TestExcludedAffirmationCodes()
		{
			var fda = Factory.New<FDA>();
			var affirmationCode1 = Factory.New<AffirmationCode>();
			affirmationCode1.CY_ParentID = fda.PK;
			affirmationCode1.CY_ParentTableCode = fda.TablePrefix;
			affirmationCode1.CY_Code = AffirmationCodeConstants.Codes.SLN;
			AssertHasMessageError(affirmationCode1.CY_CodeInfo, string.Format(AffirmationCodeValidation.AffirmationCodeExcluded, affirmationCode1.CY_Code));
			var affirmationOfCompliance = Factory.New<USCAffirmationOfCompliance>();
			affirmationOfCompliance.UL_Code = AffirmationCodeConstants.Codes.TEM;
			affirmationOfCompliance.UL_Description = "Email Address";
			affirmationOfCompliance.UL_QualifierIndicator = true;
			var affirmationCode2 = Factory.New<AffirmationCode>();
			affirmationCode2.CY_ParentID = fda.PK;
			affirmationCode2.CY_ParentTableCode = fda.TablePrefix;
			affirmationCode2.CY_Code = AffirmationCodeConstants.Codes.TEM;
			AssertHasMessageError(affirmationCode2.CY_CodeInfo, string.Format(AffirmationCodeValidation.AffirmationCodeExcluded, affirmationCode2.CY_Code));
			affirmationOfCompliance = Factory.New<USCAffirmationOfCompliance>();
			affirmationOfCompliance.UL_Code = "REG";
			affirmationOfCompliance.UL_Description = "Test";
			affirmationOfCompliance.UL_QualifierIndicator = true;
			affirmationCode1.CY_Code = "REG";
			AssertNoMessageError(affirmationCode1.CY_CodeInfo, string.Format(AffirmationCodeValidation.AffirmationCodeExcluded, affirmationCode1.CY_Code));
		}

		public void TestCheckCY_Data()
		{
			var compliance = Factory.New<USCAffirmationOfCompliance>();
			compliance.UL_Code = "Z!Z";
			compliance.UL_QualifierIndicator = true;
			var fda = Factory.New<FDA>();
			var affirmationCode = Factory.New<AffirmationCode>();
			affirmationCode.CY_ParentID = fda.PK;
			affirmationCode.CY_ParentTableCode = fda.TablePrefix;
			affirmationCode.CY_Code = compliance.UL_Code;
			affirmationCode.CY_Data = ZString.Empty;
			AssertHasMessageError(affirmationCode.CY_DataInfo, string.Format(AffirmationCodeValidation.AffirmationCodeRequiresValue, affirmationCode.CY_Code));
			affirmationCode.CY_Data = "A123";
			AssertNoMessageError(affirmationCode.CY_DataInfo, string.Format(AffirmationCodeValidation.AffirmationCodeRequiresValue, affirmationCode.CY_Code));
			compliance.UL_QualifierIndicator = false;
			affirmationCode.CY_Data = ZString.Empty;
			AssertNoMessageError(affirmationCode.CY_DataInfo, string.Format(AffirmationCodeValidation.AffirmationCodeRequiresValue, affirmationCode.CY_Code));
		}

		public void TestCheckCY_DataWhenPMN()
		{
			var compliance = Factory.New<USCAffirmationOfCompliance>();
			compliance.UL_Code = ProductCodeQualifiersList.Codes.PMNNumber;
			compliance.UL_QualifierIndicator = true;
			var affirmationCode = Factory.New<AffirmationCode>();
			affirmationCode.CY_Code = compliance.UL_Code;
			affirmationCode.CY_Data = ZString.Empty;
			AssertHasMessageError(affirmationCode.CY_DataInfo, string.Format(AffirmationCodeValidation.AffirmationCodeRequiresValue, affirmationCode.CY_Code));
			affirmationCode.CY_Data = "S12345";
			AssertHasMessageError(affirmationCode.CY_DataInfo, AffirmationCodeValidation.PMNCodeRequiresValue);
			affirmationCode.CY_Data = "K12345A";
			AssertHasMessageError(affirmationCode.CY_DataInfo, AffirmationCodeValidation.PMNCodeRequiresValue);
			affirmationCode.CY_Data = "K123456";
			AssertNoMessageError(affirmationCode.CY_DataInfo, AffirmationCodeValidation.PMNCodeRequiresValue);
			affirmationCode.CY_Data = "DEN123456";
			AssertNoMessageError(affirmationCode.CY_DataInfo, AffirmationCodeValidation.PMNCodeRequiresValue);
			affirmationCode.CY_Data = "DEN12345A";
			AssertHasMessageError(affirmationCode.CY_DataInfo, AffirmationCodeValidation.PMNCodeRequiresValue);
		}
	}
}
