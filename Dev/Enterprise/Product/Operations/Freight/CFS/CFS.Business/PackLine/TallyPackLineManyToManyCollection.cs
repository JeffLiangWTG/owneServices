using System;
using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyPackLineManyToManyCollection : CFSPackLineManyToManyCollection
	{
		public TallyPackLineManyToManyCollection(TallyContainer container) : base(container)
		{
		}

		public new TallyPackLine this[int index]
		{
			get { return (TallyPackLine)(Elements[index]); }
		}

		public new TallyPackLine AddNew()
		{
			return (TallyPackLine)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TallyPackLine);
		}
	}
}
