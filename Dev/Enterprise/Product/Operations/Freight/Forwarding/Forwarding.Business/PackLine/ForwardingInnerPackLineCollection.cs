using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingInnerPackLineCollection : InnerPackLineCollection
	{
		public ForwardingInnerPackLineCollection(ForwardingShipment master) : base(master, master.Factory)
		{
		}

		public new ForwardingPackLine AddNew()
		{
			return (ForwardingPackLine)base.AddNew();
		}

		public new ForwardingPackLine this[int index]
		{
			get { return (ForwardingPackLine)Elements[index]; }
		}
	}
}
