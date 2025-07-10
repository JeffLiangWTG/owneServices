using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestECF_Name()
		{
			var bizo = Factory.New<EDIMessageContentFilter>();
			bizo.ECF_Name = "";
			AssertMandatoryValidationError(bizo.ECF_NameInfo, true);
			bizo.ECF_Name = "Midges";
			AssertMandatoryValidationError(bizo.ECF_NameInfo, false);
		}
	}
}
