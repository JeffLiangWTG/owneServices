using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class NX5105Commodity_DutyTaxFeeAbstractTest<TCommodityDutyTaxFee> : TestCaseWithFactory
		where TCommodityDutyTaxFee : ICommodityDutyTaxFee
	{
		[ExpectNoExceptions]
		public virtual void TestCommodity_DutyTaxFee()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var dutyTaxFee = GetDutyTaxFee(entryLine1, entryLine1.RandomLine);
			NUnit.Framework.Assert.That(dutyTaxFee.AdValoremTaxBaseAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "DutyTaxFee.AdValoremTaxBaseAmount should be");
			NUnit.Framework.Assert.That(dutyTaxFee.SpecificTaxBaseQuantity, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "DutyTaxFee.SpecificTaxBaseQuantity should be");
			entryLine1.CL_CustomsValue = 1200.77m;
			NUnit.Framework.Assert.That(dutyTaxFee.AdValoremTaxBaseAmount, NUnit.Framework.Is.EqualTo(1200.77m).Using(CustomComparers.TypeComparison), "DutyTaxFee.AdValoremTaxBaseAmount should be");
			var charge = entryLine1.Fees.AddNew();
			charge.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			charge.CF_BaseValue = 12m;
			dutyTaxFee = GetDutyTaxFee(entryLine1, entryLine1.RandomLine);
			NUnit.Framework.Assert.That(dutyTaxFee.SpecificTaxBaseQuantity, NUnit.Framework.Is.EqualTo(12m).Using(CustomComparers.TypeComparison), "DutyTaxFee.SpecificTaxBaseQuantity should be");
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine1.PK;
			entryLine1.RefreshInvoiceLines();
			dutyTaxFee = GetDutyTaxFee(entryLine1, entryLine1.RandomLine);
			NUnit.Framework.Assert.That(dutyTaxFee.DutyRegimeCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyRegimeCode should be");
			invoiceLine.JI_TariffAdditionalCode = "add";
			entryLine1.RefreshInvoiceLines();
			NUnit.Framework.Assert.That(dutyTaxFee.DutyRegimeCode, NUnit.Framework.Is.EqualTo("add").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyRegimeCode should be");
			entryLine1.RefreshInvoiceLines();
			NUnit.Framework.Assert.That(dutyTaxFee.PercentageNumeric, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "DutyTaxFee.PercentageNumeric should be");
			invoiceLine.JI_CusValueConvRatio = 0.5m;
			entryLine1.RefreshInvoiceLines();
			NUnit.Framework.Assert.That(dutyTaxFee.PercentageNumeric, NUnit.Framework.Is.EqualTo(0.5m).Using(CustomComparers.TypeComparison), "DutyTaxFee.PercentageNumeric should be");
		}

		protected abstract TCommodityDutyTaxFee GetDutyTaxFee(CusEntryLine entryLine, JobComInvoiceLine invoiceLine);
	}
}
