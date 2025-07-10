using CargoWise.RefDbRepo.IEReferenceData.CmdLine.Tests;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	[TestFixture]
	class CodeListCmdLineTests
	{
		[Test]
		public void InvalidCodeListsExcludeTest()
		{
			CmdLineTest.AssertArgumentError("CODELISTS -EXCLUDE:NOTHING", $"Invalid CodeList Types to exclude entered: NOTHING");
		}

		[Test]
		public void InvalidCodeListsArgumentsTest()
		{
			CmdLineTest.AssertArgumentError("CODELISTS TOO MANY ARGUMENTS", $"Invalid arguments entered -EXCLUDE:[functions]. Valid Code List Types are {string.Join(",", Constants.CodeListConstants.ValidCodeLists)}");
		}
	}
}
