using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class DefaultCusSCAPivot : BaseCusSCAPivot
	{
		public DefaultCusSCAPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
