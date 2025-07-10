using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class DummyLandedCostDistributeToAndLandCostChargeHolder : DummyLandedCostDistributeTo, ILandedCostChargeHolder
	{
		public DummyLandedCostDistributeToAndLandCostChargeHolder(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IDefaultLandedCostInput[] ChargesToImportForLandedCostingExposed;
		public IEnumerable<IDefaultLandedCostInput> ChargesToImportForLandedCosting => ChargesToImportForLandedCostingExposed;
	}
}
