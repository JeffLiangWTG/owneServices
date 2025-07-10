
namespace Enterprise.Freight.CFS.Business
{
	public class GatePassLoadListConsolManyToManyCollection : CFSLoadListConsolManyToManyCollection
	{
		public GatePassLoadListConsolManyToManyCollection(GatePassShipment shipment) : base(shipment)
		{
		}

		public new GatePassLoadListConsol this[int index]
		{
			get { return (GatePassLoadListConsol)(Elements[index]); }
		}

		public new GatePassLoadListConsol AddNew()
		{
			return (GatePassLoadListConsol)base.AddNew();
		}
	}
}
