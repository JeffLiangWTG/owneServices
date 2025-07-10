using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ISalesValue : IBusiness, IAuditDetails
	{
		string TablePrefix { get; }
		SalesValueAssociationPivotCollection SalesAssociationPivotCollectionCompanyView { get; }
		SalesValueAssociationPivotCollection SalesAssociationPivotCollectionGlobal { get; }
	}
}
