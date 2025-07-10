namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentValidationStrategy
	{
		public void CheckProductAssignedToCorrectFixedOrDynamicLocation(WhsAdjustmentLine adjustmentLine)
		{
			CheckLocationString_ProductAssignedToCorrectFixedOrDynamicLocationCore(adjustmentLine);
		}

		protected virtual void CheckLocationString_ProductAssignedToCorrectFixedOrDynamicLocationCore(WhsAdjustmentLine adjustmentLine)
		{
			// Test in WhsAdjustmentLineValidation.TestCheckLocationString_CheckFixLocationMaxProductType
			var location = adjustmentLine.Location;
			var part = adjustmentLine.SupplierPart;
			if (part != null)
			{
				var docket = adjustmentLine.Docket;
				if (docket != null)
				{
					var client = docket.Client;
					if (client != null)
					{
						WhsValidationHelper.CheckProductAssignedToCorrectFixedOrDynamicLocation(location, client, part, adjustmentLine.LocationStringInfo);
					}
				}
			}
		}
	}
}
