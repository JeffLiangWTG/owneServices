using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICusLineTariffDetailParent : IAdditionalLineTariffDetailParent, ITariffProvider
	{
		ZString CustomsCountryCode { get; }

		ZDateTime EffectiveAssessmentDate { get; }
	}
}
