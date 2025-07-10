using System;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.ExportMeasures
{
	[TestFixture]
	sealed class TariffInformationRequestObjectFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new TariffInformationRequestObject(dateTimeProvider: null, tariffCode: "22082086"), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new TariffInformationRequestObject(dateTimeProvider: dateTimeProviderMock.Object, tariffCode: null), "When tariffCode is null");
		}

		[Test]
		public void Build()
		{
			var requestObject = new TariffInformationRequestObject(dateTimeProviderMock.Object, "22082086").Build();

			Assert.AreEqual(13, requestObject.Count);
			Assert.Multiple(() =>
			{
				Assert.AreEqual("30", requestObject["UC"], "UC Value");
				Assert.AreEqual("1", requestObject["SC"], "SC Value");
				Assert.AreEqual("2", requestObject["ST"], "ST Value");
				Assert.AreEqual("102", requestObject["Label"], "Label Value");
				Assert.AreEqual("01/05/2025", requestObject["Misure.DataRiferimento"], "Misure.DataRiferimento Value");
				Assert.AreEqual("22082086", requestObject["Misure.CodiceNomenclaturaNC"], "Misure.CodiceNomenclaturaNC Value");
				Assert.AreEqual("00", requestObject["Misure.CodiceNomenclaturaTar"], "Misure.CodiceNomenclaturaTar Value");
				Assert.AreEqual("01", requestObject["GG-Misure.DataRiferimento"], "GG-Misure.DataRiferimento Value");
				Assert.AreEqual("05", requestObject["MM-Misure.DataRiferimento"], "MM-Misure.DataRiferimento Value");
				Assert.AreEqual("2025", requestObject["AAAA-Misure.DataRiferimento"], "AAAA-Misure.DataRiferimento Value");
				Assert.AreEqual("ALL", requestObject["Misure.PaeseGruppoRegione"], "Misure.PaeseGruppoRegione Value");
				Assert.AreEqual("1", requestObject["IS_ESPORTAZIONE"], "IS_ESPORTAZIONE Value");
				Assert.AreEqual(
					"it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%8701%2F05%2F2025%C3%87java.lang.String" +
					"%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String" +
					"%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87" +
					"%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87" +
					"%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Emisure+-+esportazione%C3%87java.lang.String" +
					"%C3%87PK%C3%87%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A" +
					"%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ELAGR%C3%87%C3%87java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String" +
					"%C3%87Misure.PaeseGruppoRegione%C3%87%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaNC%C3%87%C3%87java.lang.String%C3%87FN%C3%87" +
					"%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87MODE%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String" +
					"%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PRG%C3%87%C3%87java.lang.String%C3%87EXT1%C3%87%C3%87java.lang.String%C3%87PKNOTA%C3%87" +
					"%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87Misure.CodiceNomenclaturaTar%C3%87%C3%87java.lang.String%C3%87COD_UC%C3%87%C3%87%C3%B5"
					, requestObject["$STACK$"], "$STACK$ Value");
			});
		}

		[SetUp]
		public void SetUp()
		{
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2025, 5, 1));
		}

		Mock<IDateTimeProvider> dateTimeProviderMock;
	}
}
