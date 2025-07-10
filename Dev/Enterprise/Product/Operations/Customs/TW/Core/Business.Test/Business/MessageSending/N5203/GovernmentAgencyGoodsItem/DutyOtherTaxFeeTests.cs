using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DutyOtherTaxFeeTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMethodCode()
		{
			NUnit.Framework.Assert.That(dutyOtherTaxFee.MethodCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTaxRateNumeric()
		{
			NUnit.Framework.Assert.That(dutyOtherTaxFee.TaxRateNumeric, NUnit.Framework.Is.EqualTo(0.00001m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(dutyOtherTaxFee.TypeCode, NUnit.Framework.Is.EqualTo("B52").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			invoiceLine = testHelper.CreateInvoiceLineForN5203(entryHeader);
			var fees = EntryLine.Fees;
			var fee1 = fees.AddNew();
			fee1.CF_Rate = 0.00001M;
			fee1.CF_ChargeType = RefCusTaxOrFeeCodes.TPF;
			fee1.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			fee1.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
		}

		CusEntryHeader entryHeader;
		CusEntryLine EntryLine => entryHeader.MergedLines.Cast<CusEntryLine>().First();
		ICommodity commodity => new Commodity(EntryLine, invoiceLine);
		JobComInvoiceLine invoiceLine;
		IDutyOtherTaxFee dutyOtherTaxFee => commodity.DutyOtherTaxFees.Single();
	}
}
