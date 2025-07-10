using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class PaymentTermOverrideComparer : BaseRateLineComparer
	{
		public override int Compare(FastLine line1, FastLine line2)
		{
			var entry1 = line1.ParentRateEntry;
			var entry2 = line2.ParentRateEntry;

			if
				(
					(entry1.IsIntercompanyTariff() || entry1.IsCompanyTariff() || entry1.IsClientRate())
					&& (entry2.IsIntercompanyTariff() || entry2.IsCompanyTariff() || entry2.IsClientRate())
				)
			{
				if (!entry1.TI_PaymentTerm.IsEmpty && entry2.TI_PaymentTerm.IsEmpty)
				{
					return 1;
				}

				if (entry1.TI_PaymentTerm.IsEmpty && !entry2.TI_PaymentTerm.IsEmpty)
				{
					return -1;
				}
			}

			return 0;
		}

		protected override string GetName()
		{
			return (NoResString)"Payment Term Override"; // log message, subject to change, more for support people as of now
		}
	}
}
