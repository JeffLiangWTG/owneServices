using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	public class HtmlDataExtractorTest
	{
		[Test]
		public void HTMLExtractor_RetrievesDateUpdatedOn_WhenValidHTMLIsReceived()
		{
			var dataUpdatedOnDate = HtmlDataExtractor.HtmlDataExtractor.GetDataUpdatedOnDate(SampleHtml);

			Assert.True(DateTime.Compare(dataUpdatedOnDate, new DateTime(2017, 08, 30)) == 0);
		}

		[Test]
		public void HTMLExtractor_RetrievesCustomsInformation_WhenValidHTMLIsReceived()
		{
			var nationalInformationSection = HtmlDataExtractor.HtmlDataExtractor.GetNationalInformationSection(SampleHtml);

			Assert.True(nationalInformationSection.StartsWith(@"<table id=""fourth_table"">", StringComparison.OrdinalIgnoreCase));
			Assert.True(nationalInformationSection.EndsWith(@"</table>", StringComparison.OrdinalIgnoreCase));
			Assert.True(nationalInformationSection.Contains("Nazionali"));
		}

		[Test]
		public void GetNationalInformationSection_WhenInvalidHTMLIsReceived_ThenNothingIsReturned()
		{
			var nationalInformationSection = HtmlDataExtractor.HtmlDataExtractor.GetNationalInformationSection("<html><body>Invalid content</body></html>");

			Assert.IsNull(nationalInformationSection);
		}

		[Test]
		public void GetCertificateParameterStringTest()
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ITTariffCertificateLinkPage.html"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				var htmlContent = reader.ReadToEnd();
				var result = HtmlDataExtractor.HtmlDataExtractor.GetCertificateParameterString(htmlContent);
				Assert.AreEqual("5, 1, -2, 100, 'C', '678', '01/01/2010' ", result[0]);
			}
		}

		[Test]
		public void GetMultipleCertificateParameterStringsTest()
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ITTariffMultipleCertificateLinks1006302510.html"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				var htmlContent = reader.ReadToEnd();
				var result = HtmlDataExtractor.HtmlDataExtractor.GetCertificateParameterString(htmlContent);
				Assert.AreEqual("5, 1, -2, 100, 'C', '085', '14/12/2019' ", result[0]);
				Assert.AreEqual("88,1,2,1,'YY','07','25/02/2014'", result[1]);
			}
		}

		[Test]
		public void GetCertificateDataTest()
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ITTariffCertificatePage.html"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				var htmlContent = reader.ReadToEnd();
				var result = HtmlDataExtractor.HtmlDataExtractor.GetCertificateData(htmlContent);
				Assert.AreEqual("C678", result.CertificateNumber);
				Assert.AreEqual("SUP", result.ConditionValueType);
			}
			using (var stream = GetType().Assembly.GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ITTariffCertificatePage2.html"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				var htmlContent = reader.ReadToEnd();
				var result = HtmlDataExtractor.HtmlDataExtractor.GetCertificateData(htmlContent);
				Assert.AreEqual("07CS", result.CertificateNumber);
				Assert.AreEqual("SUP", result.ConditionValueType);
			}
		}

		[Test]
		public void GetRequirementDataTest()
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ITTariffPage9503002190.html"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				var htmlContent = reader.ReadToEnd();
				var result = HtmlDataExtractor.HtmlDataExtractor.GetNationalInformationSection(htmlContent);
				var scrappedRecords = HtmlDataExtractor.HtmlDataExtractor.ScrapRecordsFromHtml(result).ToList();
				Assert.AreEqual("Controlli sanitari", scrappedRecords[0].Measure.Description);
				Assert.AreEqual("", scrappedRecords[0].Measure.Formula);
				Assert.AreEqual("Controlli sanitari", scrappedRecords[1].Measure.Description);
				Assert.AreEqual("Certificato", scrappedRecords[1].Measure.Formula);
			}
		}

		[Test]
		public void GetAllMeasures_WhenValidStringIsPassed_ReturnsMeasure()
		{
			var scrappedRecords = HtmlDataExtractor.HtmlDataExtractor.ScrapRecordsFromHtml(NationalSection).ToList();

			Assert.IsNotNull(scrappedRecords);
			Assert.AreEqual(6, scrappedRecords.Count);

			Assert.True(scrappedRecords.First().Measure.Description.StartsWith("Contributo obbligatorio", StringComparison.OrdinalIgnoreCase));
			Assert.True(scrappedRecords.First().Measure.Formula.EndsWith("150 EURO/1000 kg", StringComparison.OrdinalIgnoreCase));

			Assert.True(scrappedRecords.First().Requirement.First().RequirementType.StartsWith("Regolamento", StringComparison.OrdinalIgnoreCase));
			Assert.True(scrappedRecords.First().Requirement.First().RequirementDescription.StartsWith(@"1 009500/1992", StringComparison.OrdinalIgnoreCase));
		}

		[Test]
		public void GetAllMeasures_WhenInvalidStringIsPassed_ThenNothingIsReturned()
		{
			var allMeasuresRawHtml = HtmlDataExtractor.HtmlDataExtractor.ScrapRecordsFromHtml("<table><tr><td>Some other data</td></tr>`");

			Assert.IsNull(allMeasuresRawHtml);
		}

		[Test]
		public void MeasureData_OnValidString_SetsRequirement()
		{
			const string measure = "\r\n\t\t\t\t\r\n\t\t\t\t\t\t\t\tContributo obbligatorio consorzio oli usati &nbsp;\r\n\t\t\t\t\t\t\t\t(\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t\t<a href=\"javascript:linkToPostKeyGruppo('MisureServlet',30,1,'-2','7','1011','G')\">\r\n\t\t\t\t\t\t\t\t\t\tERGA OMNES\r\n\t\t\t\t\t\t\t\t\t</a>\r\n\t\t\t\t\t\t\t\t)\r\n\t\t\t\t\t\t\t\t:&nbsp;\r\n\t\t\t\t\t\t\t\t150 EURO/1000 kg \r\n\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t\t";
			const string requirement = "\r\n\t\t\t\r\n\t\t\t\t\t\t\t\tRegolamento:&nbsp;\r\n\t\t\t\t\t\t\t\t<a href=\"javascript:linkToPostKeyRegolamentoNazionale('ProvBaseServlet',3,2,1,0,'1','0095','00','1992')\">1 009500/1992</a>\r\n\t\t\t\t\t\t\t\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t<a href=\"javascript:linkToPostKeyCadd('MisureServlet',30,1,-2,3,'    ','15/08/2017','')\">\r\n\t\t\t\t\t\t    \r\n\t\t\t\t\t\t\t\t</a>\r\n\t\t\t\t\t\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t&nbsp;\r\n\t\t\r\n\t\t\t\t\t";

			var actual = new ScrappedRecord(measure, requirement);

			Assert.IsNotNull(actual.Measure);
			Assert.AreEqual("Contributo obbligatorio consorzio oli usati", actual.Measure.Description);
			Assert.AreEqual("ERGA OMNES", actual.Measure.TradeGroup.First());
			Assert.AreEqual("150 EURO/1000 kg", actual.Measure.Formula);

			Assert.IsNotNull(actual.Requirement);
			Assert.AreEqual(1, actual.Requirement.Count());
			Assert.AreEqual("Regolamento", actual.Requirement.First().RequirementType);
			Assert.AreEqual(@"1 009500/1992", actual.Requirement.First().RequirementDescription);
		}

		[Test]
		public void GetTariffDescription()
		{
			var resourceContent = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test.TestFiles.ITTariffPage2713200000_WithNoteA148.html");
			var tariffDescription = HtmlDataExtractor.HtmlDataExtractor.GetTariffDescription(resourceContent);
			Assert.AreEqual("Bitume di petrolio(A144)(A148)", tariffDescription);
		}

		[Test]
		public void GetTariffDescription_UnableToFindXPath()
		{
			var tariffDescription = HtmlDataExtractor.HtmlDataExtractor.GetTariffDescription(SampleHtml);
			Assert.IsNull(tariffDescription);
		}

		public const string SampleHtml = @"<html>
