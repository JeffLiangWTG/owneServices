using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ArgentinaOrgCusCodeInfoTest : TestCaseWithFactory
	{
		string CountryCode => Core.Constants.CountryCodes.Argentina;

		public void TestCodesCannotUnique()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCode);

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "ARGENTINA-1";

			var regNumber1 = new OrgRegistrationNumber(orgHeader1);
			regNumber1.NumberTypeForDisplay = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			regNumber1.Number = "55-00000001-8";

			Factory.Save();

			var regNumber2 = new OrgRegistrationNumber(Factory.New<OrgHeader>());
			regNumber2.Organization.OH_Code = "ARGENTINA-2";
			regNumber2.NumberTypeForDisplay = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			regNumber2.Number = "55-00000001-8";

			AssertHasWarning(regNumber2.NumberInfo, "This Registration Number is already in use by at least one organization. The organizations are: ARGENTINA-1 (Maximum of 10 organizations shown). Please check that the organizations are not the same.");

			regNumber1.NumberTypeForDisplay = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF;
			Factory.Save();

			regNumber2.NumberTypeForDisplay = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF;
			regNumber2.RunPreSaveValidation();

			AssertNoWarning(regNumber2.NumberInfo, "This Registration Number is already in use by at least one organization. The organizations are: ARGENTINA-1 (Maximum of 10 organizations shown). Please check that the organizations are not the same.");
		}

		public void TestCodesCannotCoexist()
		{
			var conflictingCodes1 = new ZString[] { ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF };
			var conflictingCodes2 = new ZString[] { ArgentinaOrgCusCodeInfo.OrgCusCodes.IVE, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVF, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVI, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVM, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVN, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVR, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVP, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVS, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVX };
			var conflictingCodes3 = new ZString[] { ArgentinaOrgCusCodeInfo.OrgCusCodes.IBL, ArgentinaOrgCusCodeInfo.OrgCusCodes.IBM, ArgentinaOrgCusCodeInfo.OrgCusCodes.IBS, ArgentinaOrgCusCodeInfo.OrgCusCodes.IBN };
			var conflictingCodes4 = new ZString[] { ArgentinaOrgCusCodeInfo.OrgCusCodes.MIP, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVX, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVF, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVP, ArgentinaOrgCusCodeInfo.OrgCusCodes.IVS };

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode1 = orgHeader.CustomsCodes.AddNew();
			orgCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Argentina;

			var orgCusCode2 = orgHeader.CustomsCodes.AddNew();
			orgCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Argentina;

			AssertCodesCoexist(conflictingCodes1, conflictingCodes2, "The following (AR) Registration codes cannot coexist: CUI, CUF");
			AssertCodesCoexist(conflictingCodes2, conflictingCodes1, "The following (AR) Registration codes cannot coexist: IVE, IVF, IVI, IVM, IVN, IVR, IVP, IVS, IVX");
			AssertCodesCoexist(conflictingCodes3, conflictingCodes2, "The following (AR) Registration codes cannot coexist: IBL, IBM, IBS, IBN");
			AssertCodesCoexist(conflictingCodes4, conflictingCodes3, "The following (AR) Registration codes cannot coexist: MIP, IVX, IVF, IVP, IVS");

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
	}
}
