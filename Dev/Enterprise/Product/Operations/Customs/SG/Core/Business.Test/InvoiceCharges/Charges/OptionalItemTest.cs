using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class OptionalItemTest : Common.Testing.CustomsChargeCodeTest
	{
		protected override string ExpectedCode => InvoiceLineCharge.ChargeTypes.OptionalItemCharges;
		protected override string ExpectedDescription => "Optional Item Charges";
		protected override Common.ICustomsChargeCode GetChargeCodeToTest() => IncoTermAndCustomsChargeFactory.OptionalItem;
		protected override bool ExpectedIsDutiable => false;
		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;
		protected override bool ExpectedIsPercentageApplicable => false;
		protected override bool ExpectedIsVATible => true;
		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;
		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;
		protected override bool ExpectedIsIncoTermNeutral => true;
		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;
		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;
		protected override string GetCountryContext() => Core.Constants.CountryCodes.Singapore;
	}
}
