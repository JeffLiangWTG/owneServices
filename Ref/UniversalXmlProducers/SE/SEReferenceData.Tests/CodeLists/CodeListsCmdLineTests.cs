using CargoWise.RefDbRepo.SEReferenceData.CodeLists.Business;
using CargoWise.RefDbRepo.SEReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SEReferenceData.CodeLists.Tests
{
	[TestFixture]
	class CodeListsCmdLineTests
	{
		[Test]
		public void InvalidCodeListsExcludeTest()
		{
			CmdLineTest.AssertArgumentError("CODELISTS -EXCLUDE:NOTHING", "Invalid CodeList Types to exclude entered: NOTHING");
		}

		[Test]
		public void InvalidCodeListsArgumentsTest()
		{
			CmdLineTest.AssertArgumentError("CODELISTS TOO MANY ARGEUMENTS", $"Invalid arguments entered -EXCLUDE:[functions]. Valid Code List Types are {string.Join(",", CodeListConstants.ValidCodeLists)}");
		}
	}
}
