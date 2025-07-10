using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class PackingGroupCollection : BasePackingGroupCollection
	{
		public PackingGroupCollection(Bill master)
			: base(master)
		{
		}

		public new PackingGroup this[int index]
		{
			get { return (PackingGroup)Elements[index]; }
		}

		public new PackingGroup AddNew()
		{
			return (PackingGroup)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(PackingGroup);
		}
	}
}
