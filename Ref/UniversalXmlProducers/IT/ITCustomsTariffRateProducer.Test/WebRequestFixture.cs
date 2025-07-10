using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	class WebRequestFixture
	{
		[Test]
		public void VerifyRequestObjectValues()
		{
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 01, 01));
			var requestObject = new TariffDownloadRequestObjectValues("0406401000", dateTimeProviderMock.Object);
			var expectedEncodedDateTime = "01%2F01%2F2022";
			var stackValue = "it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%87" + expectedEncodedDateTime + "%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Emisure+-+importazione%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ELAGR%C3%87%C3%87java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87Misure.PaeseGruppoRegione%C3%87%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaNC%C3%87%C3%87java.lang.String%C3%87FN%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87MODE%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PRG%C3%87%C3%87java.lang.String%C3%87EXT1%C3%87%C3%87java.lang.String%C3%87PKNOTA%C3%87%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaTar%C3%87%C3%87java.lang.String%C3%87COD_UC%C3%87%C3%87%C3%B5";

			var expectedObjects = new[]
			{
				new KeyValuePair<string, string>("Misure.CodiceNomenclaturaNC","04064010"),
				new KeyValuePair<string, string>("Misure.CodiceNomenclaturaTar","00"),
				new KeyValuePair<string, string>("Misure.PaeseGruppoRegione","ALL"),
				new KeyValuePair<string, string>("SC","1"),
				new KeyValuePair<string, string>("ST","2"),
				new KeyValuePair<string, string>("UC","30"),
				new KeyValuePair<string, string>("Label","102"),
				new KeyValuePair<string, string>("$STACK$",stackValue)
			};

			var actualObject = requestObject.AsDictionary();

			CollectionAssert.AreEquivalent(expectedObjects, actualObject);
		}

		[Test]
		public void VerifyCertificateLinkRequestObjectValues()
		{
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 01, 01));
			var requestObject = new CertificateLinkDownloadRequestObjectValues(CertificateLinkHtml, dateTimeProviderMock.Object);
			var expectedEncodedDateTime = "01%2F01%2F2022";
			var stackValue = "it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%87" + expectedEncodedDateTime + "%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Emisure+-+importazione%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87java.lang.String%C3%86" + expectedEncodedDateTime + "%C3%87java.lang.String%C3%87ELAGR%C3%87%C3%87java.lang.String%C3%87Misure.PaeseGruppoRegione%C3%87java.lang.String%C3%86ALL+%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaNC%C3%87java.lang.String%C3%8607020000%C3%87java.lang.String%C3%87FN%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.MisureFinder%C3%86null%C3%94null%C3%94ALL+%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%9407020000%C3%9499%C3%94null%C3%94P%C3%941%C3%94null%C3%94null%C3%94null%C3%94null%C3%94" + expectedEncodedDateTime + "%C3%94false%C3%94null%C3%94%C3%87java.lang.String%C3%87MODE%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87EXT1%C3%87%C3%87java.lang.String%C3%87PRG%C3%87java.lang.String%C3%86P%C3%87java.lang.String%C3%87PKNOTA%C3%87%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87COD_UC%C3%87%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaTar%C3%87java.lang.String%C3%8699%C3%87%C3%B5";

			var expectedObjects = new[]
			{
				new KeyValuePair<string, string>("Misure.CodiceNomenclaturaNC", "07020000"),
				new KeyValuePair<string, string>("SC", "1"),
				new KeyValuePair<string, string>("ST", "-2"),
				new KeyValuePair<string, string>("UC", "30"),
				new KeyValuePair<string, string>("Label", "6"),
				new KeyValuePair<string, string>("RUOLO_R", "1"),
				new KeyValuePair<string, string>("PROG_R", "1"),
				new KeyValuePair<string, string>("NUMERO_ORDINE", "1"),
				new KeyValuePair<string, string>("NUM_R", "1"),
				new KeyValuePair<string, string>("Misure.TipoRegolamento", "1"),
				new KeyValuePair<string, string>("Misure.TipoNotaAss", "1"),
				new KeyValuePair<string, string>("Misure.TipoCertificato", "1"),
				new KeyValuePair<string, string>("Misure.SidMisura", "1"),
				new KeyValuePair<string, string>("Misure.SidDesNotaAss", "1"),
				new KeyValuePair<string, string>("Misure.SidCadd", "1"),
				new KeyValuePair<string, string>("Misure.RuoloRegolamento", "1"),
				new KeyValuePair<string, string>("Misure.NumeroCertificato", "1"),
				new KeyValuePair<string, string>("Misure.InizioValDescrizioneNotaAss", "1"),
				new KeyValuePair<string, string>("Misure.IniValCaddAss", "1"),
				new KeyValuePair<string, string>("Misure.DataIniValMisura", "29/01/2013"),
				new KeyValuePair<string, string>("Misure.Condizione", "1"),
				new KeyValuePair<string, string>("Misure.CodiceRegolamento", "1"),
				new KeyValuePair<string, string>("Misure.CodiceNotaAss", "1"),
				new KeyValuePair<string, string>("Misure.CodiceNomenclaturaTar", "00"),
				new KeyValuePair<string, string>("Misure.AnnoRegolamento", "1"),
				new KeyValuePair<string, string>("DatiGenerali.PRG", "1"),
				new KeyValuePair<string, string>("DatiGenerali.CodPaeseRegGrp", "1"),
				new KeyValuePair<string, string>("DATA_RIFERIMENTO", "1"),
				new KeyValuePair<string, string>("COD_PAESE_GRUPPI", "1"),
				new KeyValuePair<string, string>("CertificatoNaz.DataInizioValiditaDescrizione", "1"),
				new KeyValuePair<string, string>("ANNO_R", "1"),
				new KeyValuePair<string, string>("$STACK$",stackValue),
				new KeyValuePair<string, string>("Misure.TipoMisura", "CSA"),
				new KeyValuePair<string, string>("Misure.CodicePaese", "1000"),
				new KeyValuePair<string, string>("MisureCodiceCadd", "010"),
				new KeyValuePair<string, string>("Misure.TipoCadd", "T")
			};

			var actualObject = requestObject.AsDictionary();

			foreach (var expectedObject in expectedObjects)
			{
				Assert.AreEqual(expectedObject.Value, actualObject[expectedObject.Key]);
			}
		}

		[Test]
		public void VerifyCertificateRequestObjectValues()
		{
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 01, 01));
			var requestObject = new CertificateDownloadRequestObjectValues("5, 1, -2, 100, 'C', '678', '14/12/2019'", new CertificateLinkDownloadRequestObjectValues(CertificateLinkHtml, dateTimeProviderMock.Object).AsDictionary(), "0702000099", dateTimeProviderMock.Object);
			var expectedEncodedDateTime = "01%2F01%2F2022";
			var stackValue = "it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%87" + expectedEncodedDateTime + "%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87N%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Emisure+-+importazione%3Econdizioni%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87java.lang.String%C3%86" + expectedEncodedDateTime + "%C3%87java.lang.String%C3%87ELAGR%C3%87%C3%87java.lang.String%C3%87Misure.PaeseGruppoRegione%C3%87java.lang.String%C3%86ALL+%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaNC%C3%87java.lang.String%C3%8607020000%C3%87java.lang.String%C3%87FN%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.MisureFinder%C3%86null%C3%94null%C3%94ALL+%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%9407020000%C3%9499%C3%94null%C3%94P%C3%941%C3%94null%C3%94null%C3%94null%C3%94null%C3%94" + expectedEncodedDateTime + "%C3%94false%C3%94null%C3%94%C3%87java.lang.String%C3%87MODE%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PRG%C3%87java.lang.String%C3%86P%C3%87java.lang.String%C3%87EXT1%C3%87%C3%87java.lang.String%C3%87PKNOTA%C3%87%C3%87java.lang.String%C3%87PK%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.MisureObject%C3%86null%C3%94null%C3%94010%C3%9407020000%C3%9400%C3%94null%C3%941000%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94T%C3%94null%C3%94CSA%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%941%C3%94null%C3%94%C3%94%C3%94%C3%9429%2F01%2F2013%C3%94" + expectedEncodedDateTime + "%C3%94%C3%94%C3%94%C3%94null%C3%94null%C3%94null%C3%94null%C3%94%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94null%C3%94%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaTar%C3%87java.lang.String%C3%8699%C3%87java.lang.String%C3%87COD_UC%C3%87%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3AMisureServlet%3A30%3A1%3A3%3A%C3%B5";

			var expectedObjects = new[]
			{
				new KeyValuePair<string, string>("Misure.TipoCertificato", "C"),
				new KeyValuePair<string, string>("Misure.NumeroCertificato", "678"),
				new KeyValuePair<string, string>("CertificatoNaz.DataInizioValiditaDescrizione", "14/12/2019"),
				new KeyValuePair<string, string>("SC", "1"),
				new KeyValuePair<string, string>("ST", "-2"),
				new KeyValuePair<string, string>("UC", "5"),
				new KeyValuePair<string, string>("Label", "100"),
				new KeyValuePair<string, string>("RUOLO_R", "1"),
				new KeyValuePair<string, string>("PROG_R", "1"),
				new KeyValuePair<string, string>("NUMERO_ORDINE", "1"),
				new KeyValuePair<string, string>("NUM_R", "1"),
				new KeyValuePair<string, string>("MisureCodiceCadd", "1"),
				new KeyValuePair<string, string>("Misure.TipoRegolamento", "1"),
				new KeyValuePair<string, string>("Misure.TipoNotaAss", "1"),
				new KeyValuePair<string, string>("Misure.TipoMisura", "1"),
				new KeyValuePair<string, string>("Misure.TipoCadd", "1"),
				new KeyValuePair<string, string>("Misure.SidMisura", "1"),
				new KeyValuePair<string, string>("Misure.SidDesNotaAss", "1"),
				new KeyValuePair<string, string>("Misure.SidCadd", "1"),
				new KeyValuePair<string, string>("Misure.RuoloRegolamento", "1"),
				new KeyValuePair<string, string>("Misure.InizioValDescrizioneNotaAss", "1"),
				new KeyValuePair<string, string>("Misure.IniValCaddAss", "1"),
				new KeyValuePair<string, string>("Misure.DataIniValMisura", "1"),
				new KeyValuePair<string, string>("Misure.Condizione", "1"),
				new KeyValuePair<string, string>("Misure.CodiceRegolamento", "1"),
				new KeyValuePair<string, string>("Misure.CodicePaese", "1"),
				new KeyValuePair<string, string>("Misure.CodiceNotaAss", "1"),
				new KeyValuePair<string, string>("Misure.CodiceNomenclaturaTar", "1"),
				new KeyValuePair<string, string>("Misure.CodiceNomenclaturaNC", "1"),
				new KeyValuePair<string, string>("Misure.AnnoRegolamento", "1"),
				new KeyValuePair<string, string>("DatiGenerali.PRG", "1"),
				new KeyValuePair<string, string>("DatiGenerali.CodPaeseRegGrp", "1"),
				new KeyValuePair<string, string>("DATA_RIFERIMENTO", "1"),
				new KeyValuePair<string, string>("COD_PAESE_GRUPPI", "1"),
				new KeyValuePair<string, string>("ANNO_R", "1"),
				new KeyValuePair<string, string>("$STACK$",stackValue)
			};

			var actualObject = requestObject.AsDictionary();

			foreach (var expectedObject in expectedObjects)
			{
				Assert.AreEqual(expectedObject.Value, actualObject[expectedObject.Key]);
			}
		}

		const string CertificateLinkHtml = @"				Controlli sanitari (USMAF/PIF)&nbsp;
				(

						<a href=""javascript:linkToPostKeyGruppo('MisureServlet',30,1,'-2','7','1011','G')"">
						ERGA OMNES
						</a>

				)
			:&nbsp;

						<a href=""javascript:linkToPostKeyBill('MisureServlet',30,1,-2,6,'CSA','1000','07020000','00','T','010','29/01/2013')"">Certificato</a>&nbsp;";
	}
}
