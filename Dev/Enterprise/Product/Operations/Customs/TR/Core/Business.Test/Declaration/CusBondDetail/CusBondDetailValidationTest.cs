using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class CusBondDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPW_CPH_Guarantee()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var organization1 = Factory.NewWithValidTestData<OrgHeader>();
				organization1.OH_Code = "Org1";
				var organization2 = Factory.NewWithValidTestData<OrgHeader>();
				organization2.OH_Code = "Org2";

				var permitHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				permitHeader1.CPH_Number = "PERMIT1";
				permitHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				permitHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
				permitHeader1.CPH_OH_PermitHolder = organization1.PK;

				var permitHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				permitHeader2.CPH_Number = "PERMIT2";
				permitHeader2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				permitHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
				permitHeader2.CPH_OH_PermitHolder = organization2.PK;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = organization1.PK;
				var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
				var cusBondDetail = instruction.Guarantee;
				ValidationTestHelper.AssertErrorIfInvalidPK(cusBondDetail.PW_CPH_GuaranteeInfo, permitHeader2.PK, permitHeader1.PK);
			}
		}

		public void TestCheckPW_BondType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var organization1 = Factory.NewWithValidTestData<OrgHeader>();
				organization1.OH_Code = "Org1";

				var permitHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				permitHeader1.CPH_Type = "T1";
				permitHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				permitHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
				permitHeader1.CPH_OH_PermitHolder = organization1.PK;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = organization1.PK;
				var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
				var cusBondDetail = instruction.Guarantee;
				ValidationTestHelper.AssertInvalidCodeMessageError(cusBondDetail.PW_BondTypeInfo, "XXX", "BANKA");
				ValidationTestHelper.AssertInvalidCodeMessageError(cusBondDetail.PW_BondTypeInfo, "XXX", "T1");

				instruction.ZG_DedicatedGuaranteeAmount = 100m;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusBondDetail.PW_BondTypeInfo);
			}
		}

		public void TestCheckPW_BondNumber()
		{
			var cusBondDetail = Factory.New<CusBondDetail>();
			cusBondDetail.PW_BondType = GuaranteeTypeList.Codes.BANKA;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusBondDetail.PW_BondNumberInfo);
		}

		public void TestCheckPW_BondAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
			instruction.ZG_DedicatedGuaranteeAmount = 100m;
			var cusBondDetail = instruction.Guarantee;
			cusBondDetail.PW_BondAmount = 10m;
			AssertNoMessageErrorContaining(cusBondDetail.PW_BondAmountInfo, MandatoryValidation.YouHaveNotEntered);

			instruction.ZG_GuaranteeRatio = 0m;
			cusBondDetail.PW_BondAmount = 0m;
			AssertHasMessageErrorContaining("ZG_GuaranteeRatio needs to be cleared, so ValidationTestHelper is not used", cusBondDetail.PW_BondAmountInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
