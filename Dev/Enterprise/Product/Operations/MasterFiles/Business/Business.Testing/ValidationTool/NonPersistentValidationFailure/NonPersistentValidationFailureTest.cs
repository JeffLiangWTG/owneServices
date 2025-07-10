using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(NonPersistentValidationFailure))]
sealed class NonPersistentValidationFailureTest : NonPersistentBusinessObjectTestCase
{
	public void TestStatus() => CombineAssertions(() =>
	{
		var nonPersistentObject = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		AssertEquals("Empty FailedRuleResults", string.Empty, nonPersistentObject.Status);

		nonPersistentObject.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		AssertEquals("Warning", "Warnings only", nonPersistentObject.Status);

		nonPersistentObject.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		AssertEquals("Message", "Requires Supervisor Override", nonPersistentObject.Status);

		nonPersistentObject.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
		AssertEquals("Error", "Please fix errors before proceeding", nonPersistentObject.Status);
	});

	public void TestSeverity() => CombineAssertions(() =>
	{
		var nonPersistentObject = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		AssertEquals("Empty FailedRuleResults", string.Empty, nonPersistentObject.Severity);

		nonPersistentObject.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		AssertEquals("Warning", ProcessTemplateValidationSeverityList.Codes.Warning, nonPersistentObject.Severity);

		nonPersistentObject.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		AssertEquals("Message", ProcessTemplateValidationSeverityList.Codes.Message, nonPersistentObject.Severity);

		nonPersistentObject.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
		AssertEquals("Error", ProcessTemplateValidationSeverityList.Codes.Error, nonPersistentObject.Severity);
	});

	public void TestFailedRuleResults() => CombineAssertions(() =>
	{
		var nonPersistentObject = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		AssertEquals("Default", 0, nonPersistentObject.FailedRuleResults.Count);

		var template = Factory.New<ProcessTaskTemplate>();

		var templateValidation1 = template.ProcessTemplateValidations.AddNew();
		templateValidation1.ProcessTemplateValidationActions.AddNew();
		var ruleValidationResult1 = NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation1).Build();
		var templateValidation2 = template.ProcessTemplateValidations.AddNew();
		templateValidation2.ProcessTemplateValidationActions.AddNew();
		var ruleValidationResult2 = NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation2).Build();
		nonPersistentObject.FailedRuleResults.Add(ruleValidationResult1);
		nonPersistentObject.FailedRuleResults.Add(ruleValidationResult2);

