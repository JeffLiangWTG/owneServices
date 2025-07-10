using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class ItemPackagingDataCollection : Customs.Business.CusCodeDataCollection<ItemPackagingData>
	{
		public ItemPackagingDataCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.NZItemPackaging)
		{
		}
	}
}
