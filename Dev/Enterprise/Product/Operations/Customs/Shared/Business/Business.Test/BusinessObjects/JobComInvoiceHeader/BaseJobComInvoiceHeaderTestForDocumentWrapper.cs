using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobComInvoiceHeaderTestForDocumentWrapper : TestCaseWithFactory
	{
		public void TestIncludedTotalInInvoiceCurr()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ZString currencyCode = declaration.LocalCurrencyCode;
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoiceHeader.Charges.RemoveAll();

			BaseJobComInvHeaderCharge osFreightCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 3m, currencyCode);
			BaseJobComInvHeaderCharge osInsuranceCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 5m, currencyCode);
			BaseJobComInvHeaderCharge exWorksCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 7m, currencyCode);
			BaseJobComInvHeaderCharge inlandFreightCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 11m, currencyCode);
			BaseJobComInvHeaderCharge packingCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 13m, currencyCode);
			BaseJobComInvHeaderCharge landingCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 17m, currencyCode);
			BaseJobComInvHeaderCharge dutiableOtherCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 19m, currencyCode);
			dutiableOtherCharge.J7_IsDutiable = true;
			BaseJobComInvHeaderCharge nonDutiableOtherCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 23m, currencyCode);
			nonDutiableOtherCharge.J7_IsDutiable = false;
			BaseJobComInvHeaderCharge comissionCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 29m, currencyCode);
			BaseJobComInvHeaderCharge discountCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 31m, currencyCode);

			AssertEquals(ExpectedIncludedTotalInInvoiceCurr, invoiceHeader.IncludedTotalInInvoiceCurr.Amount);
		}
		protected virtual ZDecimal ExpectedIncludedTotalInInvoiceCurr
		{
			get { return 96m; }
		}

		public virtual void TestCalcCommission()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.Commission, "Commission", CustomsChargeTypeList.Codes.Commission);
		}

		public void TestCalcDiscount()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.Discount, "Discount", CustomsChargeTypeList.Codes.Discount);
		}

		public void TestCalcOverseasFreight()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.OverseasFreight, "OverseasFreight", CustomsChargeTypeList.Codes.OverseasFreight);
		}

		public void TestCalcOverseasInsurance()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.OverseasInsurance, "OverseasInsurance", CustomsChargeTypeList.Codes.OverseasInsurance);
		}

		public void TestCalcForeignInlandFreight()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.ForeignInlandFreight, "ForeignInlandFreight", CustomsChargeTypeList.Codes.ForeignInlandFreight);
		}

		public void TestCalcPackingCosts()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.PackingCost, "PackingCosts", CustomsChargeTypeList.Codes.PackingCost);
		}

		public virtual void TestCalcOtherCharges1()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.OtherCharges, "OtherCharges1", CustomsChargeTypeList.Codes.AdditionCharge);
		}

		public virtual void TestCalcOtherCharges2()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.OtherCharges, "OtherCharges2", CustomsChargeTypeList.Codes.DeductionCharge);
		}

		public void TestCalcLandingCharges()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.LandingCharges, "LandingCharges", CustomsChargeTypeList.Codes.LandingCharges);
		}

		public virtual void TestCalcExWorks()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.ExWorks, "ExWorks", CustomsChargeTypeList.Codes.ExWorks);
		}

		public virtual void TestNonDutiableChargesNotIncludedInLinesWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			BaseInvoiceCharge charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			BaseInvoiceCharge overseasFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			BaseInvoiceCharge overseasInsuranceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(invoice.NonDutiableChargesNotIncludedInLines.Amount, 600m);
		}

		public virtual void TestNonDutiableChargesNotIncludedInLinesInLocalCurrencyWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			BaseInvoiceCharge charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			BaseInvoiceCharge overseasFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			BaseInvoiceCharge overseasInsuranceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(invoice.NonDutiableChargesNotIncludedInLinesInLocalCurrency.Amount, 600m);
		}

		public virtual void TestDutiableChargesNotIncludedInLinesWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			BaseInvoiceCharge charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsDutiable = true;
			charge.J7_IsIncludedInITOT = false;

			BaseInvoiceCharge overseasFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			BaseInvoiceCharge overseasInsuranceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(invoice.DutiableChargesNotIncludedInLines.Amount, 100m);
		}

		public virtual void TestDutiableChargesNotIncludedInLinesInLocalCurrencyWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			BaseInvoiceCharge charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsDutiable = true;
			charge.J7_IsIncludedInITOT = false;

			BaseInvoiceCharge overseasFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			BaseInvoiceCharge overseasInsuranceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(invoice.DutiableChargesNotIncludedInLinesInLocalCurrency.Amount, 100m);
		}

		public void TestConvertToLocalAmountRoundedWorksCorrectly()
		{
			AssertEquals(invoice.CurrencyConverter.ConvertRounded(new Money(100m, invoice.LocalCurrency), invoice.LocalCurrency).Amount, invoice.ConvertToLocalAmountRounded(100m, invoice.LocalCurrency).Amount);
		}

		public virtual void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			BaseInvoiceCharge charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			BaseInvoiceCharge overseasFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			BaseInvoiceCharge overseasInsuranceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(invoice.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Amount, 100m);
		}

		public virtual void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrencyWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			BaseInvoiceCharge charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			BaseInvoiceCharge overseasFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			BaseInvoiceCharge overseasInsuranceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(invoice.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrency.Amount, 100m);
		}

		public void TestOverseasFreightIsSumOfIncludedAndExcludedOverseasFreights()
		{
			invoice.Charges.RemoveAll();
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 110m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 200m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(invoice.OverseasFreight.Amount, invoice.IncludedOverseasFreight.Amount + invoice.ExcludedOverseasFreight.Amount);
			AssertEquals(invoice.OverseasFreight.Amount, 110m);
		}

		public void TestOverseasInsuranceIsSumOfIncludedAndExcludedOverseasInsurance()
		{
			invoice.Charges.RemoveAll();
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 110m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 200m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(invoice.OverseasInsurance.Amount, invoice.IncludedOverseasInsurance.Amount + invoice.ExcludedOverseasInsurance.Amount);
			AssertEquals(invoice.OverseasInsurance.Amount, 200m);
		}

		public void TestJZ_Calc_ConversionFactorIsZeroIfInvoiceLineTotalIsZero()
		{
			invoice.Charges.RemoveAll();
			invoice.GroupCharges.RemoveAll();
			invoice.JZ_InvoiceAmount = 0;
			AssertEquals(invoice.InvoiceLineTotal, 0m);
		}

		public void TestEntryHeaderAndDeclarationDate()
		{
			BaseJobDeclaration declaration = invoice.JobDeclaration;
			var aUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			invoice.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			BaseJobComInvoiceLine invLine1 = invoice.JobComInvoiceLines.AddNew();
			invLine1.JI_LinePrice = 1000m;
			BaseJobComInvoiceLine invLine2 = invoice.JobComInvoiceLines.AddNew();
			invLine2.JI_LinePrice = 500m;
			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			Assert("Initialy no declaration date", invoice.DeclarationDate.IsEmpty);
			StmALog[] logs = entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Preassertion", 0, logs.Length);
			entryHeader.CH_Status = "ZZ";
			entryHeader.Logs.AddNew(Events.CustomsCleared);
			logs = entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			StmALog log = logs[0];
			AssertNotNull(log);
			Assert(!invoice.DeclarationDate.IsEmpty);
			AssertEquals("Now clearance date", entryHeader.ClearanceDate, invoice.DeclarationDate);
			AssertEquals("Entry Header", entryHeader, invoice.FirstEntryHeader);
		}

		public virtual void TestJZ_Calc_OFTInInvoiceCurrency()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, groupHeader.JobDeclaration.LocalCurrencyCode);
				invoice.JZ_InvoiceAmount = 1000;
				invoice.JZ_RX_NKInvoice_Currency = groupHeader.JobDeclaration.LocalCurrencyCode;
				testDec.ResumeApportionment();
				AssertEquals("JZ_Calc_OFTInInvoiceCurrency", 100m, invoice.JZ_Calc_OFTInInvoiceCurrency);

				groupHeader.Charges.RemoveAndDeleteAll();
				testDec.ResumeApportionment();
				AssertEquals("JZ_Calc_OFTInInvoiceCurrency", 0m, invoice.JZ_Calc_OFTInInvoiceCurrency);

				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200, groupHeader.JobDeclaration.LocalCurrencyCode);
				testDec.ResumeApportionment();
				AssertEquals("JZ_Calc_OFTInInvoiceCurrency", 200m, invoice.JZ_Calc_OFTInInvoiceCurrency);
			}
		}

		public void TestJZ_Calc_ONSInInvoiceCurrency()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100, groupHeader.JobDeclaration.LocalCurrencyCode);
				invoice.JZ_InvoiceAmount = 1000;
				invoice.JZ_RX_NKInvoice_Currency = groupHeader.JobDeclaration.LocalCurrencyCode;
				testDec.ResumeApportionment();
				AssertEquals("JZ_Calc_ONSInInvoiceCurrency", 100m, invoice.JZ_Calc_ONSInInvoiceCurrency);

				groupHeader.Charges.RemoveAndDeleteAll();
				testDec.ResumeApportionment();
				AssertEquals("JZ_Calc_ONSInInvoiceCurrency", 0m, invoice.JZ_Calc_ONSInInvoiceCurrency);

				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 200, groupHeader.JobDeclaration.LocalCurrencyCode);
				testDec.ResumeApportionment();
				AssertEquals("JZ_Calc_ONSInInvoiceCurrency", 200m, invoice.JZ_Calc_ONSInInvoiceCurrency);
			}
		}

		public void TestInvoiceLineTotalInLocalCurrency()
		{
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			AssertEquals(invoice.InvoiceLineTotalInLocalCurrency, invoice.ConvertToLocalAmountRounded(invoice.InvoiceLineTotal, invoice.Invoice_Currency).Amount);
		}
		public void TestJZ_Calc_FOBAmountInLocalCurrency()
		{
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			AssertEquals(invoice.JZ_Calc_FOBAmountInLocalCurrency, invoice.ConvertToLocalAmountRounded(invoice.JZ_Calc_FOBAmount, invoice.CalcFOBCurrency).Amount);
		}
		public void TestJZ_InvoiceAmountInLocalCurrency()
		{
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			AssertEquals(invoice.JZ_InvoiceAmountInLocalCurrencyMoney.Amount, invoice.JZ_InvoiceAmountInLocalCurrency);
		}

		public void TestJZ_InvoiceAmountInLocalCurrencyMoney()
		{
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			AssertEquals(invoice.ConvertToLocalAmountRounded(invoice.JZ_InvoiceAmount, invoice.Invoice_Currency), invoice.JZ_InvoiceAmountInLocalCurrencyMoney);
		}

		public void TestHiddenOriginalParentGuid()
		{
			BaseJobDeclaration declaration = invoice.JobDeclaration;
			AssertNotNull("Precondition", declaration);
			invoice.JZ_JE = ZGuid.Empty;
			AssertEquals("JobDeclaration still returns value after clearing JZ_JE", declaration, invoice.JobDeclaration);
		}

		public virtual void TestCheckingValueOfJZ_Calc_ConversionFactor()
		{
			var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			SetExchangeRate(ZDateTime.Now, ZDateTime.Now.AddYears(10), 0.5m, uSDCurrency, "CUS");

			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			invoice.JZ_IncoTerm = "CIF";

			invoice.Charges.RemoveAll();
			BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m);
			oFT.J7_IsIncludedInITOT = true;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);

			//Line Total : 10000 - 50 as ONS is not included in lines
			//FOB amount in USD: 10000 - 500 - 50
			ZDecimal expectedValue = invoice.ConvertToLocalAmountRounded(10000 - 500 - 50.0m, uSDCurrency).Amount / (10000 - 50.0m);
			AssertEquals(ZArchitecture.Core.Utilities.Round(invoice.JZ_Calc_ConversionFactor, 4), ZArchitecture.Core.Utilities.Round(expectedValue, 4));
		}

		public virtual void TestPerformLocalRounding()
		{
			BaseJobComInvoiceHeaderExposer exposer = Factory.New<BaseJobComInvoiceHeaderExposer>();
			AssertEquals(exposer.PerformLocalRounding(10.02m), 10m);
		}

		protected void AssertCalcFields(string chargeName, string calcAmountName, string chargeType)
		{
			if (invoice.IncoTermAndChargeFactory.GetAllCharges().Any(x => x.Code == chargeName))
			{
				BaseJobComInvHeaderCharge charge = invoice.GroupCharges.AddNew(chargeType, 100m, invoice.JobDeclaration.LocalCurrencyCode);
				charge.J7_Calc_IsIncludedInInvoiceAmount = false;
				string includedAmount = "Included" + calcAmountName;
				string excludedAmount = "Excluded" + calcAmountName;

				Money actualAmount = (Money)invoice[excludedAmount];
				AssertEquals(excludedAmount, 100m, actualAmount.Amount);
				AssertEquals(excludedAmount, invoice.JobDeclaration.LocalCurrencyCode, actualAmount.Currency.Code);

				charge.J7_Calc_IsIncludedInInvoiceAmount = true;
				actualAmount = (Money)invoice[includedAmount];
				AssertEquals(includedAmount, 100m, actualAmount.Amount);
				AssertEquals(includedAmount, invoice.JobDeclaration.LocalCurrencyCode, actualAmount.Currency.Code);
			}
			else
			{
				Assert("No applicable", true);
			}
		}

		#region Implementation

		protected virtual BaseJobDeclaration GetNewDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		protected BaseJobDeclaration testDec;
		protected BaseJobComInvoiceGroupHeader groupHeader;
		protected BaseJobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = GetNewDeclaration();
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
		}

		public void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate, RefCurrency foreignCurrency, string exRateType)
		{
			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, foreignCurrency.RX_Code);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, exRateType);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, startDate.AddDays(1));
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, endDate);

			var exchangeRateDuty = Factory.LoadTop1<RefExchangeRate>(sQLFilter);
			if (exchangeRateDuty == null)
			{
				exchangeRateDuty = Factory.New<RefExchangeRate>();
				exchangeRateDuty.RE_ExpiryDate = endDate;
				exchangeRateDuty.RE_ExRateType = exRateType;
				exchangeRateDuty.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRateDuty.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
				exchangeRateDuty.RE_StartDate = startDate;
				exchangeRateDuty.RE_SellRate = exchangeRate;
			}
			else
			{
				exchangeRateDuty.RE_SellRate = exchangeRate;
			}
		}

		protected class BaseJobComInvoiceHeaderExposer : BaseJobComInvoiceHeader
		{
			public BaseJobComInvoiceHeaderExposer(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new ZDecimal PerformLocalRounding(ZDecimal d)
			{
				return base.PerformLocalRounding(d);
			}
		}
		#endregion
	}
}
