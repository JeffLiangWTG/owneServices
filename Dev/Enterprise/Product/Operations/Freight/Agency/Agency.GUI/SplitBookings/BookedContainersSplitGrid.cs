namespace Enterprise.Freight.Agency.GUI
{
	using Enterprise.Freight.Agency.Business;

	internal partial class BookedContainersSplitGrid : ContainersSplitGrid
	{
		protected override string CollectionName
		{
			get { return AgencyBooking.Schema.BookedContainers; }
		}

		protected override string[] Columns
		{
			get
			{
				return new string[]
				{
					AgencyBookingContainer.Schema.JC_ContainerNum,
					AgencyBookingContainer.Schema.JC_RC,
					AgencyBookingContainer.Schema.JC_ContainerCount,
					AgencyBookingContainer.Schema.JC_ReleaseNum,
					AgencyBookingContainer.Schema.JC_Calc_NetWeight,
					AgencyBookingContainer.Schema.JC_TareWeight,
					AgencyBookingContainer.Schema.JC_GrossWeight,
					AgencyBookingContainer.Schema.JC_GrossWeightUQ,
				};
			}
		}

		protected override string ItemsName
		{
			get { return Res.GetString("SplitGrid|920092ef-1a0b-4adc-bdea-49973b020027", "Booked Containers"); }
		}

		protected override void PerformMove(object item, SplitBookingsHeader.MoveDirection direction)
		{
			Header.MoveBookedContainer((AgencyBookingContainer)item, direction);
		}
	}
}
