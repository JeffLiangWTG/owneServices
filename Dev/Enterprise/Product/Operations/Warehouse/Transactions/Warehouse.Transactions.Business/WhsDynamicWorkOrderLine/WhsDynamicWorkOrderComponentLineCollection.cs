using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderComponentLineCollection : WhsDynamicWorkOrderLineCollection
	{
		public WhsDynamicWorkOrderComponentLineCollection(WhsDynamicWorkOrderLine master)
			: base(master, new ZQuery(), WhsDocketLineSchema.WE_WE_ParentDocketLine)
		{
		}

		protected override bool AllowNew => base.AllowNew && ParentLine.IsMainInwardProcessedItem ^ ParentLine.IsSecondaryInwardProcessedItem;

		protected override void OnAdded(WhsDocketLine lineToAdd)
		{
			base.OnAdded(lineToAdd);

			if (ParentLine != null)
			{
				lineToAdd.WE_WD = ParentLine.WE_WD;
			}
		}

		protected new WhsDynamicWorkOrderLine ParentLine => (WhsDynamicWorkOrderLine)base.ParentLine;
	}
}
