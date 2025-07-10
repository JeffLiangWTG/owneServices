using System;
using Enterprise.ZArchitecture.Web.ServerServices.Testing;

namespace Enterprise.Tracking.Web.ServerServices.Testing
{
	abstract class WarehouseDocketLineUpdateParametersTest<DocketLineParameters> : WebServiceParametersTest<DocketLineParameters>
			where DocketLineParameters : WarehouseDocketLineUpdateParameters
	{
		protected override void AssertEquals(DocketLineParameters expected, DocketLineParameters actual)
		{
			AssertEquals(expected.ModifiedControlID, actual.ModifiedControlID);
			AssertEquals(expected.NewValue, actual.NewValue);
			AssertEquals(expected.DocketRef, actual.DocketRef);
			AssertEquals(expected.LineRef, actual.LineRef);
			AssertEquals(expected.ProductControlID, actual.ProductControlID);
			AssertEquals(expected.DescriptionControlID, actual.DescriptionControlID);
			AssertEquals(expected.PacksControlID, actual.PacksControlID);
			AssertEquals(expected.PacksUQControlID, actual.PacksUQControlID);
			AssertEquals(expected.QuantityControlID, actual.QuantityControlID);
			AssertEquals(expected.ProductUQControlID, actual.ProductUQControlID);
			AssertEquals(expected.Attribute1ControlID, actual.Attribute1ControlID);
			AssertEquals(expected.Attribute2ControlID, actual.Attribute2ControlID);
			AssertEquals(expected.Attribute3ControlID, actual.Attribute3ControlID);
			AssertEquals(expected.SerialNumberControlID, actual.SerialNumberControlID);
		}

		protected override void AssertParsedParameters(DocketLineParameters parameters)
		{
			AssertEquals("Modified Control ID", parameters.ModifiedControlID);
			AssertEquals("New Value", parameters.NewValue);
			AssertEquals("Docket Ref", parameters.DocketRef);
			AssertEquals("Line Ref", parameters.LineRef);
			AssertEquals("Product Control ID", parameters.ProductControlID);
			AssertEquals("Description Control ID", parameters.DescriptionControlID);
			AssertEquals("Packs Control ID", parameters.PacksControlID);
			AssertEquals("Packs UQ Control ID", parameters.PacksUQControlID);
			AssertEquals("Quantity Control ID", parameters.QuantityControlID);
			AssertEquals("Product UQ Control ID", parameters.ProductUQControlID);
			AssertEquals("Attribute1 Control ID", parameters.Attribute1ControlID);
			AssertEquals("Attribute2 Control ID", parameters.Attribute2ControlID);
			AssertEquals("Attribute3 Control ID", parameters.Attribute3ControlID);
			AssertEquals("Serial Number Control ID", parameters.SerialNumberControlID);
		}

		protected override string GetExpectedExceptionMessageForStringWithValidationErrors() => new ArgumentNullException("ModifiedControlID").Message;

		protected override string GetParametersInvalidString() => "BLAH : BLAH;";

		protected override string GetParametersStringWithValidationErrors() => "{NewValue: ''}";
	}
}
