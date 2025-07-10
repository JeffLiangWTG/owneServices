using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class DownloadJsonTests
{
	[Test]
	public void TestDownloadJson()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var vatURL = Path.Combine(Path.GetDirectoryName(assembly.Location), @"Business\JSON\TestFiles\Input\invalid_measures_es.json");
		var json = DownloadJson.Download(vatURL);
		Assert.That(json, Does.Contain("["));
	}

	[Test]
	public void TestDownloadInvalidJson()
	{
		var exception = Assert.Throws<JSONException>(() => DownloadJson.Download("http://InvalidURL"));
		var exceptionMessage = exception.Message;

		var expectedESDescription = "Unable to download Json from the following URL: http://InvalidURL";
		Assert.That(exceptionMessage, Contains.Substring(expectedESDescription));

		var expectedHTMLDescription = "(invalidurl:80)";
		Assert.That(exceptionMessage, Contains.Substring(expectedHTMLDescription));
	}
}
