using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IFDARelatedContainer
	{
		BusinessObjectFactory Factory { get; }
		bool IsDeleted { get; }
		bool IsInDatabase { get; }
		FDARelatedContainersGenPivotCollection ContainersForFDALine { get; }
		JobComInvoiceLine InvoiceLine { get; }
		ZGuid PK { get; }
	}
}
