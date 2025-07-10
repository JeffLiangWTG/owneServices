using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class CusInBondPayInfo : AutoCusInBondPayInfo
	{
		protected CusInBondPayInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
