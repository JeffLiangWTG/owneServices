using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USDeductionChargeTest : Common.Testing.CustomsChargeCodeTest
	{
		protected override string ExpectedCode => USCustomsChargeTypeList.Codes.DeductionCharge;

		protected override string ExpectedDescription => USCustomsChargeTypeList.Descriptions.DeductionCharge;

		protected override bool ExpectedIsDutiable => false;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => true;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsVATible => false;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncoTermNeutral => true;

		protected override ICustomsChargeCode GetChargeCodeToTest() => DeliveryTermAndChargeFactory.DeductionCharge;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;
	}
}
