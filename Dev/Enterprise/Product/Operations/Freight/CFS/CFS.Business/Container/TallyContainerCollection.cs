
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyContainerCollection : CFSContainerRegistrationList
	{
		public TallyContainerCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public TallyContainerCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public new TallyContainer this[int index]
		{
			get { return (TallyContainer)(Elements[index]); }
		}

		public new TallyContainer AddNew()
		{
			return (TallyContainer)base.AddNew();
		}
	}
}
