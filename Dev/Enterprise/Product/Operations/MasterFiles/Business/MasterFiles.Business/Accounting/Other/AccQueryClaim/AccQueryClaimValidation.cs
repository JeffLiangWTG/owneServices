//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccQueryClaimValidation
//
//    This class should be used for overriding validation in AutoAccQueryClaimValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccQueryClaimValidation : AutoAccQueryClaimValidation
	{
		public AccQueryClaimValidation(AutoAccQueryClaim parent)
			: base(parent)
		{
		}

		#region Validate

		protected override void CheckAY_ShortDescriptionOfClaim()
		{
			base.CheckAY_ShortDescriptionOfClaim();
			MandatoryValidation.CheckEntered(Parent.AY_ShortDescriptionOfClaimInfo);
		}

		protected override void CheckAY_GB()
		{
			base.CheckAY_GB();
			ListValidation.ErrorIfInvalidPK(Parent.AY_GBInfo);
		}

		protected override void CheckAY_OH_Debtor()
		{
			base.CheckAY_OH_Debtor();
			ListValidation.ErrorIfInvalidPK(Parent.AY_OH_DebtorInfo);
		}

		protected override void CheckAY_GS_NKStaffAssignedTo()
		{
			base.CheckAY_GS_NKStaffAssignedTo();
			ListValidation.ErrorIfInvalidCode(Parent.AY_GS_NKStaffAssignedToInfo);
		}

		protected override void CheckAY_OC()
		{
			base.CheckAY_OC();
			ListValidation.ErrorIfInvalidPK(Parent.AY_OCInfo);
		}

		protected override void CheckAY_AH()
		{
			base.CheckAY_AH();

			if (Parent.AY_QueryClaimStatus == QueryClaimStatusCodeList.Codes.QCStatus3AcceptedAndCreditNoteIssuedAndClosed
				|| Parent.AY_QueryClaimStatus == QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed
				|| Parent.AY_QueryClaimStatus == QueryClaimStatusCodeList.Codes.QCStatus6CancelledAndClosed)
			{
				return;
			}

			if (!Parent.IsInDatabase || !Parent.AY_AHInfo.OriginalValue.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.AY_AHInfo);
			}

			var transactionHeaderQuery = new ZQuery(AccTransactionHeaderSchema.PK, Parent.AY_AH);
			transactionHeaderQuery.FetchOnlyFromLocalCache = true;
			var transactionHeader = Parent.Factory.LoadTop1<AccTransactionHeader>(transactionHeaderQuery);
			if (transactionHeader == null || transactionHeader.IsInDatabase)
			{
				ListValidation.ErrorIfInvalidPK(Parent.AY_AHInfo, Parent.Lookups.TransactionHeaders, GetErrorWhenTransactionHeadersAdditionalFilterNotMet);
			}
			else if (!(Parent as AccQueryClaim).IsCreatedFromCASS)
			{
				var claimFilter = Parent.Lookups.GetClaimQueryForTransactionUniqueFilter();
				claimFilter.AddToFilter(AccQueryClaimSchema.AY_AH, transactionHeader.PK);
				claimFilter.FetchOnlyFromLocalCache = true;
				var claim = Parent.Factory.LoadTop1(Parent.GetType(), claimFilter);
				if (claim != null)
				{
					Parent.AY_AHInfo.AddError(GetErrorWhenTransactionHeadersAdditionalFilterNotMet);
				}
			}
		}

		public MultilingualString GetErrorWhenTransactionHeadersAdditionalFilterNotMet
		{
			get { return ResString.GetMultilingualString("d4651e24-c9dd-49c9-a428-0ef7525cd31e", "Each transaction can only be associated with one claim.  This transaction cannot be chosen because it is already linked to a Claim."); }
		}

		protected override void CheckAY_QueryClaimReasonCode()
		{
			base.CheckAY_QueryClaimReasonCode();
			MandatoryValidation.CheckEntered(Parent.AY_QueryClaimReasonCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AY_QueryClaimReasonCodeInfo);
		}

		protected override void CheckAY_QueryClaimStatus()
		{
			base.CheckAY_QueryClaimStatus();
			MandatoryValidation.CheckEntered(Parent.AY_QueryClaimStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AY_QueryClaimStatusInfo);
		}

		protected override void CheckAY_QueryClaimType()
		{
			base.CheckAY_QueryClaimType();
			MandatoryValidation.CheckEntered(Parent.AY_QueryClaimTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AY_QueryClaimTypeInfo);
		}

		protected override void CheckAY_HoldOption()
		{
			base.CheckAY_HoldOption();
			MandatoryValidation.CheckEntered(Parent.AY_HoldOptionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AY_HoldOptionInfo);
		}

		protected override void CheckAY_QueryClaimAmount()
		{
			base.CheckAY_QueryClaimAmount();

			if (!Parent.IsInDatabase || Parent.AY_QueryClaimAmountInfo.HasChanges)
			{
				MandatoryValidation.CheckNotZero(Parent.AY_QueryClaimAmountInfo);
			}
		}

		#endregion
	}
}
