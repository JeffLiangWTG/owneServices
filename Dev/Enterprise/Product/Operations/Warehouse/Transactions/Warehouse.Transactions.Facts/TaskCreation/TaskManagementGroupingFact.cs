using System;
using CargoWise.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class TaskManagementGroupingFact : ITaskManagementGroupingFact
	{
		public TaskManagementGroupingFact(Guid pk, string weightUQ = "", string volumeUQ = "")
		{
			PK = pk;
			WeightUQ = Argument.NotNull(weightUQ, nameof(weightUQ));
			VolumeUQ = Argument.NotNull(volumeUQ, nameof(volumeUQ));
		}

		public Guid PK { get; }

		public Guid AssignedTask { get; set; }

		public int NumberOfLines { get; set; }

		public int NumberOfUnits { get; set; }

		public int NumberOfPacks { get; set; }

		public decimal Weight { get; set; }

		public string WeightUQ { get; }

		public decimal Volume { get; set; }

		public string VolumeUQ { get; }
	}
}
