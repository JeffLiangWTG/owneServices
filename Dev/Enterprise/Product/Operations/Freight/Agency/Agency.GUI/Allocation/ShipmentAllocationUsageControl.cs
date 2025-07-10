using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class ShipmentAllocationUsageControl : ZUserControl
	{
		public ShipmentAllocationUsageControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			AgencyShipment newShipment = dataSource == null ? null : (AgencyShipment)BindingContext[dataSource, dataMember].GetCurrent();
			base.SetDataBinding(newShipment == null ? null : newShipment.Allocation, "");
		}

		#region Implementation

		void RefreshButton_Click(object sender, System.EventArgs e)
		{
			AllocationCalcWrapper wrapper = (AllocationCalcWrapper)CurrentDataItem;

			if (wrapper != null)
			{
				wrapper.UpdateAllocatedValues();
				wrapper.UpdateRequiredValues();
			}
		}

		#endregion
	}
}



