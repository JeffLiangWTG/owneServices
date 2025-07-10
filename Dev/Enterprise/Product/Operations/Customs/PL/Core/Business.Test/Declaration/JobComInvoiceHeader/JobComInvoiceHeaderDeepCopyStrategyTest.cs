using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class JobComInvoiceHeaderDeepCopyStrategyTest : TestCaseWithFactory
{
	public void TestTranCircumstanceCodeIsCopiedForPL()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice1 = declaration.Invoices.AddNew();
		invoice1.TranCircumstanceCode1 = TranCircumstancesList.Codes.B00PL;

		CombineAssertions(() =>
		{
			var declarationCopy = (JobDeclaration)declaration.TemplateCopy();
			var invoice2 = declarationCopy.Invoices[0];
			AssertEquals("TranCircumstance is copied for IMP dec", TranCircumstancesList.Codes.B00PL, invoice2.TranCircumstanceCode1);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declarationCopy = (JobDeclaration)declaration.TemplateCopy();
			invoice2 = declarationCopy.Invoices[0];
			AssertEquals("TranCircumstance is empty when not IMP dec", true, invoice2.TranCircumstanceCode1.IsEmpty);
		});
	}

	public void TestAdditionalTranCircumstanceCodesAreCopied()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice1 = declaration.Invoices.AddNew();
		var tranCircumstance = invoice1.AdditionalTranCircumstanceCodes.AddNew();
		tranCircumstance.CY_Code = TranCircumstancesList.Codes.B00PL;

		var declarationCopy = (JobDeclaration)declaration.TemplateCopy();
		var invoice2 = declarationCopy.Invoices[0];

		CombineAssertions(() =>
		{
			AssertEquals("TranCircumstanceCodes are copied for IMP dec", 1, invoice2.AdditionalTranCircumstanceCodes.Count);
			AssertEquals("Copied TranCircumstanceCode", TranCircumstancesList.Codes.B00PL, invoice2.AdditionalTranCircumstanceCodesAsString);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declarationCopy = (JobDeclaration)declaration.TemplateCopy();
			invoice2 = declarationCopy.Invoices[0];
			AssertEquals("TranCircumstanceCodes are not copied when not IMP dec", 0, invoice2.AdditionalTranCircumstanceCodes.Count);
		});
	}
}
