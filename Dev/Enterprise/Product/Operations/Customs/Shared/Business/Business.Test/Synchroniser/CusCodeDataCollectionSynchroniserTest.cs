using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusCodeDataCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestCusCodeDataCollectionSynchroniser()
		{
			var sourceInvoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var destinationInvoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var sourceCollection = new DummyCusCodeDataCollection(sourceInvoiceLine);
			var destinationCollection = new DummyCusCodeDataCollection(destinationInvoiceLine);
			sourceCollection.AddNew("A", "AAAAAA");
			sourceCollection.AddNew("B", "BBBBBB");
			sourceCollection.AddNew("C", "CCCCCC");
			destinationCollection.AddNew("A", "AAAAAA");
			destinationCollection.AddNew("D", "DDDDDD");
			var synchnroniser = new CusCodeDataCollectionSynchroniser<DummyCusCodeData>(sourceInvoiceLine, destinationInvoiceLine, sourceCollection, destinationCollection);
			synchnroniser.Synchronise();

			AssertEquals("Destination should match source after synhcronise", 3, destinationCollection.Count);
			Assert(destinationCollection.ContainsNumber("AAAAAA"));
			Assert(destinationCollection.ContainsNumber("BBBBBB"));
			Assert(destinationCollection.ContainsNumber("CCCCCC"));
			var newElement = Factory.New<DummyCusCodeData>();
			newElement.CY_Code = "E";
			newElement.CY_Data = "EEEEEE";
			sourceCollection.Add(newElement);
			AssertEquals("one element added", 4, destinationCollection.Count);
			Assert(destinationCollection.ContainsNumber("EEEEEE"));
			newElement.CY_Code = "G";
			newElement.CY_Data = "GGGGGG";
			AssertEquals("still 4 elements", 4, destinationCollection.Count);
			var foundElements = (DummyCusCodeData[])destinationCollection.Find(new ZQuery(CusCodeDataSchema.CY_Code, "G"));
			AssertEquals("code changed", 1, foundElements.Length);
			AssertEquals("data changed", "GGGGGG", foundElements[0].CY_Data);
			sourceCollection.RemoveAndDelete(newElement);
			AssertEquals("one removed", 3, destinationCollection.Count);
			Assert(!destinationCollection.ContainsNumber("GGGGGG"));
			Assert(destinationCollection.ContainsNumber("AAAAAA"));
			Assert(destinationCollection.ContainsNumber("BBBBBB"));
			Assert(destinationCollection.ContainsNumber("CCCCCC"));
		}
	}
}