		AssertContainsExactElementsInAnyOrder("Added", [ruleValidationResult1, ruleValidationResult2], nonPersistentObject.FailedRuleResults);
	});

	public void TestMessage()
	{
		var nonPersistentObject = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		AssertEquals("Rules that failed validation for SAV - On Save", nonPersistentObject.Message);
	}

	public void TestActionSourceCode()
	{
		var nonPersistentObject = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		AssertEquals("SAV", nonPersistentObject.ActionSourceCode);
	}

	public void TestActionSourceDescription()
	{
		var nonPersistentObject = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		AssertEquals("On Save", nonPersistentObject.ActionSourceDescription);
	}

	public void TestHandleMessageErrors() => CombineAssertions(() =>
	{
		var mockMessageErrorsHandle = new Mock<IValidationToolMessageErrorsHandle>();
		mockMessageErrorsHandle.Setup(x => x.Handle()).Returns(true);

		var mockObjectHandle = new Mock<ObjectHandle>();
		mockObjectHandle.Setup(x => x.GetObject(It.IsAny<BusinessObject>())).Callback<object[]>(b =>
		{
			if (b.Single() is DummyEnterpriseBusinessObject dummy)
			{
				dummy.Logs.AddNew(Events.Authorised);
			}
		}).Returns(mockMessageErrorsHandle.Object);
		var hashTable = new Hashtable
		{
			{ DummyBusinessObject.Schema.TablePrefix, mockObjectHandle.Object }
		};
		using var substitute = ObjectFactory.Substitute("ValidationToolMessageErrorsHandles", hashTable);

		var failure = new NonPersistentValidationFailure(new BusinessObjectFactory(), "SAV", "On Save");
		AssertEquals("No MessageErrors", true, failure.HandleMessageErrors());

		var businessEntityWithHandle = Factory.New<DummyEnterpriseBusinessObject>();

		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithEntity(businessEntityWithHandle).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithEntity(businessEntityWithHandle).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());

		AssertEquals("Has MessageErrors", true, failure.HandleMessageErrors());
		AssertEquals("No changes to the BusinessObject", false, businessEntityWithHandle.HasChanges);
	});

	public void TestValidationToolMessageErrorsHandles()
	{
		AssertNotNull(ObjectFactory.Get<Hashtable>("ValidationToolMessageErrorsHandles")["JE"]);
	}

	[ExpectNoExceptions]
	public void TestHandleRequestsOnFailure_HasNoRuleResults()
	{
		var mockValidationToolWarningRequestsHandle = new Mock<IValidationToolRequestsHandle>();
		using var warningRequestsHandleSubstitute = ObjectFactory.Substitute(mockValidationToolWarningRequestsHandle.Object);

		var failure = new NonPersistentValidationFailure(new BusinessObjectFactory(), "SAV", "On Save");

		failure.HandleRequestsOnFailure();
		mockValidationToolWarningRequestsHandle.Verify(x => x.Handle(It.IsAny<BusinessObjectFactory>(), It.IsAny<IList<ValidationToolRequestHandleParameter>>()), Times.Never);
	}

	[ExpectNoExceptions]
	public void TestHandleRequestsOnFailure_IncludeError()
	{
		var mockValidationToolWarningRequestsHandle = new Mock<IValidationToolRequestsHandle>();
		using var warningRequestsHandleSubstitute = ObjectFactory.Substitute(mockValidationToolWarningRequestsHandle.Object);

		var template = Factory.New<ProcessTaskTemplate>();
		var templateValidation1 = template.ProcessTemplateValidations.AddNew();
		var templateValidation2 = template.ProcessTemplateValidations.AddNew();
		var templateValidation3 = template.ProcessTemplateValidations.AddNew();

		var failure = new NonPersistentValidationFailure(new BusinessObjectFactory(), "SAV", "On Save");
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation1).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation2).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation3).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());

		failure.HandleRequestsOnFailure();
		mockValidationToolWarningRequestsHandle.Verify(x => x.Handle(It.IsAny<BusinessObjectFactory>(), It.IsAny<IList<ValidationToolRequestHandleParameter>>()), Times.Never);
	}

	[ExpectNoExceptions]
	public void TestHandleRequestsOnFailure_IncludeMessage()
	{
		var mockValidationToolWarningRequestsHandle = new Mock<IValidationToolRequestsHandle>();
		using var warningRequestsHandleSubstitute = ObjectFactory.Substitute(mockValidationToolWarningRequestsHandle.Object);

		var template = Factory.New<ProcessTaskTemplate>();
		var templateValidation1 = template.ProcessTemplateValidations.AddNew();
		var templateValidation2 = template.ProcessTemplateValidations.AddNew();

		var newFactory = new BusinessObjectFactory();
		var failure = new NonPersistentValidationFailure(newFactory, "SAV", "On Save");
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation1).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation2).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());

		failure.HandleRequestsOnFailure();
		mockValidationToolWarningRequestsHandle.Verify(x =>
		x.Handle(
			It.Is<BusinessObjectFactory>((f) => f == newFactory),
			It.Is((IList<ValidationToolRequestHandleParameter> list) => list.Count == 2 && list.Any(r => r.ValidationRulePK == failure.FailedRuleResults[0].ValidationRule.PK) && list.Any(r => r.ValidationRulePK == failure.FailedRuleResults[1].ValidationRule.PK)))
		, Times.Once);
	}

	[ExpectNoExceptions]
	public void TestHandleRequestsOnFailure_OnlyIncludeMessage()
	{
		var mockValidationToolWarningRequestsHandle = new Mock<IValidationToolRequestsHandle>();
		using var warningRequestsHandleSubstitute = ObjectFactory.Substitute(mockValidationToolWarningRequestsHandle.Object);

		var template = Factory.New<ProcessTaskTemplate>();
		var templateValidation1 = template.ProcessTemplateValidations.AddNew();
		var templateValidation2 = template.ProcessTemplateValidations.AddNew();

		var newFactory = new BusinessObjectFactory();
		var failure = new NonPersistentValidationFailure(newFactory, "SAV", "On Save");
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation1).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation2).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());

		failure.HandleRequestsOnFailure();
		mockValidationToolWarningRequestsHandle.Verify(x =>
		x.Handle(
			It.Is<BusinessObjectFactory>((f) => f == newFactory),
			It.Is((IList<ValidationToolRequestHandleParameter> list) => list.Count == 2 && list.Any(r => r.ValidationRulePK == failure.FailedRuleResults[0].ValidationRule.PK) && list.Any(r => r.ValidationRulePK == failure.FailedRuleResults[1].ValidationRule.PK)))
		, Times.Once);
	}

	[ExpectNoExceptions]
	public void TestHandleRequestsOnFailure_OnlyIncludeWarning()
	{
		var mockValidationToolWarningRequestsHandle = new Mock<IValidationToolRequestsHandle>();
		using var warningRequestsHandleSubstitute = ObjectFactory.Substitute(mockValidationToolWarningRequestsHandle.Object);

		var template = Factory.New<ProcessTaskTemplate>();
		var templateValidation1 = template.ProcessTemplateValidations.AddNew();
		var templateValidation2 = template.ProcessTemplateValidations.AddNew();

		var newFactory = new BusinessObjectFactory();
		var failure = new NonPersistentValidationFailure(newFactory, "SAV", "On Save");
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation1).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(templateValidation2).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());

		failure.HandleRequestsOnFailure();
		mockValidationToolWarningRequestsHandle.Verify(x =>
		x.Handle(It.Is<BusinessObjectFactory>((f) => f == newFactory), It.Is((IList<ValidationToolRequestHandleParameter> list) =>
			list.Count == 2 && list.Any(r => r.ValidationRulePK == failure.FailedRuleResults[0].ValidationRule.PK) && list.Any(r => r.ValidationRulePK == failure.FailedRuleResults[1].ValidationRule.PK)))
		, Times.Once);
	}

	protected override BusinessObject GetNewBusinessObject() => new NonPersistentValidationFailure(Factory, ZString.Empty, ZString.Empty);
}
