using System;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class QuoteContainersAndPackLinesControl : ZUserControl
	{
		public QuoteContainersAndPackLinesControl()
		{
			InitializeComponent();
		}

		QuotedBooking QuotedBooking
		{
			get { return DataSource as QuotedBooking; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (
					CurrentDataItem != null
					&& CurrentDataItem is QuotedBooking quotedBooking
					&& quotedBooking?.Quote?.CurrentOneOffQuote?.TT_ContainerModeInfo != null
				)
			{
				quotedBooking.Quote.CurrentOneOffQuote.TT_ContainerModeInfo.ValueChanged -= ContainerModeValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (
					CurrentDataItem != null
					&& CurrentDataItem is QuotedBooking quotedBooking
					&& quotedBooking?.Quote?.CurrentOneOffQuote?.TT_ContainerModeInfo != null
				)
			{
				quotedBooking.Quote.CurrentOneOffQuote.TT_ContainerModeInfo.ValueChanged += ContainerModeValueChanged;
				HandleLooseCargoContainerTypeColumn(quotedBooking);
			}
		}

		void ContainerModeValueChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem != null && CurrentDataItem is QuotedBooking quotedBooking)
			{
				HandleLooseCargoContainerTypeColumn(quotedBooking);
			}
		}

		void HandleLooseCargoContainerTypeColumn(QuotedBooking quotedBooking)
		{
			var shouldBeVisible =
				quotedBooking.Booking == null
				&& Core.Constants.RateMode.ULD.Equals(QuotedBooking.Quote?.CurrentOneOffQuote?.TT_ContainerMode.ToString(), StringComparison.InvariantCultureIgnoreCase);

			var columnName = nameof(RateOneOffPackLine.LooseCargoContainerType);

			if (shouldBeVisible)
			{
				if (!LooseCargoGrid.Columns.Contains(columnName))
				{
					var dropEdit = new ZDropEditColumnStyleInfo() { ColumnName = columnName, IsVisible = true };
					dropEdit.Caption = Res.GetString("4EA69F0C-BA42-4240-8AAE-DF493870CE87", "Container Type");
					LooseCargoGrid.Columns.Add(dropEdit);
				}
			}
			else
			{
				if (LooseCargoGrid.Columns.Contains(columnName))
				{
					LooseCargoGrid.Columns.Remove(columnName);
				}
			}

			var vehicleColumnList = new string[]
			{
				AutoRateOneOffPackLine.Schema.TPL_VehicleMake,
				AutoRateOneOffPackLine.Schema.TPL_VehicleModel,
				AutoRateOneOffPackLine.Schema.TPL_VehicleYear,
				AutoRateOneOffPackLine.Schema.TPL_VehicleColor,
				AutoRateOneOffPackLine.Schema.TPL_VehicleNumberOfDoors,
				AutoRateOneOffPackLine.Schema.TPL_VehicleTransmission,
				AutoRateOneOffPackLine.Schema.TPL_RefVehicleIdentificationNumber
			};
			if (quotedBooking.ContainerMode == Enterprise.Core.Constants.ContainerModes.RollOnRollOff)
			{
				LooseCargoGrid.AddToAvailableColumns(vehicleColumnList);
			}
			else
			{
				LooseCargoGrid.RemoveFromAvailableColumns(vehicleColumnList);
			}

			LooseCargoGrid.RefreshTableStyles();
		}
	}
}
