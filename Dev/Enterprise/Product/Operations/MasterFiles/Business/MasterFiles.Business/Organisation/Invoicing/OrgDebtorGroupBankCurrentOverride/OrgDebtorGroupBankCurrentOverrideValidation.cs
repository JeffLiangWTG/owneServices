//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgDebtorGroupBankCurrentOverrideValidation
//
//    This class should be used for overriding validation in AutoOrgDebtorGroupBankCurrentOverrideValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDebtorGroupBankCurrentOverrideValidation : AutoOrgDebtorGroupBankCurrentOverrideValidation
	{
		public OrgDebtorGroupBankCurrentOverrideValidation(AutoOrgDebtorGroupBankCurrentOverride parent) : base(parent)
		{
		}

		protected override void CheckPB_RX_NKCurrency()
		{
			base.CheckPB_RX_NKCurrency();
			MandatoryValidation.CheckEntered(Parent.PB_RX_NKCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PB_RX_NKCurrencyInfo, new OrgDebtorGroupBankCurrentOverrideLookups(Parent).CurrencyList);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.PB_RX_NKCurrencyInfo, Res.GetString("b2ed3c01-cacb-45ea-905e-2600f96514f3", "There must be only one line for each currency."));
		}
	}
}
