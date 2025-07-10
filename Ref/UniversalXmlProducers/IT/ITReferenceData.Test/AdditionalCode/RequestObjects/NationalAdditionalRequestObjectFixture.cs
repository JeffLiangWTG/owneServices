using System;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.AdditionalCode
{
	[TestFixture]
	class NationalAdditionalRequestObjectFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new NationalAdditionalRequestObject(dateTimeProvider: null, parameters: new Mock<INationalRawAdditionalCodeRequestObjectParameters>().Object), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new NationalAdditionalRequestObject(dateTimeProvider: new Mock<IDateTimeProvider>().Object, parameters: null), "When parameters is null");
		}

		[Test]
		public void Build()
		{
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 07, 13));
			var parametersMock = new Mock<INationalRawAdditionalCodeRequestObjectParameters>();
			parametersMock.Setup(x => x.SC).Returns("A");
			parametersMock.Setup(x => x.ST).Returns("B");
			parametersMock.Setup(x => x.UC).Returns("C");
			parametersMock.Setup(x => x.Label).Returns("D");
			parametersMock.Setup(x => x.AdditionalCodeSequentialNumber).Returns("E");
			parametersMock.Setup(x => x.AdditionalCodeType).Returns("F");
			parametersMock.Setup(x => x.ValidityStartDate).Returns("01/01/1995");
			parametersMock.Setup(x => x.SidCad).Returns("X");
			var requestObject = new NationalAdditionalRequestObject(dateTimeProviderMock.Object, parametersMock.Object).Build();
			Assert.Multiple(() =>
			{
				Assert.AreEqual("A", requestObject["SC"], "SC Value");
				Assert.AreEqual("B", requestObject["ST"], "ST Value");
				Assert.AreEqual("C", requestObject["UC"], "UC Value");
				Assert.AreEqual("D", requestObject["Label"], "Label Value");
				Assert.AreEqual("E", requestObject["Cadd.Codice"], "Cadd.Codice Value");
				Assert.AreEqual("F", requestObject["Cadd.Tipo"], "Cadd.Tipo Value");
				Assert.AreEqual("01/01/1995", requestObject["Cadd.DataInizioValidita"], "Cadd.DataInizioValidita Value");
				Assert.AreEqual("1", requestObject["Input"], "Input Value");
				Assert.AreEqual("X", requestObject["Cadd.SidCadd"], "Cadd.SidCadd Value");
				Assert.AreEqual("1", requestObject["Cadd.CodiceNotaAssociata"], "Cadd.CodiceNotaAssociata Value");
				Assert.AreEqual("1", requestObject["Cadd.TipoNotaAssociata"], "Cadd.TipoNotaAssociata Value");
				Assert.AreEqual("1", requestObject["Cadd.DataInizioValiditaDescrizioneNotaAssociata"], "Cadd.DataInizioValiditaDescrizioneNotaAssociata Value");
				Assert.AreEqual("it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%8713%2F07%2F2022%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87" +
					"%C3%87java.lang.String%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87" +
					"%C3%87java.lang.String%C3%87EXT_CALLER%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String" +
					"%C3%87CHECK%C3%87%C3%87java.lang.String%C3%87PK%C3%87%C3%87java.lang.String%C3%87TREE%C3%87consultazione%3Ecadd+nazionale%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5it.finanze.eax.util.Action" +
					"%C3%B5%3ATaricServlet%3A1%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87ONEREC%C3%870%C3%87java.lang.String%C3%87FINDER%C3%87it.finanze.ag_dogane.dogane.dogana.taric.common.util.CaddFinder" +
					"%C3%86%C3%94%C3%94Q%C3%941%C3%9413%2F07%2F2022%C3%94%C3%87%C3%B5it.finanze.eax.util.Action%C3%B5%3ACaddServlet%3A10%3A1%3A2%3A%C3%B5it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87MODE%C3%87java.lang.String" +
					"%C3%860%C3%87%C3%B5"
					, requestObject["$STACK$"], "$STACK$ Value");
			});
		}
	}
}
