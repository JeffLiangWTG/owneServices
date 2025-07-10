using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class WhsTrackingInventorySummaryItemView : AutoWhsTrackingInventorySummaryItemView
	{
		public WhsTrackingInventorySummaryItemView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Lookups

		protected override WhsTrackingInventorySummaryItemViewLookups GetNewLookups() => new WhsTrackingInventorySummaryItemViewLookups(this);

		#endregion

		#region Validation

		protected override WhsTrackingInventorySummaryItemViewValidation GetNewValidation() => new WhsTrackingInventorySummaryItemViewValidation(this);

		#endregion

		#region Implementation

		public override bool CanDelete => false;

		public override void Delete()
		{
			throw new NotSupportedException($"{this.GetType().Name} objects cannot be deleted.");
		}

		#endregion
	}
}
