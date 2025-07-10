using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using static CargoWise.Blazor.Common.BuildFileSystem;

namespace CargoWise.Winzor.AppServer.Test;

public class ProjectSettingsTest
{
	[Test]
	[Property("DAT:CapabilityRequirements", "SOURCE_CODE")]
	public void TestReadyToRunSettingsOnAppServer()
	{
		var projectFileName = Path.Combine(BaseSourcePath, AppServerProject.ProjectFilePath);
		var csProjDocument = XDocument.Load(projectFileName);
		var r2rSettings = csProjDocument.Descendants("PublishReadyToRun").First();
		Assert.That(r2rSettings, Is.Not.Null);
		Assert.That(r2rSettings.Value, Is.EqualTo("false")); //Temperary disabled
	}

	static string BaseSourcePath
	{
		get
		{
			var baseSourcePath = Environment.GetEnvironmentVariable("DAT_TestSourcePath");
			if (string.IsNullOrEmpty(baseSourcePath))
			{
				baseSourcePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(ProjectSettingsTest).Assembly.Location)));
			}
			return baseSourcePath ?? string.Empty;
		}
	}
}
