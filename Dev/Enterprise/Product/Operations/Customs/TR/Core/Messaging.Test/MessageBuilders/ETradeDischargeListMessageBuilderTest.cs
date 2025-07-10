using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Messaging.Testing
{
	sealed class ETradeDischargeListMessageBuilderTest : TestCase
	{
		public void TestRequestMessageForDischargeList()
		{
			var iPack = new Mock<IPackBL>();
			iPack.Setup(m => m.LineNo).Returns("1");
			iPack.Setup(m => m.GoodDescription).Returns("DOKÜMAN");
			iPack.Setup(m => m.Tariff).Returns("920110100000");
			iPack.Setup(m => m.Unit).Returns("KGM");

			var iBill = CreateBillMock("1", "2247522717", "HVILEV SERGEY", "YAPI VE KREDI BANKASI AS", "CsngTaxNo01", 0.10, 0.10, new IPackBL[] { iPack.Object });
			var iBill2 = CreateBillMock("2", "2250076994", "MOLDCELL IM SA", "FIBABANKA YILDIZ BRANCH", "CsngTaxNo02", 0.20, 0.20, new IPackBL[] { iPack.Object });

			var iDischargeList = new Mock<IDischargeList>();
			iDischargeList.Setup(m => m.DeclarationOwnerRepresentativeNameAndTitle).Returns("ELİ ULUS. TAŞ. TUR. TİC. LTD. ŞTİ.");
			iDischargeList.Setup(m => m.DeclarationOwnerRepresentativeTaxNo).Returns("3310519833");
			iDischargeList.Setup(m => m.CustomsOffice).Returns("341453");
			iDischargeList.Setup(m => m.GoodsLocationName).Returns("TÜRK HAVAYOLLARI ANONİM ORTAKLIĞI");
			iDischargeList.Setup(m => m.GoodsLocationCode).Returns("G34000015");
			iDischargeList.Setup(m => m.RegistrationNo).Returns("20341453IM051515");
			iDischargeList.Setup(m => m.Bills).Returns(new IBillBL[] { iBill.Object, iBill2.Object });
			var dischargeListBuilder = new ETradeDischargeListMessageBuilder(iDischargeList.Object);

			var gpUserID = "18316465352";
			var currentDecryptedPassword = "14a4183baafab7925823897c99ce7b96";
			var applicationReference = "2020/00361/3";

			var testFileReader = new TestFileReader(GetType());
			var embeddedFolderPath = "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.ImportDischargeList";
			var expectedMessage = testFileReader.GetEmbeddedFileText(embeddedFolderPath, "RequestImportDischargeList.xml");
			AssertMultilineASCIIEquals(expectedMessage, dischargeListBuilder.GetMessageText(gpUserID, currentDecryptedPassword, applicationReference, iDischargeList.Object));
			iPack.VerifyAll();
			iBill.VerifyAll();
			iDischargeList.VerifyAll();
		}

		Mock<IBillBL> CreateBillMock(string lineNo, string billNo, string shipperName, string consigneeName, string consigneeTaxNo, ZDecimal grossWeight, ZDecimal netWeight, IPackBL[] packs)
		{
			var billMock = new Mock<IBillBL>();
			billMock.Setup(m => m.LineNo).Returns(lineNo);
			billMock.Setup(m => m.BillNo).Returns(billNo);
			billMock.Setup(m => m.ShipperName).Returns(shipperName);
			billMock.Setup(m => m.ConsigneeNameAndTitle).Returns(consigneeName);
			billMock.Setup(m => m.ConsigneeTaxNo).Returns(consigneeTaxNo);
			billMock.Setup(m => m.IsContainer).Returns(false);
			billMock.Setup(m => m.PackQuantity).Returns(1);
			billMock.Setup(m => m.MarksAndNumbers).Returns("KURYE");
			billMock.Setup(m => m.PackType).Returns("BI");
			billMock.Setup(m => m.GrossWeight).Returns(grossWeight);
			billMock.Setup(m => m.NetWeight).Returns(netWeight);
			billMock.Setup(m => m.Unit).Returns("KGM");
			billMock.Setup(m => m.SequenceNo).Returns("1");
			billMock.Setup(m => m.Packs).Returns(packs);
			return billMock;
		}
	}
}
