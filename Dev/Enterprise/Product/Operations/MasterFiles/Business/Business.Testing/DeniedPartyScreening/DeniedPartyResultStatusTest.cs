using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DeniedPartyResultStatusTest : TestCaseWithFactory
	{
		public void TestConstructorWithParams()
		{
			var header = Factory.New<OrgHeader>();
			var result = new DpsResultStatus(header, "CLR");
			CombineAssertions(() =>
			{
				AssertEquals(header, result.ScreeningEntity);
				AssertEquals("CLR", result.Status);
			});
		}
	}
}
