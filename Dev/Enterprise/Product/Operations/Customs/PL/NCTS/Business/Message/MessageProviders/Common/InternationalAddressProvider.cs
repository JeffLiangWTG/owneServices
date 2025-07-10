using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class InternationalAddressProvider : IInternationalAddress
{
	InternationalAddressProvider(OrgAddress address, bool useMaxLengthWithDependency, bool isInPhase5TransitionPeriod)
	{
		this.address = Argument.NotNull(address, nameof(address));
		this.useMaxLengthWithDependency = useMaxLengthWithDependency;
		this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
	}

	readonly OrgAddress address;
	readonly bool useMaxLengthWithDependency;
	readonly bool isInPhase5TransitionPeriod;

	public static InternationalAddressProvider NewOrNull(OrgAddress address, bool useMaxLengthWithDependency = true, bool isInPhase5TransitionPeriod = false) => address != null
		? new InternationalAddressProvider(address, useMaxLengthWithDependency, isInPhase5TransitionPeriod)
		: null;

	public string StreetAndNumber => CachedValueHelper.GetValue(ref streetAndNumber, () => address.OA_Address1 + " " + address.OA_Address2);
	CachedValue<string> streetAndNumber;

	public string Postcode => address.OA_PostCode;

	public string City => address.OA_City;

	public string Country => address.OA_RN_NKCountryCode;

	public int StreetAndNumberMaxLength => CachedValueHelper.GetValue(ref streetAndNumberMaxLength, () =>
	{
		var maxLengthInTransitionPeriod = 35;
		var maxLengthOutsideTransitionPeriod = 70;
		return useMaxLengthWithDependency && isInPhase5TransitionPeriod
			? maxLengthInTransitionPeriod
			: maxLengthOutsideTransitionPeriod;
	});
	CachedValue<int> streetAndNumberMaxLength;
}
