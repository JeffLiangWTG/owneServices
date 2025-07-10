namespace Enterprise.Freight.Agency.GUI
{
	using Enterprise.Core.Forms;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.ZArchitecture;

	class TopLevelPacksSplitGrid : ContainersSplitGrid
	{
		protected override string CollectionName
		{
			get { return AgencyBooking.Schema.TopLevelPacks; }
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
			get { return Res.GetString("SplitGrid|ed962ac8-10ac-4628-83be-e31f3e268c95", "Packs"); }
		}

		protected override ZGridColumnInfo GetColumnControl(string columnName)
		{
			ZGridColumnInfo control;

			switch (columnName)
			{
				case AgencyBookingContainer.Schema.JC_ContainerNum:
					control = new ZTextBoxColumnStyleInfo();
					control.CaptionResourceString = Res.GetData("b6f73b8f-76cf-4519-8d3a-f5663b350be1", "Ref. Number", "Reference Number", "");
					break;

				default:
					control = base.GetColumnControl(columnName);
					break;
			}

			return control;
		}

		protected override void PerformMove(object item, SplitBookingsHeader.MoveDirection direction)
		{
			Header.MoveTopLevelPack((AgencyBookingContainer)item, direction);
		}
	}
}
