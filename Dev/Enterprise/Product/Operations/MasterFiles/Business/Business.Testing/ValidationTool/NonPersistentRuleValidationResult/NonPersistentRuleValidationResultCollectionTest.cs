using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(NonPersistentRuleValidationResultCollection))]
sealed class NonPersistentRuleValidationResultCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentRuleValidationResultCollection>
{
	public void TestAllowNew()
	{
		AssertEquals(false, GetCollectionToTest().AllowNew);
	}

	public void TestAllowRemove()
	{
		AssertEquals(false, GetCollectionToTest().AllowRemove);
	}

	public override void TestAddNew()
	{
		AssertExceptionThrown<NotSupportedException>(() => GetCollectionToTest().AddNew());
	}

	public void TestAddFailedRuleResult_CannotAddPassedResult()
	{
		AssertExceptionThrown<ArgumentException>(() => new NonPersistentRuleValidationResultCollection(Factory, true).Add(NonPersistentRuleValidationResultBuilder.Pass().Build()));
	}

	public void TestErrors() => CombineAssertions(() =>
	{
		var collection = GetCollectionToTest();
		AssertEquals("Default", false, collection.Errors.Any());

		collection.Add(NonPersistentRuleValidationResultBuilder.Pass().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
		collection.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
		AssertEquals("Two", 2, collection.Errors.Count());
	});

	public void TestMessageErrors() => CombineAssertions(() =>
	{
		var collection = GetCollectionToTest();
		AssertEquals("Default", false, collection.MessageErrors.Any());

		collection.Add(NonPersistentRuleValidationResultBuilder.Pass().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		collection.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		AssertEquals("Two", 2, collection.MessageErrors.Count());
	});

	public void TestWarnings() => CombineAssertions(() =>
	{
		var collection = GetCollectionToTest();
		AssertEquals("Default", false, collection.Warnings.Any());

		collection.Add(NonPersistentRuleValidationResultBuilder.Pass().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		collection.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		AssertEquals("Two", 2, collection.Warnings.Count());
	});

	protected override NonPersistentRuleValidationResultCollection GetCollectionToTest() => new(Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection() => NonPersistentRuleValidationResultBuilder.Fail().Build();
}
