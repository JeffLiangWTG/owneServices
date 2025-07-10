using System;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class EstimateSalesAnalysisControl : ZUserControl
	{
		public EstimateSalesAnalysisControl()
		{
			InitializeComponent();
			filter = new EstimateSalesAnalysisFilter();
			dynamicTradeLaneWithDetailsControl.ShowActualsIfSupported();
		}

		readonly EstimateSalesAnalysisFilter filter;

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			filter.StatusInfo.ValueChanged += StatusInfo_ValueChanged;
			filter.ShouldMatchOnBuyerSupplierInfo.ValueChanged += ShouldMatchOnBuyerSupplierInfo_ValueChanged;
			filterControl.SetDataBinding(filter, null);

			dynamicTradeLaneWithDetailsControl.SetReadOnlyIncludingChildren();
		}

		#endregion

		#region Data Binding

		SalesHeader SalesHeader
		{
			get { return (SalesHeader)CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (SalesHeader != null)
			{
				filterControl.Visible = SalesHeader.SalesProduct.IsActualsSupported;
				RefreshCollection();
			}
		}

		#endregion

		#region Filter Control

		public EstimateSalesAnalysisFilterControl FilterControl
		{
			get { return filterControl; }
		}

		void RefreshCollection()
		{
			SalesHeader.FilterableEntitySalesCollection.StatusFilter = (filter.Status == EstimateSalesAnalysisStatusFilterList.Codes.All) ? ZString.Empty : filter.Status;
			SalesHeader.FilterableEntitySalesCollection.ShouldMatchOnBuyerSupplier = filter.ShouldMatchOnBuyerSupplier;

			foreach (EntitySalesWrapper salesWrapper in SalesHeader.FilterableEntitySalesCollection)
			{
				salesWrapper.ActualsInformation.RevenueDisplayOption = SalesHeader.RevenueDisplayOption;
			}
		}

		void StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			if (SalesHeader != null)
			{
				SalesHeader.FilterableEntitySalesCollection.StatusFilter = (filter.Status == EstimateSalesAnalysisStatusFilterList.Codes.All) ? ZString.Empty : filter.Status;
			}
		}

		void ShouldMatchOnBuyerSupplierInfo_ValueChanged(object sender, EventArgs e)
		{
			if (SalesHeader != null)
			{
				SalesHeader.FilterableEntitySalesCollection.ShouldMatchOnBuyerSupplier = filter.ShouldMatchOnBuyerSupplier;
			}
		}

		#endregion

		#region Trade Lane Control

		internal DynamicTradeLaneWithDetailsControl DynamicTradeLaneWithDetailsControl
		{
			get { return dynamicTradeLaneWithDetailsControl; }
		}

		#endregion
	}
}
