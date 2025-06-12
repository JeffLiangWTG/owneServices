using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.WindowsService.Tests
{
	[TestFixture]
	public class AuthenticationSettingsTests
	{
		[Test]
		public void TestChangeConfigFile()
		{
			var mock = new Mock<IFileSystemWatcherWrapper>();
			var settings = new DummySettings();
			settings.CreateTempFile();
			var config = settings.GetConfiguration();

			mock.Setup(x => x.EnableRaisingEvents).Returns(true);
			settings.LoadApplicationSettings(mock.Object);

			Assert.AreEqual(300, settings.INTERVAL);
			Assert.AreEqual("OldConnectionString", settings.AUTH_DB_CONNECTION_STRING);
			Assert.AreEqual("OldConnectionString2", settings.EDIPROD_DB_CONNECTION_STRING);

			try
			{
				config.AppSettings.Settings["IntervalSeconds"].Value = "100";
				config.Save(ConfigurationSaveMode.Modified);
				mock.Raise(x => x.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetDirectoryName(config.FilePath), Path.GetFileName(config.FilePath)));

				Assert.AreEqual(100, settings.INTERVAL);
			}
			finally
			{
				settings.DeleteTempFile(AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString());
			}
		}
	}

	class DummySettings : AuthenticationSettings
	{
		readonly string testConfigFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"TestFiles\TestConfigFile1.config");

		public override Configuration GetConfiguration()
		{
			if (config == null)
			{
				if (string.IsNullOrEmpty(configFilePath))
				{
					configFilePath = testConfigFile;
				}

				AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFilePath);
				ResetConfigMechanism();
				config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				config.AppSettings.SectionInformation.ForceSave = true;
			}
			return config;
		}

		public void CreateTempFile()
		{
			var tempConfigFilePath = Path.GetTempPath() + Guid.NewGuid() + ".config";
			File.Copy(testConfigFile, tempConfigFilePath);
			FileInfo fileInfo = new FileInfo(tempConfigFilePath);
			fileInfo.IsReadOnly = false;
			configFilePath = tempConfigFilePath;
		}

		public void DeleteTempFile(string path)
		{
			File.Delete(path);
		}

		private static void ResetConfigMechanism()
		{
			typeof(ConfigurationManager)
				.GetField("s_initState", BindingFlags.NonPublic |
										 BindingFlags.Static)
				.SetValue(null, 0);

			typeof(ConfigurationManager)
				.GetField("s_configSystem", BindingFlags.NonPublic |
											BindingFlags.Static)
				.SetValue(null, null);

			typeof(ConfigurationManager)
				.Assembly.GetTypes()
				.Where(x => x.FullName ==
							"System.Configuration.ClientConfigPaths")
				.First()
				.GetField("s_current", BindingFlags.NonPublic |
									   BindingFlags.Static)
				.SetValue(null, null);
		}

		string configFilePath;
		Configuration config;
	}
}
