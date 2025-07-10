namespace Enterprise.Customs.US.Business
{
	class AirAutoQueryFilter : IAutoQueryFilter
	{
		public bool RequestForRelatedBOL => false;

		public bool Filter(ICargoManifestStatusQueryData objectForQuery)
		{
			var bill = (objectForQuery as Bill);
			if (bill == null)
			{
				return false;
			}
			return (bill.IsMasterBill && !HasHouseBills(bill)) || (bill.IsHouseBill);
		}

		bool HasHouseBills(Bill bill)
		{
			return bill.ChildBills?.Count > 0;
		}
	}
}
