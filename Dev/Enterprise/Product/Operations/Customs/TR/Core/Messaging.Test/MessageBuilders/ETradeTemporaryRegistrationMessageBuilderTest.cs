using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Messaging.Test
{
	sealed class ETradeTemporaryRegistrationMessageBuilderTest : TestCase
	{
		public void TestRequestExportMessage()
		{
			var messageType = "EX";
			var gpUserID = "20201224104";
			var currentDecryptedPassword = "12345678";
			var applicationReference = "ULU-MAN0000442|20201224104|1";
			var pack = CreatePack(messageType);
			var bill = CreateBill(pack.Object, null, messageType);
			var eTradeTemporaryRegistration = CreateETradeTemporaryRegistration(bill.Object, messageType);
			var temporaryRegistrationBuilder = new ETradeTemporaryRegistrationMessageBuilder();

			var testFileReader = new TestFileReader(GetType());
			var expectedMessage = testFileReader.GetEmbeddedFileText(EmbeddedFolderPath, "RequestExportMessage.xml");
			AssertMultilineASCIIEquals(expectedMessage, temporaryRegistrationBuilder.GetMessageText(gpUserID, currentDecryptedPassword, applicationReference, eTradeTemporaryRegistration.Object));
			pack.VerifyAll();
			bill.VerifyAll();
			eTradeTemporaryRegistration.VerifyAll();
		}

		public void TestRequestImportMessage()
		{
			var messageType = "IM";
			var gpUserID = "20201224104";
			var currentDecryptedPassword = "12345678";
			var applicationReference = "ULU-MAN0000442|20201224104|2";
			var tax = CreateTax(messageType);
			var pack = CreatePack(messageType);
			var bill = CreateBill(pack.Object, tax.Object, messageType);
			var eTradeTemporaryRegistration = CreateETradeTemporaryRegistration(bill.Object, messageType);
			var temporaryRegistrationBuilder = new ETradeTemporaryRegistrationMessageBuilder();

			var testFileReader = new TestFileReader(GetType());
			var expectedMessage = testFileReader.GetEmbeddedFileText(EmbeddedFolderPath, "RequestImportMessage.xml");
			AssertMultilineASCIIEquals(expectedMessage, temporaryRegistrationBuilder.GetMessageText(gpUserID, currentDecryptedPassword, applicationReference, eTradeTemporaryRegistration.Object));
			pack.VerifyAll();
			bill.VerifyAll();
			eTradeTemporaryRegistration.VerifyAll();
		}

		Mock<IETradeTemporaryRegistration> CreateETradeTemporaryRegistration(IBill bill, string messageType)
		{
			var eTradeTemporaryRegistrationMock = new Mock<IETradeTemporaryRegistration>();
			eTradeTemporaryRegistrationMock.Setup(m => m.ReferenceNoToUpdate).Returns(ZString.Empty);
			eTradeTemporaryRegistrationMock.Setup(m => m.Declaration1).Returns(messageType);
			eTradeTemporaryRegistrationMock.Setup(m => m.Declaration2).Returns(messageType == "EX" ? "1" : "4");
			eTradeTemporaryRegistrationMock.Setup(m => m.Declaration3).Returns(messageType == "EX" ? "ETD" : "");
			eTradeTemporaryRegistrationMock.Setup(m => m.TotalLineCount).Returns(1);
			eTradeTemporaryRegistrationMock.Setup(m => m.TotalBoxQty).Returns(1);
			eTradeTemporaryRegistrationMock.Setup(m => m.ReferenceNo).Returns(ZString.Empty);
			eTradeTemporaryRegistrationMock.Setup(m => m.DeclaringRepresentativeNameAndTitle).Returns("BEYAN SAHIBI ADI");
			eTradeTemporaryRegistrationMock.Setup(m => m.DeclaringRepresentativeTCTaxNo).Returns("1111111111");
			eTradeTemporaryRegistrationMock.Setup(m => m.TypeOfVehicleOnExit).Returns("5");
			eTradeTemporaryRegistrationMock.Setup(m => m.VehiclePlateOfVehicleOnExit).Returns("TK13245");
			eTradeTemporaryRegistrationMock.Setup(m => m.CountryCodeOfVehicleOnExit).Returns("052");
			eTradeTemporaryRegistrationMock.Setup(m => m.IsContainer).Returns(messageType == "EX" ? ZBool.False : ZBool.True);
			eTradeTemporaryRegistrationMock.Setup(m => m.TypeOfVehicleOnBorder).Returns("5");
			eTradeTemporaryRegistrationMock.Setup(m => m.VehiclePlateOfVehicleOnBorder).Returns("TK13245");
			eTradeTemporaryRegistrationMock.Setup(m => m.CountryCodeOfVehicleOnBorder).Returns("052");
			eTradeTemporaryRegistrationMock.Setup(m => m.TotalInvoiceCurrencyCode).Returns("EUR");
			eTradeTemporaryRegistrationMock.Setup(m => m.TotalInvoiceCurrencyValue).Returns(900.00m);
			eTradeTemporaryRegistrationMock.Setup(m => m.TotalInvoiceExchangeRate).Returns(9.000000m);
			eTradeTemporaryRegistrationMock.Setup(m => m.InvoiceAmountInformationTurkishLira).Returns(messageType == "EX" ? 8100.00m : 100.00m);
			eTradeTemporaryRegistrationMock.Setup(m => m.StatisticalValue).Returns(ZDecimal.Zero);
			if (messageType == "IM")
			{
				eTradeTemporaryRegistrationMock.Setup(m => m.TotalExpensesFreightCurrencyCode).Returns("USD");
				eTradeTemporaryRegistrationMock.Setup(m => m.TotalExpensesFreightCurrencyValue).Returns(50.00m);
			}
			eTradeTemporaryRegistrationMock.Setup(m => m.TotalExpensesInsuranceCurrencyValue).Returns(ZDecimal.Zero);
			eTradeTemporaryRegistrationMock.Setup(m => m.OtherOverseasExpenditureCurrencyValue).Returns(ZDecimal.Zero);
			eTradeTemporaryRegistrationMock.Setup(m => m.DomesticExpenditures).Returns(ZDecimal.Zero);
			eTradeTemporaryRegistrationMock.Setup(m => m.TransportTypeCode).Returns("40");
			eTradeTemporaryRegistrationMock.Setup(m => m.LoadingUnloadingPlace).Returns("067777");
			eTradeTemporaryRegistrationMock.Setup(m => m.CustomsOfficeCodeOfEntryExit).Returns("067777");
			eTradeTemporaryRegistrationMock.Setup(m => m.GoodsLocationCode).Returns("G34000015");
			eTradeTemporaryRegistrationMock.Setup(m => m.GoodsLocationName).Returns("DEPOLAMA YERI");
			eTradeTemporaryRegistrationMock.Setup(m => m.Adjustment).Returns(ZString.Empty);
			eTradeTemporaryRegistrationMock.Setup(m => m.WarehouseTypeCode).Returns(messageType == "EX" ? ZString.Empty : "G34000015");
			eTradeTemporaryRegistrationMock.Setup(m => m.PrincipleResponsibleNameAndTitle).Returns("SORUMLU");
			eTradeTemporaryRegistrationMock.Setup(m => m.PrincipleResponsibleTCTaxNo).Returns("3310519833");
			eTradeTemporaryRegistrationMock.Setup(m => m.CustomsOfficeCodeOfPredicted).Returns("067777");
			eTradeTemporaryRegistrationMock.Setup(m => m.CountryCodeOfPredicted).Returns(messageType == "EX" ? ZString.Empty : "052");
			eTradeTemporaryRegistrationMock.Setup(m => m.TotalGuaranteesValue).Returns(ZDecimal.Zero);
			eTradeTemporaryRegistrationMock.Setup(m => m.CustomsOfficeCodeOfDestination).Returns("067777");
			eTradeTemporaryRegistrationMock.Setup(m => m.TransfersCountryCode).Returns(messageType == "EX" ? ZString.Empty : "016");
			eTradeTemporaryRegistrationMock.Setup(m => m.Explanations).Returns(ZString.Empty);
			eTradeTemporaryRegistrationMock.Setup(m => m.Bills).Returns(new IBill[] { bill });
			return eTradeTemporaryRegistrationMock;
		}

		Mock<IBill> CreateBill(IPack pack, ITax tax, string messageType)
		{
			var billMock = new Mock<IBill>();
			billMock.Setup(m => m.BillOfLadingLineNo).Returns("1");
			billMock.Setup(m => m.BillOfLadingNumber).Returns("TASSENNO");
			billMock.Setup(m => m.PackType).Returns("BI");
			billMock.Setup(m => m.PackQuantity).Returns(1);
			if (messageType == "IM")
			{
				billMock.Setup(m => m.ContainerNo).Returns("111111111111");
				billMock.Setup(m => m.ContainerBrand).Returns("ADR");
				billMock.Setup(m => m.ContainerPackType).Returns("BI");
				billMock.Setup(m => m.ContainerPackQuantity).Returns(1);
			}
			else
			{
				billMock.Setup(m => m.ContainerNo).Returns(ZString.Empty);
			}
			billMock.Setup(m => m.GrossWeight).Returns(100);
			billMock.Setup(m => m.NetWeight).Returns(90);
			billMock.Setup(m => m.ForwarderNameAndTitle).Returns("GONDERICI UNVANI");
			billMock.Setup(m => m.ForwarderTCTaxNo).Returns("8890024379");
			billMock.Setup(m => m.ConsigneeName).Returns("ALICI FIRMA");
			billMock.Setup(m => m.ConsigneeTitle).Returns(ZString.Empty);
			billMock.Setup(m => m.ConsigneeTCTaxNo).Returns(ZString.Empty);
			billMock.Setup(m => m.ConsigneeStreetNumber).Returns(ZString.Empty);
			billMock.Setup(m => m.ConsigneeCityCode).Returns("IL");
			billMock.Setup(m => m.ConsigneeTown).Returns("ILCE");
			billMock.Setup(m => m.ConsigneePostalCode).Returns("34000");
			billMock.Setup(m => m.MarketPlaceNameAndTitle).Returns("MARKET");
			billMock.Setup(m => m.MarketPlaceTCTaxNo).Returns("8890024377");
			billMock.Setup(m => m.ReferralDestinationCountryCode).Returns(messageType == "EX" ? "052" : "004");
			billMock.Setup(m => m.TradeCountryCode).Returns(messageType == "EX" ? "004" : ZString.Empty);
			billMock.Setup(m => m.ExportCountryCode).Returns(messageType == "EX" ? "052" : "004");
			billMock.Setup(m => m.DestinationCountryCode).Returns(messageType == "EX" ? "004" : "052");
			billMock.Setup(m => m.DeliveryMethod).Returns(ZString.Empty);
			billMock.Setup(m => m.DeliverLocation).Returns("G34000020");
			billMock.Setup(m => m.TransactionNature).Returns(ZString.Empty);
			billMock.Setup(m => m.ExceptionCode1).Returns(messageType == "EX" ?	"DIPL" : "DOK");
			billMock.Setup(m => m.ExceptionCode2).Returns(ZString.Empty);
			billMock.Setup(m => m.RegimeCode).Returns(messageType == "EX" ? "1000" : "4000");
			billMock.Setup(m => m.Documents).Returns(new List<IDocument> { null });
			billMock.Setup(m => m.FinancialBankingAmount).Returns(ZDecimal.Zero);
			billMock.Setup(m => m.InvoiceAmount).Returns(900.00m);
			billMock.Setup(m => m.InvoiceCurrencyCode).Returns("EUR");
			billMock.Setup(m => m.InvoiceExchangeRate).Returns(9.000000m);
			if (messageType == "EX")
			{
				billMock.Setup(m => m.Taxes).Returns(new List<ITax> { null });
			}
			else
			{
				billMock.Setup(m => m.Taxes).Returns(new ITax[] { tax });
			}
			billMock.Setup(m => m.TaxPaymentTotalTaxPaymentAmount).Returns(119.00m);
			billMock.Setup(m => m.TaxPaymentTaxAmountToBeBonded).Returns(ZDecimal.Zero);
			billMock.Setup(m => m.TaxPaymentTotalTaxAmountPayableLater).Returns(ZDecimal.Zero);
			billMock.Setup(m => m.TaxPaymentTotal).Returns(119.00m);
			billMock.Setup(m => m.GuaranteeAmount).Returns(ZDecimal.Zero);
			billMock.Setup(m => m.FreightInformationCurrencyCode).Returns(messageType == "EX" ? ZString.Empty : "USD");
			billMock.Setup(m => m.FreightInformationValue).Returns(messageType == "EX" ? ZDecimal.Zero : 10.00M);
			billMock.Setup(m => m.InsuranceInformationCurrencyCode).Returns(ZString.Empty);
			billMock.Setup(m => m.InsuranceInformationValue).Returns(ZDecimal.Zero);
			billMock.Setup(m => m.OtherOverseasExpansesCurrencyCode).Returns(ZString.Empty);
			billMock.Setup(m => m.OtherOverseasExpansesValue).Returns(ZDecimal.Zero);
			billMock.Setup(m => m.DomesticExpanses).Returns(ZDecimal.Zero);
			billMock.Setup(m => m.Packs).Returns(new IPack[] { pack });
			billMock.Setup(m => m.Volume).Returns(messageType == "EX" ? "0" : "1");
			billMock.Setup(m => m.TradeType).Returns("ET");
			return billMock;
		}

		Mock<IPack> CreatePack(string messageType)
		{
			var packMock = new Mock<IPack>();
			packMock.Setup(m => m.PackNo).Returns("1");
			packMock.Setup(m => m.CommercialDescription).Returns("DOKUMAN");
			packMock.Setup(m => m.ItemsSerialNo).Returns("KURYE");
			packMock.Setup(m => m.ItemsQuantity).Returns(1);
			packMock.Setup(m => m.ItemsBrand).Returns(messageType == "EX" ? "MARKASI" : "ETGB");
			packMock.Setup(m => m.ItemsModel).Returns(messageType == "EX" ? "MODELI" : "ETGB");
			packMock.Setup(m => m.UsedItemCode).Returns(ZString.Empty);
			packMock.Setup(m => m.ItemCode1).Returns(messageType == "EX" ? "61091000" : ZString.Empty);
			packMock.Setup(m => m.ItemCode2).Returns(messageType == "EX" ? "00" : ZString.Empty);
			packMock.Setup(m => m.ItemCode3).Returns(messageType == "EX" ? "00" : ZString.Empty);
			packMock.Setup(m => m.PreferentialTariffCode1).Returns(ZString.Empty);
			packMock.Setup(m => m.PreferentialTariffCode2).Returns(ZString.Empty);
			packMock.Setup(m => m.CountryCodeOfOrigin).Returns("075");
			packMock.Setup(m => m.ValueStatementForm).Returns(ZString.Empty);
			packMock.Setup(m => m.AgriculturePolicy).Returns(ZString.Empty);
			packMock.Setup(m => m.IsQuota).Returns(ZBool.False);
			packMock.Setup(m => m.SupplementaryMeasuresType1).Returns("C62");
			packMock.Setup(m => m.SupplementaryMeasuresQuantity1).Returns(100.00m);
			packMock.Setup(m => m.SupplementaryMeasuresType2).Returns(ZString.Empty);
			packMock.Setup(m => m.BillAmountCurrencyCode).Returns("EUR");
			packMock.Setup(m => m.BillAmountValue).Returns(900.00m);
			packMock.Setup(m => m.CalculationMethod).Returns(ZString.Empty);
			packMock.Setup(m => m.StatisticalValue).Returns(1000.00m);
			return packMock;
		}

		Mock<ITax> CreateTax(string messageType)
		{
			var taxMock = new Mock<ITax>();
			taxMock.Setup(m => m.Code).Returns("89");
			taxMock.Setup(m => m.Description).Returns("Damga Vergisi");
			taxMock.Setup(m => m.Base).Returns(ZDecimal.Zero);
			taxMock.Setup(m => m.Rate).Returns(ZDecimal.Zero);
			taxMock.Setup(m => m.Amount).Returns(119.00m);
			taxMock.Setup(m => m.PaymentType).Returns("P");
			return taxMock;
		}

		const string EmbeddedFolderPath = "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.TemporaryRegistration";
	}
}
