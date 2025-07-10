using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionMatchLinkCriticalValidation : CriticalValidation<AccTransactionMatchLink>
	{
		public AccTransactionMatchLinkCriticalValidation(AccTransactionMatchLink parent) : base(parent)
		{
		}

		#region Check Grouping Methods

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			if (Parent.HasChanges)
			{
				if (Parent.IsInDatabase)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.SavedMachLinkCannotBeModified_1,
														CriticalValidationMessageTemplate.SavedMachLinkCannotBeModifiedErrorMessage,
														Parent.GetMatchLinkInfo());
				}

				BusinessObjectCollection[] parentCollections = ((IBusinessObjectInternals)Parent).ParentCollections;

				bool isInMatchingGroup = false;
				foreach (BusinessObjectCollection parentCollection in parentCollections)
				{
					isInMatchingGroup = parentCollection is ISupportCriticalValidation;
					if (isInMatchingGroup)
					{
						break;
					}
				}

				if (!isInMatchingGroup)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.MatchLinkIsNotAMemberOfMatchGroup_1,
														CriticalValidationMessageTemplate.MatchLinkIsNotAMemberOfMatchGroupErrorMessage,
														Parent.GetMatchLinkInfo());
				}
			}

			if (Parent.AP_MatchGroupNum.IsEmpty)
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.MatchLinkWithoutGroupNumber_1,
													CriticalValidationMessageTemplate.MatchLinkWithoutGroupNumberErrorMessage,
													Parent.GetMatchLinkInfo());
			}

			yield return CheckLinkedTransactionHasChanged();

			foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)Parent).ParentCollections)
			{
				ISupportCriticalValidation collectionWithValidation = collection as ISupportCriticalValidation;
				if (collectionWithValidation != null)
				{
					collectionWithValidation.CriticalValidation.RunOnSavingCheck();
				}
			}
		}

		CriticalValidationResult CheckLinkedTransactionHasChanged()
		{
			var linkedTransaction = GetLinkedTransaction();
			if (linkedTransaction != null
				&& (!linkedTransaction.IsInDatabase
					|| ((INeedRow)linkedTransaction).Row.RowState == System.Data.DataRowState.Modified  // To check if transaction is going to be saved and will have own critical validation checks ran
					|| (Parent.AP_Amount.IsEmpty && linkedTransaction.AH_OutstandingAmount.IsEmpty && (linkedTransaction.AH_InvoiceAmount + linkedTransaction.AH_GSTAmount) == 0m)))    // Matching transaction with 0 total
			{
				return null;
			}
			else
			{
				var linkedTransactionDetails = (NoResString)"\r\nLinked Transaction Info :\r\n" + (linkedTransaction == null ? (NoResString)"Linked Transaction was not in local cache." : linkedTransaction.GetTransactionHeaderInfo());
				return new CriticalValidationResult(CriticalValidationErrorType.MatchLinkLinkedTransactionHasNotChanged_1,
														CriticalValidationMessageTemplate.MatchLinkLinkedTransactionHasNotChanged,
														Parent.GetMatchLinkInfo(),
														linkedTransactionDetails);
			}
		}

		#endregion

		#region Common Methods

		AccTransactionHeader GetLinkedTransaction()
		{
			var linkedTransactionQuery = new ZQuery(AccTransactionHeaderSchema.PK, Parent.AP_AH) { FetchOnlyFromLocalCache = true };
			return Parent.Factory.LoadTop1<AccTransactionHeader>(linkedTransactionQuery);
		}

		#endregion
	}
}
