using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing;

public sealed class NonPersistentRuleValidationResultBuilder
{
	NonPersistentRuleValidationResultBuilder()
	{
	}

	IProcessTemplateValidation Rule { get; set; }
	IBusiness BusinessEntity { get; set; }
	string Severity { get; set; }
	string Message { get; set; }
	bool Passed { get; set; }
	Func<ZPropertyInfo> PropertyGetter { get; set; }

	public static NonPersistentRuleValidationResultBuilder Pass()
	{
		return new NonPersistentRuleValidationResultBuilder().WithPassed(true);
	}

	public static NonPersistentRuleValidationResultBuilder Fail()
	{
		return new NonPersistentRuleValidationResultBuilder().WithPassed(false);
	}

	public NonPersistentRuleValidationResultBuilder WithRule(IProcessTemplateValidation rule)
	{
		Rule = rule;
		return this;
	}

	public NonPersistentRuleValidationResultBuilder WithEntity(IBusiness businessEntity)
	{
		BusinessEntity = businessEntity;
		return this;
	}

	public NonPersistentRuleValidationResultBuilder WithSeverity(string severity)
	{
		Severity = severity;
		return this;
	}

	public NonPersistentRuleValidationResultBuilder WithMessage(string message)
	{
		Message = message;
		return this;
	}

	NonPersistentRuleValidationResultBuilder WithPassed(bool passed)
	{
		Passed = passed;
		return this;
	}

	public NonPersistentRuleValidationResultBuilder WithTargetProperty(Func<ZPropertyInfo> propertyGetter)
	{
		PropertyGetter = propertyGetter;
		return this;
	}

	public NonPersistentRuleValidationResult Build(bool safe = true) => safe
		? new NonPersistentRuleValidationResult(Rule ?? Mock.Of<IProcessTemplateValidation>(), BusinessEntity ?? Mock.Of<IBusiness>(), Severity, Message, Passed, PropertyGetter)
		: new NonPersistentRuleValidationResult(Rule, BusinessEntity, Severity, Message, Passed, PropertyGetter);
}
