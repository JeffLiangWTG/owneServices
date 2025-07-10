using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class CusAuthorisationHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPH_OH_PermitHolder()
		{
			authorizationHeader.CPH_Type = "ABC";
			ValidationTestHelper.AssertFieldIsNotMandatory(authorizationHeader.CPH_OH_PermitHolderInfo);

			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration;
			ValidationTestHelper.AssertErrorIfNotEntered(authorizationHeader.CPH_OH_PermitHolderInfo);

			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration;
			ValidationTestHelper.AssertErrorIfNotEntered(authorizationHeader.CPH_OH_PermitHolderInfo);
		}

		public void TestCheckCPH_OA_AppliesTo_UniqueIDE()
			=> CheckUniqueAuthorizationForType(typeTested: CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration, counterType: CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration);

		public void TestCheckCPH_OA_AppliesTo_UniqueEDE()
			=> CheckUniqueAuthorizationForType(typeTested: CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration, counterType: CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration);

		void CheckUniqueAuthorizationForType(ZString typeTested, ZString counterType)
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddr1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddr1.OA_OH = org1.PK;
			var orgAddr2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddr2.OA_OH = org1.PK;
			var otherAuth = Factory.NewWithValidTestData<CusAuthorisationHeader>();

			otherAuth.CPH_Type = "ABC";
			otherAuth.CPH_OH_PermitHolder = org1.PK;
			otherAuth.CPH_OA_AppliesTo = orgAddr1.PK;

			CombineAssertions(() =>
			{
				authorizationHeader.CPH_Type = typeTested;
				authorizationHeader.Validation.ValidateAll();
				AssertNoErrorContaining(authorizationHeader.CPH_OA_AppliesToInfo, $"An Authorization of Type {authorizationHeader.CPH_Type} already exists for this Company.");

				authorizationHeader.CPH_OH_PermitHolder = org1.PK;
				authorizationHeader.Validation.ValidateAll();
				AssertNoErrorContaining(authorizationHeader.CPH_OA_AppliesToInfo, $"An Authorization of Type {authorizationHeader.CPH_Type} already exists for this Company.");

				authorizationHeader.CPH_OA_AppliesTo = orgAddr1.PK;
				AssertNoErrorContaining(authorizationHeader.CPH_OA_AppliesToInfo, $"An Authorization of Type {authorizationHeader.CPH_Type} already exists for this Company.");

				otherAuth.CPH_Type = typeTested;
				authorizationHeader.Validation.ValidateAll();
				AssertHasErrorContaining(authorizationHeader.CPH_OA_AppliesToInfo, $"An Authorization of Type {authorizationHeader.CPH_Type} already exists for this Company.");

				authorizationHeader.CPH_OA_AppliesTo = orgAddr2.PK;
				AssertNoErrorContaining(authorizationHeader.CPH_OA_AppliesToInfo, $"An Authorization of Type {authorizationHeader.CPH_Type} already exists for this Company.");

				otherAuth.CPH_OH_PermitHolder = org2.PK;
				authorizationHeader.CPH_OA_AppliesTo = orgAddr1.PK;
				AssertNoErrorContaining(authorizationHeader.CPH_OA_AppliesToInfo, $"An Authorization of Type {authorizationHeader.CPH_Type} already exists for this Company.");

				otherAuth.CPH_OH_PermitHolder = org1.PK;
				authorizationHeader.Validation.ValidateAll();
				AssertHasErrorContaining(authorizationHeader.CPH_OA_AppliesToInfo, $"An Authorization of Type {authorizationHeader.CPH_Type} already exists for this Company.");

				otherAuth.CPH_Type = counterType;
				authorizationHeader.Validation.ValidateAll();
				AssertNoErrorContaining(authorizationHeader.CPH_OA_AppliesToInfo, $"An Authorization of Type {authorizationHeader.CPH_Type} already exists for this Company.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			authorizationHeader = Factory.New<CusAuthorisationHeader>();
		}
		CusAuthorisationHeader authorizationHeader;
	}
}
