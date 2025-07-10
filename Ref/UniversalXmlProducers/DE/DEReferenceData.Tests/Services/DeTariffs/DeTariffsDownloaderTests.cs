using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.DEReferenceData.Testing;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.DeTariffs.Testing
{
	sealed class DeTariffsDownloaderTests
	{
		[Test]
		public async Task GetLatestBaselineDownloadSource()
		{
			const string testDirectory = $"./{nameof(GetLatestBaselineDownloadSource)}";

			using var mockHttp = new MockHttpMessageHandler();

			mockHttp.When("https://shop.ezt-online.de/gdb/")
				.Respond("text/html",
					TestHelper.ReadManifestResourceContent(
						"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.Index_gdb.html"));


			using var client = mockHttp.ToHttpClient();
			var downloader = new DeTariffsDownloader(client, new Uri("https://shop.ezt-online.de/gdb/"), new Uri("https://shop.ezt-online.de/"), testDirectory, testDirectory);

			var baselineDownloadTarget = await downloader.GetBaselineDownloadSource();

			Assert.That(baselineDownloadTarget.FileName, Is.EqualTo("XD01296401_GDB.zip"));
		}

		[Test]
		public async Task GetUpdateDownloadSources()
		{
			const string testDirectory = $"./{nameof(GetUpdateDownloadSources)}";
			using var mockHttp = new MockHttpMessageHandler();

			mockHttp.When("https://shop.ezt-online.de/")
				.Respond("text/html",
					TestHelper.ReadManifestResourceContent(
						"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.Index.html"));

			using var client = mockHttp.ToHttpClient();
			var downloader = new DeTariffsDownloader(client, new Uri("https://shop.ezt-online.de/gdb/"), new Uri("https://shop.ezt-online.de/"), testDirectory, testDirectory);

			var baselineDownloadTarget = await downloader.GetUpdateDownloadSources(
				DownloadSource.CreateOrNull(new Uri("https://shop.ezt-online.de/gdb/"), "XD01296401_GDB.zip"));

			Assert.That(baselineDownloadTarget.Select(e => e.FileName),
				Is.EquivalentTo(new[] { "XD01296501.zip", "XD01296602.zip" }));
		}

		[Test]
		public async Task DownloadTariffFileToDiskIfNotExists()
		{
			const string testDirectory = $"./{nameof(DownloadTariffFileToDiskIfNotExists)}";
			try
			{
				using var mockHttp = new MockHttpMessageHandler();

				mockHttp.When("https://shop.ezt-online.de/XD01296601.zip")
					.Respond("application/zip",
						Assembly.GetExecutingAssembly().GetManifestResourceStream(
							"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.XD01296601.zip"));

				using var client = mockHttp.ToHttpClient();
				var downloader = new DeTariffsDownloader(client, new Uri("https://shop.ezt-online.de/gdb/"), new Uri("https://shop.ezt-online.de/"), testDirectory, testDirectory);

				await downloader.DownloadTariffFileToDiskIfNotExists(
					DownloadSource.CreateOrNull(new Uri("https://shop.ezt-online.de/"), "XD01296601.zip"));

				var expectedFile = Path.Combine(testDirectory, "XD01296601.zip");


				Assert.That(expectedFile, Does.Exist);
				var creationTime = DateTime.Today.AddDays(-5);
				File.SetCreationTime(expectedFile, creationTime);

				await downloader.DownloadTariffFileToDiskIfNotExists(
					DownloadSource.CreateOrNull(new Uri("https://shop.ezt-online.de/gdb/"), "XD01296601.zip"));

				Assert.That(File.GetCreationTime(expectedFile), Is.EqualTo(creationTime));
			}
			finally
			{
				Directory.Delete(testDirectory, true);
			}
		}

		[Test]
		public async Task ExtractAllFilesInDownloadDirectory()
		{
			const string testDirectory = $"./{nameof(ExtractAllFilesInDownloadDirectory)}";
			try
			{
				var archiveDirectory = Path.Combine(testDirectory, "archives");
				var extractedDirectory = Path.Combine(testDirectory, "extracted");
				Directory.CreateDirectory(archiveDirectory);
				await using (var fileStream = File.Create(Path.Combine(archiveDirectory, "XD01296601.zip")))
				{
					await using var manifestResourceStream = Assembly.GetExecutingAssembly()
						.GetManifestResourceStream(
							"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.XD01296601.zip");
					await manifestResourceStream.CopyToAsync(fileStream);
				}

				using var mockHttp = new MockHttpMessageHandler();
				using var client = mockHttp.ToHttpClient();

				var downloader = new DeTariffsDownloader(client, new Uri("https://shop.ezt-online.de/gdb/"), new Uri("https://shop.ezt-online.de/"), archiveDirectory, extractedDirectory);

				await downloader.ExtractAllFilesInDownloadDirectory();


				var expectedFile = Path.Combine(extractedDirectory, "XD01296601.xml");
				Assert.That(expectedFile, Does.Exist);

				Assert.That((await File.ReadAllTextAsync(expectedFile)).ReplaceLineEndings("\n"),
					Is.EqualTo(TestHelper.ReadManifestResourceContent(
						"CargoWise.RefDbRepo.DEReferenceData.Tests.Business.DeTariffs.TestFiles.Input.XD01296601.xml").ReplaceLineEndings("\n")));
			}
			finally
			{
				Directory.Delete(testDirectory, true);
			}
		}

		[Test]
		public async Task DownloadTariffs()
		{
			const string testDirectory = $"./{nameof(DownloadTariffs)}";
			try
			{
				var archiveDirectory = Path.Combine(testDirectory, "archives");
				var extractedDirectory = Path.Combine(testDirectory, "extracted");
				Directory.CreateDirectory(archiveDirectory);
				Directory.CreateDirectory(extractedDirectory);

				var obsoleteFile = Path.Combine(archiveDirectory, "XD01296601.zip");
				await using (var fileStream = File.Create(obsoleteFile))
				{
					await using var manifestResourceStream = Assembly.GetExecutingAssembly()
						.GetManifestResourceStream(
							"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.XD01296601.zip");
					await manifestResourceStream.CopyToAsync(fileStream);
				}

				var obsoleteExtractedFile = Path.Combine(extractedDirectory, "XD01296601.xml");
				await File.WriteAllTextAsync(obsoleteExtractedFile, "<xml></xml>");

				using var mockHttp = new MockHttpMessageHandler();

				mockHttp.When("https://shop.ezt-online.de/gdb/XD01296401_GDB.zip")
					.Respond("application/zip",
						Assembly.GetExecutingAssembly().GetManifestResourceStream(
							"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.XD01296401_GDB.zip"));

				mockHttp.When("https://shop.ezt-online.de/XD01296501.zip")
					.Respond("application/zip",
						Assembly.GetExecutingAssembly().GetManifestResourceStream(
							"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.XD01296501.zip"));

				mockHttp.When("https://shop.ezt-online.de/XD01296602.zip")
					.Respond("application/zip",
						Assembly.GetExecutingAssembly().GetManifestResourceStream(
							"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.XD01296602.zip"));

				mockHttp.When("https://shop.ezt-online.de/gdb/")
					.Respond("text/html",
						TestHelper.ReadManifestResourceContent(
							"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.Index_gdb.html"));

				mockHttp.When("https://shop.ezt-online.de/")
					.Respond("text/html",
						TestHelper.ReadManifestResourceContent(
							"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.Index.html"));

				using var client = mockHttp.ToHttpClient();
				var downloader = new DeTariffsDownloader(client, new Uri("https://shop.ezt-online.de/gdb/"),
					new Uri("https://shop.ezt-online.de/"), archiveDirectory, extractedDirectory);

				await downloader.DownloadTariffs();

				var expectedFiles = new[]
				{
					Path.Combine(archiveDirectory, "XD01296401_GDB.zip"),
					Path.Combine(archiveDirectory, "XD01296501.zip"),
					Path.Combine(archiveDirectory, "XD01296602.zip"),
				};

				foreach (var expectedFile in expectedFiles)
				{
					Assert.That(expectedFile, Does.Exist);
				}

				var expectedExtractedFiles = new[]
				{
					Path.Combine(extractedDirectory, "XD01296401_N42000_134.xml"),
					Path.Combine(extractedDirectory, "XD01296401_N42020_137.xml"),
					Path.Combine(extractedDirectory, "XD01296401_T40010_63.xml"),
					Path.Combine(extractedDirectory, "XD01296501.xml"),
					Path.Combine(extractedDirectory, "XD01296602.xml"),
				};

				foreach (var expectedExtractedFile in expectedExtractedFiles)
				{
					Assert.That(expectedExtractedFile, Does.Exist);
				}

				var obsoleteFiles = new[] { obsoleteFile, obsoleteExtractedFile };

				foreach (var obsolete in obsoleteFiles)
				{
					Assert.That(obsolete, Does.Not.Exist);
				}
			}
			finally
			{
				Directory.Delete(testDirectory, true);
			}
		}
	}
}
