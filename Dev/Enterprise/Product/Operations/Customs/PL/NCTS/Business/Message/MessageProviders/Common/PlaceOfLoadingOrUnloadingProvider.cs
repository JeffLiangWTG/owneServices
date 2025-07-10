using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.NCTS.Business;

public class PlaceOfLoadingOrUnloadingProvider : IPlaceOfLoadingOrUnloading
{
	PlaceOfLoadingOrUnloadingProvider(ZString unlocode, ZString location, BusinessObjectFactory factory, bool isInPhase5TransitionPeriod)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.baseUnlocode = unlocode;
		this.location = location;
		this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
	}
	readonly ZString baseUnlocode;
	readonly BusinessObjectFactory factory;
	readonly ZString location;
	readonly bool isInPhase5TransitionPeriod;

	public static PlaceOfLoadingOrUnloadingProvider NewOrNull(ZString unlocode, ZString location, BusinessObjectFactory factory, bool isInPhase5TransitionPeriod) => !unlocode.IsEmpty || !location.IsEmpty
		? new PlaceOfLoadingOrUnloadingProvider(unlocode, location, factory, isInPhase5TransitionPeriod)
		: null;

	public string UNLocode => CachedValueHelper.GetValue(ref unlocode, () => baseUnlocode.Length == 5 ? baseUnlocode : null);
	CachedValue<string> unlocode;

	public string Country => CachedValueHelper.GetValue(ref country, () => Unloco?.RL_RN_NKCountryCode
																			?? (!location.IsEmpty ? baseUnlocode.Left(2) : null));
	CachedValue<string> country;

	public string Location => location;

	RefUNLOCO Unloco => CachedValueHelper.GetValue(ref unloco, () => factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, baseUnlocode));

	public int LocationMaxLength => CachedValueHelper.GetValue(ref locationMaxLength, () =>
	{
		var maxLengthInTransitionPeriod = 17;
		var maxLengthOutsideTransitionPeriod = 35;
		return isInPhase5TransitionPeriod
			? maxLengthInTransitionPeriod
			: maxLengthOutsideTransitionPeriod;
	});
	CachedValue<int> locationMaxLength;

	CachedValue<RefUNLOCO> unloco;
}
