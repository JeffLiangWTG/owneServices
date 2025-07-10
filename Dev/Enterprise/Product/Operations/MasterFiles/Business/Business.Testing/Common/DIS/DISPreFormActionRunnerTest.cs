using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.MasterFiles.Business.DIS.Testing
{
	sealed class DISPreFormActionRunnerTest : TestCaseWithFactory
	{
		public void TestExecute()
		{
			var disHostMock = new Mock<IDISHost>();
			disHostMock.Setup(m => m.NeedToDoPreFormAction()).Returns(false);
			var runner = new DISPreFormActionRunner(disHostMock.Object);
			AssertEquals(true, runner.Execute());
			disHostMock.VerifyAll();

			disHostMock.Setup(m => m.NeedToDoPreFormAction()).Returns(true);
			disHostMock.Setup(m => m.DoPreFormAction()).Returns(true);
			AssertEquals(true, runner.Execute());
			disHostMock.VerifyAll();

			disHostMock.Setup(m => m.NeedToDoPreFormAction()).Returns(true);
			disHostMock.Setup(m => m.DoPreFormAction()).Returns(false);
			AssertEquals(false, runner.Execute());
			disHostMock.VerifyAll();
		}
	}
}
