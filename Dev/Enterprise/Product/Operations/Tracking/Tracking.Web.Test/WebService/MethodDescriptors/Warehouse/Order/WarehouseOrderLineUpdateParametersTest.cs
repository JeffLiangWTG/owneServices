namespace Enterprise.Tracking.Web.ServerServices.Testing
{
	sealed class WarehouseOrderLineUpdateParametersTest : WarehouseDocketLineUpdateParametersTest<WarehouseOrderLineUpdateParameters>
	{
		protected override void AssertEquals(WarehouseOrderLineUpdateParameters expected, WarehouseOrderLineUpdateParameters actual)
		{
			base.AssertEquals(expected, actual);

			AssertEquals(expected.ShortfallControlID, actual.ShortfallControlID);
		}

		protected override void AssertParsedParameters(WarehouseOrderLineUpdateParameters parameters)
		{
			base.AssertParsedParameters(parameters);

			AssertEquals("Shortfall Control ID", parameters.ShortfallControlID);
		}

		protected override WarehouseOrderLineUpdateParameters GetNewParameters() => new WarehouseOrderLineUpdateParameters
		{
			ModifiedControlID = "Modified Control ID",
			NewValue = "New Value",
			DocketRef = "Docket Ref",
			LineRef = "Line Ref",
			ProductControlID = "Product Control ID",
			DescriptionControlID = "Description Control ID",
			PacksControlID = "Packs Control ID",
			PacksUQControlID = "Packs UQ Control ID",
			QuantityControlID = "Quantity Control ID",
			ProductUQControlID = "Product UQ Control ID",
			ShortfallControlID = "Shortfall Control ID",
			Attribute1ControlID = "Attribute1 Control ID",
			Attribute2ControlID = "Attribute2 Control ID",
			Attribute3ControlID = "Attribute3 Control ID",
			SerialNumberControlID = "Serial Number Control ID"
		};

		protected override string GetParametersString() => "{ModifiedControlID: 'Modified Control ID'," +
				"NewValue: 'New Value'," +
				"DocketRef: 'Docket Ref'," +
				"LineRef: 'Line Ref'," +
				"ProductControlID: 'Product Control ID'," +
				"DescriptionControlID: 'Description Control ID'," +
				"PacksControlID: 'Packs Control ID'," +
				"PacksUQControlID: 'Packs UQ Control ID'," +
				"QuantityControlID: 'Quantity Control ID'," +
				"ProductUQControlID: 'Product UQ Control ID'," +
				"ShortfallControlID: 'Shortfall Control ID'," +
				"Attribute1ControlID: 'Attribute1 Control ID'," +
				"Attribute2ControlID: 'Attribute2 Control ID'," +
				"Attribute3ControlID: 'Attribute3 Control ID'," +
				"SerialNumberControlID: 'Serial Number Control ID'}";

		protected override WarehouseOrderLineUpdateParameters GetNewParametersWithValidationErrors() => new WarehouseOrderLineUpdateParameters();
	}
}
