using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules
{
	public partial class OnlineSailingSchedulesFilterControl : ZFilterStripControl
	{
		public OnlineSailingSchedulesFilterControl(RoutesCollection routesCollection, OnlineSchedulesFilterStripBusinessObject filterBusinessObject)
			: base(routesCollection, filterBusinessObject)
		{
			InitializeComponent();
			if (!FreightDataRegistry.Instance.EnableGlobalSailingSchedulesCalculation.Value)
			{
				LegsGrid.ColumnStyles.Remove(LegsGrid.GetColumnStyle(nameof(Leg.Co2eKgPerTeu)));
				LegsGrid.ColumnStyles.Remove(LegsGrid.GetColumnStyle(nameof(Leg.Co2eKgPerTonne)));
				grid.ColumnStyles.Remove(grid.GetColumnStyle(nameof(Route.Co2eKgPerTeu)));
				grid.ColumnStyles.Remove(grid.GetColumnStyle(nameof(Route.Co2eKgPerTonne)));
			}
		}

		public new void ResetFilterStrips()
		{
			base.ResetFilterStrips();
		}

		protected override void HandleGridSizing()
		{
			if (FilteredGrid != null && LegsGrid != null)
			{
				Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);

				var unscaleY = 96 / (decimal)graphics.DpiY;
				var scaleY = (decimal)graphics.DpiY / 96;

				ControlDpiScalingHelper.SetLeft(FilteredGrid, 0, false);
				ControlDpiScalingHelper.SetTop(FilteredGrid, FilteredGrid.Top, false);

				var unscaledClientHeight = (int)ZArchitecture.Core.Utilities.Round(ClientSize.Height * unscaleY, 0);
				var unscaledGridTop = (int)ZArchitecture.Core.Utilities.Round(FilteredGrid.Top * unscaleY, 0);
				var unscaledRoutingHeight = (int)ZArchitecture.Core.Utilities.Round(LegsGrid.Height * unscaleY, 0);

				ControlDpiScalingHelper.SetHeight(FilteredGrid, (int)ZArchitecture.Core.Utilities.Round((unscaledClientHeight - unscaledGridTop - unscaledRoutingHeight - 20) * scaleY, 0), false);
				ControlDpiScalingHelper.SetWidth(FilteredGrid, ClientSize.Width, false);
			}
		}

		internal List<string> GetFiltersSelectedMultipleTimes()
		{
			return Strips.Select(x => x.CurrentDataItem.FilterDescriptionLocalized.ToString())
				.GroupBy(x => x)
				.Where(x => x.Count() > 1)
				.Select(x => x.Key)
				.ToList();
		}

		protected override void AddFilterStripCore(ZFilterStrip strip, FilterStrip bizO, GroupStripControl groupStripControl)
		{
			base.AddFilterStripCore(strip, bizO, groupStripControl);
			strip.IsFilterCategoriesEnabled = false;
		}

		protected override void BindCore()
		{
			base.BindCore();
			LegsGrid.SetDataBinding(GridCollection, "Legs");
			LegsGrid.LegValidityChanged += LegValidictyChanged;
		}

		protected override void UnhookEventsOnFilterBizO()
		{
			if (LegsGrid != null)
			{
				LegsGrid.LegValidityChanged -= LegValidictyChanged;
			}

			base.UnhookEventsOnFilterBizO();
		}

		void LegValidictyChanged(object sender, EventArgs e)
		{
			((BusinessObject)DataSource).RunPreSaveValidation();
		}

		protected override ZFilterGrid GetNewFilteredGrid()
		{
			return new DisplayGridWithNotifications();
		}

		protected override ZFilterStrip NewZFilterStrip() => new OnlineSailingSchedulesFilterStrip();

		class DisplayGridWithNotifications : ZDisplayGrid
		{
			protected override bool ShouldShowNotifications
			{
				get { return true; }
			}
		}
	}
}
