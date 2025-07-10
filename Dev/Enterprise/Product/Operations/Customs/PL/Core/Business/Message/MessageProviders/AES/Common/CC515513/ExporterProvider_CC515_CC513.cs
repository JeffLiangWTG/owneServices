using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public sealed class ExporterProvider_CC515_CC513 : AESExporterProvider
{
	readonly bool isAesTransitionPeriod;

	ExporterProvider_CC515_CC513(OrgAddress orgAddress, bool isAesTransitionPeriod) : base(orgAddress)
	{
		this.isAesTransitionPeriod = isAesTransitionPeriod;
	}

	public static ExporterProvider_CC515_CC513 NewOrNull(OrgAddress orgAddress, bool isAesTransitionPeriod) => orgAddress?.Header is not null
		? new ExporterProvider_CC515_CC513(orgAddress, isAesTransitionPeriod)
		: null;

	protected override string GetNameCore() => AesRuleHelper.ApplyE1104Rule(base.GetNameCore(), isAesTransitionPeriod);

	protected override IAddress GetAddressCore() => new AddressProvider_CC515_CC513(orgAddress, isAesTransitionPeriod);
}
