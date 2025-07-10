using System;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.CreditControl.Business
{
	public static class CreditLimitBalanceExtensions
	{
		#region SuppressResourceStringsCheckRegion

		public static string GetAsString(this IOrgCreditLimitAndBalanceDetails balanceDetails, string ledger)
		{
			var detailsBuilder = new ZStringBuilder();

			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.UnableToCalculateARGlobalUnpostedRevenue)}: {balanceDetails.UnableToCalculateARGlobalUnpostedRevenue}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.ARGlobalUnpostedRevenueRecognised)}: {balanceDetails.ARGlobalUnpostedRevenueRecognised}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.UnableToCalculateARGlobalOutstandingBalance)}: {balanceDetails.UnableToCalculateARGlobalOutstandingBalance}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.ARGlobalOutstandingBalance)}: {balanceDetails.ARGlobalOutstandingBalance}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.ARGlobalClaim)}: {balanceDetails.ARGlobalClaim}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.ARGlobalCreditLimit)}: {balanceDetails.ARGlobalCreditLimit}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.ARGlobalCreditCurrencyCode)}: {balanceDetails.ARGlobalCreditCurrencyCode}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.IsOverARGlobalCreditLimit)}: {balanceDetails.IsOverARGlobalCreditLimit}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.OnARGlobalCreditHold)}: {balanceDetails.OnARGlobalCreditHold}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.IsARGlobalCreditApproved)}: {balanceDetails.IsARGlobalCreditApproved}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.CompanyCode)}: {balanceDetails.CompanyCode}"));

			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.ARGlobalUnpostedRevenueUnrecognised)}: {balanceDetails.ARGlobalUnpostedRevenueUnrecognised}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.OrgCode)}: {balanceDetails.OrgCode}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.IsARGlobalCreditApproved)}: {balanceDetails.IsARGlobalCreditApproved}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.CompanyCode)}: {balanceDetails.CompanyCode}"));

			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.CreditLimit)}: {balanceDetails.CreditLimit(ledger)}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.IsOverCreditLimit)}: {balanceDetails.IsOverCreditLimit(ledger)}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.IsOverCreditTerms)}: {balanceDetails.IsOverCreditTerms(ledger)}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.OnCreditHold)}: {balanceDetails.OnCreditHold(ledger)}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.OutstandingBalance)}: {balanceDetails.OutstandingBalance(ledger)}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.Claim)}: {balanceDetails.Claim(ledger)}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.OutstandingBalanceNotOverdue)}: {balanceDetails.OutstandingBalanceNotOverdue(ledger)}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.OutstandingBalanceOverdue)}: {balanceDetails.OutstandingBalanceOverdue(ledger)}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.UnpostedRevenue)}: {balanceDetails.UnpostedRevenue(ledger)}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.UnpostedRevenueRecognised)}: {balanceDetails.UnpostedRevenueRecognised(ledger)}"));
			detailsBuilder.Append(FormattableString.Invariant($".{nameof(balanceDetails.UnpostedRevenueUnrecognised)}: {balanceDetails.UnpostedRevenueUnrecognised(ledger)}"));

			return detailsBuilder.ToStringWithNewLineBetweenAppends();
		}

		#endregion
	}
}
