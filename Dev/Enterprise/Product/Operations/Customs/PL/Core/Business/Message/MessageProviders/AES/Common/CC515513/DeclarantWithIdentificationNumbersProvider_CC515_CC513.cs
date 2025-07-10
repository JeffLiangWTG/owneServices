using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

sealed class DeclarantWithIdentificationNumbersProvider_CC515_CC513 : AESDeclarantWithIdentificationNumbersProvider
{
	readonly bool isAesTransitionPeriod;

	DeclarantWithIdentificationNumbersProvider_CC515_CC513(OrgAddress orgAddress, JobDeclaration jobDeclaration, bool isAesTransitionPeriod) : base(orgAddress, jobDeclaration)
	{
		this.isAesTransitionPeriod = isAesTransitionPeriod;
	}

	public static DeclarantWithIdentificationNumbersProvider_CC515_CC513 NewOrNull(OrgAddress orgAddress, JobDeclaration jobDeclaration, bool isAesTransitionPeriod) => orgAddress?.Header is not null
		? new DeclarantWithIdentificationNumbersProvider_CC515_CC513(orgAddress, jobDeclaration, isAesTransitionPeriod)
		: null;

	protected override string GetNameCore() => AesRuleHelper.ApplyE1104Rule(base.GetNameCore(), isAesTransitionPeriod);

	protected override IAddress GetAddressCore() => new AddressProvider_CC515_CC513(orgAddress, isAesTransitionPeriod);
}
