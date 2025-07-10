using CargoWise.Types;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	interface IDataSourceProvider
	{
		ZString SourceID { get; }
		ZString SourceType { get; }
	}
}
