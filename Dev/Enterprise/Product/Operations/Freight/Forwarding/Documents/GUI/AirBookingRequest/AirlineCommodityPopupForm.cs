using System;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	public partial class AirlineCommodityPopupForm : ZChildForm
	{
		readonly IFindBox findBox;
		readonly ImmutableList<AirlineConfigCommodityBusinessObject> airlineCommodityCollection;

		protected AirlineCommodityFilterControl FilterControl { get; set; }

		public AirlineCommodityPopupForm(IFindBox findBox)
		{
			this.findBox = findBox;
			InitializeComponent();
			if (findBox?.ListProvider?.List is AirlineConfigCommodityBusinessObjectCollection commodityCollection && !DesignModeFinder.IsDesigning)
			{
				airlineCommodityCollection = commodityCollection.Cast<AirlineConfigCommodityBusinessObject>().ToImmutableList();
				var cloneCollection = new AirlineConfigCommodityBusinessObjectCollection();
				cloneCollection.AddRange(airlineCommodityCollection);
				CreateFilterControl(cloneCollection);
			}
		}

		#region Filter

		void CreateFilterControl(AirlineConfigCommodityBusinessObjectCollection collection)
		{
			FilterControl = new AirlineCommodityFilterControl(collection, new AirlineCommodityFilterStripBusinessObject());
			filterPanel.Controls.Add(FilterControl);
			FilterControl.Dock = DockStyle.Fill;
			FilterControl.PerformSearch += FilterControl_PerformSearch;

			if (!string.IsNullOrEmpty(findBox.Code))
			{
				((AirlineCommodityFilterStripBusinessObject)FilterControl.FilterBusinessObject).CodeFilter.Property = findBox.Code;
			}
		}

		void FilterControl_PerformSearch(object sender, PerformSearchEventArgs e)
		{
			var grid = FilterControl.Grid;
			var gridCollection = FilterControl.GridCollection as AirlineConfigCommodityBusinessObjectCollection;

			try
			{
				grid.SuspendLayout();
				using (gridCollection.SuspendListChanged())
				{
					gridCollection.RemoveAll();
					gridCollection.AddRange(airlineCommodityCollection.Where(AirlineCommodityMatchesFilter));
				}
			}
			finally
			{
				grid.ForcePreFetch();
				grid.ResumeLayout();
			}
		}

		bool AirlineCommodityMatchesFilter(AirlineConfigCommodityBusinessObject airlineCommodity)
		{
			var filter = FilterControl.FilterBusinessObject as AirlineCommodityFilterStripBusinessObject;
			var matchesCode = string.IsNullOrEmpty(filter?.CodeFilter?.Property) || filter.CodeFilter.SqlComparisonOperator.GetPredicate(filter.CodeFilter.Property)(airlineCommodity.Code);
			var matchesDescription = string.IsNullOrEmpty(filter?.DescriptionFilter?.Property) || filter.DescriptionFilter.SqlComparisonOperator.GetPredicate(filter.DescriptionFilter.Property)(airlineCommodity.Description);
			return matchesCode && matchesDescription;
		}

		#endregion

		#region Buttons

		void OkBtn_Click(object sender, EventArgs e)
		{
			if (FilterControl.FilteredGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(Res.GetString("515c99cd-479f-491f-ae2e-dac587499060", "Please select one item from the grid."));
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelBtn_Clicked(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		#region Implement

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				if (FilterControl.FilteredGrid.GetFirstSelectedRow() is AirlineConfigCommodityBusinessObject airlineCommodity)
				{
					findBox.Code = airlineCommodity.Code;
				}
			}
			base.OnClosing(e);
		}

		#endregion
	}
}
