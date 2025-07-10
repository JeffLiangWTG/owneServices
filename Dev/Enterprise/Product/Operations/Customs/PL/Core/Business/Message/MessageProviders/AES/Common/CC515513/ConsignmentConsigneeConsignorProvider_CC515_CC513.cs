using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public sealed class ConsignmentConsigneeConsignorProvider_CC515_CC513 : AESConsignmentConsigneeConsignorProvider
{
	readonly bool isAesTransitionPeriod;

	ConsignmentConsigneeConsignorProvider_CC515_CC513(JobDocAddress docAddress, bool isAesTransitionPeriod) : base(docAddress)
	{
		this.isAesTransitionPeriod = isAesTransitionPeriod;
	}

	public static ConsignmentConsigneeConsignorProvider_CC515_CC513 NewOrNull(JobDocAddress docAddress, bool isAesTransitionPeriod) => docAddress?.Address?.Header is not null
		? new ConsignmentConsigneeConsignorProvider_CC515_CC513(docAddress, isAesTransitionPeriod)
		: null;

	protected override string GetNameCore() => AesRuleHelper.ApplyE1104Rule(base.GetNameCore(), isAesTransitionPeriod);

	protected override IAddress GetAddressCore() => new AddressProvider_CC515_CC513(docAddress, isAesTransitionPeriod);
}
