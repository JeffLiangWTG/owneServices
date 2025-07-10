using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TurkeyOrgCusCodeInfoTest : TestCaseWithFactory
	{
		string CountryCode => Core.Constants.CountryCodes.Turkey;

		public void TestCodesCannotCoexist()
		{
			var conflictingCodes1 = new ZString[] { TurkeyOrgCusCodeInfo.OrgCusCodes.VTE, TurkeyOrgCusCodeInfo.OrgCusCodes.VTC };
			var unconflictingCodes1 = new ZString[] { TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, TurkeyOrgCusCodeInfo.OrgCusCodes.VTP };

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode1 = orgHeader.CustomsCodes.AddNew();
			orgCusCode1.OK_RN_NKCodeCountry = CountryCode;

			var orgCusCode2 = orgHeader.CustomsCodes.AddNew();
			orgCusCode2.OK_RN_NKCodeCountry = CountryCode;

			AssertCodesCoexist(conflictingCodes1, unconflictingCodes1, "The following (TR) Registration codes cannot coexist: VTE, VTC");

			void AssertCodesCoexist(ZString[] conflictingCodes, ZString[] unconflictingCodes, ZString messageError)
			{
				foreach (var code in conflictingCodes)
				{
					orgCusCode1.OK_CodeType = code;

					foreach (var code2 in conflictingCodes)
					{
						orgCusCode2.OK_CodeType = code2;

						if (code != code2)
						{
							AssertHasError(orgCusCode2.OK_CodeTypeInfo, messageError);
						}
					}

					foreach (var code2 in unconflictingCodes)
					{
						orgCusCode2.OK_CodeType = code2;
						AssertNoError(orgCusCode2.OK_CodeTypeInfo, messageError);
					}

					orgCusCode2.OK_CodeType = OrgCusCode.AustraliaCodeTypes.ARN;
					AssertNoError(orgCusCode2.OK_CodeTypeInfo, messageError);
				}
			}
		}

		public void TestCodesCannotUnique()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "Org-1";

			var orgCusCode1 = orgHeader1.CustomsCodes.AddNew();
			orgCusCode1.OK_RN_NKCodeCountry = CountryCode;
			orgCusCode1.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.TCK;
			orgCusCode1.OK_CustomsRegNo = "55-00000001-8";

			Factory.Save();

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "Org-2";
			var orgCusCode2 = orgHeader2.CustomsCodes.AddNew();
			orgCusCode2.OK_RN_NKCodeCountry = CountryCode;
			orgCusCode2.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.TCK;
			orgCusCode2.OK_CustomsRegNo = "55000000018";

			AssertHasWarning(orgCusCode2.OK_CustomsRegNoInfo, "This Registration Number is already in use by at least one organization. The organizations are: Org-1 (Maximum of 10 organizations shown). Please check that the organizations are not the same.");

			orgCusCode1.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VTC;
			Factory.Save();

			orgCusCode2.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VTC;
			orgCusCode2.RunPreSaveValidation();

			AssertNoWarning(orgCusCode2.OK_CustomsRegNoInfo, "This Registration Number is already in use by at least one organization. The organizations are: Org-1 (Maximum of 10 organizations shown). Please check that the organizations are not the same.");

			orgCusCode1.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VTE;
			Factory.Save();

			orgCusCode2.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VTE;
			orgCusCode2.RunPreSaveValidation();

			AssertNoWarning(orgCusCode2.OK_CustomsRegNoInfo, "This Registration Number is already in use by at least one organization. The organizations are: Org-1 (Maximum of 10 organizations shown). Please check that the organizations are not the same.");

			orgCusCode1.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VDM;
			Factory.Save();

			orgCusCode2.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VDM;
			orgCusCode2.RunPreSaveValidation();

			AssertNoWarning(orgCusCode2.OK_CustomsRegNoInfo, "This Registration Number is already in use by at least one organization. The organizations are: Org-1 (Maximum of 10 organizations shown). Please check that the organizations are not the same.");
		}
	}
}
