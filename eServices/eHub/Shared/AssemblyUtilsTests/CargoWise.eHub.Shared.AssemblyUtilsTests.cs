using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using AssemblyUtils;
using System.Reflection;

namespace CargoWise.eHub.Shared.AssemblyUtilsTests
{
    [TestFixture]
    class AssemblyUtilsTests
	{
		[Test]
		public void TesteHubAssemblyNames_ValidBinPath_DoesNotThrowException()
		{
			IEnumerable<string> assemblies = null;
			TestDelegate action = () => assemblies = AssemblyChecker.GetAssemblyNamesWithReferencing(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			IEnumerable<string> mandatoryAssembies = new List<string>() {"Microsoft.BizTalk.CommonSettings.dll", "Microsoft.BizTalk.Messaging.dll", "Microsoft.BizTalk.Operations.dll", 
										"Microsoft.BizTalk.Pipeline.dll", "Microsoft.BizTalk.Streaming.dll", "CargoWise.eHub.DataAccess.Sql.dll",
											"CargoWise.eHub.Gateway.Pipelines.dll", "CargoWise.eHub.Gateway.Schemas.dll", "CargoWise.eHub.Core.Logging.dll"};

			Assert.That(action, Throws.Nothing);
			Assert.That(assemblies, Has.Length.AtLeast(500));
			Assert.That(mandatoryAssembies.Except(assemblies).Any(), Is.False);
		}

		[Test]
		public void TesteHubAssemblyNames_InvalidBinPath_ThrowsException()
		{
			Assert.That(() => AssemblyChecker.GetAssemblyNamesWithReferencing("C:\\BS\\git\\wtg\\eServices\\eServices\\eHub\\Bin\\InvalidBinPath\\InvalidSubPath"), Throws.TypeOf<DirectoryNotFoundException>());
		}

		#region AssemblyChecker

		AssemblyChecker assemblyChecker;

		AssemblyChecker AssemblyChecker
		{
			get
			{
				return assemblyChecker ?? (assemblyChecker = new AssemblyChecker());
			}
		}

		#endregion
	}
}
