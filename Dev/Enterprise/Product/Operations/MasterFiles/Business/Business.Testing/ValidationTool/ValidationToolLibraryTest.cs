using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing;

sealed class ValidationToolLibraryTest : TestCaseWithFactory
{
	public void TestValidateDocumentAttributes_Null()
	{
		AssertValidateDocumentAttributesExpression("Target business object is null", "ValidateDocumentAttributes({RequiredDocuments})", null, false);
	}

	public void TestValidateDocumentAttributes_EnableWorkflowValidation() => CombineAssertions(() =>
	{
		using var disposable = WorkflowDataRegistry.Instance.EnableWorkflowValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WorkflowValidationProcessTypeCollection { new WorkflowValidationProcessType { ProcessType = "BRK" } });

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "VWG";
		orgHeader.OH_FullName = "Vic Wang";
		var requiredDocument1 = orgHeader.RequiredDocuments.AddNew();
		requiredDocument1.EQ_DocType = "POA";
		var customsAttribute1 = requiredDocument1.Attributes.AddNew();
		customsAttribute1.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1 + "Test1";
		customsAttribute1.D0_AttribDisplayValue = "\"<OH_FullName>\" == \"Vic Wang\"";
		var customsAttribute2 = requiredDocument1.Attributes.AddNew();
		customsAttribute2.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1 + "Test2";
		customsAttribute2.D0_AttribDisplayValue = "\"<OH_Code>\" == \"VWG\"";
		var requiredDocument2 = orgHeader.RequiredDocuments.AddNew();
		requiredDocument2.EQ_DocType = "POA";

		AssertValidateDocumentAttributesExpression("Input a collection", "ValidateDocumentAttributes({RequiredDocuments})", orgHeader, true);
		AssertValidateDocumentAttributesExpression("Input filtered elements", "ValidateDocumentAttributes({RequiredDocuments.Where({EQ_DocType==\"POA\"})})", orgHeader, true);
	});

	public void TestValidateDocumentAttributes_DisableWorkflowValidation() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "VWG";
		orgHeader.OH_FullName = "Vic Wang";
		var requiredDocument1 = orgHeader.RequiredDocuments.AddNew();
		requiredDocument1.EQ_DocType = "POA";
		var customsAttribute1 = requiredDocument1.Attributes.AddNew();
		customsAttribute1.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1 + "Test1";
		customsAttribute1.D0_AttribDisplayValue = "\"<OH_FullName>\" == \"Vic Wang\"";
		var customsAttribute2 = requiredDocument1.Attributes.AddNew();
		customsAttribute2.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1 + "Test2";
		customsAttribute2.D0_AttribDisplayValue = "\"<OH_Code>\" == \"VWG\"";
		var requiredDocument2 = orgHeader.RequiredDocuments.AddNew();
		requiredDocument2.EQ_DocType = "POA";

		AssertValidateDocumentAttributesExpression("Input a collection", "ValidateDocumentAttributes({RequiredDocuments})", orgHeader, false);
		AssertValidateDocumentAttributesExpression("Input filtered elements", "ValidateDocumentAttributes({RequiredDocuments.Where({EQ_DocType==\"POA\"})})", orgHeader, false);
	});

	void AssertValidateDocumentAttributesExpression(string message, string macro, BusinessObject businessObject, bool expectedMatch)
	{
		var context = new IMacroLibrary[] { new StandardLibrary(), new ValidationToolLibrary(Factory) }.CreateContext();
		var expression = macro.With(context).CreateExpression();
		AssertEquals($"{message}: Evaluate Result", expectedMatch, expression.Evaluate(businessObject));
		AssertMultilineASCIIEquals($"{message}: no errors", "", string.Join("\r\n", expression.Errors));
	}
}
