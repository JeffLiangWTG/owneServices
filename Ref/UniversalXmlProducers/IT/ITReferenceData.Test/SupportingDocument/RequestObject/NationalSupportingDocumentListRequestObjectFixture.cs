using System;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class NationalSupportingDocumentListRequestObjectFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new NationalSupportingDocumentListRequestObject(dateTimeProvider: null), "When dateTimeProvider is null");
		}

		[Test]
		public void Build()
		{
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 01, 1));
			var requestObject = new NationalSupportingDocumentListRequestObject(dateTimeProviderMock.Object).Build();
			Assert.Multiple(() =>
			{
				Assert.AreEqual("17", requestObject["UC"], "UC Value");
				Assert.AreEqual("1", requestObject["SC"], "SC Value");
				Assert.AreEqual("2", requestObject["ST"], "ST Value");
				Assert.AreEqual("it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%8701%2F01%2F2022%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%" +
					"C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%" +
					"C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%" +
					"C3%87CHECK%C3%87%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3E+certificato+nazionale%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%" +
					"C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ONEREC%C3%87java.lang.String%C3%86Y%C3%87java.lang.String%C3%87FINDER%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.CertificatoNazFinder%C3%86%" +
					"C3%94%C3%94%C3%941%C3%94%C3%94%C3%94%C3%94%C3%87java.lang.String%C3%87PKCERT%C3%87%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ACertificatoNazServlet%3A18%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%" +
					"C3%87FINDER%C3%87java.lang.String%C3%86%C3%87java.lang.String%C3%87PKCERT%C3%87%C3%87%C3%B5", requestObject["$STACK$"], "$STACK$ Value");
				Assert.AreEqual("102", requestObject["Label"], "Label Value");
				Assert.AreEqual("", requestObject["CertificatoNaz.SequenzaInserimento"], "CertificatoNaz.SequenzaInserimento Value");
				Assert.AreEqual("", requestObject["CertificatoNaz.Tipo"], "CertificatoNaz.Tipo Value");
				Assert.AreEqual("", requestObject["GG-CertificatoNaz.DataInizioValidita"], "GG-CertificatoNaz.DataInizioValidita Value");
				Assert.AreEqual("", requestObject["MM-CertificatoNaz.DataInizioValidita"], "MM-CertificatoNaz.DataInizioValidita Value");
				Assert.AreEqual("", requestObject["AAAA-CertificatoNaz.DataInizioValidita"], "AAAA-CertificatoNaz.DataInizioValidita Value");
				Assert.AreEqual("", requestObject["CertificatoNaz.CodiceDa"], "CertificatoNaz.CodiceDa Value");
				Assert.AreEqual("", requestObject["CertificatoNaz.CodiceA"], "CertificatoNaz.CodiceA Value");
				Assert.AreEqual("FALSE", requestObject["DATA_PROVENIENZA_MISURE"], "DATA_PROVENIENZA_MISURE Value");
				Assert.AreEqual("FALSE", requestObject["PROVENIENZA_MISURE"], "PROVENIENZA_MISURE Value");
			});
		}
	}
}
