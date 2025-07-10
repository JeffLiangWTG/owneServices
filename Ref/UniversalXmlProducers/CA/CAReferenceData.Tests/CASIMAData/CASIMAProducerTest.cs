using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CAReferenceData.Business.CASIMAData;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.CASIMAData
{
	[TestFixture]
	internal class CASIMAProducerTest : TestWithApplicationTestConfig
	{
		#region expectedFile
		string expectedFile = @"<UniversalReferenceData>
  <DataSource>CA SIMA</DataSource>
  <PublicationTime>{0}</PublicationTime>
  <UpdateType>Full</UpdateType>
  <Schema>
    <EntityType Name=""RefCusTariff"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZ1_TariffCode"" />
        <PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusRate"" Type=""RefCusRate"" />
      <Property Name=""RefCusTariffRelationship"" Type=""RefCusTariffRelationship"" />
      <Property Name=""ZZ1_Description"" Type=""nvarchar"" />
      <Property Name=""ZZ1_EndDate"" Type=""datetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZ1_StartDate"" Type=""datetime"" />
      <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CA"" />
      <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CA"" />
    </EntityType>
    <EntityType Name=""RefCusTariffRelationship"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZH_TariffCode"" />
        <PropertyRef Name=""ZZH_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZH_ZZI_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZH_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZH_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""HSN"" />
      <Property Name=""ZZH_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CA"" />
    </EntityType>
    <EntityType Name=""RefCusRate"" Data=""true"">
      <Key>
        <PropertyRef Name=""RefCusApplicability"" />
        <PropertyRef Name=""ZZ2_RX_NKCurrencyOverride"" />
        <PropertyRef Name=""ZZ2_StartDate"" />
        <PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
        <PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
        <PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
      <Property Name=""ZZ2_EndDate"" Type=""datetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZ2_RateFormula"" Type=""varchar"" MaxLength=""500"" />
      <Property Name=""ZZ2_RX_NKCurrencyOverride"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ2_StartDate"" Type=""datetime"" />
      <Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" MaxLength=""3"" ConstantValue=""ADD"" />
      <Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CA"" />
      <Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CA"" />
    </EntityType>
    <EntityType Name=""RefCusApplicability"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZT_ZZA_NKTradeGroup"" />
        <PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZT_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
      <Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CA"" />
    </EntityType>
  </Schema>
  <RefCusTariff>
    <ZZ1_Description>Copper Pipe Fittings 2</ZZ1_Description>
    <ZZ1_StartDate>2014-02-14T00:00:00</ZZ1_StartDate>
    <ZZ1_TariffCode>AD1734</ZZ1_TariffCode>
    <ZZ1_ZZI_NKTariffType>SIMA</ZZ1_ZZI_NKTariffType>
    <RefCusRate>
      <ZZ2_RateFormula>0.25 * KGM</ZZ2_RateFormula>
      <ZZ2_RX_NKCurrencyOverride>RMB</ZZ2_RX_NKCurrencyOverride>
      <ZZ2_StartDate>2014-02-14T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>ADD</ZZ2_ZY1_NKRateCode>
      <RefCusApplicability>
        <ZZT_StartDate>2014-02-14T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKTradeGroup>CN</ZZT_ZZA_NKTradeGroup>
      </RefCusApplicability>
      <RefCusApplicability>
        <ZZT_StartDate>2014-02-14T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKTradeGroup>US</ZZT_ZZA_NKTradeGroup>
      </RefCusApplicability>
    </RefCusRate>
    <RefCusTariffRelationship>
      <ZZH_TariffCode>8428310000</ZZH_TariffCode>
    </RefCusTariffRelationship>
  </RefCusTariff>
</UniversalReferenceData>";
		#endregion

		const string html2 = @"<main role=""main"" property=""mainContentOfPage"" class=""container"">
<!-- MainContentStart -->
<h1 class=""mrgn-tp-md"" id=""wb-cont"">Certain Aluminum Extrusions<br>
<span style=""font-weight:normal"">Dumping &amp; Subsidizing (China)</span></h1>
<dl class=""dl-horizontal"">
	<dt>Product Information</dt>
	<dd>
		<h2 class=""h5 mrgn-tp-sm"">Product Definition</h2>
		<p>The subject goods are defined as:</p>
		<div class=""well"">
			<p class=""mrgn-bttm-0"">""Aluminum extrusions produced via an extrusion process, of alloys having metallic elements falling within the alloy designations published by The Aluminum Association commencing with 1, 2, 3, 5, 6 or 7 (or proprietary or other certifying body equivalents), with the finish being as extruded (mill), mechanical, anodized or painted or otherwise coated, whether or not worked, having a wall thickness greater than 0.5 mm., with a maximum weight per meter of 22 kilograms and a profile or cross-section which fits within a circle having a diameter of 254 mm., originating in/or exported from the People's Republic of China.""</p>
		</div>
	</dd>
	<dd>
		<h2 class=""h5 mrgn-tp-sm"">Exclusions</h2>
		<ul>
			<li>aluminum extrusions produced from either a 6063 or a 6005 alloy type with a T6 temper designation, in various lengths, with a powder coat finish on both the interior and the exterior surfaces of the extrusion, which finish is certified to meet the American Architectural Manufacturers Association AAMA 2603 standard, ""Voluntary Specification, Performance Requirements and Test Procedures for Pigmented Organic Coatings on Aluminum Extrusions and Panels"", for use in exterior railing systems;</li>
			<li>aluminum extrusions produced from a 6063 alloy type with a T5 temper designation, having a length of 3.66 m, with a powder coat finish, which finish is certified to meet the American Architectural Manufacturers Association AAMA 2603 standard, ""Voluntary Specification, Performance Requirements and Test Procedures for Pigmented Organic Coatings on Aluminum Extrusions and Panels"", for use as head rails and bottom rails in fabric window shades and blinds where the fabric has a cross-sectional honeycomb or ""cellular"" construction;</li>
			<li>aluminum extrusions produced from a 6063 alloy type with a T5 temper designation and forming part of the Vario System™ 20, 30, 40, 45 and 60 series line of profiles, or equivalent, having a length of either 4.5 or 5.8 m and a straightness tolerance of +/-1.5 mm or less per 6.0 m of length, for use in those parts of mechanical systems and automated machinery, such as gantry systems and conveyors, where precise linear movement is required;</li>
			<li>aluminum extrusions produced from either a 6063 or a 6463 alloy type, having a length of 3 m, with a hand-applied gold and silver leaf finish, for use as picture frame mouldings;</li>
			<li>aluminum extrusions produced from a 6063 alloy type with either a T5 or a T6 temper designation, having a length of between 20 and 33 ft. (between 6.10 and 10.06 m), with a powder coat finish, which finish is certified to meet the American Architectural Manufacturers Association AAMA 2603 standard (""Voluntary Specification, Performance Requirements and Test Procedures for Pigmented Organic Coatings on Aluminum Extrusions and Panels""), for use in window frames;</li>
			<li>heat sinks imported under tariff item No. 8473.30.90 and weighing 700 g or less; and</li>
			<li>aluminum extrusions produced by China Square Industrial Ltd. from either a 6063 or a 6463 alloy type with a T5 temper designation, with a profile or cross-section which fits within a circle having a diameter of 100 mm, for use by MAAX Bath Inc. in the assembly of its shower enclosures, specifically identified in the Appendix of the Determination and reasons issued by the Canadian International Trade Tribunal on February 10, 2011, in Inquiry No. NQ-2008-003R. The list of these excluded products can be found at the following link:<a href=""http://www.citt.gc.ca/en/node/6412#_Toc387403752"">http://www.citt.gc.ca/en/node/6412#_Toc387403752</a></li>
		</ul>
		<p>For the excluded products listed above that require a finish which is ""certified to meet the American Architectural Manufacturers Association AAMA 2603 standard"", the importer must be able to provide evidence that their goods meet that standard. </p>
	</dd>
	<dt>Investigation Information</dt>
	<dd>
		<p>The dates of the investigative proceedings and findings concerning this case are:</p>
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date </th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1379/ad1379-i08-de-eng.html"">Initiation of Investigation</a></td>
					<td>August 18, 2008</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1379/ad1379-i08-pd-eng.html"">Preliminary Determination</a></td>
					<td>November 17, 2008</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1379/ad1379-i08-fd-eng.html"">Final Determination</a></td>
					<td>February 16, 2009</td>
				</tr>
				<tr>
					<td><a href=""http://www.citt.gc.ca/dumping/inquirie/findings/archive_nq2i003_e"">Canadian International Trade Tribunal's Finding</a></td>
					<td>March 17, 2009</td>
				</tr>
			</tbody>
		</table>
	</dd>
	<dt>Tariff Classification Numbers</dt>
	<dd>
		<p>The subject goods are usually classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7604.10.00.30</li>
			<li>7604.10.00.40</li>
			<li>7604.21.00.10</li>
		</ul>
		<p>Please note that these classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>
	</dd>
	<dt>Duty Liability<br>
	(Anti-dumping duties)</dt>
	<dd>
		<h2 class=""h5 mrgn-tp-sm"">Country of Origin or Export: China</h2>
		<p>Effective on imports of subject goods released by the CBSA on or after 2012-02-20:</p>
		<p>Information regarding the normal values of subject goods should be obtained from the exporter. For a list of exporters in China who currently have normal values, please consult the <b><a href=""/sima-lmsi/ri-re/ad1379/ad1379-ri11-nc-eng.html"">CBSA Notice of Conclusion of Re-investigation</a></b>.</p>
		<p>For importations of subject goods originating in/or exported from China for which the exporter has not been issued its own normal values, the anti-dumping duty is equal to 101% of the export price.</p>
	</dd>
	<dt>Duty Liability<br>
	(Countervailing duties)</dt>
	<dd>
		<h2 class=""h5 mrgn-tp-sm"">Country of Origin or Export: China</h2>
		<p>Effective on imports of subject goods released by the CBSA on or after 2012-02-20:</p>
		<p>For a list of exporters in China who currently have a specific amount of subsidy, please consult the <b><a href=""/sima-lmsi/ri-re/ad1379/ad1379-ri11-nc-eng.html"">CBSA Notice of Conclusion of Re-investigation</a></b>.</p>
		<p>For importations of subject goods originating in/or exported from China for which the exporter has not been issued its own amount of subsidy, the countervailing duty is equal to 15.84 Renminbi per kilogram.</p>
	</dd>
	<dt>Disclosure of Normal Values and Amounts of Subsidy </dt>
	<dd>
		<p>The liability for anti-dumping and countervailing duty results from the proceedings conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a> and from the finding of the CITT. Information regarding the normal value and amount of subsidy of the subject goods in question and the amount of anti-dumping and countervailing duty payable should be obtained from the exporter. Related information may be made available to importers on a need-to-know basis in accordance with the provisions of <a href=""/publications/dm-md/d14/d14-1-2-eng.html"">Memorandum D14-1-2</a>, <i>Disclosure of Normal Values, Export Prices, and Amounts of Subsidy Established Under the Special Import Measures Act to Importers.</i></p>
		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
	</dd>
	<dt>Information Required on Customs Documents</dt>
	<dd>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap/menu-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		<p>The import documentation should clearly indicate the following:</p>
		<ul>
			<li>Confirmation whether the product is subject to anti-dumping and countervailing duty</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Customer's name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description of the goods, including a physical description of the type/shape of the extrusions, alloy, finish, and whether the goods fall within the scope of subject goods:</li>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (including, the unit of measure)</li>
			<li>Unit selling price, total selling price</li>
			<li>Currency of settlement used (e.g., US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g., FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada from the point of direct shipment (including, the inland and ocean freight, insurance, etc.).</li>
			<li>The amount of any export taxes applicable to the goods.</li>
		</ul>
	</dd>
	<dt>Appeal Decisions Relating to Subjectivity</dt>
	<dd>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
	</dd>
	<dt>Email for Duty Assessment Questions</dt>
	<dd>
		<p><a href=""mailto:trade_programs-programmes_commerciaux@cbsa-asfc.gc.ca"">Trade_Programs-Programmes_commerciaux@cbsa-asfc.gc.ca</a></p>
	</dd>
	<dt>CBSA Reference Number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>Dumping file #: 4214-22</li>
			<li>Dumping case #: AD1379</li>
			<li>Subsidy file #: 4218-26</li>
			<li>Subsidy case #: CV124</li>
		</ul>
	</dd>
	<dt>CITT Reference Number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>NQ-2008-003</li>
			<li>NQ-2008-003R</li>
			<li>RR-2013-003</li>
		</ul>
	</dd>
</dl>
<dl id=""wb-dtmd"">
<dt>Date modified: </dt>
<dd><time property=""dateModified"">2019-06-28</time></dd>
</dl>
</main>";

		[Test]
		public void TestUpdateTypeFullOnXML()
		{
			var helper = new Mock<IHttpClientHelper>();
			helper.Setup(x => x.GetMatchedEntityCodesAsync("XXX", It.IsAny<string[]>()))
			.Returns<string, string[]>((url, inputs) =>
			{
				if (inputs[0] == "China")
				{
					return Task.FromResult(new[] { "CN" });
				}
				else if (inputs[0] == "Republic of Korea")
				{
					return Task.FromResult(new[] { "KR" });
				}
				else if (inputs[0] == "European Union")
				{
					return Task.FromResult(new[] { "EU" });
				}
				return Task.FromResult(new string[0]);
			}
		);

			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "kilogram" }))
				.Returns(Task.FromResult(new string[] { "KGM" }));

			var simaUrl = @"https://www.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/menu-eng.html";
			var simaBaseUrl = @"https://www.cbsa-asfc.gc.ca";

			var client = new Mock<IWebDriverHelper>();
			client.Setup(x => x.GetWebPage(simaUrl, It.IsAny<int>())).Returns(html2);
			var extractor = new SIMATextExtractor(helper.Object, "XXX");
			var parser = new Mock<IWebSIMAParserV2>();

			IEnumerable<WebSIMAText> GetWebSIMA()
			{
				var webSIMAText = new WebSIMAText
				{
					ClassificationNumbers = new[] { "8428310000" },
					CBSAReferenceNumber = "AD1734",
					Description = "Copper Pipe Fittings 2",
					Duties = new[] { new WebSIMADuty
				{
					DutyType = "ADD",
					DutyValue = "0.25 * KGM",
					DutyCurrency = "RMB",
					CountryOfOriginOrExport = new [] { "CN", "US" },
					EffectiveDate = "2014-02-14"
				} }
				};
				yield return webSIMAText;
			}

			parser.Setup(x => x.GetWebSIMAText(simaUrl, simaBaseUrl)).Returns(GetWebSIMA);
			parser.Setup(x => x.PublishedDate).Returns(new DateTime(2023, 10, 17));

			new CASIMADataProducer().ProcessCASIMA(parser.Object);

			var expectedResult = string.Format(CultureInfo.InvariantCulture, expectedFile, new DateTime(2023, 10, 17).ToString("yyyy-MM-ddT00:00:00", CultureInfo.InvariantCulture));
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			string OutputFileAndPath = binPath + @"\..\..\UXmlFiles\CASIMATest.xml";
			Assert.That(File.ReadAllText(OutputFileAndPath), Is.EqualTo(expectedResult));

			if (File.Exists(OutputFileAndPath))
			{
				File.Delete(OutputFileAndPath);
			}
		}
	}
}
