using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.Business.Testing
{
	public class CusClassificationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCC_IsActive()
		{
			var classification = Factory.NewWithValidTestData<BaseCusClassification>();
			classification.CC_IsActive = true;
			Factory.Save();
			Env.Security.CusClassificationDeactivate.IsAllowed = false;
			classification.CC_IsActive = false;
			AssertHasError(classification.CC_IsActiveInfo, "You don't have rights to deactivate classifications.");

			Env.Security.CusClassificationDeactivate.IsAllowed = true;
			classification.CC_IsActive = true;
			classification.CC_IsActive = false;
			AssertNoError(classification.CC_IsActiveInfo, "You don't have rights to deactivate classifications.");
		}
	}
}
