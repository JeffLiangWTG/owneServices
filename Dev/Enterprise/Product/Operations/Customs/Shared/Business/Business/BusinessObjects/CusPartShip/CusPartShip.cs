using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusPartShip : AutoCusPartShip
	{
		public CusPartShip(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new CusPartShipTypeDecider();
	}
}
