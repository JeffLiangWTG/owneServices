using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceRequestLineCollection : BusinessObjectCollection<AccCashAdvanceRequestLine>
	{
		public AccCashAdvanceRequestLineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccCashAdvanceRequestLineCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (bizOAdded is AccCashAdvanceRequestLine line)
			{
				line.RequestHeader?.UpdateStatusFromLine();
				line.RequestHeader?.UpdatePaidAmount();
			}
		}
	}
}
