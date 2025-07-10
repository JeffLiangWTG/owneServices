using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.SEReferenceData.Tests
{
	[TestFixture]
	public abstract class DownloadFileTest<TXmlItem> where TXmlItem : class
	{
		[Test]
		public void DownloadXML()
		{
			Directory.CreateDirectory(OutputPath);
			var archiveUrl = Path.Combine(TestHelper.BaseSourcePath, InputTestFilesPath, $"{InputTestFileName}.xml");
			using (var fileStream = File.OpenRead(archiveUrl))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(EXPORT_FILE_URL).Respond(MEDIA_TYPE, fileStream);
				var xmlItems = new DownloadExportXml(mockHttp.ToHttpClient()).DownloadLatestFullAndExtract<TXmlItem>(EXPORT_FILE_URL, OutputPath);
				Assert.That(xmlItems.Length, Is.EqualTo(ItemsCount));
			}
		}

		[Test]
		public void DownloadInvalidXML()
		{
			Directory.CreateDirectory(OutputPath);
			var exception = Assert.Throws<ObjectTraderExportException>(() => new DownloadExportXml().DownloadLatestFullAndExtract<TXmlItem>("InvalidURL", OutputPath));
			Assert.That(exception.Message, Does.StartWith($"Unable to Load IncrementalObjectTraderExport XML from the following URL: InvalidURL"));
		}

		[OneTimeSetUp]
		public virtual void Setup()
		{
			Assembly = Assembly.GetExecutingAssembly();
			OutputPath = Path.Combine(Path.GetDirectoryName(Assembly.Location), TestFilesPath);
		}

		private const string MEDIA_TYPE = "text/plain";
		private const string EXPORT_FILE_URL = "http://example.com/filename.xml";

		protected Assembly Assembly { get; set; }
		protected string OutputPath { get; set; }
		protected int ItemsCount { get; set; }
		protected string InputTestFileName { get; set; }

		[TearDown]
		public virtual void TearDown()
		{
			if (Directory.Exists(OutputPath))
			{
				Directory.Delete(OutputPath, true);
			}
		}

		protected string XmlFileObjectName { get; set; }
		protected string TestFilesPath { get; set; }
		protected virtual string InputTestFilesPath { get; } = @"UniversalXmlProducers\SE\SEReferenceData.Tests\Nomenclature\TestFiles\Input\";
	}
}
