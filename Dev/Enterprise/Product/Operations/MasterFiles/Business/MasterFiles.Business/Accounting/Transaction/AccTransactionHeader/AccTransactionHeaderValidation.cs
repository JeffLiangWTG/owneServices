//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransactionHeaderValidation
//
//    This class should be used for overriding validation in AutoAccTransactionHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionHeaderValidation : AutoAccTransactionHeaderValidation
	{
		public AccTransactionHeaderValidation(AutoAccTransactionHeader parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ValidateDataRefreshBusChanges(Parent);
		}

		protected override void CheckAH_RX_NKTransactionCurrency()
		{
			base.CheckAH_RX_NKTransactionCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.AH_RX_NKTransactionCurrencyInfo);
		}

		protected override void CheckAH_ComplianceDocumentDate()
		{
			base.CheckAH_ComplianceDocumentDate();
			if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType && GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Taiwan)
			{
				if (Parent.AH_ComplianceDocumentDate < Parent.AH_InvoiceDate.Date)
				{
					Parent.AH_ComplianceDocumentDateInfo.AddError(Res.GetString("3de1e6b2-e31f-44c6-b592-fe667a73df81", "Compliance doc date must be on or after the invoice date."));
				}
			}
		}

		protected override void CheckAH_ComplianceSubType()
		{
			base.CheckAH_ComplianceSubType();
			if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
			{
				if ((Parent.AH_TransactionType == TransactionTypes.Invoice || Parent.AH_TransactionType == TransactionTypes.CreditNote || Parent.AH_TransactionType == TransactionTypes.AdjustmentNote
					|| Parent.AH_TransactionType == TransactionTypes.IncompleteInvoice || Parent.AH_TransactionType == TransactionTypes.IncompleteCreditNote || Parent.AH_TransactionType == TransactionTypes.IncompleteAdjustmentNote)
					&&
					(Parent.AH_Ledger == LedgerTypes.AccountsPayable || Parent.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || Parent.AH_Ledger == LedgerTypes.IncompleteTransactions
					|| Parent.AH_Ledger == LedgerTypes.AccountsReceivable))
				{
					if (!Parent.AH_ComplianceSubType.IsEmpty)
					{
						bool isPosted = Parent.IsInDatabase && (Parent.AH_Ledger == LedgerTypes.AccountsPayable || Parent.AH_Ledger == LedgerTypes.AccountsReceivable) && !Parent.AH_LedgerInfo.HasChanges;
						bool isPostedValue = isPosted && !Parent.AH_ComplianceSubTypeInfo.HasChanges;

						if (isPostedValue)
						{
							ListValidation.WarnIfInvalidCode(Parent.AH_ComplianceSubTypeInfo);
						}
						else
						{
							ListValidation.ErrorIfInvalidCode(Parent.AH_ComplianceSubTypeInfo);
						}
					}
				}
			}
		}

		protected bool CheckIsComplianceSubTypeValueProtectedAndNotChanged()
		{
			if (Parent.IsInDatabase
				&& !Parent.AH_ComplianceSubTypeInfo.OriginalValue.IsEmpty
				&& Parent.AH_ComplianceSubTypeInfo.OriginalValue.ToString() != Parent.AH_ComplianceSubType
				&& ObjectFactory.Get<ICountryComplianceFactory>().GetIProtectComplianceSubTypeForEInvoicingTransactions(Parent.Company?.GC_RN_NKCountryCode ?? string.Empty) is IProtectComplianceSubTypeForEInvoicingTransactions countryComplianceInfo)
			{
				var errorMessage = countryComplianceInfo.ErrorMessageIfComplianceSubTypeIsProtected(Parent as AccTransactionHeader);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					Parent.AH_ComplianceSubTypeInfo.AddError(errorMessage);
					return true;
				}
			}
			return false;
		}

		protected override void CheckAH_OA_InvoiceAddressOverride()
		{
			base.CheckAH_OA_InvoiceAddressOverride();

			if (Parent.InvoiceAddressOverride != null && Parent.InvoiceAddressOverride.OA_OH != Parent.AH_OH)
			{
				var property = Parent.AH_OA_InvoiceAddressOverrideInfo;
				property.AddError(GetAddressContactErrorMessage(property));
			}
		}

		protected override void CheckAH_OC_InvoiceContactOverride()
		{
			base.CheckAH_OC_InvoiceContactOverride();

			if (Parent.InvoiceContactOverride != null && Parent.InvoiceContactOverride.OC_OH != Parent.AH_OH)
			{
				var property = Parent.AH_OC_InvoiceContactOverrideInfo;
				property.AddError(GetAddressContactErrorMessage(property));
			}
		}

		protected virtual bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get
			{
				return false;
			}
		}

		protected virtual bool ShouldValidateBranchDepartmentCombination
		{
			get
			{
				return true;
			}
		}

		protected virtual INotificationType NotificationTypeForBranchDepartmentCombination
		{
			get
			{
				return CargoWise.EntityFramework.NotificationType.Error;
			}
		}

		protected override void CheckAH_GE()
		{
			base.CheckAH_GE();

			if (ShouldValidateBranchDepartmentCombination)
			{
				if ((ShouldValidateBranchDepartmentCombinationForParentInDatabase || !Parent.IsInDatabase)
					|| Parent.AH_GBInfo.HasChanges || Parent.AH_GEInfo.HasChanges)
				{
					GlbBranchCombinationValidation.CheckBranchDepartmentCombination(Parent.AH_GEInfo, Parent.Branch, Parent.Department, NotificationTypeForBranchDepartmentCombination);
				}
			}
		}

		string GetAddressContactErrorMessage(ZPropertyInfo property)
		{
			return Res.GetString("0da93482-9afb-45dc-b09e-093679b99aa6", "The {0} must belong to the {1}.", property.HumanReadableName, Parent.AH_OHInfo.HumanReadableName);
		}
	}
}
