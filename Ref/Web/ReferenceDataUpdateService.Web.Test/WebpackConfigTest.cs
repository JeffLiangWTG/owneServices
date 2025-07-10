using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ReferenceDataUpdateService.Web.Test
{
	[TestFixture]
	class WebpackConfigTest
	{
		[Test]
		public void WebpackConfigUseRelativeUrls()
		{
			Assert.Greater(webpackFiles.Length, 0);

			foreach (var file in webpackFiles)
			{
				var configDictionary = GetConfigDictionaryFromatFile(file);
				if (configDictionary.Keys.Count > 0)
				{
					var safeAPI = configDictionary["SafeAPI"];
					var stagingAPI = configDictionary["StagingAPI"];
					Assert.False(safeAPI.Contains("http"));
					Assert.False(stagingAPI.Contains("http"));
				}
			}
		}

		[Test]
		public void AuthorityAndAudienceSameWithSafeUpdateService()
		{
			foreach (var configFile in updateServiceConfigFiles)
			{
				var webpackFile = GetWebpackJsFileByServiceConfigFile(configFile);
				var configDic = GetConfigDictionaryFromatFile(configFile);
				var webpackConfigDic = GetConfigDictionaryFromatFile(webpackFile);
				Assert.AreEqual(configDic["OIDCAuthority"], webpackConfigDic["Authority"], "SafeUpdateService should have same Authority config with Portal");
				Assert.AreEqual(configDic["OIDCAudience"], webpackConfigDic["ClientId"], "SafeUpdateService should have same ClientId config with Portal");
			}
		}

		[Test]
		public void AuthorityAndAudienceSameWithStagingService()
		{
			foreach (var configFile in stagingServiceConfigFiles)
			{
				var configFileName = Path.GetFileName(configFile);
				var webpackFile = GetWebpackJsFileByServiceConfigFile(configFile);
				var configDic = GetConfigDictionaryFromatFile(configFile);
				var webpackConfigDic = GetConfigDictionaryFromatFile(webpackFile);
				Assert.AreEqual(configDic["OIDCAuthority"], webpackConfigDic["Authority"], $"StagingService should have same Authority config with Portal, Please check {configFileName}");
				Assert.AreEqual(configDic["OIDCAudience"], webpackConfigDic["ClientId"], $"StagingService should have same ClientId config with Portal, Please check {configFileName}");
			}
		}

		string GetWebpackJsFileByServiceConfigFile(string configFilePath)
		{
			var fileName = Path.GetFileNameWithoutExtension(configFilePath);
			var splitNames = fileName.Split('.');
			var env = splitNames.Length == 3 ? splitNames[1] : "DEV";
			var webpackFile = webpackFiles.FirstOrDefault(x => x.Contains($"webpack.{env}.js", StringComparison.OrdinalIgnoreCase));
			return webpackFile;
		}

		Dictionary<string, string> GetConfigDictionaryFromatFile(string configFilePath)
		{
			var configDictionary = new Dictionary<string, string>();
			var allText = File.ReadAllText(configFilePath).Trim();
			var fileName = Path.GetFileName(configFilePath);
			if (fileName.StartsWith("webpack", StringComparison.OrdinalIgnoreCase))
			{
				var pluginIndex = allText.IndexOf("webpack.DefinePlugin");
				if (pluginIndex > 0)
				{
					allText = allText.Substring(pluginIndex);
					var startIndex = allText.IndexOf("{");
					var endIndex = allText.IndexOf("}");
					allText = allText.Substring(startIndex, endIndex - startIndex + 1);
					allText = allText.Replace("__", "\"").Replace("JSON.stringify", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
				}
				else
				{
					return configDictionary;
				}
			}

			var jsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(allText);
			foreach (var key in jsonDictionary.Keys)
			{
				if (jsonDictionary[key].GetType() == typeof(string))
				{
					configDictionary.Add(key, jsonDictionary[key] as string);
				}
			}
			return configDictionary;
		}

		[OneTimeSetUp]
		public void Setup()
		{
			var referenceServiceWebPath = Path.Combine(rootPath, @"Web\ReferenceDataUpdateService.Web");
			var safeUpdateServicePath = Path.Combine(rootPath, @"Service\SafeDataUpdateService\NewSafeDataUpdateService");
			var stagingServicePath = Path.Combine(rootPath, @"Staging\Service\NewService");
			webpackFiles = Directory.GetFiles(referenceServiceWebPath, "webpack.*.js");
			updateServiceConfigFiles = Directory.GetFiles(safeUpdateServicePath, "*.config.json");
			stagingServiceConfigFiles = Directory.GetFiles(stagingServicePath, "*.config.json");
		}

		string[] webpackFiles;
		string[] updateServiceConfigFiles;
		string[] stagingServiceConfigFiles;
		readonly string rootPath = TestSourcePathHelper.DATTestSupplementaryContentPath;
	}
}
