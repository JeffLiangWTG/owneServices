using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class UNDGDataItemCollection : UNDGDataItemCollection<UNDGDataItem>
	{
		public UNDGDataItemCollection(IUNDGDataItemProvider master)
			: base(master)
		{
		}
	}
}
