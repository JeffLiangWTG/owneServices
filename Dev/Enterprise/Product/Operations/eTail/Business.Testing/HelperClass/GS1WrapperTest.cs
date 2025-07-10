using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.eTail.Business.Testing
{
	public class GS1WrapperTest : TestCaseWithFactory
	{
		public void TestGenerateSSCCNumber()
		{
			var numberFountain = Mock.Of<INumberFountainProxy>();
			Mock.Get(numberFountain).Setup(fountain => fountain.GetNextFormatted(Factory)).Returns("TestSSCCNumber");

			var wrapper = new GS1Wrapper(Factory.New<OrgHeader>(), string.Empty, numberFountain);
			AssertEquals("Should generate SSCC number from number fountain", "TestSSCCNumber", wrapper.GenerateSSCCNumber(Factory));
		}
	}
}
