using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(CusHAWB), "CusHAWBItemsCollection")]
	public class CusHAWBItems : AutoCusHAWBItems
	{
		public CusHAWBItems(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
