using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class ItemPackagingCollection : DependentCusAddInfoCollection<ItemPackaging, JobComInvoiceLine>
	{
		public ItemPackagingCollection(JobComInvoiceLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.NZItemPackaging)
		{
		}
	}
}
