using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.MXReferenceData.CmdLine;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.MXReferenceData.Tests
{
	[TestFixture]
	class CustomsFacilitiesProgramTest
	{
		[Test]
		public void TestFilesHasBeenUpdated()
		{
			using (var program = new CustomsFacilitiesProgramForTesting())
			{
				RunMock(program, MockHttp(), "File MX - Customs Facilities, exported with success!");

				var generatedFiles = Directory.GetFiles(ConfigurationProvider.OutputFolder);

				Assert.That(generatedFiles, Has.Length.EqualTo(2));
				Assert.True(generatedFiles[0].EndsWith("RefCusCodeList_MX_FAC.xml"), "file 0 shoulb RefCusCodeList_MX_FAC.xml");
				Assert.True(generatedFiles[1].EndsWith("RefCusCodeType_MX_FAC.xml"), "file 1 shoulb RefCusCodeType_MX_FAC.xml");
			}
		}

		[Test]
		public void TestNoFileGeneratedWhenNoChanges()
		{
			using (var program = new CustomsFacilitiesProgramForTesting())
			{
				RunMock(program, MockHttp(), "File MX - Customs Facilities, exported with success!");

				var generatedFiles = Directory.GetFiles(ConfigurationProvider.OutputFolder);
				Assert.That(generatedFiles, Has.Length.EqualTo(2));

				RunMock(program, MockHttp(), "No update found on MX - Customs Facilities!");

				Assert.That(generatedFiles, Has.Length.EqualTo(2));
				Assert.True(generatedFiles[0].EndsWith("RefCusCodeList_MX_FAC.xml"), "file 0 shoulb RefCusCodeList_MX_FAC.xml");
				Assert.True(generatedFiles[1].EndsWith("RefCusCodeType_MX_FAC.xml"), "file 1 shoulb RefCusCodeType_MX_FAC.xml");
			}
		}

		void RunMock(CustomsFacilitiesProgramForTesting program, MockHttpMessageHandler mock, string message, bool messageShouldBeEquals = true)
		{
			using (StringWriter sw = new StringWriter())
			{
				Console.SetOut(sw);
				program.mock = mock;

				Assert.DoesNotThrow(() => { program.Run(); });
				if (messageShouldBeEquals)
				{
					Assert.That(sw.ToString(), Does.Contain(message));
				}
				else
				{
					Assert.IsFalse(sw.ToString().Contains(message));
				}
			}
		}

		protected MockHttpMessageHandler MockHttp()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["CUSTOMS_SECTION_URL"]).Respond("application/html", TestUtils.GetManifestResourceStream($"CargoWise.RefDbRepo.MXReferenceData.Tests.CodeList.TestFiles.Input.Anexo22.html"));
				return mockHttp;
			}
		}

		[TearDown]
		protected void DeleteFiles()
		{
			new CustomsFacilitiesProgramForTesting().Dispose();
		}

		class CustomsFacilitiesProgramForTesting : CustomsFacilitiesProgram, IDisposable
		{
			public MockHttpMessageHandler mock { get; set; }

			protected override HttpClient GetHttpClient(HttpClientHandler handler = null) => mock?.ToHttpClient();

			public void Dispose()
			{
				DeleteLogFile();
				DeleteOutputFiles();
			}
		}
	}
}
