using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentPackLineManyToManyCollection : PackLineManyToManyCollection
	{
		public AgencyShipmentPackLineManyToManyCollection(AgencyShipmentContainer container)
			: base(container) { }

		public new AgencyShipmentPackLine AddNew()
		{
			return (AgencyShipmentPackLine)base.AddNew();
		}

		public new AgencyShipmentPackLine this[int index]
		{
			get { return (AgencyShipmentPackLine)base[index]; }
		}
	}
}


