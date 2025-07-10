using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ExportAWBSpecialHandlingCollection))]
	sealed class ExportAWBSpecialHandlingCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ExportAWBSpecialHandlingCollection(Factory.New<ConsolExportAWBHeader>());
		}

		public void TestMaxCountValidation()
		{
			var collection = GetCollectionToTest();
			AssertEquals("Max count should be 9", 9, collection.MaxCount);
		}
	}
}
