using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105Declaration_DutyTaxFeeTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDutyTaxFee_DutyExemptionWaiverNote()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			IDutyTaxFee dutyTaxFee = new NX5105Declaration_DutyTaxFee(entryHeader);
			entryHeader.EntryInstruction.CEI_WaiverOfExemption = true;
			NUnit.Framework.Assert.That(dutyTaxFee.DutyExemptionWaiverNote, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyExemptionWaiverNote should be");
			entryHeader.EntryInstruction.CEI_WaiverOfExemption = false;
			NUnit.Framework.Assert.That(dutyTaxFee.DutyExemptionWaiverNote, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyExemptionWaiverNote should be");
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFee_DutyDemoPrinted()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			IDutyTaxFee dutyTaxFee = new NX5105Declaration_DutyTaxFee(entryHeader);
			entryHeader.EntryInstruction.CEI_PrintDutyMemo = true;
			NUnit.Framework.Assert.That(dutyTaxFee.DutyMemoPrinted, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyMemoPrinted should be");
			entryHeader.EntryInstruction.CEI_PrintDutyMemo = false;
			NUnit.Framework.Assert.That(dutyTaxFee.DutyMemoPrinted, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyMemoPrinted should be");
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFee_DutyMethodCode()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			IDutyTaxFee dutyTaxFee = new NX5105Declaration_DutyTaxFee(entryHeader);
			entryHeader.Declaration.JE_PaymentMethod = ZString.Empty;
			NUnit.Framework.Assert.That(dutyTaxFee.DutyMethodCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyMethodCode should be");
			entryHeader.Declaration.JE_PaymentMethod = IMPPaymentMethod.Codes._1;
			NUnit.Framework.Assert.That(dutyTaxFee.DutyMethodCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyMethodCode should be");
			entryHeader.Declaration.JE_PaymentMethod = IMPPaymentMethod.Codes._5;
			NUnit.Framework.Assert.That(dutyTaxFee.DutyMethodCode, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyMethodCode should be");
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFee_TotalDutyTaxFeeAmount()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			IDutyTaxFee dutyTaxFee = new NX5105Declaration_DutyTaxFee(entryHeader);
			var charge1 = entryHeader.Charges.AddNew();
			charge1.C1_ChargeAmount = 10561.99m;
			var charge2 = entryHeader.Charges.AddNew();
			charge2.C1_ChargeAmount = 12778.87m;
			NUnit.Framework.Assert.That(dutyTaxFee.TotalDutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(23340m).Using(CustomComparers.TypeComparison), "DutyTaxFee.TotalDutyTaxFeeAmount should be");
			charge2.C1_ChargeAmount = 0m;
			NUnit.Framework.Assert.That(dutyTaxFee.TotalDutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(10561m).Using(CustomComparers.TypeComparison), "DutyTaxFee.TotalDutyTaxFeeAmount should be");
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFee_PaymentObligationGuaranteeReferenceID()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			IDutyTaxFee dutyTaxFee = new NX5105Declaration_DutyTaxFee(entryHeader);
			entryHeader.Declaration.JE_DefermentAccountNumber = ZString.Empty;
			NUnit.Framework.Assert.That(dutyTaxFee.PaymentObligationGuaranteeReferenceID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "DutyTaxFee.PaymentObligationGuaranteeReferenceID should be");
			entryHeader.Declaration.JE_DefermentAccountNumber = "AAA123456";
			NUnit.Framework.Assert.That(dutyTaxFee.PaymentObligationGuaranteeReferenceID, NUnit.Framework.Is.EqualTo("AAA123456").Using(CustomComparers.TypeComparison), "DutyTaxFee.PaymentObligationGuaranteeReferenceID should be");
		}

		[ExpectNoExceptions]
		public void TestTotalCashDutyTaxFeeAmount()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			IDutyTaxFee dutyTaxFee = new NX5105Declaration_DutyTaxFee(entryHeader);
			var feeCharge = entryHeader.DutyTaxFeeCharges.AddNew();
			feeCharge.MethodOfPayment = "CAS";
			feeCharge.ChargeAmount = 123d;
			NUnit.Framework.Assert.That(dutyTaxFee.TotalCashDutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(123m).Using(CustomComparers.TypeComparison), "DutyTaxFee.TotalCashDutyTaxFeeAmount should be");
		}

		[ExpectNoExceptions]
		public void TestTotalNonCashDutyTaxFeeAmount()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			IDutyTaxFee dutyTaxFee = new NX5105Declaration_DutyTaxFee(entryHeader);
			var feeCharge = entryHeader.DutyTaxFeeCharges.AddNew();
			feeCharge.MethodOfPayment = "DEF";
			feeCharge.ChargeAmount = 123d;
			NUnit.Framework.Assert.That(dutyTaxFee.TotalNonCashDutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(123m).Using(CustomComparers.TypeComparison), "DutyTaxFee.TotalNonCashDutyTaxFeeAmount should be");
		}

		CusEntryHeader GetEntryHeaderWithMinimumData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			return entryHeader;
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				new NX5105Declaration_DutyTaxFee(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				new NX5105Declaration_DutyTaxFee(GetEntryHeaderWithMinimumData());
			}

			);
		}
	}
}
