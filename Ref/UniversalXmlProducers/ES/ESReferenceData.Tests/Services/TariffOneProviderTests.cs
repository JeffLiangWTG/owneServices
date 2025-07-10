using System.IO;
using System.Text;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests.Services;

public class TariffOneProviderTests : TestCase
{
	[Test]
	public void TestGetFileContent()
	{
		var expectedText = "nomeclatures example";
		var filesPath = Path.Combine(FileHelper.OutputFolder, "TariffOne");
		Directory.CreateDirectory(filesPath);

		var filePath = Path.Combine(filesPath, "nomenclatures.jsonl");
		using (var fs = File.Create(filePath))
		{
			var title = new UTF8Encoding(true).GetBytes(expectedText);
			fs.Write(title, 0, title.Length);
		}

		var tariffOneProvider = new TariffOneProvider(FileHelper.OutputFolder, "");

		Assert.That(tariffOneProvider.GetFileContent(TariffOneProvider.TariffOneFiles.nomenclatures), Is.EqualTo(expectedText));
	}

	[Test]
	public void TestGetLatestDataFileName()
	{
		var tariffOneProvider = new TariffOneProvider(FileHelper.OutputFolder, "http://InvalidURL");


		var exception = Assert.Throws<JSONException>(() => tariffOneProvider.GetLatestDataFileName());
		var exceptionMessage = exception.Message;

		var expectedESDescription = "Unable to download Json from the following URL: http://InvalidURL";
		Assert.That(exceptionMessage, Contains.Substring(expectedESDescription));
	}

	[Test]
	public void TestCreateOrUpdateDataFilesIfNeeded()
	{
		var jsonContent = @"[
{ ""name"":""tariffone_snapshot_2024-10-14.zip"" }
]";

		var multipleJsonContent = @"[
{ ""name"":""tariffone_snapshot_2024-11-14.zip"" },
{ ""name"":""tariffone_snapshot_2024-11-14.zip"" }
]";
		var newJsonContent = @"[
{ ""name"":""tariffone_snapshot_2025-10-14.zip"" }
]";
		var filesPath = Path.Combine(FileHelper.OutputFolder, "TariffOne");
		Directory.CreateDirectory(filesPath);

		var compressedFile = Path.Combine(filesPath, "tariffone_snapshot_2024-10-14.zip");
		File.Create(compressedFile).Dispose();

		var tariffOneProvider = new TariffOneProvider(FileHelper.OutputFolder, "http://InvalidURL");

		Assert.That(tariffOneProvider.CreateOrUpdateDataFilesIfNeeded(jsonContent), Is.EqualTo(string.Empty));

		var expectedError = "DataExport load failed. Expected not null and only 1 record. Json Details:";
		Assert.That(tariffOneProvider.CreateOrUpdateDataFilesIfNeeded(multipleJsonContent), Contains.Substring(expectedError));

		var exception = Assert.Throws<JSONException>(() => tariffOneProvider.CreateOrUpdateDataFilesIfNeeded(newJsonContent));
		var exceptionMessage = exception.Message;
		var expectedZipError = "Exception downloading zip from the following URL: http://InvalidURL/tariffone_snapshot_2025-10-14.zip";
		Assert.That(exceptionMessage, Contains.Substring(expectedZipError));
	}

	protected override string TestClassName => nameof(TariffOneProviderTests);
}
