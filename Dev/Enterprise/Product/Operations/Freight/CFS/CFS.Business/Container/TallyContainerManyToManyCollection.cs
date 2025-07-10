using System;
using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyContainerManyToManyCollection : CFSContainerManyToManyCollection
	{
		public TallyContainerManyToManyCollection(TallyPackLine parent) : base(parent)
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

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TallyContainer);
		}
	}
}
