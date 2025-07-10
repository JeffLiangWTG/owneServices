using System;
using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassPackLineManyToManyCollection : CFSPackLineManyToManyCollection
	{
		public GatePassPackLineManyToManyCollection(GatePassContainer container) : base(container)
		{
		}

		public new GatePassPackLine this[int index]
		{
			get { return (GatePassPackLine)(Elements[index]); }
		}

		public new GatePassPackLine AddNew()
		{
			return (GatePassPackLine)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(GatePassPackLine);
		}
	}
}
