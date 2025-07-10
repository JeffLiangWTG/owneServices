using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business;

public class ProcessTemplateValidationConditionChecker : IEvaluateRuleWithTimeout
{
	public ProcessTemplateValidationConditionChecker(IProcessTemplateValidation validationRule, IBusiness businessEntity)
	{
		ValidationRule = Argument.NotNull(validationRule, nameof(validationRule));
		BusinessEntity = Argument.NotNull(businessEntity, nameof(businessEntity));
	}

	protected IProcessTemplateValidation ValidationRule { get; }

	protected IBusiness BusinessEntity { get; }

	protected virtual IBusiness EntityToEvaluateMacro => BusinessEntity;

	public bool AreConditionsMet()
	{
		var condition1 = ValidationRule.P0V_Condition1;
		var condition2 = ValidationRule.P0V_Condition2;
		var condition2Value = ValidationRule.P0V_Condition2Value;
		var company = ValidationRule.P0V_GC_Company;
		return (condition1.IsEmpty || IsCondition1Met(condition1)) &&
				IsCondition2Met(condition2, condition2Value) &&
				(company.IsEmpty || IsCompanyMet(company)) &&
				(ValidationRule.P0V_ContextType == ProcessTemplateValidationContextType.Codes.CargoWise);
	}

	protected virtual bool IsCondition1Met(ZString conditionCode) => false;

	bool IsCondition2Met(ZString condition2, ZString condition2Value)
	{
		if (condition2.IsEmpty)
		{
			return true;
		}

		if (condition2.EqualsIgnoringCase(ProcessTasksLookups.MacroCondition))
		{
			var entity = EntityToEvaluateMacro;
			if (entity is null)
			{
				return false;
			}

			using (var macroContext = ObjectFactory.Get<IWorkflowMacroContextDecider>().GetDefaultWorkflowMacroContext(entity.Factory, entity))
			{
				var result = ObjectFactory.Get<IWorkflowMacroValueEvaluator>().EvaluateBooleanExpression(entity.Factory, macroContext, condition2Value);
				return result ?? false;
			}
		}

		return IsCondition2MetCore(condition2, condition2Value);
	}

	protected virtual bool IsCondition2MetCore(ZString condition2Code, ZString condition2Value) => false;

	protected virtual bool IsCompanyMet(ZGuid company) => false;

	public NonPersistentRuleValidationResult EvaluateRule()
	{
		var entity = Argument.NotNull(EntityToEvaluateMacro, nameof(EntityToEvaluateMacro));

		var factory = entity.Factory;
		using var suppressReporting = factory.ThreadSentry.SuppressReporting();
		var macroEvaluator = ObjectFactory.Get<IWorkflowMacroValueEvaluator>();
		var (passed, timeout) = ((IEvaluateRuleWithTimeout)this).EvaluateWithTimeout(() =>
		{
			using var macroContext = ObjectFactory.Get<IWorkflowMacroContextDecider>().GetDefaultWorkflowMacroContext(factory, entity);
			return macroEvaluator.EvaluateBooleanExpression(factory, macroContext, ValidationRule.P0V_ValidationRule) ?? false;
		});
		var message = timeout ? (ZString)Res.GetString("a84496e2-87b7-4c40-9976-0fd6bac2135f", "Rule failed as it has exceeded the timeout threshold setup in the Registry : Workflow Manager > Workflow Templates > Validation Rule Timeout.")
			: (ZString)macroEvaluator.GetMacroValue<string>(factory, entity, MacroStringHelper.WrapWithQuotes(ValidationRule.P0V_Message));
		return new NonPersistentRuleValidationResult(ValidationRule, entity, ValidationRule.P0V_Severity, message, passed, () => FieldToDisplayValidationZPropertyInfo);
	}

	public ZPropertyInfo FieldToDisplayValidationZPropertyInfo => fieldToDisplayValidationZPropertyInfo ??= GetFieldToDisplayValidationZPropertyInfo();
	ZPropertyInfo fieldToDisplayValidationZPropertyInfo;

	ZPropertyInfo GetFieldToDisplayValidationZPropertyInfo()
	{
		var entity = Argument.NotNull(EntityToEvaluateMacro, nameof(EntityToEvaluateMacro));

		return new FieldToDisplayValidationResolver(ValidationRule.P0V_FieldToDisplayValidation).GetFieldToDisplayValidationZPropertyInfo(entity);
	}

	(bool Passed, bool Timeout) IEvaluateRuleWithTimeout.EvaluateWithTimeout(Func<bool> evaluation)
	{
		var validationRuleTimeout = TimeSpan.FromSeconds(WorkflowDataRegistry.Instance.ValidationRuleTimeout.Value);

		using var cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = cancellationTokenSource.Token;
		var task = DBConnectionDisposalAsyncStrategy.Get()
			.GetAsync(() => !cancellationToken.IsCancellationRequested && evaluation(), cancellationTokenSource: cancellationTokenSource);
		if (task.Wait(validationRuleTimeout))
		{
			return (task.Result, false);
		}

		cancellationTokenSource.Cancel();
		return (false, true);
	}
}
