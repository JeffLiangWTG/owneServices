using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public interface IPackedItemsCollection : IBusinessObjectCollection
	{
		IEnumerable<PkgPackageItemDivotsWrapper> Typed { get; }
		new PkgPackageItemDivotsWrapper this[int index] { get; }
		void RemoveAll();
	}
}
