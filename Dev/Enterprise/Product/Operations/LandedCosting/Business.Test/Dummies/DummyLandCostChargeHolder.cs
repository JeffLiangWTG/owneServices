using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	public sealed class DummyLandCostChargeHolder : ILandedCostChargeHolder
	{
		public IDefaultLandedCostInput[] ChargesToImportForLandedCostingExposed;
		public IEnumerable<IDefaultLandedCostInput> ChargesToImportForLandedCosting => ChargesToImportForLandedCostingExposed;
	}
}
