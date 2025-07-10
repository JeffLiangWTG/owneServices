using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DiscountTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => InvoiceLineCustomsChargeTypeList.Codes.Discount;
		protected override string ExpectedDescription => InvoiceLineCustomsChargeTypeList.Descriptions.Discount;
		protected override bool ExpectedIsDutiable => false;
		protected override bool ExpectedIsPercentageApplicable => true;
		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;
		protected override bool ExpectedIsVATible => false;
		protected override bool ExpectedIsDutiableDeemedForThisCharge => false;
		protected override bool ExpectedIsVATibleDeemedForThisCharge => false;
		protected override bool ExpectedIsIncoTermNeutral => true;
		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;
		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => true;
		protected override bool ExpectedIsDefaultIsIncludedInInvoice(ZString incoterm) => true;
		protected override ICustomsChargeCode GetChargeCodeToTest() => IncoTermAndCustomsChargeFactory.Discount;
	}
}
