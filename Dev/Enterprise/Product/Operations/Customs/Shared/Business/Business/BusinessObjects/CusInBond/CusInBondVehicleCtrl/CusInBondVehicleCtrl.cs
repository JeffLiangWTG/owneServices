using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class CusInBondVehicleCtrl : AutoCusInBondVehicleCtrl
	{
		protected CusInBondVehicleCtrl(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
