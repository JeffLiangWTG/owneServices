using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DutyTaxFeeTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDutyExemptionWaiverNote()
		{
			NUnit.Framework.Assert.That(DutyTaxFee.DutyExemptionWaiverNote, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestDutyMemoPrinted()
		{
			NUnit.Framework.Assert.That(DutyTaxFee.DutyMemoPrinted, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestDutyMethodCode()
		{
			entryHeader.Declaration.JE_PaymentMethod = EXPPaymentMethod.Codes._1;
			NUnit.Framework.Assert.That(DutyTaxFee.DutyMethodCode, NUnit.Framework.Is.EqualTo(EXPPaymentMethod.Codes._1).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalDutyTaxFeeAmount()
		{
			NUnit.Framework.Assert.That(DutyTaxFee.TotalDutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
		}

		[ExpectNoExceptions]
		public void TestPaymentObligationGuaranteeReferenceID()
		{
			NUnit.Framework.Assert.That(DutyTaxFee.PaymentObligationGuaranteeReferenceID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTotalCashDutyTaxFeeAmount()
		{
			NUnit.Framework.Assert.That(DutyTaxFee.TotalCashDutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
		}

		[ExpectNoExceptions]
		public void TestTotalNonCashDutyTaxFeeAmount()
		{
			NUnit.Framework.Assert.That(DutyTaxFee.TotalNonCashDutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
		}

		CusEntryHeader entryHeader;
		IDutyTaxFee DutyTaxFee => new DutyTaxFee(entryHeader);
	}
}
