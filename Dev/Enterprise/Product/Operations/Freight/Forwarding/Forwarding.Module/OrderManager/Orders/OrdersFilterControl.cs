using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public partial class OrdersFilterControl : ZFilterStripControl
	{
		public OrdersFilterControl(OrderCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, Core.Constants.DocManagerCodes.Order);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new OrdersBaseModuleStrip();
		}

		#region Grid Layout Persister

		protected CustomLabelsGridLayoutPersister fGridLayoutPersister;

		protected override void BindCore()
		{
			base.BindCore();
			fGridLayoutPersister = new CustomLabelsGridLayoutPersister(this.FilteredGrid, new Order.CustomLabelsProvider((OrdersFilterBusinessObject)this.FilterBusinessObject, true));
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fGridLayoutPersister != null)
				{
					fGridLayoutPersister.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
