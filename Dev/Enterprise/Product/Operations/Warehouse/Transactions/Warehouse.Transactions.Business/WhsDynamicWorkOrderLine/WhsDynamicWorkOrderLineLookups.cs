using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderLineLookups : WhsComponentOrderLineLookups
	{
		public WhsDynamicWorkOrderLineLookups(WhsDynamicWorkOrderLine parent)
			: base(parent)
		{
		}

		#region SupplierParts

		public override OrgSupplierPartCollection SupplierParts
			=> Parent.WE_WE_ParentDocketLine.IsEmpty || !Parent.ParentLine.IsSecondaryInwardProcessedItem ? base.SupplierParts : CreateCollectionForSecondaryLineComponents();

		OrgSupplierPartCollection CreateCollectionForSecondaryLineComponents()
		{
			var collection = new OrgSupplierPartCollection(Factory);
			var mainLine = Parent.ParentLine?.DynamicWorkOrder?.MainProductLine_UnsafeAfterFinalization;

			if (mainLine != null)
			{
				var products = IEnumerableExtensions.DistinctBy(mainLine.ChildComponentLinesCollection, l => l.WE_OP).Select(l => l.SupplierPart).WhereNotNull();
				collection.AddRange(products);
			}

			return collection;
		}

		#endregion

		new WhsDynamicWorkOrderLine Parent => (WhsDynamicWorkOrderLine)base.Parent;
	}
}
