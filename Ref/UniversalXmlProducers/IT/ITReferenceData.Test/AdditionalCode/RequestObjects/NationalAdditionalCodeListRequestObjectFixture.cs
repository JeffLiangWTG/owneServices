using System;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.AdditionalCode
{
	[TestFixture]
	class NationalAdditionalCodeListRequestObjectFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new NationalAdditionalCodeListRequestObject(dateTimeProvider: null, additionalCodeType: "A"), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new NationalAdditionalCodeListRequestObject(new Mock<IDateTimeProvider>().Object, additionalCodeType: null), "When additionalCodeType is null");
			Assert.Throws<ArgumentException>(() => new NationalAdditionalCodeListRequestObject(new Mock<IDateTimeProvider>().Object, additionalCodeType: ""), "When additionalCodeType is empty");
		}

		[Test]
		public void Build()
		{
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 07, 13));
			var requestObject = new NationalAdditionalCodeListRequestObject(dateTimeProviderMock.Object, "Q").Build();
			Assert.Multiple(() =>
			{
				Assert.AreEqual("11", requestObject["UC"], "UC Value");
				Assert.AreEqual("1", requestObject["SC"], "SC Value");
				Assert.AreEqual("2", requestObject["ST"], "ST Value");
				Assert.AreEqual("102", requestObject["Label"], "Label Value");
				Assert.AreEqual("", requestObject["Cadd.DataInizioValidita"], "Cadd.DataInizioValidita Value");
				Assert.AreEqual("Q", requestObject["Cadd.Tipo"], "Cadd.Tipo Value");
				Assert.AreEqual("", requestObject["GG-Cadd.DataInizioValidita"], "GG-Cadd.DataInizioValidita Value");
				Assert.AreEqual("", requestObject["MM-Cadd.DataInizioValidita"], "MM-Cadd.DataInizioValidita Value");
				Assert.AreEqual("", requestObject["AAAA-Cadd.DataInizioValidita"], "AAAA-Cadd.DataInizioValidita Value");
				Assert.AreEqual("", requestObject["Cadd.CodiceDa"], "Cadd.CodiceDa Value");
				Assert.AreEqual("", requestObject["Cadd.CodiceA"], "Cadd.CodiceA Value");
				Assert.AreEqual(
					"it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%8713%2F07%2F2022%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String" +
					"%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER" +
					"%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87TREE" +
					"%C3%87consultazione%3Ecadd+nazionale%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea" +
					"%C3%B5java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87FINDER%C3%87%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ACaddServlet%3A10%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87MODE" +
					"%C3%87java.lang.String%C3%860%C3%87%C3%B5"
					, requestObject["$STACK$"], "$STACK$ Value");
			});
		}
	}
}
