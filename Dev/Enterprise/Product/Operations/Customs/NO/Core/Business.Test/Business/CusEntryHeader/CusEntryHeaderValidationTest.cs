using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryHeaderValidation))]
sealed class CusEntryHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestOriginalDeclarationId_ShouldWarn_WhenRecalculation()
	{
		var declarationOriginal = Factory.New<JobDeclaration>();
		var declarationRecalc = Factory.New<JobDeclaration>();
		declarationOriginal.RelatedDeclarations.Add(declarationRecalc);
		Factory.Save();

		declarationOriginal.CustomsEntryHeaders.AddNew().MovementReferenceNumberSetter("123");
		var cusEntryHeader1 = declarationRecalc.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			declarationRecalc.JE_CopyStatus = NODeclarationCopyStatus.Codes.ReExport;
			AssertNoWarning("When recalculation", cusEntryHeader1.CH_ReCalcOrigDeclInfo, "This is a recalculation job. To see original go to Misc tab and click on Parent button.");

			declarationRecalc.JE_CopyStatus = NODeclarationCopyStatus.Codes.Recalculation;
			cusEntryHeader1.CH_ReCalcOrigDecl = "123";
			AssertHasWarning("When recalculation", cusEntryHeader1.CH_ReCalcOrigDeclInfo, "This is a recalculation job. To see original go to Misc tab and click on Parent button.");
		});
	}

	public void TestOriginalDeclarationId_ShouldBePresentInOriginalDeclarationOfTypeMRN_WhenRecalculation()
	{
		var declarationOriginal = Factory.New<JobDeclaration>();
		var declarationRecalc = Factory.New<JobDeclaration>();
		declarationOriginal.RelatedDeclarations.Add(declarationRecalc);
		declarationRecalc.JE_CopyStatus = NODeclarationCopyStatus.Codes.Recalculation;
		Factory.Save();

		declarationOriginal.CustomsEntryHeaders.AddNew().EntryNumber = "69";
		declarationOriginal.CustomsEntryHeaders.AddNew().MovementReferenceNumberSetter("420");

		var cusEntryHeaderRecalc = declarationRecalc.CustomsEntryHeaders.AddNew();
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(cusEntryHeaderRecalc.CH_ReCalcOrigDeclInfo, "69", "420");
	}

	public void TestOriginalDeclarationId_ShouldNotAllowDuplicates_WhenRecalculation()
	{
		var declarationOriginal = Factory.New<JobDeclaration>();
		var declarationRecalc = Factory.New<JobDeclaration>();
		declarationOriginal.RelatedDeclarations.Add(declarationRecalc);
		declarationRecalc.JE_CopyStatus = NODeclarationCopyStatus.Codes.Recalculation;
		Factory.Save();

		declarationOriginal.CustomsEntryHeaders.AddNew().MovementReferenceNumberSetter("69");
		declarationOriginal.CustomsEntryHeaders.AddNew().MovementReferenceNumberSetter("420");
		declarationOriginal.CustomsEntryHeaders.AddNew().MovementReferenceNumberSetter("9001");

		const string duplicateFound = "Value already defined, please ensure that there are no duplicates.";
		CombineAssertions(() =>
		{
			var cusEntryHeader1 = declarationRecalc.CustomsEntryHeaders.AddNew();
			cusEntryHeader1.CH_ReCalcOrigDecl = "69";
			var cusEntryHeader2 = declarationRecalc.CustomsEntryHeaders.AddNew();
			cusEntryHeader2.CH_ReCalcOrigDecl = "420";
			AssertNoError("(base-case): header 1", cusEntryHeader1.CH_ReCalcOrigDeclInfo, duplicateFound);
			AssertNoError("(base-case): header 2", cusEntryHeader2.CH_ReCalcOrigDeclInfo, duplicateFound);

			var cusEntryHeader3 = declarationRecalc.CustomsEntryHeaders.AddNew();
			cusEntryHeader3.CH_ReCalcOrigDecl = "420";
			AssertNoError("(with-duplicate): header 1", cusEntryHeader1.CH_ReCalcOrigDeclInfo, duplicateFound);
			AssertHasError("(with-duplicate): header 2", cusEntryHeader2.CH_ReCalcOrigDeclInfo, duplicateFound);
			AssertHasError("(with-duplicate): header 3", cusEntryHeader3.CH_ReCalcOrigDeclInfo, duplicateFound);

			cusEntryHeader3.CH_ReCalcOrigDecl = "9001";
			AssertNoError("(no-duplicates): header 1", cusEntryHeader1.CH_ReCalcOrigDeclInfo, duplicateFound);
			AssertNoError("(no-duplicates): header 2", cusEntryHeader2.CH_ReCalcOrigDeclInfo, duplicateFound);
			AssertNoError("(no-duplicates): header 3", cusEntryHeader3.CH_ReCalcOrigDeclInfo, duplicateFound);
		});
	}

	public void TestPaymentMethodValidation()
	{
		var entryHeaderMock = Factory.NewMoq<CusEntryHeader>();
		var entryHeader = entryHeaderMock.Object;

		_ = entryHeaderMock.SetupGet(x => x.HasPayments).Returns(false);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(entryHeader.CH_PaymentMethodInfo, "X", "N");

		_ = entryHeaderMock.SetupGet(x => x.HasPayments).Returns(true);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(entryHeader.CH_PaymentMethodInfo, "X", "D");
	}
}
