using System;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class EuropeanSupportingDocumentListRequestObjectFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new EuropeanSupportingDocumentListRequestObject(dateTimeProvider: null, certificateType: "A"), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new EuropeanSupportingDocumentListRequestObject(new Mock<IDateTimeProvider>().Object, certificateType: null), "When certificateType is null");
			Assert.Throws<ArgumentException>(() => new EuropeanSupportingDocumentListRequestObject(new Mock<IDateTimeProvider>().Object, certificateType: ""), "When certificateType is empty");
		}

		[Test]
		public void Build()
		{
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 02, 01));
			var requestObject = new EuropeanSupportingDocumentListRequestObject(dateTimeProviderMock.Object, "A").Build();
			Assert.Multiple(() =>
			{
				Assert.AreEqual("7", requestObject["UC"], "UC Value");
				Assert.AreEqual("1", requestObject["SC"], "SC Value");
				Assert.AreEqual("2", requestObject["ST"], "ST Value");
				Assert.AreEqual("102", requestObject["Label"], "Label Value");
				Assert.AreEqual("01/02/2022", requestObject["DatiGenerali.DataRiferimento"], "DatiGenerali.DataRiferimento Value");
				Assert.AreEqual("A", requestObject["DatiGenerali.TipoCertificato"], "DatiGenerali.TipoCertificato Value");
				Assert.AreEqual("01", requestObject["GG-DatiGenerali.DataRiferimento"], "GG-DatiGenerali.DataRiferimento Value");
				Assert.AreEqual("02", requestObject["MM-DatiGenerali.DataRiferimento"], "MM-DatiGenerali.DataRiferimento Value");
				Assert.AreEqual("2022", requestObject["AAAA-DatiGenerali.DataRiferimento"], "AAAA-DatiGenerali.DataRiferimento Value");
				Assert.AreEqual("", requestObject["DatiGenerali.NumeroCertificato"], "DatiGenerali.NumeroCertificato Value");
				Assert.AreEqual("", requestObject["DatiGenerali.NumeroCertificatoA"], "DatiGenerali.NumeroCertificatoA Value");
				Assert.AreEqual("01/02/2022", requestObject["DATA_PROVENIENZA_MISURE"], "DATA_PROVENIENZA_MISURE Value");
				Assert.AreEqual("FALSE", requestObject["PROVENIENZA_MISURE"], "PROVENIENZA_MISURE Value");
				Assert.AreEqual("it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%8701%2F02%2F2022%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String%C3%87ONERECNOTA%C3%87" +
					"%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK" +
					"%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Ecertificato%C3%87java.lang.String" +
					"%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ONEREC%C3%870%C3%87java.lang.String%C3%87FLAG_RIC%C3%87true%C3%87java.lang.String%C3%87CERTIF" +
					"%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.DatiGeneraliFinder%C3%86null%C3%94A%C3%94%C3%941%C3%94null%C3%9401%2F02%2F2022%C3%94%C3%94%C3%94%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ADatiGeneraliServlet%3A6%3A1%3A-1%3A%C3%B5"
					, requestObject["$STACK$"], "$STACK$ Value");
			});
		}
	}
}
