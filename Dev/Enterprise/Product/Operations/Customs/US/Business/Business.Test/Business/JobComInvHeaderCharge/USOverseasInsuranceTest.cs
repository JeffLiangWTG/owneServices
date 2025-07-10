using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	class USOverseasInsuranceTest : Common.Testing.CustomsChargeCodeTest
	{
		protected override string ExpectedCode => USCustomsChargeTypeList.Codes.OverseasInsurance;

		protected override string ExpectedDescription => USCustomsChargeTypeList.Descriptions.OverseasInsurance;

		protected override bool ExpectedIsDutiable => false;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => false;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => true;

		protected override bool ExpectedIsIncoTermNeutral => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => incoterm != Core.Constants.IncoTerms.DeliveredAtPlace && incoterm != Core.Constants.IncoTerms.DeliveredAtTerminal && incoterm != Core.Constants.IncoTerms.DeliveredDutyPaid;

		protected override ICustomsChargeCode GetChargeCodeToTest() => DeliveryTermAndChargeFactory.OverseasInsurance;
	}
}
