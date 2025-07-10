using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefCommodityRatingCodeMap : AutoRefCommodityRatingCodeMap
	{
		public RefCommodityRatingCodeMap(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
