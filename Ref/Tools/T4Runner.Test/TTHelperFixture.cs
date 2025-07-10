using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.T4Runner.Test
{
	[TestFixture]
	public class TTHelperFixture
	{
		[Test]
		public void GetTTInfoFromCsprojFile()
		{
			var directory = DirectoryLookup.First();
			var ttFile = TTSearch.GetTTFiles(directory).First();

			var ttFileInfo = TTHelper.GetTTConfigurationFromProjectContentDefinition(directory, new string[] { ttFile }).FirstOrDefault();
			Assert.NotNull(ttFileInfo);
			Assert.AreEqual("CargoWise.RefDbRepo.T4Runner.Test.T4TestResources", ttFileInfo.NamespaceName);
			Assert.AreEqual(directory, ttFileInfo.DirectoryPath);
			Assert.AreEqual(ttFile, ttFileInfo.FileName);
		}

		IEnumerable<string> DirectoryLookup => new string[] { Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "T4TestResources") };
	}
}
