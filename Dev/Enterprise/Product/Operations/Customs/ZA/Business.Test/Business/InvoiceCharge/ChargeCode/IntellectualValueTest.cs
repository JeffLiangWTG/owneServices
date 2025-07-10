using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class IntellectualValueTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue;
		protected override string ExpectedDescription => InvoiceLineCustomsChargeTypeList.Descriptions.IntellectualValue;
		protected override bool ExpectedIsDutiable => false;
		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => true;
		protected override bool ExpectedIsPercentageApplicable => true;
		protected override bool ExpectedIsVATible => false;
		protected override bool ExpectedIsDutiableDeemedForThisCharge => false;
		protected override bool ExpectedIsVATibleDeemedForThisCharge => false;
		protected override bool ExpectedIsIncoTermNeutral => true;
		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => true;
		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => true;
		protected override ICustomsChargeCode GetChargeCodeToTest() => IncoTermAndCustomsChargeFactory.IntellectualValue;
		protected override ChargeParentTypes ExpectedChargeParentTypes => ChargeParentTypes.InvoiceLine;
	}
}
