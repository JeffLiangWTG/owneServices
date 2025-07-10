using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.CmdLine.Testing
{
	[TestFixture]
	class CmdLineTests
	{
		[Test]
		public void TestEmptyArgument()
		{
			AssertArgumentError(string.Empty, "No arguments entered.");
		}

		[Test]
		public void TestInvalidFunction()
		{
			AssertArgumentError("NoFUNCTION", "Invalid argument entered: NOFUNCTION");
		}

		[Test]
		public void TestAllCodeListParsersAreMapped()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var cmdLineAssembly = Assembly.LoadFile(Path.Combine(binPath, "CargoWise.RefDbRepo.DEReferenceData.CmdLine.dll"));
			var codeListsProgramClass = cmdLineAssembly.GetType("CargoWise.RefDbRepo.DEReferenceData.CmdLine.CodeListsProgram");
			var getFunctionsToRun = codeListsProgramClass.GetMethod("GetFunctionsToRun", BindingFlags.Static | BindingFlags.NonPublic);
			using (var httpClient = new HttpClient())
			{
				var functionsToRun = (Dictionary<string, Action>)getFunctionsToRun.Invoke(null, new object[] { httpClient, string.Empty, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), new Dictionary<string, string>() });
				foreach (var validCodeList in CodeListsConstants.ValidCodeLists)
				{
					Assert.That(functionsToRun.Keys, Does.Contain(validCodeList));
				}
			}
		}

		internal static void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var processStartInfo = new ProcessStartInfo(Path.Combine(binPath, "CargoWise.RefDbRepo.DEReferenceData.CmdLine.exe"), parameters)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");
			var message = ProcessRunner.RunProcess(processStartInfo, false);
			StringAssert.Contains(expectedErrorMessage, message);
		}
	}
}
