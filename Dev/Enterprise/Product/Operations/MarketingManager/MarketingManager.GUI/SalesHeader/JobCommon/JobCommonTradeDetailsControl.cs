using System;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class JobCommonTradeDetailsControl : TradeDetailsControl
	{
		[Obsolete("This just for designer. Use the constructor that takes sales product.")]
		public JobCommonTradeDetailsControl()
		{
			InitializeComponent();
		}

		public JobCommonTradeDetailsControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();

			innerControl = GetNewInnerControl();
			innerControlAsUserControl = (ZUserControl)innerControl;
			innerControlAsUserControl.Dock = DockStyle.Fill;
			Controls.Add(innerControlAsUserControl);
		}

		#region Inner Control

		protected virtual IJobCommonTradeDetailsInnerControl GetNewInnerControl()
		{
			return new JobCommonTradeDetailsInnerControl(SalesProduct.MP_Code);
		}

		readonly IJobCommonTradeDetailsInnerControl innerControl;
		readonly ZUserControl innerControlAsUserControl;

		#endregion

		#region DataBinding

		void ToggleInnerGridsReadOnly()
		{
			var readOnly = EntitySales == null || EntitySales.IsDeleted;
			innerControl.GroupingGrid.ReadOnly = readOnly;
			innerControl.TradeDetailsGrid.ReadOnly = readOnly;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			ToggleInnerGridsReadOnly();
			RebuildGroupingCollection();
		}

		protected override void RebuildGroupingCollection()
		{
			if (EntitySales != null && EntitySales.Product != null)
			{
				innerControlAsUserControl.SetDataBinding(EntitySales.EntityDetailGroupings, "");

				if (innerControl.ReadOnly)
				{
					innerControlAsUserControl.SetReadOnlyIncludingChildren(true);
				}
			}
		}

		#endregion

		#region Trade Details Grid

		public override ZGrid TradeDetailsGrid
		{
			get { return innerControl != null ? innerControl.TradeDetailsGrid : null; }
		}

		#endregion
	}
}
