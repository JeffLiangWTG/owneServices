using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration;

public interface IProcessTemplateValidationManager
{
	void ValidateOnValidateAll(IBusiness businessEntity);
	void ValidateOnSave(IBusiness businessEntity, Action originalAction);
	void Validate(ZString actionSourceCode, IBusiness businessEntity, Action originalAction);
	void InjectFieldRules(IBusiness businessEntity);
}
