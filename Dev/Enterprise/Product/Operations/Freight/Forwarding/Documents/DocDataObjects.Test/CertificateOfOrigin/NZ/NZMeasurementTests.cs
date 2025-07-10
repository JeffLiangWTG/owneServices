using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.NZ
{
	[TestedType(typeof(NZMeasurement))]
	class NZMeasurementTests : NonPersistentBusinessObjectTestCase
	{
		const decimal weight = 123.45M;

		#region TestToString
		public void TestValueChanged()
		{
			var measurement = GetNewBusinessObject() as NZMeasurement;
			measurement.Unit.Code = Core.Constants.Weight.Grams;
			AssertEquals("Value", weight * 1000, measurement.Value);
		}

		public void TestValueChangedFromOriginalValue()
		{
			var measurement = GetNewBusinessObject() as NZMeasurement;
			measurement.Value = 500;
			measurement.Unit.Code = Core.Constants.Weight.Grams;
			AssertEquals("Value", weight * 1000, measurement.Value);
		}

		public void TestCodes()
		{
			var measurement = GetNewBusinessObject() as NZMeasurement;
			var codes = new NZCustomsTariffQuantityWeightUnits().CodesAsString;
			var actualCodes = (measurement.Unit.Codes as NZCustomsTariffQuantityWeightUnits).CodesAsString;
			AssertEquals("codes", codes, actualCodes);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new CommonContext(Factory);

			return new NZMeasurement(weight, Core.Constants.Weight.Kilograms);
		}

		#endregion
	}
}
