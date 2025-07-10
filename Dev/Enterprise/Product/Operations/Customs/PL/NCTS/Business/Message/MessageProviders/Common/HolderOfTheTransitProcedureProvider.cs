using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class HolderOfTheTransitProcedureProvider : IHolderOfTheTransitProcedureWithMaxLength
{
	public HolderOfTheTransitProcedureProvider(JobDocAddress docAddress, NctsDepartureMovementHeader movementHeader, bool useMaxLengthWithDependency = true)
	{
		Argument.NotNull(docAddress, nameof(docAddress));
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		orgAddress = docAddress.Address;
		orgHeader = orgAddress?.Header;
		this.useMaxLengthWithDependency = useMaxLengthWithDependency;
	}
	readonly OrgHeader orgHeader;
	readonly OrgAddress orgAddress;
	readonly NctsDepartureMovementHeader movementHeader;
	readonly bool useMaxLengthWithDependency;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => orgHeader != null
		? EuEoriResolver.GetRegNoWithCountryCode(orgHeader)
		: null);
	CachedValue<string> identificationNumber;

	public string TIRHolderIdentificationNumber => CachedValueHelper.GetValue(ref tirHolderIdentificationNumber, GetTIRHolderIdentificationNumber);
	CachedValue<string> tirHolderIdentificationNumber;

	public string Name => CachedValueHelper.GetValue(ref name, () => orgHeader == null || !string.IsNullOrEmpty(IdentificationNumber)
		? null
		: orgAddress?.OA_CompanyNameOverride.IsEmpty ?? true
			? orgHeader.OH_FullName
			: orgAddress?.OA_CompanyNameOverride);
	CachedValue<string> name;

	public int NameMaxLength => CachedValueHelper.GetValue(ref nameMaxLength, () =>
	{
		var maxLengthInTransitionPeriod = 35;
		var maxLengthOutsideTransitionPeriod = 70;
		return useMaxLengthWithDependency && IsInPhase5TransitionPeriod
			? maxLengthInTransitionPeriod
			: maxLengthOutsideTransitionPeriod;
	});
	CachedValue<int> nameMaxLength;

	public IInternationalAddress Address => CachedValueHelper.GetValue(ref address, ()
		=> string.IsNullOrEmpty(IdentificationNumber)
			? InternationalAddressProvider.NewOrNull(orgAddress, useMaxLengthWithDependency, IsInPhase5TransitionPeriod)
			: null);
	CachedValue<IInternationalAddress> address;

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson, () => ContactPersonProvider.NewOrNull(orgHeader));

	CachedValue<IContactPerson> contactPerson;

	string GetTIRHolderIdentificationNumber()
	{
		var result = (string)null;
		if (orgHeader != null && CheckRuleC0904())
		{
			var tirRegNo = orgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers);
			result = tirRegNo.IsEmpty ? EuEoriResolver.GetRegNoWithCountryCode(orgHeader) : tirRegNo;
		}
		return result;
	}

	bool CheckRuleC0904() => movementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.TIR;

	bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => movementHeader.IsInPhase5TransitionPeriod);
	CachedValue<bool> isInPhase5TransitionPeriod;
}
