using CargoWise.EntityFramework;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public partial class QuotedBookingFilterControl : ZFilterStripControl
	{
		public QuotedBookingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject, string colorContextKey = "", bool isOneOffQuote = false)
			: base(gridCollection, filterStripBusinessObject)
		{
			IsOneOffQuote = isOneOffQuote;

			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode);
				AddScreeningStatusColumnToGrid();
			}
			this.grid.ColorContextKey = colorContextKey;
		}

		void AddScreeningStatusColumnToGrid()
		{
			var columnForScreeningStatus = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var screeningStatusLabel = (!ComplianceRiskHelper.CheckIfComplianceRiskEnabled(typeof(QuotedBooking), allowViewType: true)) ? Res.GetData("8d22b02a-a968-4af8-b70b-9ccd9f937cd4", "Screening Status") : Res.GetData("4f96dee8-eb05-42b4-8a62-f4a269db122c", "Legacy Screening Status");

			columnForScreeningStatus.CaptionResourceString = screeningStatusLabel;
			columnForScreeningStatus.ColumnName = "QuotedBooking+Booking+JS_ScreeningStatus";
			columnForScreeningStatus.IsVisible = false;
			columnForScreeningStatus.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(32);

			FilteredGrid.ColumnStyles.Add(columnForScreeningStatus);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new QuotedBookingFilterStrip();
		}

		protected bool IsOneOffQuote { get; }

		#region Dispose

		readonly System.ComponentModel.Container components;
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
