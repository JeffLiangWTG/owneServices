using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Argentina.Testing
{
	sealed class ArgentinaEInvoicingExtensionTest : TestCaseWithFactory
	{
		public void TestDocumentTypeForComplianceElegibleSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var argentinaComplianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCodes.Argentina);
				var subTypeCodeProvider = argentinaComplianceInfo as IComplianceSubTypeCodeProvider;
				var complianceEligibleSubTyp = argentinaComplianceInfo as IComplianceInfoElectronicInvoicingEligibleSubType;
				foreach (var subType in subTypeCodeProvider?.GetComplianceSubTypes())
				{
					var complianceSubType = subType.Code;

					if (complianceEligibleSubTyp.IsComplianceSubTypeElegibleForEInvoicing(complianceSubType))
					{
						AssertNotNull(ArgentinaEInvoicingExtension.GetDocumentType(complianceSubType));
					}
					else
					{
						AssertEquals(ZString.Empty, ArgentinaEInvoicingExtension.GetDocumentType(complianceSubType));
					}
				}

				AssertEquals("1", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA));
				AssertEquals("2", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDA));
				AssertEquals("3", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA));
				AssertEquals("6", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXB));
				AssertEquals("7", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDB));
				AssertEquals("8", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCB));
				AssertEquals("11", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXC));
				AssertEquals("12", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDC));
				AssertEquals("13", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCC));
				AssertEquals("19", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE));
				AssertEquals("20", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDE));
				AssertEquals("21", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCE));
				AssertEquals("51", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXM));
				AssertEquals("52", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDM));
				AssertEquals("53", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCM));
				AssertEquals("202", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA));
				AssertEquals("207", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB));
				AssertEquals("212", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC));
				AssertEquals("203", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA));
				AssertEquals("208", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB));
				AssertEquals("213", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC));
				AssertEquals("201", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA));
				AssertEquals("206", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXB));
				AssertEquals("211", ArgentinaEInvoicingExtension.GetDocumentType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXC));
			}
		}

		public void TestGetRegistrationNumberOrganization()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var mockITransactionQRCodeDataProvider = new Mock<ITransactionQRCodeDataProvider>();
			mockITransactionQRCodeDataProvider.Setup(x => x.OrgHeader).Returns(organization);

			var expectedRegType = ZString.Empty;
			var expectedRegValue = ZString.Empty;

			AssertCustomsCodes(expectedRegType, expectedRegValue);

			organization.MainAddress.OA_RN_NKCountryCode = CountryCodes.Argentina;
			AssertCustomsCodes(expectedRegType, expectedRegValue);

			organization.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI, "12.082.541");
			AssertCustomsCodes("96", "12082541");

			organization.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL, "27-10067418-2");
			AssertCustomsCodes("86", "27100674182");

			organization.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "30-12345678-9");
			AssertCustomsCodes("80", "30123456789");

			organization.CustomsCodes.RemoveAndDeleteAll();

			organization.MainAddress.OA_RN_NKCountryCode = CountryCodes.Mexico;
			AssertCustomsCodes(expectedRegType, expectedRegValue);

			organization.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC-12082541");
			AssertCustomsCodes("94", "12082541");

			organization.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "30-12345678-9");
			AssertCustomsCodes("80", "30123456789");

			organization.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF, "7072 73747");
			AssertCustomsCodes("80", "707273747");

			void AssertCustomsCodes(ZString expectedOrgCusCode, ZString expectedOrgCusNo)
			{
				(ZString actualRegType, ZString actualRegValue) = ArgentinaEInvoicingExtension.GetRegistrationNumberOrganization(mockITransactionQRCodeDataProvider.Object);
				AssertEquals(expectedOrgCusCode, actualRegType);
				AssertEquals(expectedOrgCusNo, actualRegValue);
			}
		}

		public void TestGetRegistrationNumberTransactionInfo()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			transactionInfo.OrganizationAddress = null;
			AssertCustomsCodes(ZString.Empty, ZString.Empty);

			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => null);
			AssertCustomsCodes(ZString.Empty, ZString.Empty);

			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			AssertCustomsCodes(ZString.Empty, ZString.Empty);

			transactionInfo.OrganizationAddress.Country = new Country() { Code = CountryCodes.Argentina };
			AddRegistrationNumber(CountryCodes.Mexico, MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "10000000098");
			AssertCustomsCodes(ZString.Empty, ZString.Empty);

			AddRegistrationNumber(CountryCodes.Argentina, ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI, "A-000000000 03");
			AssertCustomsCodes("96", "00000000003");

			AddRegistrationNumber(CountryCodes.Argentina, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL, "A-000000000 04");
			AssertCustomsCodes("86", "00000000004");

			AddRegistrationNumber(CountryCodes.Argentina, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "A-000000000 05");
			AssertCustomsCodes("80", "00000000005");

			transactionInfo.OrganizationAddress.RegistrationNumberCollection.Clear();

			transactionInfo.OrganizationAddress.Country.Code = CountryCodes.Japan;
			AddRegistrationNumber(CountryCodes.Japan, OrgCusCode.JapanCodeTypes.CON, "00000000004");
			AssertCustomsCodes(ZString.Empty, ZString.Empty);

			transactionInfo.OrganizationAddress.Country.Code = CountryCodes.Japan;
			AddRegistrationNumber(CountryCodes.Japan, OrgCusCode.CodeTypes.PassportID, "BB 000000000-03");
			AssertCustomsCodes("94", "00000000003");

			transactionInfo.OrganizationAddress.Country.Code = CountryCodes.Japan;
			AddRegistrationNumber(CountryCodes.Japan, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF, "BB 000000000-02");
			AssertCustomsCodes("80", "00000000002");

			void AddRegistrationNumber(ZString countryOfIssue, ZString orgCusCode, ZString orgCusValue)
			{
				transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
				transactionInfo.OrganizationAddress.RegistrationNumberCollection.Add(
								new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
								{
									CountryOfIssue = new Country() { Code = countryOfIssue },
									Type = new RegistrationNumberType() { Code = orgCusCode },
									Value = orgCusValue
								});
			}

			void AssertCustomsCodes(ZString expectedOrgCusCode, ZString expectedOrgCusNo)
			{
				(ZString actualRegType, ZString actualRegValue) = ArgentinaEInvoicingExtension.GetRegistrationNumberTransactionInfo(transactionInfo);
				AssertEquals(expectedOrgCusCode, actualRegType);
				AssertEquals(expectedOrgCusNo, actualRegValue);
			}
		}

		public void TestGetCurrencyAndExchangeRate()
		{
			var mockITransactionQRCodeDataProvider = new Mock<ITransactionQRCodeDataProvider>();

			mockITransactionQRCodeDataProvider.Setup(x => x.TransactionCurrency).Returns("XXX");
			mockITransactionQRCodeDataProvider.Setup(x => x.ExchangeRate).Returns(132M);
			AssertCurrencyAndExChangeRate(ZString.Empty, ZString.Empty);

			mockITransactionQRCodeDataProvider.Setup(x => x.ExchangeRate).Returns(1.00M);
			foreach (var currency in AFIPEquivalents.CurrencyEquivalentCodes.CurrencyCodes)
			{
				mockITransactionQRCodeDataProvider.Setup(x => x.TransactionCurrency).Returns(currency.Key);
				AssertCurrencyAndExChangeRate(currency.Value, currency.Key == CurrencyCodes.Argentina ? "1" : "1.000000");
			}

			void AssertCurrencyAndExChangeRate(ZString expectedMonedaId, ZString expectedMonendaCtz)
			{
				(ZString actualMonendaId, ZString actualMonedaCtz) = ArgentinaEInvoicingExtension.GetCurrencyAndExchangeRate(mockITransactionQRCodeDataProvider.Object);
				AssertEquals(expectedMonedaId, actualMonendaId);
				AssertEquals(expectedMonendaCtz, actualMonedaCtz);
			}
		}

		public void TestGetCurrencyAndExchangeRateTransactionInfo()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			AssertCurrencyAndExChangeRate(ZString.Empty, ZString.Empty);

			transaction.ExchangeRate = 15.00M;
			transaction.OSCurrency = new Currency() { Code = "XXX" };

			AssertCurrencyAndExChangeRate(ZString.Empty, ZString.Empty);

			transaction.ExchangeRate = 1.00M;
			foreach (var currency in AFIPEquivalents.CurrencyEquivalentCodes.CurrencyCodes)
			{
				transaction.OSCurrency.Code = currency.Key;
				AssertCurrencyAndExChangeRate(currency.Value, currency.Key == CurrencyCodes.Argentina ? "1" : "1.000000");
			}

			void AssertCurrencyAndExChangeRate(ZString expectedCurrencyId, ZString expectedCurrencyCtz)
			{
				(ZString actualMonendaId, ZString actualMonedaCtz) = ArgentinaEInvoicingExtension.GetCurrencyAndExchangeRateTransactionInfo(transaction);
				AssertEquals(expectedCurrencyId, actualMonendaId);
				AssertEquals(expectedCurrencyCtz, actualMonedaCtz);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ArgentinaEInvoicingExtension = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetArgentinaEInvoicingExtension();
		}

		IArgentinaEInvoicingExtension ArgentinaEInvoicingExtension;
	}
}
