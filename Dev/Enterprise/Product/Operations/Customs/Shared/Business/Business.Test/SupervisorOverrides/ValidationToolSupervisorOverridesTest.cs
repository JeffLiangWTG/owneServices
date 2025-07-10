using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

[TestedType(typeof(ValidationToolSupervisorOverrides))]
sealed class ValidationToolSupervisorOverridesTest : NonPersistentBusinessObjectTestCase
{
	public void TestCreateMessages()
	{
		var declaration = Factory.New<BaseJobDeclaration>();
		var supervisorOverrides = new ValidationToolSupervisorOverrides(declaration);
		supervisorOverrides.CreateMessages();
		AssertEquals(true, supervisorOverrides.ShouldLogAuthorisedChanges || supervisorOverrides.SupervisorShouldApproveChanges);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<BaseJobDeclaration>();
		var supervisorOverrides = new ValidationToolSupervisorOverrides(declaration);
		return supervisorOverrides;
	}
}
