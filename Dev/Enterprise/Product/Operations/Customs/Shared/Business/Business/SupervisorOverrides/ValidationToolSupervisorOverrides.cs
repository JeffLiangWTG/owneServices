using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Customs.Business;

public sealed class ValidationToolSupervisorOverrides : SupervisorOverrides
{
	public ValidationToolSupervisorOverrides(BaseJobDeclaration declaration) : base(declaration, string.Empty)
	{
	}

	protected override void CreateMessagesCore()
	{
		CheckMessageErrors(businessEntity);
	}

	protected override void CheckMessageErrors(IBusiness entity)
	{
		if (CheckSecurityRightForCheckpointRequired(Env.Security.AllowMessageErrors))
		{
			AddMessageLog(Env.Security.AllowMessageErrors, Constants.DeclarationHasAnyMessageErrors);
		}
	}
}
