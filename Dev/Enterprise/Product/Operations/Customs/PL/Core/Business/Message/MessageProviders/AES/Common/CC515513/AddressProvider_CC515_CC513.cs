using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.PL.Business;

sealed class AddressProvider_CC515_CC513(IDocAddress docAddress, bool isAesTransitionPeriod) : AddressProvider(docAddress)
{
	protected override string GetStreetAndNumberCore() => AesRuleHelper.ApplyE1104Rule(base.GetStreetAndNumberCore(), isAesTransitionPeriod);
}
