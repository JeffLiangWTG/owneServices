using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[TestFixture]
	internal class PublishProfilesFixture
	{
		[Test]
		public void PublishProfilesShouldHaveConfigTransform()
		{
			var localDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);
			var rootPath = Environment.GetEnvironmentVariable(TestSourcePathHelper.DAT_TestSupplementaryContentPath) ?? Path.Combine(localDirectory, @"..\..\..");
			foreach (var folderName in searchFolders)
			{
				var profiles = Directory.GetFiles(Path.Combine(rootPath, folderName), "*.pubxml", SearchOption.AllDirectories);
				if (profiles != null && profiles.Length > 0)
				{
					foreach (var profile in profiles)
					{
						if (!profile.Contains(@"Properties\PublishProfiles") || ignoreFolders.Any(x => profile.Contains(x)))
						{
							continue;
						}
						var profileFullPath = Path.GetFullPath(profile);
						var configFileName = $"Web.{Path.GetFileNameWithoutExtension(profile)}.config";
						var configFilePath = Path.Combine(profileFullPath, @"..\..\..\");
						Assert.True(File.Exists(Path.Combine(configFilePath, configFileName)), $"{profileFullPath} should have corresponding web config file.");
					}
				}
			}
		}

		readonly string[] searchFolders = new[] { "Service", "Staging", "UniversalXmlProducers" };
		readonly string[] ignoreFolders = new[] { "NewService", "NewSafeDataUpdateService", "NewStaging" };
	}
}
