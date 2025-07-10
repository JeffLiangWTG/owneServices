using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingProcessTaskCollection : AgencyShipmentProcessTaskCollection
	{
		public AgencyBookingProcessTaskCollection(AgencyBooking booking)
			: base(booking)
		{
		}

		public new AgencyBookingProcessTask this[int index]
		{
			get { return (AgencyBookingProcessTask)Elements[index]; }
		}

		public new AgencyBookingProcessTask AddNew()
		{
			return (AgencyBookingProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new AgencyBookingProcessTaskCollection(Parent);
		}

		#region Implementation

		new AgencyBooking Parent
		{
			get { return (AgencyBooking)base.Parent; }
		}

		#endregion

	}
}


