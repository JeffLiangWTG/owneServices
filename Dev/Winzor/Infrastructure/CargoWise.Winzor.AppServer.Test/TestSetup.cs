using CargoWise.Blazor.Common;
using NUnit.Framework;

namespace CargoWise.Winzor.AppServer.Test
{
	[SetUpFixture]
	public class TestSetup
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			AssemblyResolver.Setup();
		}
	}
}
