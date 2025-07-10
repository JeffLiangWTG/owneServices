using CargoWise.RefDbRepo.DEReferenceData.CmdLine.Testing;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing
{
	[TestFixture]
	class CodeListsCmdLineTests
	{
		[Test]
		public void InvalidCodeListsExcludeTest()
		{
			CmdLineTests.AssertArgumentError("CODELISTS -EXCLUDE:NOTHING", $"Invalid CodeList Types to exclude entered: NOTHING");
		}

		[Test]
		public void InvalidCodeListsArgumentsTest()
		{
			CmdLineTests.AssertArgumentError("CODELISTS TOO MANY ARGEUMENTS", $"Invalid arguments entered -EXCLUDE:[functions]. Valid Code List Types are {string.Join(",", CodeListsConstants.ValidCodeLists)}");
		}
	}
}
