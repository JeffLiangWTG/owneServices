using System;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodGroupingCollection : NonPersistentBusinessObjectCollection<TradePeriodGrouping>
	{
		public TradePeriodGroupingCollection()
			: base()
		{
		}

		#region Allowed Actions

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Should be populating collection manually via 'Add'");
		}

		#endregion
	}
}
