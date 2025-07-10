using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public partial class OrderLineFilterControl : ZFilterStripControl
	{
		public OrderLineFilterControl(OrderLineCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			CreateQuantityBookedAndQuantityOpenColumns();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.OrderLineWorkflowDescriptorCode);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new OrdersBaseModuleStrip();
		}

		void CreateQuantityBookedAndQuantityOpenColumns()
		{
			if (AdvOrmFeatureHelper.IsEnabled)
			{
				var zQuantityBookedCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
				zQuantityBookedCalcEditColumnStyleInfo.CaptionResourceString = Res.GetData("OrderLineFilterControl|0a4a3034-fc30-49f5-bb49-a3b986d5e286", "Qty Booked");
				zQuantityBookedCalcEditColumnStyleInfo.ColumnName = "JO_QtyBooked";
				zQuantityBookedCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
				zQuantityBookedCalcEditColumnStyleInfo.Decimals = 5;
				zQuantityBookedCalcEditColumnStyleInfo.IsReadOnly = true;
				grid.ColumnStyles.Add(zQuantityBookedCalcEditColumnStyleInfo);

				var zQuantityOpenCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
				zQuantityOpenCalcEditColumnStyleInfo.CaptionResourceString = Res.GetData("OrderLineFilterControl|38b8b055-97e7-490a-90d0-0f40ab6a46fd", "Qty Open");
				zQuantityOpenCalcEditColumnStyleInfo.ColumnName = "JO_OpenQuantity";
				zQuantityOpenCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
				zQuantityOpenCalcEditColumnStyleInfo.Decimals = 5;
				zQuantityOpenCalcEditColumnStyleInfo.IsReadOnly = true;
				grid.ColumnStyles.Add(zQuantityOpenCalcEditColumnStyleInfo);
			}
		}
	}
}
