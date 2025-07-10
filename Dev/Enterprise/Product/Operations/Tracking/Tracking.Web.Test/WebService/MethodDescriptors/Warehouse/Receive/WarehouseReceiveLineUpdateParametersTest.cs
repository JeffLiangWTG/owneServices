namespace Enterprise.Tracking.Web.ServerServices.Testing
{
	sealed class WarehouseReceiveLineUpdateParametersTest : WarehouseDocketLineUpdateParametersTest<WarehouseReceiveLineUpdateParameters>
	{
		protected override void AssertEquals(WarehouseReceiveLineUpdateParameters expected, WarehouseReceiveLineUpdateParameters actual)
		{
			base.AssertEquals(expected, actual);

			AssertEquals(expected.ExpectedQuantityControlID, actual.ExpectedQuantityControlID);
			AssertEquals(expected.ExpiryDateControlID, actual.ExpiryDateControlID);
		}

		protected override void AssertParsedParameters(WarehouseReceiveLineUpdateParameters parameters)
		{
			base.AssertParsedParameters(parameters);

			AssertEquals("Expected Quantity Control ID", parameters.ExpectedQuantityControlID);
			AssertEquals("Expiry Date Control ID", parameters.ExpiryDateControlID);
		}

		protected override WarehouseReceiveLineUpdateParameters GetNewParameters() => new WarehouseReceiveLineUpdateParameters
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
			ExpectedQuantityControlID = "Expected Quantity Control ID",
			Attribute1ControlID = "Attribute1 Control ID",
			Attribute2ControlID = "Attribute2 Control ID",
			Attribute3ControlID = "Attribute3 Control ID",
			SerialNumberControlID = "Serial Number Control ID",
			ExpiryDateControlID = "Expiry Date Control ID"
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
				"ExpectedQuantityControlID: 'Expected Quantity Control ID'," +
				"ProductUQControlID: 'Product UQ Control ID'," +
				"Attribute1ControlID: 'Attribute1 Control ID'," +
				"Attribute2ControlID: 'Attribute2 Control ID'," +
				"Attribute3ControlID: 'Attribute3 Control ID'," +
				"SerialNumberControlID: 'Serial Number Control ID'," +
				"ExpiryDateControlID: 'Expiry Date Control ID'}";

		protected override WarehouseReceiveLineUpdateParameters GetNewParametersWithValidationErrors() => new WarehouseReceiveLineUpdateParameters();
	}
}
