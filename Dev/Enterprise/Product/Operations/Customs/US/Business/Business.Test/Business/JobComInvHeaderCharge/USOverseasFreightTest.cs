using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USOverseasFreightTest : Common.Testing.CustomsChargeCodeTest
	{
		protected override string ExpectedCode => USCustomsChargeTypeList.Codes.OverseasFreight;

		protected override string ExpectedDescription => USCustomsChargeTypeList.Descriptions.OverseasFreight;

		protected override bool ExpectedIsDutiable => false;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => false;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsIncoTermNeutral => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => incoterm != Core.Constants.IncoTerms.DeliveredAtTerminal;

		protected override ICustomsChargeCode GetChargeCodeToTest() => DeliveryTermAndChargeFactory.OverseasFreight;
	}
}
