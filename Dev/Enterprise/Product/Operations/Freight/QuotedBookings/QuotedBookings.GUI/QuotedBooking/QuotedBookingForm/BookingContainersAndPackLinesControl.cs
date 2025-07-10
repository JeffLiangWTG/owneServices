using System;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.GUI;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class BookingContainersAndPackLinesControl : ZUserControl
	{
		public BookingContainersAndPackLinesControl()
		{
			InitializeComponent();
			if (ContractsPermissions.IsAllocationsVisible())
			{
				this.ContainersGrid.ColumnStyles.Add(allocationIDColumnStyleInfo1);
			}

			if (!DesignModeFinder.IsDesigning)
			{
				new CustomFieldColumnCreator().Set(LooseCargoGrid, new PackLineCustomFieldsDescriptor());
				new UNDGDataItemFormManager(LooseCargoGrid).Initialize();
				new PackProductFormManager(LooseCargoGrid).Initialize();
				new HarmonisedCodeFormManager(LooseCargoGrid).Initialize();
			}
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
					&& quotedBooking?.ModeInfo != null
				)
			{
				quotedBooking.ModeInfo.ValueChanged -= FreigtModeValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (
					CurrentDataItem != null
					&& CurrentDataItem is QuotedBooking quotedBooking
					&& quotedBooking?.ModeInfo != null
				)
			{
				quotedBooking.ContainerModeInfo.ValueChanged += FreigtModeValueChanged;
				HandleLooseCargoContainerTypeColumn(quotedBooking);
			}
		}

		void FreigtModeValueChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem != null && CurrentDataItem is QuotedBooking quotedBooking)
			{
				HandleLooseCargoContainerTypeColumn(quotedBooking);
			}
		}

		void HandleLooseCargoContainerTypeColumn(QuotedBooking quotedBooking)
		{
			var shouldBeVisible =
				quotedBooking.Booking != null
				&& Core.Constants.RateMode.ULD.Equals(QuotedBooking.ContainerMode.ToString(), StringComparison.InvariantCultureIgnoreCase);

			var columnName = nameof(ForwardingPackLine.LooseCargoContainerType);

			if (shouldBeVisible)
			{
				if (!LooseCargoGrid.Columns.Contains(columnName))
				{
					var dropEdit = new ZDropEditColumnStyleInfo() { ColumnName = columnName, IsVisible = true };
					dropEdit.Caption = Res.GetString("8320599D-A9CE-4BFC-9972-ACD69BE25BB4", "Container Type");
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
				AutoJobPackLines.Schema.JL_VehicleMake,
				AutoJobPackLines.Schema.JL_VehicleModel,
				AutoJobPackLines.Schema.JL_VehicleYear,
				AutoJobPackLines.Schema.JL_VehicleColor,
				AutoJobPackLines.Schema.JL_VehicleNumberOfDoors,
				AutoJobPackLines.Schema.JL_VehicleTransmission,
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
