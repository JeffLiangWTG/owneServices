//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransactionMatchLinkValidation
//
//    This class should be used for overriding validation in AutoAccTransactionMatchLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionMatchLinkValidation : AutoAccTransactionMatchLinkValidation
	{
		public AccTransactionMatchLinkValidation(AutoAccTransactionMatchLink parent) : base(parent)
		{
		}

		public sealed override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			ValidateAllCore();
		}

		protected virtual void ValidateAllCore()
		{
			base.ValidateAll();
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == AccTransactionMatchLinkSchema.Constants.AP_AH && Parent.TransactionHeader != null)
			{
				if ((Parent.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable || Parent.TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable) &&
					Parent.TransactionHeader.AH_TransactionType == TransactionTypes.Discount || Parent.TransactionHeader.AH_TransactionType == TransactionTypes.Journal ||
					Parent.TransactionHeader.AH_TransactionType == TransactionTypes.ExchangeDifference || Parent.TransactionHeader.AH_TransactionType == TransactionTypes.Overpayment)
				{
					return false;
				}
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}

		protected override void CheckAP_MatchDateIsNotEmpty()
		{
			if (!((AccTransactionMatchLink)Parent).MatchDateIsNotEmptyValidationSuspender.IsSuspended)
			{
				base.CheckAP_MatchDateIsNotEmpty();
			}
		}
	}
}
