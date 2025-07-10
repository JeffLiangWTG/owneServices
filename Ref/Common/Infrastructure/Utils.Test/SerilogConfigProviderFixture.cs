using System;
using System.Collections.Immutable;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class SerilogConfigProviderFixture
	{
		[Test]
		public void TestGetSerilogConfig()
		{
			SerilogConfigProvider.SetConfigFileForTest("CargoWise.RefDbRepo.Common.Utils.Test.config.json");
			var serilogSettings = SerilogConfigProvider.GetSerilogSettings("test_serilog_file");
			Assert.That(serilogSettings, Has.Count.EqualTo(4), "there are 4 serilog config settings");
			var serilogSettingsDictionary = serilogSettings.ToImmutableDictionary();
			Assert.That("true".Equals(serilogSettingsDictionary["write-to:RollingFile.shared"], StringComparison.OrdinalIgnoreCase));
			Assert.That("Serilog.Sinks.RollingFile".Equals(serilogSettingsDictionary["using:RollingFile"], StringComparison.OrdinalIgnoreCase));
			Assert.That("{Timestamp:HH:mm:ss.fff} [{NamedContext}] [{Level}] {Message}{NewLine}{Exception}".Equals(serilogSettingsDictionary["write-to:RollingFile.outputTemplate"], StringComparison.OrdinalIgnoreCase));
			Assert.That(serilogSettingsDictionary["write-to:RollingFile.pathFormat"].EndsWith("\\test_serilog_file-{Date}.txt", StringComparison.OrdinalIgnoreCase));
		}
	}
}
