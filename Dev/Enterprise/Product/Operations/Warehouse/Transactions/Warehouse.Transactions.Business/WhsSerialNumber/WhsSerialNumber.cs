using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsSerialNumber : AutoWhsSerialNumber
	{
		public WhsSerialNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
