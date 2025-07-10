using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPutawayGroup : AutoWhsPutawayGroup, IWhsPutawayGroup
	{
		public WhsPutawayGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
