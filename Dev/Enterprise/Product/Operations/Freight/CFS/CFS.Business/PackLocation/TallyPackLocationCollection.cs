
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyPackLocationCollection : CFSPackLocationCollection
	{
		public TallyPackLocationCollection(TallyPackLine parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		public new TallyPackLocation this[int index]
		{
			get { return (TallyPackLocation)(Elements[index]); }
		}

		public new TallyPackLocation AddNew()
		{
			return (TallyPackLocation)base.AddNew();
		}

		protected new TallyPackLine Parent
		{
			get { return (TallyPackLine)base.Parent; }
		}
	}
}
