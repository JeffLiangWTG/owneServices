using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusHAWBItemsCollection : DependentBusinessObjectCollection<CusHAWBItems, CusHAWB>
	{
		public CusHAWBItemsCollection(CusHAWB master) : base(master)
		{
		}
	}
}
