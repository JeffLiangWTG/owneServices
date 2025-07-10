using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AdditionalValidatorResultTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when notificationType param is null", () => new AdditionalValidatorResult("message", null));
		}

		public void TestProperties()
		{
			var additionalValidatorResult = new AdditionalValidatorResult("Validation Message", NotificationType.Error);

			CombineAssertions("Assert properties", () =>
			{
				AssertEquals("ValidationMessage", "Validation Message", additionalValidatorResult.ValidationMessage);
				AssertEquals("NotificationType", NotificationType.Error, additionalValidatorResult.NotificationType);
			});
		}
	}
}
