using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public partial class WarehouseFilterControl : ZFilterStripControl
	{
		public WarehouseFilterControl()
		{
			InitializeComponent();
		}

		public WarehouseFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void BindCore()
		{
			base.BindCore();

			Grid.SetAvailability(WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value, WhsWarehouseSchema.Constants.WW_GG_ReleaseGroup);
		}
	}
}
