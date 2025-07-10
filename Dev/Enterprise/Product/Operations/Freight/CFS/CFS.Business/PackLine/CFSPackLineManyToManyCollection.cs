using System;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSPackLineManyToManyCollection : PackLineManyToManyCollection, Integration.CFS.ICFSPackLineManyToManyCollection
	{
		public CFSPackLineManyToManyCollection(CFSContainer container) : base(container)
		{
		}

		public new CFSPackLine this[int index]
		{
			get { return (CFSPackLine)(Elements[index]); }
		}

		public new CFSPackLine AddNew()
		{
			return (CFSPackLine)base.AddNew();
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
			return typeof(CFSPackLine);
		}
	}
}
