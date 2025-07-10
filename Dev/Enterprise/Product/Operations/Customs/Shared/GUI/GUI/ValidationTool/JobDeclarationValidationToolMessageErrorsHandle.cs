using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GUI;

public sealed class JobDeclarationValidationToolMessageErrorsHandle : IValidationToolMessageErrorsHandle
{
	readonly BaseJobDeclaration declaration;

	public JobDeclarationValidationToolMessageErrorsHandle(BaseJobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	public bool Handle()
	{
		var supervisorOverrides = new ValidationToolSupervisorOverrides(declaration);
		return SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, declaration.Logs);
	}
}
