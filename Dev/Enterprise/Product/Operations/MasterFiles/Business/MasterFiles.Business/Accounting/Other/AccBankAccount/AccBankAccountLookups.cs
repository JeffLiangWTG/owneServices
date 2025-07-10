//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccBankAccountLookups
//
//    This class should be used for overriding collections in AutoAccBankAccountLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccBankAccountLookups : AutoAccBankAccountLookups
	{
		public AccBankAccountLookups(AutoAccBankAccount parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList PaymentProviderCodeList => EPaymentProviderCodes.CodesList;

		#region AccountTypes

		public virtual AccountTypeCodeDescriptionPairList AccountTypes =>
			GetBankAccountTypesList((Parent as AccBankAccount).Company ?? GlbCompany.CurrentCompany, (Parent as AccBankAccount)?.AB_AccountType);

		public static AccountTypeCodeDescriptionPairList GetBankAccountTypesList(GlbCompany company = null, string accountType = "")
		{
			var result = new AccountTypeCodeDescriptionPairList();
			switch (accountType)
			{
				case AccountTypeCodeDescriptionPairList.Codes.CSH:
					result.RemoveCode(AccountTypeCodeDescriptionPairList.Codes.BNK);
					result.RemoveCode(AccountTypeCodeDescriptionPairList.Codes.CCD);
					result.RemoveCode(AccountTypeCodeDescriptionPairList.Codes.EPA);
					result.RemoveCode(AccountTypeCodeDescriptionPairList.Codes.LNK);
					break;

				default:
					result = new AccountTypeCodeDescriptionPairList();
					if (!AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.GetValueWithoutFallback((company ?? GlbCompany.CurrentCompany).PK.ToGuid(), Guid.Empty, Guid.Empty).IsEPaymentEnabledForAnyProvider)
					{
						result.RemoveCode(AccountTypeCodeDescriptionPairList.Codes.EPA);
					}
					if (!string.IsNullOrEmpty(accountType))
					{
						result.RemoveCode(AccountTypeCodeDescriptionPairList.Codes.CSH);
					}
					break;
			}
			return result;
		}

		#endregion

		#region Months

		public virtual CodeDescriptionPairList Months
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Months); }
		}
		#endregion
	}
}
