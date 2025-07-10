using System;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.AdditionalCode
{
	[TestFixture]
	class NationalAdditionalCodeTypeListRequestObjectFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new NationalAdditionalCodeTypeListRequestObject(dateTimeProvider: null), "When dateTimeProvider is null");
		}

		[Test]
		public void Build()
		{
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.Now).Returns(new DateTime(2022, 07, 13));
			var requestObject = new NationalAdditionalCodeTypeListRequestObject(dateTimeProviderMock.Object).Build();
			Assert.Multiple(() =>
			{
				Assert.AreEqual("1", requestObject["UC"], "UC Value");
				Assert.AreEqual("1", requestObject["SC"], "SC Value");
				Assert.AreEqual("-2", requestObject["ST"], "ST Value");
				Assert.AreEqual("3", requestObject["Label"], "Label Value");
				Assert.AreEqual(
					"it.finanze.eax.util.LocalArea%C3%B5java.lang.String%C3%87DATARIF%C3%87%C3%87java.lang.String%C3%87DATA_ELAB%C3%8713%2F07%2F2022%C3%87java.lang.String%C3%87PROVENGO_DA_STRADA_ALTERNATIVA%C3%87%C3%87java.lang.String" +
					"%C3%87ONERECNOTA%C3%87%C3%87java.lang.String%C3%87USERID%C3%87%C3%87java.lang.String%C3%87CRITERI%C3%87%C3%87java.lang.String%C3%87C_G%C3%87%C3%87java.lang.String%C3%87ONEREC%C3%87%C3%87java.lang.String%C3%87EXT_CALLER" +
					"%C3%87%C3%87java.lang.String%C3%87PAESI_GRUPPI_LINK%C3%87%C3%87java.lang.String%C3%87CRITERI_APP%C3%87%C3%87java.lang.String%C3%87CERTIF%C3%87%C3%87java.lang.String%C3%87CHECK%C3%87%C3%87java.lang.String%C3%87PK%C3%87" +
					"%C3%87java.lang.String%C3%87TREE%C3%87consultazione%C3%87java.lang.String%C3%87GlobalArea%C3%871%C3%87%C3%B5"
					, requestObject["$STACK$"], "$STACK$ Value");
			});
		}
	}
}
