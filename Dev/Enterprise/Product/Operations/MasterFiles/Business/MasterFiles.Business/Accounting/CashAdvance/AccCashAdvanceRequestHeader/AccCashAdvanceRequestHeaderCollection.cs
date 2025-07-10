using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceRequestHeaderCollection : BusinessObjectCollection<AccCashAdvanceRequestHeader>
	{
		public AccCashAdvanceRequestHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccCashAdvanceRequestHeaderCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
