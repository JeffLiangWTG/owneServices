using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class UNDGDataItemCollection : UNDGDataItemCollection<UNDGDataItem>, ISailingSynchronisationTargetCollection<MasterFiles.Business.UNDGDataItem, UNDGDataItem>
	{
		public UNDGDataItemCollection(IUNDGDataItemProvider master)
			: base(master)
		{
		}
	}
}
