using Enterprise.Security;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class WhsComponentOrderFilterBusinessObject : PickableDocketFilterBusinessObject
	{
		protected override bool AddContainerNoFilter => false;
		protected override bool AddCustomerReferenceFilter => false;
		protected override bool AddTransportReferenceFilter => false;
		protected override bool IncludeAccountingFilters => false;
		protected override SecurityCheckpoint JobInvoicingSecurity => null;

		protected override CodeDescriptionPairList GetDocketStatusCore()
		{
			var status = base.GetDocketStatusCore();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Putaway));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Held));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Cancelled));
			return status;
		}
	}
}
