using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class ConsigneeProvider : IConsignee
{
	public ConsigneeProvider(JobDocAddress docAddress, bool isInPhase5TransitionPeriod)
	{
		DocAddress = Argument.NotNull(docAddress, nameof(docAddress));
		OrgAddress = DocAddress.Address;
		OrgHeader = OrgAddress?.Header;
		this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
	}
	protected readonly OrgHeader OrgHeader;
	protected readonly OrgAddress OrgAddress;
	protected readonly JobDocAddress DocAddress;
	readonly bool isInPhase5TransitionPeriod;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => EuEoriResolver.GetRegNoWithCountryCode(OrgHeader));
	CachedValue<string> identificationNumber;

	public string Name => CachedValueHelper.GetValue(ref name, () => string.IsNullOrEmpty(IdentificationNumber) ? OrgHeader?.OH_FullName : null);
	CachedValue<string> name;

	public IInternationalAddress Address => CachedValueHelper.GetValue(ref address, () => string.IsNullOrEmpty(IdentificationNumber)
		? InternationalAddressProvider.NewOrNull(OrgAddress, isInPhase5TransitionPeriod: isInPhase5TransitionPeriod)
		: null);
	CachedValue<IInternationalAddress> address;

	public int NameMaxLength => CachedValueHelper.GetValue(ref nameMaxLength, () =>
	{
		var maxLengthInTransitionPeriod = 35;
		var maxLengthOutsideTransitionPeriod = 70;
		return isInPhase5TransitionPeriod
			? maxLengthInTransitionPeriod
			: maxLengthOutsideTransitionPeriod;
	});
	CachedValue<int> nameMaxLength;
}
