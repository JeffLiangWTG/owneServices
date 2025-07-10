using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSContainerFetchStrategy : ContainerFetchStrategy
	{
		public CFSContainerFetchStrategy(CFSContainer container) : base(container)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			var containerPackPivotRequired = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case CFSContainer.Schema.TotalShipmentWeight:
					case CFSContainer.Schema.TotalShipmentPacks:
					case CFSContainer.Schema.TotalShipmentVolume:
						containerPackPivotRequired = true;
						break;
				}
			}

			if (containerPackPivotRequired)
			{
				Factory.AddFetchHint(JobContainerPackPivotSchema.J6_JC, Container.PK);
			}

			base.FetchForViewCore(columns);
		}
	}
}
