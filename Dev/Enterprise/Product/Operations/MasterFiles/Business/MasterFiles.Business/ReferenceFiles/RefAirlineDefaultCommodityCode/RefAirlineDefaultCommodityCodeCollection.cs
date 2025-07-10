using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineDefaultCommodityCodeCollection : DependentBusinessObjectCollection<RefAirlineDefaultCommodityCode, RefAirline>
	{
		public RefAirlineDefaultCommodityCodeCollection(RefAirline parent)
			: base(parent)
		{
		}
	}
}
