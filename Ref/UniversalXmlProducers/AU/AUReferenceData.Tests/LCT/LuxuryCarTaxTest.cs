using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.CmdLine;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class LuxuryCarTaxTest
	{
		[Test]
		public void TestParse()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var webPageStream = assembly.GetManifestResourceStream(WebPageTestFilePath))
			using (var reader = new StreamReader(webPageStream))
			{
				var lctWebPage = reader.ReadToEnd();
				var result = LCTParser.Parse(lctWebPage, out var publicationDate).ToArray();

				Assert.AreEqual(28, result.Length);

				var currentLFTEntity = result[0];
				Assert.AreEqual("LFT", currentLFTEntity.ZZF_Code);
				Assert.AreEqual("LCT FEV Threshold", currentLFTEntity.ZZF_Description);
				Assert.AreEqual(91387.0m, currentLFTEntity.ZZF_Value);
				Assert.AreEqual("2025-07-01 00:00:00Z", currentLFTEntity.ZZF_StartDate.ToString("u"));
				Assert.AreEqual("2079-06-06 23:59:00Z", currentLFTEntity.ZZF_EndDate.ToString("u"));

				var currentLNTEntity = result[1];
				Assert.AreEqual("LNT", currentLNTEntity.ZZF_Code);
				Assert.AreEqual("LCT Normal Vehicle Threshold", currentLNTEntity.ZZF_Description);
				Assert.AreEqual(80567.0m, currentLNTEntity.ZZF_Value);
				Assert.AreEqual("2025-07-01 00:00:00Z", currentLNTEntity.ZZF_StartDate.ToString("u"));
				Assert.AreEqual("2079-06-06 23:59:00Z", currentLNTEntity.ZZF_EndDate.ToString("u"));

				var previousLFTEntity = result[2];
				Assert.AreEqual("LFT", previousLFTEntity.ZZF_Code);
				Assert.AreEqual("LCT FEV Threshold", previousLFTEntity.ZZF_Description);
				Assert.AreEqual(91387.0m, previousLFTEntity.ZZF_Value);
				Assert.AreEqual("2024-07-01 00:00:00Z", previousLFTEntity.ZZF_StartDate.ToString("u"));
				Assert.AreEqual("2025-06-30 23:59:59Z", previousLFTEntity.ZZF_EndDate.ToString("u"));

				var previousLNTEntity = result[3];
				Assert.AreEqual("LNT", previousLNTEntity.ZZF_Code);
				Assert.AreEqual("LCT Normal Vehicle Threshold", previousLNTEntity.ZZF_Description);
				Assert.AreEqual(80567.0m, previousLNTEntity.ZZF_Value);
				Assert.AreEqual("2024-07-01 00:00:00Z", previousLNTEntity.ZZF_StartDate.ToString("u"));
				Assert.AreEqual("2025-06-30 23:59:59Z", previousLNTEntity.ZZF_EndDate.ToString("u"));

				var earliestLFTEntity = result[26];
				Assert.AreEqual("LFT", earliestLFTEntity.ZZF_Code);
				Assert.AreEqual("LCT FEV Threshold", earliestLFTEntity.ZZF_Description);
				Assert.AreEqual(75375m, earliestLFTEntity.ZZF_Value);
				Assert.AreEqual("2012-07-01 00:00:00Z", earliestLFTEntity.ZZF_StartDate.ToString("u"));
				Assert.AreEqual("2013-06-30 23:59:59Z", earliestLFTEntity.ZZF_EndDate.ToString("u"));

				var earliestLNTEntity = result[27];
				Assert.AreEqual("LNT", earliestLNTEntity.ZZF_Code);
				Assert.AreEqual("LCT Normal Vehicle Threshold", earliestLNTEntity.ZZF_Description);
				Assert.AreEqual(59133m, earliestLNTEntity.ZZF_Value);
				Assert.AreEqual("2012-07-01 00:00:00Z", earliestLNTEntity.ZZF_StartDate.ToString("u"));
				Assert.AreEqual("2013-06-30 23:59:59Z", earliestLNTEntity.ZZF_EndDate.ToString("u"));

				Assert.AreEqual("25 May 2025", publicationDate);
			}
		}

		[Test]
		public void TestParseDecimalValue()
		{
			var result = LCTParser.Parse(SimplifiedWebContentForTesting, out var publicationDate).ToArray();

			Assert.AreEqual(6, result.Length);

			var currentLFTEntity = result[0];
			Assert.AreEqual("LFT", currentLFTEntity.ZZF_Code);
			Assert.AreEqual("LCT FEV Threshold", currentLFTEntity.ZZF_Description);
			Assert.AreEqual(84916.0m, currentLFTEntity.ZZF_Value);
			Assert.AreEqual("2022-07-01 00:00:00Z", currentLFTEntity.ZZF_StartDate.ToString("u"));
			Assert.AreEqual("2079-06-06 23:59:00Z", currentLFTEntity.ZZF_EndDate.ToString("u"));

			var currentLNTEntity = result[1];
			Assert.AreEqual("LNT", currentLNTEntity.ZZF_Code);
			Assert.AreEqual("LCT Normal Vehicle Threshold", currentLNTEntity.ZZF_Description);
			Assert.AreEqual(71849.0m, currentLNTEntity.ZZF_Value);
			Assert.AreEqual("2022-07-01 00:00:00Z", currentLNTEntity.ZZF_StartDate.ToString("u"));
			Assert.AreEqual("2079-06-06 23:59:00Z", currentLNTEntity.ZZF_EndDate.ToString("u"));

			var previousLFTEntity = result[2];
			Assert.AreEqual("LFT", previousLFTEntity.ZZF_Code);
			Assert.AreEqual("LCT FEV Threshold", previousLFTEntity.ZZF_Description);
			Assert.AreEqual(79659m, previousLFTEntity.ZZF_Value);
			Assert.AreEqual("2021-07-01 00:00:00Z", previousLFTEntity.ZZF_StartDate.ToString("u"));
			Assert.AreEqual("2022-06-30 23:59:59Z", previousLFTEntity.ZZF_EndDate.ToString("u"));

			Assert.AreEqual("31 March 2025", publicationDate);
		}

		[Test]
		public void TestLuxuryCarTaxProgram_Run()
		{
			var outputFolder = Path.GetTempPath();
			var outputFile = Path.Combine(outputFolder, "AU Customs LCT Thresholds.xml");

			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				var mockHttpClientHelper = new Mock<IHttpClientHelper>();
				mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.IsAny<string>())).Returns<string>(x =>
				{
					using (var webPageStream = assembly.GetManifestResourceStream(WebPageTestFilePath))
					using (var reader = new StreamReader(webPageStream))
					{
						return Task.FromResult(reader.ReadToEnd());
					}
				});

				var program = new LuxuryCarTaxProgramForTest(mockHttpClientHelper.Object);
				program.Run(outputFolder);

				var xmlContent = File.ReadAllText(outputFile);

				using (var testFileStream = assembly.GetManifestResourceStream(ExpectedOutputTestFilePath))
				using (var reader = new StreamReader(testFileStream))
				{
					var expectedOutput = reader.ReadToEnd();
					Assert.AreEqual(expectedOutput, xmlContent);
				}
			}
			finally
			{
				File.Delete(outputFile);
			}
		}

		class LuxuryCarTaxProgramForTest : LuxuryCarTaxProgram
		{
			public LuxuryCarTaxProgramForTest(IHttpClientHelper httpClient) : base()
			{
				this.httpClient = httpClient;
			}
			IHttpClientHelper httpClient;

			protected override IHttpClientHelper HttpClient => httpClient;
		}

		const string WebPageTestFilePath = "CargoWise.RefDbRepo.AUReferenceData.Tests.LCT.TestFiles.LCT_Thresholds_WebPage_05_2025.html";
		const string ExpectedOutputTestFilePath = "CargoWise.RefDbRepo.AUReferenceData.Tests.LCT.TestFiles.LCT_Output_2025.xml";

		const string SimplifiedWebContentForTesting = @"<!DOCTYPE html>
