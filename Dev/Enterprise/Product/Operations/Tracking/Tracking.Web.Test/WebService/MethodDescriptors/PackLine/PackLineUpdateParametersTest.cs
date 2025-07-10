using System;
using Enterprise.ZArchitecture.Web.ServerServices.Testing;

namespace Enterprise.Tracking.Web.ServerServices.Testing
{
	sealed class PackLineUpdateParametersTest : WebServiceParametersTest<PackLineUpdateParameters>
	{
		protected override void AssertEquals(PackLineUpdateParameters expected, PackLineUpdateParameters actual)
		{
			AssertEquals(expected.ModifiedControlID, actual.ModifiedControlID);
			AssertEquals(expected.NewValue, actual.NewValue);
			AssertEquals(expected.ShipmentRef, actual.ShipmentRef);
			AssertEquals(expected.LineRef, actual.LineRef);
			AssertEquals(expected.QuantityControlID, actual.QuantityControlID);
			AssertEquals(expected.LengthControlID, actual.LengthControlID);
			AssertEquals(expected.WidthControlID, actual.WidthControlID);
			AssertEquals(expected.HeightControlID, actual.HeightControlID);
			AssertEquals(expected.PackUDControlID, actual.PackUDControlID);
			AssertEquals(expected.VolumeControlID, actual.VolumeControlID);
			AssertEquals(expected.VolumeUQControlID, actual.VolumeUQControlID);
		}

		protected override void AssertParsedParameters(PackLineUpdateParameters parameters)
		{
			AssertEquals("Modified Control ID", parameters.ModifiedControlID);
			AssertEquals("New Value", parameters.NewValue);
			AssertEquals("Shipment Ref", parameters.ShipmentRef);
			AssertEquals("Line Ref", parameters.LineRef);
			AssertEquals("Quantity Control ID", parameters.QuantityControlID);
			AssertEquals("Length Control ID", parameters.LengthControlID);
			AssertEquals("Width Control ID", parameters.WidthControlID);
			AssertEquals("Height Control ID", parameters.HeightControlID);
			AssertEquals("PackUD Control ID", parameters.PackUDControlID);
			AssertEquals("Volume Control ID", parameters.VolumeControlID);
			AssertEquals("VolumeUQ Control ID", parameters.VolumeUQControlID);
		}

		protected override PackLineUpdateParameters GetNewParameters() => new PackLineUpdateParameters
		{
			ModifiedControlID = "Modified Control ID",
			NewValue = "New Value",
			ShipmentRef = "Shipment Ref",
			LineRef = "Line Ref",
			QuantityControlID = "Quantity Control ID",
			LengthControlID = "Length Control ID",
			WidthControlID = "Width Control ID",
			HeightControlID = "Height Control ID",
			PackUDControlID = "PackUD Control ID",
			VolumeControlID = "Volume Control ID",
			VolumeUQControlID = "VolumeUQ Control ID",
		};

		protected override string GetParametersString() => "{ModifiedControlID: 'Modified Control ID'," +
				"NewValue: 'New Value'," +
				"ShipmentRef: 'Shipment Ref'," +
				"LineRef: 'Line Ref'," +
				"QuantityControlID: 'Quantity Control ID'," +
				"LengthControlID: 'Length Control ID'," +
				"WidthControlID: 'Width Control ID'," +
				"HeightControlID: 'Height Control ID'," +
				"PackUDControlID: 'PackUD Control ID'," +
				"VolumeControlID: 'Volume Control ID'," +
				"VolumeUQControlID: 'VolumeUQ Control ID'}";

		protected override PackLineUpdateParameters GetNewParametersWithValidationErrors() => new PackLineUpdateParameters();

		protected override string GetExpectedExceptionMessageForStringWithValidationErrors() => new ArgumentNullException("ModifiedControlID").Message;

		protected override string GetParametersInvalidString() => "Invalid : Invalid;";

		protected override string GetParametersStringWithValidationErrors() => "{NewValue: ''}";
	}
}
