using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed partial class RefContainerStockFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public RefContainerStockFetchStrategy(RefContainerStock stock)
			: base(stock) { }

		RefContainerStock Stock => BusinessObject as RefContainerStock;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var needsLastMovement = false;
			var needsRefContainer = false;

			foreach (TableColumn column in columns)
			{
				if (column.ColumnName.StartsWith("LastMovement+", StringComparison.OrdinalIgnoreCase))
				{
					needsLastMovement = true;
				}

				switch (column.ColumnName)
				{
					case RefContainerStock.Schema.R6_RC_HasTynes:
					case RefContainerStock.Schema.R6_RC_HasVents:
					case RefContainerStock.Schema.R6_RC_IsHighCube:
					case RefContainerStock.Schema.R6_RC_ISOType:
						needsRefContainer = true;
						break;
				}
			}

			if (needsLastMovement)
			{
				Factory.AddFetchHint(new LastMovementFetchHint(Stock.PK));
			}

			if (needsRefContainer)
			{
				Factory.AddFetchHint(typeof(RefContainer), Stock.R6_RC);
			}
		}
	}
}
