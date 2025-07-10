using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;

namespace Enterprise.MasterFiles.Business
{
	class OSOutstandingAmountValidationWrapper : IOutstandingAmountValidationWrapper
	{
		public OSOutstandingAmountValidationWrapper(AccTransactionHeader parent)
		{
			Parent = parent;
		}

		public ZDecimal OutstandingAmount => Parent.AH_OSOutstandingAmount;

		public ZDecimal TotalAmount => Parent.AH_OSTotal;

		public ZDecimal MatchLinkAmountSum => MatchLinks.Sum(x => x.AP_OSAmount);

		public bool ShouldCheckTransactionHeaderOutstandingAmount => (!Parent.IsInDatabase
				|| Parent.AH_OutstandingAmountInfo.HasChanges
				|| Parent.AH_InvoiceAmountInfo.HasChanges
				|| Parent.AH_GSTAmountInfo.HasChanges
				|| Parent.AH_OSOutstandingAmountInfo.HasChanges
				|| Parent.AH_OSTotalInfo.HasChanges)
				&& Parent.AH_IsOSOutstandingAmountApplicable;

		public bool ShouldCheckMatchLinkOutstandingAmount => (ShouldCheckTransactionHeaderOutstandingAmount || MatchLinks.Any(x => x.HasChanges))
				&& Parent.AH_IsOSOutstandingAmountApplicable;

		public string GetTransactionHeaderErrorMessage()
		{
			return FormattableString.Invariant($"OS Outstanding Amount is incorrect: os outstanding amount = {OutstandingAmount}, os total amount = {TotalAmount}");
		}

		public string GetMatchLinkErrorMessage()
		{
			return FormattableString.Invariant($"OS Outstanding Amount is incorrect: os outstanding amount = {OutstandingAmount}, os total amount = {TotalAmount}, sum ap os amount = {MatchLinkAmountSum}{MatchLinks.GetTransactionHeaderMatchLinkInfosAndUnmatchDeletionInfos()}");
		}

		readonly AccTransactionHeader Parent;

		AccTransactionMatchLink[] MatchLinks => matchLinks ?? (matchLinks = AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(Parent));
		AccTransactionMatchLink[] matchLinks;
	}
}
