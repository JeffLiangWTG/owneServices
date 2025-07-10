using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class OverseasFreightTest : Common.Testing.OverseasFreightTest
	{
		protected override bool ExpectedIsPercentageApplicable => true;
		protected override Common.ICustomsChargeCode GetChargeCodeToTest() => IncoTermAndCustomsChargeFactory.OverseasFreight;
		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => incoterm != UnitPriceTermTypeCodeList.Codes.CNI && base.ExpectedGetCalculatedIncludedInITOT(incoterm, userEnteredIsDutiable);
		protected override string GetCountryContext() => Core.Constants.CountryCodes.Singapore;
	}
}
