using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IUNDGDataItemProvider
	{
		UNDGDataItemCollection UNDGs { get; }
		BusinessObjectFactory Factory { get; }
		bool NeedFetchHintForLoad { get; }
	}
}
