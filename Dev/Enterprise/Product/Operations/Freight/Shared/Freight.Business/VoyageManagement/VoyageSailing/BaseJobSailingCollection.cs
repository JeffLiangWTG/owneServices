using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class BaseJobSailingCollection : BusinessObjectCollection<BaseJobSailing>
	{
		public BaseJobSailingCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public BaseJobSailingCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
