
namespace Enterprise.Freight.CFS.Business
{
	public class GatePassInnerPackLineCollection : CFSInnerPackLineCollection
	{
		public GatePassInnerPackLineCollection(GatePassShipment master) : base(master)
		{
		}

		public new GatePassPackLine AddNew()
		{
			return (GatePassPackLine)base.AddNew();
		}

		public new GatePassPackLine this[int index]
		{
			get { return (GatePassPackLine)Elements[index]; }
		}
	}
}
