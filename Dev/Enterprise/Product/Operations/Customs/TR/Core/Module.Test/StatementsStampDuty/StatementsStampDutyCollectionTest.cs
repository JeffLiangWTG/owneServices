using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Module.Testing
{
	[TestedType(typeof(StatementsStampDutyCollection))]
	public class StatementsStampDutyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StatementsStampDutyCollection(Factory);
		}

		public void TestAdditionalFilter()
		{
			var collection = new StatementsStampDutyCollection(Factory);
			var filter = collection.CompleteFilter.FilterString;
			AssertEquals("The additional filter should contains B2_GC", true, filter.Contains("B2_GC"));
		}
	}
}
