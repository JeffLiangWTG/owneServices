using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusReconDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCRD_ApplicationCode()
		{
			const string errorMessage = "Application Code must be 3 characters.";

			CombineAssertions(() =>
			{
				reconDeclaration.CRD_ApplicationCode = "AAA";
				AssertNoError("Valid", reconDeclaration.CRD_ApplicationCodeInfo, errorMessage);
				reconDeclaration.CRD_ApplicationCode = "AA";
				AssertHasError("Invalid", reconDeclaration.CRD_ApplicationCodeInfo, errorMessage);
			});
		}

		public void TestCheckCRD_DeclarationType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(reconDeclaration.CRD_DeclarationTypeInfo, "ZZZ", CusReconDeclarationTypeList.Codes.BWH);
		}

		public void TestCheckCRD_DeclarantType()
		{
			var mockDeclaration = Factory.NewMoq<CusReconDeclaration>();
			var declaration = mockDeclaration.Object;
			var mockLookups = new Mock<CusReconDeclarationLookups>(declaration);
			var list = new CodeDescriptionPairList();
			list.AddPair("ABC");
			mockLookups.Setup(m => m.DeclarantTypeList).Returns(list);
			mockDeclaration
				.Protected()
				.Setup<CusReconBase.CusReconDeclarationLookups>("GetNewLookups")
				.Returns(mockLookups.Object);

			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.CRD_DeclarantTypeInfo, "ZZZ", "ABC");
		}

		protected override void SetUp()
		{
			base.SetUp();
			reconDeclaration = Factory.NewWithValidTestData<CusReconDeclaration>();
		}
		CusReconDeclaration reconDeclaration;
	}
}
