using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public interface INonPersistentBusinessObjectFlatten : INonPersistentBusinessObject
	{
		Guid? TariffPK { get; }
		Guid? TariffNationalCodePK { get; }
		Guid? NomenclatureGroupPK { get; }
		INonPersistentBusinessObjectParent Parent { get; }
		IEnumerable<object> LinkedObjects { get; }
		void Link(INonPersistentBusinessObjectParent parent, INonPersistentBusinessObjectComparison comp, out IEnumerable<object> unLinkedObjs);
		IEnumerable<object> Create();
		void Update(bool updateParent);
	}
}
