using System;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Argentina.Testing
{
	sealed class ArgentinaQRCodeDataProviderTest : TestCaseWithFactory
	{
		public void TestGetQRCodeDataProvider_GovernmentAllocatedNumber()
		{
			var mockITransactionQRCodeDataProvider = new Mock<ITransactionQRCodeDataProvider>();

			AssertQRCodeData(ZString.Empty, 0L);

			AssertQRCodeData("70417054367476", 70417054367476);

			void AssertQRCodeData(ZString governmentAllocatedNumber, long expectedValue)
			{
				mockITransactionQRCodeDataProvider.Setup(x => x.EInvoicingGovernmentAllocatedNumber).Returns(governmentAllocatedNumber);

				var expectedQRCodeData = GetExpectedResult(string.Empty, 0L, 0, 0, 0, 0.0m, ZString.Empty, 0.0m, 0, 0L, expectedValue);

				var actualQRCodeData = ObjectFactory.Get<ICountryComplianceFactory>().GetIQRCodeDataProvider(CountryCodes.Argentina).GetTransactionQRCodeString(mockITransactionQRCodeDataProvider.Object);

				AssertEquals("GovernmentAllocatedNumber", expectedQRCodeData, actualQRCodeData);
			}
		}

		public void TestGetQRCodeDataProvider_CompanyCuit()
		{
			var company = TestData();

			var mockITransactionQRCodeDataProvider = new Mock<ITransactionQRCodeDataProvider>();

			mockITransactionQRCodeDataProvider.Setup(x => x.EInvoicingGovernmentAllocatedNumber).Returns("70417054367476");
			mockITransactionQRCodeDataProvider.Setup(x => x.Company).Returns(company);

			AssertQRCodeData(0L);

			company.GC_OH_OrgProxy = (company.GetNewOrgProxy(Factory)).PK;
			company.OrgProxy.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "30-99999999-1", CountryCodes.Argentina);

			AssertQRCodeData(30999999991);

			void AssertQRCodeData(long expectedCUIT)
			{
				var expectedQRCodeData = GetExpectedResult(string.Empty, expectedCUIT, 0, 0, 0, 0.0m, ZString.Empty, 0.0m, 0, 0L, 70417054367476);

				var actualQRCodeData = ObjectFactory.Get<ICountryComplianceFactory>().GetIQRCodeDataProvider(CountryCodes.Argentina).GetTransactionQRCodeString(mockITransactionQRCodeDataProvider.Object);

				AssertEquals("Company Cuit", expectedQRCodeData, actualQRCodeData);
			}
		}

		public void TestGetQRCodeDataProvider_PtoVta_NroCmp()
		{
			var mockITransactionQRCodeDataProvider = new Mock<ITransactionQRCodeDataProvider>();
			mockITransactionQRCodeDataProvider.Setup(x => x.EInvoicingGovernmentAllocatedNumber).Returns("70417054367476");

			AssertQRCodeData(ZString.Empty, 0, 0);

			AssertQRCodeData("0001000000094", 10, 94);

			AssertQRCodeData("194", 194, 0);

			AssertQRCodeData("123456", 12345, 6);

			AssertQRCodeData("A-12345B6C789011111", 12345, 6789011111);

			void AssertQRCodeData(ZString transactionReference, int expectedPtoVta, long expectedNroCmp)
			{
				mockITransactionQRCodeDataProvider.Setup(x => x.TransactionReference).Returns(transactionReference);

				var expectedQRCodeData = GetExpectedResult(string.Empty, 0L, expectedPtoVta, 0, expectedNroCmp, 0.0m, ZString.Empty, 0.0m, 0, 0L, 70417054367476);

				var actualQRCodeData = ObjectFactory.Get<ICountryComplianceFactory>().GetIQRCodeDataProvider(CountryCodes.Argentina).GetTransactionQRCodeString(mockITransactionQRCodeDataProvider.Object);

				AssertEquals("PtoVta & NroCmp", expectedQRCodeData, actualQRCodeData);
			}
		}

		public void TestGetQRCodeDataProvider_Amount()
		{
			var mockITransactionQRCodeDataProvider = new Mock<ITransactionQRCodeDataProvider>();
			mockITransactionQRCodeDataProvider.Setup(x => x.EInvoicingGovernmentAllocatedNumber).Returns("70417054367476");

			AssertQRCodeData(TransactionTypes.Invoice, 1120.66m, 1120.66m);

			AssertQRCodeData(TransactionTypes.CreditNote, -1120.66m, 1120.66m);

			void AssertQRCodeData(ZString transactionType, ZDecimal osTotal, ZDecimal expectedAmount)
			{
				mockITransactionQRCodeDataProvider.Setup(x => x.TransactionType).Returns(transactionType);
				mockITransactionQRCodeDataProvider.Setup(x => x.OSInvoiceTotal).Returns(osTotal);

				var expectedQRCodeData = GetExpectedResult(string.Empty, 0L, 0, 0, 0, expectedAmount, ZString.Empty, 0.0m, 0, 0L, 70417054367476);

				var actualQRCodeData = ObjectFactory.Get<ICountryComplianceFactory>().GetIQRCodeDataProvider(CountryCodes.Argentina).GetTransactionQRCodeString(mockITransactionQRCodeDataProvider.Object);

				AssertEquals("Amount", expectedQRCodeData, actualQRCodeData);
			}
		}

		public void TestGetQRCodeDataProvider_InvoiceDate()
		{
			var mockITransactionQRCodeDataProvider = new Mock<ITransactionQRCodeDataProvider>();

			mockITransactionQRCodeDataProvider.Setup(x => x.EInvoicingGovernmentAllocatedNumber).Returns("70417054367476");
			mockITransactionQRCodeDataProvider.Setup(x => x.InvoiceDate).Returns(new ZDateTime(2021, 04, 26));

			var expectedQRCodeData = GetExpectedResult("2021-04-26", 0L, 0, 0, 0, 0.0m, ZString.Empty, 0.0m, 0, 0L, 70417054367476);

			var actualQRCodeData = ObjectFactory.Get<ICountryComplianceFactory>().GetIQRCodeDataProvider(CountryCodes.Argentina).GetTransactionQRCodeString(mockITransactionQRCodeDataProvider.Object);

			AssertEquals("Invoice Date", expectedQRCodeData, actualQRCodeData);
		}

		public void TestGetQRCodeDataProvider_TipoCmp()
		{
			var mockITransactionQRCodeDataProvider = new Mock<ITransactionQRCodeDataProvider>();
			var mockIArgentinaEInvoicingExtension = new Mock<IArgentinaEInvoicingExtension>();
			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();

			mockITransactionQRCodeDataProvider.Setup(x => x.EInvoicingGovernmentAllocatedNumber).Returns("70417054367476");

			AssertQRCodeData_TipoCmp(ZString.Empty, 0);

			AssertQRCodeData_TipoCmp("1", 1);

			void AssertQRCodeData_TipoCmp(ZString documentType, int expectedTipoCmp)
			{
				mockIArgentinaEInvoicingExtension.Reset();

				mockIArgentinaEInvoicingExtension.Setup(x => x.GetDocumentType(It.IsAny<ZString>())).Returns(documentType);
				mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetArgentinaEInvoicingExtension()).Returns(mockIArgentinaEInvoicingExtension.Object);

				using (ObjectFactory.Substitute(mockIAccountingMasterFilesDependencyFactory.Object))
				{
					var expectedQRCodeData = GetExpectedResult(string.Empty, 0L, 0, expectedTipoCmp, 0, 0.0m, ZString.Empty, 0.0m, 0, 0L, 70417054367476);

					var actualQRCodeData = ObjectFactory.Get<ICountryComplianceFactory>().GetIQRCodeDataProvider(CountryCodes.Argentina).GetTransactionQRCodeString(mockITransactionQRCodeDataProvider.Object);

					AssertEquals("TipoCmp", expectedQRCodeData, actualQRCodeData);

					mockIArgentinaEInvoicingExtension.Verify(x => x.GetDocumentType(mockITransactionQRCodeDataProvider.Object.ComplianceSubType), Times.Once);
				}
			}
		}

		public void TestGetQRCodeDataProvider_CurrencyAndExChangeRate()
		{
			var mockIArgentinaEInvoicingExtension = new Mock<IArgentinaEInvoicingExtension>();
			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();

			var mockITransactionQRCodeDataProvider = new Mock<ITransactionQRCodeDataProvider>();
			mockITransactionQRCodeDataProvider.Setup(x => x.EInvoicingGovernmentAllocatedNumber).Returns("70417054367476");

			AssertQRCodeData_CurrencyAndExChangeRate("ABC", "65.000000", "ABC", 65.000000m);

			AssertQRCodeData_CurrencyAndExChangeRate("DEF", "1.0", "DEF", 1.0m);

			AssertQRCodeData_CurrencyAndExChangeRate("XXX", ZString.Empty, "XXX", 0.0m);

			void AssertQRCodeData_CurrencyAndExChangeRate(ZString currency, ZString exChangeRate, ZString expectedCurrency, ZDecimal expectedExChangeRate)
			{
				mockIArgentinaEInvoicingExtension.Reset();

				mockIArgentinaEInvoicingExtension.Setup(x => x.GetCurrencyAndExchangeRate(It.IsAny<ITransactionQRCodeDataProvider>())).Returns((currency, exChangeRate));
				mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetArgentinaEInvoicingExtension()).Returns(mockIArgentinaEInvoicingExtension.Object);

				using (ObjectFactory.Substitute(mockIAccountingMasterFilesDependencyFactory.Object))
				{
					var expectedQRCodeData = GetExpectedResult(string.Empty, 0L, 0, 0, 0, 0.0m, expectedCurrency, expectedExChangeRate, 0, 0L, 70417054367476);

					var actualQRCodeData = ObjectFactory.Get<ICountryComplianceFactory>().GetIQRCodeDataProvider(CountryCodes.Argentina).GetTransactionQRCodeString(mockITransactionQRCodeDataProvider.Object);

					AssertEquals("Currency And ExChangeRate", expectedQRCodeData, actualQRCodeData);

					mockIArgentinaEInvoicingExtension.Verify(x => x.GetCurrencyAndExchangeRate(mockITransactionQRCodeDataProvider.Object), Times.Once);
				}
			}
		}

		public void TestGetQRCodeDataProvider_RegistrationNumberOrganization()
		{
			var mockIArgentinaEInvoicingExtension = new Mock<IArgentinaEInvoicingExtension>();
			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();

			var mockITransactionQRCodeDataProvider = new Mock<ITransactionQRCodeDataProvider>();
			mockITransactionQRCodeDataProvider.Setup(x => x.EInvoicingGovernmentAllocatedNumber).Returns("70417054367476");

			AssertQRCodeData_RegistrationNumberOrganization("80", "30123456789", 80, 30123456789);

			AssertQRCodeData_RegistrationNumberOrganization("XXX", ZString.Empty, 0, 0);

			void AssertQRCodeData_RegistrationNumberOrganization(ZString tipoDocRec, ZString nroDocRec, int expectedTipoDocRec, long expectedNroDocRec)
			{
				mockIArgentinaEInvoicingExtension.Reset();

				mockIArgentinaEInvoicingExtension.Setup(x => x.GetRegistrationNumberOrganization((It.IsAny<ITransactionQRCodeDataProvider>()))).Returns((tipoDocRec, nroDocRec));
				mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetArgentinaEInvoicingExtension()).Returns(mockIArgentinaEInvoicingExtension.Object);

				using (ObjectFactory.Substitute(mockIAccountingMasterFilesDependencyFactory.Object))
				{
					var expectedQRCodeData = GetExpectedResult(string.Empty, 0L, 0, 0, 0, 0.0m, ZString.Empty, 0.0m, expectedTipoDocRec, expectedNroDocRec, 70417054367476);

					var actualQRCodeData = ObjectFactory.Get<ICountryComplianceFactory>().GetIQRCodeDataProvider(CountryCodes.Argentina).GetTransactionQRCodeString(mockITransactionQRCodeDataProvider.Object);

					AssertEquals("Registration Number Organization", expectedQRCodeData, actualQRCodeData);

					mockIArgentinaEInvoicingExtension.Verify(x => x.GetRegistrationNumberOrganization(mockITransactionQRCodeDataProvider.Object), Times.Once);
				}
			}
		}

		ZString GetExpectedResult(ZString fecha, long cuit, int ptoVta, int tipoCmp, long nroCmp, decimal amount, ZString currency, decimal exchangeRate, int tipoDocRec, long nroDocRec, long governmentAllocatedNumber)
		{
			var expectedResult = ZString.Empty;

			if (governmentAllocatedNumber != 0)
			{
				var inputText = "{" + $"\"ver\":1,\"fecha\":\"{fecha}\",\"cuit\":{cuit},\"ptoVta\":{ptoVta},\"tipoCmp\":{tipoCmp},\"nroCmp\":{nroCmp},\"importe\":{amount},\"moneda\":\"{currency}\",\"ctz\":{exchangeRate},\"tipoDocRec\":{tipoDocRec},\"nroDocRec\":{nroDocRec},\"tipoCodAut\":\"E\",\"codAut\":{governmentAllocatedNumber}" + "}";

				expectedResult = "https://www.afip.gob.ar/fe/qr/?p=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(inputText));
			}
			return expectedResult;
		}

		GlbCompany TestData()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.Argentina;

			return company;
		}
	}
}
