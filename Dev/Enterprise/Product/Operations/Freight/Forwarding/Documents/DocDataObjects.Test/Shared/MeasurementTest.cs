using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(Measurement))]
	sealed class MeasurementTest : NonPersistentBusinessObjectTestCase
	{
		#region TestToString

		public void TestToString()
		{
			var measurement = GetNewBusinessObject();
			AssertEquals("ToString", "123.45 KG", measurement.ToString());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new CommonContext(Factory);

			return new Measurement
			{
				Value = 123.45,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.Weight.Kilograms
				}
			};
		}

		#endregion
	}
}
