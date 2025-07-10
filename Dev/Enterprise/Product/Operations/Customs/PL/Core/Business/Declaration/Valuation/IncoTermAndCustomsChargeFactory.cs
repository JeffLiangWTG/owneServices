using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class IncoTermAndCustomsChargeFactory : EUIncoTermAndCustomsChargeFactory
{
	public IncoTermAndCustomsChargeFactory() { }

	protected override ICustomsChargeCode[] GetCharges()
	{
		return ChargesProvider.Codes;
	}

	protected override void SetupIncotermChargeConfigurations()
	{
		ChargesProvider provider = new ChargesProvider();
		foreach (ICustomsChargeCode code in ChargesProvider.Codes)
		{
			foreach (string incoTerm in ChargesProvider.ConfiguredIncoTerms)
			{
				AddChargeConfiguration(incoTerm, code, provider.GetChargeConfiguration(incoTerm, code));
			}
		}
	}

	protected override void SetupErrorConfiguration()
	{
	}

	protected override string FreightToEUBorderCodeCore => PLCustomsChargeTypeList.Codes.AK;
	protected override MultilingualString FreightToEUBorderDesc => PLCustomsChargeTypeList.Descriptions.AK;

	protected override string FreightAfterEUBorderCodeCore => PLCustomsChargeTypeList.Codes._071V;
	protected override MultilingualString FreightAfterEUBorderDesc => PLCustomsChargeTypeList.Descriptions._071V;

	public override void SetupAfterEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
	{
		base.SetupAfterEUBorderCharge(charge, amount, currency);
		charge.J7_IsStatisticalValueApplicable = false;
	}
}