<head>
<title>TARIC - MISURE</title>
</head>
<body>
<table id="""">
	<tr>
		<td></td>
		<td><font>Dati aggiornati al: 30/08/2017</font></td>
	</tr>
</table>
<TABLE id=""first_table"">
	<TBODY>
		<TR>
			<TD class=""TDPARAMETRO"" style=""color : navy;background-color : white;"" ><IMG src=""/nsiweb/images/logoagenziadelledogane.gif"" border=""0"" height=""47"" width=""122"">&nbsp;</TD>
		</TR>
		<TR>
			<TD style=""font-family : Thaoma;color : navy; font-size : xsmall;background-color : white;"" valign=""top""></TD>
		</TR>
  </TBODY>
</TABLE>

<TABLE id=""second_table"">
	<TR>
		<TD class=""TDPERCORSO"" colspan=""2"" width=""100%""><A href=""javascript:linkToPost('MisureServlet',30,1,'-1','112')"" title=""Torna alla HomePage"">home</a>><A href=""javascript:linkToPost('MisureServlet',30,1,'-1','119')"" title=""Torna al menu"">consultazione</a>>misure - importazione</TD>
	</TR>
	<TR>
		<TD class=""TDTITOLI"" colspan=""2"" WIDTH=""100%"">Elenco Misure Taric Importazione al 15/08/2017</TD>
	</TR>
	<TR>
		<TD class=""TDCRITERISX"" colspan=""2"" WIDTH=""100%"">
		</TD>
	</TR>
	<TR>
		<TD  CLASS=""TDBARRANAVIGAZIONE"">
		</TD>
		<TD class=""TDPARAMETRO"" WIDTH=100% >
			<table id=""third_table"">
				<TBODY>
				<tr>
					<td  valign=""top"" width=""100%"">
						<TABLE cellspacing=""0"" width=""100%"">
						<tr>
							<TD class=""TDNOMECOLONNA"" width=""15%"">&nbsp;Nomenclatura&nbsp;</TD>
							<TD class=""TDNOMECOLONNA"" width=""10%"">&nbsp;Taric</TD>
							<TD class=""TDNOMECOLONNA"">&nbsp;Descrizione</TD>
						</tr>
						<tr>
							<TD class=""TDOUTPUT"">&nbsp;87031018&nbsp;</TD>
							<TD class=""TDOUTPUT"">&nbsp;00&nbsp;</TD>
							<TD class=""TDOUTPUTSX"">altri&nbsp;</TD>
						</tr>
						<tr>
							<TD class=""TDPARAMETRO"" colspan=""3"">&nbsp;</TD>
						</tr>
						</TABLE>
					</td>
				</tr>
				<tr>
					<td align=""center"">" + NationalSection + @"</td>
				</tr>
				</tbody>
			</table>
		</td>
	</tr>
</TABLE>
</BODY>
</HTML>";

		const string NationalSection = @"<table id=""fourth_table"">
	<tr>
		<td class=""TDNOMECOLONNA"" colspan=""4"">Nazionali</td>
	</tr>
	<tr>
		<td class=""TDPARAMETRO"" colspan=""4"">&nbsp;</td>
	</tr>
	<tr>
		<td class=""TDOUTPUTSX"" colspan=""2"" width=""60%"">
			Contributo obbligatorio consorzio oli usati &nbsp; (<a href=""javascript:linkToPostKeyGruppo('MisureServlet',30,1,'-2','7','1011','G')"">ERGA OMNES </a>)
			:&nbsp;150 EURO/1000 kg
		</td>
		<td class=""TDOUTPUTSX"" colspan=""2"">
			Regolamento:&nbsp;<a href=""javascript:linkToPostKeyRegolamentoNazionale('ProvBaseServlet',3,2,1,0,'1','0095','00','1992')"">1 009500/1992</A>
			<a href=""javascript:linkToPostKeyCadd('MisureServlet',30,1,-2,3,'    ','15/08/2017','')""></a>
			&nbsp;
		</td>
	</tr>
	<tr>
		<td class=""TDOUTPUTSX"" colspan=""2"" width=""60%"">
			Accise&nbsp; (<a href=""javascript:linkToPostKeyGruppo('MisureServlet',30,1,'-2','7','1011','G')"">ERGA OMNES</a>)
			:&nbsp;100 EURO/1000 kg
		</td>
		<td class=""TDOUTPUTSX"" colspan=""2"">
			Regolamento:&nbsp;<a href=""javascript:linkToPostKeyRegolamentoNazionale('ProvBaseServlet',3,2,1,0,'1','0037','00','2010')"">1 003700/2010</A>
			Cadd:&nbsp;<a href=""javascript:linkToPostKeyCadd('MisureServlet',30,1,-2,3,'T001','15/08/2017','')"">T001</a>
			&nbsp;
		</td>
	</tr>
	<tr>
		<td class=""TDOUTPUTSX"" colspan=""2"" width=""60%"">
			Controlli sanitari (USMAF/PIF)&nbsp;(<a href=""javascript:linkToPostKeyGruppo('MisureServlet',30,1,'-2','7','1011','G')"">ERGA OMNES</a>)
			:&nbsp;<a href=""javascript:linkToPostKeyBill('MisureServlet',30,1,-2,6,'CSA','1000','87031018','00','T','028','08/03/2013')"">Certificato</a>&nbsp;
		</td>
		<td class=""TDOUTPUTSX"" colspan=""2"">
			Regolamento:&nbsp;<a href=""javascript:linkToPostKeyRegolamentoNazionale('ProvBaseServlet',3,2,1,0,'1','0037','00','2010')"">1 003700/2010</A>
			Cadd:&nbsp;<a href=""javascript:linkToPostKeyCadd('MisureServlet',30,1,-2,3,'T028','15/08/2017','')"">T028</a>
			&nbsp;
		</td>
	</tr>
	<tr>
		<td class=""TDOUTPUTSX"" colspan=""2"" width=""60%"">
			Imposta di consumo&nbsp;(<a href=""javascript:linkToPostKeyGruppo('MisureServlet',30,1,'-2','7','1011','G')"">ERGA OMNES</a>)
			:&nbsp;787.81 EURO/1000 kg
		</td>
		<td class=""TDOUTPUTSX"" colspan=""2"">
			Regolamento:&nbsp;<a href=""javascript:linkToPostKeyRegolamentoNazionale('ProvBaseServlet',3,2,1,0,'1','0135','00','2009')"">1 013500/2009</A>
			<a href=""javascript:linkToPostKeyCadd('MisureServlet',30,1,-2,3,'    ','15/08/2017','')""></a>
			&nbsp;
		</td>
	</tr>
	<tr>
		<td class=""TDOUTPUTSX"" colspan=""2"" width=""60%"">
			Imposta Valore Aggiunto&nbsp;(<a href=""javascript:linkToPostKeyGruppo('MisureServlet',30,1,'-2','7','1011','G')"">ERGA OMNES</a>)
			:&nbsp;22
		</td>
		<td class=""TDOUTPUTSX"" colspan=""2"">
			Regolamento:&nbsp;<a href=""javascript:linkToPostKeyRegolamentoNazionale('ProvBaseServlet',3,2,1,0,'1','0633','00','1972')"">1 063300/1972</A>
			<a href=""javascript:linkToPostKeyCadd('MisureServlet',30,1,-2,3,'    ','15/08/2017','')""></a>
			&nbsp;
		</td>
	</tr>
	<tr>
		<td class=""TDOUTPUTSX"" colspan=""2"" width=""60%"">
			Imposta Valore Aggiunto&nbsp;(<a href=""javascript:linkToPostKeyGruppo('MisureServlet',30,1,'-2','7','1011','G')"">ERGA OMNES</a>)
			:&nbsp;4
		</td>
		<td class=""TDOUTPUTSX"" colspan=""2"">
			Cadd:&nbsp;<a href=""javascript:linkToPostKeyCadd('MisureServlet',30,1,-2,3,'Q056','15/08/2017','')"">Q056</a>
			&nbsp;
		</td>
	</tr>
</table>";
	}
}
