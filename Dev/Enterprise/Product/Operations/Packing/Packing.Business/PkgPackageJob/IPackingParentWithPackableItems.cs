using System;
using System.Collections.Generic;

namespace Enterprise.Packing.Business
{
	public interface IPackingParentWithPackableItems : IPackingParent
	{
		IEnumerable<IPackableItemParent> PackableItemParents { get; }

		YesNoWithReasonForNo IsAutoPackAllowed { get; }
		YesNoWithReasonForNo IsScanQtyAllowed { get; }

		event EventHandler PackableItemParentsCountChanged;

		void LoadAllPackableItemParentsInOneHit(PkgPackageJob packageJob);
	}
}
