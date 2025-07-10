using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(NonPersistentRuleValidationResult))]
sealed class NonPersistentRuleValidationResultTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null Rule", () => NonPersistentRuleValidationResultBuilder.Fail().WithRule(null).WithEntity(Mock.Of<IBusiness>()).Build(false));
		AssertExceptionThrown<ArgumentNullException>("Null Entity", () => NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).WithEntity(null).Build(false));
	});

	public void TestRule() => CombineAssertions(() =>
	{
		var ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail().Build();

		AssertEquals("Caption", "Rule", DataBoundResourceStrings.GetDataForProperty(ruleValidationResult.RuleInfo).Caption);
		AssertEquals("Default", string.Empty, ruleValidationResult.Rule);

		ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>(x => x.P0V_Description == new ZString("Rule1"))).Build();
		AssertEquals("Via Constructor Setter", "Rule1", ruleValidationResult.Rule);
	});

	public void TestSeverity() => CombineAssertions(() =>
	{
		var ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail().Build();

		AssertEquals("Caption", "Severity", DataBoundResourceStrings.GetDataForProperty(ruleValidationResult.SeverityInfo).Caption);
		AssertEquals("Default", string.Empty, ruleValidationResult.Severity);

		ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail().WithSeverity("ERR").Build();
		AssertEquals("Via Constructor Setter", "ERR", ruleValidationResult.Severity);
	});

	public void TestMessage() => CombineAssertions(() =>
	{
		var ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail().Build();

		AssertEquals("Caption", "Message", DataBoundResourceStrings.GetDataForProperty(ruleValidationResult.MessageInfo).Caption);
		AssertEquals("Fallback if empty", "Validation Failed", ruleValidationResult.Message);

		ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail().WithMessage("XXX").Build();
		AssertEquals("Via Constructor Setter", "XXX", ruleValidationResult.Message);
	});

	public void TestPassed() => CombineAssertions(() =>
	{
		var ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail().Build();

		AssertEquals("Default", ZBool.False, ruleValidationResult.Passed);

		ruleValidationResult = NonPersistentRuleValidationResultBuilder.Pass().Build();
		AssertEquals("Via Constructor Setter", ZBool.True, ruleValidationResult.Passed);
	});

	public void TestPropertyInfoGetter() => CombineAssertions(() =>
	{
		AssertNull("Default", NonPersistentRuleValidationResultBuilder.Fail().Build().PropertyInfoGetter);

		var dummy = Factory.New<DummyBusinessObject>();
		var validationResult = NonPersistentRuleValidationResultBuilder.Fail()
			.WithEntity(dummy)
			.WithTargetProperty(() => dummy.Z0_BitTrueInfo)
			.Build();
		AssertNotNull("Via Constructor Setter", validationResult.PropertyInfoGetter);
	});

	public void TestDisplayValidationOnField() => CombineAssertions(() =>
	{
		var dummy = Factory.New<DummyBusinessObject>();

		var ruleValidationResult1 = NonPersistentRuleValidationResultBuilder.Pass()
			.WithEntity(dummy)
			.WithTargetProperty(PropertyGetter)
			.Build();
		ruleValidationResult1.DisplayValidationOnField();
		AssertEquals("It is a passed result", false, dummy.Z0_BitTrueInfo.HasNotifications());

		var ruleValidationResult2 = NonPersistentRuleValidationResultBuilder.Fail()
			.WithEntity(dummy)
			.WithSeverity(ZString.Empty)
			.WithMessage(ZString.Empty)
			.WithTargetProperty(PropertyGetter)
			.Build();
		ruleValidationResult2.DisplayValidationOnField();
		AssertEquals("Empty Message", false, dummy.Z0_BitTrueInfo.HasNotifications());

		var ruleValidationResult3 = NonPersistentRuleValidationResultBuilder.Fail()
			.WithEntity(dummy)
			.WithSeverity(ZString.Empty)
			.WithMessage("This is an ERR")
			.WithTargetProperty(PropertyGetter)
			.Build();
		ruleValidationResult3.DisplayValidationOnField();
		AssertEquals("Empty Severity", false, dummy.Z0_BitTrueInfo.HasNotifications());

		return;

		ZPropertyInfo PropertyGetter() => dummy.Z0_BitTrueInfo;
	});

	public void TestDisplayValidationOnField_Error()
	{
		var dummy = Factory.New<DummyBusinessObject>();
		var ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail()
			.WithEntity(dummy)
			.WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error)
			.WithMessage("This is an ERR")
			.WithTargetProperty(() => dummy.Z0_BitTrueInfo)
			.Build();
		ruleValidationResult.DisplayValidationOnField();
		AssertEquals(true, dummy.Z0_BitTrueInfo.HasError("This is an ERR"));
	}

	public void TestDisplayValidationOnField_MessageError()
	{
		var dummy = Factory.New<DummyBusinessObject>();
		var ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail()
			.WithEntity(dummy)
			.WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message)
			.WithMessage("This is a MSG")
			.WithTargetProperty(() => dummy.Z0_BitTrueInfo)
			.Build();
		ruleValidationResult.DisplayValidationOnField();
		AssertEquals(true, dummy.Z0_BitTrueInfo.HasMessageError("This is a MSG"));
	}

	public void TestDisplayValidationOnField_Warning()
	{
		var dummy = Factory.New<DummyBusinessObject>();
		var ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail()
			.WithEntity(dummy)
			.WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning)
			.WithMessage("This is a WRN")
			.WithTargetProperty(() => dummy.Z0_BitTrueInfo)
			.Build();
		ruleValidationResult.DisplayValidationOnField();
		AssertEquals(true, dummy.Z0_BitTrueInfo.HasWarning("This is a WRN"));
	}

	public void TestLogValidationFailEvent() => CombineAssertions(() =>
	{
		var dummy = Factory.New<DummyWithLogs>();
		var ruleValidationResult = NonPersistentRuleValidationResultBuilder.Fail()
			.WithRule(Mock.Of<IProcessTemplateValidation>(x => x.P0V_LogValidationFailEvent == ZBool.True && x.Factory == Factory))
			.WithEntity(dummy)
			.WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error)
			.WithMessage("This is an ERR")
			.Build();
		ruleValidationResult.LogValidationFailEvent();
		AssertEquals(true, dummy.Logs.HasLogWith(x => x.SL_SE_NKEvent == Enterprise.ZArchitecture.Business.Events.ValidationRuleFailed.Code && x.SL_Reference == "This is an ERR"));
	});

	protected override BusinessObject GetNewBusinessObject() => NonPersistentRuleValidationResultBuilder.Fail().Build();
}
