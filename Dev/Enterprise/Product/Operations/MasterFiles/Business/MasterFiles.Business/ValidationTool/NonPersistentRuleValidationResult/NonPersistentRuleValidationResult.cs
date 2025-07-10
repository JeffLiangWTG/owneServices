using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business;

public sealed class NonPersistentRuleValidationResult : AutoNonPersistentRuleValidationResult
{
	readonly ZBool _logValidationFailEvent;

	public NonPersistentRuleValidationResult(IProcessTemplateValidation rule, IBusiness businessEntity, ZString severity, ZString message, ZBool passed, Func<ZPropertyInfo> propertyInfoGetter)
		: base(Argument.NotNull(rule, nameof(rule)).Factory, rule.P0V_Description, severity, message, passed)
	{
		BusinessEntity = Argument.NotNull(businessEntity, nameof(businessEntity));
		ValidationRule = rule;
		PropertyInfoGetter = propertyInfoGetter;
		_logValidationFailEvent = rule.P0V_LogValidationFailEvent;
	}

	public IBusiness BusinessEntity { get; }
	public IProcessTemplateValidation ValidationRule { get; }

	public Func<ZPropertyInfo> PropertyInfoGetter { get; }

	public override ZString Message
	{
		get => base.Message.IfEmptyUse(() => (ZString)Enterprise.MasterFiles.Business.Res.GetString("c9781fbf-68fa-4b88-a169-6a6c29336340", "Validation Failed"));
	}

	public void DisplayValidationOnField()
	{
		if (Passed || Message.IsEmpty || PropertyInfoGetter?.Invoke() is not { } fieldToDisplayValidationPropertyInfo)
		{
			return;
		}

		switch ((string)Severity)
		{
			case ProcessTemplateValidationSeverityList.Codes.Warning:
				fieldToDisplayValidationPropertyInfo.AddWarningWithoutValidationCheck(Message);
				break;
			case ProcessTemplateValidationSeverityList.Codes.Message:
				fieldToDisplayValidationPropertyInfo.AddMessageErrorWithoutValidationCheck(Message);
				break;
			case ProcessTemplateValidationSeverityList.Codes.Error:
				fieldToDisplayValidationPropertyInfo.AddErrorWithoutValidationCheck(Message);
				break;
		}
	}

	public void LogValidationFailEvent()
	{
		if (Passed ||
			!_logValidationFailEvent ||
			BusinessEntity is not BusinessObject businessObject ||
			Factory.ImportFromAnotherFactorySafe(businessObject) is not { } cleanRevisedBusinessObject ||
			cleanRevisedBusinessObject is not IStmALogParent logParent)
		{
			return;
		}

		DoNotSaveCleanRevisedBusinessObject();
		logParent.Logs.AddNew(Events.ValidationRuleFailed, Message);

		return;

		void DoNotSaveCleanRevisedBusinessObject()
		{
			cleanRevisedBusinessObject.IsNull = true;
		}
	}
}
