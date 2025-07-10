using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class ControllingCustomerComparer : ColumnComparer
	{
		public ControllingCustomerComparer(RatingCriteria criteria) : base(RateEntrySchema.TI_OH_ControllingCustomer)
		{
			this.criteria = criteria;
		}

		readonly RatingCriteria criteria;

		public override int Compare(FastLine line1, FastLine line2)
		{
			if (line1.IsIntercompanyTariff() && line2.IsIntercompanyTariff())
			{
				var currentEntryControllerCustomerOrder = GetControllerCustomer(line1.ParentRateEntry);
				var overridenEntryControllerCustomerOrder = GetControllerCustomer(line2.ParentRateEntry);

				if (currentEntryControllerCustomerOrder > overridenEntryControllerCustomerOrder)
				{
					return -1;
				}

				if (currentEntryControllerCustomerOrder < overridenEntryControllerCustomerOrder)
				{
					return 1;
				}
			}

			return base.Compare(line1, line2);
		}

		int GetControllerCustomer(IRateEntry entry)
		{
			if (criteria.SortedControllingCustomerPKs.Contains(entry.TI_OH_ControllingCustomer))
			{
				return criteria.SortedControllingCustomerPKs.IndexOf(entry.TI_OH_ControllingCustomer);
			}

			return int.MaxValue;
		}

		protected override string GetName()
		{
			return (NoResString)"Controlling Customer"; // log message, subject to change, more for support people as of now
		}
	}
}
