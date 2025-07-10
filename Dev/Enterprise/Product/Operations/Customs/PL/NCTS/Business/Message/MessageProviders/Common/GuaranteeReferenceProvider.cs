using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class GuaranteeReferenceProvider : IGuaranteeReference
{
	public GuaranteeReferenceProvider(int sequenceNumber, NctsGuarantee nctsGuarantee)
	{
		this.nctsGuarantee = Argument.NotNull(nctsGuarantee, nameof(nctsGuarantee));
		SequenceNumber = sequenceNumber.ToString();
	}
	readonly NctsGuarantee nctsGuarantee;

	public string SequenceNumber { get; }

	public string GRN => CachedValueHelper.GetValue(ref grn, () => RuleC0086
		? nctsGuarantee.PW_BondNumber
		: null);
	CachedValue<string> grn;

	public string AccessCode => CachedValueHelper.GetValue(ref accessCode, () => RuleC0086
		? nctsGuarantee.PW_Password
		: null);
	CachedValue<string> accessCode;

	public decimal? AmountToBeCovered => nctsGuarantee.PW_BondAmount;

	public string Currency => CachedValueHelper.GetValue(ref currency, () => AmountToBeCovered > 0
		? nctsGuarantee.PW_RX_NKCurrency
		: null);
	CachedValue<string> currency;

	bool RuleC0086 => CachedValueHelper.GetValue(ref ruleC0086, () => CheckRuleC0086());
	CachedValue<bool> ruleC0086;

	bool CheckRuleC0086()
	{
		var bondType = nctsGuarantee.PW_BondType;
		return bondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver
				|| bondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee
				|| bondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor
				|| bondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee
				|| bondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher
				|| bondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur
				|| bondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage;
	}
}
