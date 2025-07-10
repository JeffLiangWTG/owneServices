using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingContainerManyToManyCollection : CommonContainerManyToManyCollection
	{
		public ForwardingContainerManyToManyCollection(ForwardingPackLine parent)
			: base(parent)
		{
		}

		public new ForwardingContainer this[int index]
		{
			get { return (ForwardingContainer)(Elements[index]); }
		}

		public new ForwardingContainer AddNew()
		{
			return (ForwardingContainer)base.AddNew();
		}
	}
}
