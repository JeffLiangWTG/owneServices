using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public sealed class CarrierContractForUtilizationSimulation : RatingContract
	{
		public CarrierContractForUtilizationSimulation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public IContractSimulationFormConfiguration FormConfiguration { get; internal set; }

		#region Quantity Fields

		public AllocationRouteForUtilizationSimulationCollection AllocationRoutesForUtilizationSimulation
		{
			get
			{
				if (allocationRoutesForUtilizationSimulation == null)
				{
					allocationRoutesForUtilizationSimulation = new AllocationRouteForUtilizationSimulationCollection(this, FormConfiguration);
				}
				return allocationRoutesForUtilizationSimulation;
			}
		}

		AllocationRouteForUtilizationSimulationCollection allocationRoutesForUtilizationSimulation;

		#endregion

		#region Active Filter

		public static ZQuery ActiveFilter
		{
			get { return new ZQuery(RatingContractSchema.RCT_IsActive, SQLComparisonOperator.Equal, true); }
		}

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CarrierContractForUtilizationSimulationFetchStrategy(this);
		}
	}
}
