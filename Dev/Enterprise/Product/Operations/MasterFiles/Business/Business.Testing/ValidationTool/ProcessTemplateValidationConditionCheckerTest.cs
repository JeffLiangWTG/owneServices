using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Moq;
using Moq.Protected;

namespace Enterprise.MasterFiles.Business.Testing;

sealed class ProcessTemplateValidationConditionCheckerTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Rule is null", () =>
		{
			_ = new ProcessTemplateValidationConditionChecker(null, Mock.Of<IBusiness>());
		});

		AssertExceptionThrown<ArgumentNullException>("Entity is null", () =>
		{
			_ = new ProcessTemplateValidationConditionChecker(Mock.Of<IProcessTemplateValidation>(), null);
		});
	});

	public void TestAreConditionsMet() => CombineAssertions(() =>
	{
		var company = Factory.New<GlbCompany>();
		var dummyBusinessObject = Factory.New<DummyBusinessObject>();
		dummyBusinessObject.Z0_BitTrue = ZBool.True;
		var ruleMock = new Mock<IProcessTemplateValidation>();
		ruleMock.SetupGet(x => x.WorkflowProcessType).Returns(DummyWorkflowDescriptor.Instance.Code);
		ruleMock.SetupGet(x => x.Factory).Returns(Factory);
		ruleMock.SetupGet(x => x.P0V_Condition1).Returns(ZString.Empty);
		ruleMock.SetupGet(x => x.P0V_Condition2).Returns(ProcessTasksLookups.MacroCondition);
		ruleMock.SetupGet(x => x.P0V_Condition2Value).Returns("\"<Z0_BitTrue>\" == \"Y\"");
		ruleMock.SetupGet(x => x.P0V_ContextType).Returns(ProcessTemplateValidationContextType.Codes.CargoWise);
		ruleMock.SetupGet(x => x.P0V_GC_Company).Returns(company.PK);
		var rule = ruleMock.Object;
		var mockerChecker1 = new Mock<ProcessTemplateValidationConditionChecker>(rule, dummyBusinessObject)
		{
			CallBase = true
		};
		mockerChecker1.Protected().Setup<bool>("IsCompanyMet", ItExpr.IsAny<ZGuid>()).Returns((ZGuid g) => g == company.PK);
		AssertEquals("MacroCondition on input entity", true, mockerChecker1.Object.AreConditionsMet());

		var mockerChecker2 = new Mock<ProcessTemplateValidationConditionChecker>(rule, Mock.Of<IBusiness>())
		{
			CallBase = true
		};
		mockerChecker2.Protected().Setup<bool>("IsCompanyMet", ItExpr.IsAny<ZGuid>()).Returns((ZGuid g) => g == company.PK);
		mockerChecker2.Protected().SetupGet<IBusiness>("EntityToEvaluateMacro").Returns(dummyBusinessObject);
		AssertEquals("MacroCondition on redirected entity", true, mockerChecker2.Object.AreConditionsMet());
	});

	public void TestEvaluateRuleWithTimeout() => CombineAssertions(() =>
	{
		var dummyBusinessObject = Factory.New<DummyBusinessObject>();
		dummyBusinessObject.Z0_BitTrue = ZBool.True;
		var ruleMock = new Mock<IProcessTemplateValidation>();
		ruleMock.SetupGet(x => x.WorkflowProcessType).Returns(DummyWorkflowDescriptor.Instance.Code);
		ruleMock.SetupGet(x => x.Factory).Returns(Factory);
		ruleMock.SetupGet(x => x.P0V_Severity).Returns("ERR");
		ruleMock.SetupGet(x => x.P0V_FieldToDisplayValidation).Returns(ProcessTasksLookups.MacroCondition);
		ruleMock.SetupGet(x => x.P0V_ValidationRule).Returns("\"<Z0_BitTrue>\" == \"Y\"");
		var rule = ruleMock.Object;
		var mockChecker = new Mock<ProcessTemplateValidationConditionChecker>(rule, dummyBusinessObject) { CallBase = true };
		mockChecker.As<IEvaluateRuleWithTimeout>().Setup(x => x.EvaluateWithTimeout(It.IsAny<Func<bool>>())).Returns((false, true));
		var result = mockChecker.Object.EvaluateRule();
		AssertEquals("Failed", false, result.Passed);
		AssertEquals("Message", "Rule failed as it has exceeded the timeout threshold setup in the Registry : Workflow Manager > Workflow Templates > Validation Rule Timeout.", result.Message);
	});

	public void TestEvaluateRule() => CombineAssertions(() =>
	{
		var template = Factory.New<ProcessTaskTemplate>();
		template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		var templateValidation = template.ProcessTemplateValidations.AddNew();
		templateValidation.P0V_Description = "D1";

		templateValidation.P0V_Severity = "ERR";
		templateValidation.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";

		var dummyBusinessObject = Factory.New<DummyBusinessObject>();
		dummyBusinessObject.Z0_BitTrue = ZBool.True;

		var checker = new ProcessTemplateValidationConditionChecker(templateValidation, dummyBusinessObject);

		var result = checker.EvaluateRule();
		AssertEquals("Severity", "ERR", result.Severity);
		AssertEquals("Fallback if without Message", "Validation Failed", result.Message);
		AssertEquals("PropertyInfoGetter", dummyBusinessObject.Z0_BitTrueInfo, result.PropertyInfoGetter());
		AssertEquals("Passed", true, result.Passed);

		templateValidation.P0V_Message = "Check Z0_BitTrue: <Z0_BitTrue>";
		result = checker.EvaluateRule();
		AssertEquals("With Message", "Check Z0_BitTrue: Y", result.Message);
	});

	public void TestEvaluateRule_RedirectEntity() => CombineAssertions(() =>
	{
		var template = Factory.New<ProcessTaskTemplate>();
		template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		var templateValidation = template.ProcessTemplateValidations.AddNew();
		templateValidation.P0V_Description = "D1";

		templateValidation.P0V_Severity = "ERR";
		templateValidation.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";
		templateValidation.P0V_Message = "Check Z0_BitTrue: <Z0_BitTrue>";

		var dummyBusinessObject = Factory.New<DummyBusinessObject>();
		dummyBusinessObject.Z0_BitTrue = ZBool.True;

		var mockChecker = new Mock<ProcessTemplateValidationConditionChecker>(templateValidation, Mock.Of<IBusiness>()) { CallBase = true };
		mockChecker.Protected().SetupGet<IBusiness>("EntityToEvaluateMacro").Returns(dummyBusinessObject);

		var result = mockChecker.Object.EvaluateRule();
		AssertEquals("Severity", "ERR", result.Severity);
		AssertEquals("Message", "Check Z0_BitTrue: Y", result.Message);
		AssertNotNull("PropertyInfoGetter", result.PropertyInfoGetter);
		AssertEquals("Passed", true, result.Passed);
	});

	public void TestEvaluateRule_GetFieldToDisplayValidationPropertyInfo_CustomField()
	{
		using var suppressReporting = Factory.ThreadSentry.SuppressReporting();
		var template = Factory.New<ProcessTaskTemplate>();
		template.P0_ProcessType = "SHP";
		var templateValidation = template.ProcessTemplateValidations.AddNew();
		templateValidation.P0V_Description = "D1";

		templateValidation.P0V_Severity = "ERR";
		templateValidation.P0V_FieldToDisplayValidation = "Consignee.GetCustomField(Custom Field 1)";
		templateValidation.P0V_ValidationRule = "\"<Consignee.GetCustomField(\"Custom Field 1\")>\" == \"Custom Value 1\"";
		templateValidation.P0V_Message = "Consignee.GetCustomField(\\\"Custom Field 1\\\"): <Consignee.GetCustomField(\"Custom Field 1\")>";

		var bizo = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
		var shipment = (Forwarding.IForwardingShipment)bizo;
		var org = Factory.New<OrgHeader>();
		org.SetUserDefinedValue("Custom Field 1", new ZString("Custom Value 1"));
		shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;

		var checker = new ProcessTemplateValidationConditionChecker(templateValidation, bizo);

		var result = checker.EvaluateRule();
		AssertEquals("__CUSTOM FIELD 1__prop__ZString", result.PropertyInfoGetter().Name);
	}

	public void TestFieldToDisplayValidationZPropertyInfo()
	{
		var template = Factory.New<ProcessTaskTemplate>();
		template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		var templateValidation = template.ProcessTemplateValidations.AddNew();
		templateValidation.P0V_Description = "D1";

		templateValidation.P0V_Severity = "ERR";
		templateValidation.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";
		templateValidation.P0V_Message = "Check Z0_BitTrue: <Z0_BitTrue>";

		var dummyBusinessObject = Factory.New<DummyBusinessObject>();
		dummyBusinessObject.Z0_BitTrue = ZBool.True;

		var mockChecker = new Mock<ProcessTemplateValidationConditionChecker>(templateValidation, Mock.Of<IBusiness>()) { CallBase = true };
		mockChecker.Protected().SetupGet<IBusiness>("EntityToEvaluateMacro").Returns(dummyBusinessObject);

		AssertNotNull(mockChecker.Object.FieldToDisplayValidationZPropertyInfo);
	}
}
