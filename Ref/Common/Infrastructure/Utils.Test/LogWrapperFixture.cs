using System;
using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	public class LogWrapperFixture
	{
		[Test]
		public void GetLog_WriteToDifferentFiles()
		{
			var tempPath = Path.GetTempPath();
			Environment.SetEnvironmentVariable("TempPath", tempPath);

			var quartzJobName1 = "Xml Test 1";
			var logPath1 = Path.Combine(tempPath, $"{quartzJobName1}-{DateTime.Now:yyyyMMdd}.txt");
			if (File.Exists(logPath1))
			{
				File.Delete(logPath1);
			}
			using (var logWrapper = new LogWrapper(quartzJobName1))
			{
				var log1 = logWrapper.GetLog(quartzJobName1);
				log1.Info($"write to separate log file. Quartz Job Name: {quartzJobName1}");
				Assert.True(File.Exists(logPath1));
			}

			var quartzJobName2 = "Xml Test 2";
			var logPath2 = Path.Combine(tempPath, $"{quartzJobName2}-{DateTime.Now:yyyyMMdd}.txt");
			if (File.Exists(logPath2))
			{
				File.Delete(logPath2);
			}
			using (var logWrapper = new LogWrapper(quartzJobName2))
			{
				var log2 = logWrapper.GetLog(quartzJobName2);
				log2.Info($"write to separate log file. Quartz Job Name: {quartzJobName2}");
				Assert.True(File.Exists(logPath2));
			}

			File.Delete(logPath1);
			File.Delete(logPath2);
		}

		[SetUp]
		public void SetUp()
		{
			SerilogConfigProvider.SetConfigFileForTest("CargoWise.RefDbRepo.Common.Utils.Test.config.json");
		}
	}
}
