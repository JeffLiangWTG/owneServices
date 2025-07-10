using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Environment;
using NUnit.Framework;
using static Enterprise.Customs.Business.MessagingProcess.CustomsSupervisorOverrides;

namespace Enterprise.Customs.Business.Testing.MessagingProcess
{
	[TestedType(typeof(CustomsSupervisorOverrides))]
	sealed class CustomsSupervisorOverridesTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CreateCustomsSupervisorOverrides();

		CustomsSupervisorOverrides CreateCustomsSupervisorOverrides()
		{
			var bo = Factory.NewWithValidTestData<BaseJobDeclaration>();
			return new CustomsSupervisorOverrides(bo, Env.Security.AllowMessageErrors, CustomsSupervisorOverridesContext.SendMessagesWithErrors);
		}

		public void TestCreateMessages_SendMessagesWithErrors()
		{
			var overrides = CreateCustomsSupervisorOverrides();

			Env.Security.AllowMessageErrors.IsAllowed = true;

			overrides.CreateMessages();
			AssertEquals("One Log", 1, overrides.AuthorisedMessagesForLog.Count);
			var log = overrides.AuthorisedMessagesForLog[0];

			AssertEquals("Log message", "Sending with message errors", log.Message);
		}
	}
}
