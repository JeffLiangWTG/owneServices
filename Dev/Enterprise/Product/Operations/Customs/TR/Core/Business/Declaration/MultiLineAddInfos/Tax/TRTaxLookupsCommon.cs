using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class TRTaxLookupsCommon : EU.Business.Declaration.MultiLineAddInfos.TaxLookupsCommon
	{
		public TRTaxLookupsCommon(EU.Business.Declaration.IEuTax parent) : base(parent)
		{
		}

		protected override ZString[] GetRateTypesToExcludeForOtherCountries()
		{
			return new ZString[] { UniversalReferenceConstants.RefCusRateTypes.BanderolDuty, UniversalReferenceConstants.RefCusRateTypes.ETradeExemptionCodes, UniversalReferenceConstants.RefCusRateTypes.SpecialConsumptionDuty };
		}
	}
}
