using CargoWise.Types;

namespace Enterprise.Warehouse.Transit.Document
{
	public interface IDataSourceProvider
	{
		ZString SourceID { get; }
		ZString SourceType { get; }
	}
}
