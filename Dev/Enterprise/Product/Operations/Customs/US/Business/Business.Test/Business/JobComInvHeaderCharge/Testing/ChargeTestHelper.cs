using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using CustomsChargeTypes = Enterprise.Customs.Common.CustomsChargeTypeList.Codes;

namespace Enterprise.Customs.US.Business.Testing
{
	public abstract class ChargeTestHelper : TestCaseWithFactory
	{
		protected void SetupBasicCharges(JobComInvChargeCollectionBase<JobComInvCharge> charges)
		{
			JobComInvCharge dutiableAllowanceIncludedInLine = SetupCharge(charges.AddNew(), CustomsChargeTypes.Discount, 30m, aUDCurrency.RX_Code, true, true, true);
			JobComInvCharge dutiableAllowanceNotIncludedInLine = SetupCharge(charges.AddNew(), CustomsChargeTypes.Discount, 15m, aUDCurrency.RX_Code, true, false, true);
			JobComInvCharge dutiableChargeIncludedInLine = SetupCharge(charges.AddNew(), "ZZ3", 55m, uSDCurrency.RX_Code, true, true, true);
			JobComInvCharge dutiableChargeNotIncludedInLine = SetupCharge(charges.AddNew(), "ZZ4", 110m, uSDCurrency.RX_Code, true, false, true);
			JobComInvCharge dutiableCBPAdjustmentNotIncludedInLine = SetupCharge(charges.AddNew(), "ZZ6", 22m, uSDCurrency.RX_Code, true, false, false);
			JobComInvCharge nonDutiableAllowanceIncludedInLine = SetupCharge(charges.AddNew(), CustomsChargeTypes.Discount, 33m, uSDCurrency.RX_Code, false, true, true);
			JobComInvCharge nonDutiableAllowanceNotIncludedInLine = SetupCharge(charges.AddNew(), CustomsChargeTypes.Discount, 66m, uSDCurrency.RX_Code, false, false, true);
			JobComInvCharge nonDutiableChargeNotIncludedInLine = SetupCharge(charges.AddNew(), "ZZ0", 60m, aUDCurrency.RX_Code, false, false, true);
			JobComInvCharge nonDutiableCBPAdjustmentIncludedInLine = SetupCharge(charges.AddNew(), "ZY1", 44m, uSDCurrency.RX_Code, false, true, true);
			JobComInvCharge nonDutiableCBPAdjustmentNotIncludedInLine = SetupCharge(charges.AddNew(), "ZY2", 88m, uSDCurrency.RX_Code, false, false, false);
		}

		protected JobComInvCharge SetupCharge(JobComInvCharge charge, ZString chargeCode, ZDecimal amount, ZString currencyCode, bool isDutiable, bool isIncludedInITOT, bool isIncluedInInvoice)
		{
			charge.J7_ChargeType = chargeCode;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currencyCode;
			charge.J7_IsDutiable = isDutiable;
			charge.J7_IsIncludedInITOT = isIncludedInITOT;
			charge.J7_IsNotIncludedInInvoice = !isIncludedInITOT && !isIncluedInInvoice;
			return charge;
		}

		protected override void SetUp()
		{
			base.SetUp();
			aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia);
			aUDCurrency.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddYears(1), 0.666667m);
			uSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
		protected RefCurrency aUDCurrency;
		protected RefCurrency uSDCurrency;
	}
}
