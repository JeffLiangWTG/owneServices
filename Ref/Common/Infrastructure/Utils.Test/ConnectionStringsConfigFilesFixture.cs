using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class ConnectionStringsConfigFilesFixture
	{
		[Test]
		public void CheckAllConfigFiles()
		{
			var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var binFolder = Directory.GetParent(currentFolder).FullName;
			var folders = Directory.GetDirectories(binFolder);
			var allConfigFiles = new List<string>();
			foreach (var folder in folders)
			{
				var configFiles = Directory.GetFiles(folder, "*.config").Where(x => x.EndsWith("exe.config") || x.EndsWith("dll.config"));
				if (configFiles.Any())
				{
					allConfigFiles.AddRange(configFiles);
				}
			}

			foreach (var configFile in allConfigFiles)
			{
				var document = new XmlDocument();
				document.Load(configFile);
				var connectionStringNode = document.DocumentElement.SelectSingleNode("connectionStrings");
				if (connectionStringNode != null)
				{
					if (connectionStringNode.HasChildNodes)
					{
						var names = connectionStringNode.ChildNodes.Cast<XmlNode>().Select(x => x.Attributes["name"].Value);
						Assert.False(names.Any(x => connectionStringEntities.Contains(x)), $"connectionStrings shouldn't contain ReferenceDataEntities or RefDbRepoStagingEntities in {configFile}");
					}
					else
					{
						var expectedConfigSource = configFileName;
						var actualConfigSource = connectionStringNode.Attributes["configSource"].Value;
						if (configFile.EndsWith("CargoWise.RefDbRepo.Staging.Service.dll.config"))
						{
							expectedConfigSource = "bin\\" + configFileName;
						}
						Assert.AreEqual(expectedConfigSource, actualConfigSource, $"configSource attribute should be {expectedConfigSource} in {configFile}");
					}
				}
			}
		}

		readonly string configFileName = "ConnectionStrings.config";
		readonly string[] connectionStringEntities = new[] { "ReferenceDataEntities", "RefDbRepoStagingEntities" };
	}
}
