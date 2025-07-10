using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Messaging.Testing;
using Moq;

namespace Enterprise.Customs.TR.Messaging.Test
{
	class ManifestMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGetMessageText()
		{
			var mock = new Mock<ISummaryDeclarationInformation>();
			mock.Setup(m => m.ManifestType).Returns("DENİTH");
			mock.Setup(m => m.UserID).Returns("userIDData");
			mock.Setup(m => m.Nature).Returns("rejimData");
			mock.Setup(m => m.CarrierBusinessRegNo).Returns("tasiyiciVergiData");
			mock.Setup(m => m.XmlRefId).Returns("xmlRefIdData");
			trManifestMessageBuilder = new TRManifestMessageBuilder(mock.Object);
			var result = trManifestMessageBuilder.GetMessageText("userData", "passData", "refData", mock.Object);

			CombineAssertions("XML Content", () =>
			{
				Assert("xml", result.Contains("<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
				Assert("Gelen", result.Contains("<Gelen xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://LoginKontrol.YeniOzetBeyanGelen\">"));
				Assert("RefID", result.Contains("<RefID xmlns=\"\">refData</RefID>"));
				Assert("KullaniciAdi", result.Contains("<KullaniciAdi xmlns=\"\">userData</KullaniciAdi>"));
				Assert("Sifre", result.Contains("<Sifre xmlns=\"\">189dfc9eac5852b1dbfb8ef40d70e156</Sifre>"));
				Assert("OzetBeyanBilgisi", result.Contains($"<{XmlHelper.GetNamespacePrefix()}OzetBeyanBilgisi xmlns{XmlHelper.GetNamespaceSuffix()}=\"http://www.gumruk.gov.tr/\">"));
				Assert("BeyanTuru", result.Contains($"<{XmlHelper.GetNamespacePrefix()}BeyanTuru>DENİTH</{XmlHelper.GetNamespacePrefix()}BeyanTuru>"));
				Assert("EkBelgeSayisi", result.Contains($"<{XmlHelper.GetNamespacePrefix()}EkBelgeSayisi>0</{XmlHelper.GetNamespacePrefix()}EkBelgeSayisi>"));
				Assert("KullaniciKodu", result.Contains($"<{XmlHelper.GetNamespacePrefix()}KullaniciKodu>userIDData</{XmlHelper.GetNamespacePrefix()}KullaniciKodu>"));
				Assert("Rejim", result.Contains($"<{XmlHelper.GetNamespacePrefix()}Rejim>rejimData</{XmlHelper.GetNamespacePrefix()}Rejim>"));
				Assert("TasiyiciFirma", result.Contains($"<{XmlHelper.GetNamespacePrefix()}TasiyiciFirma />"));
				Assert("TasiyiciVergiNo", result.Contains($"<{XmlHelper.GetNamespacePrefix()}TasiyiciVergiNo>tasiyiciVergiData</{XmlHelper.GetNamespacePrefix()}TasiyiciVergiNo>"));
				Assert("VarisTarihSaati", result.Contains($"<{XmlHelper.GetNamespacePrefix()}VarisTarihSaati>0001-01-01T00:00:00</{XmlHelper.GetNamespacePrefix()}VarisTarihSaati>"));
				Assert("XmlRefId", result.Contains($"<{XmlHelper.GetNamespacePrefix()}XmlRefId>xmlRefIdData</{XmlHelper.GetNamespacePrefix()}XmlRefId>"));
				Assert("OzetBeyanBilgisi", result.Contains($"</{XmlHelper.GetNamespacePrefix()}OzetBeyanBilgisi>"));
				Assert("Gelen", result.Contains("</Gelen>"));
			});
		}
		TRManifestMessageBuilder trManifestMessageBuilder;
	}
}
