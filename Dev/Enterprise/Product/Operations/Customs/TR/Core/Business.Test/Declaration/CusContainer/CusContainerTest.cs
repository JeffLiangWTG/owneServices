using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusContainer))]
	class CusContainerTest : EU.Business.Declaration.Testing.CusContainerTest
	{
		public void TestValidation()
		{
			AssertType<CusContainerValidation>(Factory.New<CusContainer>().Validation);
		}
	}
}
