using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsSalesChannel : AutoWhsSalesChannel, IWhsSalesChannel
	{
		public WhsSalesChannel(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
