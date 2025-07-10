using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ChargeCollectionHelperTest : TestCaseWithFactory
	{
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		}

		public void TestAdditionCharges()
		{
			var invoice = GetInvoiceChargesForTest();
			var charges = invoice.Charges.AdditionCharges();
			AssertEquals(12, charges.Count());
			AssertEquals(82m, charges.Sum(x => x.J7_Amount));
		}

		public void TestDeductionCharges()
		{
			var invoice = GetInvoiceChargesForTest();
			var expectedAmount = 17m + 18m + 19m + 20m;
			var charges = invoice.Charges.DeductionCharges();
			AssertEquals(4, charges.Count());
			AssertEquals(expectedAmount, charges.Sum(x => x.J7_Amount));
		}

		JobComInvoiceHeader GetInvoiceChargesForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AddCharge(invoice, false, false, false, false, 1m);
			AddCharge(invoice, false, false, false, true, 2m);
			AddCharge(invoice, false, false, true, false, 3m);
			AddCharge(invoice, false, false, true, true, 4m);
			AddCharge(invoice, false, true, false, false, 5m);
			AddCharge(invoice, false, true, false, true, 6m);
			AddCharge(invoice, false, true, true, false, 7m);
			AddCharge(invoice, false, true, true, true, 8m);
			AddCharge(invoice, true, false, false, false, 9m);
			AddCharge(invoice, true, false, false, true, 10m);
			AddCharge(invoice, true, false, true, false, 11m);
			AddCharge(invoice, true, false, true, true, 12m);
			AddCharge(invoice, true, true, false, false, 13m);
			AddCharge(invoice, true, true, false, true, 14m);
			AddCharge(invoice, true, true, true, false, 15m);
			AddCharge(invoice, true, true, true, true, 16m);
			AddCharge(invoice, false, false, false, false, 17m, chargeType: CustomsChargeTypeList.Codes.DeductionCharge);
			AddCharge(invoice, false, false, false, true, 18m, chargeType: CustomsChargeTypeList.Codes.DeductionCharge);
			AddCharge(invoice, false, false, true, false, 19m, chargeType: CustomsChargeTypeList.Codes.DeductionCharge);
			AddCharge(invoice, false, false, true, true, 20m, chargeType: CustomsChargeTypeList.Codes.DeductionCharge);
			AddCharge(invoice, true, false, false, false, 21m, isGroupCharge: true);
			AddCharge(invoice, true, false, false, true, 22m, isGroupCharge: true);
			AddCharge(invoice, true, false, true, false, 23m, isGroupCharge: true);
			AddCharge(invoice, true, false, true, true, 24m, isGroupCharge: true);
			AddCharge(invoice, false, false, false, false, 25m, chargeType: CustomsChargeTypeList.Codes.DeductionCharge, isGroupCharge: true);
			AddCharge(invoice, false, false, false, true, 26m, chargeType: CustomsChargeTypeList.Codes.DeductionCharge, isGroupCharge: true);
			AddCharge(invoice, false, false, true, false, 27m, chargeType: CustomsChargeTypeList.Codes.DeductionCharge, isGroupCharge: true);
			AddCharge(invoice, false, false, true, true, 28m, chargeType: CustomsChargeTypeList.Codes.DeductionCharge, isGroupCharge: true);
			return invoice;
		}

		void AddCharge(JobComInvoiceHeader invoice, ZBool isIncludedInInvoiceAmount, ZBool isDutiable, ZBool isIncludedInITOT, ZBool isGSTApplicable, ZDecimal amount, string chargeType = "", bool isGroupCharge = false)
		{
			if (isGroupCharge)
			{
				var charge = invoice.GroupHeader.Charges.AddNew();
				charge.J7_ChargeType = chargeType;
				charge.J7_IsIncludedInITOT = isIncludedInITOT;
				charge.J7_IsDutiable = isDutiable;
				charge.J7_IsGSTApplicable = isGSTApplicable;
				charge.J7_Amount = amount;
			}
			else
			{
				var charge = invoice.Charges.AddNew();
				charge.J7_ChargeType = chargeType;
				charge.J7_IsIncludedInITOT = isIncludedInITOT;
				charge.J7_Calc_IsIncludedInInvoiceAmount = isIncludedInInvoiceAmount;
				charge.J7_IsDutiable = isDutiable;
				charge.J7_IsGSTApplicable = isGSTApplicable;
				charge.J7_Amount = amount;
			}
		}
	}
}
