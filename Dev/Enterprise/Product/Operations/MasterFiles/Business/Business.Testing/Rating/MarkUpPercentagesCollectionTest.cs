using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Testing
{
	[TestedType(typeof(MarkUpPercentagesCollection))]
	sealed class MarkUpPercentagesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MarkUpPercentagesCollection>
	{
		protected override MarkUpPercentagesCollection GetCollectionToTest()
		{
			return new MarkUpPercentagesCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MarkUpPercentage("Mode", "Location", 10m, 0m, 0m, new MarkUpPercentagesCollection(Factory));
		}

		public void TestGetPercentage()
		{
			ILocation aU = LocationHelper.GetLocationFromString("AU", Factory);
			ILocation aUSYD = LocationHelper.GetLocationFromString("AUSYD", Factory);
			ILocation aUMEL = LocationHelper.GetLocationFromString("AUMEL", Factory);

			MarkUpPercentagesCollection collection = new MarkUpPercentagesCollection("");
			AssertNull(collection.FindElement(MarkUpPercentage.ALL, aU, true));
			AssertNull(collection.FindElement(MarkUpPercentage.ALL, aUSYD, true));

			collection = new MarkUpPercentagesCollection("AU|1.1");
			AssertPercentageMinimumAndPerUnit(1.1m, 0m, 0m, collection.FindElement(MarkUpPercentage.ALL, aU, true));
			AssertPercentageMinimumAndPerUnit(1.1m, 0m, 0m, collection.FindElement(MarkUpPercentage.ALL, aUSYD, true));

			collection = new MarkUpPercentagesCollection("AU|1.1;AUSYD|1.3");
			AssertPercentageMinimumAndPerUnit(1.1m, 0m, 0m, collection.FindElement(MarkUpPercentage.ALL, aU, true));
			AssertPercentageMinimumAndPerUnit(1.3m, 0m, 0m, collection.FindElement(MarkUpPercentage.ALL, aUSYD, true));

			collection = new MarkUpPercentagesCollection("AUSYD|1.3");
			AssertNull(collection.FindElement(MarkUpPercentage.ALL, aU, true));
			AssertPercentageMinimumAndPerUnit(1.3m, 0m, 0m, collection.FindElement(MarkUpPercentage.ALL, aUSYD, true));

			collection = new MarkUpPercentagesCollection("AUSYD|1.3;12.01");
			AssertPercentageMinimumAndPerUnit(12.01m, 0m, 0m, collection.FindElement(MarkUpPercentage.ALL, aU, true));
			AssertPercentageMinimumAndPerUnit(1.3m, 0m, 0m, collection.FindElement(MarkUpPercentage.ALL, aUSYD, true));

			collection = new MarkUpPercentagesCollection("ALL|AUSYD|1.8;AIR|AUSYD|1.6;FCL|AUSYD|1.3;12.01");
			AssertPercentageMinimumAndPerUnit(1.8m, 0m, 0m, collection.FindElement(MarkUpPercentage.ALL, aUSYD, true));
			AssertPercentageMinimumAndPerUnit(1.6m, 0m, 0m, collection.FindElement("AIR", aUSYD, true));
			AssertPercentageMinimumAndPerUnit(1.8m, 0m, 0m, collection.FindElement("LCL", aUSYD, true));
			AssertPercentageMinimumAndPerUnit(1.3m, 0m, 0m, collection.FindElement("FCL", aUSYD, true));
			AssertPercentageMinimumAndPerUnit(12.01m, 0m, 0m, collection.FindElement("FCL", aUMEL, true));
		}

		public void TestGetMinimumAndPerUnit()
		{
			ILocation aUSYD = LocationHelper.GetLocationFromString("AUSYD", Factory);

			MarkUpPercentagesCollection collection = new MarkUpPercentagesCollection("");
			AssertNull(collection.FindElement(MarkUpPercentage.ALL, aUSYD, true));

			collection = new MarkUpPercentagesCollection("AIR|AU|0|100|5;FCL|AUSYD|0|0|500;LCL|AUSYD|0|50|20.5;ORG|AUSYD|0|100|5");
			AssertPercentageMinimumAndPerUnit(0m, 0m, 0m, collection.FindElement("ORG", aUSYD, true));
			AssertPercentageMinimumAndPerUnit(0m, 100m, 5m, collection.FindElement("AIR", aUSYD, true));
			AssertPercentageMinimumAndPerUnit(0m, 50m, 20.5m, collection.FindElement("LCL", aUSYD, true));
			AssertPercentageMinimumAndPerUnit(0m, 0m, 500m, collection.FindElement("FCL", aUSYD, true));
		}

		void AssertPercentageMinimumAndPerUnit(ZDecimal percentage, ZDecimal minimum, ZDecimal perUnit, MarkUpPercentage markUp)
		{
			AssertEquals(percentage, markUp.Percentage);
			AssertEquals(minimum, markUp.Minimum);
			AssertEquals(perUnit, markUp.PerUnit);
		}
	}
}