<html lang=""en"">
<head><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""></head>
<body>
<div>
<p class=""AtoDefaultPageHeader_bottom__date__L4xB4"" data-testid=""date""><strong>Last updated </strong>31 March 2025</p>
<table>
<caption>LCT thresholds</caption>
<tr>
<th><p>Financial year</p>
</th>
<th><p>Fuel-efficient vehicles</p>
</th>
<th><p>Other vehicles</p>
</th>
</tr>
<tr>
<th><p>2022&#x2013;23</p>
</th>
<td><p>$84,916.00</p>
</td>
<td><p>$71,849.00</p>
</td>
</tr>
<tr>
<th><p>2021&#x2013;2022</p>
</th>
<td><p>$79,659</p>
</td>
<td><p>$69,152</p>
</td>
</tr>
<tr>
<th><p>2020&#x2013;21</p>
</th>
<td><p>$77,565</p>
</td>
<td><p>$68,740</p>
</td>
</tr>
</table>
<p>The indexation factor for the 2022–23 financial year for:</p>
<ul>
<li>fuel-efficient vehicles is 1.066</li>
<li>other vehicles is 1.039.</li>
</ul>
<p>Find out what <a href=""https://www.ato.gov.au/Business/Luxury-car-tax/In-detail/Definitions---Luxury-car-tax/"">defines a fuel-efficient car</a>.</p>
<span>Luxury car tax (LCT) rate and thresholds.</span>
<span>QC 38161</span>
</div>
</body></html>";
	}
}
