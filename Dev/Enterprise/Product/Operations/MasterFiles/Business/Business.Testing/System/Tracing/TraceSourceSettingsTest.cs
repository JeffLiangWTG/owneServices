using System.Diagnostics;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TraceSourceSettings))]
	public class TraceSourceSettingsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSourceLevel()
		{
			var settings = new TraceSourceSettings();

			settings.TraceLevel = TraceSourceLevels.Codes.All;
			AssertEquals(SourceLevels.All, settings.SourceLevel);

			settings.TraceLevel = TraceSourceLevels.Codes.ActivityTracing;
			AssertEquals(SourceLevels.ActivityTracing, settings.SourceLevel);

			settings.TraceLevel = TraceSourceLevels.Codes.Critical;
			AssertEquals(SourceLevels.Critical, settings.SourceLevel);

			settings.TraceLevel = TraceSourceLevels.Codes.Error;
			AssertEquals(SourceLevels.Error, settings.SourceLevel);

			settings.TraceLevel = TraceSourceLevels.Codes.Information;
			AssertEquals(SourceLevels.Information, settings.SourceLevel);

			settings.TraceLevel = TraceSourceLevels.Codes.Off;
			AssertEquals(SourceLevels.Off, settings.SourceLevel);

			settings.TraceLevel = TraceSourceLevels.Codes.Verbose;
			AssertEquals(SourceLevels.Verbose, settings.SourceLevel);

			settings.TraceLevel = TraceSourceLevels.Codes.Warning;
			AssertEquals(SourceLevels.Warning, settings.SourceLevel);
		}

		public void TestProperties()
		{
			var settings = new TraceSourceSettings();
			settings.TraceSourceName = "SourceName";
			settings.TraceSourceDescription = "Source Description";
			settings.TraceLevel = TraceSourceLevels.Codes.Error;
			settings.TraceFilter = "Trace Filter";

			AssertEquals(nameof(settings.TraceSourceName), "SourceName", settings.TraceSourceName);
			AssertEquals(nameof(settings.TraceSourceDescription), "Source Description", settings.TraceSourceDescription);
			AssertEquals(nameof(settings.TraceLevel), TraceSourceLevels.Codes.Error, settings.TraceLevel);
			AssertEquals(nameof(settings.SourceLevel), SourceLevels.Error, settings.SourceLevel);
			AssertEquals(nameof(settings.TraceFilter), "Trace Filter", settings.TraceFilter);

			AssertEquals(nameof(settings.TraceSourceName), true, settings.TraceSourceNameInfo.ReadOnly);
			AssertEquals(nameof(settings.TraceSourceDescription), true, settings.TraceSourceDescriptionInfo.ReadOnly);
		}
	}
}
