using CargoWise.Blazor.Common;
using WinzorFramework;

[SetUpFixture]
public class WinFormsTestSetup
{
	[OneTimeSetUp]
	public void RunBeforeAnyTests()
	{
		AssemblyResolver.Setup();
	}

	[OneTimeTearDown]
	public void AfterRunningAllTests()
	{
		WinzorDispatcher.Dispose();
	}

	public static readonly WinzorDispatcher WinzorDispatcher = new WinzorDispatcher(new FormOpener(), new RegisteredFormInstances());
}
