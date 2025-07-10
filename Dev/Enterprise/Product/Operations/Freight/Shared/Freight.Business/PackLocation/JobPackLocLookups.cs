//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobPackLocLookups
//
//    This class should be used for overriding collections in AutoJobPackLocLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Freight.Business
{
	public class JobPackLocLookups : AutoJobPackLocLookups
	{
		public JobPackLocLookups(AutoJobPackLoc parent) : base(parent)
		{
		}

		public IWhsWarehouseCollection Warehouses
		{
			get
			{
				var result = ObjectFactory.Get<IWhsWarehouseCollection>("IWhsWarehouseCollection", Factory);
				result.Load();
				return result;
			}
		}
	}
}
