using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business;

public sealed class NonPersistentValidationFailure : AutoNonPersistentValidationFailure
{
	public NonPersistentValidationFailure(BusinessObjectFactory factory, ZString actionSourceCode, ZString actionSourceDescription)
		: base(factory, actionSourceCode, actionSourceDescription)
	{
	}

	public NonPersistentRuleValidationResultCollection FailedRuleResults => failedRules ??= new NonPersistentRuleValidationResultCollection(Factory, true);
	NonPersistentRuleValidationResultCollection failedRules;

	public override ZString Message => Res.GetString("789cb9a1-61e9-4084-aa8a-54b3ac62a131", "Rules that failed validation for {0} - {1}", ActionSourceCode, ActionSourceDescription);

	public override ZString Status => (string)Severity switch
	{
		ProcessTemplateValidationSeverityList.Codes.Error => Res.GetString("5ea9f6f4-de3d-444a-a631-12b5b9ab556c", "Please fix errors before proceeding"),
		ProcessTemplateValidationSeverityList.Codes.Message => Res.GetString("26ba0d5c-cafe-4320-a930-b255c77f7e5c", "Requires Supervisor Override"),
		ProcessTemplateValidationSeverityList.Codes.Warning => Res.GetString("4b79db7d-e181-4249-ab15-c5dc041a373e", "Warnings only"),
		_ => default(ZString)
	};

	public override ZString Severity
	{
		get
		{
			if (FailedRuleResults.Errors.Any())
			{
				return ProcessTemplateValidationSeverityList.Codes.Error;
			}

			if (FailedRuleResults.MessageErrors.Any())
			{
				return ProcessTemplateValidationSeverityList.Codes.Message;
			}

			if (FailedRuleResults.Warnings.Any())
			{
				return ProcessTemplateValidationSeverityList.Codes.Warning;
			}

			return default;
		}
	}

	public bool HandleMessageErrors()
	{
		var businessObjects = FailedRuleResults.MessageErrors
			.Select(x => x.BusinessEntity as BusinessObject)
			.WhereNotNull()
			.ToHashSet();
		return businessObjects.Count == 0 || businessObjects.All(x => GetValidationToolMessageErrorsHandle(x) is not { } handle || handle.Handle());
	}

	IValidationToolMessageErrorsHandle GetValidationToolMessageErrorsHandle(BusinessObject businessObject)
	{
		var handles = ObjectFactory.Get<Hashtable>("ValidationToolMessageErrorsHandles");
		var handle = (ObjectHandle)handles[businessObject.TablePrefix];
		var cleanRevisedBusinessObject = Factory.ImportFromAnotherFactorySafe(businessObject);
		return (IValidationToolMessageErrorsHandle)handle?.GetObject(cleanRevisedBusinessObject);
	}

	public void HandleRequestsOnFailure()
	{
		if (!FailedRuleResults.Errors.Any() && FailedRuleResults.Any())
		{
			ObjectFactory.Get<IValidationToolRequestsHandle>().Handle(Factory, FailedRuleResults.MessageErrors.Concat(FailedRuleResults.Warnings).Select(x =>
				new ValidationToolRequestHandleParameter
				{
					BusinessEntity = x.BusinessEntity,
					ValidationRulePK = x.ValidationRule.PK,
					RequestTypePKOnFailure = x.ValidationRule.P0V_RQT_RequestTypeOnFailure
				}).ToList());
		}
	}
}
