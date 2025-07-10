using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface ICartageExportSupport : IImportExport
	{
		string JobNumber { get; }
		bool HasChanges { get; }
		bool IsInDatabase { get; }
		BusinessObjectFactory Factory { get; }
	}
}
