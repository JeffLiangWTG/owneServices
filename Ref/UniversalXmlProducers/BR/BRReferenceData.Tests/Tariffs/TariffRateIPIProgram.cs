using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class TariffRateIPIProgramTest
	{
		[Test]
		public void TestHasDecreeChanged()
		{
			using (var program = new BRTariffRateIPIProgramForTesting())
			{
				using (var stream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TIPI.TIPI_HOME.txt"))
				using (var updatedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TIPI.TIPI_HOME_NEW_DECREE.txt"))
				{
					using (var sw = new StringWriter())
					{
						Console.SetOut(sw);
						var mock = new MockHttpMessageHandler();

						mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TIPI_HOME"]).Respond("application/html", stream);
						program.mock = mock;
						program.DeleteLogFile();

						Assert.DoesNotThrow(() => { program.Run(); });
						Assert.AreEqual("File IPI Home, exported with success!\r\n", sw.ToString());
					}
					using (var sw = new StringWriter())
					{
						Console.SetOut(sw);

						var mock = new MockHttpMessageHandler();
						program.mock = mock;
						mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TIPI_HOME"]).Respond("application/html", updatedStream);

						Assert.DoesNotThrow(() => { program.Run(); });
						Assert.AreEqual("File IPI Home, exported with success!\r\n", sw.ToString());
					}
				}
			}
		}

		[Test]
		public void TestNoDecreeChange()
		{
			using (var program = new BRTariffRateIPIProgramForTesting())
			{
				using (var stream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TIPI.TIPI_HOME.txt"))
				using (var sameStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TIPI.TIPI_HOME.txt"))
				{
					using (var sw = new StringWriter())
					{
						Console.SetOut(sw);
						var mock = new MockHttpMessageHandler();

						mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TIPI_HOME"]).Respond("application/html", stream);
						program.mock = mock;
						program.DeleteLogFile();

						Assert.DoesNotThrow(() => { program.Run(); });
						Assert.AreEqual("File IPI Home, exported with success!\r\n", sw.ToString());
					}
					using (var sw = new StringWriter())
					{
						Console.SetOut(sw);

						var mock = new MockHttpMessageHandler();
						program.mock = mock;
						mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TIPI_HOME"]).Respond("application/html", sameStream);

						Assert.DoesNotThrow(() => { program.Run(); });
						Assert.AreEqual("No update found on IPI Home!\r\n", sw.ToString());
					}
				}
			}
		}

		class BRTariffRateIPIProgramForTesting : TariffRateIPIProgram, IDisposable
		{
			public MockHttpMessageHandler mock { get; set; }

			protected override HttpClient GetHttpClient(HttpClientHandler handler = null) => mock?.ToHttpClient();

			protected override void ExportToXMLFile(byte[] bFile)
			{
			}

			public void Dispose()
			{
				DeleteLogFile();
				DeleteOutputFiles();
			}
		}
	}
}
