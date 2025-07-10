using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI
{
	public partial class RealTimeRoutingFilterControl : ZFilterStripControl
	{
		public RealTimeRoutingFilterControl(RoutingResponseHeaderCollection responseHeaders, RoutingRequestFilterStripBusinessObject filterBizo)
			: base(responseHeaders, filterBizo)
		{
			InitializeComponent();

			ShouldRunSearchOnStripsInitialized = false;
			this.filterBizo = filterBizo;

			SingleDayRadioButton.AllowOverlap(FilterStripsPanel);
			SingleDayRadioButton.AllowOverlap(ToolStripPermissionsLabel);
			WeeklyTimetableRadioButton.AllowOverlap(FilterStripsPanel);
		}

		readonly RoutingRequestFilterStripBusinessObject filterBizo;

		#region GUI Layout

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Cannot reference a GUI dll. Scaling is done manually")]
		protected override void HandleGridSizing()
		{
			if (FilteredGrid != null && RoutingLinesGrid != null)
			{
				Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);

				var unscaleY = 96 / (decimal)graphics.DpiY;
				var scaleY = (decimal)graphics.DpiY / 96;

				FilteredGrid.Location = new Point(0, FilteredGrid.Top);

				var unscaledClientHeight = (int)ZArchitecture.Core.Utilities.Round(ClientSize.Height * unscaleY, 0);
				var unscaledGridTop = (int)ZArchitecture.Core.Utilities.Round(FilteredGrid.Top * unscaleY, 0);
				var unscaledRoutingHeight = (int)ZArchitecture.Core.Utilities.Round(RoutingLinesGrid.Height * unscaleY, 0);

				FilteredGrid.Height = (int)ZArchitecture.Core.Utilities.Round((unscaledClientHeight - unscaledGridTop - unscaledRoutingHeight - 20) * scaleY, 0);
				FilteredGrid.Width = ClientSize.Width;
			}
		}

		protected override void BindCore()
		{
			base.BindCore();
			RoutingLinesGrid.SetDataBinding(GridCollection, "Lines");

			FilteredGrid.DoubleClick += delegate
			{
				if (FilteredGrid.SelectedElements.Length > 0)
				{
					FindForm().DialogResult = DialogResult.OK;
					FindForm().Close();
				}
			};
		}

		#endregion

		protected override void OnSearchPerformed(bool showError, bool didSearch, bool isManualSearch, Form form)
		{
			base.OnSearchPerformed(showError, didSearch, isManualSearch, form);

			if (!previousIncludeWeeklyTimetableValue.HasValue || previousIncludeWeeklyTimetableValue.Value != filterBizo.Manager.IncludeWeeklyTimetable)
			{
				bool gridContainsDayOfWeekColumn = FilteredGrid.Columns.Contains(dayOfWeekColumnStyleInfo1.ColumnName);
				if (filterBizo.Manager.IncludeWeeklyTimetable && !gridContainsDayOfWeekColumn)
				{
					FilteredGrid.ColumnStyles.Add(dayOfWeekColumnStyleInfo1);
					FilteredGrid.Columns.Add(dayOfWeekColumnStyleInfo1);
					FilteredGrid.RefreshTableStyles();

					RoutingLinesGrid.ColumnStyles.Add(dayOfWeekColumnStyleInfo2);
					RoutingLinesGrid.Columns.Add(dayOfWeekColumnStyleInfo2);
					RoutingLinesGrid.RefreshTableStyles();

					previousIncludeWeeklyTimetableValue = filterBizo.Manager.IncludeWeeklyTimetable;
				}
				else if (!filterBizo.Manager.IncludeWeeklyTimetable && gridContainsDayOfWeekColumn)
				{
					FilteredGrid.ColumnStyles.Remove(dayOfWeekColumnStyleInfo1);
					FilteredGrid.Columns.Remove(dayOfWeekColumnStyleInfo1.ColumnName);
					FilteredGrid.RefreshTableStyles();

					RoutingLinesGrid.ColumnStyles.Remove(dayOfWeekColumnStyleInfo2);
					RoutingLinesGrid.Columns.Remove(dayOfWeekColumnStyleInfo2.ColumnName);
					RoutingLinesGrid.RefreshTableStyles();
					previousIncludeWeeklyTimetableValue = filterBizo.Manager.IncludeWeeklyTimetable;
				}
			}
		}

		bool? previousIncludeWeeklyTimetableValue;
	}
}


