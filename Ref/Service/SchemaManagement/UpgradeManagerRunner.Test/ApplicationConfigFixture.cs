using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class ApplicationConfigFixture
	{
		[Test]
		public void UserIdMustBeRefDbRepoAdmin()
		{
			var configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CargoWise.RefDbRepo.Service.UpgradeManagerRunner.config.json");
			Assert.True(File.Exists(configFile));

			var folderPath = Path.Combine(TestSourcePathHelper.DATTestSupplementaryContentPath, @"Service\SchemaManagement\UpgradeManagerRunner");
			var deployConfigJsonFiles = Directory.GetFiles(folderPath, "deploy.*.json");
			Assert.Greater(deployConfigJsonFiles.Length, 0);
			foreach (var jsonFile in deployConfigJsonFiles)
			{
				var contents = File.ReadAllText(jsonFile);
				var jObject = (JObject)JsonConvert.DeserializeObject(contents);
				var connectionString = jObject["ConnectionStrings"]["safe"].Value<string>();
				Assert.True(connectionString.Contains("User Id=RefDbRepoAdmin", StringComparison.OrdinalIgnoreCase));
			}
		}
	}
}
