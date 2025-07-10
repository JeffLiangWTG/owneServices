using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[TestFixture]
	class SdkVersionFixture
	{
		[Test]
		public void CheckSdkVersion()
		{
			var rootPath = Path.Combine(TestSourcePathHelper.DATTestSupplementaryContentPath);
			var fileName = "global.json";
			var globalJsonFilePath = Path.Combine(rootPath, fileName);
			if(!File.Exists(globalJsonFilePath))
			{
				globalJsonFilePath = Path.Combine(rootPath, "..\\", fileName);
			}
			var contents = File.ReadAllText(globalJsonFilePath);
			var jObject = (JObject)JsonConvert.DeserializeObject(contents);
			var version = jObject["sdk"]["version"].Value<string>();
			var majorVersion = int.Parse(version.Split('.')[0], CultureInfo.InvariantCulture);
			Assert.AreEqual(8, majorVersion, "sdk major version should be 8 in global.json file.");
		}
	}
}
