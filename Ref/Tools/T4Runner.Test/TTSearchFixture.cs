using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.T4Runner.Test
{
	[TestFixture]
	public class TTSearchFixture
	{
		[Test]
		public void IsFindingTTFilesInADirectory()
		{
			Assert.True(TTSearch.GetTTFiles(DirectoryLookup.First()).Any());
		}

		[Test]
		public void IsFindingDirectoriesWithTTFiles()
		{
			Assert.True(DirectoryLookup.Any());
		}

		IEnumerable<string> DirectoryLookup => new string[] { Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "T4TestResources") };
	}
}
