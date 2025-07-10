using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects
{
	interface IDataSourceProvider
	{
		ZString SourceID { get; }
		ZString SourceType { get; }
	}
}
