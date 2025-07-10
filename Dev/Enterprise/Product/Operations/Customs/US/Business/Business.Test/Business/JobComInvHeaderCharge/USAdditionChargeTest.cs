using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAdditionChargeTest : Common.Testing.CustomsChargeCodeTest
	{
		protected override string ExpectedCode => USCustomsChargeTypeList.Codes.AdditionCharge;

		protected override string ExpectedDescription => USCustomsChargeTypeList.Descriptions.AdditionCharge;

		protected override bool ExpectedIsDutiable => true;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => true;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncoTermNeutral => true;

		protected override ICustomsChargeCode GetChargeCodeToTest() => DeliveryTermAndChargeFactory.AdditionCharge;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;
	}
}
