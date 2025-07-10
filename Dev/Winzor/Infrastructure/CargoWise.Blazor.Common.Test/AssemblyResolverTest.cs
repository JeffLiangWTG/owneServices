using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace CargoWise.Blazor.Common.Test
{
	public class AssemblyResolverTest
	{
		[Test]
		public void AssemblyFilesInDirectoryShouldBeSameToAssemblyResolvingPaths()
		{
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			var searchedPaths = Directory.GetFiles(BinDirectory, "CargoWise.Blazor.Common.dll", SearchOption.AllDirectories);
			stopwatch.Stop();
			if (stopwatch.Elapsed.TotalSeconds > 2)
			{
				Assert.Fail($"Directory.GetFiles cost {stopwatch.Elapsed.TotalSeconds} seconds");
			}

			var assemblyPathList = AssemblyResolver.AssemblyResolvingPaths;
			var relativePathList = searchedPaths
				.Select(commonAssembly => Path.GetRelativePath(BinDirectory, Path.GetDirectoryName(commonAssembly)))
				.ToList();

			bool match = relativePathList.OrderBy(x => x).SequenceEqual(assemblyPathList.OrderBy(x => x));
			if (match)
			{
				return;
			}

			// In theory, this unit test should not fail, but in fact, DAT has many timeouts or matching failures.
			// I suspect that there are differences in the assembly loading process and that searchFile is too time-consuming.
			// Therefore, I added some confirmation information above to facilitate quick confirmation of core information
			// when problems occur in the future.
			Assert.Multiple(() =>
			{
				Assert.That(relativePathList, Is.Empty, "relativePathList value is");
				Assert.That(assemblyPathList, Is.Empty, "assemblyPathList value is");
				var assemblyResolverLocation = typeof(AssemblyResolver).Assembly.Location;
				Assert.That(assemblyResolverLocation, Is.Empty, "assemblyResolverLocation value is");
			});
		}

		[Test]
		public void AssemblySearchDirectoriesShouldHaveCorrectPaths()
		{
			Assert.That(Assembly.GetExecutingAssembly().Location, Does.EndWith("\\winzor\\CargoWise.Blazor.Common.Test.dll"));

			var expectedPaths = new List<string>
			{
				BinDirectory,
				$"{BinDirectory}\\winzor",
#if USE_NET_CORE_LIBS_IN_WINZOR
				$"{BinDirectory}\\net8.0",
				RuntimeEnvironment.GetRuntimeDirectory(),
#endif
				Environment.ExpandEnvironmentVariables(@"%WINDIR%\Microsoft.NET\Framework\v4.0.30319"),
			};
			Assert.That(AssemblyResolver.AssemblySearchDirectories, Is.EquivalentTo(expectedPaths));
		}

		[Test]
		public void GetBinDirectoryReturnsCorrectPath()
		{
			var result = AssemblyResolver.GetRootDirectory($"{BinDirectory}\\winzor");
			Assert.That(result, Is.EqualTo(BinDirectory));

			result = AssemblyResolver.GetRootDirectory($"{BinDirectory}\\AppServer");
			Assert.That(result, Is.EqualTo(BinDirectory));

			result = AssemblyResolver.GetRootDirectory($"{BinDirectory}\\CargoWise.Blazor.SessionBroker.Test-bin");
			Assert.That(result, Is.EqualTo(BinDirectory));

			var exception = Assert.Throws<InvalidOperationException>(
				() => AssemblyResolver.GetRootDirectory(BinDirectory));
			Assert.That(exception.Message, Does.Contain("does not contain any of the expected paths"));
		}

		[Test]
		public void GetBinDirectoryIsCaseInsensitive()
		{
			var result = AssemblyResolver.GetRootDirectory($"{BinDirectory}\\wInZor");
			Assert.That(result, Is.EqualTo(BinDirectory));
		}

		[Test]
		public void GetBinDirectoryThrowsIfRunningFromWrongPath()
		{
			var exception = Assert.Throws<InvalidOperationException>(
				() => AssemblyResolver.GetRootDirectory("C:\\"));
			Assert.That(exception.Message, Does.Contain("does not contain any of the expected paths"));

			exception = Assert.Throws<InvalidOperationException>(
				() => AssemblyResolver.GetRootDirectory("C:\\AppServer"));
			Assert.That(exception.Message, Does.Contain("does not contain the expected Winzor directory."));

			exception = Assert.Throws<InvalidOperationException>(
				() => AssemblyResolver.GetRootDirectory($"{BinDirectory}\\winzor\\winzor"));
			Assert.That(exception.Message, Does.Contain("does not contain the expected Winzor directory."));

			exception = Assert.Throws<InvalidOperationException>(
				() => AssemblyResolver.GetRootDirectory($"{BinDirectory}\\winzor\\AppServer"));
			Assert.That(exception.Message, Does.Contain("does not contain the expected Winzor directory."));

			exception = Assert.Throws<InvalidOperationException>(
				() => AssemblyResolver.GetRootDirectory($"{BinDirectory}\\winzor\\wwwroot\\images"));
			Assert.That(exception.Message, Does.Contain("does not contain any of the expected paths"));

			exception = Assert.Throws<InvalidOperationException>(
				() => AssemblyResolver.GetRootDirectory($"{BinDirectory}\\net8.0\\runtimes"));
			Assert.That(exception.Message, Does.Contain("does not contain any of the expected paths"));
		}

		readonly string BinDirectory
			 = (new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).Parent.FullName;
	}
}
