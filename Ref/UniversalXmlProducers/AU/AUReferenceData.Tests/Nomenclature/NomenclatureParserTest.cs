using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.Nomenclature
{
	[TestFixture]
	[SetCulture("en-AU")]
	internal class NomenclatureParseTest
	{
		[Test, Timeout(10000)]
		public void TestReadGroups()
		{
			ParseNomenclatureForceDownloaded();

			using (var expectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ManifestResourcePathBase + ".RefCusNomenclatureGroup_AU.xml"))
			using (var producedStream = new FileStream(Path.Combine(ApplicationConfig.OutputPath, "AU Customs Nomenclature.xml"), FileMode.Open))
			using (var expectedReader = new StreamReader(expectedStream))
			using (var producedReader = new StreamReader(producedStream))
			{
				Assert.AreEqual(expectedReader.ReadToEnd(), producedReader.ReadToEnd());
			}
		}

		[Test, Timeout(10000)]
		public void TestCreateNewTariffFile()
		{
			var tariffFilePath = ApplicationConfig.AUTariffsOutputPath;
			if (File.Exists(tariffFilePath))
			{
				File.Delete(tariffFilePath);
			}

			ParseNomenclatureForceDownloaded();

			using (var expectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ManifestResourcePathBase + ".Expected_AU_Tariffs.json"))
			using (var producedStream = new FileStream(tariffFilePath, FileMode.Open))
			using (var expectedReader = new StreamReader(expectedStream))
			using (var producedReader = new StreamReader(producedStream))
			{
				Assert.AreEqual(expectedReader.ReadToEnd(), producedReader.ReadToEnd());
			}
		}

		[Test, Timeout(10000)]
		public void TestUpdateExistingTariffFileOverrideAllowed()
		{
			var producedFilePath = ApplicationConfig.AUTariffsOutputPath;
			Directory.CreateDirectory(Path.GetDirectoryName(producedFilePath));
			using (var modifiedExpectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ManifestResourcePathBase + ".AU_Tariffs_Override_Allowed.json"))
			using (var modifiedExpectedReader = new StreamReader(modifiedExpectedStream))
			using (var producedWriteStream = new FileStream(producedFilePath, FileMode.Create, FileAccess.Write))
			using (var writer = new StreamWriter(producedWriteStream))
			{
				writer.Write(modifiedExpectedReader.ReadToEnd());
			}

			ParseNomenclatureForceDownloaded();

			using (var expectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ManifestResourcePathBase + ".Expected_AU_Tariffs.json"))
			using (var producedReadStream = new FileStream(producedFilePath, FileMode.Open))
			using (var expectedReader = new StreamReader(expectedStream))
			using (var producedReader = new StreamReader(producedReadStream))
			{
				Assert.That(expectedReader.ReadToEnd(), Is.EqualTo(producedReader.ReadToEnd()));
			}
		}

		[Test, Timeout(10000)]
		public void TestUpdateExistingTariffFileOverrideNotAllowed()
		{
			List<TariffOutput> overrideNotAllowedTariffs;
			var producedFilePath = ApplicationConfig.AUTariffsOutputPath;
			Directory.CreateDirectory(Path.GetDirectoryName(producedFilePath));
			using (var originalContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ManifestResourcePathBase + ".AU_Tariffs_Override_Not_Allowed.json"))
			using (var originalContentReader = new StreamReader(originalContentStream))
			using (var producedWriteStream = new FileStream(producedFilePath, FileMode.Create, FileAccess.Write))
			using (var writer = new StreamWriter(producedWriteStream))
			{
				var content = originalContentReader.ReadToEnd();
				writer.Write(content);
				overrideNotAllowedTariffs = JsonSerializer.Deserialize<List<TariffOutput>>(content, jsonOptions);
			}

			ParseNomenclatureForceDownloaded();

			using (var updatingContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ManifestResourcePathBase + ".Expected_AU_Tariffs.json"))
			using (var producedReadStream = new FileStream(producedFilePath, FileMode.Open))
			using (var updatingContentReader = new StreamReader(updatingContentStream))
			using (var producedReader = new StreamReader(producedReadStream))
			{
				var updatingTariffs = JsonSerializer.Deserialize<List<TariffOutput>>(updatingContentReader.ReadToEnd(), jsonOptions);
				var producedTariffs = JsonSerializer.Deserialize<List<TariffOutput>>(producedReader.ReadToEnd(), jsonOptions);

				Assert.That(producedTariffs, Has.Count.AtLeast(updatingTariffs.Count));

				foreach (var overrideNotAllowedTariff in overrideNotAllowedTariffs)
				{
					Assert.That(updatingTariffs, Does.Not.Contain(overrideNotAllowedTariff));
					Assert.That(producedTariffs, Contains.Item(overrideNotAllowedTariff));
				}
			}
		}

		[Test, Timeout(10000)]
		public void TestUpdateExistingTariffFile()
		{
			List<TariffOutput> preservedTariffs;
			var producedFilePath = ApplicationConfig.AUTariffsOutputPath;
			Directory.CreateDirectory(Path.GetDirectoryName(producedFilePath));
			using (var originalContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ManifestResourcePathBase + ".AU_Tariffs_Preserved.json"))
			using (var originalContentReader = new StreamReader(originalContentStream))
			using (var producedWriteStream = new FileStream(producedFilePath, FileMode.Create, FileAccess.Write))
			using (var writer = new StreamWriter(producedWriteStream))
			{
				var content = originalContentReader.ReadToEnd();
				writer.Write(content);
				preservedTariffs = JsonSerializer.Deserialize<List<TariffOutput>>(content, jsonOptions);
			}

			ParseNomenclatureForceDownloaded();

			using (var updatingContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ManifestResourcePathBase + ".Expected_AU_Tariffs.json"))
			using (var producedReadStream = new FileStream(producedFilePath, FileMode.Open))
			using (var updatingContentReader = new StreamReader(updatingContentStream))
			using (var producedReader = new StreamReader(producedReadStream))
			{
				var updatingTariffs = JsonSerializer.Deserialize<List<TariffOutput>>(updatingContentReader.ReadToEnd(), jsonOptions);
				var producedTariffs = JsonSerializer.Deserialize<List<TariffOutput>>(producedReader.ReadToEnd(), jsonOptions);

				Assert.That(producedTariffs, Has.Count.AtLeast(updatingTariffs.Count));

				foreach (var preservedTariff in preservedTariffs)
				{
					Assert.That(updatingTariffs, Does.Not.Contain(preservedTariff));
					Assert.That(producedTariffs, Contains.Item(preservedTariff));
				}
			}
		}

		[Test]
		public void TestNomenclatureParserWhenForceDownloadIsTrue()
		{
			var propertyDict = new Dictionary<string, string>
			{
				{ "AUNomenclatureForceDownloaded", "true" }
			};

			ApplicationConfigTests.ChangeConfigTemprory4Test(propertyDict, () =>
			{
				var mockHttpClientHelper = new Mock<IHttpClientHelper>();
				mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.IsAny<string>())).Returns<string>(x =>
				{
					var resourcePath = string.Join(".", new Uri(x).LocalPath.Replace('-', '_').Split('/').Where(_ => !string.IsNullOrEmpty(_)).Prepend(ManifestResourcePathBase).Append("page.html"));
					var today = DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
					var content = LoadAndModifyHtmlResource(resourcePath, today);

					return Task.FromResult(content);
				});
				nomenclatureParser = new TestNomenclatureParser
				{
					HttpClientHelper = mockHttpClientHelper.Object
				};
				
				Assert.DoesNotThrow(() => nomenclatureParser.Parse(new DateTime(2022, 8, 10)));
			});
		}

		[Test]
		public void TestSendEmailMaxRetriesReached()
		{
			var propertyDict = new Dictionary<string, string>
			{
				{ "AUNomenclatureWebsiteMaxAttempts", "6" },
				{ "EmailGroup", "email@hotmail.com" }
			};

			ApplicationConfigTests.ChangeConfigTemprory4Test(propertyDict, () =>
			{
				var processingData = new NomenclatureProcessingData
				{
					AUNomenclatureRetryCount = 5
				};
				var mockDataFilePath = Path.Combine(ApplicationConfig.OutputDirectory, "AUCustomsProcessingData", "NomenclatureProcessingData.json");
				Directory.CreateDirectory(Path.GetDirectoryName(mockDataFilePath));
				File.WriteAllText(mockDataFilePath, System.Text.Json.JsonSerializer.Serialize(processingData));
				try
				{
					var mockHttpClientHelper = new Mock<IHttpClientHelper>();
					nomenclatureParser = new TestNomenclatureParser
					{
						HttpClientHelper = mockHttpClientHelper.Object,
					};
					nomenclatureParser.Parse(new DateTime(2022, 8, 10));
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Exception caught: {ex.Message}");
				}
				Assert.IsTrue(nomenclatureParser.EmailSent, "Email should have been sent when the website is inaccessible.");

				if (File.Exists(mockDataFilePath))
				{
					File.Delete(mockDataFilePath);
				}
			});
		}

		[Test]
		public void TestEmailNotSentWhenWebsiteAccessible()
		{
			var mockHttpClientHelper = new Mock<IHttpClientHelper>();
			mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.IsAny<string>())).Returns<string>(x =>
			{
				var resourcePath = string.Join(".", new Uri(x).LocalPath.Replace('-', '_').Split('/').Where(_ => !string.IsNullOrEmpty(_)).Prepend(ManifestResourcePathBase).Append("page.html"));
				var today = DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
				var content = LoadAndModifyHtmlResource(resourcePath, today);

				return Task.FromResult(content);
			});
			new TestNomenclatureParser() { HttpClientHelper = mockHttpClientHelper.Object}.Parse(new DateTime(2022, 8, 10));

			Assert.IsFalse(nomenclatureParser.EmailSent, "Email should not be sent when the website is accessible.");
		}

		[Test]
		public void TestGetWebsiteLastUpdatedDateTimeWorksAsExpectedRegardlessOfCurrentCulture()
		{
			var originalCulture = CultureInfo.CurrentCulture;
			var originalUICulture = CultureInfo.CurrentUICulture;

			try
			{
				CultureInfo.CurrentCulture = new CultureInfo("en-US");
				CultureInfo.CurrentUICulture = new CultureInfo("en-US");

				ApplicationConfigTests.ChangeConfigTemprory4Test(new Dictionary<string, string>(), () =>
				{
					var mockHttpClientHelper = new Mock<IHttpClientHelper>();
					mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.IsAny<string>())).Returns<string>(x =>
					{
						var content = @"
<!DOCTYPE HTML>
<html>
<head></head>
<footer>
	<span id=""pageModified"" class=""hide"">22/09/2024 20:25</span>
</footer>
</body>
</html>
";
						return Task.FromResult(content);
					});
					var parser = new NomenclatureParser { HttpClientHelper = mockHttpClientHelper.Object };

					Assert.DoesNotThrow(() => parser.GetWebsiteLastUpdatedDateTime("https://www.example.com"));
				});
			}
			finally
			{
				// Restore the original culture
				CultureInfo.CurrentCulture = originalCulture;
				CultureInfo.CurrentUICulture = originalUICulture;
			}
		}

		string LoadAndModifyHtmlResource(string resourcePath, string dateReplacement)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
			{
				if (stream == null)
					throw new InvalidOperationException($"Resource not found: {resourcePath}");
				using (var reader = new StreamReader(stream))
				{
					var content = reader.ReadToEnd();
					return Regex.Replace(content,
						"<span id=\"pageModified\" class=\"hide\">.*?</span>",
						$"<span id=\"pageModified\" class=\"hide\">{dateReplacement}</span>",
						RegexOptions.IgnoreCase | RegexOptions.Singleline);
				}
			}
		}

		void ParseNomenclatureForceDownloaded()
		{
			var mockHttpClientHelper = new Mock<IHttpClientHelper>();
			mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.IsAny<string>())).Returns<string>(x =>
			{
				var resourcePath = string.Join(".", new Uri(x).LocalPath.Replace('-', '_').Split('/').Where(_ => !string.IsNullOrEmpty(_)).Prepend(ManifestResourcePathBase).Append("page.html"));
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
				using (var reader = new StreamReader(stream))
				{
					return Task.FromResult(reader.ReadToEnd());
				}
			});

			var propertyDict = new Dictionary<string, string>
			{
				{ "AUNomenclatureForceDownloaded", "true" }
			};

			ApplicationConfigTests.ChangeConfigTemprory4Test(propertyDict, () =>
			{
				new TestNomenclatureParser() { HttpClientHelper = mockHttpClientHelper.Object }.Parse(new DateTime(2022, 8, 10));
			});
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
				PropertyNameCaseInsensitive = true
			};
		}

		[SetUp]
		public void SetUp()
		{
			nomenclatureParser = new TestNomenclatureParser
			{
				EmailSent = false,
				HttpClientHelper = new HttpClientHelper()
			};
		}

		[TearDown]
		public void TearDown()
		{
			string filePath = Path.Combine(ApplicationConfig.OutputPath, "AU Customs Nomenclature.xml");
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			var tariffFilePath = ApplicationConfig.AUTariffsOutputPath;
			if (File.Exists(tariffFilePath))
			{
				File.Delete(tariffFilePath);
			}
		}

		class TestNomenclatureParser : NomenclatureParser
		{
			public bool EmailSent { get; set; }

			public override DateTime GetWebsiteLastUpdatedDateTime(string baseUri)
			{
				return DateTime.Now;
			}

			public override void SendEmail(string emailGroup, string subject, string body)
			{
				EmailSent = true;
			}
		}

		const string ManifestResourcePathBase = "CargoWise.RefDbRepo.AUReferenceData.Tests.Nomenclature.TestFiles";

		TestNomenclatureParser nomenclatureParser;

		JsonSerializerOptions jsonOptions;
	}
}
