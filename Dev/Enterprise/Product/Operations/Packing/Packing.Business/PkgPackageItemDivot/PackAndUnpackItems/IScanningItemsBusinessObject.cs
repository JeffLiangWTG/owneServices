using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public interface IScanningItemsBusinessObject
	{
		void SelectPackableItemParent(PackableItemParentWrapper wrapper);

		bool IsUserEnteringQty { get; }
		bool IsTUN { get; }
		ZString TUNCode { get; }

		bool AddFilterIfValidAttribute(ZString value);
		IEnumerable<PackableItemParentWrapper> GetAllWrappers();
	}
}
