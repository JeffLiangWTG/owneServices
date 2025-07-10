using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbExternalPasswordValidationBaseOnlyTest : TestCaseWithFactory
	{
		public void TestValidateDuplicateConstraint()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "GC1";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "GC2";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "GB1";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			branch2.GB_Code = "GB2";

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "GG1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "GG2";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "GS1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "GS2";

			var passwordType1 = "ITB";
			var passwordType2 = "ESB";
			var userID1 = "same UserID1";
			var userID2 = "same UserID2";
			var sub1 = CreateGlbExternalPassword(company2.PK, branch2.PK, group2.PK, staff2.PK, passwordType2, userID1);
			sub1.Validation.ValidateDuplicateConstraint();
			AssertNoErrors(sub1);
			var sub2 = CreateGlbExternalPassword(company2.PK, branch2.PK, group2.PK, staff2.PK, passwordType2, userID2);
			sub2.Validation.ValidateDuplicateConstraint();
			AssertNoErrors(sub2);
			var messageError = "Duplicate credential found; there is already another GlbExternalPassword with the same data.";
			Factory.Save();
			Action<string, IZType, IZType, string, IZType> validateDuplicate = (string propertyName, IZType value1, IZType value2, string propertyName2, IZType property2Value) =>
			{
				var sub3 = CreateGlbExternalPassword(company2.PK, branch2.PK, group2.PK, staff2.PK, passwordType2, userID2);
				sub3.Validation.ValidateDuplicateConstraint();
				AssertHasRowError(sub3, messageError);
				sub1.Validation.ValidateDuplicateConstraint();
				AssertNoErrors(sub1);
				sub2.Validation.ValidateDuplicateConstraint();
				AssertNoErrors(sub2);

				var sub3PropertyInfo = sub3.ZPropertyInfoHash.GetPropertySafe(propertyName);
				sub3PropertyInfo.Value = value1;
				sub3.Validation.ValidateDuplicateConstraint();
				AssertHasRowError(sub3, messageError);
				sub1.Validation.ValidateDuplicateConstraint();
				AssertNoErrors(sub1);
				sub2.Validation.ValidateDuplicateConstraint();
				AssertNoErrors(sub2);
				var sub1PropertyInfo = sub1.ZPropertyInfoHash.GetPropertySafe(propertyName);
				sub1PropertyInfo.Value = value2;
				sub1.Validation.ValidateDuplicateConstraint();
				AssertHasRowError(sub1, messageError);

				var sub1Property2Info = sub1.ZPropertyInfoHash.GetPropertySafe(propertyName2);
				sub1Property2Info.Value = property2Value;
				sub1.Validation.ValidateAll();
				AssertNoErrors(sub1);
				sub3.Delete();
				Factory.Save();
			};

			validateDuplicate(GlbExternalPassword.Schema.GP_UserID, (ZString)userID1, (ZString)userID2, GlbExternalPassword.Schema.GP_PasswordType, (ZString)passwordType1);
			validateDuplicate(GlbExternalPassword.Schema.GP_PasswordType, (ZString)passwordType1, (ZString)passwordType2, GlbExternalPassword.Schema.GP_GS, staff1.PK);
			validateDuplicate(GlbExternalPassword.Schema.GP_GS, staff1.PK, staff2.PK, GlbExternalPassword.Schema.GP_GG, group1.PK);
			validateDuplicate(GlbExternalPassword.Schema.GP_GG, group1.PK, group2.PK, GlbExternalPassword.Schema.GP_GB, branch1.PK);
			validateDuplicate(GlbExternalPassword.Schema.GP_GB, branch1.PK, branch2.PK, GlbExternalPassword.Schema.GP_GC, company1.PK);
			validateDuplicate(GlbExternalPassword.Schema.GP_GC, company1.PK, company2.PK, GlbExternalPassword.Schema.GP_UserID, (ZString)userID1);
		}

		public void TestCurrentDecryptedPassword_WesternEuropeanRule()
		{
			var pw = Factory.New<GlbExternalPassword>();
			pw.GP_PasswordType = PasswordTypesList.Codes.AUB;
			pw.CurrentDecryptedPassword = ZString.Empty;
			AssertNoError(pw.CurrentDecryptedPasswordInfo, "Current Decrypted Password only accepts Western European languages characters.");

			pw.CurrentDecryptedPassword = "password";
			AssertNoError(pw.CurrentDecryptedPasswordInfo, "Current Decrypted Password only accepts Western European languages characters.");

			pw.CurrentDecryptedPassword = "ƤǠȜзшѳяԀ";
			AssertHasError(pw.CurrentDecryptedPasswordInfo, "Current Decrypted Password only accepts Western European languages characters.");
		}

		GlbExternalPassword CreateGlbExternalPassword(ZGuid gc, ZGuid gb, ZGuid gg, ZGuid gs, ZString passwordType, ZString userID)
		{
			var data = Factory.New<GlbExternalPassword>();
			data.GP_GC = gc;
			data.GP_GB = gb;
			data.GP_GG = gg;
			data.GP_GS = gs;
			data.GP_PasswordType = passwordType;
			data.GP_UserID = userID;
			return data;
		}
	}
}
