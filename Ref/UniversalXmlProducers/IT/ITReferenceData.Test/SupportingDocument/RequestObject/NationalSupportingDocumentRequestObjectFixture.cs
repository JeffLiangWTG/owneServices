using System;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class NationalSupportingDocumentRequestObjectFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new NationalSupportingDocumentRequestObject(dateTimeProvider: null, parameters: new Mock<IRawSupportingDocumentRequestObjectParameters>().Object), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new NationalSupportingDocumentRequestObject(dateTimeProvider: new Mock<IDateTimeProvider>().Object, parameters: null), "When parameters is null");
		}

		[Test]
		public void Build()
		{
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 01, 1));
			var parametersMock = new Mock<IRawSupportingDocumentRequestObjectParameters>();
			parametersMock.Setup(x => x.SC).Returns("A");
			parametersMock.Setup(x => x.ST).Returns("B");
			parametersMock.Setup(x => x.UC).Returns("C");
			parametersMock.Setup(x => x.Label).Returns("D");
			parametersMock.Setup(x => x.Suffix).Returns("E");
			parametersMock.Setup(x => x.ProgressiveNumber).Returns("F");
			parametersMock.Setup(x => x.DescriptionValidityStartDate).Returns("G");
			var requestObject = new NationalSupportingDocumentRequestObject(dateTimeProviderMock.Object, parametersMock.Object).Build();
			Assert.Multiple(() =>
			{
				Assert.AreEqual("A", requestObject["SC"], "SC Value");
				Assert.AreEqual("B", requestObject["ST"], "ST Value");
				Assert.AreEqual("C", requestObject["UC"], "UC Value");
				Assert.AreEqual("it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%8701%2F01%2F2022%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%" +
					"C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%" +
					"C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%" +
					"C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3E+certificato+nazionale%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%" +
					"C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ONEREC%C3%87java.lang.String%C3%86Y%C3%87java.lang.String%C3%87FINDER%" +
					"C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.CertificatoNazFinder%C3%86%C3%94%C3%94%C3%941%C3%94%C3%94%C3%94%C3%94%C3%87java.lang.String%C3%87PKCERT%C3%87%C3%87%" +
					"C3%B5it.finanze.eax.util.Action%C3%B5%3ACertificatoNazServlet%3A18%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ONEREC%C3%87java.lang.String%C3%86N%C3%87java.lang.String%C3%87FINDER%" +
					"C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.CertificatoNazFinder%C3%86%C3%94%C3%94%C3%941%C3%9401%2F01%2F2022%C3%94%C3%94%C3%94%C3%87java.lang.String%C3%87PKCERT%C3%87%C3%87%C3%B5", requestObject["$STACK$"], "$STACK$ Value");
				Assert.AreEqual("D", requestObject["Label"], "Label Value");
				Assert.AreEqual("E", requestObject["CertificatoNaz.Certificato"], "CertificatoNaz.Certificato Value");
				Assert.AreEqual("F", requestObject["CertificatoNaz.NumeroProgressivo"], "CertificatoNaz.NumeroProgressivo Value");
				Assert.AreEqual("G", requestObject["CertificatoNaz.DataInizioValiditaDescrizione"], "CertificatoNaz.DataInizioValiditaDescrizione Value");
				Assert.AreEqual("1", requestObject["CertificatoNaz.DataFineValiditaDescrizione"], "CertificatoNaz.DataFineValiditaDescrizione Value");
				Assert.AreEqual("FALSE", requestObject["DATA_PROVENIENZA_MISURE"], "DATA_PROVENIENZA_MISURE Value");
				Assert.AreEqual("FALSE", requestObject["PROVENIENZA_MISURE"], "PROVENIENZA_MISURE Value");
			});
		}
	}
}
