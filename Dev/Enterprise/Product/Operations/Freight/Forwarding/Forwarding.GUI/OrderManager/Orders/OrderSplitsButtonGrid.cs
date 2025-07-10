using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrderSplitsButtonGrid : ZModuleButtonGridWithoutColumnStylesSerialisation
	{
		protected CustomLabelsGridLayoutPersister fGridLayoutPersister;

		public OrderSplitsButtonGrid()
		{
			InitializeComponent();
			InnerGrid.ReadOnly = true;
		}

		public Order Order { get; set; }

		#region Implementation

		protected bool IsBound
		{
			get { return DataSource != null; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (fGridLayoutPersister != null)
			{
				fGridLayoutPersister.Dispose();
				fGridLayoutPersister = null;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				fGridLayoutPersister = new CustomLabelsGridLayoutPersister(InnerGrid, new Order.CustomLabelsProvider(Order, true));
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.fGridLayoutPersister != null)
				{
					this.fGridLayoutPersister.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
