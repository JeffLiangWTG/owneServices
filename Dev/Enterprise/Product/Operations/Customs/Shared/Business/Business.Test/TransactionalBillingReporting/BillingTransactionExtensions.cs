using System;
using Enterprise.Billing.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.TransactionalBillingReporting.Testing
{
	public static class BillingTransactionExtensions
	{
		public static bool HasAnyReferenceContaining(this BillingTransaction t, string reference)
		{
			return (t.Reference1 != null && StringExtension.Contains(t.Reference1, reference, StringComparison.InvariantCultureIgnoreCase))
				|| (t.Reference2 != null && StringExtension.Contains(t.Reference2, reference, StringComparison.InvariantCultureIgnoreCase))
				|| (t.Reference3 != null && StringExtension.Contains(t.Reference3, reference, StringComparison.InvariantCultureIgnoreCase))
				|| (t.Reference4 != null && StringExtension.Contains(t.Reference4, reference, StringComparison.InvariantCultureIgnoreCase))
				|| (t.Reference5 != null && StringExtension.Contains(t.Reference5, reference, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}
