using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class WiseRatesFilterStripControl : ZFilterStripCommonControl
	{
		public WiseRatesFilterStripControl(WiseRatingHeaderView ratesServiceContainer)
			: base(new WiseRatesFilterStripBusinessObject())
		{
			View = ratesServiceContainer;
			InitializeComponent();
			FilterStripsPanel.SizeChanged += FilterStripsPanel_SizeChanged;

			ratesServiceViewPanel.AllowOverlap(ToolStrip);
			ratesServiceViewPanel.AllowOverlap(ToolStripHelp);
			ToolStripPermissionsLabel.AllowOverlap(searchResultsBox);

			searchResultsBox.AllowOutsideOfParent();
		}

		readonly WiseRatingHeaderView View;

		internal bool HasDatesFilterSelectedMoreThanOnce => Strips.Count(x => x.CurrentDataItem?.CurrentModuleFilter != null && x.CurrentDataItem.CurrentModuleFilter.OriginalCode == RateEntryFilterUtility.Constants.Codes.EffectiveOn) > 1;

		#region Overrides

		public override IBusinessObjectCollection GridCollection => View.WiseEntryViews;

		protected override bool CanSaveColumnLayouts => true;

		protected override Control ControlForLayout => ratesServiceViewPanel ?? (ratesServiceViewPanel = new WiseRatesViewPanel());

		protected override ZFilterGrid GetNewFilteredGrid()
		{
			return ratesServiceViewPanel.RateEntryGrid as ZDisplayGrid;
		}

		protected override void InitialiseGridCore()
		{
			Grid.ReadOnly = true;
			Grid.IsWholeRowSelectedOnClick = true;
		}

		public void ShowResultsChart(RatesSearchResponseDTO response, IEnumerable<string> warnings)
		{
			this.searchResultsBox.DisplayResults(response, warnings);
		}

		void FilterStripsPanel_SizeChanged(object sender, System.EventArgs e)
		{
			var fixedWidth = ControlDpiScalingHelper.ScaleToCurrentDpiY(500);
			// isInStandardDpi = false because width and height have been scaled to user's DPI
			searchResultsBox.Size = ControlDpiScalingHelper.NewScaledSize(fixedWidth, FilterStripsPanel.Height, isInStandardDpi: false);
		}

		public override void ToolStripFindDropButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			base.ToolStripFindDropButton_DropDownItemClicked(sender, e);
			this.searchResultsBox.Reset();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.searchResultsBox != null)
				{
					this.searchResultsBox.Dispose();
					this.searchResultsBox = null;
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#endregion

	}
}

