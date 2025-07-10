using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;

namespace Enterprise.MasterFiles.Business
{
	class LocalOutstandingAmountValidationWrapper : IOutstandingAmountValidationWrapper
	{
		public LocalOutstandingAmountValidationWrapper(AccTransactionHeader parent)
		{
			Parent = parent;
		}

		public ZDecimal OutstandingAmount => Parent.AH_OutstandingAmount;

		public ZDecimal TotalAmount => Parent.AH_LocalTotal;

		public ZDecimal MatchLinkAmountSum => MatchLinks.Sum(x => x.AP_Amount);

		public bool ShouldCheckTransactionHeaderOutstandingAmount => !Parent.IsInDatabase
				|| Parent.AH_OutstandingAmountInfo.HasChanges
				|| Parent.AH_InvoiceAmountInfo.HasChanges
				|| Parent.AH_GSTAmountInfo.HasChanges;

		public bool ShouldCheckMatchLinkOutstandingAmount => ShouldCheckTransactionHeaderOutstandingAmount || MatchLinks.Any(x => x.HasChanges);

		public string GetTransactionHeaderErrorMessage()
		{
			return FormattableString.Invariant($"Outstanding Amount is incorrect: outstanding amount = {OutstandingAmount}, local total amount = {TotalAmount}");
		}

		public string GetMatchLinkErrorMessage()
		{
			return FormattableString.Invariant($"Outstanding Amount is incorrect: outstanding amount = {OutstandingAmount}, local total amount = {TotalAmount}, sum ap amount = {MatchLinkAmountSum}{MatchLinks.GetTransactionHeaderMatchLinkInfosAndUnmatchDeletionInfos()}");
		}

		readonly AccTransactionHeader Parent;

		AccTransactionMatchLink[] MatchLinks => matchLinks ?? (matchLinks = AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(Parent));
		AccTransactionMatchLink[] matchLinks;
	}
}
