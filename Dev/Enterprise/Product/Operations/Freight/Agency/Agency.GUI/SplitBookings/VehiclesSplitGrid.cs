namespace Enterprise.Freight.Agency.GUI
{
	using Enterprise.Core.Forms;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.ZArchitecture;

	internal class VehiclesSplitGrid : ContainersSplitGrid
	{
		protected override string CollectionName
		{
			get { return AgencyBooking.Schema.Vehicles; }
		}

		protected override string[] Columns
		{
			get
			{
				return new string[]
				{
					AgencyBookingContainer.Schema.JC_ContainerNum,
					AgencyBookingContainer.Schema.JC_ContainerCount,
					AgencyBookingContainer.Schema.JC_Calc_NetWeight,
					AgencyBookingContainer.Schema.JC_TareWeight,
					AgencyBookingContainer.Schema.JC_GrossWeight,
					AgencyBookingContainer.Schema.JC_GrossWeightUQ,
				};
			}
		}

		protected override string ItemsName
		{
			get { return Res.GetString("SplitGrid|1282246e-3b2f-422d-aabf-bca0088f7912", "Vehicles"); }
		}

		protected override void PerformMove(object item, SplitBookingsHeader.MoveDirection direction)
		{
			Header.MoveVehicle((AgencyBookingContainer)item, direction);
		}

		protected override ZGridColumnInfo GetColumnControl(string columnName)
		{
			ZGridColumnInfo control;

			switch (columnName)
			{
				case AgencyBookingContainer.Schema.JC_ContainerNum:
					control = new ZTextBoxColumnStyleInfo();
					control.CaptionResourceString = Res.GetData("38ccc30c-3a71-4ab6-95ea-c20f65ab2675", "VIN / Serial", "Reference Number that is applicable to this vehicle (i.e. VIN or Serial Number).");
					break;

				default:
					control = base.GetColumnControl(columnName);
					break;
			}

			return control;
		}
	}
}
