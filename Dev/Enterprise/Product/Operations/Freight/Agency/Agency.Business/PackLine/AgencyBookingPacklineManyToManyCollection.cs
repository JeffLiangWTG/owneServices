namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingPackLineManyToManyCollection : AgencyShipmentPackLineManyToManyCollection
	{
		public AgencyBookingPackLineManyToManyCollection(AgencyBookingContainer container)
			: base(container) { }

		public new AgencyBookingPackLine AddNew()
		{
			return (AgencyBookingPackLine)base.AddNew();
		}

		public new AgencyBookingPackLine this[int index]
		{
			get { return (AgencyBookingPackLine)base[index]; }
		}
	}
}


