using Enterprise.Customs.Common;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.Types;

	class CustomsChargeCodeTest : Common.Testing.CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CustomsChargeTypeList.TSWCodes.Royalties;

		protected override string ExpectedDescription => CustomsChargeTypeList.TSWDescriptions.Royalties;

		protected override bool ExpectedIsDutiable => true;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => false;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncoTermNeutral => true;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override ICustomsChargeCode GetChargeCodeToTest() => TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge;
	}
}
