using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class DeclarationProviderHelperTest : TestCaseWithDummy
	{
		public void TestGetCustomsRatioCode()
		{
			var isExport = false;
			var taxCode = string.Empty;
			var list = new CodeDescriptionPairList();

			taxCode = DeclarationProviderHelper.GetCustomsRatioCode(isExport, list, "MUAF");
			AssertEquals("List Empty", ZString.Empty, taxCode);

			list.AddPair("MUAF", "VAT 0%");
			taxCode = DeclarationProviderHelper.GetCustomsRatioCode(isExport, list, "MUAF");
			AssertEquals("List Only 1", ZString.Empty, taxCode);

			list.AddPair("KD1", "VAT 1%");
			list.AddPair("KD10", "VAT 10%");
			list.AddPair("KD20", "VAT 20%");

			CombineAssertions("VAT Ratio", () =>
			{
				taxCode = DeclarationProviderHelper.GetCustomsRatioCode(isExport, list, "MUAF");
				AssertEquals("Tax Code MUAF", "KDV0", taxCode);

				taxCode = DeclarationProviderHelper.GetCustomsRatioCode(isExport, list, "KD1");
				AssertEquals("Tax Code KD1", "KDV1", taxCode);

				taxCode = DeclarationProviderHelper.GetCustomsRatioCode(isExport, list, "KD10");
				AssertEquals("Tax Code KD10", "KDV10", taxCode);

				taxCode = DeclarationProviderHelper.GetCustomsRatioCode(isExport, list, "KD20");
				AssertEquals("Tax Code KD20", "KDV20", taxCode);

				isExport = true;
				taxCode = DeclarationProviderHelper.GetCustomsRatioCode(isExport, list, "KD1");
				AssertEquals("Export", ZString.Empty, taxCode);
			});
		}

		public void TestGetContainerCount()
		{
			var container1 = invoiceHeader.JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "11111";
			container1.CO_ContainerSize = "40";
			container1.CO_Weight = 7.8m;
			container1.CO_WeightUQ = "T";
			container1.CO_RN_NKOwnerCountry = "TR";

			var container2 = invoiceHeader.JobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "22222";
			container2.CO_ContainerSize = "40";
			container2.CO_Weight = 7.8m;
			container2.CO_WeightUQ = "T";
			container2.CO_RN_NKOwnerCountry = "TR";

			CombineAssertions("Currency Code", () =>
			{
				var countainerCount = DeclarationProviderHelper.GetContainerCount(cusEntryLine);
				AssertEquals("Zero Count", ZDecimal.Zero, countainerCount);

				invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

				countainerCount = DeclarationProviderHelper.GetContainerCount(cusEntryLine);
				AssertEquals("2 Count", 2m, countainerCount);
			});
		}

		#region Charge Methods

		public void TestGetDefaultCurrencyCode()
		{
			CombineAssertions("Invoice Hader", () =>
			{
				var currencyCode = DeclarationProviderHelper.GetDefaultCurrencyCode(invoiceHeader, "COM");
				AssertEquals("Code Empty", ZString.Empty, currencyCode);

				var invoiceHeaderCharge = invoiceHeader.Charges.AddNew();
				invoiceHeaderCharge.J7_ChargeType = "COM";
				invoiceHeaderCharge.J7_Amount = 10m;
				invoiceHeaderCharge.J7_RX_NKCurrency = "USD";
				currencyCode = DeclarationProviderHelper.GetDefaultCurrencyCode(invoiceHeader, "COM");
				AssertEquals("Default Code", "USD", currencyCode);
			});

			CombineAssertions("Invoice Line", () =>
			{
				var currencyCode = DeclarationProviderHelper.GetDefaultCurrencyCode(cusEntryLine, "COM");
				AssertEquals("Code Empty", ZString.Empty, currencyCode);

				var invoiceLineCharge = invoiceLine.Charges.AddNew();
				invoiceLineCharge.J7_ChargeType = "COM";
				invoiceLineCharge.J7_Amount = 10m;
				invoiceLineCharge.J7_RX_NKCurrency = "USD";
				currencyCode = DeclarationProviderHelper.GetDefaultCurrencyCode(cusEntryLine, "COM");
				AssertEquals("Default Code", "USD", currencyCode);
			});
		}

		public void TestGetChargesTotal()
		{
			CombineAssertions("Invoice Header", () =>
			{
				var totalValue = DeclarationProviderHelper.GetChargesTotal(invoiceHeader, "COM");
				AssertEquals("Zero Value", ZDecimal.Zero, totalValue);

				var invoiceHeaderCharge = invoiceHeader.Charges.AddNew();
				invoiceHeaderCharge.J7_ChargeType = "COM";
				invoiceHeaderCharge.J7_Amount = 10m;
				invoiceHeaderCharge.J7_RX_NKCurrency = "USD";

				totalValue = DeclarationProviderHelper.GetChargesTotal(invoiceHeader, "COM");
				AssertEquals("USD Value", 10m, totalValue);
			});

			CombineAssertions("Invoice Line", () =>
			{
				var totalValue = DeclarationProviderHelper.GetChargesTotal(cusEntryLine, false, "COM");
				AssertEquals("Zero Value", ZDecimal.Zero, totalValue);

				var invoiceLineCharge = invoiceLine.Charges.AddNew();
				invoiceLineCharge.J7_ChargeType = "COM";
				invoiceLineCharge.J7_Amount = 10m;
				invoiceLineCharge.J7_RX_NKCurrency = "USD";

				totalValue = DeclarationProviderHelper.GetChargesTotal(cusEntryLine, false, "COM");
				AssertEquals("USD Value", 10m, totalValue);

				using (Env.SetTemporaryUserContext(new UserContext(loggedInUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					totalValue = DeclarationProviderHelper.GetChargesTotal(cusEntryLine, true, "COM");
					AssertEquals("TRY Value", 270m, totalValue);
				}
			});
		}

		public void TestFindFromValidCharges()
		{
			var chargeTypes = new ZString[] { "COM", "DEM" };
			CombineAssertions("Invoice Header", () =>
			{
				var validCharges = DeclarationProviderHelper.FindFromValidCharges(invoiceHeader, chargeTypes);
				AssertEquals("Zero Charge", ZInt.Zero, validCharges.Length);

				var chargeCOM = invoiceHeader.Charges.AddNew();
				chargeCOM.J7_ChargeType = "COM";
				chargeCOM.J7_Amount = 10m;
				chargeCOM.J7_RX_NKCurrency = "USD";

				validCharges = DeclarationProviderHelper.FindFromValidCharges(invoiceHeader, chargeTypes);
				AssertEquals("1 charges", 1, validCharges.Length);
				AssertEquals("(1) J7_Amount", 10m, validCharges[0].J7_Amount);
				AssertEquals("(1) J7_RX_NKCurrency", "USD", validCharges[0].J7_RX_NKCurrency);
			});

			CombineAssertions("Invoice Line", () =>
			{
				var validCharges = DeclarationProviderHelper.FindFromValidCharges(cusEntryLine, chargeTypes);
				AssertEquals("Zero Charge", ZInt.Zero, validCharges.Length);

				var chargeCOM = invoiceLine.Charges.AddNew();
				chargeCOM.J7_ChargeType = "COM";
				chargeCOM.J7_Amount = 10m;
				chargeCOM.J7_RX_NKCurrency = "USD";

				var apportionedChargeDEM = invoiceLine.ApportionedCharges.AddNew();
				apportionedChargeDEM.J7_ChargeType = "DEM";
				apportionedChargeDEM.J7_Amount = 20m;
				apportionedChargeDEM.J7_RX_NKCurrency = "USD";

				validCharges = DeclarationProviderHelper.FindFromValidCharges(cusEntryLine, chargeTypes);
				AssertEquals("2 charges", 2, validCharges.Length);
				AssertEquals("(1) J7_Amount", 10m, validCharges[0].J7_Amount);
				AssertEquals("(1) J7_RX_NKCurrency", "USD", validCharges[0].J7_RX_NKCurrency);
				AssertEquals("(2) J7_Amount", 20m, validCharges[1].J7_Amount);
				AssertEquals("(2) J7_RX_NKCurrency", "USD", validCharges[1].J7_RX_NKCurrency);
			});
		}

		public void TestGetChargesExplanation()
		{
			CombineAssertions(() =>
			{
				var explanation = DeclarationProviderHelper.GetChargesExplanation(cusEntryLine, "LOT");
				AssertEquals("Empty Explanation", ZString.Empty, explanation);

				var chargeLOT = invoiceLine.Charges.AddNew();
				chargeLOT.J7_ChargeType = "LOT";
				chargeLOT.J7_Amount = 10m;
				chargeLOT.J7_RX_NKCurrency = "USD";
				chargeLOT.Explanation = "Charge Exp 1";

				explanation = DeclarationProviderHelper.GetChargesExplanation(cusEntryLine, "LOT");
				AssertEquals("Explanation", "Charge Exp 1", explanation);

				var chargeOTH = invoiceLine.Charges.AddNew();
				chargeOTH.J7_ChargeType = "OTH";
				chargeOTH.J7_Amount = 10m;
				chargeOTH.J7_RX_NKCurrency = "USD";
				chargeOTH.Explanation = "Charge Exp 2";

				explanation = DeclarationProviderHelper.GetChargesExplanation(cusEntryLine, "OTH");
				AssertEquals("Explanation", "Charge Exp 2", explanation);
			});
		}

		#endregion

		public void TestCheck8ThousandCodes()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZBool.False, DeclarationProviderHelper.Check8ThousandCodes(""));
				AssertEquals("4000", ZBool.False, DeclarationProviderHelper.Check8ThousandCodes("4000"));
				AssertEquals("8000", ZBool.True, DeclarationProviderHelper.Check8ThousandCodes("8000"));
				AssertEquals("8100", ZBool.True, DeclarationProviderHelper.Check8ThousandCodes("8100"));
				AssertEquals("8200", ZBool.True, DeclarationProviderHelper.Check8ThousandCodes("8200"));
			});
		}

		public void TestRoundAmount()
		{
			var amount1 = decimal.Zero;
			CombineAssertions("Decimal 2 Digit", () =>
			{
				amount1 = 123.10m;
				AssertEquals(123.10m, amount1.RoundAmount());
				amount1 = 123.12m;
				AssertEquals(123.12m, amount1.RoundAmount());
				amount1 = 123.1200m;
				AssertEquals(123.12m, amount1.RoundAmount());
				amount1 = 123.123m;
				AssertEquals(123.12m, amount1.RoundAmount());
				amount1 = 123.1234m;
				AssertEquals(123.12m, amount1.RoundAmount());
			});

			var amount2 = ZDecimal.Zero;
			CombineAssertions("ZDecimal 2 Digit", () =>
			{
				amount2 = 123.10m;
				AssertEquals(123.10m, amount2.RoundAmount());
				amount2 = 123.12m;
				AssertEquals(123.12m, amount2.RoundAmount());
				amount2 = 123.1200m;
				AssertEquals(123.12m, amount2.RoundAmount());
				amount2 = 123.123m;
				AssertEquals(123.12m, amount2.RoundAmount());
				amount2 = 123.1234m;
				AssertEquals(123.12m, amount2.RoundAmount());
			});
		}

		public void TestRoundExchangeAmount()
		{
			var amount = ZDecimal.Zero;
			CombineAssertions("ZDecimal 6 Digit", () =>
			{
				amount = 123.10m;
				AssertEquals(123.10000m, amount.RoundExchangeAmount());
				amount = 123.12m;
				AssertEquals(123.12000m, amount.RoundExchangeAmount());
				amount = 123.1200m;
				AssertEquals(123.12000m, amount.RoundExchangeAmount());
				amount = 123.123m;
				AssertEquals(123.12300m, amount.RoundExchangeAmount());
				amount = 123.1234m;
				AssertEquals(123.12340m, amount.RoundExchangeAmount());
				amount = 123.12345m;
				AssertEquals(123.12345m, amount.RoundExchangeAmount());
				amount = 123.123456m;
				AssertEquals(123.123456m, amount.RoundExchangeAmount());
				amount = 123.1234567m;
				AssertEquals(123.123457m, amount.RoundExchangeAmount());
			});
		}

		public void TestRoundPerceptionQuantity()
		{
			var perceptionQuantity = decimal.Zero;
			CombineAssertions("Decimal 4 Digit", () =>
			{
				perceptionQuantity = 123.1000m;
				AssertEquals(123.1000m, perceptionQuantity.RoundPerceptionQuantity());
				perceptionQuantity = 123.1200m;
				AssertEquals(123.1200m, perceptionQuantity.RoundPerceptionQuantity());
				perceptionQuantity = 123.12000m;
				AssertEquals(123.1200m, perceptionQuantity.RoundPerceptionQuantity());
				perceptionQuantity = 123.12300m;
				AssertEquals(123.1230m, perceptionQuantity.RoundPerceptionQuantity());
				perceptionQuantity = 123.12340m;
				AssertEquals(123.1234m, perceptionQuantity.RoundPerceptionQuantity());
				perceptionQuantity = 123.12349m;
				AssertEquals(123.1235m, perceptionQuantity.RoundPerceptionQuantity());
			});
		}

		public void TestRoundWeight()
		{
			var weight = ZDecimal.Zero;
			CombineAssertions("Decimal 4 Digit", () =>
			{
				weight = 123.1000m;
				AssertEquals(123.1000m, weight.RoundWeight());
				weight = 123.1200m;
				AssertEquals(123.1200m, weight.RoundWeight());
				weight = 123.12000m;
				AssertEquals(123.1200m, weight.RoundWeight());
				weight = 123.12300m;
				AssertEquals(123.1230m, weight.RoundWeight());
				weight = 123.12340m;
				AssertEquals(123.1234m, weight.RoundWeight());
				weight = 123.12349m;
				AssertEquals(123.1235m, weight.RoundWeight());
			});
		}

		public void TestRoundRatio()
		{
			var ratioValue = ZString.Empty;
			CombineAssertions("Decimal Digit", () =>
			{
				ratioValue = "123.00";
				AssertEquals("0 Digit", "123", ratioValue.RoundRatio());
				ratioValue = "123.10";
				AssertEquals("2 Digit", "123.10", ratioValue.RoundRatio());
				ratioValue = "123.12";
				AssertEquals("2 Digit", "123.12", ratioValue.RoundRatio());
				ratioValue = "123.1200";
				AssertEquals("2 Digit", "123.12", ratioValue.RoundRatio());
				ratioValue = "123.123";
				AssertEquals("3 Digit", "123.123", ratioValue.RoundRatio());
				ratioValue = "123.1234";
				AssertEquals("3 Digit", "123.123", ratioValue.RoundRatio());
			});

			ratioValue = "123.10";
			using (TRCustomsDataRegistry.Instance.TRTestingSystem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				using (TRCustomsDataRegistry.Instance.SendTRDeclarationWithComma.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Used dot", "123.10", ratioValue.RoundRatio());
				}
				using (TRCustomsDataRegistry.Instance.SendTRDeclarationWithComma.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Used dot", "123.10", ratioValue.RoundRatio());
				}
			}

			using (TRCustomsDataRegistry.Instance.TRTestingSystem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (TRCustomsDataRegistry.Instance.SendTRDeclarationWithComma.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Used dot", "123.10", ratioValue.RoundRatio());
				}
				using (TRCustomsDataRegistry.Instance.SendTRDeclarationWithComma.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Used comma", "123,10", ratioValue.RoundRatio());
				}
			}
		}

		public void TestGetMailAddress()
		{
			using (Env.SetTemporaryUserContext(new UserContext(loggedInUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions("Mail Address", () =>
				{
					AssertEquals("İlker", "ilker.pakten@wisetechglobal.com", DeclarationProviderHelper.GetMailAddress(1));
					AssertEquals("Mehmet", "mehmet.cakmak@@wisetechglobal.com", DeclarationProviderHelper.GetMailAddress(2));
					AssertEquals("Empty", ZString.Empty, DeclarationProviderHelper.GetMailAddress(3));
				});
			}
		}

		public void TestGetCurrency()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var invoiceLines = headerJobDeclaration.Invoices.Cast<JobComInvoiceHeader>().Single();

				var result = DeclarationProviderHelper.GetCurrency(ZDecimal.Zero, invoiceLines.OverseasInsurance.Currency);
				AssertEquals("When Total Value Is Empty", ZString.Empty, result);

				result = DeclarationProviderHelper.GetCurrency(100m, invoiceLines.OverseasInsurance.Currency);
				AssertEquals("When Currency Code Is Not Null", "USD", result);

				invoiceLines = null;
				result = DeclarationProviderHelper.GetCurrency(100m, null);
				AssertEquals("When Currency Code Is Null", ZString.Empty, result);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			loggedInUser = Factory.NewWithValidTestData<GlbStaff>();
			loggedInUser.GS_Code = "ULU";
			loggedInUser.GS_FullName = "test ulukom";
			loggedInUser.GS_MobilePhone = "+905322173033";
			var emailAddress = loggedInUser.EmailAddresses.AddNew();
			emailAddress.GSE_EmailAddress = "ilker.pakten@wisetechglobal.com";
			emailAddress.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
			var emailAddress2 = loggedInUser.EmailAddresses.AddNew();
			emailAddress2.GSE_EmailAddress = "mehmet.cakmak@@wisetechglobal.com";
			emailAddress2.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Default;
			Factory.Save();

			var currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-2);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
			exchangeRate.RE_SellRate = 27m;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			jobDeclaration.JE_TransportMode = "ROA";
			jobDeclaration.Company.GC_IsReciprocal = true;

			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "21340300IM123456";

			cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;
			cusEntryLine.CL_AdValoremTariff = "660320000000";

			invoiceHeader = jobDeclaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2021, 3, 30);
			invoiceHeader.JZ_InvoiceNumber = "5435345345";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_InvoiceCurrExRate = 20m;
			invoiceHeader.JZ_IncoTerm = "FOB";

			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.JI_Tariff = "123456789";
		}

		GlbStaff loggedInUser;
		CusEntryLine cusEntryLine;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
