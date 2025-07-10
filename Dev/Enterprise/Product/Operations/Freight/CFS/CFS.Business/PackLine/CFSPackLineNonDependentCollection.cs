using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSPackLineNonDependentCollection : PackLineNonDependentCollection
	{
		public CFSPackLineNonDependentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new CFSPackLine AddNew()
		{
			return (CFSPackLine)base.AddNew();
		}

		public new CFSPackLine this[int index]
		{
			get { return (CFSPackLine)base[index]; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(CFSPackLine);
		}
	}
}
